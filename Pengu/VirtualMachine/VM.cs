using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace Pengu.VirtualMachine
{
    class VM
    {
        public readonly int[] Registers;
        public readonly byte[] Memory;
        public Memory<byte> StartInstructionPointer, InstructionPointer;

        public VM(int registers, int memory)
        {
            Registers = new int[registers];
            Memory = new byte[memory];
        }

        public int LoadCode(IList<byte> code, int ipOffset = 0)
        {
            code.CopyTo(Memory, Memory.Length - code.Count);
            StartInstructionPointer = InstructionPointer = Memory.AsMemory(^(code.Count + ipOffset)..);
            return code.Count;
        }

        public int RunNextInstruction(int cycles = 1)
        {
            while (cycles-- > 0 && InstructionPointer.Length > 0)
                InstructionPointer = InstructionSet.InstructionDefinitions[(Instruction)InstructionPointer.Span[0]](this, InstructionPointer.Slice(1));

            return 1;
        }
    }
}
