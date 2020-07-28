using System;
using System.Collections.Generic;

namespace Pengu.VirtualMachine
{
    class MemoryComponent : IMemory
    {
        public override IMemory CloneAsMemory() =>
            new MemoryComponent(MemoryName, Memory.Count);

        public MemoryComponent(string name, int? memorySize = default, byte[]? memoryData = null) :
            base(memoryData is null
                ? memorySize.HasValue
                    ? new byte[memorySize.Value]
                    : throw new ArgumentException("Cannot create a memory with no size and no data")
                : memorySize.HasValue
                    ? CloneMemoryData(memorySize.Value, memoryData)
                    : (byte[])memoryData.Clone(), name)
        {
        }
    }
}