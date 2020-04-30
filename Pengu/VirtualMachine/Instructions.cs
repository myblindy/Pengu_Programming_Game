

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

        public static List<byte> Assemble(string s)
        {
            var bytes = new List<byte>();

			int i0, i1;

            var reader = new StringReader(s);
            string line;
            var tokens = new List<string>();
            while ((line = reader.ReadLine()) != null)
            {
                tokens.Clear();
                for (int idx = 0; idx < line.Length && !char.IsWhiteSpace(line[idx]); ++idx)
                {
                    var start = idx;
                    while (idx < line.Length && !char.IsWhiteSpace(line[idx]))
                        ++idx;
                    tokens.Add(line[start..idx]);
                }

				if(!tokens.Any()) continue;

				static bool IsReg(string s, out int r) 
				{ 
					var m = Regex.Match(s, @"^r(\d+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled); 
					r = m.Success ? Convert.ToInt32(m.Groups[1].Value) : 0; 
					return m.Success; 
				}
				static bool IsI8(string s, out int r) 
				{ 
					var m = Regex.Match(s, @"^(\d+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled); 
					r = m.Success ? Convert.ToInt32(m.Groups[1].Value) : 0; 
					return m.Success; 
				}
				static bool IsPReg(string s, out int r) 
				{ 
					var m = Regex.Match(s, @"^\[r(\d+)\]$", RegexOptions.IgnoreCase | RegexOptions.Compiled); 
					r = m.Success ? Convert.ToInt32(m.Groups[1].Value) : 0; 
					return m.Success; 
				}
				static bool IsPI8(string s, out int r) 
				{ 
					var m = Regex.Match(s, @"^\[(\d+)\]$", RegexOptions.IgnoreCase | RegexOptions.Compiled); 
					r = m.Success ? Convert.ToInt32(m.Groups[1].Value) : 0; 
					return m.Success; 
				}

									if(tokens[0].Equals("Mov", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsI8(tokens[2], out i1) )
						{
							bytes.Add((int)Instruction.Mov_Reg_I8);
															bytes.Add((byte)i0);
								bytes.Add((byte)i1);
														continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsReg(tokens[2], out i1) )
						{
							bytes.Add((int)Instruction.Mov_Reg_Reg);
															bytes.Add((byte)(((i0 & 0xF) << 4) | (i1 & 0xF)));
														continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsPI8(tokens[2], out i1) )
						{
							bytes.Add((int)Instruction.Mov_Reg_PI8);
															bytes.Add((byte)i0);
								bytes.Add((byte)i1);
														continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsPReg(tokens[2], out i1) )
						{
							bytes.Add((int)Instruction.Mov_Reg_PReg);
															bytes.Add((byte)(((i0 & 0xF) << 4) | (i1 & 0xF)));
														continue;
						}
												if(tokens.Count == 3
							 && IsPI8(tokens[1], out i0)  && IsI8(tokens[2], out i1) )
						{
							bytes.Add((int)Instruction.Mov_PI8_I8);
															bytes.Add((byte)i0);
								bytes.Add((byte)i1);
														continue;
						}
												if(tokens.Count == 3
							 && IsPI8(tokens[1], out i0)  && IsReg(tokens[2], out i1) )
						{
							bytes.Add((int)Instruction.Mov_PI8_Reg);
															bytes.Add((byte)i0);
								bytes.Add((byte)i1);
														continue;
						}
												if(tokens.Count == 3
							 && IsPI8(tokens[1], out i0)  && IsPI8(tokens[2], out i1) )
						{
							bytes.Add((int)Instruction.Mov_PI8_PI8);
															bytes.Add((byte)i0);
								bytes.Add((byte)i1);
														continue;
						}
												if(tokens.Count == 3
							 && IsPI8(tokens[1], out i0)  && IsPReg(tokens[2], out i1) )
						{
							bytes.Add((int)Instruction.Mov_PI8_PReg);
															bytes.Add((byte)i0);
								bytes.Add((byte)i1);
														continue;
						}
												if(tokens.Count == 3
							 && IsPReg(tokens[1], out i0)  && IsI8(tokens[2], out i1) )
						{
							bytes.Add((int)Instruction.Mov_PReg_I8);
															bytes.Add((byte)i0);
								bytes.Add((byte)i1);
														continue;
						}
												if(tokens.Count == 3
							 && IsPReg(tokens[1], out i0)  && IsReg(tokens[2], out i1) )
						{
							bytes.Add((int)Instruction.Mov_PReg_Reg);
															bytes.Add((byte)(((i0 & 0xF) << 4) | (i1 & 0xF)));
														continue;
						}
												if(tokens.Count == 3
							 && IsPReg(tokens[1], out i0)  && IsPI8(tokens[2], out i1) )
						{
							bytes.Add((int)Instruction.Mov_PReg_PI8);
															bytes.Add((byte)i0);
								bytes.Add((byte)i1);
														continue;
						}
												if(tokens.Count == 3
							 && IsPReg(tokens[1], out i0)  && IsPReg(tokens[2], out i1) )
						{
							bytes.Add((int)Instruction.Mov_PReg_PReg);
															bytes.Add((byte)(((i0 & 0xF) << 4) | (i1 & 0xF)));
														continue;
						}
											}
									if(tokens[0].Equals("AddI", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsI8(tokens[2], out i1) )
						{
							bytes.Add((int)Instruction.AddI_Reg_I8);
															bytes.Add((byte)i0);
								bytes.Add((byte)i1);
														continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsReg(tokens[2], out i1) )
						{
							bytes.Add((int)Instruction.AddI_Reg_Reg);
															bytes.Add((byte)(((i0 & 0xF) << 4) | (i1 & 0xF)));
														continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsPI8(tokens[2], out i1) )
						{
							bytes.Add((int)Instruction.AddI_Reg_PI8);
															bytes.Add((byte)i0);
								bytes.Add((byte)i1);
														continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsPReg(tokens[2], out i1) )
						{
							bytes.Add((int)Instruction.AddI_Reg_PReg);
															bytes.Add((byte)(((i0 & 0xF) << 4) | (i1 & 0xF)));
														continue;
						}
											}
									if(tokens[0].Equals("SubI", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsI8(tokens[2], out i1) )
						{
							bytes.Add((int)Instruction.SubI_Reg_I8);
															bytes.Add((byte)i0);
								bytes.Add((byte)i1);
														continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsReg(tokens[2], out i1) )
						{
							bytes.Add((int)Instruction.SubI_Reg_Reg);
															bytes.Add((byte)(((i0 & 0xF) << 4) | (i1 & 0xF)));
														continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsPI8(tokens[2], out i1) )
						{
							bytes.Add((int)Instruction.SubI_Reg_PI8);
															bytes.Add((byte)i0);
								bytes.Add((byte)i1);
														continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsPReg(tokens[2], out i1) )
						{
							bytes.Add((int)Instruction.SubI_Reg_PReg);
															bytes.Add((byte)(((i0 & 0xF) << 4) | (i1 & 0xF)));
														continue;
						}
											}
									if(tokens[0].Equals("MulI", StringComparison.OrdinalIgnoreCase))
					{
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsI8(tokens[2], out i1) )
						{
							bytes.Add((int)Instruction.MulI_Reg_I8);
															bytes.Add((byte)i0);
								bytes.Add((byte)i1);
														continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsReg(tokens[2], out i1) )
						{
							bytes.Add((int)Instruction.MulI_Reg_Reg);
															bytes.Add((byte)(((i0 & 0xF) << 4) | (i1 & 0xF)));
														continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsPI8(tokens[2], out i1) )
						{
							bytes.Add((int)Instruction.MulI_Reg_PI8);
															bytes.Add((byte)i0);
								bytes.Add((byte)i1);
														continue;
						}
												if(tokens.Count == 3
							 && IsReg(tokens[1], out i0)  && IsPReg(tokens[2], out i1) )
						{
							bytes.Add((int)Instruction.MulI_Reg_PReg);
															bytes.Add((byte)(((i0 & 0xF) << 4) | (i1 & 0xF)));
														continue;
						}
											}
				            }

            return bytes;
        }

	    static void I8ToI4I4(int input, out int v1, out int v2)
        {
            v1 = input & 0xF;
            v2 = (input & 0xF0) >> 4;
        }
	}
}