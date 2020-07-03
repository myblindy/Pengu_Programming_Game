using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Pengu.VirtualMachine
{
    public enum VMType { BitLength8, BitLength16 };

    public class VM
    {
        public int[] Registers { get; }
        public ushort StackRegister { get; set; }
        public byte[] Memory { get; }
        public ushort StartInstructionPointer { get; set; }
        public ushort InstructionPointer { get; set; }
        public sbyte FlagCompare { get; set; }
        public VMType Type { get; }

        private readonly Dictionary<int, Action<VM>> Interrupts = new Dictionary<int, Action<VM>>();

        public event Action<VM> RefreshRequired;
        public void FireRefreshRequired() => RefreshRequired?.Invoke(this);

        public VM(VMType type, int registers, int memory)
        {
            Type = type;
            Registers = new int[registers];
            Memory = new byte[memory];
        }

        public void Reset()
        {
            switch (Type)
            {
                case VMType.BitLength8:
                    InstructionPointer = Memory[^1];
                    StackRegister = (ushort)(Memory.Length - 2);
                    break;
                case VMType.BitLength16:
                    InstructionPointer = BitConverter.ToUInt16(Memory, Memory.Length - 2);
                    StackRegister = (ushort)(Memory.Length - 3);
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected VM type: {Type}");
            }
        }

        public ushort RunNextInstruction(int cycles = 1)
        {
            while (cycles-- > 0 && InstructionPointer < Memory.Length - 1)
            {
                ushort nextIp = ushort.MaxValue;
                var instruction = Memory[InstructionPointer];
                if (InstructionSet.InstructionDefinitions.Length > instruction)
                    nextIp = InstructionSet.InstructionDefinitions[instruction](this, (ushort)(InstructionPointer + 1));
                if (nextIp < 0) break; else InstructionPointer = nextIp;
            }

            return 1;
        }

        public void RegisterInterrupt(int irq, Action<VM> action)
        {
            if (Interrupts.ContainsKey(irq))
                Interrupts[irq] = action;
            else
                Interrupts.Add(irq, action);
        }

        public void CallInterrupt(int irq)
        {
            if (Interrupts.TryGetValue(irq, out var action))
                action?.Invoke(this);
        }

        public void ClearInterrupt(int irq) => Interrupts.Remove(irq);
    }
}
