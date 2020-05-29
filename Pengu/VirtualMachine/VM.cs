﻿using System;
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
        public int StackRegister { get; set; }
        public byte[] Memory { get; }
        public ushort StartInstructionPointer { get; set; }
        public ushort InstructionPointer { get; set; }
        public VMType Type { get; }

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
                    StackRegister = Memory.Length - 2;
                    break;
                case VMType.BitLength16:
                    InstructionPointer = BitConverter.ToUInt16(Memory, Memory.Length - 2);
                    StackRegister = Memory.Length - 3;
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected VM type {Type}");
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
    }
}
