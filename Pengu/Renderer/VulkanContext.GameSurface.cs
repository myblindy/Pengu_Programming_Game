using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Linq;
using GLFW;
using Pengu.VirtualMachine;

namespace Pengu.Renderer
{
    partial class VulkanContext
    {
        class GameSurface : IRenderableModule
        {
            readonly VulkanContext context;
            readonly FontString hexEditorFontString;
            readonly VM vm;

            const int editorLineBytes = 0x10;
            const int addressSizeBytes = 2;
            const int linesCount = 30;

            int selectedHalfByte;
            bool dirty = false;

            public GameSurface(VulkanContext context, VM vm)
            {
                this.context = context;
                this.vm = vm;

                hexEditorFontString = context.monospaceFont.AllocateString(new Vector2(-1f * context.extent.AspectRatio, -0.9f), 0.055f);
                FillFontString();
                SelectByteInFontString();
            }

            private void FillFontString()
            {
                static string TryGetHexAt(byte[] array, int index) => index < array.Length ? array[index].ToString("X2") : "..";

                var header = new string(' ', addressSizeBytes * 2) + " | " + string.Join(' ', Enumerable.Range(0, editorLineBytes).Select(idx => $"{idx:X2}"));
                hexEditorFontString.Value =
                    header + "\n" +
                    new string('-', header.Length) + "\n" +
                    string.Join('\n', Enumerable.Range(0, linesCount).Select(lineIdx =>
                        $"{lineIdx * editorLineBytes:X4} | {string.Join(' ', Enumerable.Range(0, editorLineBytes).Select(idx => TryGetHexAt(vm.Memory, lineIdx * editorLineBytes + idx)))}")) + "\n\n" +
                    InstructionSet.Disassemble(vm.Memory.AsMemory(selectedHalfByte / 2), out _);
            }

            void SelectByteInFontString()
            {
                var line = Math.DivRem(selectedHalfByte, editorLineBytes * 2, out var halfIndexInLine);

                var headerAddress0 = 7 + 3 * (halfIndexInLine / 2);
                var leftAddress0 = (2 + line) * (7 + 3 * editorLineBytes);
                var value0 = leftAddress0 + 4 + 3 + 3 * (halfIndexInLine / 2) + halfIndexInLine % 2;

                hexEditorFontString.SelectedCharacters = new[]
                {
                    headerAddress0, headerAddress0 + 1,
                    leftAddress0, leftAddress0 + 1, leftAddress0 + 2, leftAddress0 + 3,
                    value0
                };
            }

            public void UpdateLogic()
            {
            }

            public void PreRender()
            {
                if (dirty)
                {
                    FillFontString();
                    SelectByteInFontString();
                    dirty = false;
                }
            }

            public bool ProcessKey(Keys key, int scanCode, InputState state, ModifierKeys modifiers)
            {
                if (key == Keys.Left && state != InputState.Release && selectedHalfByte > 0) { --selectedHalfByte; dirty = true; return true; }
                if (key == Keys.Right && state != InputState.Release && selectedHalfByte < vm.Memory.Length * 2 - 1) { ++selectedHalfByte; dirty = true; return true; }

                if (key == Keys.Up && state != InputState.Release && selectedHalfByte >= editorLineBytes * 2) { selectedHalfByte -= editorLineBytes * 2; dirty = true; return true; }
                if (key == Keys.Down && state != InputState.Release && selectedHalfByte < vm.Memory.Length * 2 - editorLineBytes * 2) { selectedHalfByte += editorLineBytes * 2; dirty = true; return true; }

                void UpdateHalfByteWithNumber(int n)
                {
                    if (selectedHalfByte % 2 == 1)
                        vm.Memory[selectedHalfByte / 2] = (byte)(vm.Memory[selectedHalfByte / 2] & 0xF0 | n);
                    else
                        vm.Memory[selectedHalfByte / 2] = (byte)(vm.Memory[selectedHalfByte / 2] & 0xF | (n << 4));

                    if (selectedHalfByte < vm.Memory.Length * 2 - 1)
                        ++selectedHalfByte;

                    dirty = true;
                }

                if (key >= Keys.Numpad0 && key <= Keys.Numpad9 && state != InputState.Release) { UpdateHalfByteWithNumber(key - Keys.Numpad0); return true; }
                if (key >= Keys.Alpha0 && key <= Keys.Alpha9 && state != InputState.Release) { UpdateHalfByteWithNumber(key - Keys.Alpha0); return true; }
                if (key >= Keys.A && key <= Keys.F && state != InputState.Release) { UpdateHalfByteWithNumber(key - Keys.A + 10); return true; }

                return false;
            }
        }
    }
}
