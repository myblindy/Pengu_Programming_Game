using GLFW;
using MoreLinq;
using Pengu.Support;
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

        readonly List<string> lines = new List<string>();
        readonly List<FontOverride> syntaxHighlightOverride = new List<FontOverride>();
        bool syntaxHighlightDirty = true;
        int verticalOffset, lineIndex, lineCharacterIndex;

        public AssemblerWindow(VulkanContext context, PenguGameSurface surface, string? asm, VM vm) :
            base(context, surface, "ASSEMBLER", positionX: 90, positionY: 2, chromeBackground: FontColor.Black, chromeForeground: FontColor.BrightGreen)
        {
            this.vm = vm;
            if (!string.IsNullOrWhiteSpace(asm))
                lines.AddRange(asm.Split('\n'));
        }

        void UpdateSyntaxHighlight()
        {
            syntaxHighlightOverride.Clear();

            int idxAtLine = 0, lastLineForIdxAtLine = 0;
            int GetIndexAtLine(int line)
            {
                if (lastLineForIdxAtLine != line)
                    idxAtLine += lines.Skip(lastLineForIdxAtLine + verticalOffset).Take(line - lastLineForIdxAtLine).Sum(l => l.Length + 1);
                lastLineForIdxAtLine = line;
                return idxAtLine;
            }

            int offset = 0;

            foreach (var result in InstructionSet.SyntaxHighlight(lines.Skip(verticalOffset).Take(editorLineRows)))
            {
                if (offset == 0 && lineIndex < result.Line)
                    offset = lines[verticalOffset + lineIndex].Length == lineCharacterIndex ? 1 : 2;

                var realOffset = offset + (lineIndex != result.Line ? 0 :
                    result.Start > lineCharacterIndex
                    ? lines[verticalOffset + lineIndex].Length == lineCharacterIndex ? 1 : 2 : 0);

                var realCountOffset = lineIndex != result.Line ? 0 :
                    result.Start <= lineCharacterIndex && result.Start + result.Count > lineCharacterIndex
                    ? lines[verticalOffset + lineIndex].Length == lineCharacterIndex ? 1 : 2 : 0;

                syntaxHighlightOverride.Add(new FontOverride(GetIndexAtLine(result.Line) + result.Start + realOffset,
                    result.Count + realCountOffset, FontColor.Transparent, result.Type switch
                    {
                        SyntaxHighlightType.Comment => FontColor.BrightBlack,
                        SyntaxHighlightType.Label => FontColor.DarkCyan,
                        SyntaxHighlightType.Instruction => FontColor.DarkRed,
                        SyntaxHighlightType.Register => FontColor.BrightMagenta,
                        SyntaxHighlightType.NumericBaseSpecifier => FontColor.BrightRed,
                        SyntaxHighlightType.AtAddress => FontColor.DarkBlue,
                        _ => throw new NotImplementedException()
                    }, false));
            }
        }

        protected override void FillContentFontString(bool first)
        {
            if (syntaxHighlightDirty) UpdateSyntaxHighlight();

            var value = string.Join('\n', lines.Skip(verticalOffset).Take(editorLineRows)
                .Select((val, idx) => idx == lineIndex
                    ? lineCharacterIndex == val.Length
                        ? $"{val}_"
                        : string.Concat(val.AsSpan(0, lineCharacterIndex + 1), "\b_".AsSpan(), val.AsSpan(lineCharacterIndex + 1))
                    : val));
            ContentFontString.Set(value, overrides: syntaxHighlightOverride);

            if (first)
                ContentFontString.Set(defaultBg: FontColor.Transparent, defaultFg: chromeForeground,
                    offset: surface.CharacterToScreenSize(positionX, positionY, ContentFontString));
        }

        public override bool ProcessCharacter(string character, ModifierKeys modifiers)
        {
            lines[verticalOffset + lineIndex] = lines[verticalOffset + lineIndex].Insert(lineCharacterIndex++, character);
            syntaxHighlightDirty = true;
            return contentFontStringDirty = true;
        }

        public override bool ProcessKey(Keys key, int scanCode, InputState action, ModifierKeys modifiers)
        {
            if (key == Keys.Right && action != InputState.Release)
            {
                if (modifiers.HasFlag(ModifierKeys.Control))
                    FindNextWord(out lineIndex, out lineCharacterIndex);
                else if (lineCharacterIndex >= lines[verticalOffset + lineIndex].Length)
                {
                    if (verticalOffset + lineIndex < lines.Count - 1)
                        (lineIndex, lineCharacterIndex) = (lineIndex + 1, 0);
                }
                else
                    ++lineCharacterIndex;

                syntaxHighlightDirty = contentFontStringDirty = true;
            }

            if (key == Keys.Left && action != InputState.Release)
            {
                if (modifiers.HasFlag(ModifierKeys.Control))
                    FindPreviousWord(out lineIndex, out lineCharacterIndex);
                else if (lineCharacterIndex == 0)
                {
                    if (verticalOffset + lineIndex > 0)
                        (lineIndex, lineCharacterIndex) = (lineIndex - 1, lines[verticalOffset + lineIndex - 1].Length);
                }
                else
                    --lineCharacterIndex;

                syntaxHighlightDirty = contentFontStringDirty = true;
            }

            if (key == Keys.Down && action != InputState.Release && verticalOffset + lineIndex < lines.Count - 1)
            {
                (lineIndex, lineCharacterIndex) = (lineIndex + 1, Math.Min(lineCharacterIndex, lines[verticalOffset + lineIndex + 1].Length));
                syntaxHighlightDirty = contentFontStringDirty = true;
            }

            if (key == Keys.Up && action != InputState.Release && verticalOffset + lineIndex > 0)
            {
                (lineIndex, lineCharacterIndex) = (lineIndex - 1, Math.Min(lineCharacterIndex, lines[verticalOffset + lineIndex - 1].Length));
                syntaxHighlightDirty = contentFontStringDirty = true;
            }

            if (key == Keys.Enter && action != InputState.Release)
            {
                var line = lines[verticalOffset + lineIndex];
                if (lineCharacterIndex == line.Length)
                    lines.Insert(verticalOffset + lineIndex + 1, "");
                else
                {
                    lines.Insert(verticalOffset + lineIndex + 1, line[lineCharacterIndex..]);
                    lines[verticalOffset + lineIndex] = line[..lineCharacterIndex];
                }

                (lineIndex, lineCharacterIndex) = (lineIndex + 1, 0);
                syntaxHighlightDirty = contentFontStringDirty = true;
            }

            if (key == Keys.Backspace && action != InputState.Release)
                if (modifiers.HasFlag(ModifierKeys.Control))
                {
                    FindPreviousWord(out var newLineIndex, out var newLineCharacterIndex);

                    if (newLineIndex != lineIndex)
                    {
                        lines[verticalOffset + newLineIndex] = string.Concat(lines[verticalOffset + newLineIndex].AsSpan(0, newLineCharacterIndex), lines[verticalOffset + lineIndex].AsSpan(lineCharacterIndex));
                        lines.RemoveAt(verticalOffset + lineIndex);
                    }
                    else
                        lines[verticalOffset + lineIndex] = string.Concat(lines[verticalOffset + lineIndex].AsSpan(0, newLineCharacterIndex), lines[verticalOffset + lineIndex].AsSpan(lineCharacterIndex));
                    (lineIndex, lineCharacterIndex) = (newLineIndex, newLineCharacterIndex);
                    syntaxHighlightDirty = contentFontStringDirty = true;
                }
                else
                {
                    var line = lines[verticalOffset + lineIndex];
                    if (lineCharacterIndex > 0)
                    {
                        lines[verticalOffset + lineIndex] = string.Concat(line.AsSpan(0, lineCharacterIndex - 1), line.AsSpan(lineCharacterIndex--));
                        syntaxHighlightDirty = contentFontStringDirty = true;
                    }
                    else if (lineIndex + verticalOffset > 0)
                    {
                        var oldLength = lines[verticalOffset + lineIndex - 1].Length;
                        lines[verticalOffset + lineIndex - 1] += line;
                        lines.RemoveAt(verticalOffset + lineIndex);

                        (lineIndex, lineCharacterIndex) = (lineIndex - 1, oldLength);
                        syntaxHighlightDirty = contentFontStringDirty = true;
                    }
                }

            if (key == Keys.Delete && action != InputState.Release)
                if (modifiers.HasFlag(ModifierKeys.Control))
                {
                    FindNextWord(out var newLineIndex, out var newLineCharacterIndex);

                    if (newLineIndex != lineIndex)
                    {
                        lines[verticalOffset + lineIndex] = string.Concat(lines[verticalOffset + lineIndex].AsSpan(0, lineCharacterIndex), lines[verticalOffset + newLineIndex].AsSpan(newLineCharacterIndex));
                        lines.RemoveAt(verticalOffset + newLineIndex);
                    }
                    else
                        lines[verticalOffset + lineIndex] = string.Concat(lines[verticalOffset + lineIndex].AsSpan(0, lineCharacterIndex), lines[verticalOffset + lineIndex].AsSpan(newLineCharacterIndex));
                    syntaxHighlightDirty = contentFontStringDirty = true;
                }
                else
                {
                    var line = lines[verticalOffset + lineIndex];
                    if (lineCharacterIndex < line.Length)
                    {
                        lines[verticalOffset + lineIndex] = string.Concat(line.AsSpan(0, lineCharacterIndex), line.AsSpan(lineCharacterIndex + 1));
                        syntaxHighlightDirty = contentFontStringDirty = true;
                    }
                    else if (lineIndex + verticalOffset < lines.Count - 1)
                    {
                        lines[verticalOffset + lineIndex] += lines[verticalOffset + lineIndex + 1];
                        lines.RemoveAt(verticalOffset + lineIndex + 1);
                        syntaxHighlightDirty = contentFontStringDirty = true;
                    }
                }

            if (key == Keys.Home && action == InputState.Press && lines.Any())
            {
                if (modifiers.HasFlag(ModifierKeys.Control))
                    lineIndex = lineCharacterIndex = 0;
                else
                    lineCharacterIndex = 0;
                syntaxHighlightDirty = contentFontStringDirty = true;
            }

            if (key == Keys.End && action == InputState.Press && lines.Any())
            {
                if (modifiers.HasFlag(ModifierKeys.Control))
                    (lineIndex, lineCharacterIndex) = (lines.Count - verticalOffset - 1, lines[^1].Length);
                else
                    lineCharacterIndex = lines[verticalOffset + lineIndex].Length;
                syntaxHighlightDirty = contentFontStringDirty = true;
            }

            if (key == Keys.PageUp && action != InputState.Release && lineIndex + verticalOffset > 0)
            {
                lineIndex = Math.Max(-verticalOffset, lineIndex - editorLineRows);
                syntaxHighlightDirty = contentFontStringDirty = true;
            }

            if (key == Keys.PageDown && action != InputState.Release && lineIndex + verticalOffset < lines.Count - 1)
            {
                lineIndex = Math.Min(lines.Count - 1 - verticalOffset, lineIndex + editorLineRows);
                syntaxHighlightDirty = contentFontStringDirty = true;
            }

            // ensure cursor visible
            if (lineIndex < 0)
            {
                (lineIndex, verticalOffset) = (0, verticalOffset + lineIndex);
                contentFontStringDirty = syntaxHighlightDirty = true;
            }
            else if (lineIndex >= Height)
            {
                (lineIndex, verticalOffset) = (Height - 1, verticalOffset + lineIndex - Height + 1);
                contentFontStringDirty = syntaxHighlightDirty = true;
            }

            if (key == Keys.F6 && action == InputState.Press)
            {
                // queue the assembly task
                _ = InstructionSet.AssembleAsync(vm, string.Join('\n', lines));
                return true;
            }

            return false;

            void FindPreviousWord(out int newLineIndex, out int newLineCharacterIndex)
            {
                (newLineIndex, newLineCharacterIndex) = (lineIndex, lineCharacterIndex);

                if (lineCharacterIndex == 0)
                {
                    if (verticalOffset + lineIndex > 0)
                    {
                        var previousLine = lines[verticalOffset + lineIndex - 1];
                        (newLineIndex, newLineCharacterIndex) = (lineIndex - 1, previousLine.Length - previousLine.CountWhileLast(char.IsWhiteSpace));
                    }
                }
                else
                {
                    var line = lines[verticalOffset + lineIndex];

                    // 1. move along spaces
                    while (newLineCharacterIndex > 0 && char.IsWhiteSpace(line[newLineCharacterIndex - 1])) --newLineCharacterIndex;

                    // 2. move along either letters/digits or symbols 
                    if (newLineCharacterIndex > 0)
                        if (char.IsLetterOrDigit(line[newLineCharacterIndex - 1]))
                            while (newLineCharacterIndex > 0 && char.IsLetterOrDigit(line[newLineCharacterIndex - 1])) --newLineCharacterIndex;
                        else if (char.IsSymbol(line[newLineCharacterIndex - 1]) || char.IsPunctuation(line[newLineCharacterIndex - 1]))
                            while (newLineCharacterIndex > 0 && (char.IsSymbol(line[newLineCharacterIndex - 1]) || char.IsPunctuation(line[newLineCharacterIndex - 1]))) --newLineCharacterIndex;
                }
            }

            void FindNextWord(out int newLineIndex, out int newLineCharacterIndex)
            {
                (newLineIndex, newLineCharacterIndex) = (lineIndex, lineCharacterIndex);

                if (lineCharacterIndex >= lines[verticalOffset + lineIndex].Length)
                {
                    if (verticalOffset + lineIndex < lines.Count - 1)
                        (newLineIndex, newLineCharacterIndex) = (lineIndex + 1, lines[verticalOffset + lineIndex + 1].CountWhile(char.IsWhiteSpace));
                }
                else
                {
                    var line = lines[verticalOffset + lineIndex];

                    // 1. move along either letters/digits or symbols 
                    if (char.IsLetterOrDigit(line[newLineCharacterIndex]))
                        while (newLineCharacterIndex < line.Length && char.IsLetterOrDigit(line[newLineCharacterIndex])) ++newLineCharacterIndex;
                    else if (char.IsSymbol(line[newLineCharacterIndex]) || char.IsPunctuation(line[newLineCharacterIndex]))
                        while (newLineCharacterIndex < line.Length && (char.IsSymbol(line[newLineCharacterIndex]) || char.IsPunctuation(line[newLineCharacterIndex]))) ++newLineCharacterIndex;

                    // 2. move along spaces
                    while (newLineCharacterIndex < line.Length && char.IsWhiteSpace(line[newLineCharacterIndex])) ++newLineCharacterIndex;
                }
            }
        }

        public override void UpdateLogic(TimeSpan elapsedTime) { }
    }
}
