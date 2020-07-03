using GLFW;
using Pengu.VirtualMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Threading.Tasks;

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
            base(context, surface, "ASSEMBLER", positionX: 90, positionY: 2,
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

                contentFontStringDirty = true;
            }

            if (key == Keys.Left && action != InputState.Release)
            {
                if (lineCharacterIndex == 0)
                {
                    if (verticalOffset + lineIndex > 0)
                        (lineIndex, lineCharacterIndex) = (lineIndex - 1, Lines[verticalOffset + lineIndex - 1].Length);
                }
                else
                    --lineCharacterIndex;

                contentFontStringDirty = true;
            }

            if (key == Keys.Down && action != InputState.Release && verticalOffset + lineIndex < Lines.Count - 1)
            {
                (lineIndex, lineCharacterIndex) = (lineIndex + 1, Math.Min(lineCharacterIndex, Lines[verticalOffset + lineIndex + 1].Length));
                contentFontStringDirty = true;
            }

            if (key == Keys.Up && action != InputState.Release && verticalOffset + lineIndex > 0)
            {
                (lineIndex, lineCharacterIndex) = (lineIndex - 1, Math.Min(lineCharacterIndex, Lines[verticalOffset + lineIndex - 1].Length));
                contentFontStringDirty = true;
            }

            if (key == Keys.Enter && action != InputState.Release)
            {
                if (lineCharacterIndex == Lines[verticalOffset + lineIndex].Length)
                    Lines.Insert(verticalOffset + lineIndex + 1, "");
                else
                {
                    var line = Lines[verticalOffset + lineIndex];
                    Lines.Insert(verticalOffset + lineIndex + 1, line[lineCharacterIndex..]);
                    Lines[verticalOffset + lineIndex] = line[..lineCharacterIndex];
                }

                (lineIndex, lineCharacterIndex) = (lineIndex + 1, 0);
                contentFontStringDirty = true;
            }

            // ensure cursor visible
            if (lineIndex < 0)
                (lineIndex, verticalOffset) = (0, verticalOffset + lineIndex);
            else if (lineIndex >= Height)
                (lineIndex, verticalOffset) = (Height - 1, verticalOffset + lineIndex - Height + 1);


            if (key == Keys.F6 && action == InputState.Press)
            {
                var asm = string.Join('\n', Lines);
                Array.Clear(vm.Memory, 0, vm.Memory.Length);
                InstructionSet.Assemble(vm, asm);
                vm.FireRefreshRequired();
                return true;
            }

            return false;
        }

        public override void UpdateLogic(TimeSpan elapsedTime) { }
    }
}
