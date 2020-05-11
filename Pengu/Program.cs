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

            var vm = new VM(registers: 1, memory: 20);

            InstructionSet.Assemble(@"
@1
db 2
db 15

org
mov r0 [1]
muli r0 r0
addi r0 [2]
subi r0 6
mov [0] r0", vm);

            using var renderer = new VulkanContext(vm, debug);
            renderer.Run();

            //vm.Reset();
            //vm.RunNextInstruction(int.MaxValue);

            //Debug.WriteLine($"R: {vm.Registers[0]:X4} MEM: {string.Join(" ", vm.Memory.Select(m => m.ToString("X2")))}");

            //for (var m = vm.Memory.AsMemory(vm.StartInstructionPointer..); m.Length > 0;)
            //{
            //    var line = InstructionSet.Disassemble(m, out var size);
            //    if (line is null) break;
            //    Debug.WriteLine(line);
            //    m = m.Slice(size);
            //}
        }
    }
}
