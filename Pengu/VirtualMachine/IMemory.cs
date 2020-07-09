using System;

namespace Pengu.VirtualMachine
{
    public interface IMemory
    {
        public event Action<IMemory> RefreshRequired;
        public void FireRefreshRequired();
        
        public byte[] Memory { get; }
    }
}