using System;
using System.Collections.Generic;

namespace Pengu.VirtualMachine
{
    public interface IMemory
    {
        public event Action<IMemory> RefreshRequired;
        public void FireRefreshRequired();
        
        public IList<byte> Memory { get; }
        public string MemoryName { get; }
    }
}