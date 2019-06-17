namespace SpanJson
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using SpanJson.Internal;

    partial struct JsonWriter<TSymbol>
    {
        public void WriteUtf16String(in JsonEncodedText value)
        {
            ref var pos = ref _pos;
            var utf16Text = value.ToString();
            Ensure(pos, utf16Text.Length + 3);

            ref char pinnableAddr = ref Utf16PinnableAddress;
            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
            utf16Text.AsSpan().CopyTo(Utf16Span);
            pos += utf16Text.Length;
            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
        }

        public void WriteUtf16String(string value)
        {
            WriteUtf16StringEscapeValue(value.AsSpan(), false);
        }

        public void WriteUtf16String(in ReadOnlySpan<char> value)
        {
            WriteUtf16StringEscapeValue(value, false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16String(string value, StringEscapeHandling escapeHandling)
        {
            WriteUtf16String(value.AsSpan(), escapeHandling);
        }

        public void WriteUtf16String(in ReadOnlySpan<char> value, StringEscapeHandling escapeHandling)
        {
            switch (escapeHandling)
            {
                case StringEscapeHandling.EscapeNonAscii:
                    WriteUtf16StringEscapeNonAsciiValue(value, false);
                    break;

                case StringEscapeHandling.EscapeHtml:
                    WriteUtf16StringEscapeHtmlValue(value, false);
                    break;

                case StringEscapeHandling.Default:
                default:
                    WriteUtf16StringEscapeValue(value, false);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteUtf16StringEscapeValue(in ReadOnlySpan<char> value, bool withNameSeparator)
        {
            ref var pos = ref _pos;
            var valueLength = value.Length;
            uint nValueLength = (uint)valueLength;
            Ensure(pos, valueLength + 12); // assume that a fully escaped char fits too (5 * 2 + two double quotes)

            ref char pinnableAddr = ref Utf16PinnableAddress;

            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);

            var consumed = 0;
            ref char utf16Source = ref MemoryMarshal.GetReference(value);
            while ((uint)consumed < nValueLength)
            {
                char val = Unsafe.Add(ref utf16Source, consumed);
                if (EscapingHelper.Default.NeedsEscaping(val))
                {
                    EscapingHelper.EscapeNextChars(StringEscapeHandling.Default, ref utf16Source, nValueLength, val, ref pinnableAddr, ref consumed, ref pos);
                    var remaining = 10 + valueLength - consumed; // make sure that all characters and an extra 5 for a full escape still fit
                    if ((uint)remaining >= (uint)(_capacity - pos))
                    {
                        CheckAndResizeBuffer(pos, remaining);
                        pinnableAddr = ref Utf16PinnableAddress;
                    }
                }
                else
                {
                    Unsafe.Add(ref pinnableAddr, pos++) = val;
                }
                consumed++;
            }

            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);

            if (withNameSeparator) { Unsafe.Add(ref pinnableAddr, pos++) = JsonUtf16Constant.NameSeparator; }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void WriteUtf16StringEscapeHtmlValue(in ReadOnlySpan<char> value, bool withNameSeparator)
        {
            ref var pos = ref _pos;
            var valueLength = value.Length;
            uint nValueLength = (uint)valueLength;
            Ensure(pos, valueLength + 12); // assume that a fully escaped char fits too (5 * 2 + two double quotes)

            ref char pinnableAddr = ref Utf16PinnableAddress;

            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);

            var consumed = 0;
            ref char utf16Source = ref MemoryMarshal.GetReference(value);
            while ((uint)consumed < nValueLength)
            {
                char val = Unsafe.Add(ref utf16Source, consumed);
                if (EscapingHelper.Html.NeedsEscaping(val))
                {
                    EscapingHelper.EscapeNextChars(StringEscapeHandling.EscapeHtml, ref utf16Source, nValueLength, val, ref pinnableAddr, ref consumed, ref pos);
                    var remaining = 10 + valueLength - consumed; // make sure that all characters and an extra 5 for a full escape still fit
                    if ((uint)remaining >= (uint)(_capacity - pos))
                    {
                        CheckAndResizeBuffer(pos, remaining);
                        pinnableAddr = ref Utf16PinnableAddress;
                    }
                }
                else
                {
                    Unsafe.Add(ref pinnableAddr, pos++) = val;
                }
                consumed++;
            }

            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);

            if (withNameSeparator) { Unsafe.Add(ref pinnableAddr, pos++) = JsonUtf16Constant.NameSeparator; }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void WriteUtf16StringEscapeNonAsciiValue(in ReadOnlySpan<char> value, bool withNameSeparator)
        {
            ref var pos = ref _pos;
            var valueLength = value.Length;
            uint nValueLength = (uint)valueLength;
            Ensure(pos, valueLength + 12); // assume that a fully escaped char fits too (5 * 2 + two double quotes)

            ref char pinnableAddr = ref Utf16PinnableAddress;

            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);

            var consumed = 0;
            ref char utf16Source = ref MemoryMarshal.GetReference(value);
            while ((uint)consumed < nValueLength)
            {
                char val = Unsafe.Add(ref utf16Source, consumed);
                if (EscapingHelper.NonAscii.NeedsEscaping(val))
                {
                    EscapingHelper.EscapeNextChars(StringEscapeHandling.EscapeNonAscii, ref utf16Source, nValueLength, val, ref pinnableAddr, ref consumed, ref pos);
                    var remaining = 10 + valueLength - consumed; // make sure that all characters and an extra 5 for a full escape still fit
                    if ((uint)remaining >= (uint)(_capacity - pos))
                    {
                        CheckAndResizeBuffer(pos, remaining);
                        pinnableAddr = ref Utf16PinnableAddress;
                    }
                }
                else
                {
                    Unsafe.Add(ref pinnableAddr, pos++) = val;
                }
                consumed++;
            }

            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);

            if (withNameSeparator) { Unsafe.Add(ref pinnableAddr, pos++) = JsonUtf16Constant.NameSeparator; }
        }
    }
}
