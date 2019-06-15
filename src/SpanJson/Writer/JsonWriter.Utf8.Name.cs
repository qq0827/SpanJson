namespace SpanJson
{
    using System;
    using System.Buffers;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using SpanJson.Internal;

    partial struct JsonWriter<TSymbol>
    {
        /// <summary>The value should already be properly escaped.</summary>
        public void WriteUtf8EscapedName(string value)
        {
            WriteUtf8StringEscapedValue(value.AsSpan(), true);
        }

        /// <summary>The value should already be properly escaped.</summary>
        public void WriteUtf8EscapedName(in ReadOnlySpan<char> value)
        {
            WriteUtf8StringEscapedValue(value, true);
        }

        public void WriteUtf8Name(string value)
        {
            WriteUtf8StringEscapeValue(value.AsSpan(), true);
        }

        public void WriteUtf8Name(in ReadOnlySpan<char> value)
        {
            WriteUtf8StringEscapeValue(value, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8Name(string value, StringEscapeHandling escapeHandling)
        {
            WriteUtf8Name(value.AsSpan(), escapeHandling);
        }

        public void WriteUtf8Name(in ReadOnlySpan<char> value, StringEscapeHandling escapeHandling)
        {
            switch (escapeHandling)
            {
                case StringEscapeHandling.EscapeNonAscii:
                    WriteUtf8StringEscapeNonAsciiValue(value, true);
                    break;

                case StringEscapeHandling.Default:
                default:
                    WriteUtf8StringEscapeValue(value, true);
                    break;
            }
        }

        /// <summary>The value should already be properly escaped.</summary>
        public void WriteUtf8VerbatimEscapedNameSpan(in ReadOnlySpan<byte> value)
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
        public void WriteUtf8VerbatimNameSpan(in ReadOnlySpan<byte> value)
        {
            WriteUtf8VerbatimNameSpan(value, StringEscapeHandling.Default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUtf8VerbatimNameSpan(in ReadOnlySpan<byte> value, StringEscapeHandling escapeHandling)
        {
            var firstEscapeIndex = EscapingHelper.NeedsEscaping(value, escapeHandling);
            if ((uint)firstEscapeIndex > JsonSharedConstant.TooBigOrNegative) // -1
            {
                WriteUtf8VerbatimEscapedNameSpan(value);
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

            WriteUtf8VerbatimEscapedNameSpan(escapedPropertyName.Slice(0, written));

            if (propertyArray != null)
            {
                ArrayPool<byte>.Shared.Return(propertyArray);
            }
        }
    }
}
