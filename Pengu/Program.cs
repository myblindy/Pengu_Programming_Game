using Pengu.Renderer;
using Pengu.VirtualMachine;
using System;
using System.Linq;

namespace Pengu
{
    class Program
    {
        static void Main(string[] _)
        {
            using var renderer = new VulkanContext(true);
            renderer.Run();

            // mov r0 [1]
            // muli r0 r0
            // addi r0 [2]
            // subi r0 6
            // mov [0] r0

            var vm = new VM(1, 20);
            var len = vm.LoadCode(new byte[] { 0x02, 0x00, 0x01, 0x15, 0x00, 0x0e, 0x00, 0x02, 0x10, 0x00, 0x06, 0x05, 0x00, 0x00 });
            vm.Memory[0x01] = 2;
            vm.Memory[0x02] = 15;
            vm.RunNextInstruction(int.MaxValue);

            Console.WriteLine($"R: {vm.Registers[0]:X4} MEM: {string.Join(" ", vm.Memory.Select(m => m.ToString("X2")))}");

            for (var m = vm.Memory.AsMemory(^len..); m.Length > 0;)
            {
                Console.WriteLine(InstructionSet.Disassemble(m, out var size));
                m = m.Slice(size);
            }
        }
    }
}
