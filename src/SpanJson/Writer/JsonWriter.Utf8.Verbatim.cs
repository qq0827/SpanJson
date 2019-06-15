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
            if (null == value) { return; }

            UnsafeMemory.WriteRaw(ref this, value, ref _pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Raw(in ReadOnlySpan<byte> value)
        {
            var count = value.Length;
            if (0u >= (uint)count) { return; }

            ref var pos = ref _pos;
            Ensure(pos, count);
            UnsafeMemory.WriteRawUnsafe(ref PinnableUtf8Address, ref MemoryMarshal.GetReference(value), count, ref pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(byte[] value)
        {
            if (null == value) { return; }

            ref var pos = ref _pos;
            UnsafeMemory.WriteRawBytes(ref this, value, ref pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(in ReadOnlySpan<byte> value)
        {
            var count = value.Length;
            if (0u >= (uint)count) { return; }

            ref var pos = ref _pos;
            Ensure(pos, count);
            UnsafeMemory.WriteRawBytesUnsafe(ref PinnableUtf8Address, ref MemoryMarshal.GetReference(value), count, ref pos);
        }
    }
}
