using System;
using System.Runtime.CompilerServices;

namespace SpanJson.Internal
{
    public static class ArraySegmentExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty<T>(this in ArraySegment<T> segment)
        {
            var array = segment.Array;
            return array is null || 0U >= (uint)array.Length ? true : false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NonEmpty<T>(this in ArraySegment<T> segment)
        {
            var array = segment.Array;
            return array is null || 0U >= (uint)array.Length ? false : true;
        }

#if NETSTANDARD2_0 || NET471 || NET451

        public static ArraySegment<T> Slice<T>(this in ArraySegment<T> segment, int index)
        {
            var array = segment.Array;
            if (array is null)
            {
                ThrowHelper.ThrowInvalidOperationException_NullArray();
            }

            var count = segment.Count;
            if ((uint)index > (uint)count)
            {
                ThrowHelper.ThrowArgumentOutOfRange_IndexException();
            }

            return new ArraySegment<T>(array!, segment.Offset + index, count - index);
        }

        public static ArraySegment<T> Slice<T>(this in ArraySegment<T> segment, int index, int length)
        {
            var array = segment.Array;
            if (array is null)
            {
                ThrowHelper.ThrowInvalidOperationException_NullArray();
            }

            var count = segment.Count;
            if ((uint)index > (uint)count || (uint)length > (uint)(count - index))
            {
                ThrowHelper.ThrowArgumentOutOfRange_IndexException();
            }

            return new ArraySegment<T>(array!, segment.Offset + index, length);
        }

#endif
    }
}
