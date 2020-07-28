using System;
using System.Collections.Generic;

namespace Pengu.VirtualMachine
{
    partial class SevenSegmentDigitDisplayComponent : IMemory
    {
        public override IMemory CloneAsMemory() =>
            new MemoryComponent(MemoryName, 1);

        public SevenSegmentDigitDisplayComponent(string name) : base(new MemoryByteWithRefreshFeedback(), name) =>
            ((MemoryByteWithRefreshFeedback)Memory).Component = this;
    }
}