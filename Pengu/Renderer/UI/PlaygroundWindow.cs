using MoreLinq;
using Pengu.VirtualMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Pengu.Renderer.UI
{
    class PlaygroundWindow : BaseWindow
    {
        private readonly (SevenSegmentDigitDisplayComponent ssdd, SevenSegmnetDigitRepresentation ssd, int posX, int posY)[] SevenSegmentDigitDisplayComponents;
        private readonly (string text, int posX, int posY)[] Labels;

        readonly StringBuilder contentStringBuilder;
        readonly int width, height;

        public override int Width => width;
        public override int Height => height;

        public PlaygroundWindow(VulkanContext context, VulkanContext.GameSurface surface, int positionX, int positionY, int width, int height,
            IEnumerable<(object data, int posX, int posY)>? components) :
            base(context, surface, "PLAY", positionX: positionX, positionY: positionY, chromeBackground: FontColor.BrightBlack)
        {
            (this.width, this.height) = (width, height);

            // build the string builder backing the display
            contentStringBuilder = new StringBuilder((width + 1) * height - 1);
            var emptyLine = new string(' ', width);
            for (int line = 0; line < height; ++line)
            {
                contentStringBuilder.Append(emptyLine);
                if (line < height - 1)
                    contentStringBuilder.Append('\n');
            }

            // decode the components
            SevenSegmentDigitDisplayComponents = components?.Select(w => (ssdd: w.data as SevenSegmentDigitDisplayComponent, w.posX, w.posY)).Where(w => !(w.ssdd is null))
                    .Select(w => (w.ssdd!, new SevenSegmnetDigitRepresentation(), w.posX, w.posY)).ToArray()
                ?? Array.Empty<(SevenSegmentDigitDisplayComponent ssdd, SevenSegmnetDigitRepresentation ssd, int posX, int posY)>();
            SevenSegmentDigitDisplayComponents.ForEach(c => c.ssdd.RefreshRequired += _ =>
            {
                if (c.ssdd.Memory[0] != c.ssd.Value)
                {
                    c.ssd.Value = c.ssdd.Memory[0];
                    contentFontStringDirty = true;
                }
            });

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            Labels = components?.Select(w => (text: w.data as string, w.posX, w.posY)).Where(w => !(w.text is null)).ToArray()
                ?? Array.Empty<(string, int, int)>();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }

        public override void UpdateLogic(TimeSpan elapsedTime)
        {
        }

        class SevenSegmnetDigitRepresentation
        {
            public const int Width = 4;
            public const int Height = 5;

            int value;
            public int Value
            {
                get => value;
                set
                {
                    this.value = value;

                    bool on(int b0) => (value & (1 << b0)) != 0;
                    bool on2(int b0, int b1) { int mask = (1 << b0) | (1 << b1); return (value & mask) == mask; }
                    bool on3(int b0, int b1, int b2) { int mask = (1 << b0) | (1 << b1) | (1 << b2); return (value & mask) == mask; }

                    Lines[0] = (on2(0, 5) ? "┌" : " ") + (on(0) ? "──" : "  ") + (on2(0, 1) ? "┐" : " ");
                    Lines[1] = (on(5) ? "│" : " ") + "  " + (on(1) ? "│" : " ");
                    Lines[2] = (on3(6, 5, 4) ? "├" : on2(6, 5) ? "└" : on2(6, 4) ? "┌" : on2(5, 4) ? "│" : " ") +
                        (on(6) ? "──" : "  ") +
                        (on3(6, 1, 2) ? "┤" : on2(6, 1) ? "┘" : on2(6, 2) ? "┐" : on2(1, 2) ? "│" : " ");
                    Lines[3] = (on(4) ? "│" : " ") + "  " + (on(2) ? "│" : " ");
                    Lines[4] = (on2(3, 4) ? "└" : " ") + (on(3) ? "──" : "  ") + (on2(3, 2) ? "┘" : " ");
                }
            }

            readonly string[] Lines = Enumerable.Range(0, Height).Select(idx => "    ").ToArray();
            public string this[int idx] => Lines[idx];
        }

        protected override void FillContentFontString(bool first)
        {
            // update the builder backing the display
            foreach (var (_, ssd, posX, posY) in SevenSegmentDigitDisplayComponents)
                for (int lineIdx = 0; lineIdx < SevenSegmnetDigitRepresentation.Height; ++lineIdx)
                    for (int charIdx = 0; charIdx < SevenSegmnetDigitRepresentation.Width; ++charIdx)
                        contentStringBuilder[(posY + lineIdx) * (Width + 1) + posX + charIdx] = ssd[lineIdx][charIdx];

            // since labels are static, only copy them once
            if (first) 
                foreach (var (text, posX, posY) in Labels)
                    for (int charIdx = 0; charIdx < text.Length; ++charIdx)
                        contentStringBuilder[posY * (Width + 1) + posX + charIdx] = text[charIdx];

            ContentFontString.Set(contentStringBuilder.ToString());

            if (first)
                ContentFontString.Set(null, chromeBackground, chromeForeground, surface.CharacterToScreenSize(positionX, positionY, ChromeFontString));
        }
    }
}
