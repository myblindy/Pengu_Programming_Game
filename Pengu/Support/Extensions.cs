using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Pengu.Support
{
    static class Extensions
    {
        public static int CountWhile<T>(this IEnumerable<T> source, Func<T, bool> test)
        {
            int count = 0;
            foreach (var item in source)
                if (test(item))
                    ++count;
                else
                    break;

            return count;
        }

        public static int CountWhileLast<T>(this IEnumerable<T> source, Func<T, bool> test)
        {
            if (source is string s)
            {
                int idx;
                for (idx = s.Length - 1; test((T)(object)s[idx]) && idx >= 0; --idx) { }
                return s.Length - idx - 1;
            }

            bool lastOkay = false;
            int count = 0, lastOkayIndex = 0;
            foreach (var item in source)
                if (test(item))
                {
                    if (!lastOkay)
                        (lastOkayIndex, lastOkay) = (count, true);
                    ++count;
                }
                else
                    lastOkay = false;

            return lastOkay ? count - lastOkayIndex : 0;
        }

        public static unsafe void CopyTo<T>(this Span<T> source, IntPtr dest, int destSize) where T : struct =>
            MemoryMarshal.Cast<T, byte>(source).CopyTo(new Span<byte>(dest.ToPointer(), destSize));

        public static void SetAll<T>(this IList<T> list, T val)
        {
            // todo optimize for arrays and Array.Clear
            for (int i = 0; i < list.Count; ++i)
                list[i] = val;
        }

        public static Memory<T> AsMemory<T>(this IList<T> source, Index index) =>
            source is T[] arr ? MemoryExtensions.AsMemory(arr, index) : throw new InvalidOperationException();

        public static Memory<T> AsMemory<T>(this IList<T> source, Range range) =>
            source is T[] arr ? MemoryExtensions.AsMemory(arr, range) : throw new InvalidOperationException();

        public static Memory<T> AsMemory<T>(this IList<T> source, int start) =>
            source is T[] arr ? MemoryExtensions.AsMemory(arr, start) : throw new InvalidOperationException();

        public static Memory<T> AsMemory<T>(this IList<T> source, int start, int count) =>
            source is T[] arr ? MemoryExtensions.AsMemory(arr, start, count) : throw new InvalidOperationException();

        public static Span<T> AsSpan<T>(this IList<T> source, Index index) =>
            source is T[] arr ? MemoryExtensions.AsSpan(arr, index) : throw new InvalidOperationException();

        public static Span<T> AsSpan<T>(this IList<T> source, Range range) =>
            source is T[] arr ? MemoryExtensions.AsSpan(arr, range) : throw new InvalidOperationException();

        public static Span<T> AsSpan<T>(this IList<T> source, int start) =>
            source is T[] arr ? MemoryExtensions.AsSpan(arr, start) : throw new InvalidOperationException();

        public static Span<T> AsSpan<T>(this IList<T> source, int start, int count) =>
            source is T[] arr ? MemoryExtensions.AsSpan(arr, start, count) : throw new InvalidOperationException();

        public static int CeilingIntegerDivide(this int dividend, int divisor) =>
            (dividend + (divisor - 1)) / divisor;
    }
}
