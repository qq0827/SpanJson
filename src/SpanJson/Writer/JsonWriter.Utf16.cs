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
            Ensure(pos, JsonSharedConstant.MaximumFormatDoubleLength);
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
            Ensure(pos, JsonSharedConstant.MaximumFormatDoubleLength);
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
            Ensure(pos, JsonSharedConstant.MaximumFormatDecimalLength);
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

        private static void WriteEscapedUtf16CharInternal(ref char destination, char value, ref int pos)
        {
            switch (value)
            {
                case JsonUtf16Constant.DoubleQuote:
                    WriteUtf16SingleEscapedChar(ref destination, JsonUtf16Constant.DoubleQuote, ref pos);
                    break;
                case JsonUtf16Constant.Solidus:
                    WriteUtf16SingleEscapedChar(ref destination, JsonUtf16Constant.Solidus, ref pos);
                    break;
                case JsonUtf16Constant.ReverseSolidus:
                    WriteUtf16SingleEscapedChar(ref destination, JsonUtf16Constant.ReverseSolidus, ref pos);
                    break;
                case '\b':
                    WriteUtf16SingleEscapedChar(ref destination, 'b', ref pos);
                    break;
                case '\f':
                    WriteUtf16SingleEscapedChar(ref destination, 'f', ref pos);
                    break;
                case '\n':
                    WriteUtf16SingleEscapedChar(ref destination, 'n', ref pos);
                    break;
                case '\r':
                    WriteUtf16SingleEscapedChar(ref destination, 'r', ref pos);
                    break;
                case '\t':
                    WriteUtf16SingleEscapedChar(ref destination, 't', ref pos);
                    break;
                case '\x0':
                    WriteUtf16DoubleEscapedChar(ref destination, '0', '0', ref pos);
                    break;
                case '\x1':
                    WriteUtf16DoubleEscapedChar(ref destination, '0', '1', ref pos);
                    break;
                case '\x2':
                    WriteUtf16DoubleEscapedChar(ref destination, '0', '2', ref pos);
                    break;
                case '\x3':
                    WriteUtf16DoubleEscapedChar(ref destination, '0', '3', ref pos);
                    break;
                case '\x4':
                    WriteUtf16DoubleEscapedChar(ref destination, '0', '4', ref pos);
                    break;
                case '\x5':
                    WriteUtf16DoubleEscapedChar(ref destination, '0', '5', ref pos);
                    break;
                case '\x6':
                    WriteUtf16DoubleEscapedChar(ref destination, '0', '6', ref pos);
                    break;
                case '\x7':
                    WriteUtf16DoubleEscapedChar(ref destination, '0', '7', ref pos);
                    break;
                case '\xB':
                    WriteUtf16DoubleEscapedChar(ref destination, '0', 'B', ref pos);
                    break;
                case '\xE':
                    WriteUtf16DoubleEscapedChar(ref destination, '0', 'E', ref pos);
                    break;
                case '\xF':
                    WriteUtf16DoubleEscapedChar(ref destination, '0', 'F', ref pos);
                    break;
                case '\x10':
                    WriteUtf16DoubleEscapedChar(ref destination, '1', '0', ref pos);
                    break;
                case '\x11':
                    WriteUtf16DoubleEscapedChar(ref destination, '1', '1', ref pos);
                    break;
                case '\x12':
                    WriteUtf16DoubleEscapedChar(ref destination, '1', '2', ref pos);
                    break;
                case '\x13':
                    WriteUtf16DoubleEscapedChar(ref destination, '1', '3', ref pos);
                    break;
                case '\x14':
                    WriteUtf16DoubleEscapedChar(ref destination, '1', '4', ref pos);
                    break;
                case '\x15':
                    WriteUtf16DoubleEscapedChar(ref destination, '1', '5', ref pos);
                    break;
                case '\x16':
                    WriteUtf16DoubleEscapedChar(ref destination, '1', '6', ref pos);
                    break;
                case '\x17':
                    WriteUtf16DoubleEscapedChar(ref destination, '1', '7', ref pos);
                    break;
                case '\x18':
                    WriteUtf16DoubleEscapedChar(ref destination, '1', '8', ref pos);
                    break;
                case '\x19':
                    WriteUtf16DoubleEscapedChar(ref destination, '1', '9', ref pos);
                    break;
                case '\x1A':
                    WriteUtf16DoubleEscapedChar(ref destination, '1', 'A', ref pos);
                    break;
                case '\x1B':
                    WriteUtf16DoubleEscapedChar(ref destination, '1', 'B', ref pos);
                    break;
                case '\x1C':
                    WriteUtf16DoubleEscapedChar(ref destination, '1', 'C', ref pos);
                    break;
                case '\x1D':
                    WriteUtf16DoubleEscapedChar(ref destination, '1', 'D', ref pos);
                    break;
                case '\x1E':
                    WriteUtf16DoubleEscapedChar(ref destination, '1', 'E', ref pos);
                    break;
                case '\x1F':
                    WriteUtf16DoubleEscapedChar(ref destination, '1', 'F', ref pos);
                    break;
            }
        }

        public void WriteUtf16Char(char value)
        {
            ref var pos = ref _pos;
            const int size = 8; // 1-6 chars + two JsonUtf16Constant.DoubleQuote
            Ensure(pos, size);
            ref char pinnableAddr = ref PinnableUtf16Address;

            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
            if (value < 0x20 || value == JsonUtf16Constant.DoubleQuote || value == JsonUtf16Constant.Solidus || value == JsonUtf16Constant.ReverseSolidus)
            {
                WriteEscapedUtf16CharInternal(ref pinnableAddr, value, ref pos);
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
            WriteUtf16String(value.AsSpan());
        }

        public void WriteUtf16String(in ReadOnlySpan<char> value)
        {
            ref var pos = ref _pos;
            var valueLength = value.Length;
            var sLength = valueLength + 7; // assume that a fully escaped char fits too (5 + two double quotes)
            Ensure(pos, sLength);
            ref char pinnableAddr = ref PinnableUtf16Address;

            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
            ref char utf16Source = ref MemoryMarshal.GetReference(value);
            for (var i = 0; i < valueLength; i++)
            {
                ref readonly var c = ref Unsafe.Add(ref utf16Source, i);
                if (c < 0x20 || c == JsonUtf16Constant.DoubleQuote || c == JsonUtf16Constant.Solidus || c == JsonUtf16Constant.ReverseSolidus)
                {
                    WriteEscapedUtf16CharInternal(ref pinnableAddr, c, ref pos);
                    var remaining = 5 + valueLength - i; // make sure that all characters and an extra 5 for a full escape still fit
                    Ensure(pos, remaining);
                    pinnableAddr = ref PinnableUtf16Address;
                }
                else
                {
                    Unsafe.Add(ref pinnableAddr, pos++) = c;
                }
            }

            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
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

        public void WriteUtf16VerbatimNameSpan(in ReadOnlySpan<char> value)
        {
            WriteUtf16Name(value);
        }

        /// <summary>The value should already be properly escaped.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16Name(string value)
        {
            WriteUtf16Name(value.AsSpan());
        }

        /// <summary>The value should already be properly escaped.</summary>
        public void WriteUtf16Name(in ReadOnlySpan<char> value)
        {
            ref var pos = ref _pos;
            Ensure(pos, value.Length + 3);

            ref char pinnableAddr = ref PinnableUtf16Address;
            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
            value.CopyTo(Utf16Span);
            pos += value.Length;
            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
            Unsafe.Add(ref pinnableAddr, pos++) = JsonUtf16Constant.NameSeparator;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteUtf16SingleEscapedChar(ref char destination, char toEscape, ref int pos)
        {
            Unsafe.Add(ref destination, pos + 1) = toEscape;
            Unsafe.Add(ref destination, pos) = JsonUtf16Constant.ReverseSolidus;
            pos += 2;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteUtf16DoubleEscapedChar(ref char destination, char firstToEscape, char secondToEscape, ref int pos)
        {
            Unsafe.Add(ref destination, pos + 5) = secondToEscape;
            Unsafe.Add(ref destination, pos + 4) = firstToEscape;
            Unsafe.Add(ref destination, pos + 3) = '0';
            Unsafe.Add(ref destination, pos + 2) = '0';
            Unsafe.Add(ref destination, pos + 1) = 'u';
            Unsafe.Add(ref destination, pos) = JsonUtf16Constant.ReverseSolidus;
            pos += 6;
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
