namespace SpanJson
{
    using System;
    using System.Buffers.Text;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using SpanJson.Helpers;
    using SpanJson.Internal;
    using SpanJson.Internal.DoubleConversion;

    partial struct JsonWriter<TSymbol>
    {
        public byte[] ToByteArray()
        {
            ref var alreadyWritten = ref _pos;
            if (0u >= (uint)alreadyWritten) { return JsonHelpers.Empty<byte>(); }

            var borrowedBuffer = _utf8Buffer;
            if (null == borrowedBuffer) { return JsonHelpers.Empty<byte>(); }

            var destination = new byte[alreadyWritten];
            BinaryUtil.CopyMemory(borrowedBuffer, 0, destination, 0, alreadyWritten);
            Dispose();
            return destination;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8SByte(sbyte value)
        {
            WriteUtf8Int64Internal(ref this, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Int16(short value)
        {
            WriteUtf8Int64Internal(ref this, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Int32(int value)
        {
            WriteUtf8Int64Internal(ref this, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Int64(long value)
        {
            WriteUtf8Int64Internal(ref this, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteUtf8Int64Internal(ref JsonWriter<TSymbol> writer, long value)
        {
            if (value < 0L)
            {
                ref var pos = ref writer._pos;
                writer.Ensure(pos, 1);

                ref byte pinnableAddr = ref writer.PinnableUtf8Address;
                Unsafe.AddByteOffset(ref pinnableAddr, (IntPtr)pos) = (byte)'-';
                pos++;

                value = unchecked(-value);
            }

            WriteUtf8UInt64Internal(ref writer, (ulong)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteUtf8UInt64Internal(ref JsonWriter<TSymbol> writer, ulong value)
        {
            ref var pos = ref writer._pos;

            if (value < 10ul)
            {
                writer.Ensure(pos, 1);

                Unsafe.AddByteOffset(ref writer.PinnableUtf8Address, (IntPtr)pos) = (byte)('0' + value);
                pos++;
                return;
            }

            var digits = FormatterUtils.CountDigits(value);

            writer.Ensure(pos, digits);
            ref byte pinnableAddr = ref writer.PinnableUtf8Address;

            var offset = (IntPtr)pos;
            for (var i = digits - 1; i >= 0; i--)
            {
                var temp = '0' + value;
                value /= 10ul;
                Unsafe.AddByteOffset(ref pinnableAddr, offset + i) = (byte)(temp - value * 10ul);
            }

            pos += digits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Byte(byte value)
        {
            WriteUtf8UInt64Internal(ref this, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8UInt16(ushort value)
        {
            WriteUtf8UInt64Internal(ref this, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8UInt32(uint value)
        {
            WriteUtf8UInt64Internal(ref this, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8UInt64(ulong value)
        {
            WriteUtf8UInt64Internal(ref this, value);
        }

        public void WriteUtf8Single(float value)
        {
#if NETSTANDARD2_0 || NET471 || NET451
            if (float.IsNaN(value) || float.IsInfinity(value))
#else
            if (!float.IsFinite(value))
#endif
            {
                ThrowHelper.ThrowArgumentException_InvalidFloatValueForJson();
            }

            var buffer = TinyMemoryPool<byte>.GetBuffer();
            var count = DoubleToStringConverter.GetBytes(ref buffer, 0, value);

            ref var pos = ref _pos;
            Ensure(pos, count);
            UnsafeMemory.WriteRaw(ref PinnableUtf8Address, ref buffer[0], count, ref pos);
        }

        public void WriteUtf8Double(double value)
        {
#if NETSTANDARD2_0 || NET471 || NET451
            if (double.IsNaN(value) || double.IsInfinity(value))
#else
            if (!double.IsFinite(value))
#endif
            {
                ThrowHelper.ThrowArgumentException_InvalidDoubleValueForJson();
            }

            var buffer = TinyMemoryPool<byte>.GetBuffer();
            var count = DoubleToStringConverter.GetBytes(ref buffer, 0, value);

            ref var pos = ref _pos;
            Ensure(pos, count);
            UnsafeMemory.WriteRaw(ref PinnableUtf8Address, ref buffer[0], count, ref pos);
        }

        public void WriteUtf8Decimal(decimal value)
        {
            ref var pos = ref _pos;
            Ensure(pos, JsonSharedConstant.MaximumFormatDecimalLength);
            var result = Utf8Formatter.TryFormat(value, Utf8Span, out int bytesWritten);
            Debug.Assert(result);
            pos += bytesWritten;
        }

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

        public void WriteUtf8Char(char value)
        {
            ref var pos = ref _pos;
            const int size = 8; // 1-6 chars + two JsonUtf8Constant.DoubleQuote
            Ensure(pos, size);

            ref byte pinnableAddr = ref PinnableUtf8Address;

            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
            WriteUtf8CharInternal(ref this, ref pinnableAddr, value, ref pos);
            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
        }

        public void WriteUtf8DateTime(DateTime value)
        {
            ref var pos = ref _pos;
            const int dtSize = 35; // Form o + two JsonUtf8Constant.DoubleQuote
            Ensure(pos, dtSize);

            ref byte pinnableAddr = ref PinnableUtf8Address;

            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
            DateTimeFormatter.TryFormat(value, Utf8Span, out var bytesWritten);
            pos += bytesWritten;
            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
        }

        public void WriteUtf8DateTimeOffset(DateTimeOffset value)
        {
            ref var pos = ref _pos;
            const int dtSize = 35; // Form o + two JsonUtf8Constant.DoubleQuote
            Ensure(pos, dtSize);

            ref byte pinnableAddr = ref PinnableUtf8Address;

            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
            DateTimeFormatter.TryFormat(value, Utf8Span, out var bytesWritten);
            pos += bytesWritten;
            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
        }

        public void WriteUtf8TimeSpan(TimeSpan value)
        {
            ref var pos = ref _pos;
            const int dtSize = 20; // Form o + two JsonUtf8Constant.DoubleQuote
            Ensure(pos, dtSize);

            ref byte pinnableAddr = ref PinnableUtf8Address;

            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
            Utf8Formatter.TryFormat(value, Utf8Span, out var bytesWritten);
            pos += bytesWritten;
            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
        }

        public void WriteUtf8Guid(Guid value)
        {
            ref var pos = ref _pos;
            const int guidSize = 42; // Format D + two JsonUtf8Constant.DoubleQuote;
            Ensure(pos, guidSize);

            ref byte pinnableAddr = ref PinnableUtf8Address;

            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
            new GuidBits(ref value).Write(ref pinnableAddr, ref pos); // len = 36
            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8String(string value)
        {
            WriteUtf8String(value.AsSpan());
        }

        /// <summary>We know that for a pure ascii string all characters will fit if there are no escapes
        /// We make sure that initially the buffer is large enough and an additional fully escaped char fits too
        /// After each escape we make sure that all remaining ascii chars and extra fully escaped char fit
        /// For UTF8 encoded bytes we make sure that the 5 for the fully escaped value and 4 for the utf fit
        /// That's all done to make sure we don't have resizing in the fast path (the ascii case).</summary>
        public void WriteUtf8String(in ReadOnlySpan<char> value)
        {
            ref var pos = ref _pos;
            var valueLen = value.Length;
            uint nLen = (uint)valueLen;
            var sLength = Encoding.UTF8.GetMaxByteCount(valueLen) + 7; // assume that a fully escaped char fits too + 2 double quotes
            Ensure(pos, sLength);
            ref byte pinnableAddr = ref PinnableUtf8Address;

            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
            var index = 0;
            var from = 0;
            ref char utf16Source = ref MemoryMarshal.GetReference(value);
            while ((uint)index < nLen)
            {
                ref readonly var c = ref Unsafe.Add(ref utf16Source, index);
                if (c < 0x20 || c == JsonUtf8Constant.DoubleQuote || c == JsonUtf8Constant.Solidus || c == JsonUtf8Constant.ReverseSolidus)
                {
                    var length = index - from;
#if NETSTANDARD2_0 || NET471 || NET451
                    pos += TextEncodings.Utf8.GetBytes(value.Slice(from, length), Utf8Span);
#else
                    pos += Encoding.UTF8.GetBytes(value.Slice(from, length), Utf8Span);
#endif
                    WriteEscapedUtf8CharInternal(ref pinnableAddr, c, ref pos);

                    index++;
                    var remaining = 5 + valueLen - index; // make sure that all characters and an extra 5 for a full escape still fit
                    Ensure(pos, remaining);
                    pinnableAddr = ref PinnableUtf8Address;
                    from = index;
                }
                else
                {
                    index++;
                }
            }

            // Still chars to encode
            if ((uint)from < nLen)
            {
#if NETSTANDARD2_0 || NET471 || NET451
                pos += TextEncodings.Utf8.GetBytes(value.Slice(from), Utf8Span);
#else
                pos += Encoding.UTF8.GetBytes(value.Slice(from), Utf8Span);
#endif
            }

            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
        }

        private static void WriteEscapedUtf8CharInternal(ref byte destination, char value, ref int pos)
        {
            switch (value)
            {
                case JsonUtf16Constant.DoubleQuote:
                    WriteUtf8SingleEscapedChar(ref destination, JsonUtf16Constant.DoubleQuote, ref pos);
                    break;
                case JsonUtf16Constant.Solidus:
                    WriteUtf8SingleEscapedChar(ref destination, JsonUtf16Constant.Solidus, ref pos);
                    break;
                case JsonUtf16Constant.ReverseSolidus:
                    WriteUtf8SingleEscapedChar(ref destination, JsonUtf16Constant.ReverseSolidus, ref pos);
                    break;
                case '\b':
                    WriteUtf8SingleEscapedChar(ref destination, 'b', ref pos);
                    break;
                case '\f':
                    WriteUtf8SingleEscapedChar(ref destination, 'f', ref pos);
                    break;
                case '\n':
                    WriteUtf8SingleEscapedChar(ref destination, 'n', ref pos);
                    break;
                case '\r':
                    WriteUtf8SingleEscapedChar(ref destination, 'r', ref pos);
                    break;
                case '\t':
                    WriteUtf8SingleEscapedChar(ref destination, 't', ref pos);
                    break;
                case '\x0':
                    WriteUtf8DoubleEscapedChar(ref destination, '0', '0', ref pos);
                    break;
                case '\x1':
                    WriteUtf8DoubleEscapedChar(ref destination, '0', '1', ref pos);
                    break;
                case '\x2':
                    WriteUtf8DoubleEscapedChar(ref destination, '0', '2', ref pos);
                    break;
                case '\x3':
                    WriteUtf8DoubleEscapedChar(ref destination, '0', '3', ref pos);
                    break;
                case '\x4':
                    WriteUtf8DoubleEscapedChar(ref destination, '0', '4', ref pos);
                    break;
                case '\x5':
                    WriteUtf8DoubleEscapedChar(ref destination, '0', '5', ref pos);
                    break;
                case '\x6':
                    WriteUtf8DoubleEscapedChar(ref destination, '0', '6', ref pos);
                    break;
                case '\x7':
                    WriteUtf8DoubleEscapedChar(ref destination, '0', '7', ref pos);
                    break;
                case '\xB':
                    WriteUtf8DoubleEscapedChar(ref destination, '0', 'B', ref pos);
                    break;
                case '\xE':
                    WriteUtf8DoubleEscapedChar(ref destination, '0', 'E', ref pos);
                    break;
                case '\xF':
                    WriteUtf8DoubleEscapedChar(ref destination, '0', 'F', ref pos);
                    break;
                case '\x10':
                    WriteUtf8DoubleEscapedChar(ref destination, '1', '0', ref pos);
                    break;
                case '\x11':
                    WriteUtf8DoubleEscapedChar(ref destination, '1', '1', ref pos);
                    break;
                case '\x12':
                    WriteUtf8DoubleEscapedChar(ref destination, '1', '2', ref pos);
                    break;
                case '\x13':
                    WriteUtf8DoubleEscapedChar(ref destination, '1', '3', ref pos);
                    break;
                case '\x14':
                    WriteUtf8DoubleEscapedChar(ref destination, '1', '4', ref pos);
                    break;
                case '\x15':
                    WriteUtf8DoubleEscapedChar(ref destination, '1', '5', ref pos);
                    break;
                case '\x16':
                    WriteUtf8DoubleEscapedChar(ref destination, '1', '6', ref pos);
                    break;
                case '\x17':
                    WriteUtf8DoubleEscapedChar(ref destination, '1', '7', ref pos);
                    break;
                case '\x18':
                    WriteUtf8DoubleEscapedChar(ref destination, '1', '8', ref pos);
                    break;
                case '\x19':
                    WriteUtf8DoubleEscapedChar(ref destination, '1', '9', ref pos);
                    break;
                case '\x1A':
                    WriteUtf8DoubleEscapedChar(ref destination, '1', 'A', ref pos);
                    break;
                case '\x1B':
                    WriteUtf8DoubleEscapedChar(ref destination, '1', 'B', ref pos);
                    break;
                case '\x1C':
                    WriteUtf8DoubleEscapedChar(ref destination, '1', 'C', ref pos);
                    break;
                case '\x1D':
                    WriteUtf8DoubleEscapedChar(ref destination, '1', 'D', ref pos);
                    break;
                case '\x1E':
                    WriteUtf8DoubleEscapedChar(ref destination, '1', 'E', ref pos);
                    break;
                case '\x1F':
                    WriteUtf8DoubleEscapedChar(ref destination, '1', 'F', ref pos);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void WriteUtf8CharInternal(ref JsonWriter<TSymbol> writer, ref byte destination, char value, ref int pos)
        {
            if (value < 0x20 || value == JsonUtf8Constant.DoubleQuote || value == JsonUtf8Constant.Solidus || value == JsonUtf8Constant.ReverseSolidus)
            {
                WriteEscapedUtf8CharInternal(ref destination, value, ref pos);
            }
            else if (value < 0x80)
            {
                Unsafe.AddByteOffset(ref destination, (IntPtr)pos) = (byte)value;
                pos++;
            }
            else
            {
                fixed (byte* bytesPtr = &Unsafe.AddByteOffset(ref destination, (IntPtr)pos))
                {
                    pos += TextEncodings.UTF8.GetBytes(&value, 1, bytesPtr, writer.FreeCapacity);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Verbatim(byte[] value)
        {
            if (null == value) { return; }

            ref var pos = ref _pos;
            UnsafeMemory.WriteRaw(ref this, value, ref pos);
        }

        public void WriteUtf8Verbatim(in ReadOnlySpan<byte> value)
        {
            var count = value.Length;
            if (0u >= (uint)count) { return; }

            ref var pos = ref _pos;
            Ensure(pos, count);
            UnsafeMemory.WriteRaw(ref PinnableUtf8Address, ref MemoryMarshal.GetReference(value), count, ref pos);
        }

        public void WriteUtf8VerbatimNameSpan(in ReadOnlySpan<byte> value)
        {
            ref var pos = ref _pos;
            Ensure(pos, value.Length + 3);

            ref byte pinnableAddr = ref PinnableUtf8Address;

            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
            UnsafeMemory.WriteRaw(ref pinnableAddr, ref MemoryMarshal.GetReference(value), value.Length, ref pos);
            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
            Unsafe.AddByteOffset(ref pinnableAddr, (IntPtr)pos) = JsonUtf8Constant.NameSeparator;
            pos++;
        }

        /// <summary>The value should already be properly escaped</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Name(string value)
        {
            WriteUtf8Name(value.AsSpan());
        }

        /// <summary>The value should already be properly escaped</summary>
        public unsafe void WriteUtf8Name(in ReadOnlySpan<char> value)
        {
            ref var pos = ref _pos;
            var sLength = TextEncodings.Utf8.GetMaxByteCount(value.Length) + 3;
            Ensure(pos, sLength);

            ref byte pinnableAddr = ref PinnableUtf8Address;

            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
            fixed (char* charsPtr = &MemoryMarshal.GetReference(value))
            fixed (byte* bytesPtr = &Unsafe.AddByteOffset(ref pinnableAddr, (IntPtr)pos))
            {
                pos += TextEncodings.UTF8.GetBytes(charsPtr, value.Length, bytesPtr, FreeCapacity);
            }
            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
            Unsafe.AddByteOffset(ref pinnableAddr, (IntPtr)pos) = JsonUtf8Constant.NameSeparator;
            pos++;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteUtf8SingleEscapedChar(ref byte destination, char toEscape, ref int pos)
        {
            var offset = (IntPtr)pos;
            Unsafe.AddByteOffset(ref destination, offset + 1) = (byte)toEscape;
            Unsafe.AddByteOffset(ref destination, offset) = JsonUtf8Constant.ReverseSolidus;
            pos += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteUtf8DoubleEscapedChar(ref byte destination, char firstToEscape, char secondToEscape, ref int pos)
        {
            var offset = (IntPtr)pos;
            Unsafe.AddByteOffset(ref destination, offset + 5) = (byte)secondToEscape;
            Unsafe.AddByteOffset(ref destination, offset + 4) = (byte)firstToEscape;
            Unsafe.AddByteOffset(ref destination, offset + 3) = (byte)'0';
            Unsafe.AddByteOffset(ref destination, offset + 2) = (byte)'0';
            Unsafe.AddByteOffset(ref destination, offset + 1) = (byte)'u';
            Unsafe.AddByteOffset(ref destination, offset) = JsonUtf8Constant.ReverseSolidus;
            pos += 6;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8BeginObject()
        {
            ref var pos = ref _pos;
            Ensure(1);

            Unsafe.AddByteOffset(ref PinnableUtf8Address, (IntPtr)pos) = JsonUtf8Constant.BeginObject;
            pos++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8EndObject()
        {
            ref var pos = ref _pos;
            Ensure(1);

            Unsafe.AddByteOffset(ref PinnableUtf8Address, (IntPtr)pos) = JsonUtf8Constant.EndObject;
            pos++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8BeginArray()
        {
            ref var pos = ref _pos;
            Ensure(1);

            Unsafe.AddByteOffset(ref PinnableUtf8Address, (IntPtr)pos) = JsonUtf8Constant.BeginArray;
            pos++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8EndArray()
        {
            ref var pos = ref _pos;
            Ensure(1);

            Unsafe.AddByteOffset(ref PinnableUtf8Address, (IntPtr)pos) = JsonUtf8Constant.EndArray;
            pos++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8ValueSeparator()
        {
            ref var pos = ref _pos;
            Ensure(1);

            Unsafe.AddByteOffset(ref PinnableUtf8Address, (IntPtr)pos) = JsonUtf8Constant.ValueSeparator;
            pos++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Null()
        {
            WriteUtf8Verbatim(0x6C6C756E);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8NewLine()
        {
            const int newLineLength = 2;

            ref var pos = ref _pos;
            Ensure(pos, newLineLength);

            ref byte pinnableAddr = ref PinnableUtf8Address;
            var offset = (IntPtr)pos;
            Unsafe.AddByteOffset(ref pinnableAddr, offset) = JsonUtf8Constant.CarriageReturn;
            Unsafe.AddByteOffset(ref pinnableAddr, offset + 1) = JsonUtf8Constant.LineFeed;
            pos += newLineLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Indentation(int count)
        {
            ref var pos = ref _pos;
            Ensure(pos, count);

            ref byte pinnableAddr = ref Unsafe.AddByteOffset(ref PinnableUtf8Address, (IntPtr)pos);
            Unsafe.InitBlockUnaligned(ref pinnableAddr, JsonUtf8Constant.Space, unchecked((uint)count));
            pos += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8DoubleQuote()
        {
            ref var pos = ref _pos;
            Ensure(pos, 1);

            WriteUtf8DoubleQuote(ref PinnableUtf8Address, ref pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteUtf8DoubleQuote(ref byte destination, ref int pos)
        {
            Unsafe.AddByteOffset(ref destination, (IntPtr)pos) = JsonUtf8Constant.String;
            pos++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Version(Version value)
        {
#if NETSTANDARD2_0 || NET471 || NET451
            WriteUtf8String(value.ToString());
#else
            ref var pos = ref _pos;
            Ensure(JsonSharedConstant.MaxVersionLength);

            ref byte pinnableAddr = ref PinnableUtf8Address;

            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
            Span<char> tempSpan = TinyMemoryPool<char>.GetBuffer();
            var result = value.TryFormat(tempSpan, out var charsWritten);
            Debug.Assert(result);
            pos += Encoding.UTF8.GetBytes(tempSpan.Slice(0, charsWritten), Utf8Span);
            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Uri(Uri value)
        {
            WriteUtf8String(value.ToString()); // Uri does not implement ISpanFormattable
        }
    }
}
