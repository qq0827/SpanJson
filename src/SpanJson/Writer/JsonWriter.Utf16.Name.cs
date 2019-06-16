namespace SpanJson
{
    using System;
    using System.Runtime.CompilerServices;

    partial struct JsonWriter<TSymbol>
    {
        /// <summary>The value should already be properly escaped.</summary>
        public void WriteUtf16Name(in JsonEncodedText value)
        {
            ref var pos = ref _pos;
            var utf16Text = value.ToString();
            Ensure(pos, utf16Text.Length + 3);

            ref char pinnableAddr = ref PinnableUtf16Address;
            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);
            utf16Text.AsSpan().CopyTo(Utf16Span);
            pos += utf16Text.Length;
            WriteUtf16DoubleQuote(ref pinnableAddr, ref pos);

            Unsafe.Add(ref pinnableAddr, pos++) = JsonUtf16Constant.NameSeparator;
        }

        public void WriteUtf16Name(string value)
        {
            WriteUtf16StringEscapeValue(value.AsSpan(), true);
        }

        public void WriteUtf16Name(in ReadOnlySpan<char> value)
        {
            WriteUtf16StringEscapeValue(value, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16Name(string value, StringEscapeHandling escapeHandling)
        {
            WriteUtf16Name(value.AsSpan(), escapeHandling);
        }

        public void WriteUtf16Name(in ReadOnlySpan<char> value, StringEscapeHandling escapeHandling)
        {
            switch (escapeHandling)
            {
                case StringEscapeHandling.EscapeNonAscii:
                    WriteUtf16StringEscapeNonAsciiValue(value, true);
                    break;

                case StringEscapeHandling.EscapeHtml:
                    WriteUtf16StringEscapeHtmlValue(value, true);
                    break;

                case StringEscapeHandling.Default:
                default:
                    WriteUtf16StringEscapeValue(value, true);
                    break;
            }
        }

        /// <summary>The value should already be properly escaped.</summary>
        public void WriteUtf16VerbatimNameSpan(in ReadOnlySpan<char> value)
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
    }
}
