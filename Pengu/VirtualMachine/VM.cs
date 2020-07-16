using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Pengu.VirtualMachine
{
    public enum VMType { BitLength8, BitLength16 };

    public class VM : IMemory
    {
        public int[] Registers { get; }
        public ushort StackRegister { get; set; }
        public IList<byte> Memory { get; }
        public string MemoryName { get; }
        public ushort StartInstructionPointer { get; set; }
        public ushort InstructionPointer { get; set; }
        public sbyte FlagCompare { get; set; }
        public VMType Type { get; }

        private readonly Dictionary<int, Action<VM>> Interrupts = new Dictionary<int, Action<VM>>();

        public event Action<IMemory>? RefreshRequired;
        public void FireRefreshRequired() => RefreshRequired?.Invoke(this);

        public VM(VMType type, int registers, string memoryName, int? memorySize = default, byte[]? memoryData = null)
        {
            static byte[] CloneMemoryData(int memorySize, byte[] memoryData)
            {
                var result = new byte[Math.Max(memorySize, memoryData.Length)];
                Array.Copy(memoryData, result, memoryData.Length);
                return result;
            }

            Type = type;
            Registers = new int[registers];
            MemoryName = memoryName;
            Memory = memoryData is null
                ? memorySize.HasValue
                    ? new byte[memorySize.Value]
                    : throw new ArgumentException("Cannot create a memory with no size and no data")
                : memorySize.HasValue
                    ? CloneMemoryData(memorySize.Value, memoryData)
                    : (byte[])memoryData.Clone();
        }

        public void Reset()
        {
            switch (Type)
            {
                case VMType.BitLength8:
                    InstructionPointer = Memory[^1];
                    StackRegister = (ushort)(Memory.Count - 2);
                    break;
                case VMType.BitLength16:
                    InstructionPointer = BitConverter.ToUInt16((byte[])Memory, Memory.Count - 2);
                    StackRegister = (ushort)(Memory.Count - 3);
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected VM type: {Type}");
            }
        }

        public ushort RunNextInstruction(int cycles = 1)
        {
            while (cycles-- > 0 && InstructionPointer < Memory.Count - 1)
            {
                ushort nextIp = ushort.MaxValue;
                var instruction = Memory[InstructionPointer];
                if (InstructionSet.InstructionDefinitions.Length > instruction)
                    nextIp = InstructionSet.InstructionDefinitions[instruction](this, (ushort)(InstructionPointer + 1));
                if (nextIp < 0) break; else InstructionPointer = nextIp;
            }

            return 1;
        }

        public void RegisterInterrupt(int irq, Action<VM> action) => Interrupts[irq] = action;

        public void CallInterrupt(int irq)
        {
            if (Interrupts.TryGetValue(irq, out var action))
                action?.Invoke(this);
        }

        public void ClearInterrupt(int irq) => Interrupts.Remove(irq);
    }
}
