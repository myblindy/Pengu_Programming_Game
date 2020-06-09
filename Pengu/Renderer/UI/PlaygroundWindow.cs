using Pengu.VirtualMachine;
using System;
using System.Linq;
using System.Numerics;

namespace Pengu.Renderer.UI
{
    class PlaygroundWindow : BaseWindow
    {
        private const VulkanContext.FontColor fgDigitColor = VulkanContext.FontColor.White;

        private readonly VM vm;
        private readonly Digit[] Digits = Enumerable.Range(0, 2).Select(_ => new Digit()).ToArray();

        public PlaygroundWindow(VulkanContext context, VulkanContext.GameSurface surface, VM vm) : base(context, surface)
        {
            this.vm = vm;
            fontString = context.monospaceFont.AllocateString(new Vector2(-1f * context.extent.AspectRatio, -1), 0.055f);

            (positionX, positionY) = (0, 8);

            vm.RegisterInterrupt(0x45, vm => SetDigit(vm.Registers[0]));
        }

        private void SetDigit(int val)
        {
            Digits[(val & 0x80) > 0 ? 1 : 0].Value = val;
            fontStringDirty = true;
        }

        public override void UpdateLogic(TimeSpan elapsedTime)
        {
        }

        class Digit
        {
            int value;
            public int Value
            {
                get => value;
                set
                {
                    this.value = value;

                    Lines[0] = (value & (1 << 0)) > 0 ? " -- " : "    ";
                    Lines[1] = ((value & (1 << 5)) > 0 ? "|" : " ") + "  " + ((value & (1 << 1)) > 0 ? "|" : " ");
                    Lines[2] = (value & (1 << 6)) > 0 ? " -- " : "    ";
                    Lines[3] = ((value & (1 << 4)) > 0 ? "|" : " ") + "  " + ((value & (1 << 2)) > 0 ? "|" : " ");
                    Lines[4] = (value & (1 << 3)) > 0 ? " -- " : "    ";
                }
            }

            readonly string[] Lines = Enumerable.Range(0, 5).Select(idx => "    ").ToArray();
            public string this[int idx] => Lines[idx];
        }

        static readonly FontOverride[] fontOverrides = new[]
        {
            new FontOverride(16, 9, VulkanContext.FontColor.BrightBlack, fgDigitColor, false),
            new FontOverride(30, 9, VulkanContext.FontColor.BrightBlack, fgDigitColor, false),
            new FontOverride(44, 9, VulkanContext.FontColor.BrightBlack, fgDigitColor, false),
            new FontOverride(58, 9, VulkanContext.FontColor.BrightBlack, fgDigitColor, false),
            new FontOverride(72, 9, VulkanContext.FontColor.BrightBlack, fgDigitColor, false),
        };

        protected override void FillFontString(bool first)
        {
            fontString.Set((
                "╔═══ PLAY ══╗\n" +
                $"║ {Digits[0][0]} {Digits[1][0]} ║\n" +
                $"║ {Digits[0][1]} {Digits[1][1]} ║\n" +
                $"║ {Digits[0][2]} {Digits[1][2]} ║\n" +
                $"║ {Digits[0][3]} {Digits[1][3]} ║\n" +
                $"║ {Digits[0][4]} {Digits[1][4]} ║\n" +
                "╚═══════════╝").Replace(' ', VulkanContext.Font.PrintableSpace),
                VulkanContext.FontColor.BrightBlack, VulkanContext.FontColor.Black,
                surface.CharacterToScreenSize(positionX, positionY, fontString), fontOverrides);
        }
    }
}
