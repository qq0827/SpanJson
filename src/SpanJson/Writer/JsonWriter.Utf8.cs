namespace SpanJson
{
    using System;
    using System.Runtime.CompilerServices;
    using SpanJson.Internal;

    partial struct JsonWriter<TSymbol>
    {
        public byte[] ToByteArray()
        {
            ref var alreadyWritten = ref _pos;
            if (0u >= (uint)alreadyWritten) { return JsonHelpers.Empty<byte>(); }

            var borrowedBuffer = _utf8Buffer;
            if (borrowedBuffer is null) { return JsonHelpers.Empty<byte>(); }

            var destination = new byte[alreadyWritten];
            BinaryUtil.CopyMemory(borrowedBuffer, 0, destination, 0, alreadyWritten);
            Dispose();
            return destination;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8BeginObject()
        {
            ref var pos = ref _pos;
            EnsureUnsafe(pos, 1);

            Unsafe.AddByteOffset(ref Utf8PinnableAddress, (IntPtr)pos++) = JsonUtf8Constant.BeginObject;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8EndObject()
        {
            ref var pos = ref _pos;
            EnsureUnsafe(pos, 1);

            Unsafe.AddByteOffset(ref Utf8PinnableAddress, (IntPtr)pos++) = JsonUtf8Constant.EndObject;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8BeginArray()
        {
            ref var pos = ref _pos;
            EnsureUnsafe(pos, 1);

            Unsafe.AddByteOffset(ref Utf8PinnableAddress, (IntPtr)pos++) = JsonUtf8Constant.BeginArray;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8EndArray()
        {
            ref var pos = ref _pos;
            EnsureUnsafe(pos, 1);

            Unsafe.AddByteOffset(ref Utf8PinnableAddress, (IntPtr)pos++) = JsonUtf8Constant.EndArray;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8ValueSeparator()
        {
            ref var pos = ref _pos;
            EnsureUnsafe(pos, 1);

            Unsafe.AddByteOffset(ref Utf8PinnableAddress, (IntPtr)pos++) = JsonUtf8Constant.ValueSeparator;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8NewLine()
        {
            const int newLineLength = 2;

            ref var pos = ref _pos;
            EnsureUnsafe(pos, newLineLength);

            ref byte pinnableAddr = ref Utf8PinnableAddress;
            var offset = (IntPtr)pos;
            Unsafe.AddByteOffset(ref pinnableAddr, offset) = JsonUtf8Constant.CarriageReturn;
            Unsafe.AddByteOffset(ref pinnableAddr, offset + 1) = JsonUtf8Constant.LineFeed;
            pos += newLineLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Indentation(int count)
        {
            ref var pos = ref _pos;
            EnsureUnsafe(pos, count);

            ref byte pinnableAddr = ref Unsafe.AddByteOffset(ref Utf8PinnableAddress, (IntPtr)pos);
            Unsafe.InitBlockUnaligned(ref pinnableAddr, JsonUtf8Constant.Space, unchecked((uint)count));
            pos += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8DoubleQuote()
        {
            ref var pos = ref _pos;
            EnsureUnsafe(pos, 1);

            WriteUtf8DoubleQuote(ref Utf8PinnableAddress, ref pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteUtf8DoubleQuote(ref byte destination, ref int pos)
        {
            Unsafe.AddByteOffset(ref destination, (IntPtr)pos) = JsonUtf8Constant.String;
            pos++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8NameSeparator()
        {
            ref var pos = ref _pos;
            EnsureUnsafe(pos, 1);

            WriteUtf8NameSeparator(ref Utf8PinnableAddress, ref pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteUtf8NameSeparator(ref byte destination, ref int pos)
        {
            Unsafe.AddByteOffset(ref destination, (IntPtr)pos++) = JsonUtf8Constant.NameSeparator;
        }
    }
}
