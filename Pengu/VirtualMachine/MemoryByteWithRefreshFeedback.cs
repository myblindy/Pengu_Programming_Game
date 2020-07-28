using System;
using System.Collections;
using System.Collections.Generic;

namespace Pengu.VirtualMachine
{
    class MemoryByteWithRefreshFeedback : IList<byte>
    {
        public IMemory? Component { get; set; }

        byte value;

        public byte this[int index] { get => value; set { this.value = value; Component?.FireRefreshRequired(); } }

        public int Count => 1;

        public bool IsReadOnly => false;

        public void Add(byte item) => throw new NotImplementedException();

        public void Clear() => throw new NotImplementedException();

        public bool Contains(byte item) => value == item;

        public void CopyTo(byte[] array, int arrayIndex) => throw new NotImplementedException();

        public IEnumerator<byte> GetEnumerator()
        {
            yield return value;
        }

        public int IndexOf(byte item) => throw new NotImplementedException();

        public void Insert(int index, byte item) => throw new NotImplementedException();

        public bool Remove(byte item) => throw new NotImplementedException();

        public void RemoveAt(int index) => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
