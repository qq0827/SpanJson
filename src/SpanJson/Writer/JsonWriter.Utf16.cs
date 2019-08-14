namespace SpanJson
{
    using System;
    using System.Runtime.CompilerServices;
    using SpanJson.Internal;

    partial struct JsonWriter<TSymbol>
    {
        public override string ToString()
        {
            var s = new ReadOnlySpan<char>(_utf16Buffer, 0, _pos).ToString();
            Dispose();
            return s;
        }

        public char[] ToCharArray()
        {
            ref var alreadyWritten = ref _pos;
            if (0u >= (uint)alreadyWritten) { return JsonHelpers.Empty<char>(); }

            var borrowedBuffer = _utf16Buffer;
            if (borrowedBuffer is null) { return JsonHelpers.Empty<char>(); }

            var destination = new char[alreadyWritten];
            Utf16WrittenSpan.CopyTo(destination);
            Dispose();
            return destination;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16BeginObject()
        {
            ref var pos = ref _pos;
            EnsureUnsafe(pos, 1);

            Unsafe.Add(ref Utf16PinnableAddress, pos++) = JsonUtf16Constant.BeginObject;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16EndObject()
        {
            ref var pos = ref _pos;
            EnsureUnsafe(pos, 1);

            Unsafe.Add(ref Utf16PinnableAddress, pos++) = JsonUtf16Constant.EndObject;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16BeginArray()
        {
            ref var pos = ref _pos;
            EnsureUnsafe(pos, 1);

            Unsafe.Add(ref Utf16PinnableAddress, pos++) = JsonUtf16Constant.BeginArray;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16EndArray()
        {
            ref var pos = ref _pos;
            EnsureUnsafe(pos, 1);

            Unsafe.Add(ref Utf16PinnableAddress, pos++) = JsonUtf16Constant.EndArray;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16ValueSeparator()
        {
            ref var pos = ref _pos;
            EnsureUnsafe(pos, 1);

            Unsafe.Add(ref Utf16PinnableAddress, pos++) = JsonUtf16Constant.ValueSeparator;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16NewLine()
        {
            const int newLineLength = 2;

            ref var pos = ref _pos;
            EnsureUnsafe(pos, newLineLength);

            ref char pinnableAddr = ref Utf16PinnableAddress;
            Unsafe.Add(ref pinnableAddr, pos) = JsonUtf16Constant.CarriageReturn;
            Unsafe.Add(ref pinnableAddr, pos + 1) = JsonUtf16Constant.LineFeed;
            pos += newLineLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16Indentation(int count)
        {
            ref var pos = ref _pos;
            EnsureUnsafe(pos, count);

            ref char pinnableAddr = ref Unsafe.Add(ref Utf16PinnableAddress, pos);
            for (var i = 0; i < count; i++)
            {
                Unsafe.Add(ref pinnableAddr, i) = JsonUtf16Constant.Space;
            }
            pos += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16DoubleQuote()
        {
            ref var pos = ref _pos;
            EnsureUnsafe(pos, 1);

            WriteUtf16DoubleQuote(ref Utf16PinnableAddress, ref pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteUtf16DoubleQuote(ref char destination, ref int pos)
        {
            Unsafe.Add(ref destination, pos++) = JsonUtf16Constant.String;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16NameSeparator()
        {
            ref var pos = ref _pos;
            EnsureUnsafe(pos, 1);

            WriteUtf16NameSeparator(ref Utf16PinnableAddress, ref pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteUtf16NameSeparator(ref char destination, ref int pos)
        {
            Unsafe.Add(ref destination, pos++) = JsonUtf16Constant.NameSeparator;
        }
    }
}
