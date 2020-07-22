using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Linq;
using GLFW;
using Pengu.VirtualMachine;
using SharpVk;
using Pengu.Renderer.UI;
using Pengu.Support;

namespace Pengu.Renderer
{
    partial class VulkanContext
    {
        internal class GameSurface : IRenderableModule
        {
            readonly List<BaseWindow> Windows = new List<BaseWindow>();

            BaseWindow? focusedWindow;
            internal BaseWindow? FocusedWindow
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

            private void MoveWindowToTop(BaseWindow? window)
            {
                if (!(window is null) && Windows[0] != window)
                {
                    context.monospaceFont.MoveStringToTop(window.ChromeFontString);
                    context.monospaceFont.MoveStringToTop(window.ContentFontString);
                    Windows.Remove(window);
                    Windows.Insert(0, window);
                }
            }

            readonly VulkanContext context;
            readonly Vector2 characterSize;
            internal List<Solution>? Solutions;
            internal readonly List<IMemory> memories = new List<IMemory>();

            internal IMemory FindMemory(string name) => memories!.First(mem => mem.MemoryName == name);

            public GameSurface(VulkanContext context)
            {
                this.context = context;

                var (u0, v0, u1, v1) = context.monospaceFont[' '];
                characterSize = new Vector2(u1 - u0, v1 - v0);

                LoadExercise("Test Exercise");
            }

            private void LoadExercise(string testName)
            {
                var exercise = Exercises.Get(testName);
                Solutions = exercise.Solutions;

                memories.Clear();
                var labels = new Dictionary<string, string>();

                object FindAny(string name) =>
                    labels!.TryGetValue(name, out var stringValue) ? (object)stringValue : FindMemory(name);

                if (!(exercise.Labels is null))
                    foreach (var label in exercise.Labels)
                        labels.Add(label.Name!, label.Text!);
                if (!(exercise.Memories is null))
                    memories.AddRange(exercise.Memories.Select(mem => (IMemory)new MemoryComponent(mem.Name!, mem.Size, mem.Data)));
                if (!(exercise.SevenSegmentDigitDisplays is null))
                    memories.AddRange(exercise.SevenSegmentDigitDisplays.Select(sdd => new SevenSegmentDigitDisplayComponent(sdd.Name!)));
                memories.AddRange(exercise.CPUs!
                    .Select(cpu =>
                    {
                        var vm = new VM(VMType.BitLength8, cpu.RegisterCount, cpu.Memory!.Name!, cpu.Memory!.Size, cpu.Memory!.Data);
                        if (!(cpu.Interrupts is null))
                            foreach (var interrupt in cpu.Interrupts)
                            {
                                var mem = FindMemory(interrupt.MemoryName!);

                                Action<VM> action = interrupt.Type switch
                                {
                                    InterruptType.ReadMemory => vm =>
                                        vm.Registers[interrupt.OutputRegisterNumber!.Value] = mem.Memory[vm.Registers[interrupt.InputRegisterNumber!.Value]],
                                    InterruptType.WriteMemoryLiteral => vm =>
                                        mem.Memory[interrupt.OutputLiteral!.Value] = (byte)vm.Registers[interrupt.InputRegisterNumber!.Value],
                                    _ => throw new NotImplementedException(),
                                };
                                vm.RegisterInterrupt(interrupt.Irq, action);
                            }
                        return vm;
                    }));

                foreach (var window in exercise.Windows!)
                    switch (window.Type)
                    {
                        case WindowType.HexEditor:
                            AddHexEditorWindow(FindMemory(window.MemoryName!), window.PositionX, window.PositionY, window.MemoryName, window.LinesCount);
                            break;
                        case WindowType.Assembler:
                            AddNewWindow(new AssemblerWindow(context, this, string.IsNullOrWhiteSpace(window.LoadFile) ? null : exercise.ReadAllAssociatedFile(window.LoadFile),
                                (VM)FindMemory(window.MemoryName!)));
                            break;
                        case WindowType.Playground:
                            AddNewWindow(new PlaygroundWindow(context, this, window.PositionX!.Value, window.PositionY!.Value, window.Width!.Value, window.Height!.Value,
                                window.DisplayComponents?.Select(c => (FindAny(c.Name!), c.PositionX, c.PositionY))));
                            break;
                        default:
                            throw new NotImplementedException();
                    }
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

            internal void AddHexEditorWindow(IMemory mem, int? positionX = null, int? positionY = null, string? title = null, int? linesCount = null)
            {
                if (mem is VM vm)
                    AddNewWindow(new HexEditorWindow<VM>(context, this, vm, positionX, positionY, title, linesCount));
                else
                    AddNewWindow(new HexEditorWindow<MemoryComponent>(context, this, (MemoryComponent)mem, positionX, positionY, title, linesCount));
            }
        }
    }
}
