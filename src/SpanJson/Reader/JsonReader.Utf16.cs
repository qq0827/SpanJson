using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SpanJson.Formatters.Dynamic;
using SpanJson.Helpers;
using SpanJson.Internal;

namespace SpanJson
{
    public ref partial struct JsonReader<TSymbol> where TSymbol : struct
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte ReadUtf16SByte()
        {
            return checked((sbyte)ReadUtf16NumberInt64(ref MemoryMarshal.GetReference(_chars), ref _pos, _length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadUtf16Int16()
        {
            return checked((short)ReadUtf16NumberInt64(ref MemoryMarshal.GetReference(_chars), ref _pos, _length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadUtf16Int32()
        {
            return checked((int)ReadUtf16NumberInt64(ref MemoryMarshal.GetReference(_chars), ref _pos, _length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadUtf16Int64()
        {
            return ReadUtf16NumberInt64(ref MemoryMarshal.GetReference(_chars), ref _pos, _length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadUtf16Byte()
        {
            return checked((byte)ReadUtf16NumberUInt64(ref MemoryMarshal.GetReference(_chars), ref _pos, _length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUtf16UInt16()
        {
            return checked((ushort)ReadUtf16NumberUInt64(ref MemoryMarshal.GetReference(_chars), ref _pos, _length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUtf16UInt32()
        {
            return checked((uint)ReadUtf16NumberUInt64(ref MemoryMarshal.GetReference(_chars), ref _pos, _length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadUtf16UInt64()
        {
            return ReadUtf16NumberUInt64(ref MemoryMarshal.GetReference(_chars), ref _pos, _length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadUtf16Single()
        {
#if NETSTANDARD2_0 || NET471 || NET451
            return float.Parse(ReadUtf16NumberInternal().ToString(), NumberStyles.Float, CultureInfo.InvariantCulture);
#else
            return float.Parse(ReadUtf16NumberInternal(), NumberStyles.Float, CultureInfo.InvariantCulture);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ReadUtf16Double()
        {
#if NETSTANDARD2_0 || NET471 || NET451
            return double.Parse(ReadUtf16NumberInternal().ToString(), NumberStyles.Float, CultureInfo.InvariantCulture);
#else
            return double.Parse(ReadUtf16NumberInternal(), NumberStyles.Float, CultureInfo.InvariantCulture);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ReadOnlySpan<char> ReadUtf16NumberInternal()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_chars);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            if (TryFindEndOfUtf16Number(ref cStart, pos, _length, out var charsConsumed))
            {
                var result = _chars.Slice(pos, charsConsumed);
                pos += charsConsumed;
                return result;
            }

            throw GetJsonParserException(JsonParserException.ParserError.EndOfData, pos);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long ReadUtf16NumberInt64(ref char cStart, ref int pos, uint length)
        {
            SkipWhitespaceUtf16(ref cStart, ref pos, length);
            if ((uint)pos >= length)
            {
                ThrowJsonParserException(JsonParserException.ParserError.EndOfData, pos);
            }

            var neg = false;
            if (Unsafe.Add(ref cStart, pos) == '-')
            {
                pos++;
                neg = true;

                if ((uint)pos >= length) // we still need one digit
                {
                    ThrowJsonParserException(JsonParserException.ParserError.EndOfData, pos);
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
                ThrowJsonParserException(JsonParserException.ParserError.InvalidNumberFormat, pos);
            }

            pos++;
            while ((uint)pos < length && (value = Unsafe.Add(ref c, pos) - 48U) <= 9)
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
                ThrowJsonParserException(JsonParserException.ParserError.EndOfData, pos);
            }

            return ReadUtf16NumberDigits(ref cStart, ref pos, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsNumericUtf16Symbol(char c)
        {
            switch (c)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '+':
                case '-':
                case '.':
                case 'E':
                case 'e':
                    return true;
            }

            return false;
        }

        public bool ReadUtf16Boolean()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_chars);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            if ((uint)pos <= _length - 4u)
            {
                ref var start = ref Unsafe.Add(ref cStart, pos);
                ref var bstart = ref Unsafe.As<char, byte>(ref start);
                var value = Unsafe.ReadUnaligned<ulong>(ref bstart);
                if (value == 0x0065007500720074UL /*eurt */)
                {
                    pos += 4;
                    return true;
                }

                if ((uint)pos <= _length - 5u && value == 0x0073006C00610066UL /* slaf */ && Unsafe.Add(ref cStart, pos + 4) == 'e')
                {
                    pos += 5;
                    return false;
                }
            }

            throw GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.Bool, pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char ReadUtf16Char()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_chars);
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
                ref readonly var current = ref span[pos++];
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

            throw GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, position);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadUtf16EndObjectOrThrow()
        {
            if (!ReadUtf16IsEndObject())
            {
                ThrowJsonParserException(JsonParserException.ParserError.ExpectedEndObject, _pos);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadUtf16EndArrayOrThrow()
        {
            if (!ReadUtf16IsEndArray())
            {
                ThrowJsonParserException(JsonParserException.ParserError.ExpectedEndArray, _pos);
            }
        }

        public DateTime ReadUtf16DateTime()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_chars);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            var span = ReadUtf16StringSpanInternal(ref cStart, ref pos, _length, out var escapedCharsSize);
            return 0u >= (uint)escapedCharsSize ? ParseUtf16DateTime(span) : ParseUtf16DateTimeAllocating(span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DateTime ParseUtf16DateTime(in ReadOnlySpan<char> span)
        {
            if (DateTimeParser.TryParseDateTime(span, out var value, out var bytesConsumed) && span.Length == bytesConsumed)
            {
                return value;
            }

            throw GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.DateTime, _pos);
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private DateTime ParseUtf16DateTimeAllocating(in ReadOnlySpan<char> input)
        {
            Span<char> span = stackalloc char[JsonSharedConstant.MaxDateTimeLength];
            UnescapeUtf16Chars(input, ref span);
            if (DateTimeParser.TryParseDateTime(span, out var value, out var bytesConsumed) && span.Length == bytesConsumed)
            {
                return value;
            }

            throw GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.DateTime, _pos);
        }

        public DateTimeOffset ReadUtf16DateTimeOffset()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_chars);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            var span = ReadUtf16StringSpanInternal(ref cStart, ref pos, _length, out var escapedCharsSize);
            return 0u >= (uint)escapedCharsSize ? ParseUtf16DateTimeOffset(span) : ParseUtf16DateTimeOffsetAllocating(span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DateTimeOffset ParseUtf16DateTimeOffset(in ReadOnlySpan<char> span)
        {
            if (DateTimeParser.TryParseDateTimeOffset(span, out var value, out var bytesConsumed) && span.Length == bytesConsumed)
            {
                return value;
            }

            throw GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.DateTimeOffset, _pos);
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private DateTimeOffset ParseUtf16DateTimeOffsetAllocating(in ReadOnlySpan<char> input)
        {
            Span<char> span = stackalloc char[JsonSharedConstant.MaxDateTimeOffsetLength];
            UnescapeUtf16Chars(input, ref span);
            if (DateTimeParser.TryParseDateTimeOffset(span, out var value, out var bytesConsumed) && span.Length == bytesConsumed)
            {
                return value;
            }

            throw GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.DateTimeOffset, _pos);
        }

        public TimeSpan ReadUtf16TimeSpan()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_chars);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            var span = ReadUtf16StringSpanInternal(ref cStart, ref pos, _length, out var escapedCharsSize);
            return 0u >= (uint)escapedCharsSize ? ConvertTimeSpanViaUtf8(span, _pos) : ParseUtf16TimeSpanAllocating(span);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private TimeSpan ParseUtf16TimeSpanAllocating(in ReadOnlySpan<char> input)
        {
            Span<char> span = stackalloc char[JsonSharedConstant.MaxTimeSpanLength];
            UnescapeUtf16Chars(input, ref span);
            return ConvertTimeSpanViaUtf8(span, _pos);
        }

        public Guid ReadUtf16Guid()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_chars);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            var span = ReadUtf16StringSpanInternal(ref cStart, ref pos, _length, out var escapedCharsSize);
            return 0u >= (uint)escapedCharsSize ? ConvertGuidViaUtf8(span, _pos) : ParseUtf16GuidAllocating(span);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private Guid ParseUtf16GuidAllocating(in ReadOnlySpan<char> input)
        {
            Span<char> span = stackalloc char[JsonSharedConstant.MaxGuidLength];
            UnescapeUtf16Chars(input, ref span);
            return ConvertGuidViaUtf8(span, _pos);
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

            throw GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.TimeSpan, position);
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

            throw GetJsonParserException(JsonParserException.ParserError.InvalidSymbol, JsonParserException.ValueType.Guid, position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadUtf16EscapedName()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_chars);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            var span = ReadUtf16StringSpanInternal(ref cStart, ref pos, _length, out var escapedCharsSize);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            if (_chars[_pos++] != JsonUtf16Constant.NameSeparator)
            {
                ThrowJsonParserException(JsonParserException.ParserError.ExpectedDoubleQuote, _pos);
            }

            return 0u >= (uint)escapedCharsSize ? span.ToString() : UnescapeUtf16(span, escapedCharsSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<char> ReadUtf16VerbatimNameSpan()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_chars);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            var span = ReadUtf16StringSpanInternal(ref cStart, ref pos, _length, out _);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            if (_chars[_pos++] != JsonUtf16Constant.NameSeparator)
            {
                ThrowJsonParserException(JsonParserException.ParserError.ExpectedDoubleQuote, _pos);
            }

            return span;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<char> ReadUtf16EscapedNameSpan()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_chars);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            var span = ReadUtf16StringSpanInternal(ref cStart, ref pos, _length, out var escapedCharsSize);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            if (_chars[_pos++] != JsonUtf16Constant.NameSeparator)
            {
                ThrowJsonParserException(JsonParserException.ParserError.ExpectedDoubleQuote, _pos);
            }

#if NETSTANDARD2_0 || NET471 || NET451
            return 0u >= (uint)escapedCharsSize ? span : UnescapeUtf16(span, escapedCharsSize).AsSpan();
#else
            return 0u >= (uint)escapedCharsSize ? span : UnescapeUtf16(span, escapedCharsSize);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadUtf16String()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_chars);
            if (ReadUtf16IsNullInternal(ref cStart, ref pos, _length))
            {
                return null;
            }

            var span = ReadUtf16StringSpanInternal(ref cStart, ref pos, _length, out var escapedCharSize);
            return escapedCharSize == 0 ? span.ToString() : UnescapeUtf16(span, escapedCharSize);
        }

        private static
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
                ref readonly var current = ref Unsafe.Add(ref source, index);
                if (current == JsonUtf16Constant.ReverseSolidus)
                {
                    // We copy everything up to the escaped char as utf16 to the string
                    var copyLength = index - from;
                    span.Slice(from, copyLength).CopyTo(result.Slice(charOffset));
                    charOffset += copyLength;
                    index++;
                    current = ref Unsafe.Add(ref source, index++);
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

                                ThrowJsonParserException(JsonParserException.ParserError.InvalidSymbol, index);
                                break;
                            }
                        default:
                            {
                                ThrowJsonParserException(JsonParserException.ParserError.InvalidSymbol, index);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<char> ReadUtf16StringSpan()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_chars);
            if (ReadUtf16IsNullInternal(ref cStart, ref pos, _length))
            {
                return JsonUtf16Constant.NullTerminator;
            }

            var span = ReadUtf16StringSpanInternal(ref cStart, ref pos, _length, out var escapedCharSize);
#if NETSTANDARD2_0 || NET471 || NET451
            return escapedCharSize == 0 ? span : UnescapeUtf16(span, escapedCharSize).AsSpan();
#else
            return escapedCharSize == 0 ? span : UnescapeUtf16(span, escapedCharSize);
#endif
        }

        private static ReadOnlySpan<char> ReadUtf16StringSpanInternal(ref char cStart, ref int pos, uint length, out int escapedCharsSize)
        {
            if ((uint)pos <= length - 2u)
            {
                ref var stringStart = ref Unsafe.Add(ref cStart, pos++);
                if (stringStart != JsonUtf16Constant.String)
                {
                    ThrowJsonParserException(JsonParserException.ParserError.ExpectedDoubleQuote, pos);
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

            throw GetJsonParserException(JsonParserException.ParserError.ExpectedDoubleQuote, pos);
        }

        /// <summary>
        ///     Includes the quotes on each end
        /// </summary>
        private static ReadOnlySpan<char> ReadUtf16StringSpanWithQuotes(ref char cStart, ref int pos, uint length)
        {
            if ((uint)pos <= length - 2u)
            {
                ref var stringStart = ref Unsafe.Add(ref cStart, pos++);
                if (stringStart != JsonUtf16Constant.String)
                {
                    ThrowJsonParserException(JsonParserException.ParserError.ExpectedDoubleQuote, pos);
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

            throw GetJsonParserException(JsonParserException.ParserError.ExpectedDoubleQuote, pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public decimal ReadUtf16Decimal()
        {
            return decimal.Parse(ReadUtf16NumberInternal()
#if NETSTANDARD2_0 || NET471 || NET451
                .ToString()
#endif
                , NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadUtf16IsNull()
        {
            return ReadUtf16IsNullInternal(ref MemoryMarshal.GetReference(_chars), ref _pos, _length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ReadUtf16IsNullInternal(ref char cStart, ref int pos, uint length)
        {
            SkipWhitespaceUtf16(ref cStart, ref pos, length);
            ref var start = ref Unsafe.Add(ref cStart, pos);
            ref var bstart = ref Unsafe.As<char, byte>(ref start);
            if ((uint)pos <= length - 4u && Unsafe.ReadUnaligned<ulong>(ref bstart) == 0x006C006C0075006EUL /* llun */)
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
                ThrowJsonParserException(JsonParserException.ParserError.InvalidSymbol, _pos);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SkipWhitespaceUtf16(ref char cStart, ref int pos, uint length)
        {
            while ((uint)pos < length)
            {
                var c = Unsafe.Add(ref cStart, pos);
                switch (c)
                {
                    case JsonUtf16Constant.Space:
                    case JsonUtf16Constant.Tab:
                    case JsonUtf16Constant.CarriageReturn:
                    case JsonUtf16Constant.LineFeed:
                        {
                            pos++;
                            continue;
                        }
                    default:
                        return;
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadUtf16BeginArrayOrThrow()
        {
            if (!ReadUtf16BeginArray())
            {
                ThrowJsonParserException(JsonParserException.ParserError.ExpectedBeginArray, _pos);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadUtf16BeginArray()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_chars);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            if ((uint)pos < _length && Unsafe.Add(ref cStart, pos) == JsonUtf16Constant.BeginArray)
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
            ref var cStart = ref MemoryMarshal.GetReference(_chars);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            if ((uint)pos < _length && Unsafe.Add(ref cStart, pos) == JsonUtf16Constant.EndArray)
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

                ThrowJsonParserException(JsonParserException.ParserError.ExpectedSeparator, pos);
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadUtf16IsBeginObject()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_chars);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            if (Unsafe.Add(ref cStart, pos) == JsonUtf16Constant.BeginObject)
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
                ThrowJsonParserException(JsonParserException.ParserError.ExpectedBeginObject, _pos);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadUtf16IsEndObject()
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_chars);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            if (Unsafe.Add(ref cStart, pos) == JsonUtf16Constant.EndObject)
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
            ref var cStart = ref MemoryMarshal.GetReference(_chars);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            if (Unsafe.Add(ref cStart, pos) == JsonUtf16Constant.EndArray)
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
            ref var cStart = ref MemoryMarshal.GetReference(_chars);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            if ((uint)pos < _length && Unsafe.Add(ref cStart, pos) == JsonUtf16Constant.EndObject)
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

                ThrowJsonParserException(JsonParserException.ParserError.ExpectedSeparator, pos);
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Version ReadUtf16Version()
        {
            var stringValue = ReadUtf16String();
            if (stringValue == null)
            {
                return default;
            }

            return Version.Parse(stringValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Uri ReadUtf16Uri()
        {
            var stringValue = ReadUtf16String();
            if (stringValue == null)
            {
                return default;
            }

            return new Uri(stringValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadUtf16SymbolOrThrow(char constant)
        {
            if (!ReadUtf16IsSymbol(constant))
            {
                ThrowJsonParserException(JsonParserException.ParserError.InvalidSymbol, _pos);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadUtf16IsSymbol(char constant)
        {
            ref var pos = ref _pos;
            ref var cStart = ref MemoryMarshal.GetReference(_chars);
            SkipWhitespaceUtf16(ref cStart, ref pos, _length);
            if (Unsafe.Add(ref cStart, pos) == constant)
            {
                pos++;
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SkipNextUtf16Segment()
        {
            SkipNextUtf16Segment(ref MemoryMarshal.GetReference(_chars), ref _pos, _length, 0);
        }

        private static void SkipNextUtf16Segment(ref char cStart, ref int pos, uint length, int stack)
        {
            while ((uint)pos < length)
            {
                var token = ReadUtf16NextTokenInternal(ref cStart, ref pos, length);
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
        public void SkipNextUtf16Value(JsonToken token)
        {
            SkipNextUtf16ValueInternal(ref MemoryMarshal.GetReference(_chars), ref _pos, _length, token);
        }

        private static void SkipNextUtf16ValueInternal(ref char cStart, ref int pos, uint length, JsonToken token)
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
                        if (TryFindEndOfUtf16Number(ref cStart, pos, length, out var charsConsumed))
                        {
                            pos += charsConsumed;
                        }

                        break;
                    }
                case JsonToken.String:
                    {
                        if (SkipUtf16String(ref cStart, ref pos, length))
                        {
                            return;
                        }

                        ThrowJsonParserException(JsonParserException.ParserError.ExpectedDoubleQuote, pos);
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
        private static bool TryFindEndOfUtf16Number(ref char cStart, int pos, uint length, out int charsConsumed)
        {
            var i = pos;
            for (; (uint)i < length; i++)
            {
                var c = Unsafe.Add(ref cStart, i);
                if (!IsNumericUtf16Symbol(c))
                {
                    break;
                }
            }

            if (i > pos)
            {
                charsConsumed = i - pos;
                return true;
            }

            charsConsumed = default;
            return false;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFindEndOfUtf16String(ref char cStart, uint length, ref int stringLength, out int escapedCharsSize)
        {
            escapedCharsSize = 0;
            while ((uint)stringLength < length)
            {
                ref var c = ref Unsafe.Add(ref cStart, ++stringLength);
                if (c == JsonUtf16Constant.ReverseSolidus)
                {
                    escapedCharsSize++;
                    c = ref Unsafe.Add(ref cStart, ++stringLength);
                    if (c == 'u')
                    {
                        escapedCharsSize += 4; // add only 4 and not 5 as we still need one unescaped char
                        stringLength += 4;
                    }
                }
                else if (c == JsonUtf16Constant.String)
                {
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
                ThrowJsonParserException(JsonParserException.ParserError.ExpectedDoubleQuote, pos);
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
                ThrowJsonParserException(JsonParserException.ParserError.NestingTooDeep, pos);
            }

            switch (nextToken)
            {
                case JsonToken.Null:
                    {
                        ReadUtf16Null();
                        return null;
                    }
                case JsonToken.False:
                case JsonToken.True:
                    {
                        return ReadUtf16Boolean();
                    }
                case JsonToken.Number:
                    {
                        return new SpanJsonDynamicUtf16Number(ReadUtf16NumberInternal());
                    }
                case JsonToken.String:
                    {
                        var span = ReadUtf16StringSpanWithQuotes(ref MemoryMarshal.GetReference(_chars), ref _pos, _length);
                        return new SpanJsonDynamicUtf16String(span);
                    }
                case JsonToken.BeginObject:
                    {
                        pos++;
                        var count = 0;
                        var dictionary = new Dictionary<string, object>();
                        while (!TryReadUtf16IsEndObjectOrValueSeparator(ref count))
                        {
                            var name = ReadUtf16EscapedName();
                            var value = ReadUtf16Dynamic(stack + 1);
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
                            while (!TryReadUtf16IsEndArrayOrValueSeparator(ref count))
                            {
                                if (count == temp.Length)
                                {
                                    FormatterUtils.GrowArray(ref temp);
                                }

                                temp[count - 1] = ReadUtf16Dynamic(stack + 1);
                            }

                            object[] result;
                            if (count == 0)
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
                        throw GetJsonParserException(JsonParserException.ParserError.EndOfData, pos);
                    }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JsonToken ReadUtf16NextToken()
        {
            return ReadUtf16NextTokenInternal(ref MemoryMarshal.GetReference(_chars), ref _pos, _length);
        }

        public static JsonToken ReadUtf16NextTokenInternal(ref char cStart, ref int pos, uint length)
        {
            SkipWhitespaceUtf16(ref cStart, ref pos, length);
            if ((uint)pos >= length)
            {
                return JsonToken.None;
            }

            var c = Unsafe.Add(ref cStart, pos);
            switch (c)
            {
                case JsonUtf16Constant.BeginObject:
                    return JsonToken.BeginObject;
                case JsonUtf16Constant.EndObject:
                    return JsonToken.EndObject;
                case JsonUtf16Constant.BeginArray:
                    return JsonToken.BeginArray;
                case JsonUtf16Constant.EndArray:
                    return JsonToken.EndArray;
                case JsonUtf16Constant.String:
                    return JsonToken.String;
                case JsonUtf16Constant.True:
                    return JsonToken.True;
                case JsonUtf16Constant.False:
                    return JsonToken.False;
                case JsonUtf16Constant.Null:
                    return JsonToken.Null;
                case JsonUtf16Constant.ValueSeparator:
                    return JsonToken.ValueSeparator;
                case JsonUtf16Constant.NameSeparator:
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