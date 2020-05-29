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

            const int editorLineBytes = 0x10;
            const int addressSizeBytes = 2;
            const int linesCount = 15;

            int selectedHalfByte;
            int positionX, positionY = 3;

            public HexEditorWindow(VulkanContext context, GameSurface surface, VM vm) : base(context, surface)
            {
                this.vm = vm;

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

                fontString.Set(
                    "╔" + frameForAddress + "╤" + new string('═', titleHalfOffset - 4 - 1 - addressSizeBytes * 2 - 2) + " " + title + " " + new string('═', titleHalfOffset) + "╗\n" +
                    "║" + new string(' ', addressSizeBytes * 2 + 2) + "│" + string.Concat(Enumerable.Range(0, editorLineBytes).Select(idx => $" {idx:X2}")) + " ║\n" +
                    "╟" + new string('─', addressSizeBytes * 2 + 2) + "┼" + new string('─', editorLineBytes * 3 + 1) + "╢\n" +
                    string.Concat(Enumerable.Range(0, linesCount).Select(lineIdx =>
                        $"║ {lineIdx * editorLineBytes:X4} │ {string.Join(' ', Enumerable.Range(0, editorLineBytes).Select(idx => TryGetHexAt(vm.Memory, lineIdx * editorLineBytes + idx)))} ║\n")) +
                    "╟" + new string('─', addressSizeBytes * 2 + 2) + "┴" + new string('─', editorLineBytes * 3 + 1) + "╢\n" +
                    "║ ASM: " + (InstructionSet.Disassemble(vm.Memory.AsMemory(selectedHalfByte / 2), out _) ?? "---").PadRight(addressSizeBytes * 2 + editorLineBytes * 3 - 2) + "║\n" +
                    "╚" + new string('═', addressSizeBytes * 2 + 2 + 1 + editorLineBytes * 3 + 1) + "╝",
                    FontColor.Black, FontColor.BrightGreen, surface.CharacterToScreenSize(positionX, positionY, fontString), new[]
                    {
                        (1 + frameForAddress.Length + 1 + titleHalfOffset - 4 - 1 - addressSizeBytes * 2 - 2, title.Length + 2, FontColor.White, FontColor.Black, false),
                        (headerAddress0, 2, FontColor.Black, FontColor.BrightCyan, true),
                        (leftAddress0, 4, FontColor.Black, FontColor.BrightCyan, true),
                        (value0, 1, FontColor.Black, FontColor.BrightCyan, true),
                    });
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

                return false;
            }

            public override void UpdateLogic(TimeSpan elapsedTime) { }
        }
    }
}