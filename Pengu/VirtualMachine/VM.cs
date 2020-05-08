using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Pengu.VirtualMachine
{
    class VM
    {
        public readonly int[] Registers;
        public readonly byte[] Memory;
        public ushort StartInstructionPointer, InstructionPointer;

        public VM(int registers, int memory)
        {
            Registers = new int[registers];
            Memory = new byte[memory];
        }

        public void Reset() => InstructionPointer = BitConverter.ToUInt16(Memory, Memory.Length - 2);

        public ushort RunNextInstruction(int cycles = 1)
        {
            while (cycles-- > 0 && InstructionPointer < Memory.Length - 1)
            {
                ushort nextIp = ushort.MaxValue;
                if (InstructionSet.InstructionDefinitions.TryGetValue((Instruction)Memory[InstructionPointer], out var fn))
                    nextIp = fn(this, (ushort)(InstructionPointer + 1));
                if (nextIp < 0) break; else InstructionPointer = nextIp;
            }

            return 1;
        }
    }
}
