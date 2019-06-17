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
        #region -- Signed Number --

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16SByte(sbyte value)
        {
            WriteUtf16Int64Internal(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16Int16(short value)
        {
            WriteUtf16Int64Internal(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16Int32(int value)
        {
            WriteUtf16Int64Internal(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16Int64(long value)
        {
            WriteUtf16Int64Internal(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteUtf16Int64Internal(long value)
        {
            if (value < 0)
            {
                ref var pos = ref _pos;
                Ensure(pos, 1);

                ref char pinnableAddr = ref Utf16PinnableAddress;
                Unsafe.Add(ref pinnableAddr, pos++) = '-';

                value = unchecked(-value);
            }

            WriteUtf16UInt64Internal((ulong)value);
        }

        #endregion

        #region -- Unsigned Number --

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteUtf16UInt64Internal(ulong value)
        {
            ref var pos = ref _pos;
            if (value < 10ul)
            {
                Ensure(pos, 1);

                ref char pinnableAddr0 = ref Utf16PinnableAddress;
                Unsafe.Add(ref pinnableAddr0, pos++) = (char)('0' + value);
                return;
            }

            var digits = FormatterUtils.CountDigits(value);

            Ensure(pos, digits);
            ref char pinnableAddr = ref Utf16PinnableAddress;

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
            WriteUtf16UInt64Internal(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16UInt16(ushort value)
        {
            WriteUtf16UInt64Internal(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16UInt32(uint value)
        {
            WriteUtf16UInt64Internal(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16UInt64(ulong value)
        {
            WriteUtf16UInt64Internal(value);
        }

        #endregion

        #region -- Float / Double --

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
            ref char pinnableAddr = ref Utf16PinnableAddress;
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
            ref char pinnableAddr = ref Utf16PinnableAddress;
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

        #endregion

        #region -- Decimal --

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

        #endregion

        #region -- Char --

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
            ref char pinnableAddr = ref Utf16PinnableAddress;

            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
            if (EscapingHelper.NeedsEscaping(value, escapeHandling))
            {
                EscapingHelper.EscapeChar(escapeHandling, ref pinnableAddr, value, ref pos);
            }
            else
            {
                Unsafe.Add(ref pinnableAddr, pos++) = value;
            }
            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
        }

        #endregion

        #region -- DateTime --

        public void WriteUtf16DateTime(DateTime value)
        {
            ref var pos = ref _pos;
            const int dtSize = JsonSharedConstant.MaxDateTimeLength; // Form o + two JsonUtf16Constant.DoubleQuote
            Ensure(pos, dtSize);
            ref char pinnableAddr = ref Utf16PinnableAddress;

            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
            DateTimeFormatter.TryFormat(value, Utf16Span, out var written);
            pos += written;
            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
        }

        #endregion

        #region -- DateTimeOffset --

        public void WriteUtf16DateTimeOffset(DateTimeOffset value)
        {
            ref var pos = ref _pos;
            const int dtSize = JsonSharedConstant.MaxDateTimeOffsetLength; // Form o + two JsonUtf16Constant.DoubleQuote
            Ensure(pos, dtSize);
            ref char pinnableAddr = ref Utf16PinnableAddress;

            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
            DateTimeFormatter.TryFormat(value, Utf16Span, out var written);
            pos += written;
            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
        }

        #endregion

        #region -- TimeSpan --

        public void WriteUtf16TimeSpan(TimeSpan value)
        {
            ref var pos = ref _pos;
            const int tsSize = JsonSharedConstant.MaxTimeSpanLength; // Form c + two JsonUtf16Constant.DoubleQuote
            Ensure(pos, tsSize);
            ref char pinnableAddr = ref Utf16PinnableAddress;

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

        #endregion

        #region -- Guid --

        public void WriteUtf16Guid(Guid value)
        {
            ref var pos = ref _pos;
            const int guidSize = JsonSharedConstant.MaxGuidLength; // Format D + two JsonUtf16Constant.DoubleQuote;
            Ensure(pos, guidSize);
            ref char pinnableAddr = ref Utf16PinnableAddress;

            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
            new GuidBits(ref value).Write(ref pinnableAddr, ref pos); // len = 36
            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
        }

        #endregion

        #region -- Version --

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16Version(Version value)
        {
            const int versionLength = JsonSharedConstant.MaxVersionLength;
            ref var pos = ref _pos;
            Ensure(pos, versionLength);

            ref char pinnableAddr = ref Utf16PinnableAddress;
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

        #endregion

        #region -- Uri --

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16Uri(Uri value)
        {
            WriteUtf16String(value.ToString()); // Uri does not implement ISpanFormattable
        }

        #endregion
    }
}
