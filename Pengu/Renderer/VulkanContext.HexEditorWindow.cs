using GLFW;
using Pengu.VirtualMachine;
using System;
using System.Linq;
using System.Numerics;

namespace Pengu.Renderer
{
    partial class VulkanContext
    {
        class HexEditorWindow : BaseWindow
        {
            readonly VM vm;

            const int editorLineBytes = 0x20;
            const int addressSizeBytes = 2;
            const int linesCount = 15;

            int selectedHalfByte;

            bool done, running;

            public HexEditorWindow(VulkanContext context, GameSurface surface, VM vm) : base(context, surface)
            {
                this.vm = vm;
                vm.RegisterInterrupt(0, _ => { done = true; running = false; });

                fontString = context.monospaceFont.AllocateString(new Vector2(-1f * context.extent.AspectRatio, -0.9f), 0.055f);
                FillFontString();
            }

            protected override void FillFontString()
            {
                static string TryGetHexAt(byte[] array, int index) => index < array.Length ? array[index].ToString("X2") : "..";

                var frameForAddress = new string('═', addressSizeBytes * 2 + 2);
                var frameForHexDump = new string('═', editorLineBytes * 3 + 1);

                const int windowFrameWidth = 1 + addressSizeBytes * 2 + 2 + 1 + editorLineBytes * 3 + 1 + 1;
                const string title = "HEX EDITOR";
                int titleHalfOffset = (windowFrameWidth - title.Length) / 2;

                var line = Math.DivRem(selectedHalfByte, editorLineBytes * 2, out var halfIndexInLine);

                const int frameLength = addressSizeBytes * 2 + 4 + editorLineBytes * 3 + 3;
                var headerAddress0 = frameLength + 2 + 7 + 3 * (halfIndexInLine / 2);
                var leftAddress0 = (3 + line) * frameLength + 2;
                var value0 = leftAddress0 + 4 + 3 + 3 * (halfIndexInLine / 2) + halfIndexInLine % 2;

                var ipLine = Math.DivRem(vm.InstructionPointer, editorLineBytes, out var ipIndexInLine);
                var disasmNext = InstructionSet.Disassemble(vm.Memory.AsMemory(vm.InstructionPointer), out var instructionByteSize);
                var ip0 = (3 + ipLine) * frameLength + 4 + 5 + 3 * ipIndexInLine;

                var valOverride = (value0, 1, FontColor.Black, FontColor.BrightCyan, true);
                var ipOverride = (ip0, 3 * instructionByteSize, done ? FontColor.DarkGreen : FontColor.DarkRed, done ? FontColor.Black : FontColor.White, false);
                var overrides = new[]
                    {
                        (1 + frameForAddress.Length + 1 + titleHalfOffset - 4 - 1 - addressSizeBytes * 2 - 2, title.Length + 2, FontColor.White, FontColor.Black, false),
                        (headerAddress0, 2, FontColor.Black, FontColor.BrightCyan, true),
                        (leftAddress0, 4, FontColor.Black, FontColor.BrightCyan, true),
                        value0 < ip0 ? valOverride : ipOverride,
                        value0 >= ip0 ? valOverride : ipOverride,
                    };

                var regFlags = string.Concat(vm.Registers.Select((val, idx) => $"R{idx}: 0x{val:X2} ")) +
                    $"SR: 0x{vm.StackRegister:X2} IP: 0x{vm.InstructionPointer:X2} F: {(vm.FlagCompare < 0 ? "-" : vm.FlagCompare == 0 ? "0" : "+")} ";
                var statusLine = " NEXT ASM: " + disasmNext.PadRight(addressSizeBytes * 2 + editorLineBytes * 3 - 7 - regFlags.Length) + regFlags;

                var disasmSelected = InstructionSet.Disassemble(vm.Memory.AsMemory(selectedHalfByte / 2), out _) ?? "---";
                fontString.Set(
                    "╔" + frameForAddress + "╤" + new string('═', titleHalfOffset - 4 - 1 - addressSizeBytes * 2 - 2) + " " + title + " " + new string('═', titleHalfOffset) + "╗\n" +
                    "║" + new string(' ', addressSizeBytes * 2 + 2) + "│" + string.Concat(Enumerable.Range(0, editorLineBytes).Select(idx => $" {idx:X2}")) + " ║\n" +
                    "╟" + new string('─', addressSizeBytes * 2 + 2) + "┼" + new string('─', editorLineBytes * 3 + 1) + "╢\n" +
                    string.Concat(Enumerable.Range(0, linesCount).Select(lineIdx =>
                        $"║ {lineIdx * editorLineBytes:X4} │ {string.Join(' ', Enumerable.Range(0, editorLineBytes).Select(idx => TryGetHexAt(vm.Memory, lineIdx * editorLineBytes + idx)))} ║\n")) +
                    "╟" + new string('─', addressSizeBytes * 2 + 2) + "┴" + new string('─', editorLineBytes * 3 + 1) + "╢\n" +
                    "║" + statusLine + "║\n" +
                    "║ SEL  ASM: " + disasmSelected.PadRight(addressSizeBytes * 2 + editorLineBytes * 3 - 7) + "║\n" +
                    "╚" + new string('═', addressSizeBytes * 2 + 2 + 1 + editorLineBytes * 3 + 1) + "╝",
                    FontColor.Black, FontColor.BrightGreen, surface.CharacterToScreenSize(positionX, positionY, fontString), overrides);
            }

            public override bool ProcessKey(Keys key, int scanCode, InputState action, ModifierKeys modifiers)
            {
                if (key == Keys.Right && action != InputState.Release && modifiers.HasFlag(ModifierKeys.Control))
                {
                    InstructionSet.Disassemble(vm.Memory.AsMemory(selectedHalfByte / 2), out var size);
                    if (size == 0) size = 1;
                    selectedHalfByte = Math.Min(size * 2 + selectedHalfByte & 0xFFFE, vm.Memory.Length * 2 - 2);
                    fontStringDirty = true;
                    return true;
                }

                if (key == Keys.Left && action != InputState.Release && selectedHalfByte > 0) { --selectedHalfByte; fontStringDirty = true; return true; }
                if (key == Keys.Right && action != InputState.Release && selectedHalfByte < vm.Memory.Length * 2 - 1) { ++selectedHalfByte; fontStringDirty = true; return true; }

                if (key == Keys.Up && action != InputState.Release && selectedHalfByte >= editorLineBytes * 2) { selectedHalfByte -= editorLineBytes * 2; fontStringDirty = true; return true; }
                if (key == Keys.Down && action != InputState.Release && selectedHalfByte < vm.Memory.Length * 2 - editorLineBytes * 2) { selectedHalfByte += editorLineBytes * 2; fontStringDirty = true; return true; }

                void UpdateHalfByteWithNumber(int n)
                {
                    if (selectedHalfByte % 2 == 1)
                        vm.Memory[selectedHalfByte / 2] = (byte)(vm.Memory[selectedHalfByte / 2] & 0xF0 | n);
                    else
                        vm.Memory[selectedHalfByte / 2] = (byte)(vm.Memory[selectedHalfByte / 2] & 0xF | (n << 4));

                    if (selectedHalfByte < vm.Memory.Length * 2 - 1)
                        ++selectedHalfByte;

                    fontStringDirty = true;
                }

                if (key >= Keys.Numpad0 && key <= Keys.Numpad9 && action != InputState.Release) { UpdateHalfByteWithNumber(key - Keys.Numpad0); return true; }
                if (key >= Keys.Alpha0 && key <= Keys.Alpha9 && action != InputState.Release) { UpdateHalfByteWithNumber(key - Keys.Alpha0); return true; }
                if (key >= Keys.A && key <= Keys.F && action != InputState.Release) { UpdateHalfByteWithNumber(key - Keys.A + 10); return true; }

                if (key == Keys.F11 && action != InputState.Release && !done && !running)
                {
                    vm.RunNextInstruction();
                    fontStringDirty = true;
                }
                if (key == Keys.F5 && modifiers == 0 && action == InputState.Press && !done && !running)
                    running = true;
                if (key == Keys.F5 && modifiers.HasFlag(ModifierKeys.Shift) && action == InputState.Press && !done && running)
                    running = false;
                if (key == Keys.R && action == InputState.Press && !running)
                {
                    vm.Reset();
                    done = false;
                    fontStringDirty = true;
                }

                return false;
            }

            TimeSpan partialElapsedTime;
            const double InstructionRunFrequencyMSec = 1000.0 / 60.0;
            public override void UpdateLogic(TimeSpan elapsedTime)
            {
                if (!running) return;

                // execute 1 instruction per frame at 60 fps
                var totalTime = elapsedTime + partialElapsedTime;
                var cycles = (int)(totalTime.TotalMilliseconds / InstructionRunFrequencyMSec);
                if (cycles > 0)
                {
                    vm.RunNextInstruction(cycles);
                    fontStringDirty = true;
                    partialElapsedTime = TimeSpan.FromTicks((long)(totalTime.TotalMilliseconds - cycles * InstructionRunFrequencyMSec * TimeSpan.TicksPerMillisecond));
                }
                else
                    partialElapsedTime += elapsedTime;
            }
        }
    }
}