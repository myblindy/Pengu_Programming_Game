using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Linq;
using GLFW;

namespace Pengu.Renderer
{
    partial class VulkanContext
    {
        class GameSurface : IRenderableModule
        {
            readonly VulkanContext context;
            readonly FontString hexEditorFontString;

            const int editorLineBytes = 0x10;
            const int addressSizeBytes = 2;
            const int linesCount = 32;

            int selectedByte;
            bool dirty = false;

            public GameSurface(VulkanContext context)
            {
                this.context = context;

                byte mem = 0;

                hexEditorFontString = context.monospaceFont.AllocateString(new Vector2(-1f * context.extent.AspectRatio, -0.9f), 0.055f);

                var header = new string(' ', addressSizeBytes * 2) + " | " + string.Join(' ', Enumerable.Range(0, editorLineBytes).Select(idx => $"{idx:X2}"));
                hexEditorFontString.Value =
                    header + "\n" +
                    new string('-', header.Length) + "\n" +
                    string.Join('\n', Enumerable.Range(0, linesCount).Select(lineIdx =>
                        $"{lineIdx * editorLineBytes:X4} | {string.Join(' ', Enumerable.Range(0, editorLineBytes).Select(idx => $"{mem++:X2}"))}"));

                SelectByteInFontString();
            }

            void SelectByteInFontString()
            {
                var line = Math.DivRem(selectedByte, editorLineBytes, out var indexInLine);

                var headerAddress0 = 7 + 3 * indexInLine;
                var leftAddress0 = (2 + line) * (7 + 3 * editorLineBytes);
                var value0 = leftAddress0 + 4 + 3 + 3 * indexInLine;

                hexEditorFontString.SelectedCharacters = new[]
                {
                    headerAddress0, headerAddress0 + 1,
                    leftAddress0, leftAddress0 + 1, leftAddress0 + 2, leftAddress0 + 3,
                    value0, value0 + 1
                };
            }

            public void UpdateLogic()
            {
            }

            public void PreRender()
            {
                if (dirty)
                {
                    SelectByteInFontString();
                    dirty = false;
                }
            }

            public bool ProcessKey(Keys key, int scanCode, InputState state, ModifierKeys modifiers)
            {
                if (key == Keys.Left && state != InputState.Release && selectedByte > 0) { --selectedByte; dirty = true; return true; }
                if (key == Keys.Right && state != InputState.Release) { ++selectedByte; dirty = true; return true; }

                if (key == Keys.Up && state != InputState.Release && selectedByte >= editorLineBytes) { selectedByte -= editorLineBytes; dirty = true; return true; }
                if (key == Keys.Down && state != InputState.Release) { selectedByte += editorLineBytes; dirty = true; return true; }

                return false;
            }
        }
    }
}
