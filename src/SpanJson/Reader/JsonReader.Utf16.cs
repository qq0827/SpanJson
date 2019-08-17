using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CuteAnt;
using SpanJson.Dynamic;
using SpanJson.Helpers;
using SpanJson.Internal;

namespace SpanJson
{
    public ref partial struct JsonReader<TSymbol> where TSymbol : struct
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void EnsureUtf16InnerBufferCreated()
        {
            if (_utf16Json.NonEmpty()) { return; }
            _utf16Json = new ArraySegment<char>(_utf16Span.ToArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte ReadUtf16SByte()
        {
            return checked((sbyte)ReadUtf16NumberInt64(ref MemoryMarshal.GetReference(_utf16Span), ref _pos, _length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadUtf16Int16()
        {
            return checked((short)ReadUtf16NumberInt64(ref MemoryMarshal.GetReference(_utf16Span), ref _pos, _length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadUtf16Int32()
        {
            return checked((int)ReadUtf16NumberInt64(ref MemoryMarshal.GetReference(_utf16Span), ref _pos, _length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadUtf16Int64()
        {
            return ReadUtf16NumberInt64(ref MemoryMarshal.GetReference(_utf16Span), ref _pos, _length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadUtf16Byte()
        {
            return checked((byte)ReadUtf16NumberUInt64(ref MemoryMarshal.GetReference(_utf16Span), ref _pos, _length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUtf16UInt16()
        {
            return checked((ushort)ReadUtf16NumberUInt64(ref MemoryMarshal.GetReference(_utf16Span), ref _pos, _length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUtf16UInt32()
        {
            return checked((uint)ReadUtf16NumberUInt64(ref MemoryMarshal.GetReference(_utf16Span), ref _pos, _length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadUtf16UInt64()
        {
            return ReadUtf16NumberUInt64(ref MemoryMarshal.GetReference(_utf16Span), ref _pos, _length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadUtf16Single()
        {
#if NETSTANDARD2_0 || NET471 || NET451
            return float.Parse(ReadUtf16NumberSpan().ToString(), NumberStyles.Float, CultureInfo.InvariantCulture);
#else
            return float.Parse(ReadUtf16NumberSpan(), NumberStyles.Float, CultureInfo.InvariantCulture);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadUtf16Double()
        {
#if NETSTANDARD2_0 || NET471 || NET451
            return double.Parse(ReadUtf16NumberSpan().ToString(), NumberStyles.Float, CultureInfo.InvariantCulture);
#else
            return double.Parse(ReadUtf16NumberSpan(), NumberStyles.Float, CultureInfo.InvariantCulture);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<char> ReadUtf16NumberSpan()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_utf16Span);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            if (TryFindEndOfUtf16Number(ref cStart, pos, _length, out var charsConsumed))
            {
                var result = _utf16Span.Slice(pos, charsConsumed);
                pos += charsConsumed;
                return result;
            }

            throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.EndOfData, pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ArraySegment<char> ReadUtf16RawNumber()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_utf16Span);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            if (TryFindEndOfUtf16Number(ref cStart, pos, _length, out var charsConsumed))
            {
                var result = _utf16Json.Slice(pos, charsConsumed);
                pos += charsConsumed;
                return result;
            }

            throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.EndOfData, pos);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long ReadUtf16NumberInt64(ref char cStart, ref int pos, uint length)
        {
            var currentChar = SkipWhitespaceUtf16(ref cStart, ref pos, length);
            if ((uint)pos >= length)
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.EndOfData, pos);
            }

            var neg = false;
            if (currentChar == '-')
            {
                pos++;
                neg = true;

                if ((uint)pos >= length) // we still need one digit
                {
                    ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.EndOfData, pos);
                }
            }

            var result = ReadUtf16NumberDigits(ref cStart, ref pos, length);
            return neg ? unchecked(-(long)result) : checked((long)result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong ReadUtf16NumberDigits(ref char c, ref int pos, uint length)
        {
            uint value;
            var result = Unsafe.Add(ref c, pos) - 48UL;
            if (result > 9ul) // this includes '-'
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.InvalidNumberFormat, pos);
            }

            pos++;
            while ((uint)pos < length && (value = Unsafe.Add(ref c, pos) - 48U) <= 9u)
            {
                result = checked(result * 10ul + value);
                pos++;
            }

            return result;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong ReadUtf16NumberUInt64(ref char cStart, ref int pos, uint length)
        {
            SkipWhitespaceUtf16(ref cStart, ref pos, length);
            if ((uint)pos >= length)
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.EndOfData, pos);
            }

            return ReadUtf16NumberDigits(ref cStart, ref pos, length);
        }

        public bool ReadUtf16Boolean()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_utf16Span);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            if ((uint)pos + 4u <= _length)
            {
                ref var start = ref Unsafe.Add(ref cStart, pos);
                ref var bstart = ref Unsafe.As<char, byte>(ref start);
                var value = Unsafe.ReadUnaligned<ulong>(ref bstart);
                if (value == 0x0065007500720074UL /*eurt */)
                {
                    pos += 4;
                    return true;
                }

                if ((uint)pos + 5u <= _length && value == 0x0073006C00610066UL /* slaf */ && Unsafe.Add(ref cStart, pos + 4) == 'e')
                {
                    pos += 5;
                    return false;
                }
            }

            throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.Bool, pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char ReadUtf16Char()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_utf16Span);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            var span = ReadUtf16StringSpanInternal(ref cStart, ref pos, _length, out _);
            return ReadUtf16CharInternal(span, _pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static char ReadUtf16CharInternal(in ReadOnlySpan<char> span, int position)
        {
            var pos = 0;
            if (span.Length == 1)
            {
                return span[pos];
            }

            if (span[pos++] == JsonUtf16Constant.ReverseSolidus)
            {
                var current = span[pos++];
                switch (current)
                {
                    case JsonUtf16Constant.DoubleQuote:
                    case JsonUtf16Constant.ReverseSolidus:
                        return current;
                    case JsonUtf16Constant.Solidus:
                        return current;
                    case 'b':
                        return '\b';
                    case 'f':
                        return '\f';
                    case 'n':
                        return '\n';
                    case 'r':
                        return '\r';
                    case 't':
                        return '\t';
                    case 'u':
                        {
                            if (int.TryParse(span.Slice(pos, 4)
#if NETSTANDARD2_0 || NET471 || NET451
                                .ToString()
#endif
                                , NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var value))
                            {
                                return (char)value;
                            }

                            break;
                        }
                }
            }

            throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, position);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadUtf16EndObjectOrThrow()
        {
            if (!ReadUtf16IsEndObject())
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.ExpectedEndObject, _pos);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadUtf16EndArrayOrThrow()
        {
            if (!ReadUtf16IsEndArray())
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.ExpectedEndArray, _pos);
            }
        }

        public DateTime ReadUtf16DateTime()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_utf16Span);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            var span = ReadUtf16StringSpanInternal(ref cStart, ref pos, _length, out var escapedCharsSize);
            return 0u >= (uint)escapedCharsSize ? ParseUtf16DateTime(span, pos) : ParseUtf16DateTimeAllocating(span, pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DateTime ParseUtf16DateTime(in ReadOnlySpan<char> span, int pos)
        {
            if (JsonReaderHelper.TryParseAsISO(span, out DateTime value))
            {
                return value;
            }

            throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.DateTime, pos);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static DateTime ParseUtf16DateTimeAllocating(in ReadOnlySpan<char> input, int pos)
        {
            char[] unescapedArray = null;
            Span<char> utf16Unescaped = (uint)input.Length <= JsonSharedConstant.StackallocThreshold ?
                stackalloc char[input.Length] :
                (unescapedArray = ArrayPool<char>.Shared.Rent(input.Length));
            try
            {
                UnescapeUtf16Chars(input, ref utf16Unescaped);
                if (JsonReaderHelper.TryParseAsISO(utf16Unescaped, out DateTime value))
                {
                    return value;
                }

                throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.DateTime, pos);
            }
            finally
            {
                if (unescapedArray is object) { ArrayPool<char>.Shared.Return(unescapedArray); }
            }
        }

        public DateTimeOffset ReadUtf16DateTimeOffset()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_utf16Span);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            var span = ReadUtf16StringSpanInternal(ref cStart, ref pos, _length, out var escapedCharsSize);
            return 0u >= (uint)escapedCharsSize ? ParseUtf16DateTimeOffset(span, pos) : ParseUtf16DateTimeOffsetAllocating(span, pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DateTimeOffset ParseUtf16DateTimeOffset(in ReadOnlySpan<char> span, int pos)
        {
            if (JsonReaderHelper.TryParseAsISO(span, out DateTimeOffset value))
            {
                return value;
            }

            throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.DateTimeOffset, pos);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static DateTimeOffset ParseUtf16DateTimeOffsetAllocating(in ReadOnlySpan<char> input, int pos)
        {
            char[] unescapedArray = null;
            Span<char> utf16Unescaped = (uint)input.Length <= JsonSharedConstant.StackallocThreshold ?
                stackalloc char[input.Length] :
                (unescapedArray = ArrayPool<char>.Shared.Rent(input.Length));
            try
            {
                UnescapeUtf16Chars(input, ref utf16Unescaped);
                if (JsonReaderHelper.TryParseAsISO(utf16Unescaped, out DateTimeOffset value))
                {
                    return value;
                }

                throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.DateTimeOffset, pos);
            }
            finally
            {
                if (unescapedArray is object) { ArrayPool<char>.Shared.Return(unescapedArray); }
            }
        }

        public TimeSpan ReadUtf16TimeSpan()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_utf16Span);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            var span = ReadUtf16StringSpanInternal(ref cStart, ref pos, _length, out var escapedCharsSize);
            return 0u >= (uint)escapedCharsSize ? ConvertTimeSpanViaUtf8(span, _pos) : ParseUtf16TimeSpanAllocating(span, pos);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static TimeSpan ParseUtf16TimeSpanAllocating(in ReadOnlySpan<char> input, int pos)
        {
            char[] unescapedArray = null;
            Span<char> utf16Unescaped = (uint)input.Length <= JsonSharedConstant.StackallocThreshold ?
                stackalloc char[input.Length] :
                (unescapedArray = ArrayPool<char>.Shared.Rent(input.Length));
            try
            {
                UnescapeUtf16Chars(input, ref utf16Unescaped);
                return ConvertTimeSpanViaUtf8(utf16Unescaped, pos);
            }
            finally
            {
                if (unescapedArray is object) { ArrayPool<char>.Shared.Return(unescapedArray); }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TimeSpan ConvertTimeSpanViaUtf8(in ReadOnlySpan<char> span, int position)
        {
            Span<byte> byteSpan = stackalloc byte[JsonSharedConstant.MaxTimeSpanLength];
            for (var i = 0; i < span.Length; i++)
            {
                byteSpan[i] = (byte)span[i];
            }

            // TODO: replace with utf16 code in .net core 3.0
            if (Utf8Parser.TryParse(byteSpan, out TimeSpan value, out var bytesConsumed) && bytesConsumed == span.Length)
            {
                return value;
            }

            throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.TimeSpan, position);
        }

        public Guid ReadUtf16Guid()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_utf16Span);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            var span = ReadUtf16StringSpanInternal(ref cStart, ref pos, _length, out var escapedCharsSize);
            return 0u >= (uint)escapedCharsSize ? ConvertGuidViaUtf8(span, _pos) : ParseUtf16GuidAllocating(span, pos);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Guid ParseUtf16GuidAllocating(in ReadOnlySpan<char> input, int pos)
        {
            char[] unescapedArray = null;
            Span<char> utf16Unescaped = (uint)input.Length <= JsonSharedConstant.StackallocThreshold ?
                stackalloc char[input.Length] :
                (unescapedArray = ArrayPool<char>.Shared.Rent(input.Length));
            try
            {
                UnescapeUtf16Chars(input, ref utf16Unescaped);
                return ConvertGuidViaUtf8(utf16Unescaped, pos);
            }
            finally
            {
                if (unescapedArray is object) { ArrayPool<char>.Shared.Return(unescapedArray); }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Guid ConvertGuidViaUtf8(in ReadOnlySpan<char> span, int position)
        {
            Span<byte> byteSpan = stackalloc byte[JsonSharedConstant.MaxGuidLength]; // easy way
            for (var i = 0; i < span.Length; i++)
            {
                byteSpan[i] = (byte)span[i];
            }

            // TODO: replace with utf16 code in .net core 3.0
            if (Utf8Parser.TryParse(byteSpan, out Guid result, out var bytesConsumed, 'D') && bytesConsumed == span.Length)
            {
                return result;
            }

            throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.Guid, position);
        }

        public CombGuid ReadUtf16CombGuid()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_utf16Span);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            var span = ReadUtf16StringSpanInternal(ref cStart, ref pos, _length, out var escapedCharsSize);
            return 0u >= (uint)escapedCharsSize ? ParseUtf16CombGuid(span, _pos) : ParseUtf16CombGuidAllocating(span, pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static CombGuid ParseUtf16CombGuid(in ReadOnlySpan<char> span, int pos)
        {
            if (CombGuid.TryParse(span
#if NETSTANDARD2_0 || NET471 || NET451
                .ToString()
#endif
                , CombGuidSequentialSegmentType.Comb, out CombGuid value))
            {
                return value;
            }

            throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.CombGuid, pos);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static CombGuid ParseUtf16CombGuidAllocating(in ReadOnlySpan<char> input, int pos)
        {
            char[] unescapedArray = null;
            Span<char> utf16Unescaped = (uint)input.Length <= JsonSharedConstant.StackallocThreshold ?
                stackalloc char[input.Length] :
                (unescapedArray = ArrayPool<char>.Shared.Rent(input.Length));
            try
            {
                UnescapeUtf16Chars(input, ref utf16Unescaped);
                if (CombGuid.TryParse(utf16Unescaped
#if NETSTANDARD2_0 || NET471 || NET451
                    .ToString()
#endif
                    , CombGuidSequentialSegmentType.Comb, out CombGuid value))
                {
                    return value;
                }

                throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.CombGuid, pos);
            }
            finally
            {
                if (unescapedArray is object) { ArrayPool<char>.Shared.Return(unescapedArray); }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadUtf16EscapedName()
        {
            var span = ReadUtf16VerbatimNameSpan(out int escapedCharsSize);

            return 0u >= (uint)escapedCharsSize ? span.ToString() : UnescapeUtf16(span, escapedCharsSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<char> ReadUtf16EscapedNameSpan()
        {
            var span = ReadUtf16VerbatimNameSpan(out int escapedCharsSize);

#if NETSTANDARD2_0 || NET471 || NET451
            return 0u >= (uint)escapedCharsSize ? span : UnescapeUtf16(span, escapedCharsSize).AsSpan();
#else
            return 0u >= (uint)escapedCharsSize ? span : UnescapeUtf16(span, escapedCharsSize);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<char> ReadUtf16VerbatimNameSpan()
        {
            return ReadUtf16VerbatimNameSpan(out _);
        }

        public ReadOnlySpan<char> ReadUtf16VerbatimNameSpan(out int escapedCharsSize)
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_utf16Span);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            var span = ReadUtf16StringSpanInternal(ref cStart, ref pos, _length, out escapedCharsSize);
            var currentChar = SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            pos++;
            if (currentChar != JsonUtf16Constant.NameSeparator)
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.ExpectedDoubleQuote, pos);
            }

            return span;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ReadUtf16BytesFromBase64()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_utf16Span);
            if (ReadUtf16IsNullInternal(ref cStart, ref pos, _length))
            {
                return null;
            }

            var base64Span = ReadUtf16StringSpanInternal(ref cStart, ref pos, _length, out var escapedCharSize);
            if (escapedCharSize > 0) { base64Span = UnescapeUtf16(base64Span, escapedCharSize).AsSpan(); }

            var inputLength = base64Span.Length;
            // We need to get rid of any trailing white spaces.
            // Otherwise we would be rejecting input such as "abc= ":
            while (inputLength > 0)
            {
                int lastChar = base64Span[inputLength - 1];
                if (lastChar != (int)' ' && lastChar != (int)'\n' && lastChar != (int)'\r' && lastChar != (int)'\t') { break; }
                inputLength--;
            }

            if (0u >= (uint)inputLength) { return JsonHelpers.Empty<byte>(); }

            unsafe
            {
                fixed (char* inArrayPtr = &MemoryMarshal.GetReference(base64Span))
                {
                    return Base64Helper.FromBase64CharPtr(inArrayPtr, base64Span.Length);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadUtf16String()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_utf16Span);
            if (ReadUtf16IsNullInternal(ref cStart, ref pos, _length))
            {
                return null;
            }

            var span = ReadUtf16StringSpanInternal(ref cStart, ref pos, _length, out int escapedCharSize);

            return 0u >= (uint)escapedCharSize ? span.ToString() : UnescapeUtf16(span, escapedCharSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<char> ReadUtf16StringSpan()
        {
            var span = ReadUtf16VerbatimStringSpan(out int escapedCharSize);

#if NETSTANDARD2_0 || NET471 || NET451
            return 0u >= (uint)escapedCharSize ? span : UnescapeUtf16(span, escapedCharSize).AsSpan();
#else
            return 0u >= (uint)escapedCharSize ? span : UnescapeUtf16(span, escapedCharSize);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<char> ReadUtf16VerbatimStringSpan()
        {
            return ReadUtf16VerbatimStringSpan(out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<char> ReadUtf16VerbatimStringSpan(out int escapedCharSize)
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_utf16Span);
            if (ReadUtf16IsNullInternal(ref cStart, ref pos, _length))
            {
                escapedCharSize = 0;
                return default/*JsonUtf16Constant.NullTerminator*/;
            }

            return ReadUtf16StringSpanInternal(ref cStart, ref pos, _length, out escapedCharSize);
        }

        internal static
#if NETSTANDARD2_0 || NET471 || NET451
            unsafe
#endif
            string UnescapeUtf16(in ReadOnlySpan<char> span, int escapedCharSize)
        {
            var unescapedLength = span.Length - escapedCharSize;
            var result = new string('\0', unescapedLength);
#if NETSTANDARD2_0 || NET471 || NET451
            var writeableSpan = new Span<char>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(result.AsSpan())), unescapedLength);
#else
            var writeableSpan = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(result.AsSpan()), unescapedLength);
#endif
            UnescapeUtf16Chars(span, ref writeableSpan);
            return result;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void UnescapeUtf16Chars(in ReadOnlySpan<char> span, ref Span<char> result)
        {
            var ulen = (uint)span.Length;
            var charOffset = 0;
            var from = 0;
            var index = 0;
            ref char source = ref MemoryMarshal.GetReference(span);
            while ((uint)index < ulen)
            {
                uint current = Unsafe.Add(ref source, index);
                if (current == JsonUtf16Constant.ReverseSolidus)
                {
                    // We copy everything up to the escaped char as utf16 to the string
                    var copyLength = index - from;
                    span.Slice(from, copyLength).CopyTo(result.Slice(charOffset));
                    charOffset += copyLength;
                    index++;
                    current = Unsafe.Add(ref source, index++);
                    char unescaped = default;
                    switch (current)
                    {
                        case JsonUtf16Constant.DoubleQuote:
                            unescaped = JsonUtf16Constant.DoubleQuote;
                            break;
                        case JsonUtf16Constant.ReverseSolidus:
                            unescaped = JsonUtf16Constant.ReverseSolidus;
                            break;
                        case JsonUtf16Constant.Solidus:
                            unescaped = JsonUtf16Constant.Solidus;
                            break;
                        case 'b':
                            unescaped = '\b';
                            break;
                        case 'f':
                            unescaped = '\f';
                            break;
                        case 'n':
                            unescaped = '\n';
                            break;
                        case 'r':
                            unescaped = '\r';
                            break;
                        case 't':
                            unescaped = '\t';
                            break;
                        case 'u':
                            {
                                if (int.TryParse(span.Slice(index, 4)
#if NETSTANDARD2_0 || NET471 || NET451
                                    .ToString()
#endif
                                    , NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var value))
                                {
                                    index += 4;
                                    unescaped = (char)value;
                                    break;
                                }

                                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.InvalidSymbol, index);
                                break;
                            }
                        default:
                            {
                                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.InvalidSymbol, index);
                                break;
                            }
                    }

                    result[charOffset++] = unescaped;
                    from = index;
                }
                else
                {
                    index++;
                }
            }

            if ((uint)from < ulen) // still data to copy
            {
                var sliceLength = span.Length - from;
                span.Slice(from, sliceLength).CopyTo(result.Slice(charOffset));
                charOffset += sliceLength;
            }

            result = result.Slice(0, charOffset);
        }

        private static ReadOnlySpan<char> ReadUtf16StringSpanInternal(ref char cStart, ref int pos, uint length, out int escapedCharsSize)
        {
            if ((uint)pos + 2u <= length)
            {
                ref var stringStart = ref Unsafe.Add(ref cStart, pos++);
                if (stringStart != JsonUtf16Constant.String)
                {
                    ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.ExpectedDoubleQuote, pos);
                }

                var stringLength = 0;
                // We should also get info about how many escaped chars exist from here
                if (TryFindEndOfUtf16String(ref stringStart, length - (uint)pos, ref stringLength, out escapedCharsSize))
                {
#if NETSTANDARD2_0 || NET471 || NET451
                    unsafe
                    {
                        var result = new ReadOnlySpan<char>(Unsafe.AsPointer(ref Unsafe.Add(ref stringStart, 1)), stringLength - 1);
                        pos += stringLength; // skip the doublequote too
                        return result;
                    }
#else
                    var result = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref stringStart, 1), stringLength - 1);
                    pos += stringLength; // skip the doublequote too
                    return result;
#endif
                }
            }

            throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.ExpectedDoubleQuote, pos);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<char> ReadUtf16StringSpanWithQuotes()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_utf16Span);

            return ReadUtf16StringSpanWithQuotes(ref cStart, ref pos, _length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ReadUtf16StringSpanWithQuotes(out ArraySegment<char> result)
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_utf16Span);

            ReadUtf16StringSpanWithQuotes(ref cStart, ref pos, _length, out result);
        }

        /// <summary>
        ///     Includes the quotes on each end
        /// </summary>
        internal static ReadOnlySpan<char> ReadUtf16StringSpanWithQuotes(ref char cStart, ref int pos, uint length)
        {
            if ((uint)pos + 2u <= length)
            {
                ref var stringStart = ref Unsafe.Add(ref cStart, pos++);
                if (stringStart != JsonUtf16Constant.String)
                {
                    ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.ExpectedDoubleQuote, pos);
                }

                var stringLength = 0;
                // We should also get info about how many escaped chars exist from here
                if (TryFindEndOfUtf16String(ref stringStart, length - (uint)pos, ref stringLength, out _))
                {
#if NETSTANDARD2_0 || NET471 || NET451
                    unsafe
                    {
                        var result = new ReadOnlySpan<char>(Unsafe.AsPointer(ref stringStart), stringLength + 1);
                        pos += stringLength; // skip the doublequote too
                        return result;
                    }
#else
                    var result = MemoryMarshal.CreateReadOnlySpan(ref stringStart, stringLength + 1);
                    pos += stringLength; // skip the doublequote too
                    return result;
#endif
                }
            }

            throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.ExpectedDoubleQuote, pos);
        }
        internal void ReadUtf16StringSpanWithQuotes(ref char cStart, ref int pos, uint length, out ArraySegment<char> result)
        {
            if ((uint)pos + 2u <= length)
            {
                var offset = pos;
                ref var stringStart = ref Unsafe.Add(ref cStart, pos++);
                if (stringStart != JsonUtf16Constant.String)
                {
                    ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.ExpectedDoubleQuote, pos);
                }

                var stringLength = 0;
                // We should also get info about how many escaped chars exist from here
                if (TryFindEndOfUtf16String(ref stringStart, length - (uint)pos, ref stringLength, out _))
                {
                    result = _utf16Json.Slice(offset, stringLength + 1);
                    pos += stringLength; // skip the doublequote too
                    return;
                }
            }

            throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.ExpectedDoubleQuote, pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal ReadUtf16Decimal()
        {
            return decimal.Parse(ReadUtf16NumberSpan()
#if NETSTANDARD2_0 || NET471 || NET451
                .ToString()
#endif
                , NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadUtf16IsNull()
        {
            return ReadUtf16IsNullInternal(ref MemoryMarshal.GetReference(_utf16Span), ref _pos, _length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ReadUtf16IsNullInternal(ref char cStart, ref int pos, uint length)
        {
            SkipWhitespaceUtf16(ref cStart, ref pos, length);
            ref var start = ref Unsafe.Add(ref cStart, pos);
            ref var bstart = ref Unsafe.As<char, byte>(ref start);
            if ((uint)pos + 4u <= length && Unsafe.ReadUnaligned<ulong>(ref bstart) == 0x006C006C0075006EUL /* llun */)
            {
                pos += 4;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadUtf16Null()
        {
            if (!ReadUtf16IsNull())
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.InvalidSymbol, _pos);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint SkipWhitespaceUtf16(ref char cStart, ref int pos, uint length)
        {
            uint currentChar = Unsafe.Add(ref cStart, pos);
            switch (currentChar)
            {
                case JsonUtf16Constant.Space:
                case JsonUtf16Constant.Tab:
                case JsonUtf16Constant.CarriageReturn:
                case JsonUtf16Constant.LineFeed:
                    pos++;
                    break;
                default: return currentChar;
            }
            return SkipWhitespaceUtf16Slow(ref cStart, ref pos, length);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static uint SkipWhitespaceUtf16Slow(ref char cStart, ref int pos, uint length)
        {
            uint currentChar = 0u;
            while ((uint)pos < length)
            {
                currentChar = Unsafe.Add(ref cStart, pos);
                switch (currentChar)
                {
                    case JsonUtf16Constant.Space:
                    case JsonUtf16Constant.Tab:
                    case JsonUtf16Constant.CarriageReturn:
                    case JsonUtf16Constant.LineFeed:
                        pos++;
                        continue;
                    default:
                        return currentChar;
                }
            }
            return currentChar;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadUtf16BeginArrayOrThrow()
        {
            if (!ReadUtf16BeginArray())
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.ExpectedBeginArray, _pos);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadUtf16BeginArray()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_utf16Span);
            var currentChar = SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            if ((uint)pos < _length && currentChar == JsonUtf16Constant.BeginArray)
            {
                pos++;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReadUtf16IsEndArrayOrValueSeparator(ref int count)
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_utf16Span);
            var currentChar = SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            if ((uint)pos < _length && currentChar == JsonUtf16Constant.EndArray)
            {
                pos++;
                return true;
            }

            if (count++ > 0)
            {
                if ((uint)pos < _length && Unsafe.Add(ref cStart, pos) == JsonUtf16Constant.ValueSeparator)
                {
                    pos++;
                    return false;
                }

                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.ExpectedSeparator, pos);
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadUtf16IsBeginObject()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_utf16Span);
            var currentChar = SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            if (currentChar == JsonUtf16Constant.BeginObject)
            {
                pos++;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadUtf16BeginObjectOrThrow()
        {
            if (!ReadUtf16IsBeginObject())
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.ExpectedBeginObject, _pos);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadUtf16IsEndObject()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_utf16Span);
            var currentChar = SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            if (currentChar == JsonUtf16Constant.EndObject)
            {
                pos++;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadUtf16IsEndArray()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_utf16Span);
            var currentChar = SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            if (currentChar == JsonUtf16Constant.EndArray)
            {
                pos++;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReadUtf16IsEndObjectOrValueSeparator(ref int count)
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_utf16Span);
            var currentChar = SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            if ((uint)pos < _length && currentChar == JsonUtf16Constant.EndObject)
            {
                pos++;
                return true;
            }

            if (count++ > 0)
            {
                if (Unsafe.Add(ref cStart, pos) == JsonUtf16Constant.ValueSeparator)
                {
                    pos++;
                    return false;
                }

                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.ExpectedSeparator, pos);
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Version ReadUtf16Version()
        {
            var stringValue = ReadUtf16String();
            if (stringValue is null)
            {
                return default;
            }

            return Version.Parse(stringValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Uri ReadUtf16Uri()
        {
            var stringValue = ReadUtf16String();
            if (stringValue is null)
            {
                return default;
            }

            if (Uri.TryCreate(stringValue, UriKind.RelativeOrAbsolute, out Uri value))
            {
                return value;
            }
            throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.Uri, _pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadUtf16SymbolOrThrow(char constant)
        {
            if (!ReadUtf16IsSymbol(constant))
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.InvalidSymbol, _pos);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadUtf16IsSymbol(char constant)
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_utf16Span);
            var currentChar = SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            if (currentChar == constant)
            {
                pos++;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SkipNextUtf16Segment()
        {
            SkipNextUtf16Segment(ref MemoryMarshal.GetReference(_utf16Span), ref _pos, _length, 0);
        }

        private static void SkipNextUtf16Segment(ref char cStart, ref int pos, uint length, int stack)
        {
            while ((uint)pos < length)
            {
                var token = ReadUtf16NextTokenInternal(ref cStart, ref pos, length);
                switch (token)
                {
                    case JsonTokenType.None:
                        return;
                    case JsonTokenType.BeginArray:
                    case JsonTokenType.BeginObject:
                        {
                            pos++;
                            stack++;
                            continue;
                        }
                    case JsonTokenType.EndObject:
                    case JsonTokenType.EndArray:
                        {
                            pos++;
                            if (stack - 1 > 0)
                            {
                                stack--;
                                continue;
                            }

                            return;
                        }
                    case JsonTokenType.Number:
                    case JsonTokenType.String:
                    case JsonTokenType.True:
                    case JsonTokenType.False:
                    case JsonTokenType.Null:
                    case JsonTokenType.ValueSeparator:
                    case JsonTokenType.NameSeparator:
                        {
                            do
                            {
                                SkipNextUtf16ValueInternal(ref cStart, ref pos, length, token);
                                token = ReadUtf16NextTokenInternal(ref cStart, ref pos, length);
                            } while (stack > 0 && (uint)token > 4u); // No None or the Begin/End-Array/Object tokens

                            if (stack > 0)
                            {
                                continue;
                            }

                            return;
                        }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SkipNextUtf16Value(JsonTokenType token)
        {
            SkipNextUtf16ValueInternal(ref MemoryMarshal.GetReference(_utf16Span), ref _pos, _length, token);
        }

        private static void SkipNextUtf16ValueInternal(ref char cStart, ref int pos, uint length, JsonTokenType token)
        {
            switch (token)
            {
                case JsonTokenType.None:
                    break;
                case JsonTokenType.BeginObject:
                case JsonTokenType.EndObject:
                case JsonTokenType.BeginArray:
                case JsonTokenType.EndArray:
                case JsonTokenType.ValueSeparator:
                case JsonTokenType.NameSeparator:
                    pos++;
                    break;
                case JsonTokenType.Number:
                    {
                        if (TryFindEndOfUtf16Number(ref cStart, pos, length, out var charsConsumed))
                        {
                            pos += charsConsumed;
                        }

                        break;
                    }
                case JsonTokenType.String:
                    {
                        if (SkipUtf16String(ref cStart, ref pos, length))
                        {
                            return;
                        }

                        ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.ExpectedDoubleQuote, pos);
                        break;
                    }
                case JsonTokenType.Null:
                case JsonTokenType.True:
                    pos += 4;
                    break;
                case JsonTokenType.False:
                    pos += 5;
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFindEndOfUtf16Number(ref char cStart, int pos, uint length, out int charsConsumed)
        {
            uint nValue;
            var i = pos;
            for (; (uint)i < length; i++)
            {
                nValue = Unsafe.Add(ref cStart, i);
                if (!IsNumericSymbol(nValue))
                {
                    break;
                }
            }

            if ((uint)i > (uint)pos)
            {
                charsConsumed = i - pos;
                return true;
            }

            charsConsumed = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFindEndOfUtf16String(ref char stringStart, uint length, ref int stringLength, out int escapedCharsSize)
        {
            const uint DoubleQuote = JsonUtf16Constant.String;

            var idx = SpanHelpers.IndexOfAny(ref Unsafe.Add(ref stringStart, 1), JsonUtf16Constant.String, JsonUtf16Constant.ReverseSolidus, (int)length);

            if ((uint)idx > JsonSharedConstant.TooBigOrNegative) // -1
            {
                stringLength = 0;
                escapedCharsSize = 0;
                return false;
            }

            uint foundByte = Unsafe.Add(ref stringStart, idx + 1);
            if (foundByte == DoubleQuote)
            {
                stringLength = idx + 1;
                escapedCharsSize = 0;
                return true;
            }

            stringLength = idx;
            return TryFindEndOfUtf16StringSlow(ref stringStart, length, ref stringLength, out escapedCharsSize);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool TryFindEndOfUtf16StringSlow(ref char stringStart, uint length, ref int stringLength, out int escapedCharsSize)
        {
            const uint DoubleQuote = JsonUtf16Constant.String;
            const uint BackSlash = JsonUtf16Constant.ReverseSolidus;
            const uint Unicode = 'u';

            escapedCharsSize = 0;
            uint currentChar;
            while ((uint)stringLength < length)
            {
                currentChar = Unsafe.Add(ref stringStart, ++stringLength);
                switch (currentChar)
                {
                    case BackSlash:
                        escapedCharsSize++;
                        currentChar = Unsafe.Add(ref stringStart, ++stringLength);
                        if (currentChar == Unicode)
                        {
                            escapedCharsSize += 4; // add only 4 and not 5 as we still need one unescaped char
                            stringLength += 4;
                        }
                        break;

                    case DoubleQuote:
                        return true;
                }
            }

            return false;
        }

        private static bool SkipUtf16String(ref char cStart, ref int pos, uint length)
        {
            ref var stringStart = ref Unsafe.Add(ref cStart, pos++);
            if (stringStart != JsonUtf16Constant.String)
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.ExpectedDoubleQuote, pos);
            }

            var stringLength = 0;
            // We should also get info about how many escaped chars exist from here
            if (TryFindEndOfUtf16String(ref stringStart, length - (uint)pos, ref stringLength, out _))
            {
                pos += stringLength; // skip the doublequote too
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object ReadUtf16Dynamic()
        {
            return ReadUtf16Dynamic(0);
        }

        public object ReadUtf16Dynamic(int stack)
        {
            ref var pos = ref _pos;
            var nextToken = ReadUtf16NextToken();
            if ((uint)stack > JsonSharedConstant.NestingLimit)
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.NestingTooDeep, pos);
            }

            switch (nextToken)
            {
                case JsonTokenType.Null:
                    ReadUtf16Null();
                    return null;

                case JsonTokenType.False:
                case JsonTokenType.True:
                    return ReadUtf16Boolean();

                case JsonTokenType.Number:
                    if (_utf16Json.NonEmpty())
                    {
                        return new SpanJsonDynamicUtf16Number(ReadUtf16RawNumber());
                    }
                    else
                    {
                        return new SpanJsonDynamicUtf16Number(ReadUtf16NumberSpan());
                    }

                case JsonTokenType.String:
                    if (_utf16Json.NonEmpty())
                    {
                        ReadUtf16StringSpanWithQuotes(ref MemoryMarshal.GetReference(_utf16Span), ref pos, _length, out ArraySegment<char> result);
                        return new SpanJsonDynamicUtf16String(result);
                    }
                    else
                    {
                        var span = ReadUtf16StringSpanWithQuotes(ref MemoryMarshal.GetReference(_utf16Span), ref pos, _length);
                        return new SpanJsonDynamicUtf16String(span);
                    }

                case JsonTokenType.BeginObject:
                    {
                        var startOffset = pos;
                        pos++;
                        var count = 0;
                        var dictionary = new Dictionary<string, object>(StringComparer.Ordinal);
                        while (!TryReadUtf16IsEndObjectOrValueSeparator(ref count))
                        {
                            var name = ReadUtf16EscapedName();
                            var value = ReadUtf16Dynamic(stack + 1);
                            dictionary[name] = value; // take last one
                        }

                        if (_utf16Json.NonEmpty())
                        {
                            return new SpanJsonDynamicObject(dictionary, _utf16Json.Slice(startOffset, pos - startOffset), true);
                        }
                        else
                        {
                            return new SpanJsonDynamicObject(dictionary);
                        }
                    }
                case JsonTokenType.BeginArray:
                    {
                        var startOffset = pos;
                        pos++;
                        var count = 0;
                        object[] temp = null;
                        try
                        {
                            temp = ArrayPool<object>.Shared.Rent(4);
                            while (!TryReadUtf16IsEndArrayOrValueSeparator(ref count))
                            {
                                if (count == temp.Length)
                                {
                                    FormatterUtils.GrowArray(ref temp);
                                }

                                temp[count - 1] = ReadUtf16Dynamic(stack + 1);
                            }

                            object[] result;
                            if (0u >= (uint)count)
                            {
                                result = JsonHelpers.Empty<object>();
                            }
                            else
                            {
                                result = FormatterUtils.CopyArray(temp, count);
                            }

                            if (_utf16Json.NonEmpty())
                            {
                                var rawJson = _utf16Json.Slice(startOffset, pos - startOffset);
                                return new SpanJsonDynamicArray<TSymbol>(result, Unsafe.As<ArraySegment<char>, ArraySegment<TSymbol>>(ref rawJson));
                            }
                            else
                            {
                                return new SpanJsonDynamicArray<TSymbol>(result);
                            }
                        }
                        finally
                        {
                            if (temp is object)
                            {
                                ArrayPool<object>.Shared.Return(temp);
                            }
                        }
                    }
                default:
                    throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.EndOfData, pos);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JsonTokenType ReadUtf16NextToken()
        {
            return ReadUtf16NextTokenInternal(ref MemoryMarshal.GetReference(_utf16Span), ref _pos, _length);
        }

        public static JsonTokenType ReadUtf16NextTokenInternal(ref char cStart, ref int pos, uint length)
        {
            var currentChar = SkipWhitespaceUtf16(ref cStart, ref pos, length);
            if ((uint)pos >= length)
            {
                return JsonTokenType.None;
            }

            switch (currentChar)
            {
                case JsonUtf16Constant.BeginObject:
                    return JsonTokenType.BeginObject;
                case JsonUtf16Constant.EndObject:
                    return JsonTokenType.EndObject;
                case JsonUtf16Constant.BeginArray:
                    return JsonTokenType.BeginArray;
                case JsonUtf16Constant.EndArray:
                    return JsonTokenType.EndArray;
                case JsonUtf16Constant.String:
                    return JsonTokenType.String;
                case JsonUtf16Constant.True:
                    return JsonTokenType.True;
                case JsonUtf16Constant.False:
                    return JsonTokenType.False;
                case JsonUtf16Constant.Null:
                    return JsonTokenType.Null;
                case JsonUtf16Constant.ValueSeparator:
                    return JsonTokenType.ValueSeparator;
                case JsonUtf16Constant.NameSeparator:
                    return JsonTokenType.NameSeparator;
                case '-':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '0':
                    return JsonTokenType.Number;
                default:
                    return JsonTokenType.None;
            }
        }
    }
}