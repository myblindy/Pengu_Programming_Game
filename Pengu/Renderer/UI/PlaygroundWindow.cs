using Pengu.VirtualMachine;
using System;
using System.Numerics;

namespace Pengu.Renderer.UI
{
    class PlaygroundWindow : BaseWindow
    {
        private readonly VM vm;
        private int val0, val1;

        public PlaygroundWindow(VulkanContext context, VulkanContext.GameSurface surface, VM vm) : base(context, surface)
        {
            this.vm = vm;
            fontString = context.monospaceFont.AllocateString(new Vector2(-1f * context.extent.AspectRatio, -0.9f), 0.055f);

            val0 = val1 = 0;

            vm.RegisterInterrupt(0x45, vm => SetDigit(vm.Registers[0]));
        }

        static readonly (int bit, FontOverride @override)[] SegmentOverrides = new[]
        {
            (0, new FontOverride(1, 2, VulkanContext.FontColor.Black, VulkanContext.FontColor.BrightGreen, false)),
            (5, new FontOverride(10, 1, VulkanContext.FontColor.Black, VulkanContext.FontColor.BrightGreen, false)),
            (1, new FontOverride(13, 1, VulkanContext.FontColor.Black, VulkanContext.FontColor.BrightGreen, false)),
            (6, new FontOverride(21, 2, VulkanContext.FontColor.Black, VulkanContext.FontColor.BrightGreen, false)),
            (4, new FontOverride(30, 1, VulkanContext.FontColor.Black, VulkanContext.FontColor.BrightGreen, false)),
            (2, new FontOverride(33, 1, VulkanContext.FontColor.Black, VulkanContext.FontColor.BrightGreen, false)),
            (3, new FontOverride(41, 2, VulkanContext.FontColor.Black, VulkanContext.FontColor.BrightGreen, false)),
        };

        private void SetDigit(int val)
        {
            if ((val & 0x80) != 0) val1 = val; else val0 = val;
            var overrideCount = 0;

            // count 1 bits (overrides)
            CountOnes(ref overrideCount, val0 & 0x7F);
            CountOnes(ref overrideCount, val1 & 0x7F);

            // add the overrides in order to a vector
            var overrides = new FontOverride[overrideCount];
            var idx = 0;
            foreach (var (bit, @override) in SegmentOverrides)
            {
                if ((val0 & (1 << bit)) != 0)
                    overrides[idx++] = @override;
                if ((val1 & (1 << bit)) != 0)
                {
                    var offsetOverride = @override;
                    offsetOverride.start += 5;
                    overrides[idx++] = offsetOverride;
                }
            }

            Array.Sort(overrides, (a, b) => a.start.CompareTo(b.start));

            fontString.Set(overrides: overrides);

            static void CountOnes(ref int overrideCount, int n)
            {
                while (n != 0)
                {
                    n &= n - 1;
                    overrideCount++;
                }
            }
        }

        public override void UpdateLogic(TimeSpan elapsedTime)
        {
        }

        protected override void FillFontString()
        {
            fontString.Set(
                " --   -- \n" +
                "|  | |  |\n" +
                " --   -- \n" +
                "|  | |  |\n" +
                " --   -- ",
                VulkanContext.FontColor.Black, VulkanContext.FontColor.Black, surface.CharacterToScreenSize(positionX, positionY, fontString));
        }
    }
}
