using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Linq;
using GLFW;
using Pengu.VirtualMachine;
using System.Diagnostics;
using SharpVk.Multivendor;

namespace Pengu.Renderer
{
    partial class VulkanContext
    {
        class GameSurface : IRenderableModule
        {
            readonly List<BaseWindow> Windows = new List<BaseWindow>();
            readonly VulkanContext context;
            readonly Vector2 characterSize;

            public GameSurface(VulkanContext context)
            {
                this.context = context;

                var (u0, v0, u1, v1) = context.monospaceFont[' '];
                characterSize = new Vector2(u1 - u0, v1 - v0);
            }

            public void UpdateLogic(TimeSpan elapsedTime) => Windows.ForEach(w => w.UpdateLogic(elapsedTime));

            public void PreRender(uint nextImage) => Windows.ForEach(w => w.PreRender(nextImage));

            public Vector2 ScreenToCharacterSize(Vector2 vec, FontString fs)
            {
                var xFontSize = fs.Size / characterSize.Y * characterSize.X;
                return new Vector2((int)(2 / xFontSize * vec.X), (int)(2 / fs.Size * vec.Y));
            }

            public Vector2 CharacterToScreenSize(int x, int y, FontString fs)
            {
                var xFontSize = fs.Size / characterSize.Y * characterSize.X;
                return new Vector2(/*fs.Offset.X +*/ x * xFontSize, /*fs.Offset.Y +*/ y * fs.Size);
            }

            public Vector2 CharacterToScreenSize(Vector2 v, FontString fs)
            {
                var xFontSize = fs.Size / characterSize.Y * characterSize.X;
                return new Vector2(/*fs.Offset.X +*/ v.X * xFontSize, /*fs.Offset.Y +*/ v.Y * fs.Size);
            }

            public bool ProcessMouseMove(double x, double y)
            {
                foreach (var w in Windows)
                    if (w.ProcessMouseMove(x, y))
                        return true;
                return false;
            }

            public bool ProcessMouseButton(MouseButton button, InputState action, ModifierKeys modifiers)
            {
                foreach (var w in Windows)
                    if (w.ProcessMouseButton(button, action, modifiers))
                        return true;
                return false;
            }

            public bool ProcessKey(Keys key, int scanCode, InputState action, ModifierKeys modifiers)
            {
                foreach (var w in Windows)
                    if (w.ProcessKey(key, scanCode, action, modifiers))
                        return true;
                return false;
            }

            public void AddHexEditorWindow(VM vm) => Windows.Insert(0, new HexEditorWindow(context, this, vm));
        }
    }
}
