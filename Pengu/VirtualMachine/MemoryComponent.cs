using System;

namespace Pengu.VirtualMachine
{
    class MemoryComponent : IMemory
    {
        public byte[] Memory { get; }

        public event Action<IMemory> RefreshRequired;
        public void FireRefreshRequired() => RefreshRequired?.Invoke(this);

        public MemoryComponent(int size) => Memory = new byte[size];
    }
}