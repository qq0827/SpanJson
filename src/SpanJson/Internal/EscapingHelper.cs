// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Largely based on https://github.com/dotnet/corefx/blob/8135319caa7e457ed61053ca1418313b88057b51/src/System.Text.Json/src/System/Text/Json/Writer/JsonWriterHelper.Transcoding.cs#L12

using System;
using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using SpanJson.Helpers;

namespace SpanJson.Internal
{
    internal static partial class EscapingHelper
    {
        // A simple lookup table for converting numbers to hex.
        private const string HexTableLower = "0123456789abcdef";
        private const string HexFormatString = "x4";
        private static readonly StandardFormat s_hexStandardFormat = new StandardFormat('x', 4);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMaxEscapedLength(int textLength, int firstIndexToEscape)
        {
            Debug.Assert(textLength > 0);
            Debug.Assert(firstIndexToEscape >= 0 && firstIndexToEscape < textLength);
            return firstIndexToEscape + JsonConstants.MaxExpansionFactorWhileEscaping * (textLength - firstIndexToEscape);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NeedsEscaping(char value, StringEscapeHandling escapeHandling)
        {
            switch (escapeHandling)
            {
                case StringEscapeHandling.EscapeNonAscii:
                    return NonAscii.NeedsEscaping(value);
                case StringEscapeHandling.Default:
                default:
                    return Default.NeedsEscaping(value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NeedsEscaping(in ReadOnlySpan<byte> value, StringEscapeHandling escapeHandling)
        {
            switch (escapeHandling)
            {
                case StringEscapeHandling.EscapeNonAscii:
                    return NonAscii.NeedsEscaping(value);
                case StringEscapeHandling.Default:
                default:
                    return Default.NeedsEscaping(value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NeedsEscaping(in ReadOnlySpan<char> value, StringEscapeHandling escapeHandling)
        {
            switch (escapeHandling)
            {
                case StringEscapeHandling.EscapeNonAscii:
                    return NonAscii.NeedsEscaping(value);
                case StringEscapeHandling.Default:
                default:
                    return Default.NeedsEscaping(value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EscapeString(in ReadOnlySpan<byte> value, Span<byte> destination, StringEscapeHandling escapeHandling, int indexOfFirstByteToEscape, out int written)
        {
            switch (escapeHandling)
            {
                case StringEscapeHandling.EscapeNonAscii:
                    NonAscii.EscapeString(value, destination, indexOfFirstByteToEscape, out written);
                    break;
                case StringEscapeHandling.Default:
                default:
                    Default.EscapeString(value, destination, indexOfFirstByteToEscape, out written);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EscapeString(in ReadOnlySpan<char> value, Span<char> destination, StringEscapeHandling escapeHandling, int indexOfFirstByteToEscape, out int written)
        {
            switch (escapeHandling)
            {
                case StringEscapeHandling.EscapeNonAscii:
                    NonAscii.EscapeString(value, destination, indexOfFirstByteToEscape, out written);
                    break;
                case StringEscapeHandling.Default:
                default:
                    Default.EscapeString(value, destination, indexOfFirstByteToEscape, out written);
                    break;
            }
        }

        public static string EscapeString(string input, StringEscapeHandling escapeHandling)
        {
            ReadOnlySpan<char> source = input.AsSpan();
            int firstEscapeIndex;
            switch (escapeHandling)
            {
                case StringEscapeHandling.EscapeNonAscii:
                    firstEscapeIndex = NonAscii.NeedsEscaping(source);
                    if ((uint)firstEscapeIndex > JsonSharedConstant.TooBigOrNegative) // -1
                    {
                        return input;
                    }
                    else
                    {
                        var length = GetMaxEscapedLength(source.Length, firstEscapeIndex);
                        var tempArray = ArrayPool<char>.Shared.Rent(length);
                        try
                        {
                            Span<char> escapedName = tempArray;
                            NonAscii.EscapeString(source, escapedName, firstEscapeIndex, out int written);

                            return escapedName.Slice(0, written).ToString();
                        }
                        finally
                        {
                            ArrayPool<char>.Shared.Return(tempArray);
                        }
                    }

                case StringEscapeHandling.Default:
                default:
                    firstEscapeIndex = Default.NeedsEscaping(source);
                    if ((uint)firstEscapeIndex > JsonSharedConstant.TooBigOrNegative) // -1
                    {
                        return input;
                    }
                    else
                    {
                        var length = GetMaxEscapedLength(source.Length, firstEscapeIndex);
                        var tempArray = ArrayPool<char>.Shared.Rent(length);
                        try
                        {
                            Span<char> escapedName = tempArray;
                            Default.EscapeString(source, escapedName, firstEscapeIndex, out int written);

                            return escapedName.Slice(0, written).ToString();
                        }
                        finally
                        {
                            ArrayPool<char>.Shared.Return(tempArray);
                        }
                    }
            }
        }

        public static void EscapeChar(int value, ref char destSpace, ref int written)
        {
            Unsafe.Add(ref destSpace, written++) = '\\';
            switch (value)
            {
                case JsonConstants.Quote:
                    Unsafe.Add(ref destSpace, written++) = '"';
                    break;
                case JsonConstants.Slash:
                    Unsafe.Add(ref destSpace, written++) = '/';
                    break;

                case JsonConstants.LineFeed:
                    Unsafe.Add(ref destSpace, written++) = 'n';
                    break;
                case JsonConstants.CarriageReturn:
                    Unsafe.Add(ref destSpace, written++) = 'r';
                    break;
                case JsonConstants.Tab:
                    Unsafe.Add(ref destSpace, written++) = 't';
                    break;
                case JsonConstants.BackSlash:
                    Unsafe.Add(ref destSpace, written++) = '\\';
                    break;
                case JsonConstants.BackSpace:
                    Unsafe.Add(ref destSpace, written++) = 'b';
                    break;
                case JsonConstants.FormFeed:
                    Unsafe.Add(ref destSpace, written++) = 'f';
                    break;
                default:
                    Unsafe.Add(ref destSpace, written++) = 'u';
                    WriteHex(value, ref destSpace, ref written);
                    break;
            }
        }

        public static void EscapeChar(int value, ref byte destSpace, ref int written)
        {
            IntPtr offset = (IntPtr)written;
            Unsafe.AddByteOffset(ref destSpace, offset) = (byte)'\\';
            switch (value)
            {
                case JsonConstants.Quote:
                    Unsafe.AddByteOffset(ref destSpace, offset + 1) = (byte)'"';
                    written += 2;
                    break;
                case JsonConstants.Slash:
                    Unsafe.AddByteOffset(ref destSpace, offset + 1) = (byte)'/';
                    written += 2;
                    break;

                case JsonConstants.LineFeed:
                    Unsafe.AddByteOffset(ref destSpace, offset + 1) = (byte)'n';
                    written += 2;
                    break;
                case JsonConstants.CarriageReturn:
                    Unsafe.AddByteOffset(ref destSpace, offset + 1) = (byte)'r';
                    written += 2;
                    break;
                case JsonConstants.Tab:
                    Unsafe.AddByteOffset(ref destSpace, offset + 1) = (byte)'t';
                    written += 2;
                    break;
                case JsonConstants.BackSlash:
                    Unsafe.AddByteOffset(ref destSpace, offset + 1) = (byte)'\\';
                    written += 2;
                    break;
                case JsonConstants.BackSpace:
                    Unsafe.AddByteOffset(ref destSpace, offset + 1) = (byte)'b';
                    written += 2;
                    break;
                case JsonConstants.FormFeed:
                    Unsafe.AddByteOffset(ref destSpace, offset + 1) = (byte)'f';
                    written += 2;
                    break;
                default:
                    Unsafe.AddByteOffset(ref destSpace, offset + 1) = (byte)'u';
                    WriteHex(value, ref destSpace, offset);
                    written += 6;
                    break;
            }
        }

        private static bool EscapeNextBytes(ref byte sourceSpace, ref int consumed, uint remaining,
            Span<byte> destination, ref byte destSpace, ref int written)
        {
            SequenceValidity status = PeekFirstSequence(ref sourceSpace, consumed, remaining, out int numBytesConsumed, out int scalar);
            if (status != SequenceValidity.WellFormed) { return false; }

            consumed += numBytesConsumed;

            IntPtr offset = (IntPtr)written;
            Unsafe.AddByteOffset(ref destSpace, offset) = (byte)'\\';
            switch (scalar)
            {
                case JsonConstants.Quote:
                    Unsafe.AddByteOffset(ref destSpace, offset + 1) = (byte)'"';
                    written += 2;
                    break;
                case JsonConstants.Slash:
                    Unsafe.AddByteOffset(ref destSpace, offset + 1) = (byte)'/';
                    written += 2;
                    break;

                case JsonConstants.LineFeed:
                    Unsafe.AddByteOffset(ref destSpace, offset + 1) = (byte)'n';
                    written += 2;
                    break;
                case JsonConstants.CarriageReturn:
                    Unsafe.AddByteOffset(ref destSpace, offset + 1) = (byte)'r';
                    written += 2;
                    break;
                case JsonConstants.Tab:
                    Unsafe.AddByteOffset(ref destSpace, offset + 1) = (byte)'t';
                    written += 2;
                    break;
                case JsonConstants.BackSlash:
                    Unsafe.AddByteOffset(ref destSpace, offset + 1) = (byte)'\\';
                    written += 2;
                    break;
                case JsonConstants.BackSpace:
                    Unsafe.AddByteOffset(ref destSpace, offset + 1) = (byte)'b';
                    written += 2;
                    break;
                case JsonConstants.FormFeed:
                    Unsafe.AddByteOffset(ref destSpace, offset + 1) = (byte)'f';
                    written += 2;
                    break;
                default:
                    Unsafe.AddByteOffset(ref destSpace, offset + 1) = (byte)'u';
                    written += 2;
                    if (scalar < JsonConstants.UnicodePlane01StartValue)
                    {
                        bool result = Utf8Formatter.TryFormat(scalar, destination.Slice(written), out int bytesWritten, format: s_hexStandardFormat);
                        Debug.Assert(result);
                        Debug.Assert(bytesWritten == 4);
                        written += bytesWritten;
                    }
                    else
                    {
                        // Divide by 0x400 to shift right by 10 in order to find the surrogate pairs from the scalar
                        // High surrogate = ((scalar -  0x10000) / 0x400) + D800
                        // Low surrogate = ((scalar -  0x10000) % 0x400) + DC00
                        int quotient = Math.DivRem(scalar - JsonConstants.UnicodePlane01StartValue, JsonConstants.BitShiftBy10, out int remainder);
                        int firstChar = quotient + JsonConstants.HighSurrogateStartValue;
                        int nextChar = remainder + JsonConstants.LowSurrogateStartValue;
                        bool result = Utf8Formatter.TryFormat(firstChar, destination.Slice(written), out int bytesWritten, format: s_hexStandardFormat);
                        Debug.Assert(result);
                        Debug.Assert(bytesWritten == 4);
                        written += bytesWritten;
                        offset = (IntPtr)written;
                        Unsafe.AddByteOffset(ref destSpace, offset) = (byte)'\\';
                        Unsafe.AddByteOffset(ref destSpace, offset + 1) = (byte)'u';
                        written += 2;
                        result = Utf8Formatter.TryFormat(nextChar, destination.Slice(written), out bytesWritten, format: s_hexStandardFormat);
                        Debug.Assert(result);
                        Debug.Assert(bytesWritten == 4);
                        written += bytesWritten;
                    }
                    break;
            }
            return true;
        }

        private static bool IsAsciiValue(byte value) => value < 0x80u ? true : false;

        /// <summary>Returns <see langword="true"/> if <paramref name="value"/> is a UTF-8 continuation byte.
        /// A UTF-8 continuation byte is a byte whose value is in the range 0x80-0xBF, inclusive.</summary>
        private static bool IsUtf8ContinuationByte(byte value) => (value & 0xC0) == 0x80 ? true : false;

        /// <summary>Returns <see langword="true"/> if the low word of <paramref name="char"/> is a UTF-16 surrogate.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsLowWordSurrogate(uint @char) => (@char & 0xF800U) == 0xD800U ? true : false;

        // We can't use the type Rune since it is not available on netstandard2.0
        // To avoid extensive ifdefs and for simplicity, just using an int to reprepsent the scalar value, instead.
        private static SequenceValidity PeekFirstSequence(ref byte sourceSpace, int consumed, uint remaining, out int numBytesConsumed, out int rune)
        {
            // This method is implemented to match the behavior of System.Text.Encoding.UTF8 in terms of
            // how many bytes it consumes when reporting invalid sequences. The behavior is as follows:
            //
            // - Some bytes are *always* invalid (ranges [ C0..C1 ] and [ F5..FF ]), and when these
            //   are encountered it's an invalid sequence of length 1.
            //
            // - Multi-byte sequences which are overlong are reported as an invalid sequence of length 2,
            //   since per the Unicode Standard Table 3-7 it's always possible to tell these by the second byte.
            //   Exception: Sequences which begin with [ C0..C1 ] are covered by the above case, thus length 1.
            //
            // - Multi-byte sequences which are improperly terminated (no continuation byte when one is
            //   expected) are reported as invalid sequences up to and including the last seen continuation byte.

            Debug.Assert(JsonHelpers.IsValidUnicodeScalar(ReplacementChar));
            rune = ReplacementChar;

            IntPtr offset = (IntPtr)consumed;

            if (0u >= remaining)
            {
                // No data to peek at
                numBytesConsumed = 0;
                return SequenceValidity.Empty;
            }

            byte firstByte = Unsafe.AddByteOffset(ref sourceSpace, offset);

            if (IsAsciiValue(firstByte))
            {
                // ASCII byte = well-formed one-byte sequence.
                Debug.Assert(JsonHelpers.IsValidUnicodeScalar(firstByte));
                rune = firstByte;
                numBytesConsumed = 1;
                return SequenceValidity.WellFormed;
            }

            if (!JsonHelpers.IsInRangeInclusive(firstByte, (byte)0xC2U, (byte)0xF4U))
            {
                // Standalone continuation byte or "always invalid" byte = ill-formed one-byte sequence.
                goto InvalidOneByteSequence;
            }

            // At this point, we know we're working with a multi-byte sequence,
            // and we know that at least the first byte is potentially valid.

            if (remaining < 2u)
            {
                // One byte of an incomplete multi-byte sequence.
                goto OneByteOfIncompleteMultiByteSequence;
            }

            byte secondByte = Unsafe.AddByteOffset(ref sourceSpace, offset + 1);

            if (!IsUtf8ContinuationByte(secondByte))
            {
                // One byte of an improperly terminated multi-byte sequence.
                goto InvalidOneByteSequence;
            }

            if (firstByte < (byte)0xE0U)
            {
                // Well-formed two-byte sequence.
                uint scalar = (((uint)firstByte & 0x1FU) << 6) | ((uint)secondByte & 0x3FU);
                Debug.Assert(JsonHelpers.IsValidUnicodeScalar(scalar));
                rune = (int)scalar;
                numBytesConsumed = 2;
                return SequenceValidity.WellFormed;
            }

            if (firstByte < (byte)0xF0U)
            {
                // Start of a three-byte sequence.
                // Need to check for overlong or surrogate sequences.

                uint scalar = (((uint)firstByte & 0x0FU) << 12) | (((uint)secondByte & 0x3FU) << 6);
                if (scalar < 0x800U || IsLowWordSurrogate(scalar))
                {
                    goto OverlongOutOfRangeOrSurrogateSequence;
                }

                // At this point, we have a valid two-byte start of a three-byte sequence.

                if (remaining < 3u)
                {
                    // Two bytes of an incomplete three-byte sequence.
                    goto TwoBytesOfIncompleteMultiByteSequence;
                }
                else
                {
                    byte thirdByte = Unsafe.AddByteOffset(ref sourceSpace, offset + 2);
                    if (IsUtf8ContinuationByte(thirdByte))
                    {
                        // Well-formed three-byte sequence.
                        scalar |= (uint)thirdByte & 0x3FU;
                        Debug.Assert(JsonHelpers.IsValidUnicodeScalar(scalar));
                        rune = (int)scalar;
                        numBytesConsumed = 3;
                        return SequenceValidity.WellFormed;
                    }
                    else
                    {
                        // Two bytes of improperly terminated multi-byte sequence.
                        goto InvalidTwoByteSequence;
                    }
                }
            }

            {
                // Start of four-byte sequence.
                // Need to check for overlong or out-of-range sequences.

                uint scalar = (((uint)firstByte & 0x07U) << 18) | (((uint)secondByte & 0x3FU) << 12);
                Debug.Assert(JsonHelpers.IsValidUnicodeScalar(scalar));
                if (!JsonHelpers.IsInRangeInclusive(scalar, 0x10000U, 0x10FFFFU))
                {
                    goto OverlongOutOfRangeOrSurrogateSequence;
                }

                // At this point, we have a valid two-byte start of a four-byte sequence.

                if (remaining < 3u)
                {
                    // Two bytes of an incomplete four-byte sequence.
                    goto TwoBytesOfIncompleteMultiByteSequence;
                }
                else
                {
                    byte thirdByte = Unsafe.AddByteOffset(ref sourceSpace, offset + 2);
                    if (IsUtf8ContinuationByte(thirdByte))
                    {
                        // Valid three-byte start of a four-byte sequence.

                        if (remaining < 4u)
                        {
                            // Three bytes of an incomplete four-byte sequence.
                            goto ThreeBytesOfIncompleteMultiByteSequence;
                        }
                        else
                        {
                            byte fourthByte = Unsafe.AddByteOffset(ref sourceSpace, offset + 3);
                            if (IsUtf8ContinuationByte(fourthByte))
                            {
                                // Well-formed four-byte sequence.
                                scalar |= (((uint)thirdByte & 0x3FU) << 6) | ((uint)fourthByte & 0x3FU);
                                Debug.Assert(JsonHelpers.IsValidUnicodeScalar(scalar));
                                rune = (int)scalar;
                                numBytesConsumed = 4;
                                return SequenceValidity.WellFormed;
                            }
                            else
                            {
                                // Three bytes of an improperly terminated multi-byte sequence.
                                goto InvalidThreeByteSequence;
                            }
                        }
                    }
                    else
                    {
                        // Two bytes of improperly terminated multi-byte sequence.
                        goto InvalidTwoByteSequence;
                    }
                }
            }

        // Everything below here is error handling.

        InvalidOneByteSequence:
            numBytesConsumed = 1;
            return SequenceValidity.Invalid;

        InvalidTwoByteSequence:
        OverlongOutOfRangeOrSurrogateSequence:
            numBytesConsumed = 2;
            return SequenceValidity.Invalid;

        InvalidThreeByteSequence:
            numBytesConsumed = 3;
            return SequenceValidity.Invalid;

        OneByteOfIncompleteMultiByteSequence:
            numBytesConsumed = 1;
            return SequenceValidity.Incomplete;

        TwoBytesOfIncompleteMultiByteSequence:
            numBytesConsumed = 2;
            return SequenceValidity.Incomplete;

        ThreeBytesOfIncompleteMultiByteSequence:
            numBytesConsumed = 3;
            return SequenceValidity.Incomplete;
        }

        private static void EscapeNextChars(ref char sourceSpace, uint srcLength, int firstChar, ref char destSpace, ref int consumed, ref int written)
        {
            int nextChar = -1;
            if (JsonHelpers.IsInRangeInclusive(firstChar, JsonConstants.HighSurrogateStartValue, JsonConstants.LowSurrogateEndValue))
            {
                consumed++;
                if (srcLength <= (uint)consumed || firstChar >= JsonConstants.LowSurrogateStartValue)
                {
                    ThrowHelper.ThrowArgumentException_InvalidUTF16(firstChar);
                }

                nextChar = Unsafe.Add(ref sourceSpace, consumed);
                if (!JsonHelpers.IsInRangeInclusive(nextChar, JsonConstants.LowSurrogateStartValue, JsonConstants.LowSurrogateEndValue))
                {
                    ThrowHelper.ThrowArgumentException_InvalidUTF16(nextChar);
                }
            }

            Unsafe.Add(ref destSpace, written++) = '\\';
            switch (firstChar)
            {
                case JsonConstants.Quote:
                    Unsafe.Add(ref destSpace, written++) = '"';
                    break;
                case JsonConstants.Slash:
                    Unsafe.Add(ref destSpace, written++) = '/';
                    break;

                case JsonConstants.LineFeed:
                    Unsafe.Add(ref destSpace, written++) = 'n';
                    break;
                case JsonConstants.CarriageReturn:
                    Unsafe.Add(ref destSpace, written++) = 'r';
                    break;
                case JsonConstants.Tab:
                    Unsafe.Add(ref destSpace, written++) = 't';
                    break;
                case JsonConstants.BackSlash:
                    Unsafe.Add(ref destSpace, written++) = '\\';
                    break;
                case JsonConstants.BackSpace:
                    Unsafe.Add(ref destSpace, written++) = 'b';
                    break;
                case JsonConstants.FormFeed:
                    Unsafe.Add(ref destSpace, written++) = 'f';
                    break;
                default:
                    Unsafe.Add(ref destSpace, written++) = 'u';
                    WriteHex(firstChar, ref destSpace, ref written);
                    if (nextChar != -1)
                    {
                        Unsafe.Add(ref destSpace, written++) = '\\';
                        Unsafe.Add(ref destSpace, written++) = 'u';
                        WriteHex(nextChar, ref destSpace, ref written);
                    }
                    break;
            }
        }

        /// <summary>A scalar that represents the Unicode replacement character U+FFFD.</summary>
        private const int ReplacementChar = 0xFFFD;

        private static void WriteHex(int value, ref char destSpace, ref int written)
        {
            Unsafe.Add(ref destSpace, written++) = (char)Int32LsbToHexDigit(value >> 12);
            Unsafe.Add(ref destSpace, written++) = (char)Int32LsbToHexDigit((int)((value >> 8) & 0xFU));
            Unsafe.Add(ref destSpace, written++) = (char)Int32LsbToHexDigit((int)((value >> 4) & 0xFU));
            Unsafe.Add(ref destSpace, written++) = (char)Int32LsbToHexDigit((int)(value & 0xFU));
        }

        private static void WriteHex(int value, ref byte destSpace, IntPtr offset)
        {
            Unsafe.AddByteOffset(ref destSpace, offset + 2) = Int32LsbToHexDigit(value >> 12);
            Unsafe.AddByteOffset(ref destSpace, offset + 3) = Int32LsbToHexDigit((int)((value >> 8) & 0xFU));
            Unsafe.AddByteOffset(ref destSpace, offset + 4) = Int32LsbToHexDigit((int)((value >> 4) & 0xFU));
            Unsafe.AddByteOffset(ref destSpace, offset + 5) = Int32LsbToHexDigit((int)(value & 0xFU));
        }

        /// <summary>
        /// Converts a number 0 - 15 to its associated hex character '0' - 'f' as byte.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte Int32LsbToHexDigit(int value)
        {
            Debug.Assert(value < 16);
            return (byte)((value < 10) ? ('0' + value) : ('a' + (value - 10)));
        }
    }
}
