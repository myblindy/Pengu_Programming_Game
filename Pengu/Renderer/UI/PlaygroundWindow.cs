using Pengu.VirtualMachine;
using System;
using System.Linq;
using System.Numerics;

namespace Pengu.Renderer.UI
{
    class PlaygroundWindow : BaseWindow
    {
        private readonly Digit[] Digits = Enumerable.Range(0, 2).Select(_ => new Digit()).ToArray();

        public PlaygroundWindow(VulkanContext context, VulkanContext.GameSurface surface, VM vm) :
            base(context, surface, "PLAY", positionX: 1, positionY: 8, chromeBackground: FontColor.BrightBlack)
        {
            vm.RegisterInterrupt(0x45, vm => SetDigit(vm.Registers[0]));
        }

        private void SetDigit(int val)
        {
            Digits[val >> 7].Value = val;
            contentFontStringDirty = true;
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

            readonly string[] Lines = Enumerable.Range(0, 5).Select(idx => "    ").ToArray();
            public string this[int idx] => Lines[idx];
        }

        protected override void FillContentFontString(bool first)
        {
            ContentFontString.Set(
                $" {Digits[0][0]} {Digits[1][0]} \n" +
                $" {Digits[0][1]} {Digits[1][1]} \n" +
                $" {Digits[0][2]} {Digits[1][2]} \n" +
                $" {Digits[0][3]} {Digits[1][3]} \n" +
                $" {Digits[0][4]} {Digits[1][4]} ");

            if (first)
                ContentFontString.Set(null, chromeBackground, chromeForeground, surface.CharacterToScreenSize(positionX, positionY, ChromeFontString));
        }
    }
}
