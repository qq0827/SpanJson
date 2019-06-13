namespace SpanJson
{
    using System;
    using System.Buffers;
    using System.Buffers.Text;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
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
                Unsafe.AddByteOffset(ref pinnableAddr, (IntPtr)pos++) = (byte)'-';

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

                Unsafe.AddByteOffset(ref writer.PinnableUtf8Address, (IntPtr)pos++) = (byte)('0' + value);
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
            UnsafeMemory.WriteRawUnsafe(ref PinnableUtf8Address, ref buffer[0], count, ref pos);
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
            UnsafeMemory.WriteRawUnsafe(ref PinnableUtf8Address, ref buffer[0], count, ref pos);
        }

        public void WriteUtf8Decimal(decimal value)
        {
            ref var pos = ref _pos;
            Ensure(pos, JsonConstants.MaximumFormatDecimalLength);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Char(char value)
        {
            WriteUtf8Char(value, StringEscapeHandling.Default);
        }

        public void WriteUtf8Char(char value, StringEscapeHandling escapeHandling)
        {
            ref var pos = ref _pos;
            const int size = 8; // 1-6 chars + two JsonUtf8Constant.DoubleQuote
            Ensure(pos, size);

            ref byte pinnableAddr = ref PinnableUtf8Address;

            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
            if (EscapingHelper.NeedsEscaping(value, escapeHandling))
            {
                EscapingHelper.EscapeChar(value, ref pinnableAddr, ref pos);
            }
            else
            {
                unsafe
                {
                    fixed (byte* bytesPtr = &Unsafe.AddByteOffset(ref pinnableAddr, (IntPtr)pos))
                    {
                        pos += TextEncodings.UTF8NoBOM.GetBytes(&value, 1, bytesPtr, FreeCapacity);
                    }
                }
            }
            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
        }

        public void WriteUtf8DateTime(DateTime value)
        {
            ref var pos = ref _pos;
            const int dtSize = JsonSharedConstant.MaxDateTimeLength; // Form o + two JsonUtf8Constant.DoubleQuote
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
            const int dtSize = JsonSharedConstant.MaxDateTimeOffsetLength; // Form o + two JsonUtf8Constant.DoubleQuote
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
            const int tsSize = JsonSharedConstant.MaxTimeSpanLength; // Form o + two JsonUtf8Constant.DoubleQuote
            Ensure(pos, tsSize);

            ref byte pinnableAddr = ref PinnableUtf8Address;

            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
            Utf8Formatter.TryFormat(value, Utf8Span, out var bytesWritten);
            pos += bytesWritten;
            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
        }

        public void WriteUtf8Guid(Guid value)
        {
            ref var pos = ref _pos;
            const int guidSize = JsonSharedConstant.MaxGuidLength; // Format D + two JsonUtf8Constant.DoubleQuote;
            Ensure(pos, guidSize);

            ref byte pinnableAddr = ref PinnableUtf8Address;

            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
            new GuidBits(ref value).Write(ref pinnableAddr, ref pos); // len = 36
            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8String(string value)
        {
            WriteUtf8String(value.AsSpan(), StringEscapeHandling.Default);
        }

        /// <summary>We know that for a pure ascii string all characters will fit if there are no escapes
        /// We make sure that initially the buffer is large enough and an additional fully escaped char fits too
        /// After each escape we make sure that all remaining ascii chars and extra fully escaped char fit
        /// For UTF8 encoded bytes we make sure that the 5 for the fully escaped value and 4 for the utf fit
        /// That's all done to make sure we don't have resizing in the fast path (the ascii case).</summary>
        public void WriteUtf8String(in ReadOnlySpan<char> value)
        {
            WriteUtf8String(value, StringEscapeHandling.Default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8String(string value, StringEscapeHandling escapeHandling)
        {
            WriteUtf8String(value.AsSpan(), escapeHandling);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8String(in ReadOnlySpan<char> value, StringEscapeHandling escapeHandling)
        {
            var firstEscapeIndex = EscapingHelper.NeedsEscaping(value, escapeHandling);
            if ((uint)firstEscapeIndex > JsonSharedConstant.TooBigOrNegative) // -1
            {
                WriteUtf8StringEscapedValue(ref this, value, false);
            }
            else
            {
                WriteUtf8StringEscapeValue(ref this, value, escapeHandling, firstEscapeIndex, false);
            }
        }

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

        /// <summary>The value should already be properly escaped.</summary>
        public void WriteUtf8VerbatimNameSpan(in ReadOnlySpan<byte> value)
        {
            ref var pos = ref _pos;
            Ensure(pos, value.Length + 3);

            ref byte pinnableAddr = ref PinnableUtf8Address;

            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
            UnsafeMemory.WriteRawUnsafe(ref pinnableAddr, ref MemoryMarshal.GetReference(value), value.Length, ref pos);
            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
            Unsafe.AddByteOffset(ref pinnableAddr, (IntPtr)pos++) = JsonUtf8Constant.NameSeparator;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8VerbatimNameSpan(in ReadOnlySpan<byte> value, StringEscapeHandling escapeHandling)
        {
            var firstEscapeIndex = EscapingHelper.NeedsEscaping(value, escapeHandling);
            if ((uint)firstEscapeIndex > JsonSharedConstant.TooBigOrNegative) // -1
            {
                WriteUtf8VerbatimNameSpan(value);
            }
            else
            {
                WriteUtf8EscapeVerbatimNameSpan(value, escapeHandling, firstEscapeIndex);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void WriteUtf8EscapeVerbatimNameSpan(in ReadOnlySpan<byte> value, StringEscapeHandling escapeHandling, int firstEscapeIndex)
        {

            byte[] propertyArray = null;

            int length = EscapingHelper.GetMaxEscapedLength(value.Length, firstEscapeIndex);
            Span<byte> escapedPropertyName;
            if ((uint)length > c_stackallocThreshold)
            {
                propertyArray = ArrayPool<byte>.Shared.Rent(length);
                escapedPropertyName = propertyArray;
            }
            else
            {
                // Cannot create a span directly since it gets passed to instance methods on a ref struct.
                unsafe
                {
                    byte* ptr = stackalloc byte[length];
                    escapedPropertyName = new Span<byte>(ptr, length);
                }
            }
            EscapingHelper.EscapeString(value, escapedPropertyName, escapeHandling, firstEscapeIndex, out int written);

            WriteUtf8VerbatimNameSpan(escapedPropertyName.Slice(0, written));

            if (propertyArray != null)
            {
                ArrayPool<byte>.Shared.Return(propertyArray);
            }
        }

        /// <summary>The value should already be properly escaped.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Name(string value)
        {
            WriteUtf8StringEscapedValue(ref this, value.AsSpan(), true);
        }

        /// <summary>The value should already be properly escaped.</summary>
        public void WriteUtf8Name(in ReadOnlySpan<char> value)
        {
            WriteUtf8StringEscapedValue(ref this, value, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Name(string value, StringEscapeHandling escapeHandling)
        {
            WriteUtf8Name(value.AsSpan(), escapeHandling);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Name(in ReadOnlySpan<char> value, StringEscapeHandling escapeHandling)
        {
            var firstEscapeIndex = EscapingHelper.NeedsEscaping(value, escapeHandling);
            if ((uint)firstEscapeIndex > JsonSharedConstant.TooBigOrNegative) // -1
            {
                WriteUtf8StringEscapedValue(ref this, value, true);
            }
            else
            {
                WriteUtf8StringEscapeValue(ref this, value, escapeHandling, firstEscapeIndex, true);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void WriteUtf8StringEscapeValue(ref JsonWriter<TSymbol> writer, in ReadOnlySpan<char> value,
            StringEscapeHandling escapeHandling, int firstEscapeIndex, bool withNameSeparator)
        {
            char[] propertyArray = null;

            int length = EscapingHelper.GetMaxEscapedLength(value.Length, firstEscapeIndex);
            Span<char> escapedPropertyName;
            if ((uint)length > c_stackallocThreshold)
            {
                propertyArray = ArrayPool<char>.Shared.Rent(length);
                escapedPropertyName = propertyArray;
            }
            else
            {
                // Cannot create a span directly since it gets passed to instance methods on a ref struct.
                unsafe
                {
                    char* ptr = stackalloc char[length];
                    escapedPropertyName = new Span<char>(ptr, length);
                }
            }
            EscapingHelper.EscapeString(value, escapedPropertyName, escapeHandling, firstEscapeIndex, out int written);

            WriteUtf8StringEscapedValue(ref writer, escapedPropertyName.Slice(0, written), withNameSeparator);

            if (propertyArray != null)
            {
                ArrayPool<char>.Shared.Return(propertyArray);
            }
        }

        private static void WriteUtf8StringEscapedValue(ref JsonWriter<TSymbol> writer, in ReadOnlySpan<char> value, bool withNameSeparator)
        {
            ref var pos = ref writer._pos;
            var sLength = TextEncodings.UTF8NoBOM.GetMaxByteCount(value.Length) + 3;
            writer.Ensure(pos, sLength);

            ref byte pinnableAddr = ref writer.PinnableUtf8Address;

            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
            pos += TextEncodings.Utf8.GetBytes(value, writer.Utf8Span);
            WriteUtf8DoubleQuote(ref pinnableAddr, ref pos);
            if (withNameSeparator) { Unsafe.AddByteOffset(ref pinnableAddr, (IntPtr)pos++) = JsonUtf8Constant.NameSeparator; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8BeginObject()
        {
            ref var pos = ref _pos;
            Ensure(1);

            Unsafe.AddByteOffset(ref PinnableUtf8Address, (IntPtr)pos++) = JsonUtf8Constant.BeginObject;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8EndObject()
        {
            ref var pos = ref _pos;
            Ensure(1);

            Unsafe.AddByteOffset(ref PinnableUtf8Address, (IntPtr)pos++) = JsonUtf8Constant.EndObject;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8BeginArray()
        {
            ref var pos = ref _pos;
            Ensure(1);

            Unsafe.AddByteOffset(ref PinnableUtf8Address, (IntPtr)pos++) = JsonUtf8Constant.BeginArray;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8EndArray()
        {
            ref var pos = ref _pos;
            Ensure(1);

            Unsafe.AddByteOffset(ref PinnableUtf8Address, (IntPtr)pos++) = JsonUtf8Constant.EndArray;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8ValueSeparator()
        {
            ref var pos = ref _pos;
            Ensure(1);

            Unsafe.AddByteOffset(ref PinnableUtf8Address, (IntPtr)pos++) = JsonUtf8Constant.ValueSeparator;
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
        public void WriteUtf8NameSeparator()
        {
            ref var pos = ref _pos;
            Ensure(pos, 1);

            WriteUtf8NameSeparator(ref PinnableUtf8Address, ref pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteUtf8NameSeparator(ref byte destination, ref int pos)
        {
            Unsafe.AddByteOffset(ref destination, (IntPtr)pos++) = JsonUtf8Constant.NameSeparator;
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
            pos += TextEncodings.UTF8NoBOM.GetBytes(tempSpan.Slice(0, charsWritten), Utf8Span);
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
