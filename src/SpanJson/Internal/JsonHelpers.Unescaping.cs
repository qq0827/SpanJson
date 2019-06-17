// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpanJson.Internal
{
    internal static partial class JsonHelpers
    {
        public static bool TryGetUnescapedBase64Bytes(in ReadOnlySpan<byte> utf8Source, int idx, out byte[] bytes)
        {
            byte[] unescapedArray = null;

            Span<byte> utf8Unescaped = utf8Source.Length <= JsonConstants.StackallocThreshold ?
                stackalloc byte[utf8Source.Length] :
                (unescapedArray = ArrayPool<byte>.Shared.Rent(utf8Source.Length));

            Unescape(utf8Source, utf8Unescaped, idx, out int written);
            Debug.Assert(written > 0);

            utf8Unescaped = utf8Unescaped.Slice(0, written);
            Debug.Assert(!utf8Unescaped.IsEmpty);

            bool result = TryDecodeBase64InPlace(utf8Unescaped, out bytes);

            if (unescapedArray != null)
            {
                utf8Unescaped.Clear();
                ArrayPool<byte>.Shared.Return(unescapedArray);
            }

            return result;
        }

        //// Reject any invalid UTF-8 data rather than silently replacing.
        //public static readonly UTF8Encoding s_utf8Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        // TODO: Similar to escaping, replace the unescaping logic with publicly shipping APIs from https://github.com/dotnet/corefx/issues/33509
        public static string GetUnescapedString(in ReadOnlySpan<byte> utf8Source, int idx)
        {
            byte[] unescapedArray = null;
            Span<byte> utf8Unescaped = utf8Source.Length <= JsonConstants.StackallocThreshold ?
                stackalloc byte[utf8Source.Length] :
                (unescapedArray = ArrayPool<byte>.Shared.Rent(utf8Source.Length));
            try
            {
                Unescape(utf8Source, utf8Unescaped, idx, out int written);
                Debug.Assert(written > 0);

                utf8Unescaped = utf8Unescaped.Slice(0, written);
                Debug.Assert(!utf8Unescaped.IsEmpty);

                return TextEncodings.Utf8.GetString(utf8Unescaped);
            }
            finally
            {
                if (unescapedArray != null)
                {
                    //utf8Unescaped.Clear();
                    ArrayPool<byte>.Shared.Return(unescapedArray);
                }
            }
        }

        public static bool UnescapeAndCompare(in ReadOnlySpan<byte> utf8Source, ReadOnlySpan<byte> other)
        {
            Debug.Assert(utf8Source.Length >= other.Length && utf8Source.Length / JsonConstants.MaxExpansionFactorWhileEscaping <= other.Length);

            byte[] unescapedArray = null;

            Span<byte> utf8Unescaped = utf8Source.Length <= JsonConstants.StackallocThreshold ?
                stackalloc byte[utf8Source.Length] :
                (unescapedArray = ArrayPool<byte>.Shared.Rent(utf8Source.Length));

            Unescape(utf8Source, utf8Unescaped, 0, out int written);
            Debug.Assert(written > 0);

            utf8Unescaped = utf8Unescaped.Slice(0, written);
            Debug.Assert(!utf8Unescaped.IsEmpty);

            bool result = other.SequenceEqual(utf8Unescaped);

            if (unescapedArray != null)
            {
                //utf8Unescaped.Clear();
                ArrayPool<byte>.Shared.Return(unescapedArray);
            }

            return result;
        }

        public static bool UnescapeAndCompare(in ReadOnlySequence<byte> utf8Source, ReadOnlySpan<byte> other)
        {
            Debug.Assert(!utf8Source.IsSingleSegment);
            Debug.Assert(utf8Source.Length >= other.Length && utf8Source.Length / JsonConstants.MaxExpansionFactorWhileEscaping <= other.Length);

            byte[] escapedArray = null;
            byte[] unescapedArray = null;

            int length = checked((int)utf8Source.Length);

            Span<byte> utf8Unescaped = length <= JsonConstants.StackallocThreshold ?
                stackalloc byte[length] :
                (unescapedArray = ArrayPool<byte>.Shared.Rent(length));

            Span<byte> utf8Escaped = length <= JsonConstants.StackallocThreshold ?
                stackalloc byte[length] :
                (escapedArray = ArrayPool<byte>.Shared.Rent(length));

            utf8Source.CopyTo(utf8Escaped);
            utf8Escaped = utf8Escaped.Slice(0, length);

            Unescape(utf8Escaped, utf8Unescaped, 0, out int written);
            Debug.Assert(written > 0);

            utf8Unescaped = utf8Unescaped.Slice(0, written);
            Debug.Assert(!utf8Unescaped.IsEmpty);

            bool result = other.SequenceEqual(utf8Unescaped);

            if (unescapedArray != null)
            {
                Debug.Assert(escapedArray != null);
                //utf8Unescaped.Clear();
                ArrayPool<byte>.Shared.Return(unescapedArray);
                //utf8Escaped.Clear();
                ArrayPool<byte>.Shared.Return(escapedArray);
            }

            return result;
        }

        public static bool TryDecodeBase64InPlace(Span<byte> utf8Unescaped, out byte[] bytes)
        {
            OperationStatus status = Base64.DecodeFromUtf8InPlace(utf8Unescaped, out int bytesWritten);
            if (status != OperationStatus.Done)
            {
                bytes = null;
                return false;
            }
            bytes = utf8Unescaped.Slice(0, bytesWritten).ToArray();
            return true;
        }

        public static bool TryDecodeBase64(in ReadOnlySpan<byte> utf8Unescaped, out byte[] bytes)
        {
            byte[] pooledArray = null;

            Span<byte> byteSpan = utf8Unescaped.Length <= JsonConstants.StackallocThreshold ?
                stackalloc byte[utf8Unescaped.Length] :
                (pooledArray = ArrayPool<byte>.Shared.Rent(utf8Unescaped.Length));

            OperationStatus status = Base64.DecodeFromUtf8(utf8Unescaped, byteSpan, out int bytesConsumed, out int bytesWritten);

            if (status != OperationStatus.Done)
            {
                bytes = null;

                if (pooledArray != null)
                {
                    //byteSpan.Clear();
                    ArrayPool<byte>.Shared.Return(pooledArray);
                }

                return false;
            }
            Debug.Assert(bytesConsumed == utf8Unescaped.Length);

            bytes = byteSpan.Slice(0, bytesWritten).ToArray();

            if (pooledArray != null)
            {
                //byteSpan.Clear();
                ArrayPool<byte>.Shared.Return(pooledArray);
            }

            return true;
        }

        internal static void Unescape(in ReadOnlySpan<byte> utf8Source, Span<byte> destination, int index, out int written)
        {
            utf8Source.Slice(0, index).CopyTo(destination);
            written = index;

            var from = index;
            var offset = (IntPtr)index;
            var nSrcLen = (uint)utf8Source.Length;
            ref byte sourceSpace = ref MemoryMarshal.GetReference(utf8Source);
            ref byte destSpace = ref MemoryMarshal.GetReference(destination);
            while ((uint)index < nSrcLen)
            {
                uint current = Unsafe.AddByteOffset(ref sourceSpace, offset);
                if (current == JsonUtf8Constant.ReverseSolidus)
                {
                    // We copy everything up to the escaped char as utf8 to the string
                    var sliceLength = index - from;
                    utf8Source.Slice(from, sliceLength).CopyTo(destination.Slice(written));
                    written += sliceLength;
                    current = Unsafe.AddByteOffset(ref sourceSpace, offset + 1);
                    index += 2;
                    byte unescaped = default;
                    switch (current)
                    {
                        case JsonUtf8Constant.DoubleQuote:
                            unescaped = JsonUtf8Constant.DoubleQuote;
                            break;
                        case JsonUtf8Constant.ReverseSolidus:
                            unescaped = JsonUtf8Constant.ReverseSolidus;
                            break;
                        case JsonUtf8Constant.Solidus:
                            unescaped = JsonUtf8Constant.Solidus;
                            break;
                        case 'b':
                            unescaped = (byte)'\b';
                            break;
                        case 'f':
                            unescaped = (byte)'\f';
                            break;
                        case 'n':
                            unescaped = (byte)'\n';
                            break;
                        case 'r':
                            unescaped = (byte)'\r';
                            break;
                        case 't':
                            unescaped = (byte)'\t';
                            break;
                        case (byte)'u':
                            // The source is known to be valid JSON, and hence if we see a \u, it is guaranteed to have 4 hex digits following it
                            // Otherwise, the Utf8JsonReader would have alreayd thrown an exception.
                            Debug.Assert(nSrcLen >= ((uint)index + 4u));

                            bool result = Utf8Parser.TryParse(utf8Source.Slice(index, 4), out int scalar, out int bytesConsumed, 'x');
                            if (!result)
                            {
                                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.InvalidSymbol, index);
                            }
                            Debug.Assert(bytesConsumed == 4);
                            index += bytesConsumed;

                            if (IsInRangeInclusive((uint)scalar, JsonConstants.HighSurrogateStartValue, JsonConstants.LowSurrogateEndValue))
                            {
                                // The first hex value cannot be a low surrogate.
                                if (scalar >= JsonConstants.LowSurrogateStartValue)
                                {
                                    ThrowHelper.ThrowInvalidOperationException_ReadInvalidUTF16(scalar);
                                }

                                Debug.Assert(IsInRangeInclusive((uint)scalar, JsonConstants.HighSurrogateStartValue, JsonConstants.HighSurrogateEndValue));

                                index += 2; // Skip the next \u

                                // We must have a low surrogate following a high surrogate.
                                if (nSrcLen < ((uint)index + 4u) ||
                                    Unsafe.AddByteOffset(ref sourceSpace, offset + 6) != '\\' ||
                                    Unsafe.AddByteOffset(ref sourceSpace, offset + 7) != 'u')
                                {
                                    ThrowHelper.ThrowInvalidOperationException_ReadInvalidUTF16();
                                }

                                // The source is known to be valid JSON, and hence if we see a \u, it is guaranteed to have 4 hex digits following it
                                // Otherwise, the Utf8JsonReader would have alreayd thrown an exception.
                                result = Utf8Parser.TryParse(utf8Source.Slice(index, 4), out int lowSurrogate, out bytesConsumed, 'x');
                                if (!result)
                                {
                                    ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.InvalidSymbol, index);
                                }
                                Debug.Assert(bytesConsumed == 4);

                                // If the first hex value is a high surrogate, the next one must be a low surrogate.
                                if (!IsInRangeInclusive((uint)lowSurrogate, JsonConstants.LowSurrogateStartValue, JsonConstants.LowSurrogateEndValue))
                                {
                                    ThrowHelper.ThrowInvalidOperationException_ReadInvalidUTF16(lowSurrogate);
                                }

                                index += bytesConsumed;

                                // To find the unicode scalar:
                                // (0x400 * (High surrogate - 0xD800)) + Low surrogate - 0xDC00 + 0x10000
                                scalar = (JsonConstants.BitShiftBy10 * (scalar - JsonConstants.HighSurrogateStartValue))
                                    + (lowSurrogate - JsonConstants.LowSurrogateStartValue)
                                    + JsonConstants.UnicodePlane01StartValue;

                                offset += 12;
                            }
                            else
                            {
                                offset += 6;
                            }

                            EncodeToUtf8Bytes((uint)scalar, ref destSpace, ref written);

                            from = index;
                            continue;

                        default:
                            ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.InvalidSymbol, index);
                            break;
                    }

                    destination[written++] = unescaped;
                    offset += 2;
                    from = index;
                }
                else
                {
                    index++;
                    offset += 1;
                }
            }

            if ((uint)from < nSrcLen) // still data to copy
            {
                var remaining = utf8Source.Slice(from);
                remaining.CopyTo(destination.Slice(written));
                written += remaining.Length;
            }
        }

        /// <summary>Copies the UTF-8 code unit representation of this scalar to an output buffer.
        /// The buffer must be large enough to hold the required number of <see cref="byte"/>s.</summary>
        private static void EncodeToUtf8Bytes(uint scalar, Span<byte> utf8Destination, out int bytesWritten)
        {
            Debug.Assert(JsonHelpers.IsValidUnicodeScalar(scalar));
            Debug.Assert(utf8Destination.Length >= 4);

            if (scalar < 0x80U)
            {
                // Single UTF-8 code unit
                utf8Destination[0] = (byte)scalar;
                bytesWritten = 1;
            }
            else if (scalar < 0x800U)
            {
                // Two UTF-8 code units
                utf8Destination[0] = (byte)(0xC0U | (scalar >> 6));
                utf8Destination[1] = (byte)(0x80U | (scalar & 0x3FU));
                bytesWritten = 2;
            }
            else if (scalar < 0x10000U)
            {
                // Three UTF-8 code units
                utf8Destination[0] = (byte)(0xE0U | (scalar >> 12));
                utf8Destination[1] = (byte)(0x80U | ((scalar >> 6) & 0x3FU));
                utf8Destination[2] = (byte)(0x80U | (scalar & 0x3FU));
                bytesWritten = 3;
            }
            else
            {
                // Four UTF-8 code units
                utf8Destination[0] = (byte)(0xF0U | (scalar >> 18));
                utf8Destination[1] = (byte)(0x80U | ((scalar >> 12) & 0x3FU));
                utf8Destination[2] = (byte)(0x80U | ((scalar >> 6) & 0x3FU));
                utf8Destination[3] = (byte)(0x80U | (scalar & 0x3FU));
                bytesWritten = 4;
            }
        }

        /// <summary>Copies the UTF-8 code unit representation of this scalar to an output buffer.
        /// The buffer must be large enough to hold the required number of <see cref="byte"/>s.</summary>
        private static void EncodeToUtf8Bytes(uint scalar, ref byte utf8Destination, ref int written)
        {
            Debug.Assert(IsValidUnicodeScalar(scalar));

            IntPtr offset = (IntPtr)written;
            if (scalar < 0x80U)
            {
                // Single UTF-8 code unit
                Unsafe.AddByteOffset(ref utf8Destination, offset) = (byte)scalar;
                written++;
            }
            else if (scalar < 0x800U)
            {
                // Two UTF-8 code units
                Unsafe.AddByteOffset(ref utf8Destination, offset) = (byte)(0xC0U | (scalar >> 6));
                Unsafe.AddByteOffset(ref utf8Destination, offset + 1) = (byte)(0x80U | (scalar & 0x3FU));
                written += 2;
            }
            else if (scalar < 0x10000U)
            {
                // Three UTF-8 code units
                Unsafe.AddByteOffset(ref utf8Destination, offset) = (byte)(0xE0U | (scalar >> 12));
                Unsafe.AddByteOffset(ref utf8Destination, offset + 1) = (byte)(0x80U | ((scalar >> 6) & 0x3FU));
                Unsafe.AddByteOffset(ref utf8Destination, offset + 2) = (byte)(0x80U | (scalar & 0x3FU));
                written += 3;
            }
            else
            {
                // Four UTF-8 code units
                Unsafe.AddByteOffset(ref utf8Destination, offset) = (byte)(0xF0U | (scalar >> 18));
                Unsafe.AddByteOffset(ref utf8Destination, offset + 1) = (byte)(0x80U | ((scalar >> 12) & 0x3FU));
                Unsafe.AddByteOffset(ref utf8Destination, offset + 2) = (byte)(0x80U | ((scalar >> 6) & 0x3FU));
                Unsafe.AddByteOffset(ref utf8Destination, offset + 3) = (byte)(0x80U | (scalar & 0x3FU));
                written += 4;
            }
        }
    }
}
