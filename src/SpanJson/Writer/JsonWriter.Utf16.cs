namespace SpanJson
{
    using System;
    using System.Buffers;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using SpanJson.Internal;
    using System.Buffers.Text;
    using System.Runtime.InteropServices;
    using SpanJson.Helpers;
#if NETSTANDARD2_0 || NET471 || NET451
    using SpanJson.Internal.DoubleConversion;
#endif

    partial struct JsonWriter<TSymbol>
    {
        public override string ToString()
        {
            var s = new ReadOnlySpan<char>(_utf16Buffer, 0, _pos).ToString();
            Dispose();
            return s;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16SByte(sbyte value)
        {
            WriteUtf16Int64Internal(ref this, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16Int16(short value)
        {
            WriteUtf16Int64Internal(ref this, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16Int32(int value)
        {
            WriteUtf16Int64Internal(ref this, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16Int64(long value)
        {
            WriteUtf16Int64Internal(ref this, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteUtf16Int64Internal(ref JsonWriter<TSymbol> writer, long value)
        {
            if (value < 0)
            {
                ref var pos = ref writer._pos;
                writer.Ensure(pos, 1);

                ref char pinnableAddr = ref writer.PinnableUtf16Address;
                Unsafe.Add(ref pinnableAddr, pos++) = '-';

                value = unchecked(-value);
            }

            WriteUtf16UInt64Internal(ref writer, (ulong)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteUtf16UInt64Internal(ref JsonWriter<TSymbol> writer, ulong value)
        {
            ref var pos = ref writer._pos;
            if (value < 10ul)
            {
                writer.Ensure(pos, 1);

                ref char pinnableAddr0 = ref writer.PinnableUtf16Address;
                Unsafe.Add(ref pinnableAddr0, pos++) = (char)('0' + value);
                return;
            }

            var digits = FormatterUtils.CountDigits(value);

            writer.Ensure(pos, digits);
            ref char pinnableAddr = ref writer.PinnableUtf16Address;

            for (var i = digits - 1; i >= 0; i--)
            {
                var temp = '0' + value;
                value /= 10ul;
                Unsafe.Add(ref pinnableAddr, pos + i) = (char)(temp - value * 10ul);
            }

            pos += digits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16Byte(byte value)
        {
            WriteUtf16UInt64Internal(ref this, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16UInt16(ushort value)
        {
            WriteUtf16UInt64Internal(ref this, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16UInt32(uint value)
        {
            WriteUtf16UInt64Internal(ref this, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16UInt64(ulong value)
        {
            WriteUtf16UInt64Internal(ref this, value);
        }

        public void WriteUtf16Single(float value)
        {
#if NETSTANDARD2_0 || NET471 || NET451
            if (float.IsNaN(value) || float.IsInfinity(value))
#else
            if (!float.IsFinite(value))
#endif
            {
                ThrowHelper.ThrowArgumentException_InvalidFloatValueForJson();
            }

            ref var pos = ref _pos;
#if NETSTANDARD2_0 || NET471 || NET451
            var buffer = TinyMemoryPool<byte>.GetBuffer();
            var count = DoubleToStringConverter.GetBytes(ref buffer, 0, value);
            Ensure(pos, count);
            ref char pinnableAddr = ref PinnableUtf16Address;
            ref byte utf8Source = ref buffer[0];
            var offset = (IntPtr)0;
            for (int i = 0; i < count; i++)
            {
                Unsafe.Add(ref pinnableAddr, pos + i) = (char)Unsafe.AddByteOffset(ref utf8Source, offset + i);
            }
            pos += count;
#else
            Ensure(pos, JsonConstants.MaximumFormatDoubleLength);
            var result = value.TryFormat(Utf16Span, out var written, provider: CultureInfo.InvariantCulture);
            if (result)
            {
                pos += written;
                return;
            }
            WriteFloatingPoint(ref this, value, ref pos);
#endif
        }

        public void WriteUtf16Double(double value)
        {
#if NETSTANDARD2_0 || NET471 || NET451
            if (double.IsNaN(value) || double.IsInfinity(value))
#else
            if (!double.IsFinite(value))
#endif
            {
                ThrowHelper.ThrowArgumentException_InvalidDoubleValueForJson();
            }

            ref var pos = ref _pos;
#if NETSTANDARD2_0 || NET471 || NET451
            var buffer = TinyMemoryPool<byte>.GetBuffer();
            var count = DoubleToStringConverter.GetBytes(ref buffer, 0, value);
            Ensure(pos, count);
            ref char pinnableAddr = ref PinnableUtf16Address;
            ref byte utf8Source = ref buffer[0];
            var offset = (IntPtr)0;
            for (int i = 0; i < count; i++)
            {
                Unsafe.Add(ref pinnableAddr, pos + i) = (char)Unsafe.AddByteOffset(ref utf8Source, offset + i);
            }
            pos += count;
#else
            Ensure(pos, JsonConstants.MaximumFormatDoubleLength);
            var result = value.TryFormat(Utf16Span, out var written, provider: CultureInfo.InvariantCulture);
            if (result)
            {
                pos += written;
                return;
            }
            WriteFloatingPoint(ref this, value, ref pos);
#endif
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void WriteFloatingPoint<T>(ref JsonWriter<TSymbol> writer, T value, ref int pos) where T : IFormattable
        {
            StandardFormat format = 'G';
            string formatString = format.ToString();
            string utf16Text = value.ToString(formatString, CultureInfo.InvariantCulture);

            int length = utf16Text.Length;
            writer.Ensure(pos, length);
            utf16Text.AsSpan().CopyTo(writer.Utf16Span);
            pos += length;
        }

        public void WriteUtf16Decimal(decimal value)
        {
            ref var pos = ref _pos;
#if NETSTANDARD2_0 || NET471 || NET451
            var utf16Text = value.ToString("G", CultureInfo.InvariantCulture);
            var written = utf16Text.Length;
            Ensure(pos, written);
            utf16Text.AsSpan().CopyTo(Utf16Span);
#else
            Ensure(pos, JsonConstants.MaximumFormatDecimalLength);
            var result = value.TryFormat(Utf16Span, out var written, provider: CultureInfo.InvariantCulture);
            Debug.Assert(result);
#endif
            pos += written;
        }

        public void WriteUtf16Boolean(bool value)
        {
            const int trueLength = 4;
            const int falseLength = 5;

            ref var pos = ref _pos;
            Ensure(pos, falseLength);

            ref char pinnableAddr = ref PinnableUtf16Address;
            if (value)
            {
                Unsafe.Add(ref pinnableAddr, pos + 3) = 'e';
                Unsafe.Add(ref pinnableAddr, pos + 2) = 'u';
                Unsafe.Add(ref pinnableAddr, pos + 1) = 'r';
                Unsafe.Add(ref pinnableAddr, pos) = JsonUtf16Constant.True;
                pos += trueLength;
            }
            else
            {
                Unsafe.Add(ref pinnableAddr, pos + 4) = 'e';
                Unsafe.Add(ref pinnableAddr, pos + 3) = 's';
                Unsafe.Add(ref pinnableAddr, pos + 2) = 'l';
                Unsafe.Add(ref pinnableAddr, pos + 1) = 'a';
                Unsafe.Add(ref pinnableAddr, pos) = JsonUtf16Constant.False;
                pos += falseLength;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16Char(char value)
        {
            WriteUtf16Char(value, StringEscapeHandling.Default);
        }

        public void WriteUtf16Char(char value, StringEscapeHandling escapeHandling)
        {
            ref var pos = ref _pos;
            const int size = 8; // 1-6 chars + two JsonUtf16Constant.DoubleQuote
            Ensure(pos, size);
            ref char pinnableAddr = ref PinnableUtf16Address;

            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
            if (EscapingHelper.NeedsEscaping(value, escapeHandling))
            {
                EscapingHelper.EscapeChar(value, ref pinnableAddr, ref pos);
            }
            else
            {
                Unsafe.Add(ref pinnableAddr, pos++) = value;
            }
            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
        }

        public void WriteUtf16DateTime(DateTime value)
        {
            ref var pos = ref _pos;
            const int dtSize = JsonSharedConstant.MaxDateTimeLength; // Form o + two JsonUtf16Constant.DoubleQuote
            Ensure(pos, dtSize);
            ref char pinnableAddr = ref PinnableUtf16Address;

            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
            DateTimeFormatter.TryFormat(value, Utf16Span, out var written);
            pos += written;
            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
        }

        public void WriteUtf16DateTimeOffset(DateTimeOffset value)
        {
            ref var pos = ref _pos;
            const int dtSize = JsonSharedConstant.MaxDateTimeOffsetLength; // Form o + two JsonUtf16Constant.DoubleQuote
            Ensure(pos, dtSize);
            ref char pinnableAddr = ref PinnableUtf16Address;

            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
            DateTimeFormatter.TryFormat(value, Utf16Span, out var written);
            pos += written;
            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
        }

        public void WriteUtf16TimeSpan(TimeSpan value)
        {
            ref var pos = ref _pos;
            const int tsSize = JsonSharedConstant.MaxTimeSpanLength; // Form c + two JsonUtf16Constant.DoubleQuote
            Ensure(pos, tsSize);
            ref char pinnableAddr = ref PinnableUtf16Address;

            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
            Span<byte> byteSpan = stackalloc byte[tsSize];
            var result = Utf8Formatter.TryFormat(value, byteSpan, out var bytesWritten);
            Debug.Assert(result);
            ref byte utf8Source = ref MemoryMarshal.GetReference(byteSpan);
            var offset = (IntPtr)0;
            for (var i = 0; i < bytesWritten; i++)
            {
                Unsafe.Add(ref pinnableAddr, pos + i) = (char)Unsafe.AddByteOffset(ref utf8Source, offset + i);
            }

            pos += bytesWritten;
            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
        }

        public void WriteUtf16Guid(Guid value)
        {
            ref var pos = ref _pos;
            const int guidSize = JsonSharedConstant.MaxGuidLength; // Format D + two JsonUtf16Constant.DoubleQuote;
            Ensure(pos, guidSize);
            ref char pinnableAddr = ref PinnableUtf16Address;

            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
            new GuidBits(ref value).Write(ref pinnableAddr, ref pos); // len = 36
            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16String(string value)
        {
            WriteUtf16String(value.AsSpan(), StringEscapeHandling.Default);
        }

        public void WriteUtf16String(in ReadOnlySpan<char> value)
        {
            WriteUtf16String(value, StringEscapeHandling.Default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16String(string value, StringEscapeHandling escapeHandling)
        {
            WriteUtf16String(value.AsSpan(), escapeHandling);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16String(in ReadOnlySpan<char> value, StringEscapeHandling escapeHandling)
        {
            var firstEscapeIndex = EscapingHelper.NeedsEscaping(value, escapeHandling);
            if ((uint)firstEscapeIndex > JsonSharedConstant.TooBigOrNegative) // -1
            {
                WriteUtf16StringEscapedValue(ref this, value, false);
            }
            else
            {
                WriteUtf16StringEscapeValue(ref this, value, escapeHandling, firstEscapeIndex, false);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16Verbatim(string value)
        {
            WriteUtf16Verbatim(value.AsSpan());
        }

        public void WriteUtf16Verbatim(in ReadOnlySpan<char> value)
        {
            ref var pos = ref _pos;
            Ensure(pos, value.Length);

            value.CopyTo(Utf16Span);
            pos += value.Length;
        }

        /// <summary>The value should already be properly escaped.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16VerbatimNameSpan(in ReadOnlySpan<char> value)
        {
            WriteUtf16StringEscapedValue(ref this, value, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16VerbatimNameSpan(in ReadOnlySpan<char> value, StringEscapeHandling escapeHandling)
        {
            WriteUtf16Name(value, escapeHandling);
        }

        /// <summary>The value should already be properly escaped.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16Name(string value)
        {
            WriteUtf16StringEscapedValue(ref this, value.AsSpan(), true);
        }

        /// <summary>The value should already be properly escaped.</summary>
        public void WriteUtf16Name(in ReadOnlySpan<char> value)
        {
            WriteUtf16StringEscapedValue(ref this, value, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16Name(string value, StringEscapeHandling escapeHandling)
        {
            WriteUtf16Name(value.AsSpan(), escapeHandling);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16Name(in ReadOnlySpan<char> value, StringEscapeHandling escapeHandling)
        {
            var firstEscapeIndex = EscapingHelper.NeedsEscaping(value, escapeHandling);
            if ((uint)firstEscapeIndex > JsonSharedConstant.TooBigOrNegative) // -1
            {
                WriteUtf16StringEscapedValue(ref this, value, true);
            }
            else
            {
                WriteUtf16StringEscapeValue(ref this, value, escapeHandling, firstEscapeIndex, true);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void WriteUtf16StringEscapeValue(ref JsonWriter<TSymbol> writer, in ReadOnlySpan<char> value,
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

            WriteUtf16StringEscapedValue(ref writer, escapedPropertyName.Slice(0, written), withNameSeparator);

            if (propertyArray != null)
            {
                ArrayPool<char>.Shared.Return(propertyArray);
            }
        }

        private static void WriteUtf16StringEscapedValue(ref JsonWriter<TSymbol> writer, in ReadOnlySpan<char> value, bool withNameSeparator)
        {
            ref var pos = ref writer._pos;
            writer.Ensure(pos, value.Length + 3);

            ref char pinnableAddr = ref writer.PinnableUtf16Address;
            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
            value.CopyTo(writer.Utf16Span);
            pos += value.Length;
            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
            if (withNameSeparator) { Unsafe.Add(ref pinnableAddr, pos++) = JsonUtf16Constant.NameSeparator; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16BeginObject()
        {
            ref var pos = ref _pos;
            Ensure(pos, 1);

            Unsafe.Add(ref PinnableUtf16Address, pos++) = JsonUtf16Constant.BeginObject;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16EndObject()
        {
            ref var pos = ref _pos;
            Ensure(pos, 1);

            Unsafe.Add(ref PinnableUtf16Address, pos++) = JsonUtf16Constant.EndObject;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16BeginArray()
        {
            ref var pos = ref _pos;
            Ensure(pos, 1);

            Unsafe.Add(ref PinnableUtf16Address, pos++) = JsonUtf16Constant.BeginArray;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16EndArray()
        {
            ref var pos = ref _pos;
            Ensure(pos, 1);

            Unsafe.Add(ref PinnableUtf16Address, pos++) = JsonUtf16Constant.EndArray;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16ValueSeparator()
        {
            ref var pos = ref _pos;
            Ensure(pos, 1);

            Unsafe.Add(ref PinnableUtf16Address, pos++) = JsonUtf16Constant.ValueSeparator;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16Null()
        {
            const int nullLength = 4;

            ref var pos = ref _pos;
            Ensure(pos, nullLength);

            ref char pinnableAddr = ref PinnableUtf16Address;
            Unsafe.Add(ref pinnableAddr, pos + 3) = 'l';
            Unsafe.Add(ref pinnableAddr, pos + 2) = 'l';
            Unsafe.Add(ref pinnableAddr, pos + 1) = 'u';
            Unsafe.Add(ref pinnableAddr, pos) = JsonUtf16Constant.Null;
            pos += nullLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16NewLine()
        {
            const int newLineLength = 2;

            ref var pos = ref _pos;
            Ensure(pos, newLineLength);

            ref char pinnableAddr = ref PinnableUtf16Address;
            Unsafe.Add(ref pinnableAddr, pos) = JsonUtf16Constant.CarriageReturn;
            Unsafe.Add(ref pinnableAddr, pos + 1) = JsonUtf16Constant.LineFeed;
            pos += newLineLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16Indentation(int count)
        {
            ref var pos = ref _pos;
            Ensure(pos, count);

            ref char pinnableAddr = ref Unsafe.Add(ref PinnableUtf16Address, pos);
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
            Ensure(pos, 1);

            WriteUtf16DoubleQuote(ref PinnableUtf16Address, ref pos);
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
            Ensure(pos, 1);

            WriteUtf16NameSeparator(ref PinnableUtf16Address, ref pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteUtf16NameSeparator(ref char destination, ref int pos)
        {
            Unsafe.Add(ref destination, pos++) = JsonUtf16Constant.NameSeparator;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16Version(Version value)
        {
            const int versionLength = JsonSharedConstant.MaxVersionLength;
            ref var pos = ref _pos;
            Ensure(pos, versionLength);

            ref char pinnableAddr = ref PinnableUtf16Address;
            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
#if NETSTANDARD2_0 || NET471 || NET451
            var utf16Text = value.ToString();
            utf16Text.AsSpan().CopyTo(Utf16Span);
            pos += utf16Text.Length;
#else
            value.TryFormat(Utf16Span, out var written);
            pos += written;
#endif
            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16Uri(Uri value)
        {
            WriteUtf16String(value.ToString()); // Uri does not implement ISpanFormattable
        }
    }
}
