using GLFW;
using Pengu.Support;
using Pengu.VirtualMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Pengu.Renderer.UI
{
    class HexEditorWindow<TMemory> : BaseWindow where TMemory : IMemory
    {
        readonly TMemory memory;
        readonly VM? vm;

        const int editorLineBytes = 0x15;
        const int addressSizeBytes = 2;
        readonly int linesCount;

        int selectedHalfByte;

        bool done, running, solved;

        public HexEditorWindow(VulkanContext context, VulkanContext.GameSurface surface, TMemory memory,
            int? positionX = null, int? positionY = null, string? title = null, int? linesCount = null)
            : base(context, surface, "HEX EDITOR" + (string.IsNullOrWhiteSpace(title) ? "" : $" - {title}"),
                  positionX: positionX ?? 8, positionY: positionY ?? 2, chromeBackground: FontColor.Black, chromeForeground: FontColor.BrightGreen)
        {
            (this.memory, vm, this.linesCount) = (memory, memory as VM, linesCount ?? 15);

            memory.RefreshRequired += _ => contentFontStringDirty = true;
            vm?.RegisterInterrupt(0, _ => { done = true; running = false; });
        }

        readonly List<FontOverride> fontOverride = new List<FontOverride>();

        protected override void FillContentFontString(bool first)
        {
            static string TryGetHexAt(IList<byte> array, int index) => index < array.Count ? array[index].ToString("X2") : "..";

            var frameForAddress = new string('═', addressSizeBytes * 2 + 2);
            var frameForHexDump = new string('═', editorLineBytes * 3 + 1);

            var line = Math.DivRem(selectedHalfByte, editorLineBytes * 2, out var halfIndexInLine);

            const int frameLength = addressSizeBytes * 2 + 3 + editorLineBytes * 3 + 2;
            var headerAddress0 = 8 + 3 * (halfIndexInLine / 2);
            var leftAddress0 = (2 + line) * frameLength + 1;
            var value0 = leftAddress0 + 4 + 3 + 3 * (halfIndexInLine / 2) + halfIndexInLine % 2;

            fontOverride.Clear();
            fontOverride.Add((headerAddress0, 2, chromeBackground, FontColor.BrightCyan, true));
            fontOverride.Add((leftAddress0, 4, chromeBackground, FontColor.BrightCyan, true));
            fontOverride.Add((value0, 1, chromeBackground, FontColor.BrightCyan, true));

            string? additionalText = null;
            var editorLineBytes31Lines = new string('─', editorLineBytes * 3 + 1);
            var addressSizeBytes22Lines = new string('─', addressSizeBytes * 2 + 2);

            if (!(vm is null))
            {
                var ipLine = Math.DivRem(vm.InstructionPointer, editorLineBytes, out var ipIndexInLine);
                var disasmNext = InstructionSet.Disassemble(memory.Memory.AsMemory(vm.InstructionPointer), out var instructionByteSize) ?? "---";

                var ip0 = (2 + ipLine) * frameLength + 3 + 5 + 3 * ipIndexInLine;
                var ip0len = 3 * Math.Min(editorLineBytes - ipIndexInLine, instructionByteSize);
                var ip1 = (3 + ipLine) * frameLength + 3 + 5 + 0;
                var ip1len = 3 * instructionByteSize - ip0len;

                fontOverride.Add((ip0, ip0len, done ? FontColor.DarkGreen : FontColor.DarkRed, done ? chromeBackground : FontColor.White, false));
                fontOverride.Add((ip1, ip1len, done ? FontColor.DarkGreen : FontColor.DarkRed, done ? chromeBackground : FontColor.White, false));

                var regFlags = (solved ? "(SOLVED) " : "         ") +
                    string.Concat(vm.Registers.Select((val, idx) => $"R{idx}: 0x{val:X2} ")) +
                    $"SR: 0x{vm.StackRegister:X2} IP: 0x{vm.InstructionPointer:X2} F: {(vm.FlagCompare < 0 ? "-" : vm.FlagCompare == 0 ? "0" : "+")} ";
                var statusLine = " NEXT ASM: " + disasmNext.PadRight(addressSizeBytes * 2 + editorLineBytes * 3 - 7 - regFlags.Length) + regFlags;

                var disasmSelected = InstructionSet.Disassemble(memory.Memory.AsMemory(selectedHalfByte / 2), out _) ?? "---";

                additionalText = "\n" +
                    addressSizeBytes22Lines + "┴" + editorLineBytes31Lines + "\n" +
                    statusLine + "\n" +
                    " SEL  ASM: " + disasmSelected.PadRight(addressSizeBytes * 2 + editorLineBytes * 3 - 7);
            }

            fontOverride.Sort((a, b) => a.start.CompareTo(b.start));

            ContentFontString.Set(
                new string(' ', addressSizeBytes * 2 + 2) + "│" + string.Concat(Enumerable.Range(0, editorLineBytes).Select(idx => $" {idx:X2}")) + " \n" +
                addressSizeBytes22Lines + "┼" + editorLineBytes31Lines + "\n" +
                string.Join("\n", Enumerable.Range(0, linesCount).Select(lineIdx =>
                    $" {lineIdx * editorLineBytes:X4} │ {string.Join(' ', Enumerable.Range(0, editorLineBytes).Select(idx => TryGetHexAt(memory.Memory, lineIdx * editorLineBytes + idx)))} ")) +
                additionalText,
                overrides: fontOverride);

            if (first)
                ContentFontString.Set(defaultBg: chromeBackground, defaultFg: chromeForeground,
                    offset: surface.CharacterToScreenSize(positionX, positionY, ContentFontString));
        }

        public override bool ProcessKey(Keys key, int scanCode, InputState action, ModifierKeys modifiers)
        {
            if (key == Keys.Right && action != InputState.Release && modifiers.HasFlag(ModifierKeys.Control))
            {
                InstructionSet.Disassemble(memory.Memory.AsMemory(selectedHalfByte / 2), out var size);
                if (size == 0) size = 1;
                selectedHalfByte = Math.Min(size * 2 + selectedHalfByte & 0xFFFE, memory.Memory.Count * 2 - 2);
                contentFontStringDirty = true;
                return true;
            }

            if (key == Keys.Left && action != InputState.Release && selectedHalfByte > 0) { --selectedHalfByte; contentFontStringDirty = true; return true; }
            if (key == Keys.Right && action != InputState.Release && selectedHalfByte < memory.Memory.Count * 2 - 1) { ++selectedHalfByte; contentFontStringDirty = true; return true; }

            if (key == Keys.Up && action != InputState.Release && selectedHalfByte >= editorLineBytes * 2) { selectedHalfByte -= editorLineBytes * 2; contentFontStringDirty = true; return true; }
            if (key == Keys.Down && action != InputState.Release && selectedHalfByte < memory.Memory.Count * 2 - editorLineBytes * 2) { selectedHalfByte += editorLineBytes * 2; contentFontStringDirty = true; return true; }

            void UpdateHalfByteWithNumber(int n)
            {
                if (selectedHalfByte % 2 == 1)
                    memory.Memory[selectedHalfByte / 2] = (byte)(memory.Memory[selectedHalfByte / 2] & 0xF0 | n);
                else
                    memory.Memory[selectedHalfByte / 2] = (byte)(memory.Memory[selectedHalfByte / 2] & 0xF | (n << 4));

                if (selectedHalfByte < memory.Memory.Count * 2 - 1)
                    ++selectedHalfByte;

                contentFontStringDirty = true;
            }

            if (key >= Keys.Numpad0 && key <= Keys.Numpad9 && action != InputState.Release) { UpdateHalfByteWithNumber(key - Keys.Numpad0); return true; }
            if (key >= Keys.Alpha0 && key <= Keys.Alpha9 && action != InputState.Release) { UpdateHalfByteWithNumber(key - Keys.Alpha0); return true; }
            if (key >= Keys.A && key <= Keys.F && action != InputState.Release) { UpdateHalfByteWithNumber(key - Keys.A + 10); return true; }

            if (key == Keys.F11 && action != InputState.Release && !done && !running)
            {
                vm?.RunNextInstruction();
                contentFontStringDirty = true;
            }
            if (key == Keys.F5 && !modifiers.HasFlag(ModifierKeys.Shift) && !modifiers.HasFlag(ModifierKeys.Control) && action == InputState.Press && !done && !running)
                running = true;
            if (key == Keys.F5 && modifiers.HasFlag(ModifierKeys.Shift) && !modifiers.HasFlag(ModifierKeys.Control) && action == InputState.Press && !done && running)
                running = false;
            if (key == Keys.F5 && !modifiers.HasFlag(ModifierKeys.Shift) && modifiers.HasFlag(ModifierKeys.Control) && action == InputState.Press && !done && !running && !(vm is null))
            {
                var anyProblems = false;
                solved = false;

                if (!(surface.Solutions is null))
                    foreach (var solution in surface.Solutions)
                    {
                        foreach (var input in solution.Inputs!)
                        {
                            var mem = surface.FindMemory(input.MemoryName!);
                            for (int idx = 0; idx < input.Data!.Length; ++idx)
                                mem.Memory[input.MemoryIndex + idx] = input.Data[idx];
                        }

                        vm.Reset();

                        vm.RunNextInstruction(100000, () =>
                        {
                            anyProblems = false;
                            foreach (var expectation in solution.Expectations!)
                                foreach (var expectationItem in expectation.ExpectationGroup!)
                                    if (!surface.FindMemory(expectationItem.MemoryName!).Memory.Skip(expectationItem.MemoryIndex).SequenceEqual(expectationItem.Data!))
                                    {
                                        anyProblems = true;
                                        break;
                                    }

                            return !anyProblems;
                        });
                    }

                contentFontStringDirty = true;
                solved = !anyProblems;
            }

            if (key == Keys.R && action == InputState.Press && !running)
            {
                vm?.Reset();
                done = false;
                contentFontStringDirty = true;
            }

            return false;
        }

        TimeSpan partialElapsedTime;
        const double InstructionRunFrequencyMSec = 1000.0 / 120.0;
        public override void UpdateLogic(TimeSpan elapsedTime)
        {
            if (!running) return;

            // execute 1 instruction per frame at {InstructionRunFrequencyMSec} fps
            var totalTime = elapsedTime + partialElapsedTime;
            var cycles = (int)(totalTime.TotalMilliseconds / InstructionRunFrequencyMSec);
            if (cycles > 0)
            {
                vm?.RunNextInstruction(cycles);
                contentFontStringDirty = true;
                partialElapsedTime = TimeSpan.FromTicks((long)(totalTime.TotalMilliseconds - cycles * InstructionRunFrequencyMSec * TimeSpan.TicksPerMillisecond));
            }
            else
                partialElapsedTime += elapsedTime;
        }
    }
}
