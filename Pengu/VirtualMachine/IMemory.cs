using System;
using System.Collections.Generic;

namespace Pengu.VirtualMachine
{
    public abstract class IMemory
    {
        public IMemory(IList<byte> memory, string name) => (Memory, MemoryName) = (memory, name);

        public event Action<IMemory>? RefreshRequired;
        public void FireRefreshRequired() => RefreshRequired?.Invoke(this);

        public IList<byte> Memory { get; }
        public string MemoryName { get; }

        public abstract IMemory CloneAsMemory();

        protected static byte[] CloneMemoryData(int memorySize, byte[] memoryData)
        {
            var result = new byte[Math.Max(memorySize, memoryData.Length)];
            Array.Copy(memoryData, result, memoryData.Length);
            return result;
        }

        public void CopyFromMemory(IMemory mem)
        {
            if (mem is null)
                throw new ArgumentNullException(nameof(mem));

            if (Memory is byte[] arr)
                mem.Memory.CopyTo(arr, 0);
            else if (Memory is MemoryByteWithRefreshFeedback mb)
                mb[0] = mem.Memory[0];
            else
                throw new InvalidOperationException();
        }
    }
}