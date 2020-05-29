﻿

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
					Int_I8, 
					DivI_Reg_I8, 
					DivI_Reg_Reg, 
					DivI_Reg_PI8, 
					DivI_Reg_PReg, 
					ModI_Reg_I8, 
					ModI_Reg_Reg, 
					ModI_Reg_PI8, 
					ModI_Reg_PReg, 
			}

	[System.CodeDom.Compiler.GeneratedCode("Instructions.tt", null)]
	internal static class InstructionSet
	{
		public static readonly Func<VM, ushort, ushort>[] InstructionDefinitions =
			new Func<VM, ushort, ushort>[]
			{
									(vm, m) =>
					{
						return m;
					},
									(vm, m) =>
					{
						vm.Registers[vm.Memory[m]] = vm.Memory[m + 1]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						
			I8ToI4I4(vm.Memory[m], out var r0, out var r1);
			vm.Registers[r0] = vm.Registers[r1];
			return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						vm.Registers[vm.Memory[m]] = vm.Memory[vm.Memory[m + 1]]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						
		    I8ToI4I4(vm.Memory[m], out var r0, out var r1);
            vm.Registers[r0] = vm.Registers[r1];
            return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						vm.Memory[vm.Memory[m]] = vm.Memory[m + 1]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						vm.Memory[vm.Memory[m]] = (byte)vm.Registers[vm.Memory[m + 1]]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						vm.Memory[vm.Memory[m]] = vm.Memory[vm.Memory[m + 1]]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						vm.Memory[vm.Memory[m]] = vm.Memory[vm.Registers[vm.Memory[m + 1]]]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						vm.Memory[vm.Registers[vm.Memory[m]]] = vm.Memory[m + 1]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						
            I8ToI4I4(vm.Memory[m], out var r0, out var r1);
            vm.Memory[vm.Registers[r0]] = (byte)vm.Registers[r1];
            return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						vm.Memory[vm.Registers[vm.Memory[m]]] = vm.Memory[vm.Memory[m + 1]]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						
			I8ToI4I4(vm.Memory[m], out var r0, out var r1);
			vm.Memory[vm.Registers[r0]] = vm.Memory[vm.Registers[r1]];
			return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						vm.Registers[vm.Memory[m]] += vm.Memory[m + 1]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						
            I8ToI4I4(vm.Memory[m], out var r0, out var r1);
            vm.Registers[r0] += vm.Registers[r1];
            return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						vm.Registers[vm.Memory[m]] += vm.Memory[vm.Memory[m + 1]]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						
            I8ToI4I4(vm.Memory[m], out var r0, out var r1);
            vm.Registers[r0] += vm.Memory[vm.Registers[r1]];
            return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						vm.Registers[vm.Memory[m]] -= vm.Memory[m + 1]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						
            I8ToI4I4(vm.Memory[m], out var r0, out var r1);
            vm.Registers[r0] -= vm.Registers[r1];
            return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						vm.Registers[vm.Memory[m]] -= vm.Memory[vm.Memory[m + 1]]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						
            I8ToI4I4(vm.Memory[m], out var r0, out var r1);
            vm.Registers[r0] -= vm.Memory[vm.Registers[r1]];
            return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						vm.Registers[vm.Memory[m]] *= vm.Memory[m + 1]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						
            I8ToI4I4(vm.Memory[m], out var r0, out var r1);
            vm.Registers[r0] *= vm.Registers[r1];
            return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						vm.Registers[vm.Memory[m]] *= vm.Memory[vm.Memory[m + 1]]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						
            I8ToI4I4(vm.Memory[m], out var r0, out var r1);
            vm.Registers[r0] *= vm.Memory[vm.Registers[r1]];
            return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						vm.Registers[vm.Memory[m]] /= vm.Memory[m + 1]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						
            I8ToI4I4(vm.Memory[m], out var r0, out var r1);
            vm.Registers[r0] /= vm.Registers[r1];
            return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						vm.Registers[vm.Memory[m]] /= vm.Memory[vm.Memory[m + 1]]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						
            I8ToI4I4(vm.Memory[m], out var r0, out var r1);
            vm.Registers[r0] /= vm.Memory[vm.Registers[r1]];
            return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						vm.Registers[vm.Memory[m]] %= vm.Memory[m + 1]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						
            I8ToI4I4(vm.Memory[m], out var r0, out var r1);
            vm.Registers[r0] %= vm.Registers[r1];
            return (ushort)(m + 1);
					},
									(vm, m) =>
					{
						vm.Registers[vm.Memory[m]] %= vm.Memory[vm.Memory[m + 1]]; return (ushort)(m + 2);
					},
									(vm, m) =>
					{
						
            I8ToI4I4(vm.Memory[m], out var r0, out var r1);
            vm.Registers[r0] %= vm.Memory[vm.Registers[r1]];
            return (ushort)(m + 1);
					},
							};

		static readonly Dictionary<Instruction, Func<Memory<byte>, (string result, int size)>> InstructionDecompilation = 
			new Dictionary<Instruction, Func<Memory<byte>, (string result, int size)>>()
		{
							[Instruction.Nop] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 0 ? ($"NOP ", 1) : (null, 0);
				},
							[Instruction.Mov_Reg_I8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"MOV r{s[0] & 0xF} 0x{s[1]:X2} ", 3) : (null, 0);
				},
							[Instruction.Mov_Reg_Reg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"MOV r{s[0] & 0xF} r{(s[0] & 0xF0) >> 4} ", 2) : (null, 0);
				},
							[Instruction.Mov_Reg_PI8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"MOV r{s[0] & 0xF} [0x{s[1]:X2}] ", 3) : (null, 0);
				},
							[Instruction.Mov_Reg_PReg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"MOV r{s[0] & 0xF} [r{(s[0] & 0xF0) >> 4}] ", 2) : (null, 0);
				},
							[Instruction.Mov_PI8_I8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"MOV [0x{s[0]:X2}] 0x{s[1]:X2} ", 3) : (null, 0);
				},
							[Instruction.Mov_PI8_Reg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"MOV [0x{s[0]:X2}] r{s[1] & 0xF} ", 3) : (null, 0);
				},
							[Instruction.Mov_PI8_PI8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"MOV [0x{s[0]:X2}] [0x{s[1]:X2}] ", 3) : (null, 0);
				},
							[Instruction.Mov_PI8_PReg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"MOV [0x{s[0]:X2}] [r{s[1] & 0xF}] ", 3) : (null, 0);
				},
							[Instruction.Mov_PReg_I8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"MOV [r{s[0] & 0xF}] 0x{s[1]:X2} ", 3) : (null, 0);
				},
							[Instruction.Mov_PReg_Reg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"MOV [r{s[0] & 0xF}] r{(s[0] & 0xF0) >> 4} ", 2) : (null, 0);
				},
							[Instruction.Mov_PReg_PI8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"MOV [r{s[0] & 0xF}] [0x{s[1]:X2}] ", 3) : (null, 0);
				},
							[Instruction.Mov_PReg_PReg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"MOV [r{s[0] & 0xF}] [r{(s[0] & 0xF0) >> 4}] ", 2) : (null, 0);
				},
							[Instruction.AddI_Reg_I8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"ADDI r{s[0] & 0xF} 0x{s[1]:X2} ", 3) : (null, 0);
				},
							[Instruction.AddI_Reg_Reg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"ADDI r{s[0] & 0xF} r{(s[0] & 0xF0) >> 4} ", 2) : (null, 0);
				},
							[Instruction.AddI_Reg_PI8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"ADDI r{s[0] & 0xF} [0x{s[1]:X2}] ", 3) : (null, 0);
				},
							[Instruction.AddI_Reg_PReg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"ADDI r{s[0] & 0xF} [r{(s[0] & 0xF0) >> 4}] ", 2) : (null, 0);
				},
							[Instruction.SubI_Reg_I8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"SUBI r{s[0] & 0xF} 0x{s[1]:X2} ", 3) : (null, 0);
				},
							[Instruction.SubI_Reg_Reg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"SUBI r{s[0] & 0xF} r{(s[0] & 0xF0) >> 4} ", 2) : (null, 0);
				},
							[Instruction.SubI_Reg_PI8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"SUBI r{s[0] & 0xF} [0x{s[1]:X2}] ", 3) : (null, 0);
				},
							[Instruction.SubI_Reg_PReg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"SUBI r{s[0] & 0xF} [r{(s[0] & 0xF0) >> 4}] ", 2) : (null, 0);
				},
							[Instruction.MulI_Reg_I8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"MULI r{s[0] & 0xF} 0x{s[1]:X2} ", 3) : (null, 0);
				},
							[Instruction.MulI_Reg_Reg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"MULI r{s[0] & 0xF} r{(s[0] & 0xF0) >> 4} ", 2) : (null, 0);
				},
							[Instruction.MulI_Reg_PI8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"MULI r{s[0] & 0xF} [0x{s[1]:X2}] ", 3) : (null, 0);
				},
							[Instruction.MulI_Reg_PReg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"MULI r{s[0] & 0xF} [r{(s[0] & 0xF0) >> 4}] ", 2) : (null, 0);
				},
							[Instruction.Int_I8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 1 ? ($"INT 0x{s[0]:X2} ", 2) : (null, 0);
				},
							[Instruction.DivI_Reg_I8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"DIVI r{s[0] & 0xF} 0x{s[1]:X2} ", 3) : (null, 0);
				},
							[Instruction.DivI_Reg_Reg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"DIVI r{s[0] & 0xF} r{(s[0] & 0xF0) >> 4} ", 2) : (null, 0);
				},
							[Instruction.DivI_Reg_PI8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"DIVI r{s[0] & 0xF} [0x{s[1]:X2}] ", 3) : (null, 0);
				},
							[Instruction.DivI_Reg_PReg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"DIVI r{s[0] & 0xF} [r{(s[0] & 0xF0) >> 4}] ", 2) : (null, 0);
				},
							[Instruction.ModI_Reg_I8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"MODI r{s[0] & 0xF} 0x{s[1]:X2} ", 3) : (null, 0);
				},
							[Instruction.ModI_Reg_Reg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"MODI r{s[0] & 0xF} r{(s[0] & 0xF0) >> 4} ", 2) : (null, 0);
				},
							[Instruction.ModI_Reg_PI8] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"MODI r{s[0] & 0xF} [0x{s[1]:X2}] ", 3) : (null, 0);
				},
							[Instruction.ModI_Reg_PReg] = m =>
				{
					ReadOnlySpan<byte> s = m.Span;

					
					return s.Length >= 2 ? ($"MODI r{s[0] & 0xF} [r{(s[0] & 0xF0) >> 4}] ", 2) : (null, 0);
				},
					};

		public static string Disassemble(Memory<byte> m, out int size)
		{
            if (m.Length > 0 && InstructionDecompilation.TryGetValue((Instruction)m.Span[0], out var fn))
            {
                var (result, sz) = fn(m.Slice(1));
                size = sz;
                return result;
            }

            size = 0;
            return null;
		}

        public static void Assemble(string s, VM vm)
        {
			ushort memidx = 0;
			int i0, i1;

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
                    var start = idx;
                    while (idx < line.Length && !char.IsWhiteSpace(line[idx]))
                        ++idx;
                    tokens.Add(line[start..idx]);
                }

				if(!tokens.Any()) continue;

				static int GetNumber(string n) => n.StartsWith("0x") ? Convert.ToInt32(n, 16) : int.Parse(n);
				static bool IsReg(string s, out int r) 
				{ 
					var m = Regex.Match(s, @"^r(\d+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled); 
					r = m.Success ? GetNumber(m.Groups[1].Value) : 0; 
					return m.Success; 
				}
				static bool IsI8(string s, out int r) 
				{ 
					var m = Regex.Match(s, @"^(\d+|0x[\dA-Fa-f]+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled); 
					r = m.Success ? GetNumber(m.Groups[1].Value) : 0; 
					return m.Success; 
				}
				static bool IsPReg(string s, out int r) 
				{ 
					var m = Regex.Match(s, @"^\[r(\d+)\]$", RegexOptions.IgnoreCase | RegexOptions.Compiled); 
					r = m.Success ? GetNumber(m.Groups[1].Value) : 0; 
					return m.Success; 
				}
				static bool IsPI8(string s, out int r) 
				{ 
					var m = Regex.Match(s, @"^\[(\d+|0x[\dA-Fa-f]+)\]$", RegexOptions.IgnoreCase | RegexOptions.Compiled); 
					r = m.Success ? GetNumber(m.Groups[1].Value) : 0; 
					return m.Success; 
				}
				static bool IsAtAddress(string s, out int r)
				{ 
					var m = Regex.Match(s, @"^@(\d+|0x[\dA-Fa-f]+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled); 
					r = m.Success ? GetNumber(m.Groups[1].Value) : 0; 
					return m.Success; 
				}

                if (tokens.Count == 1 && IsAtAddress(tokens[0], out i0))
                {
                    // @addr
                    memidx = (ushort)i0;
                    continue;
                }

				if(tokens.Count == 2 && tokens[0].Equals("DB", StringComparison.OrdinalIgnoreCase) && IsI8(tokens[1], out i0))
				{
					// db i8
					vm.Memory[memidx++] = (byte)i0;
					continue;
				}

				if(tokens.Count == 1 && tokens[0].Equals("ORG", StringComparison.OrdinalIgnoreCase))
				{
                    MemoryMarshal.Write(vm.Memory.AsSpan(^2..), ref memidx);
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
									if(tokens[0].Equals("AddI", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.AddI_Reg_I8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.AddI_Reg_Reg;
																								vm.Memory[memidx++] = (byte)(((i0 & 0xF) << 4) | (i1 & 0xF));
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsPI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.AddI_Reg_PI8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsPReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.AddI_Reg_PReg;
																								vm.Memory[memidx++] = (byte)(((i0 & 0xF) << 4) | (i1 & 0xF));
																						continue;
						}
											}
									if(tokens[0].Equals("SubI", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.SubI_Reg_I8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.SubI_Reg_Reg;
																								vm.Memory[memidx++] = (byte)(((i0 & 0xF) << 4) | (i1 & 0xF));
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsPI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.SubI_Reg_PI8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsPReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.SubI_Reg_PReg;
																								vm.Memory[memidx++] = (byte)(((i0 & 0xF) << 4) | (i1 & 0xF));
																						continue;
						}
											}
									if(tokens[0].Equals("MulI", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.MulI_Reg_I8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.MulI_Reg_Reg;
																								vm.Memory[memidx++] = (byte)(((i0 & 0xF) << 4) | (i1 & 0xF));
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsPI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.MulI_Reg_PI8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsPReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.MulI_Reg_PReg;
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
											}
									if(tokens[0].Equals("DivI", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.DivI_Reg_I8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.DivI_Reg_Reg;
																								vm.Memory[memidx++] = (byte)(((i0 & 0xF) << 4) | (i1 & 0xF));
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsPI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.DivI_Reg_PI8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsPReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.DivI_Reg_PReg;
																								vm.Memory[memidx++] = (byte)(((i0 & 0xF) << 4) | (i1 & 0xF));
																						continue;
						}
											}
									if(tokens[0].Equals("ModI", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.ModI_Reg_I8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.ModI_Reg_Reg;
																								vm.Memory[memidx++] = (byte)(((i0 & 0xF) << 4) | (i1 & 0xF));
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsPI8(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.ModI_Reg_PI8;
																								vm.Memory[memidx++] = (byte)i0;
									vm.Memory[memidx++] = (byte)i1;
																						continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsPReg(tokens[2], out i1) )
						{
							vm.Memory[memidx++] = (byte)Instruction.ModI_Reg_PReg;
																								vm.Memory[memidx++] = (byte)(((i0 & 0xF) << 4) | (i1 & 0xF));
																						continue;
						}
											}
				
				throw new AssemblerException(lineidx - 1, line);
            }
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