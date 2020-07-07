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
                set
                {
                    if (focusedWindow == value) return;

                    var prevFocus = focusedWindow;
                    MoveWindowToTop(focusedWindow = value);

                    if (!(prevFocus is null)) prevFocus.ChromeFontStringDirty = true;
                    if (!(focusedWindow is null)) focusedWindow.ChromeFontStringDirty = true;
                }
            }

            private void MoveWindowToTop(BaseWindow window)
            {
                if (Windows[0] != window)
                {
                    context.monospaceFont.MoveStringToTop(window.ChromeFontString);
                    context.monospaceFont.MoveStringToTop(window.ContentFontString);
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

            static float Map(float val, float srcMin, float srcMax, float dstMin, float dstMax) =>
                (val - srcMin) / (srcMax - srcMin) * (dstMax - dstMin) + dstMin;

            public Vector2 ScreenToCharacterSize(Vector2 vec, FontString fs)
            {
                var xFontSize = fs.Size / characterSize.Y * characterSize.X;
                var v = new Vector2(
                    MathF.Floor(Map(vec.X, 0, 2, 0, MathF.Floor(4 / xFontSize))),
                    MathF.Floor(Map(vec.Y, 0, 2, 0, MathF.Floor(2 / fs.Size))));
                return v;
            }

            public Vector2 CharacterToScreenSize(int x, int y, FontString fs)
            {
                var xFontSize = fs.Size / characterSize.Y * characterSize.X;
                //return new Vector2(x * xFontSize / 1, y * fs.Size / 1);
                return new Vector2(
                    Map(x, 0, MathF.Floor(4 / xFontSize), 0, 2),
                    Map(y, 0, MathF.Floor(2 / fs.Size), 0, 2));
            }

            public Vector2 CharacterToScreenSize(Vector2 v, FontString fs) =>
                CharacterToScreenSize((int)v.X, (int)v.Y, fs);

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

            public bool ProcessKey(Keys key, int scanCode, InputState action, ModifierKeys modifiers) =>
                FocusedWindow?.ProcessKey(key, scanCode, action, modifiers) == true;

            public bool ProcessCharacter(string character, ModifierKeys modifiers) =>
                FocusedWindow?.ProcessCharacter(character, modifiers) == true;

            private void AddNewWindow(BaseWindow window)
            {
                Windows.Insert(0, window);
                FocusedWindow = window;
            }

            internal void AddHexEditorWindow(VM vm) => AddNewWindow(new HexEditorWindow(context, this, vm));
            internal void AddPlaygroundWindow(VM vm) => AddNewWindow(new PlaygroundWindow(context, this, vm));
            internal void AddAssemblerWindow(string asm, VM vm) => AddNewWindow(new AssemblerWindow(context, this, asm, vm));
        }
    }
}
