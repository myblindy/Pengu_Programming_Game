using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Pengu.Support
{
    static class Extensions
    {
        public static unsafe void CopyTo<T>(this Span<T> source, IntPtr dest, int destSize) where T : struct =>
            MemoryMarshal.Cast<T, byte>(source).CopyTo(new Span<byte>(dest.ToPointer(), destSize));
    }
}
