namespace SpanJson
{
    using System;
    using System.Runtime.CompilerServices;

    partial struct JsonWriter<TSymbol>
    {
        /// <summary>The value should already be properly escaped.</summary>
        public void WriteUtf16EscapedName(string value)
        {
            WriteUtf16StringEscapedValue(value.AsSpan(), true);
        }

        /// <summary>The value should already be properly escaped.</summary>
        public void WriteUtf16EscapedName(in ReadOnlySpan<char> value)
        {
            WriteUtf16StringEscapedValue(value, true);
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

                case StringEscapeHandling.Default:
                default:
                    WriteUtf16StringEscapeValue(value, true);
                    break;
            }
        }

        /// <summary>The value should already be properly escaped.</summary>
        public void WriteUtf16VerbatimEscapedNameSpan(in ReadOnlySpan<char> value)
        {
            WriteUtf16StringEscapedValue(value, true);
        }

        public void WriteUtf16VerbatimNameSpan(in ReadOnlySpan<char> value)
        {
            WriteUtf16StringEscapeValue(value, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf16VerbatimNameSpan(in ReadOnlySpan<char> value, StringEscapeHandling escapeHandling)
        {
            WriteUtf16Name(value, escapeHandling);
        }
    }
}
