using GLFW;
using SharpVk;
using System;
using System.Numerics;

namespace Pengu.Renderer.UI
{
    abstract class BaseWindow : IRenderableModule
    {
        protected readonly VulkanContext context;
        protected readonly VulkanContext.GameSurface surface;
        protected VulkanContext.FontString fontString;
        protected bool fontStringDirty = true;
        protected int positionX, positionY;

        bool dragging;

        Vector2 lastMouseCharacterPosition, newMouseCharacterPosition;

        public BaseWindow(VulkanContext context, VulkanContext.GameSurface surface) =>
            (this.context, this.surface) = (context, surface);

        protected abstract void FillFontString(bool first);

        bool firstFillFontString = true;
        public virtual CommandBuffer[] PreRender(uint nextImage)
        {
            if (fontStringDirty)
            {
                FillFontString(firstFillFontString);
                firstFillFontString = fontStringDirty = false;
            }

            return Array.Empty<CommandBuffer>();
        }

        public virtual bool ProcessKey(Keys key, int scanCode, InputState action, ModifierKeys modifiers) => false;

        public virtual bool ProcessMouseButton(MouseButton button, InputState action, ModifierKeys modifiers)
        {
            if (newMouseCharacterPosition.X < positionX || newMouseCharacterPosition.Y < positionY ||
                newMouseCharacterPosition.X > positionX + fontString.Width || newMouseCharacterPosition.Y > positionY + fontString.Height)
            {
                return false;
            }


            if (button == MouseButton.Left && action == InputState.Press)
                dragging = true;
            else if (button == MouseButton.Left && action == InputState.Release)
            {
                dragging = false;
                var dXY = newMouseCharacterPosition - lastMouseCharacterPosition;
                (positionX, positionY) = (positionX + (int)dXY.X, positionY + (int)dXY.Y);
                lastMouseCharacterPosition = newMouseCharacterPosition;     // write the current mouse position
            }

            return false;
        }

        public virtual bool ProcessMouseMove(double x, double y)
        {
            newMouseCharacterPosition = surface.ScreenToCharacterSize(new Vector2((float)x, (float)y), fontString);

            if (dragging)
            {
                // update the offset
                fontString.Set(offset: surface.CharacterToScreenSize(
                    new Vector2(positionX, positionY) + newMouseCharacterPosition - lastMouseCharacterPosition, fontString));
            }
            else
                lastMouseCharacterPosition = newMouseCharacterPosition;

            return false;
        }

        public abstract void UpdateLogic(TimeSpan elapsedTime);
    }
}