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

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(uint a, byte b)
        {
            ref var pos = ref _pos;
            Ensure(pos, 5);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 4;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(uint a, ushort b)
        {
            ref var pos = ref _pos;
            Ensure(pos, 6);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 4;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(uint a, ushort b, byte c)
        {
            ref var pos = ref _pos;
            Ensure(pos, 7);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 4;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 2;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), c);
            pos += 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a)
        {
            ref var pos = ref _pos;
            Ensure(pos, 8);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, byte b)
        {
            ref var pos = ref _pos;
            Ensure(pos, 9);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ushort b)
        {
            ref var pos = ref _pos;
            Ensure(pos, 10);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ushort b, byte c)
        {
            ref var pos = ref _pos;
            Ensure(pos, 11);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 2;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), c);
            pos += 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, uint b)
        {
            ref var pos = ref _pos;
            Ensure(pos, 12);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, uint b, byte c)
        {
            ref var pos = ref _pos;
            Ensure(pos, 13);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 4;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), c);
            pos += 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, uint b, ushort c)
        {
            ref var pos = ref _pos;
            Ensure(pos, 14);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 4;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), c);
            pos += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, uint b, ushort c, byte d)
        {
            ref var pos = ref _pos;
            Ensure(pos, 15);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 4;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), c);
            pos += 2;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), d);
            pos += 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b)
        {
            ref var pos = ref _pos;
            Ensure(pos, 16);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, byte c)
        {
            ref var pos = ref _pos;
            Ensure(pos, 17);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), c);
            pos += 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, ushort c)
        {
            ref var pos = ref _pos;
            Ensure(pos, 18);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), c);
            pos += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, ushort c, byte d)
        {
            ref var pos = ref _pos;
            Ensure(pos, 19);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), c);
            pos += 2;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), d);
            pos += 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, uint c)
        {
            ref var pos = ref _pos;
            Ensure(pos, 20);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), c);
            pos += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, uint c, byte d)
        {
            ref var pos = ref _pos;
            Ensure(pos, 21);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), c);
            pos += 4;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), d);
            pos += 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, uint c, ushort d)
        {
            ref var pos = ref _pos;
            Ensure(pos, 22);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), c);
            pos += 4;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), d);
            pos += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, uint c, ushort d, byte e)
        {
            ref var pos = ref _pos;
            Ensure(pos, 23);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), c);
            pos += 4;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), d);
            pos += 2;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), e);
            pos += 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, ulong c)
        {
            ref var pos = ref _pos;
            Ensure(pos, 24);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), c);
            pos += 8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, ulong c, byte d)
        {
            ref var pos = ref _pos;
            Ensure(pos, 25);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), c);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), d);
            pos += 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, ulong c, ushort d)
        {
            ref var pos = ref _pos;
            Ensure(pos, 26);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), c);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), d);
            pos += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, ulong c, ushort d, byte e)
        {
            ref var pos = ref _pos;
            Ensure(pos, 27);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), c);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), d);
            pos += 2;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), e);
            pos += 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, ulong c, uint d)
        {
            ref var pos = ref _pos;
            Ensure(pos, 28);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), c);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), d);
            pos += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, ulong c, uint d, byte e)
        {
            ref var pos = ref _pos;
            Ensure(pos, 29);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), c);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), d);
            pos += 4;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), e);
            pos += 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, ulong c, uint d, ushort e)
        {
            ref var pos = ref _pos;
            Ensure(pos, 30);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), c);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), d);
            pos += 4;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), e);
            pos += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, ulong c, uint d, ushort e, byte f)
        {
            ref var pos = ref _pos;
            Ensure(pos, 31);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), c);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), d);
            pos += 4;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), e);
            pos += 2;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), f);
            pos += 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(ulong a, ulong b, ulong c, ulong d)
        {
            ref var pos = ref _pos;
            Ensure(pos, 32);

            ref var bStart = ref PinnableUtf8Address;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), a);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), b);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), c);
            pos += 8;
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref bStart, pos), d);
            pos += 8;
        }

    }
}