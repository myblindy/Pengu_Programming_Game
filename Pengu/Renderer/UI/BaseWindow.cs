using GLFW;
using SharpVk;
using SixLabors.ImageSharp.Advanced;
using System;
using System.Linq;
using System.Numerics;

namespace Pengu.Renderer.UI
{
    abstract class BaseWindow : IRenderableModule
    {
        protected readonly VulkanContext context;
        protected readonly VulkanContext.GameSurface surface;
        protected int positionX, positionY;

        public VulkanContext.FontString ChromeFontString { get; private set; }
        private bool chromeFontStringDirty = true;

        protected string ChromeTitle;
        protected VulkanContext.FontColor ChromeBackground = VulkanContext.FontColor.Black, ChromeForeground = VulkanContext.FontColor.White;

        public VulkanContext.FontString ContentFontString { get; protected set; }
        protected bool contentFontStringDirty = true;

        bool dragging;

        Vector2 lastMouseCharacterPosition, newMouseCharacterPosition;

        protected VulkanContext.FontString AllocateWindowFontString() =>
             context.monospaceFont.AllocateString(new Vector2(-1f * context.extent.AspectRatio, -1), 0.055f);

        public BaseWindow(VulkanContext context, VulkanContext.GameSurface surface)
        {
            (this.context, this.surface) = (context, surface);
            ChromeFontString = AllocateWindowFontString();
        }

        protected abstract void FillContentFontString(bool first);

        bool firstFillFontString = true;
        public virtual CommandBuffer[] PreRender(uint nextImage)
        {
            if (chromeFontStringDirty || contentFontStringDirty)
            {
                FillContentFontString(firstFillFontString);
                if (firstFillFontString)
                {
                    ContentFontString.Changed += () => FillChromeFontString(false);
                    FillChromeFontString(true);
                }

                firstFillFontString = chromeFontStringDirty = contentFontStringDirty = false;
            }

            return Array.Empty<CommandBuffer>();
        }

        private void FillChromeFontString(bool first)
        {
            var titleHalfOffset = (ContentFontString.Width - ChromeTitle.Length - 2) / 2;
            var titleHalfOffsetExtra = (ContentFontString.Width - ChromeTitle.Length - 2) % 2;
            var line = "║" + new string(' ', ContentFontString.Width) + "║\n";

            ChromeFontString.Set(
                "╔" + new string('═', titleHalfOffset + titleHalfOffsetExtra) +
                    VulkanContext.Font.PrintableSpace + ChromeTitle.Replace(' ', VulkanContext.Font.PrintableSpace) + VulkanContext.Font.PrintableSpace +
                    new string('═', titleHalfOffset) + "╗\n" +
                string.Concat(Enumerable.Repeat(line, ContentFontString.Height)) +
                "╚" + new string('═', ContentFontString.Width) + "╝");

            if (first)
                ChromeFontString.Set(defaultBg: ChromeBackground, defaultFg: ChromeForeground,
                    offset: surface.CharacterToScreenSize(positionX - 1, positionY - 1, ChromeFontString), fillBackground: true);
        }

        public virtual bool ProcessKey(Keys key, int scanCode, InputState action, ModifierKeys modifiers) => false;

        public virtual bool ProcessMouseButton(MouseButton button, InputState action, ModifierKeys modifiers)
        {
            var newPos = new Vector2(positionX, positionY) + newMouseCharacterPosition - lastMouseCharacterPosition;
            if (newMouseCharacterPosition.X < newPos.X - 1 || newMouseCharacterPosition.Y < newPos.Y - 1 ||
                newMouseCharacterPosition.X > newPos.X - 1 + ChromeFontString.Width || newMouseCharacterPosition.Y > newPos.Y - 1 + ChromeFontString.Height)
            {
                return false;
            }

            surface.FocusedWindow = this;

            if (button == MouseButton.Left && action == InputState.Press)
                dragging = true;
            else if (button == MouseButton.Left && action == InputState.Release)
            {
                dragging = false;
                var dXY = newMouseCharacterPosition - lastMouseCharacterPosition;
                (positionX, positionY) = (positionX + (int)dXY.X, positionY + (int)dXY.Y);
                lastMouseCharacterPosition = newMouseCharacterPosition;     // write the current mouse position
            }

            return true;
        }

        public virtual bool ProcessMouseMove(double x, double y)
        {
            newMouseCharacterPosition = surface.ScreenToCharacterSize(new Vector2((float)x * 2, (float)y * 2), ChromeFontString);

            if (dragging && surface.FocusedWindow == this)
            {
                // update the offset
                ChromeFontString.Set(offset: surface.CharacterToScreenSize(
                    new Vector2(positionX - 1, positionY - 1) + newMouseCharacterPosition - lastMouseCharacterPosition, ChromeFontString));
                ContentFontString.Set(offset: surface.CharacterToScreenSize(
                    new Vector2(positionX, positionY) + newMouseCharacterPosition - lastMouseCharacterPosition, ContentFontString));
            }
            else
                lastMouseCharacterPosition = newMouseCharacterPosition;

            return false;
        }

        public abstract void UpdateLogic(TimeSpan elapsedTime);
    }
}