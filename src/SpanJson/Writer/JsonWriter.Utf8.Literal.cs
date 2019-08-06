namespace SpanJson
{
    using System;
    using System.Buffers;
    using System.Buffers.Text;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    partial struct JsonWriter<TSymbol>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Boolean(bool value)
        {
            if (value)
            {
                WriteUtf8Verbatim(0x65757274); // True
            }
            else
            {
                WriteUtf8Verbatim(0x736C6166, 0x65); // False
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Null()
        {
            WriteUtf8Verbatim(0x6C6C756E);
        }

        public void WriteUtf8Base64String(in ReadOnlySpan<byte> bytes)
        {
            ref var pos = ref _pos;
            int encodedLength = Base64.GetMaxEncodedToUtf8Length(bytes.Length);
            EnsureUnsafe(pos, encodedLength + 3);

            ref byte pinnableAddr = ref Utf8PinnableAddress;

            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);

            OperationStatus status = Base64.EncodeToUtf8(bytes, Utf8FreeSpan, out int consumed, out int written);
            Debug.Assert(status == OperationStatus.Done);
            Debug.Assert(consumed == bytes.Length);
            pos += written;

            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
        }
    }
}
