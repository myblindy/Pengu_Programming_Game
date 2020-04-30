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
        public int StartInstructionPointer, InstructionPointer;

        public VM(int registers, int memory)
        {
            Registers = new int[registers];
            Memory = new byte[memory];
        }

        public int LoadCode(IList<byte> code, int ipOffset = 0)
        {
            code.CopyTo(Memory, Memory.Length - code.Count);
            StartInstructionPointer = InstructionPointer = code.Count + ipOffset;
            return code.Count;
        }

        public int RunNextInstruction(int cycles = 1)
        {
            while (cycles-- > 0 && InstructionPointer < Memory.Length - 1)
            {
                var nextip = InstructionSet.InstructionDefinitions[(Instruction)Memory[InstructionPointer]](this, InstructionPointer + 1);
                if (nextip < 0) break; else InstructionPointer = nextip;
            }

            return 1;
        }
    }
}
