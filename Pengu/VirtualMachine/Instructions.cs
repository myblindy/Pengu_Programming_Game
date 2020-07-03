
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace Pengu.VirtualMachine
{
	internal enum Instruction
	{
					Nop, 
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
					Int_I8, 
					Int_Reg, 
					Jmp_I8, 
					Jmp_RI8, 
					Jmp_Reg, 
					Jmp_RReg, 
					Cmp_I8_I8, 
					Cmp_Reg_I8, 
					Cmp_Reg_Reg, 
					Jl_I8, 
					Jl_RI8, 
					Jl_Reg, 
					Jl_RReg, 
					Jle_I8, 
					Jle_RI8, 
					Jle_Reg, 
					Jle_RReg, 
					Jg_I8, 
					Jg_RI8, 
					Jg_Reg, 
					Jg_RReg, 
					Jge_I8, 
					Jge_RI8, 
					Jge_Reg, 
					Jge_RReg, 
					Je_I8, 
					Je_RI8, 
					Je_Reg, 
					Je_RReg, 
					Jne_I8, 
					Jne_RI8, 
					Jne_Reg, 
					Jne_RReg, 
					Push_I8, 
					Push_Reg, 
					Pop_Reg, 
					Call_I8, 
					Call_RI8, 
					Call_Reg, 
					Call_RReg, 
					Ret, 
					Shl_Reg_Reg, 
					Shl_Reg_I8, 
					Shr_Reg_Reg, 
					Shr_Reg_I8, 
					Or_Reg_Reg, 
					Or_Reg_I8, 
					And_Reg_Reg, 
					And_Reg_I8, 
					Xor_Reg_Reg, 
					Xor_Reg_I8, 
					AddI_Reg_Reg, 
					AddI_Reg_I8, 
					SubI_Reg_Reg, 
					SubI_Reg_I8, 
					MulI_Reg_Reg, 
					MulI_Reg_I8, 
					DivI_Reg_Reg, 
					DivI_Reg_I8, 
					ModI_Reg_Reg, 
					ModI_Reg_I8, 
					Not_Reg, 
			}

	[System.CodeDom.Compiler.GeneratedCode("Instructions.tt", null)]
	internal static class InstructionSet
	{
		public static readonly Func<VM, ushort, ushort>[] InstructionDefinitions =
			new Func<VM, ushort, ushort>[]
			{
									(vm, m) =>
					{
						// Nop
						return m;
					},
									(vm, m) =>
					{
						// Mov_Reg_I8
						vm.Registers[vm.Memory[m]] = vm.Memory[m + 1]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						// Mov_Reg_Reg
						
			I8ToI4I4(vm.Memory[m], out var r0, out var r1);
			vm.Registers[r0] = vm.Registers[r1];
			return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						// Mov_Reg_PI8
						vm.Registers[vm.Memory[m]] = vm.Memory[vm.Memory[m + 1]]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						// Mov_Reg_PReg
						
		    I8ToI4I4(vm.Memory[m], out var r0, out var r1);
            vm.Registers[r0] = vm.Memory[vm.Registers[r1]];
            return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						// Mov_PI8_I8
						vm.Memory[vm.Memory[m]] = vm.Memory[m + 1]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						// Mov_PI8_Reg
						vm.Memory[vm.Memory[m]] = (byte)vm.Registers[vm.Memory[m + 1]]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						// Mov_PI8_PI8
						vm.Memory[vm.Memory[m]] = vm.Memory[vm.Memory[m + 1]]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						// Mov_PI8_PReg
						vm.Memory[vm.Memory[m]] = vm.Memory[vm.Registers[vm.Memory[m + 1]]]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						// Mov_PReg_I8
						vm.Memory[vm.Registers[vm.Memory[m]]] = vm.Memory[m + 1]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						// Mov_PReg_Reg
						
            I8ToI4I4(vm.Memory[m], out var r0, out var r1);
            vm.Memory[vm.Registers[r0]] = (byte)vm.Registers[r1];
            return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						// Mov_PReg_PI8
						vm.Memory[vm.Registers[vm.Memory[m]]] = vm.Memory[vm.Memory[m + 1]]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						// Mov_PReg_PReg
						
			I8ToI4I4(vm.Memory[m], out var r0, out var r1);
			vm.Memory[vm.Registers[r0]] = vm.Memory[vm.Registers[r1]];
			return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						// Int_I8
						vm.CallInterrupt(vm.Memory[m]); return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						// Int_Reg
						vm.CallInterrupt(vm.Registers[vm.Memory[m]]); return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						// Jmp_I8
						return vm.Memory[m];
					},
									(vm, m) =>
					{
						// Jmp_RI8
						return (ushort)(m + 1 + vm.Memory[m]);
					},
									(vm, m) =>
					{
						// Jmp_Reg
						return (ushort)(vm.Registers[vm.Memory[m]]);
					},
									(vm, m) =>
					{
						// Jmp_RReg
						return (ushort)(m + 1 + vm.Registers[vm.Memory[m]]);
					},
									(vm, m) =>
					{
						// Cmp_I8_I8
						vm.FlagCompare = (sbyte)vm.Memory[m].CompareTo(vm.Memory[m + 1]); return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						// Cmp_Reg_I8
						vm.FlagCompare = (sbyte)vm.Registers[vm.Memory[m]].CompareTo(vm.Memory[m + 1]); return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						// Cmp_Reg_Reg
						vm.FlagCompare = (sbyte)vm.Registers[vm.Memory[m]].CompareTo(vm.Registers[vm.Memory[m + 1]]); return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						// Jl_I8
						return (ushort)(vm.FlagCompare < 0 ? vm.Memory[m] : m + 1);
					},
									(vm, m) =>
					{
						// Jl_RI8
						return (ushort)(vm.FlagCompare < 0 ? m + 1 + vm.Memory[m] : m + 1);
					},
									(vm, m) =>
					{
						// Jl_Reg
						return (ushort)(vm.FlagCompare < 0 ? vm.Registers[vm.Memory[m]] : m + 1);
					},
									(vm, m) =>
					{
						// Jl_RReg
						return (ushort)(vm.FlagCompare < 0 ? m + 1 + vm.Registers[vm.Memory[m]] : m + 1);
					},
									(vm, m) =>
					{
						// Jle_I8
						return (ushort)(vm.FlagCompare <= 0 ? vm.Memory[m] : m + 1);
					},
									(vm, m) =>
					{
						// Jle_RI8
						return (ushort)(vm.FlagCompare <= 0 ? m + 1 + vm.Memory[m] : m + 1);
					},
									(vm, m) =>
					{
						// Jle_Reg
						return (ushort)(vm.FlagCompare <= 0 ? vm.Registers[vm.Memory[m]] : m + 1);
					},
									(vm, m) =>
					{
						// Jle_RReg
						return (ushort)(vm.FlagCompare <= 0 ? m + 1 + vm.Registers[vm.Memory[m]] : m + 1);
					},
									(vm, m) =>
					{
						// Jg_I8
						return (ushort)(vm.FlagCompare > 0 ? vm.Memory[m] : m + 1);
					},
									(vm, m) =>
					{
						// Jg_RI8
						return (ushort)(vm.FlagCompare > 0 ? m + 1 + vm.Memory[m] : m + 1);
					},
									(vm, m) =>
					{
						// Jg_Reg
						return (ushort)(vm.FlagCompare > 0 ? vm.Registers[vm.Memory[m]] : m + 1);
					},
									(vm, m) =>
					{
						// Jg_RReg
						return (ushort)(vm.FlagCompare > 0 ? m + 1 + vm.Registers[vm.Memory[m]] : m + 1);
					},
									(vm, m) =>
					{
						// Jge_I8
						return (ushort)(vm.FlagCompare >= 0 ? vm.Memory[m] : m + 1);
					},
									(vm, m) =>
					{
						// Jge_RI8
						return (ushort)(vm.FlagCompare >= 0 ? m + 1 + vm.Memory[m] : m + 1);
					},
									(vm, m) =>
					{
						// Jge_Reg
						return (ushort)(vm.FlagCompare >= 0 ? vm.Registers[vm.Memory[m]] : m + 1);
					},
									(vm, m) =>
					{
						// Jge_RReg
						return (ushort)(vm.FlagCompare >= 0 ? m + 1 + vm.Registers[vm.Memory[m]] : m + 1);
					},
									(vm, m) =>
					{
						// Je_I8
						return (ushort)(vm.FlagCompare == 0 ? vm.Memory[m] : m + 1);
					},
									(vm, m) =>
					{
						// Je_RI8
						return (ushort)(vm.FlagCompare == 0 ? m + 1 + vm.Memory[m] : m + 1);
					},
									(vm, m) =>
					{
						// Je_Reg
						return (ushort)(vm.FlagCompare == 0 ? vm.Registers[vm.Memory[m]] : m + 1);
					},
									(vm, m) =>
					{
						// Je_RReg
						return (ushort)(vm.FlagCompare == 0 ? m + 1 + vm.Registers[vm.Memory[m]] : m + 1);
					},
									(vm, m) =>
					{
						// Jne_I8
						return (ushort)(vm.FlagCompare != 0 ? vm.Memory[m] : m + 1);
					},
									(vm, m) =>
					{
						// Jne_RI8
						return (ushort)(vm.FlagCompare != 0 ? m + 1 + vm.Memory[m] : m + 1);
					},
									(vm, m) =>
					{
						// Jne_Reg
						return (ushort)(vm.FlagCompare != 0 ? vm.Registers[vm.Memory[m]] : m + 1);
					},
									(vm, m) =>
					{
						// Jne_RReg
						return (ushort)(vm.FlagCompare != 0 ? m + 1 + vm.Registers[vm.Memory[m]] : m + 1);
					},
									(vm, m) =>
					{
						// Push_I8
						
					switch(vm.Type)
					{
						case VMType.BitLength8:
							vm.Memory[vm.StackRegister--] = vm.Memory[m];
							break;
						default: throw new InvalidOperationException($"Unexpected VM type enountered: {vm.Type}.");
					}
					return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						// Push_Reg
						
				switch(vm.Type)
				{
					case VMType.BitLength8:
						vm.Memory[vm.StackRegister--] = (byte)vm.Registers[vm.Memory[m]];
						break;
					default: throw new InvalidOperationException($"Unexpected VM type enountered: {vm.Type}.");
				}
				return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						// Pop_Reg
						
				switch(vm.Type)
				{
					case VMType.BitLength8:
						vm.Registers[vm.Memory[m]] = vm.Memory[++vm.StackRegister];
						break;
					default: throw new InvalidOperationException($"Unexpected VM type enountered: {vm.Type}.");
				}
				return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						// Call_I8
						vm.Memory[vm.StackRegister--] = (byte)(m + 1); return vm.Memory[m];
					},
									(vm, m) =>
					{
						// Call_RI8
						vm.Memory[vm.StackRegister--] = (byte)(m + 1); return (ushort)(m + 1 + vm.Memory[m]);
					},
									(vm, m) =>
					{
						// Call_Reg
						vm.Memory[vm.StackRegister--] = (byte)(m + 1); return (ushort)(vm.Registers[vm.Memory[m]]);
					},
									(vm, m) =>
					{
						// Call_RReg
						vm.Memory[vm.StackRegister--] = (byte)(m + 1); return (ushort)(m + 1 + vm.Registers[vm.Memory[m]]);
					},
									(vm, m) =>
					{
						// Ret
						return vm.Memory[++vm.StackRegister];
					},
									(vm, m) =>
					{
						// Shl_Reg_Reg
						I8ToI4I4(vm.Memory[m], out var r0, out var r1); vm.Registers[r0] <<= vm.Registers[r1]; return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						// Shl_Reg_I8
						vm.Registers[vm.Memory[m]] <<= vm.Memory[m + 1]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						// Shr_Reg_Reg
						I8ToI4I4(vm.Memory[m], out var r0, out var r1); vm.Registers[r0] >>= vm.Registers[r1]; return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						// Shr_Reg_I8
						vm.Registers[vm.Memory[m]] >>= vm.Memory[m + 1]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						// Or_Reg_Reg
						I8ToI4I4(vm.Memory[m], out var r0, out var r1); vm.Registers[r0] |= vm.Registers[r1]; return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						// Or_Reg_I8
						vm.Registers[vm.Memory[m]] |= vm.Memory[m + 1]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						// And_Reg_Reg
						I8ToI4I4(vm.Memory[m], out var r0, out var r1); vm.Registers[r0] &= vm.Registers[r1]; return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						// And_Reg_I8
						vm.Registers[vm.Memory[m]] &= vm.Memory[m + 1]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						// Xor_Reg_Reg
						I8ToI4I4(vm.Memory[m], out var r0, out var r1); vm.Registers[r0] ^= vm.Registers[r1]; return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						// Xor_Reg_I8
						vm.Registers[vm.Memory[m]] ^= vm.Memory[m + 1]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						// AddI_Reg_Reg
						I8ToI4I4(vm.Memory[m], out var r0, out var r1); vm.Registers[r0] += vm.Registers[r1]; return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						// AddI_Reg_I8
						vm.Registers[vm.Memory[m]] += vm.Memory[m + 1]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						// SubI_Reg_Reg
						I8ToI4I4(vm.Memory[m], out var r0, out var r1); vm.Registers[r0] -= vm.Registers[r1]; return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						// SubI_Reg_I8
						vm.Registers[vm.Memory[m]] -= vm.Memory[m + 1]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						// MulI_Reg_Reg
						I8ToI4I4(vm.Memory[m], out var r0, out var r1); vm.Registers[r0] *= vm.Registers[r1]; return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						// MulI_Reg_I8
						vm.Registers[vm.Memory[m]] *= vm.Memory[m + 1]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						// DivI_Reg_Reg
						I8ToI4I4(vm.Memory[m], out var r0, out var r1); vm.Registers[r0] /= vm.Registers[r1]; return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						// DivI_Reg_I8
						vm.Registers[vm.Memory[m]] /= vm.Memory[m + 1]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						// ModI_Reg_Reg
						I8ToI4I4(vm.Memory[m], out var r0, out var r1); vm.Registers[r0] %= vm.Registers[r1]; return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						// ModI_Reg_I8
						vm.Registers[vm.Memory[m]] %= vm.Memory[m + 1]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						// Not_Reg
						vm.Registers[vm.Memory[m]] = ~vm.Registers[vm.Memory[m]] & 
			(vm.Type switch { VMType.BitLength8 => 0xFF, VMType.BitLength16 => 0xFFFF, _ => throw new InvalidOperationException() });
		  return (ushort)(m + 1);
					},
							};

		static readonly Func<Memory<byte>, (string result, int size)>[] InstructionDecompilation = 
			new Func<Memory<byte>, (string result, int size)>[]
			{
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 0 ? ($"NOP ", 1) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"MOV r{s[0] & 0xF} 0x{s[1]:X2} ", 3) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"MOV r{s[0] & 0xF} r{(s[0] & 0xF0) >> 4} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"MOV r{s[0] & 0xF} [0x{s[1]:X2}] ", 3) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"MOV r{s[0] & 0xF} [r{(s[0] & 0xF0) >> 4}] ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"MOV [0x{s[0]:X2}] 0x{s[1]:X2} ", 3) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"MOV [0x{s[0]:X2}] r{s[1] & 0xF} ", 3) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"MOV [0x{s[0]:X2}] [0x{s[1]:X2}] ", 3) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"MOV [0x{s[0]:X2}] [r{s[1] & 0xF}] ", 3) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"MOV [r{s[0] & 0xF}] 0x{s[1]:X2} ", 3) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"MOV [r{s[0] & 0xF}] r{(s[0] & 0xF0) >> 4} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"MOV [r{s[0] & 0xF}] [0x{s[1]:X2}] ", 3) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"MOV [r{s[0] & 0xF}] [r{(s[0] & 0xF0) >> 4}] ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"INT 0x{s[0]:X2} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"INT r{s[0] & 0xF} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JMP 0x{s[0]:X2} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JMP $+0x{s[0]:X2} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JMP r{s[0] & 0xF} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JMP $+r{s[0] & 0xF} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"CMP 0x{s[0]:X2} 0x{s[1]:X2} ", 3) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"CMP r{s[0] & 0xF} 0x{s[1]:X2} ", 3) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"CMP r{s[0] & 0xF} r{(s[0] & 0xF0) >> 4} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JL 0x{s[0]:X2} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JL $+0x{s[0]:X2} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JL r{s[0] & 0xF} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JL $+r{s[0] & 0xF} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JLE 0x{s[0]:X2} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JLE $+0x{s[0]:X2} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JLE r{s[0] & 0xF} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JLE $+r{s[0] & 0xF} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JG 0x{s[0]:X2} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JG $+0x{s[0]:X2} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JG r{s[0] & 0xF} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JG $+r{s[0] & 0xF} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JGE 0x{s[0]:X2} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JGE $+0x{s[0]:X2} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JGE r{s[0] & 0xF} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JGE $+r{s[0] & 0xF} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JE 0x{s[0]:X2} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JE $+0x{s[0]:X2} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JE r{s[0] & 0xF} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JE $+r{s[0] & 0xF} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JNE 0x{s[0]:X2} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JNE $+0x{s[0]:X2} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JNE r{s[0] & 0xF} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"JNE $+r{s[0] & 0xF} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"PUSH 0x{s[0]:X2} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"PUSH r{s[0] & 0xF} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"POP r{s[0] & 0xF} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"CALL 0x{s[0]:X2} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"CALL $+0x{s[0]:X2} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"CALL r{s[0] & 0xF} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"CALL $+r{s[0] & 0xF} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 0 ? ($"RET ", 1) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"SHL r{s[0] & 0xF} r{(s[0] & 0xF0) >> 4} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"SHL r{s[0] & 0xF} 0x{s[1]:X2} ", 3) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"SHR r{s[0] & 0xF} r{(s[0] & 0xF0) >> 4} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"SHR r{s[0] & 0xF} 0x{s[1]:X2} ", 3) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"OR r{s[0] & 0xF} r{(s[0] & 0xF0) >> 4} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"OR r{s[0] & 0xF} 0x{s[1]:X2} ", 3) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"AND r{s[0] & 0xF} r{(s[0] & 0xF0) >> 4} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"AND r{s[0] & 0xF} 0x{s[1]:X2} ", 3) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"XOR r{s[0] & 0xF} r{(s[0] & 0xF0) >> 4} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"XOR r{s[0] & 0xF} 0x{s[1]:X2} ", 3) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"ADDI r{s[0] & 0xF} r{(s[0] & 0xF0) >> 4} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"ADDI r{s[0] & 0xF} 0x{s[1]:X2} ", 3) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"SUBI r{s[0] & 0xF} r{(s[0] & 0xF0) >> 4} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"SUBI r{s[0] & 0xF} 0x{s[1]:X2} ", 3) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"MULI r{s[0] & 0xF} r{(s[0] & 0xF0) >> 4} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"MULI r{s[0] & 0xF} 0x{s[1]:X2} ", 3) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"DIVI r{s[0] & 0xF} r{(s[0] & 0xF0) >> 4} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"DIVI r{s[0] & 0xF} 0x{s[1]:X2} ", 3) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"MODI r{s[0] & 0xF} r{(s[0] & 0xF0) >> 4} ", 2) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 2 ? ($"MODI r{s[0] & 0xF} 0x{s[1]:X2} ", 3) : (null, 0);
					},
									m =>
					{
						ReadOnlySpan<byte> s = m.Span;

						
						return s.Length >= 1 ? ($"NOT r{s[0] & 0xF} ", 2) : (null, 0);
					},
							};

		public static string Disassemble(Memory<byte> m, out int size)
		{
			if (m.Length > 0)
			{
				var instr = m.Span[0];
				if (instr < InstructionDecompilation.Length)
				{
					var (result, sz) = InstructionDecompilation[instr](m.Slice(1));
					size = sz;
					return result;
				}
			}

            size = 0;
            return null;
		}

        public static void Assemble(VM vm, string s)
        {
			ushort memidx = 0, org = 0;
			int i0, i1;

			var labels = new Dictionary<string, ushort>();

            var reader = new StringReader(s);
            string line;
			int lineidx = 0;
            var tokens = new List<string>();
            while ((line = reader.ReadLine()) != null)
            {
				++lineidx;

                tokens.Clear();
                for (int idx = 0; idx < line.Length && !char.IsWhiteSpace(line[idx]); ++idx)
                {
					while (idx < line.Length && char.IsWhiteSpace(line[idx])) 
						++idx;

                    var start = idx;
					if(idx < line.Length && line[idx] == '$')
					{
						++idx;																// $
						while (idx < line.Length && char.IsWhiteSpace(line[idx]))			// spaces
							++idx;
						while (idx < line.Length && !char.IsWhiteSpace(line[idx]))			// +
							++idx;					
						while (idx < line.Length && char.IsWhiteSpace(line[idx]))			// spaces
							++idx;
						while (idx < line.Length && !char.IsWhiteSpace(line[idx]))			// value
							++idx;					
					}
					else
						while (idx < line.Length && !char.IsWhiteSpace(line[idx]))
							++idx;
                    tokens.Add(line[start..idx]);
                }

				redo:
				if(!tokens.Any() || tokens[0].StartsWith(";")) continue;

				int GetNumber(string n) => n.StartsWith("0x") ? Convert.ToInt32(n, 16) : n.StartsWith("0b") ? Convert.ToInt32(n[2..], 2) : n[0] == '.' ? labels[n[1..]] : int.Parse(n);
				bool IsReg(string s, out int r) 
				{ 
					var m = Regex.Match(s, @"^r(\d+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled); 
					r = m.Success ? GetNumber(m.Groups[1].Value) : 0; 
					return m.Success; 
				}
				bool IsI8(string s, out int r) 
				{ 
					var m = Regex.Match(s, @"^(\d+|0x[\dA-Fa-f]+|0b[01]+|\.\w+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled); 
					r = m.Success ? GetNumber(m.Groups[1].Value) : 0; 
					return m.Success; 
				}
				bool IsPReg(string s, out int r) 
				{ 
					var m = Regex.Match(s, @"^\[r(\d+)\]$", RegexOptions.IgnoreCase | RegexOptions.Compiled); 
					r = m.Success ? GetNumber(m.Groups[1].Value) : 0; 
					return m.Success; 
				}
				bool IsPI8(string s, out int r) 
				{ 
					var m = Regex.Match(s, @"^\[(\d+|0x[\dA-Fa-f]+|0b[01]+|\.\w+)\]$", RegexOptions.IgnoreCase | RegexOptions.Compiled); 
					r = m.Success ? GetNumber(m.Groups[1].Value) : 0; 
					return m.Success; 
				}
				bool IsRReg(string s, out int r)
				{
					var m = Regex.Match(s, @"^\$\s*\+\s*r(\d+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled); 
					r = m.Success ? GetNumber(m.Groups[1].Value) : 0; 
					return m.Success; 
				}
				bool IsRI8(string s, out int r)
				{
					var m = Regex.Match(s, @"^\$\s*\+\s*(\d+|0x[\dA-Fa-f]+|0b[01]+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled); 
					r = m.Success ? GetNumber(m.Groups[1].Value) : 0; 
					return m.Success; 
				}
				bool IsAtAddress(string s, out int r)
				{ 
					var m = Regex.Match(s, @"^@(\d+|0x[\dA-Fa-f]+|0b[01]+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled); 
					r = m.Success ? GetNumber(m.Groups[1].Value) : 0; 
					return m.Success; 
				}

                if (tokens.Count == 1 && IsAtAddress(tokens[0], out i0))
                {
                    // @addr
                    memidx = (ushort)i0;
                    continue;
                }

                if (tokens.Count >= 1 && tokens[0].FirstOrDefault() == '.')
                {
                    labels.Add(tokens[0][1..], memidx);
                    tokens.RemoveAt(0);
                    goto redo;
                }

				if(tokens.Count == 2 && tokens[0].Equals("DB", StringComparison.OrdinalIgnoreCase) && IsI8(tokens[1], out i0))
				{
					// db i8
					vm.Memory[memidx++] = (byte)i0;
					continue;
				}

				if(tokens.Count == 1 && tokens[0].Equals("ORG", StringComparison.OrdinalIgnoreCase))
				{
					org = memidx;
					continue;
				}

									if(tokens[0].Equals("Nop", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 1
							)
						{
							vm.Memory[memidx++] = (byte)Instruction.Nop;
														continue;
						}
											}
									if(tokens[0].Equals("Mov", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Mov_Reg_I8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Mov_Reg_Reg;
																								vm.Memory[memidx++] = (byte)(((i0 & 0xF) << 4) | (i1 & 0xF));
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsPI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Mov_Reg_PI8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsPReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Mov_Reg_PReg;
																								vm.Memory[memidx++] = (byte)(((i0 & 0xF) << 4) | (i1 & 0xF));
																						continue;
						}
												if(tokens.Count == 3
							 && IsPI8(tokens[1], out i0)  && IsI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Mov_PI8_I8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
												if(tokens.Count == 3
							 && IsPI8(tokens[1], out i0)  && IsReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Mov_PI8_Reg;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
												if(tokens.Count == 3
							 && IsPI8(tokens[1], out i0)  && IsPI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Mov_PI8_PI8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
												if(tokens.Count == 3
							 && IsPI8(tokens[1], out i0)  && IsPReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Mov_PI8_PReg;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
												if(tokens.Count == 3
							 && IsPReg(tokens[1], out i0)  && IsI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Mov_PReg_I8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
												if(tokens.Count == 3
							 && IsPReg(tokens[1], out i0)  && IsReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Mov_PReg_Reg;
																								vm.Memory[memidx++] = (byte)(((i0 & 0xF) << 4) | (i1 & 0xF));
																						continue;
						}
												if(tokens.Count == 3
							 && IsPReg(tokens[1], out i0)  && IsPI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Mov_PReg_PI8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
												if(tokens.Count == 3
							 && IsPReg(tokens[1], out i0)  && IsPReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Mov_PReg_PReg;
																								vm.Memory[memidx++] = (byte)(((i0 & 0xF) << 4) | (i1 & 0xF));
																						continue;
						}
											}
									if(tokens[0].Equals("Int", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 2
							 && IsI8(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Int_I8;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
												if(tokens.Count == 2
							 && IsReg(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Int_Reg;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
											}
									if(tokens[0].Equals("Jmp", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 2
							 && IsI8(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Jmp_I8;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
												if(tokens.Count == 2
							 && IsRI8(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Jmp_RI8;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
												if(tokens.Count == 2
							 && IsReg(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Jmp_Reg;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
												if(tokens.Count == 2
							 && IsRReg(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Jmp_RReg;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
											}
									if(tokens[0].Equals("Cmp", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 3
							 && IsI8(tokens[1], out i0)  && IsI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Cmp_I8_I8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Cmp_Reg_I8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Cmp_Reg_Reg;
																								vm.Memory[memidx++] = (byte)(((i0 & 0xF) << 4) | (i1 & 0xF));
																						continue;
						}
											}
									if(tokens[0].Equals("Jl", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 2
							 && IsI8(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Jl_I8;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
												if(tokens.Count == 2
							 && IsRI8(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Jl_RI8;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
												if(tokens.Count == 2
							 && IsReg(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Jl_Reg;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
												if(tokens.Count == 2
							 && IsRReg(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Jl_RReg;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
											}
									if(tokens[0].Equals("Jle", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 2
							 && IsI8(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Jle_I8;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
												if(tokens.Count == 2
							 && IsRI8(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Jle_RI8;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
												if(tokens.Count == 2
							 && IsReg(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Jle_Reg;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
												if(tokens.Count == 2
							 && IsRReg(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Jle_RReg;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
											}
									if(tokens[0].Equals("Jg", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 2
							 && IsI8(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Jg_I8;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
												if(tokens.Count == 2
							 && IsRI8(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Jg_RI8;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
												if(tokens.Count == 2
							 && IsReg(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Jg_Reg;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
												if(tokens.Count == 2
							 && IsRReg(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Jg_RReg;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
											}
									if(tokens[0].Equals("Jge", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 2
							 && IsI8(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Jge_I8;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
												if(tokens.Count == 2
							 && IsRI8(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Jge_RI8;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
												if(tokens.Count == 2
							 && IsReg(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Jge_Reg;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
												if(tokens.Count == 2
							 && IsRReg(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Jge_RReg;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
											}
									if(tokens[0].Equals("Je", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 2
							 && IsI8(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Je_I8;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
												if(tokens.Count == 2
							 && IsRI8(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Je_RI8;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
												if(tokens.Count == 2
							 && IsReg(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Je_Reg;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
												if(tokens.Count == 2
							 && IsRReg(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Je_RReg;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
											}
									if(tokens[0].Equals("Jne", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 2
							 && IsI8(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Jne_I8;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
												if(tokens.Count == 2
							 && IsRI8(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Jne_RI8;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
												if(tokens.Count == 2
							 && IsReg(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Jne_Reg;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
												if(tokens.Count == 2
							 && IsRReg(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Jne_RReg;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
											}
									if(tokens[0].Equals("Push", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 2
							 && IsI8(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Push_I8;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
												if(tokens.Count == 2
							 && IsReg(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Push_Reg;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
											}
									if(tokens[0].Equals("Pop", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 2
							 && IsReg(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Pop_Reg;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
											}
									if(tokens[0].Equals("Call", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 2
							 && IsI8(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Call_I8;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
												if(tokens.Count == 2
							 && IsRI8(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Call_RI8;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
												if(tokens.Count == 2
							 && IsReg(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Call_Reg;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
												if(tokens.Count == 2
							 && IsRReg(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Call_RReg;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
											}
									if(tokens[0].Equals("Ret", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 1
							)
						{
							vm.Memory[memidx++] = (byte)Instruction.Ret;
														continue;
						}
											}
									if(tokens[0].Equals("Shl", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Shl_Reg_Reg;
																								vm.Memory[memidx++] = (byte)(((i0 & 0xF) << 4) | (i1 & 0xF));
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Shl_Reg_I8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
											}
									if(tokens[0].Equals("Shr", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Shr_Reg_Reg;
																								vm.Memory[memidx++] = (byte)(((i0 & 0xF) << 4) | (i1 & 0xF));
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Shr_Reg_I8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
											}
									if(tokens[0].Equals("Or", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Or_Reg_Reg;
																								vm.Memory[memidx++] = (byte)(((i0 & 0xF) << 4) | (i1 & 0xF));
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Or_Reg_I8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
											}
									if(tokens[0].Equals("And", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.And_Reg_Reg;
																								vm.Memory[memidx++] = (byte)(((i0 & 0xF) << 4) | (i1 & 0xF));
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.And_Reg_I8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
											}
									if(tokens[0].Equals("Xor", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Xor_Reg_Reg;
																								vm.Memory[memidx++] = (byte)(((i0 & 0xF) << 4) | (i1 & 0xF));
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Xor_Reg_I8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
											}
									if(tokens[0].Equals("AddI", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.AddI_Reg_Reg;
																								vm.Memory[memidx++] = (byte)(((i0 & 0xF) << 4) | (i1 & 0xF));
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.AddI_Reg_I8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
											}
									if(tokens[0].Equals("SubI", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.SubI_Reg_Reg;
																								vm.Memory[memidx++] = (byte)(((i0 & 0xF) << 4) | (i1 & 0xF));
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.SubI_Reg_I8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
											}
									if(tokens[0].Equals("MulI", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.MulI_Reg_Reg;
																								vm.Memory[memidx++] = (byte)(((i0 & 0xF) << 4) | (i1 & 0xF));
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.MulI_Reg_I8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
											}
									if(tokens[0].Equals("DivI", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.DivI_Reg_Reg;
																								vm.Memory[memidx++] = (byte)(((i0 & 0xF) << 4) | (i1 & 0xF));
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.DivI_Reg_I8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
											}
									if(tokens[0].Equals("ModI", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.ModI_Reg_Reg;
																								vm.Memory[memidx++] = (byte)(((i0 & 0xF) << 4) | (i1 & 0xF));
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.ModI_Reg_I8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
											}
									if(tokens[0].Equals("Not", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 2
							 && IsReg(tokens[1], out i0) )
						{
							vm.Memory[memidx++] = (byte)Instruction.Not_Reg;
															vm.Memory[memidx++] = (byte)i0;
														continue;
						}
											}
				
				throw new AssemblerException(lineidx - 1, line);
            }

			// write org
			switch(vm.Type)
			{
				case VMType.BitLength8:
					vm.Memory[^1] = (byte)org;
					break;
				case VMType.BitLength16:
					MemoryMarshal.Write(vm.Memory.AsSpan(^2..), ref org);
					break;
				default:
					throw new InvalidOperationException($"Invalid VM type enountered: {vm.Type}.");
			}

			vm.Reset();
        }

		static void I8ToI4I4(int input, out int v1, out int v2)
		{
			v1 = input & 0xF;
			v2 = (input & 0xF0) >> 4;
		}
	}

	[System.CodeDom.Compiler.GeneratedCode("Instructions.tt", null)]
	class AssemblerException : Exception
	{
		public int LineIndex { get; }
		public string Line { get; }

		public AssemblerException(int lineidx, string line) : base($"Assembler exception in line {lineidx}: {line}")
		{
			LineIndex = lineidx;
			Line = line;
		}
	}
}

