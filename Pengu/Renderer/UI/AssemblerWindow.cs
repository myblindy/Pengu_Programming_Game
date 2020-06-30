using GLFW;
using Pengu.VirtualMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Pengu.Renderer.UI
{
    class AssemblerWindow : BaseWindow
    {
        readonly VM vm;

        const int editorLineCharacters = 35, editorLineRows = 30;

        public override int Width => editorLineCharacters;
        public override int Height => editorLineRows;

        readonly List<string> Lines = new List<string>();
        int verticalOffset, lineIndex, lineCharacterIndex;

        public AssemblerWindow(VulkanContext context, VulkanContext.GameSurface surface, string asm, VM vm) :
            base(context, surface, "ASSEMBLER", positionX: 8, positionY: 2,
                chromeBackground: FontColor.Black, chromeForeground: FontColor.BrightGreen)
        {
            this.vm = vm;
            Lines.AddRange(asm.Split('\n'));
        }

        protected override void FillContentFontString(bool first)
        {
            ContentFontString.Set(string.Join('\n',
                Lines.Skip(verticalOffset).Take(editorLineRows)
                    .Select((val, idx) => idx == lineIndex
                        ? lineCharacterIndex == val.Length ? $"{val}_" : $"{val[..(lineCharacterIndex + 1)]}\b_{val[(lineCharacterIndex + 1)..]}"
                        : val)));

            if (first)
                ContentFontString.Set(defaultBg: FontColor.Transparent, defaultFg: chromeForeground,
                    offset: surface.CharacterToScreenSize(positionX, positionY, ContentFontString));
        }

        public override bool ProcessCharacter(string character, ModifierKeys modifiers)
        {
            Lines[verticalOffset + lineIndex] = Lines[verticalOffset + lineIndex].Insert(lineCharacterIndex++, character);
            return contentFontStringDirty = true;
        }

        public override bool ProcessKey(Keys key, int scanCode, InputState action, ModifierKeys modifiers)
        {
            if (key == Keys.Right && action != InputState.Release)
            {
                if (lineCharacterIndex >= Lines[verticalOffset + lineIndex].Length)
                {
                    if (verticalOffset + lineIndex < Lines.Count - 1)
                        (lineIndex, lineCharacterIndex) = (lineIndex + 1, 0);
                }
                else
                    ++lineCharacterIndex;

                return contentFontStringDirty = true;
            }

            return false;
        }

        public override void UpdateLogic(TimeSpan elapsedTime) { }
    }
}
