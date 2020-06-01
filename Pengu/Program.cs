using Pengu.Renderer;
using Pengu.VirtualMachine;
using System;
using System.Diagnostics;
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

            var vm = new VM(VMType.BitLength8, registers: 1, memory: 30);

            InstructionSet.Assemble(@"
@1
db 2
db 15

org
mov r0 0

.loop1
addi r0 1
cmp r0 10
jl .loop1

.loop2
subi r0 1
cmp r0 0
jg .loop2

jmp .loop1", vm);
            vm.Reset();

            using var renderer = new VulkanContext(vm, debug);
            renderer.Run();
        }
    }
}
