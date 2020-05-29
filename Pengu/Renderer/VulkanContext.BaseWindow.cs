using GLFW;
using System;
using System.Numerics;

namespace Pengu.Renderer
{
    partial class VulkanContext
    {
        abstract class BaseWindow : IRenderableModule
        {
            protected readonly VulkanContext context;
            protected readonly GameSurface surface;
            protected FontString fontString;
            protected bool fontStringDirty = false;
            protected int positionX, positionY = 3;

            bool dragging;

            Vector2 lastMouseCharacterPosition, newMouseCharacterPosition;

            public BaseWindow(VulkanContext context, GameSurface surface) =>
                (this.context, this.surface) = (context, surface);

            protected abstract void FillFontString();

            public virtual void PreRender(uint nextImage)
            {
                if (fontStringDirty)
                {
                    FillFontString();
                    fontStringDirty = false;
                }
            }

            public abstract bool ProcessKey(Keys key, int scanCode, InputState action, ModifierKeys modifiers);

            public virtual bool ProcessMouseButton(MouseButton button, InputState action, ModifierKeys modifiers)
            {
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
}