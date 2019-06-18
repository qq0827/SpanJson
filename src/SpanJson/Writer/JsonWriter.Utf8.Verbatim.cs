namespace SpanJson
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using SpanJson.Internal;

    partial struct JsonWriter<TSymbol>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Raw(byte[] value)
        {
            var count = value.Length;
            if (0u >= (uint)count) { return; }

            ref var pos = ref _pos;
            EnsureUnsafe(pos, count);
            UnsafeMemory.WriteRaw(ref Utf8PinnableAddress, ref value[0], count, ref pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Raw(in ReadOnlySpan<byte> value)
        {
            var count = value.Length;
            if (0u >= (uint)count) { return; }

            ref var pos = ref _pos;
            EnsureUnsafe(pos, count);
            UnsafeMemory.WriteRaw(ref Utf8PinnableAddress, ref MemoryMarshal.GetReference(value), count, ref pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(byte[] value)
        {
            var count = value.Length;
            if (0u >= (uint)count) { return; }

            ref var pos = ref _pos;
            EnsureUnsafe(pos, count);
            UnsafeMemory.WriteRawBytes(ref Utf8PinnableAddress, ref value[0], count, ref pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(in ReadOnlySpan<byte> value)
        {
            var count = value.Length;
            if (0u >= (uint)count) { return; }

            ref var pos = ref _pos;
            EnsureUnsafe(pos, count);
            UnsafeMemory.WriteRawBytes(ref Utf8PinnableAddress, ref MemoryMarshal.GetReference(value), count, ref pos);
        }
    }
}
