using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Linq;
using GLFW;
using Pengu.VirtualMachine;
using SharpVk;
using Pengu.Renderer.UI;

namespace Pengu.Renderer
{
    partial class VulkanContext
    {
        internal class GameSurface : IRenderableModule
        {
            readonly List<BaseWindow> Windows = new List<BaseWindow>();

            BaseWindow focusedWindow;
            internal BaseWindow FocusedWindow
            {
                get => focusedWindow;
                set => MoveWindowToTop(focusedWindow = value);
            }

            private void MoveWindowToTop(BaseWindow window)
            {
                if (Windows[0] != window)
                {
                    context.monospaceFont.MoveStringToTop(window.ChromeFontString);
                    Windows.Remove(window);
                    Windows.Insert(0, window);
                }
            }

            readonly VulkanContext context;
            readonly Vector2 characterSize;

            public GameSurface(VulkanContext context)
            {
                this.context = context;

                var (u0, v0, u1, v1) = context.monospaceFont[' '];
                characterSize = new Vector2(u1 - u0, v1 - v0);
            }

            public void UpdateLogic(TimeSpan elapsedTime) => Windows.ForEach(w => w.UpdateLogic(elapsedTime));

            public CommandBuffer[] PreRender(uint nextImage)
            {
                Windows.ForEach(w => w.PreRender(nextImage));
                return Array.Empty<CommandBuffer>();
            }

            public Vector2 ScreenToCharacterSize(Vector2 vec, FontString fs)
            {
                var xFontSize = fs.Size / characterSize.Y * characterSize.X;
                return new Vector2((int)(4 / xFontSize * vec.X), (int)(4 / fs.Size * vec.Y));
            }

            public Vector2 CharacterToScreenSize(int x, int y, FontString fs)
            {
                var xFontSize = fs.Size / characterSize.Y * characterSize.X;
                return new Vector2(x * xFontSize / 2, y * fs.Size / 2);
            }

            public Vector2 CharacterToScreenSize(Vector2 v, FontString fs)
            {
                var xFontSize = fs.Size / characterSize.Y * characterSize.X;
                return new Vector2(v.X * xFontSize / 2, v.Y * fs.Size / 2);
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

            private void AddNewWindow(BaseWindow window)
            {
                Windows.Insert(0, window);
                if (Windows.Count == 1)
                    FocusedWindow = window;
            }

            public void AddHexEditorWindow(VM vm) => AddNewWindow(new HexEditorWindow(context, this, vm));
            internal void AddPlaygroundWindow(VM vm) => AddNewWindow(new PlaygroundWindow(context, this, vm));
        }
    }
}
