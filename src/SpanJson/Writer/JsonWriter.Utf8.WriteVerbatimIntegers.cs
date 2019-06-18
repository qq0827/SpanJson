using System;
using System.Runtime.CompilerServices;

namespace SpanJson
{
    partial struct JsonWriter<TSymbol>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(uint a)
        {
            ref var pos = ref _pos;
            Ensure(pos, 4);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            pos += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(uint a, byte b)
        {
            ref var pos = ref _pos;
            Ensure(pos, 5);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 4), b);
            pos += 5;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(uint a, ushort b)
        {
            ref var pos = ref _pos;
            Ensure(pos, 6);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 4), b);
            pos += 6;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(uint a, ushort b, byte c)
        {
            ref var pos = ref _pos;
            Ensure(pos, 7);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 4), b);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 6), c);
            pos += 7;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a)
        {
            ref var pos = ref _pos;
            Ensure(pos, 8);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            pos += 8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, byte b)
        {
            ref var pos = ref _pos;
            Ensure(pos, 9);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 8), b);
            pos += 9;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ushort b)
        {
            ref var pos = ref _pos;
            Ensure(pos, 10);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 8), b);
            pos += 10;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ushort b, byte c)
        {
            ref var pos = ref _pos;
            Ensure(pos, 11);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 8), b);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 10), c);
            pos += 11;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, uint b)
        {
            ref var pos = ref _pos;
            Ensure(pos, 12);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 8), b);
            pos += 12;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, uint b, byte c)
        {
            ref var pos = ref _pos;
            Ensure(pos, 13);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 8), b);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 12), c);
            pos += 13;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, uint b, ushort c)
        {
            ref var pos = ref _pos;
            Ensure(pos, 14);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 8), b);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 12), c);
            pos += 14;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, uint b, ushort c, byte d)
        {
            ref var pos = ref _pos;
            Ensure(pos, 15);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 8), b);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 12), c);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 14), d);
            pos += 15;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b)
        {
            ref var pos = ref _pos;
            Ensure(pos, 16);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 8), b);
            pos += 16;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, byte c)
        {
            ref var pos = ref _pos;
            Ensure(pos, 17);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 8), b);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 16), c);
            pos += 17;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, ushort c)
        {
            ref var pos = ref _pos;
            Ensure(pos, 18);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 8), b);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 16), c);
            pos += 18;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, ushort c, byte d)
        {
            ref var pos = ref _pos;
            Ensure(pos, 19);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 8), b);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 16), c);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 18), d);
            pos += 19;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, uint c)
        {
            ref var pos = ref _pos;
            Ensure(pos, 20);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 8), b);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 16), c);
            pos += 20;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, uint c, byte d)
        {
            ref var pos = ref _pos;
            Ensure(pos, 21);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 8), b);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 16), c);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 20), d);
            pos += 21;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, uint c, ushort d)
        {
            ref var pos = ref _pos;
            Ensure(pos, 22);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 8), b);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 16), c);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 20), d);
            pos += 22;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, uint c, ushort d, byte e)
        {
            ref var pos = ref _pos;
            Ensure(pos, 23);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 8), b);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 16), c);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 20), d);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 22), e);
            pos += 23;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, ulong c)
        {
            ref var pos = ref _pos;
            Ensure(pos, 24);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 8), b);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 16), c);
            pos += 24;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, ulong c, byte d)
        {
            ref var pos = ref _pos;
            Ensure(pos, 25);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 8), b);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 16), c);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 24), d);
            pos += 25;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, ulong c, ushort d)
        {
            ref var pos = ref _pos;
            Ensure(pos, 26);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 8), b);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 16), c);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 24), d);
            pos += 26;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, ulong c, ushort d, byte e)
        {
            ref var pos = ref _pos;
            Ensure(pos, 27);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 8), b);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 16), c);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 24), d);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 26), e);
            pos += 27;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, ulong c, uint d)
        {
            ref var pos = ref _pos;
            Ensure(pos, 28);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 8), b);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 16), c);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 24), d);
            pos += 28;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, ulong c, uint d, byte e)
        {
            ref var pos = ref _pos;
            Ensure(pos, 29);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 8), b);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 16), c);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 24), d);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 28), e);
            pos += 29;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, ulong c, uint d, ushort e)
        {
            ref var pos = ref _pos;
            Ensure(pos, 30);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 8), b);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 16), c);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 24), d);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 28), e);
            pos += 30;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, ulong c, uint d, ushort e, byte f)
        {
            ref var pos = ref _pos;
            Ensure(pos, 31);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 8), b);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 16), c);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 24), d);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 28), e);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 30), f);
            pos += 31;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, ulong c, ulong d)
        {
            ref var pos = ref _pos;
            Ensure(pos, 32);

            ref var bStart = ref Utf8PinnableAddress;
            IntPtr offset = (IntPtr)pos;
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset), a);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 8), b);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 16), c);
            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref bStart, offset + 24), d);
            pos += 32;
        }

    }
}