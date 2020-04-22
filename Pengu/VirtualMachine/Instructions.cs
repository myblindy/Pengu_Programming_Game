

using System;
using System.Collections.Generic;

namespace Pengu.VirtualMachine
{
	internal enum Instruction
	{
					Mov_Reg_I8, 
					Mov_Reg_Reg, 
					Mov_Reg_PI8, 
					Mov_Reg_PReg, 
					Mov_PI8_I8, 
					Mov_PI8_Reg, 
					Mov_PI8_PI8, 
					Mov_PI8_PReg, 
					Mov_PReg_I8, 
					Mov_PReg_Reg, 
					Mov_PReg_PI8, 
					Mov_PReg_PReg, 
					AddI_Reg_I8, 
					AddI_Reg_Reg, 
					AddI_Reg_PI8, 
					AddI_Reg_PReg, 
					SubI_Reg_I8, 
					SubI_Reg_Reg, 
					SubI_Reg_PI8, 
					SubI_Reg_PReg, 
					MulI_Reg_I8, 
					MulI_Reg_Reg, 
					MulI_Reg_PI8, 
					MulI_Reg_PReg, 
			}

	internal static class InstructionSet
	{
		public static readonly Dictionary<Instruction, Func<VM, Memory<byte>, Memory<byte>>> InstructionDefinitions =
			new Dictionary<Instruction, Func<VM, Memory<byte>, Memory<byte>>>()
		{
							[Instruction.Mov_Reg_I8] = (vm, m) =>
				{
					vm.Registers[m.Span[0]] = m.Span[1]; return m.Slice(2);
				},
							[Instruction.Mov_Reg_Reg] = (vm, m) =>
				{
					
			I8ToI4I4(m.Span[0], out var r0, out var r1);
			vm.Registers[r0] = vm.Registers[r1];
			return m.Slice(1);
				},
							[Instruction.Mov_Reg_PI8] = (vm, m) =>
				{
					vm.Registers[m.Span[0]] = vm.Memory[m.Span[1]]; return m.Slice(2);
				},
							[Instruction.Mov_Reg_PReg] = (vm, m) =>
				{
					
		    I8ToI4I4(m.Span[0], out var r0, out var r1);
            vm.Registers[r0] = vm.Registers[r1];
            return m.Slice(1);
				},
							[Instruction.Mov_PI8_I8] = (vm, m) =>
				{
					vm.Memory[m.Span[0]] = m.Span[1]; return m.Slice(2);
				},
							[Instruction.Mov_PI8_Reg] = (vm, m) =>
				{
					vm.Memory[m.Span[0]] = (byte)vm.Registers[m.Span[1]]; return m.Slice(2);
				},
							[Instruction.Mov_PI8_PI8] = (vm, m) =>
				{
					vm.Memory[m.Span[0]] = vm.Memory[m.Span[1]]; return m.Slice(2);
				},
							[Instruction.Mov_PI8_PReg] = (vm, m) =>
				{
					vm.Memory[m.Span[0]] = vm.Memory[vm.Registers[m.Span[1]]]; return m.Slice(2);
				},
							[Instruction.Mov_PReg_I8] = (vm, m) =>
				{
					vm.Memory[vm.Registers[m.Span[0]]] = m.Span[1]; return m.Slice(2);
				},
							[Instruction.Mov_PReg_Reg] = (vm, m) =>
				{
					
            I8ToI4I4(m.Span[0], out var r0, out var r1);
            vm.Memory[vm.Registers[r0]] = (byte)vm.Registers[r1];
            return m.Slice(1);
				},
							[Instruction.Mov_PReg_PI8] = (vm, m) =>
				{
					vm.Memory[vm.Registers[m.Span[0]]] = vm.Memory[m.Span[1]]; return m.Slice(2);
				},
							[Instruction.Mov_PReg_PReg] = (vm, m) =>
				{
					
			I8ToI4I4(m.Span[0], out var r0, out var r1);
			vm.Memory[vm.Registers[r0]] = vm.Memory[vm.Registers[r1]];
			return m.Slice(1);
				},
							[Instruction.AddI_Reg_I8] = (vm, m) =>
				{
					vm.Registers[m.Span[0]] += m.Span[1]; return m.Slice(2);
				},
							[Instruction.AddI_Reg_Reg] = (vm, m) =>
				{
					
            I8ToI4I4(m.Span[0], out var r0, out var r1);
            vm.Registers[r0] += vm.Registers[r1];
            return m.Slice(1);
				},
							[Instruction.AddI_Reg_PI8] = (vm, m) =>
				{
					vm.Registers[m.Span[0]] += vm.Memory[m.Span[1]]; return m.Slice(2);
				},
							[Instruction.AddI_Reg_PReg] = (vm, m) =>
				{
					
            I8ToI4I4(m.Span[0], out var r0, out var r1);
            vm.Registers[r0] += vm.Memory[vm.Registers[r1]];
            return m.Slice(1);
				},
							[Instruction.SubI_Reg_I8] = (vm, m) =>
				{
					vm.Registers[m.Span[0]] -= m.Span[1]; return m.Slice(2);
				},
							[Instruction.SubI_Reg_Reg] = (vm, m) =>
				{
					
            I8ToI4I4(m.Span[0], out var r0, out var r1);
            vm.Registers[r0] -= vm.Registers[r1];
            return m.Slice(1);
				},
							[Instruction.SubI_Reg_PI8] = (vm, m) =>
				{
					vm.Registers[m.Span[0]] -= vm.Memory[m.Span[1]]; return m.Slice(2);
				},
							[Instruction.SubI_Reg_PReg] = (vm, m) =>
				{
					
            I8ToI4I4(m.Span[0], out var r0, out var r1);
            vm.Registers[r0] -= vm.Memory[vm.Registers[r1]];
            return m.Slice(1);
				},
							[Instruction.MulI_Reg_I8] = (vm, m) =>
				{
					vm.Registers[m.Span[0]] *= m.Span[1]; return m.Slice(2);
				},
							[Instruction.MulI_Reg_Reg] = (vm, m) =>
				{
					
            I8ToI4I4(m.Span[0], out var r0, out var r1);
            vm.Registers[r0] *= vm.Registers[r1];
            return m.Slice(1);
				},
							[Instruction.MulI_Reg_PI8] = (vm, m) =>
				{
					vm.Registers[m.Span[0]] *= vm.Memory[m.Span[1]]; return m.Slice(2);
				},
							[Instruction.MulI_Reg_PReg] = (vm, m) =>
				{
					
            I8ToI4I4(m.Span[0], out var r0, out var r1);
            vm.Registers[r0] *= vm.Memory[vm.Registers[r1]];
            return m.Slice(1);
				},
					};

		static readonly Dictionary<Instruction, Func<Memory<byte>, (string result, int size)>> InstructionDecompilation = 
			new Dictionary<Instruction, Func<Memory<byte>, (string result, int size)>>()
		{
							[Instruction.Mov_Reg_I8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return ($"MOV r{s[0] & 0xF} {s[1]} ", 3);
				},
							[Instruction.Mov_Reg_Reg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return ($"MOV r{s[0] & 0xF} r{(s[0] & 0xF0) >> 4} ", 2);
				},
							[Instruction.Mov_Reg_PI8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return ($"MOV r{s[0] & 0xF} [{s[1]}] ", 3);
				},
							[Instruction.Mov_Reg_PReg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return ($"MOV r{s[0] & 0xF} [r{(s[0] & 0xF0) >> 4}] ", 2);
				},
							[Instruction.Mov_PI8_I8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return ($"MOV [{s[0]}] {s[1]} ", 3);
				},
							[Instruction.Mov_PI8_Reg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return ($"MOV [{s[0]}] r{s[1] & 0xF} ", 3);
				},
							[Instruction.Mov_PI8_PI8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return ($"MOV [{s[0]}] [{s[1]}] ", 3);
				},
							[Instruction.Mov_PI8_PReg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return ($"MOV [{s[0]}] [r{s[1] & 0xF}] ", 3);
				},
							[Instruction.Mov_PReg_I8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return ($"MOV [r{s[0] & 0xF}] {s[1]} ", 3);
				},
							[Instruction.Mov_PReg_Reg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return ($"MOV [r{s[0] & 0xF}] r{(s[0] & 0xF0) >> 4} ", 2);
				},
							[Instruction.Mov_PReg_PI8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return ($"MOV [r{s[0] & 0xF}] [{s[1]}] ", 3);
				},
							[Instruction.Mov_PReg_PReg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return ($"MOV [r{s[0] & 0xF}] [r{(s[0] & 0xF0) >> 4}] ", 2);
				},
							[Instruction.AddI_Reg_I8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return ($"ADDI r{s[0] & 0xF} {s[1]} ", 3);
				},
							[Instruction.AddI_Reg_Reg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return ($"ADDI r{s[0] & 0xF} r{(s[0] & 0xF0) >> 4} ", 2);
				},
							[Instruction.AddI_Reg_PI8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return ($"ADDI r{s[0] & 0xF} [{s[1]}] ", 3);
				},
							[Instruction.AddI_Reg_PReg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return ($"ADDI r{s[0] & 0xF} [r{(s[0] & 0xF0) >> 4}] ", 2);
				},
							[Instruction.SubI_Reg_I8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return ($"SUBI r{s[0] & 0xF} {s[1]} ", 3);
				},
							[Instruction.SubI_Reg_Reg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return ($"SUBI r{s[0] & 0xF} r{(s[0] & 0xF0) >> 4} ", 2);
				},
							[Instruction.SubI_Reg_PI8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return ($"SUBI r{s[0] & 0xF} [{s[1]}] ", 3);
				},
							[Instruction.SubI_Reg_PReg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return ($"SUBI r{s[0] & 0xF} [r{(s[0] & 0xF0) >> 4}] ", 2);
				},
							[Instruction.MulI_Reg_I8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return ($"MULI r{s[0] & 0xF} {s[1]} ", 3);
				},
							[Instruction.MulI_Reg_Reg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return ($"MULI r{s[0] & 0xF} r{(s[0] & 0xF0) >> 4} ", 2);
				},
							[Instruction.MulI_Reg_PI8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return ($"MULI r{s[0] & 0xF} [{s[1]}] ", 3);
				},
							[Instruction.MulI_Reg_PReg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return ($"MULI r{s[0] & 0xF} [r{(s[0] & 0xF0) >> 4}] ", 2);
				},
					};

		public static string Disassemble(Memory<byte> m, out int size)
		{
			var (result, sz) = InstructionDecompilation[(Instruction)m.Span[0]](m.Slice(1));
			size = sz;
			return result;
		}

	    static void I8ToI4I4(int input, out int v1, out int v2)
        {
            v1 = input & 0xF;
            v2 = (input & 0xF0) >> 4;
        }
	}
}