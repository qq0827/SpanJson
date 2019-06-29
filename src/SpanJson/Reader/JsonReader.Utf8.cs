using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CuteAnt;
using SpanJson.Formatters.Dynamic;
using SpanJson.Helpers;
using SpanJson.Internal;

namespace SpanJson
{
    public ref partial struct JsonReader<TSymbol> where TSymbol : struct
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte ReadUtf8SByte()
        {
            return checked((sbyte)ReadUtf8NumberInt64(ref MemoryMarshal.GetReference(_bytes), ref _pos, _length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadUtf8Int16()
        {
            return checked((short)ReadUtf8NumberInt64(ref MemoryMarshal.GetReference(_bytes), ref _pos, _length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadUtf8Int32()
        {
            return checked((int)ReadUtf8NumberInt64(ref MemoryMarshal.GetReference(_bytes), ref _pos, _length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadUtf8Int64()
        {
            return ReadUtf8NumberInt64(ref MemoryMarshal.GetReference(_bytes), ref _pos, _length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadUtf8Byte()
        {
            return checked((byte)ReadUtf8NumberUInt64(ref MemoryMarshal.GetReference(_bytes), ref _pos, _length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUtf8UInt16()
        {
            return checked((ushort)ReadUtf8NumberUInt64(ref MemoryMarshal.GetReference(_bytes), ref _pos, _length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUtf8UInt32()
        {
            return checked((uint)ReadUtf8NumberUInt64(ref MemoryMarshal.GetReference(_bytes), ref _pos, _length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadUtf8UInt64()
        {
            return ReadUtf8NumberUInt64(ref MemoryMarshal.GetReference(_bytes), ref _pos, _length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadUtf8Single()
        {
            var span = ReadUtf8NumberInternal();
            if (!Utf8Parser.TryParse(span, out float value, out var consumed) || span.Length != consumed)
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.InvalidNumberFormat, _pos);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadUtf8Double()
        {
            var span = ReadUtf8NumberInternal();
            if (!Utf8Parser.TryParse(span, out double value, out var consumed) || span.Length != consumed)
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.InvalidNumberFormat, _pos);
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ReadOnlySpan<byte> ReadUtf8NumberInternal()
        {
            ref var pos = ref _pos;
            ref byte bStart = ref MemoryMarshal.GetReference(_bytes);
            SkipWhitespaceUtf8(ref bStart, ref pos, _length);
            if (TryFindEndOfUtf8Number(ref bStart, pos, _length, out var bytesConsumed))
            {
                var result = _bytes.Slice(pos, bytesConsumed);
                pos += bytesConsumed;
                return result;
            }

            throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.EndOfData, pos);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long ReadUtf8NumberInt64(ref byte bStart, ref int pos, uint length)
        {
            var currentByte = SkipWhitespaceUtf8(ref bStart, ref pos, length);
            if ((uint)pos >= length)
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.EndOfData, pos);
            }

            var neg = false;
            if (currentByte == (byte)'-')
            {
                pos++;
                neg = true;

                if ((uint)pos >= length) // we still need one digit
                {
                    ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.EndOfData, pos);
                }
            }

            var result = ReadUtf8NumberDigits(ref bStart, ref pos, length);
            return neg ? unchecked(-(long)result) : checked((long)result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong ReadUtf8NumberUInt64(ref byte bStart, ref int pos, uint length)
        {
            SkipWhitespaceUtf8(ref bStart, ref pos, length);
            if ((uint)pos >= length)
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.EndOfData, pos);
            }

            return ReadUtf8NumberDigits(ref bStart, ref pos, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong ReadUtf8NumberDigits(ref byte b, ref int pos, uint length)
        {
            uint value;
            var result = Unsafe.AddByteOffset(ref b, (IntPtr)pos) - 48UL;
            if (result > 9ul) // this includes '-'
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.InvalidNumberFormat, pos);
            }

            pos++;
            while ((uint)pos < length && (value = Unsafe.AddByteOffset(ref b, (IntPtr)pos) - 48U) <= 9u)
            {
                result = checked(result * 10ul + value);
                pos++;
            }

            return result;
        }

        public bool ReadUtf8Boolean()
        {
            ref var pos = ref _pos;
            ref byte bStart = ref MemoryMarshal.GetReference(_bytes);
            SkipWhitespaceUtf8(ref bStart, ref pos, _length);
            if ((uint)pos + 4u <= _length)
            {
                ref var start = ref Unsafe.AddByteOffset(ref bStart, (IntPtr)pos);
                var value = Unsafe.ReadUnaligned<uint>(ref start);
                if (value == 0x65757274U /*eurt */)
                {
                    pos += 4;
                    return true;
                }

                if ((uint)pos + 5u <= _length && value == 0x736C6166U /* slaf */ && Unsafe.AddByteOffset(ref bStart, (IntPtr)(pos + 4)) == (byte)'e')
                {
                    pos += 5;
                    return false;
                }
            }

            throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.Bool, pos);
        }

        public char ReadUtf8Char()
        {
            ref var pos = ref _pos;
            ref byte bStart = ref MemoryMarshal.GetReference(_bytes);
            SkipWhitespaceUtf8(ref bStart, ref pos, _length);
            var span = ReadUtf8StringSpanInternal(ref bStart, ref pos, _length, out _);
            return ReadUtf8CharInternal(span, pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static char ReadUtf8CharInternal(in ReadOnlySpan<byte> span, int position)
        {
            if (span.Length == 1)
            {
                return (char)span[0];
            }

            ref byte source = ref MemoryMarshal.GetReference(span);
            var offset = (IntPtr)0;
            uint nValue = Unsafe.AddByteOffset(ref source, offset);
            if (nValue == JsonUtf8Constant.ReverseSolidus)
            {
                nValue = Unsafe.AddByteOffset(ref source, offset + 1);
                switch (nValue)
                {
                    case JsonUtf8Constant.DoubleQuote:
                        return JsonUtf16Constant.DoubleQuote;
                    case JsonUtf8Constant.ReverseSolidus:
                        return JsonUtf16Constant.ReverseSolidus;
                    case JsonUtf8Constant.Solidus:
                        return JsonUtf16Constant.Solidus;
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
                        if (Utf8Parser.TryParse(span.Slice(2, 4), out int value, out _, 'X'))
                        {
                            return (char)value;
                        }

                        ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.InvalidSymbol, position);
                        break;
                }
            }

            Span<char> charSpan = stackalloc char[1];
            TextEncodings.Utf8.GetChars(span, charSpan);
            return charSpan[0];
        }

        public DateTime ReadUtf8DateTime()
        {
            ref var pos = ref _pos;
            ref byte bStart = ref MemoryMarshal.GetReference(_bytes);
            SkipWhitespaceUtf8(ref bStart, ref pos, _length);
            var span = ReadUtf8StringSpanInternal(ref bStart, ref pos, _length, out var backslashIdx);
            return (uint)backslashIdx > JsonSharedConstant.TooBigOrNegative ? ParseUtf8DateTime(span, pos) : ParseUtf8DateTimeAllocating(span, backslashIdx, pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DateTime ParseUtf8DateTime(in ReadOnlySpan<byte> span, int pos)
        {
            if (JsonHelpers.TryParseAsISO(span, out DateTime value, out var bytesConsumed) && span.Length == bytesConsumed)
            {
                return value;
            }

            throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.DateTime, pos);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static DateTime ParseUtf8DateTimeAllocating(in ReadOnlySpan<byte> input, int backslashIdx, int pos)
        {
            byte[] unescapedArray = null;
            Span<byte> utf8Unescaped = (uint)input.Length <= JsonSharedConstant.StackallocThreshold ?
                stackalloc byte[input.Length] :
                (unescapedArray = ArrayPool<byte>.Shared.Rent(input.Length));
            try
            {
                JsonHelpers.Unescape(input, utf8Unescaped, backslashIdx, out var written);
                if (JsonHelpers.TryParseAsISO(utf8Unescaped.Slice(0, written), out DateTime value, out var bytesConsumed) && written == bytesConsumed)
                {
                    return value;
                }

                throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.DateTime, pos);
            }
            finally
            {
                if (unescapedArray != null) { ArrayPool<byte>.Shared.Return(unescapedArray); }
            }
        }

        public DateTimeOffset ReadUtf8DateTimeOffset()
        {
            ref var pos = ref _pos;
            ref byte bStart = ref MemoryMarshal.GetReference(_bytes);
            SkipWhitespaceUtf8(ref bStart, ref pos, _length);
            var span = ReadUtf8StringSpanInternal(ref bStart, ref pos, _length, out var backslashIdx);
            return (uint)backslashIdx > JsonSharedConstant.TooBigOrNegative ? ParseUtf8DateTimeOffset(span, pos) : ParseUtf8DateTimeOffsetAllocating(span, backslashIdx, pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DateTimeOffset ParseUtf8DateTimeOffset(in ReadOnlySpan<byte> span, int pos)
        {
            if (JsonHelpers.TryParseAsISO(span, out DateTimeOffset value, out var bytesConsumed) && span.Length == bytesConsumed)
            {
                return value;
            }

            throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.DateTimeOffset, pos);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static DateTimeOffset ParseUtf8DateTimeOffsetAllocating(in ReadOnlySpan<byte> input, int backslashIdx, int pos)
        {
            byte[] unescapedArray = null;
            Span<byte> utf8Unescaped = (uint)input.Length <= JsonSharedConstant.StackallocThreshold ?
                stackalloc byte[input.Length] :
                (unescapedArray = ArrayPool<byte>.Shared.Rent(input.Length));
            try
            {
                JsonHelpers.Unescape(input, utf8Unescaped, backslashIdx, out var written);
                if (JsonHelpers.TryParseAsISO(utf8Unescaped.Slice(0, written), out DateTimeOffset value, out var bytesConsumed) && written == bytesConsumed)
                {
                    return value;
                }

                throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.DateTimeOffset, pos);
            }
            finally
            {
                if (unescapedArray != null) { ArrayPool<byte>.Shared.Return(unescapedArray); }
            }
        }

        public TimeSpan ReadUtf8TimeSpan()
        {
            ref var pos = ref _pos;
            ref byte bStart = ref MemoryMarshal.GetReference(_bytes);
            SkipWhitespaceUtf8(ref bStart, ref pos, _length);
            var span = ReadUtf8StringSpanInternal(ref bStart, ref pos, _length, out var backslashIdx);
            return (uint)backslashIdx > JsonSharedConstant.TooBigOrNegative ? ParseUtf8TimeSpan(span, pos) : ParseUtf8TimeSpanAllocating(span, backslashIdx, pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TimeSpan ParseUtf8TimeSpan(in ReadOnlySpan<byte> span, int pos)
        {
            if (Utf8Parser.TryParse(span, out TimeSpan result, out var bytesConsumed) && span.Length == bytesConsumed)
            {
                return result;
            }

            throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.TimeSpan, pos);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static TimeSpan ParseUtf8TimeSpanAllocating(in ReadOnlySpan<byte> input, int backslashIdx, int pos)
        {
            byte[] unescapedArray = null;
            Span<byte> utf8Unescaped = (uint)input.Length <= JsonSharedConstant.StackallocThreshold ?
                stackalloc byte[input.Length] :
                (unescapedArray = ArrayPool<byte>.Shared.Rent(input.Length));
            try
            {
                JsonHelpers.Unescape(input, utf8Unescaped, backslashIdx, out var written);
                if (Utf8Parser.TryParse(utf8Unescaped.Slice(0, written), out TimeSpan result, out var bytesConsumed) && written == bytesConsumed)
                {
                    return result;
                }

                throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.TimeSpan, pos);
            }
            finally
            {
                if (unescapedArray != null) { ArrayPool<byte>.Shared.Return(unescapedArray); }
            }
        }

        public Guid ReadUtf8Guid()
        {
            ref var pos = ref _pos;
            ref byte bStart = ref MemoryMarshal.GetReference(_bytes);
            SkipWhitespaceUtf8(ref bStart, ref pos, _length);
            var span = ReadUtf8StringSpanInternal(ref bStart, ref pos, _length, out var backslashIdx);
            return (uint)backslashIdx > JsonSharedConstant.TooBigOrNegative ? ParseUtf8Guid(span, pos) : ParseUtf8GuidAllocating(span, backslashIdx, pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Guid ParseUtf8Guid(in ReadOnlySpan<byte> span, int pos)
        {
            if (Utf8Parser.TryParse(span, out Guid result, out var bytesConsumed) && span.Length == bytesConsumed)
            {
                return result;
            }

            throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.Guid, pos);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Guid ParseUtf8GuidAllocating(in ReadOnlySpan<byte> input, int backslashIdx, int pos)
        {
            byte[] unescapedArray = null;
            Span<byte> utf8Unescaped = (uint)input.Length <= JsonSharedConstant.StackallocThreshold ?
                stackalloc byte[input.Length] :
                (unescapedArray = ArrayPool<byte>.Shared.Rent(input.Length));
            try
            {
                JsonHelpers.Unescape(input, utf8Unescaped, backslashIdx, out var written);
                if (Utf8Parser.TryParse(utf8Unescaped.Slice(0, written), out Guid result, out var bytesConsumed) && written == bytesConsumed)
                {
                    return result;
                }

                throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.Guid, pos);
            }
            finally
            {
                if (unescapedArray != null) { ArrayPool<byte>.Shared.Return(unescapedArray); }
            }
        }

        public CombGuid ReadUtf8CombGuid()
        {
            ref var pos = ref _pos;
            ref byte bStart = ref MemoryMarshal.GetReference(_bytes);
            SkipWhitespaceUtf8(ref bStart, ref pos, _length);
            var span = ReadUtf8StringSpanInternal(ref bStart, ref pos, _length, out var backslashIdx);
            return (uint)backslashIdx > JsonSharedConstant.TooBigOrNegative ? ParseUtf8CombGuid(span, pos) : ParseUtf8CombGuidAllocating(span, backslashIdx, pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static CombGuid ParseUtf8CombGuid(in ReadOnlySpan<byte> span, int pos)
        {
#if NETSTANDARD2_0 || NET471 || NET451
            if (CombGuid.TryParse(TextEncodings.Utf8.GetString(span), CombGuidSequentialSegmentType.Comb, out CombGuid result))
#else
            if (CombGuid.TryParse(span, CombGuidSequentialSegmentType.Comb, out CombGuid result, out var bytesConsumed) && span.Length == bytesConsumed)
#endif
            {
                return result;
            }

            throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.CombGuid, pos);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static CombGuid ParseUtf8CombGuidAllocating(in ReadOnlySpan<byte> input, int backslashIdx, int pos)
        {
            byte[] unescapedArray = null;
            Span<byte> utf8Unescaped = (uint)input.Length <= JsonSharedConstant.StackallocThreshold ?
                stackalloc byte[input.Length] :
                (unescapedArray = ArrayPool<byte>.Shared.Rent(input.Length));
            try
            {
                JsonHelpers.Unescape(input, utf8Unescaped, backslashIdx, out var written);
#if NETSTANDARD2_0 || NET471 || NET451
                if (CombGuid.TryParse(TextEncodings.Utf8.GetString(utf8Unescaped.Slice(0, written)), CombGuidSequentialSegmentType.Comb, out CombGuid result))
#else
                if (CombGuid.TryParse(utf8Unescaped.Slice(0, written), CombGuidSequentialSegmentType.Comb, out CombGuid result, out var bytesConsumed) && written == bytesConsumed)
#endif
                {
                    return result;
                }

                throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.CombGuid, pos);
            }
            finally
            {
                if (unescapedArray != null) { ArrayPool<byte>.Shared.Return(unescapedArray); }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadUtf8EscapedName()
        {
            ref var pos = ref _pos;
            ref byte bStart = ref MemoryMarshal.GetReference(_bytes);
            SkipWhitespaceUtf8(ref bStart, ref pos, _length);
            var span = ReadUtf8StringSpanInternal(ref bStart, ref pos, _length, out var backslashIdx);
            var currentByte = SkipWhitespaceUtf8(ref bStart, ref pos, _length);
            pos++;
            if (currentByte != JsonUtf8Constant.NameSeparator)
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.ExpectedDoubleQuote, pos);
            }

            return (uint)backslashIdx > JsonSharedConstant.TooBigOrNegative
                ? TextEncodings.Utf8.GetString(span)
                : JsonHelpers.GetUnescapedString(span, backslashIdx);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> ReadUtf8EscapedNameSpan()
        {
            ref var pos = ref _pos;
            ref byte bStart = ref MemoryMarshal.GetReference(_bytes);
            SkipWhitespaceUtf8(ref bStart, ref pos, _length);
            var span = ReadUtf8StringSpanInternal(ref bStart, ref pos, _length, out var backslashIdx);
            var currentByte = SkipWhitespaceUtf8(ref bStart, ref pos, _length);
            pos++;
            if (currentByte != JsonUtf8Constant.NameSeparator)
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.ExpectedDoubleQuote, pos);
            }
            return (uint)backslashIdx > JsonSharedConstant.TooBigOrNegative ? span : UnescapeUtf8Bytes(span, backslashIdx);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> ReadUtf8VerbatimNameSpan()
        {
            return ReadUtf8VerbatimNameSpan(out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> ReadUtf8VerbatimNameSpan(out int backslashIdx)
        {
            ref var pos = ref _pos;
            ref byte bStart = ref MemoryMarshal.GetReference(_bytes);
            SkipWhitespaceUtf8(ref bStart, ref pos, _length);
            var span = ReadUtf8StringSpanInternal(ref bStart, ref pos, _length, out backslashIdx);
            var currentByte = SkipWhitespaceUtf8(ref bStart, ref pos, _length);
            pos++;
            if (currentByte != JsonUtf8Constant.NameSeparator)
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.ExpectedDoubleQuote, pos);
            }

            return span;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadUtf8String()
        {
            ref var pos = ref _pos;
            ref byte bStart = ref MemoryMarshal.GetReference(_bytes);
            if (ReadUtf8IsNullInternal(ref bStart, ref pos, _length))
            {
                return null;
            }

            var span = ReadUtf8StringSpanInternal(ref bStart, ref pos, _length, out var backslashIdx);
            return (uint)backslashIdx > JsonSharedConstant.TooBigOrNegative
                ? TextEncodings.Utf8.GetString(span)
                : JsonHelpers.GetUnescapedString(span, backslashIdx);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> ReadUtf8StringSpan()
        {
            ref var pos = ref _pos;
            ref byte bStart = ref MemoryMarshal.GetReference(_bytes);
            if (ReadUtf8IsNullInternal(ref bStart, ref pos, _length))
            {
                return default/*JsonUtf8Constant.NullTerminator*/;
            }

            var span = ReadUtf8StringSpanInternal(ref bStart, ref pos, _length, out var backslashIdx);
            return (uint)backslashIdx > JsonSharedConstant.TooBigOrNegative ? span : UnescapeUtf8Bytes(span, backslashIdx);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> ReadUtf8VerbatimStringSpan()
        {
            return ReadUtf8VerbatimStringSpan(out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> ReadUtf8VerbatimStringSpan(out int backslashIdx)
        {
            ref var pos = ref _pos;
            ref byte bStart = ref MemoryMarshal.GetReference(_bytes);
            if (ReadUtf8IsNullInternal(ref bStart, ref pos, _length))
            {
                backslashIdx = -1;
                return default/*JsonUtf8Constant.NullTerminator*/;
            }

            return ReadUtf8StringSpanInternal(ref bStart, ref pos, _length, out backslashIdx);
        }

        private static ReadOnlySpan<byte> UnescapeUtf8Bytes(in ReadOnlySpan<byte> span, int backslashIdx)
        {
            // not necessarily correct, just needs to be a good upper bound
            // this gets slightly too high, as the normal escapes are two bytes, and the \u1234 escapes are 6 bytes, but we only need 4
            var unescapedLength = span.Length;
            Span<byte> result = new byte[unescapedLength];
            JsonHelpers.Unescape(span, result, backslashIdx, out var written);
            return result.Slice(0, written);
        }

        private static ReadOnlySpan<byte> ReadUtf8StringSpanInternal(ref byte bStart, ref int pos, uint length, out int backslashIdx)
        {
            if ((uint)pos + 2u <= length)
            {
                ref var stringStart = ref Unsafe.AddByteOffset(ref bStart, (IntPtr)pos++);
                if (stringStart != JsonUtf8Constant.String)
                {
                    ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.ExpectedDoubleQuote, pos);
                }

                // We should also get info about how many escaped chars exist from here
                if (TryFindEndOfUtf8String(ref stringStart, length - (uint)pos, out int stringLength, out backslashIdx))
                {
#if NETSTANDARD2_0 || NET471 || NET451
                    unsafe
                    {
                        var result = new ReadOnlySpan<byte>(Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref stringStart, (IntPtr)1)), stringLength - 1);
                        pos += stringLength; // skip the doublequote too
                        return result;
                    }
#else
                    var result = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AddByteOffset(ref stringStart, (IntPtr)1), stringLength - 1);
                    pos += stringLength; // skip the doublequote too
                    return result;
#endif
                }
            }

            throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.ExpectedDoubleQuote, pos);
        }

        /// <summary>
        ///     Includes the quotes on each end
        /// </summary>
        private static ReadOnlySpan<byte> ReadUtf8StringSpanWithQuotes(ref byte bStart, ref int pos, uint length)
        {
            if ((uint)pos + 2u <= length)
            {
                ref var stringStart = ref Unsafe.AddByteOffset(ref bStart, (IntPtr)pos++);
                if (stringStart != JsonUtf8Constant.String)
                {
                    ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.ExpectedDoubleQuote, pos);
                }

                // We should also get info about how many escaped chars exist from here
                if (TryFindEndOfUtf8String(ref stringStart, length - (uint)pos, out int stringLength, out _))
                {
#if NETSTANDARD2_0 || NET471 || NET451
                    unsafe
                    {
                        var result = new ReadOnlySpan<byte>(Unsafe.AsPointer(ref stringStart), stringLength + 1);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal ReadUtf8Decimal()
        {
            if (Utf8Parser.TryParse(ReadUtf8NumberInternal(), out decimal result, out var bytesConsumed))
            {
                return result;
            }

            throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.InvalidNumberFormat, _pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadUtf8IsNull()
        {
            return ReadUtf8IsNullInternal(ref MemoryMarshal.GetReference(_bytes), ref _pos, _length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ReadUtf8IsNullInternal(ref byte bStart, ref int pos, uint length)
        {
            SkipWhitespaceUtf8(ref bStart, ref pos, length);
            ref var start = ref Unsafe.AddByteOffset(ref bStart, (IntPtr)pos);
            if ((uint)pos + 4u <= length && Unsafe.ReadUnaligned<uint>(ref start) == 0x6C6C756EU /* llun */)
            {
                pos += 4;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadUtf8Null()
        {
            if (!ReadUtf8IsNull())
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.InvalidSymbol, _pos);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint SkipWhitespaceUtf8(ref byte bStart, ref int pos, uint length)
        {
            uint currentByte = Unsafe.AddByteOffset(ref bStart, (IntPtr)pos);
            switch (currentByte)
            {
                case JsonUtf8Constant.Space:
                case JsonUtf8Constant.Tab:
                case JsonUtf8Constant.CarriageReturn:
                case JsonUtf8Constant.LineFeed:
                    pos++;
                    break;
                default: return currentByte;
            }
            return SkipWhitespaceUtf8Slow(ref bStart, ref pos, length);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static uint SkipWhitespaceUtf8Slow(ref byte bStart, ref int pos, uint length)
        {
            uint currentByte = 0u;
            while ((uint)pos < length)
            {
                currentByte = Unsafe.AddByteOffset(ref bStart, (IntPtr)pos);
                switch (currentByte)
                {
                    case JsonUtf8Constant.Space:
                    case JsonUtf8Constant.Tab:
                    case JsonUtf8Constant.CarriageReturn:
                    case JsonUtf8Constant.LineFeed:
                        pos++;
                        continue;
                    default:
                        return currentByte;
                }
            }
            return currentByte;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadUtf8BeginArrayOrThrow()
        {
            if (!ReadUtf8BeginArray())
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.ExpectedBeginArray, _pos);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadUtf8BeginArray()
        {
            ref var pos = ref _pos;
            ref byte bStart = ref MemoryMarshal.GetReference(_bytes);
            var currentByte = SkipWhitespaceUtf8(ref bStart, ref pos, _length);
            if ((uint)pos < _length && currentByte == JsonUtf8Constant.BeginArray)
            {
                pos++;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReadUtf8IsEndArrayOrValueSeparator(ref int count)
        {
            ref var pos = ref _pos;
            ref byte bStart = ref MemoryMarshal.GetReference(_bytes);
            var currentByte = SkipWhitespaceUtf8(ref bStart, ref pos, _length);
            if ((uint)pos < _length && currentByte == JsonUtf8Constant.EndArray)
            {
                pos++;
                return true;
            }

            if (count++ > 0)
            {
                if ((uint)pos < _length && Unsafe.AddByteOffset(ref bStart, (IntPtr)pos) == JsonUtf8Constant.ValueSeparator)
                {
                    pos++;
                    return false;
                }

                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.ExpectedSeparator, pos);
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadUtf8IsBeginObject()
        {
            ref var pos = ref _pos;
            ref byte bStart = ref MemoryMarshal.GetReference(_bytes);
            var currentByte = SkipWhitespaceUtf8(ref bStart, ref pos, _length);
            if (currentByte == JsonUtf8Constant.BeginObject)
            {
                pos++;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadUtf8BeginObjectOrThrow()
        {
            if (!ReadUtf8IsBeginObject())
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.ExpectedBeginObject, _pos);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadUtf8EndObjectOrThrow()
        {
            if (!ReadUtf8IsEndObject())
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.ExpectedEndObject, _pos);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadUtf8EndArrayOrThrow()
        {
            if (!ReadUtf8IsEndArray())
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.ExpectedEndArray, _pos);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadUtf8IsEndObject()
        {
            ref var pos = ref _pos;
            ref byte bStart = ref MemoryMarshal.GetReference(_bytes);
            var currentByte = SkipWhitespaceUtf8(ref bStart, ref pos, _length);
            if (currentByte == JsonUtf8Constant.EndObject)
            {
                pos++;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadUtf8IsEndArray()
        {
            ref var pos = ref _pos;
            ref byte bStart = ref MemoryMarshal.GetReference(_bytes);
            var currentByte = SkipWhitespaceUtf8(ref bStart, ref pos, _length);
            if (currentByte == JsonUtf8Constant.EndArray)
            {
                pos++;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReadUtf8IsEndObjectOrValueSeparator(ref int count)
        {
            ref var pos = ref _pos;
            ref byte bStart = ref MemoryMarshal.GetReference(_bytes);
            var currentByte = SkipWhitespaceUtf8(ref bStart, ref pos, _length);
            if ((uint)pos < _length && currentByte == JsonUtf8Constant.EndObject)
            {
                pos++;
                return true;
            }

            if (count++ > 0)
            {
                if (Unsafe.AddByteOffset(ref bStart, (IntPtr)pos) == JsonUtf8Constant.ValueSeparator)
                {
                    pos++;
                    return false;
                }

                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.ExpectedSeparator, pos);
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Version ReadUtf8Version()
        {
            var stringValue = ReadUtf8String();
            if (stringValue == null)
            {
                return default;
            }

            return Version.Parse(stringValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Uri ReadUtf8Uri()
        {
            var stringValue = ReadUtf8String();
            if (stringValue == null)
            {
                return default;
            }

            return new Uri(stringValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadUtf8SymbolOrThrow(byte constant)
        {
            if (!ReadUtf8IsSymbol(constant))
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.InvalidSymbol, _pos);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadUtf8IsSymbol(byte constant)
        {
            ref var pos = ref _pos;
            ref byte bStart = ref MemoryMarshal.GetReference(_bytes);
            var currentByte = SkipWhitespaceUtf8(ref bStart, ref pos, _length);
            if ((uint)pos < _length && currentByte == constant)
            {
                pos++;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SkipNextUtf8Segment()
        {
            SkipNextUtf8Segment(ref MemoryMarshal.GetReference(_bytes), ref _pos, _length, 0);
        }

        private static void SkipNextUtf8Segment(ref byte bStart, ref int pos, uint length, int stack)
        {
            while ((uint)pos < length)
            {
                var token = ReadUtf8NextTokenInternal(ref bStart, ref pos, length);
                switch (token)
                {
                    case JsonToken.None:
                        return;
                    case JsonToken.BeginArray:
                    case JsonToken.BeginObject:
                        {
                            pos++;
                            stack++;
                            continue;
                        }
                    case JsonToken.EndObject:
                    case JsonToken.EndArray:
                        {
                            pos++;
                            if (stack - 1 > 0)
                            {
                                stack--;
                                continue;
                            }

                            return;
                        }
                    case JsonToken.Number:
                    case JsonToken.String:
                    case JsonToken.True:
                    case JsonToken.False:
                    case JsonToken.Null:
                    case JsonToken.ValueSeparator:
                    case JsonToken.NameSeparator:
                        {
                            do
                            {
                                SkipNextUtf8ValueInternal(ref bStart, ref pos, length, token);
                                token = ReadUtf8NextTokenInternal(ref bStart, ref pos, length);
                            } while (stack > 0 && (byte)token > 4); // No None or the Begin/End-Array/Object tokens

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
        public void SkipNextUtf8Value(JsonToken token)
        {
            SkipNextUtf8ValueInternal(ref MemoryMarshal.GetReference(_bytes), ref _pos, _length, token);
        }

        private static void SkipNextUtf8ValueInternal(ref byte bStart, ref int pos, uint length, JsonToken token)
        {
            switch (token)
            {
                case JsonToken.None:
                    break;
                case JsonToken.BeginObject:
                case JsonToken.EndObject:
                case JsonToken.BeginArray:
                case JsonToken.EndArray:
                case JsonToken.ValueSeparator:
                case JsonToken.NameSeparator:
                    pos++;
                    break;
                case JsonToken.Number:
                    {
                        if (TryFindEndOfUtf8Number(ref bStart, pos, length, out var bytesConsumed))
                        {
                            pos += bytesConsumed;
                        }

                        break;
                    }
                case JsonToken.String:
                    {
                        if (SkipUtf8String(ref bStart, ref pos, length))
                        {
                            return;
                        }

                        ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.ExpectedDoubleQuote, pos);
                        break;
                    }
                case JsonToken.Null:
                case JsonToken.True:
                    pos += 4;
                    break;
                case JsonToken.False:
                    pos += 5;
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFindEndOfUtf8Number(ref byte bStart, int pos, uint length, out int bytesConsumed)
        {
            uint nValue;
            var i = pos;
            for (; (uint)i < length; i++)
            {
                nValue = Unsafe.AddByteOffset(ref bStart, (IntPtr)i);
                if (!IsNumericSymbol(nValue))
                {
                    break;
                }
            }

            if ((uint)i > (uint)pos)
            {
                bytesConsumed = i - pos;
                return true;
            }

            bytesConsumed = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFindEndOfUtf8String(ref byte stringStart, uint length, out int stringLength, out int backslashIdx)
        {
            const uint DoubleQuote = JsonUtf8Constant.String;

            IntPtr offset = (IntPtr)1; // igore '"'
            var idx = SpanHelpers.IndexOfAny(ref Unsafe.AddByteOffset(ref stringStart, offset), JsonUtf8Constant.String, JsonUtf8Constant.ReverseSolidus, (int)length);

            if ((uint)idx > JsonSharedConstant.TooBigOrNegative) // -1
            {
                stringLength = 0;
                backslashIdx = -1;
                return false;
            }

            uint foundByte = Unsafe.AddByteOffset(ref stringStart, offset + idx);
            if (foundByte == DoubleQuote)
            {
                stringLength = idx + 1;
                backslashIdx = -1;
                return true;
            }

            backslashIdx = idx;
            stringLength = idx;
            return TryFindEndOfUtf8StringSlow(ref stringStart, length, ref stringLength);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool TryFindEndOfUtf8StringSlow(ref byte stringStart, uint length, ref int stringLength)
        {
            const uint DoubleQuote = JsonUtf8Constant.String;
            const uint BackSlash = JsonUtf8Constant.ReverseSolidus;
            const uint Unicode = (byte)'u';

            IntPtr offset = (IntPtr)1;
            uint currentByte;
            while ((uint)stringLength < length)
            {
                currentByte = Unsafe.AddByteOffset(ref stringStart, offset + stringLength++);
                switch (currentByte)
                {
                    case BackSlash:
                        currentByte = Unsafe.AddByteOffset(ref stringStart, offset + stringLength++);
                        if (currentByte == Unicode)
                        {
                            stringLength += 4;
                        }
                        break;

                    case DoubleQuote:
                        return true;
                }
            }

            return false;
        }

        private static bool SkipUtf8String(ref byte b, ref int pos, uint length)
        {
            ref var stringStart = ref Unsafe.AddByteOffset(ref b, (IntPtr)pos++);
            // We should also get info about how many escaped chars exist from here
            if (TryFindEndOfUtf8String(ref stringStart, length - (uint)pos, out int stringLength, out _))
            {
                pos += stringLength; // skip the doublequote too
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object ReadUtf8Dynamic()
        {
            return ReadUtf8Dynamic(0);
        }

        public object ReadUtf8Dynamic(int stack)
        {
            ref var pos = ref _pos;
            var nextToken = ReadUtf8NextToken();
            if ((uint)stack > JsonSharedConstant.NestingLimit)
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.NestingTooDeep, pos);
            }

            switch (nextToken)
            {
                case JsonToken.Null:
                    {
                        ReadUtf8Null();
                        return null;
                    }
                case JsonToken.False:
                case JsonToken.True:
                    {
                        return ReadUtf8Boolean();
                    }
                case JsonToken.Number:
                    {
                        return new SpanJsonDynamicUtf8Number(ReadUtf8NumberInternal());
                    }
                case JsonToken.String:
                    {
                        var span = ReadUtf8StringSpanWithQuotes(ref MemoryMarshal.GetReference(_bytes), ref pos, _length);
                        return new SpanJsonDynamicUtf8String(span);
                    }
                case JsonToken.BeginObject:
                    {
                        pos++;
                        var count = 0;
                        var dictionary = new Dictionary<string, object>(StringComparer.Ordinal);
                        while (!TryReadUtf8IsEndObjectOrValueSeparator(ref count))
                        {
                            var name = ReadUtf8EscapedName();
                            var value = ReadUtf8Dynamic(stack + 1);
                            dictionary[name] = value; // take last one
                        }

                        return new SpanJsonDynamicObject(dictionary);
                    }
                case JsonToken.BeginArray:
                    {
                        pos++;
                        var count = 0;
                        object[] temp = null;
                        try
                        {
                            temp = ArrayPool<object>.Shared.Rent(4);
                            while (!TryReadUtf8IsEndArrayOrValueSeparator(ref count))
                            {
                                if (count == temp.Length)
                                {
                                    FormatterUtils.GrowArray(ref temp);
                                }

                                temp[count - 1] = ReadUtf8Dynamic(stack + 1);
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

                            return new SpanJsonDynamicArray<TSymbol>(result);
                        }
                        finally
                        {
                            if (temp != null)
                            {
                                ArrayPool<object>.Shared.Return(temp);
                            }
                        }
                    }
                default:
                    {
                        ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.EndOfData, pos);
                        return default;
                    }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JsonToken ReadUtf8NextToken()
        {
            return ReadUtf8NextTokenInternal(ref MemoryMarshal.GetReference(_bytes), ref _pos, _length);
        }

        private static JsonToken ReadUtf8NextTokenInternal(ref byte bStart, ref int pos, uint length)
        {
            var currentByte = SkipWhitespaceUtf8(ref bStart, ref pos, length);
            if ((uint)pos >= length)
            {
                return JsonToken.None;
            }

            switch (currentByte)
            {
                case JsonUtf8Constant.BeginObject:
                    return JsonToken.BeginObject;
                case JsonUtf8Constant.EndObject:
                    return JsonToken.EndObject;
                case JsonUtf8Constant.BeginArray:
                    return JsonToken.BeginArray;
                case JsonUtf8Constant.EndArray:
                    return JsonToken.EndArray;
                case JsonUtf8Constant.String:
                    return JsonToken.String;
                case JsonUtf8Constant.True:
                    return JsonToken.True;
                case JsonUtf8Constant.False:
                    return JsonToken.False;
                case JsonUtf8Constant.Null:
                    return JsonToken.Null;
                case JsonUtf8Constant.ValueSeparator:
                    return JsonToken.ValueSeparator;
                case JsonUtf8Constant.NameSeparator:
                    return JsonToken.NameSeparator;
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
                    return JsonToken.Number;
                default:
                    return JsonToken.None;
            }
        }
    }
}