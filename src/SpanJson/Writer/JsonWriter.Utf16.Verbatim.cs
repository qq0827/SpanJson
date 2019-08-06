namespace SpanJson
{
    using System;
    using System.Runtime.CompilerServices;

    partial struct JsonWriter<TSymbol>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16Verbatim(string value)
        {
            WriteUtf16Verbatim(value.AsSpan());
        }

        public void WriteUtf16Verbatim(in ReadOnlySpan<char> value)
        {
            ref var pos = ref _pos;
            EnsureUnsafe(pos, value.Length);

            value.CopyTo(Utf16FreeSpan);
            pos += value.Length;
        }
    }
}
