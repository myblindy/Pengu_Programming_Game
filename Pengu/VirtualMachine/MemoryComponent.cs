using System;
using System.Collections.Generic;

namespace Pengu.VirtualMachine
{
    class MemoryComponent : IMemory
    {
        public IList<byte> Memory { get; }
        public string MemoryName { get; }

        public event Action<IMemory>? RefreshRequired;
        public void FireRefreshRequired() => RefreshRequired?.Invoke(this);

        public MemoryComponent(string name, int? memorySize = default, byte[]? memoryData = null)
        {
            static byte[] CloneMemoryData(int memorySize, byte[] memoryData)
            {
                var result = new byte[Math.Max(memorySize, memoryData.Length)];
                Array.Copy(memoryData, result, memoryData.Length);
                return result;
            }

            MemoryName = name;
            Memory = memoryData is null
                ? memorySize.HasValue
                    ? new byte[memorySize.Value]
                    : throw new ArgumentException("Cannot create a memory with no size and no data")
                : memorySize.HasValue
                    ? CloneMemoryData(memorySize.Value, memoryData)
                    : (byte[])memoryData.Clone();
        }
    }
}