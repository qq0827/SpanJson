// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Largely based on https://github.com/dotnet/corefx/blob/8135319caa7e457ed61053ca1418313b88057b51/src/System.Text.Json/src/System/Text/Json/Writer/JsonWriterHelper.Transcoding.cs#L12

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Encodings.Web;

namespace SpanJson.Internal
{
    partial class EscapingHelper
    {
        public static partial class NonAscii
        {
            // Only allow ASCII characters between ' ' (0x20) and '~' (0x7E), inclusively,
            // but exclude characters that need to be escaped as hex: '"', '\'', '&', '+', '<', '>', '`'
            // and exclude characters that need to be escaped by adding a backslash: '\n', '\r', '\t', '\\', '\b', '\f'
            //
            // non-zero = allowed, 0 = disallowed
            private static ReadOnlySpan<byte> AllowList => new byte[byte.MaxValue + 1]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // U+0000..U+000F
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // U+0010..U+001F
                1, 1, 0, 1, 1, 1, 0, 0, 1, 1, 1, 0, 1, 1, 1, 1, // U+0020..U+002F
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 0, 1, // U+0030..U+003F
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // U+0040..U+004F
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, // U+0050..U+005F
                0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // U+0060..U+006F
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, // U+0070..U+007F

                // Also include the ranges from U+0080 to U+00FF for performance to avoid UTF8 code from checking boundary.
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // U+00F0..U+00FF
            };

            private static readonly ConcurrentDictionary<string, JsonEncodedText> s_encodedTextCache =
                new ConcurrentDictionary<string, JsonEncodedText>(StringComparer.Ordinal);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if !NET451
            public static JsonEncodedText GetEncodedText(string text, JavaScriptEncoder encoder)
            {
                if (encoder is null)
                {
                    return s_encodedTextCache.GetOrAdd(text, s => JsonEncodedText.Encode(s, JsonEscapeHandling.EscapeNonAscii));
                }
                else
                {
                    return JsonEncodedText.Encode(text, JsonEscapeHandling.EscapeNonAscii, encoder);
                }
            }
#else
            public static JsonEncodedText GetEncodedText(string text)
            {
                return s_encodedTextCache.GetOrAdd(text, s => JsonEncodedText.Encode(s, JsonEscapeHandling.EscapeNonAscii));
            }
#endif


            private static bool NeedsEscapingNoBoundsCheck(char value) => 0u >= (uint)AllowList[value] ? true : false;


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool NeedsEscaping(byte utf8Value) => 0u >= (uint)AllowList[utf8Value] ? true : false;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool NeedsEscaping(char value) => ((uint)value > byte.MaxValue || 0u >= AllowList[value]) ? true : false;

            public static int NeedsEscaping(in ReadOnlySpan<byte> utf8Source, JavaScriptEncoder encoder = null)
            {
                int idx;

#if !NET451
                if (encoder is object)
                {
                    idx = encoder.FindFirstCharacterToEncodeUtf8(utf8Source);
                    goto Return;
                }
#endif

                ref byte space = ref MemoryMarshal.GetReference(utf8Source);
                idx = 0;
                uint nlen = (uint)utf8Source.Length;
                while ((uint)idx < nlen)
                {
                    if (NeedsEscaping(Unsafe.Add(ref space, idx))) { goto Return; }
                    idx++;
                }

                idx = -1; // all characters allowed

            Return:
                return idx;
            }

            public static int NeedsEscaping(in ReadOnlySpan<char> utf16Source, JavaScriptEncoder encoder = null)
            {
                int idx;

#if !NET451
                if (encoder is object)
                {
                    idx = encoder.FindFirstCharacterToEncodeUtf8(MemoryMarshal.Cast<char, byte>(utf16Source));
                    goto Return;
                }
#endif

                ref char space = ref MemoryMarshal.GetReference(utf16Source);
                idx = 0;
                uint nlen = (uint)utf16Source.Length;
                while ((uint)idx < nlen)
                {
                    if (NeedsEscaping(Unsafe.Add(ref space, idx))) { goto Return; }
                    idx++;
                }

                idx = -1; // all characters allowed

            Return:
                return idx;
            }

#if !NET451

            public static void EscapeString(in ReadOnlySpan<byte> utf8Source, Span<byte> destination, int indexOfFirstByteToEscape, JavaScriptEncoder encoder, out int written)
            {
                Debug.Assert(indexOfFirstByteToEscape >= 0 && indexOfFirstByteToEscape < utf8Source.Length);

                utf8Source.Slice(0, indexOfFirstByteToEscape).CopyTo(destination);
                written = indexOfFirstByteToEscape;

                if (encoder is object)
                {
                    destination = destination.Slice(indexOfFirstByteToEscape);
                    var utf8Value = utf8Source.Slice(indexOfFirstByteToEscape);
                    EscapeStringInternal(utf8Value, destination, encoder, ref written);
                }
                else
                {
                    // For performance when no encoder is specified, perform escaping here for Ascii and on the
                    // first occurrence of a non-Ascii character, then call into the default encoder.
                    var utf8Value = utf8Source;
                    ref byte sourceSpace = ref MemoryMarshal.GetReference(utf8Value);
                    ref byte destSpace = ref MemoryMarshal.GetReference(destination);
                    uint nlen = (uint)utf8Value.Length;
                    while ((uint)indexOfFirstByteToEscape < (uint)utf8Value.Length)
                    {
                        byte val = Unsafe.Add(ref sourceSpace, indexOfFirstByteToEscape);
                        if (IsAsciiValue(val))
                        {
                            if (NeedsEscaping(val))
                            {
                                EscapeNextBytes(JsonEscapeHandling.EscapeNonAscii, val, destination, ref destSpace, ref written);
                                indexOfFirstByteToEscape++;
                            }
                            else
                            {
                                Unsafe.Add(ref destSpace, written++) = val;
                                indexOfFirstByteToEscape++;
                            }
                        }
                        else
                        {
                            // Fall back to default encoder.
                            destination = destination.Slice(written);
                            utf8Value = utf8Value.Slice(indexOfFirstByteToEscape);
                            EscapeStringInternal(utf8Value, destination, JavaScriptEncoder.Default, ref written);
                            break;
                        }
                    }
                }
            }

            private static void EscapeStringInternal(in ReadOnlySpan<byte> value, Span<byte> destination, JavaScriptEncoder encoder, ref int written)
            {
                Debug.Assert(encoder is object);

                OperationStatus result = encoder.EncodeUtf8(value, destination, out int encoderBytesConsumed, out int encoderBytesWritten);

                Debug.Assert(result != OperationStatus.DestinationTooSmall);
                Debug.Assert(result != OperationStatus.NeedMoreData);

                if (result != OperationStatus.Done)
                {
                    ThrowHelper.ThrowArgumentException_InvalidUTF8(value, encoderBytesWritten);
                }

                Debug.Assert(encoderBytesConsumed == value.Length);

                written += encoderBytesWritten;
            }

            public static void EscapeString(in ReadOnlySpan<char> utf16Source, Span<char> destination, int indexOfFirstByteToEscape, JavaScriptEncoder encoder, out int written)
            {
                Debug.Assert(indexOfFirstByteToEscape >= 0 && indexOfFirstByteToEscape < utf16Source.Length);

                utf16Source.Slice(0, indexOfFirstByteToEscape).CopyTo(destination);
                written = indexOfFirstByteToEscape;

                if (encoder is object)
                {
                    destination = destination.Slice(indexOfFirstByteToEscape);
                    var utf16Value = utf16Source.Slice(indexOfFirstByteToEscape);
                    EscapeStringInternal(utf16Value, destination, encoder, ref written);
                }
                else
                {
                    // For performance when no encoder is specified, perform escaping here for Ascii and on the
                    // first occurrence of a non-Ascii character, then call into the default encoder.
                    var utf16Value = utf16Source;
                    ref char sourceSpace = ref MemoryMarshal.GetReference(utf16Value);
                    ref char destSpace = ref MemoryMarshal.GetReference(destination);
                    uint nlen = (uint)utf16Value.Length;
                    while ((uint)indexOfFirstByteToEscape < nlen)
                    {
                        char val = Unsafe.Add(ref sourceSpace, indexOfFirstByteToEscape);
                        if (IsAsciiValue(val))
                        {
                            if (NeedsEscapingNoBoundsCheck(val))
                            {
                                EscapeNextChars(JsonEscapeHandling.EscapeNonAscii, val, ref destSpace, ref written);
                                indexOfFirstByteToEscape++;
                            }
                            else
                            {
                                Unsafe.Add(ref destSpace, written++) = val;
                                indexOfFirstByteToEscape++;
                            }
                        }
                        else
                        {
                            // Fall back to default encoder.
                            destination = destination.Slice(written);
                            utf16Value = utf16Value.Slice(indexOfFirstByteToEscape);
                            EscapeStringInternal(utf16Value, destination, JavaScriptEncoder.Default, ref written);
                            break;
                        }
                    }
                }
            }

            private static void EscapeStringInternal(in ReadOnlySpan<char> value, Span<char> destination, JavaScriptEncoder encoder, ref int written)
            {
                Debug.Assert(encoder is object);

                OperationStatus result = encoder.Encode(value, destination, out int encoderBytesConsumed, out int encoderCharsWritten);

                Debug.Assert(result != OperationStatus.DestinationTooSmall);
                Debug.Assert(result != OperationStatus.NeedMoreData);

                if (result != OperationStatus.Done)
                {
                    ThrowHelper.ThrowArgumentException_InvalidUTF16(value[encoderCharsWritten]);
                }

                Debug.Assert(encoderBytesConsumed == value.Length);

                written += encoderCharsWritten;
            }

#endif
        }
    }
}
