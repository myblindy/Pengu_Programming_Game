﻿using Pengu.Renderer;
using Pengu.VirtualMachine;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Pengu
{
    class Program
    {
        static void Main(string[] _)
        {
#if DEBUG 
            const bool debug = true;
#else
            const bool debug = false;
#endif

            var vm = new VM(VMType.BitLength8, registers: 1, memory: 60);

            InstructionSet.Assemble(vm, @"
db 0b0111111
db 0b0000110
db 0b1011011
db 0b1001111
db 0b1100110
db 0b1101101
db 0b1111101
db 0b0000111
db 0b1111111
db 0b1100111

; temporary storage for the current counter
.tmp db 0

org
mov r0 0

.loop
addi r0 1
modi r0 100
mov [.tmp] r0 

; first digit
divi r0 10
mov r0 [r0]
int 0x45

; second digit
mov r0 [.tmp]
modi r0 10
mov r0 [r0]
addi r0 0b10000000
int 0x45

; restore and loop
mov r0 [.tmp]
jmp .loop");

            using var renderer = new VulkanContext(vm, debug);
            renderer.Run();
        }
    }
}
