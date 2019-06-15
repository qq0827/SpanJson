// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Largely based on https://github.com/dotnet/corefx/blob/8135319caa7e457ed61053ca1418313b88057b51/src/System.Text.Json/src/System/Text/Json/Writer/JsonWriterHelper.Transcoding.cs#L12

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpanJson.Internal
{
    partial class EscapingHelper
    {
        public static class NonAscii
        {
            private static ReadOnlySpan<byte> AllowList => new byte[byte.MaxValue + 1]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0,
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            };

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool NeedsEscaping(byte utf8Value) => 0u >= AllowList[utf8Value] ? true : false;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool NeedsEscaping(char value) => (uint)value > byte.MaxValue || 0u >= AllowList[value] ? true : false;

            public static int NeedsEscaping(in ReadOnlySpan<byte> utf8Source)
            {
                ref byte space = ref MemoryMarshal.GetReference(utf8Source);
                IntPtr offset = (IntPtr)0;
                int idx = 0;
                uint nlen = (uint)utf8Source.Length;
                while ((uint)idx < nlen)
                {
                    if (NeedsEscaping(Unsafe.AddByteOffset(ref space, offset + idx))) { goto Return; }
                    idx++;
                }

                idx = -1; // all characters allowed

            Return:
                return idx;
            }

            public static int NeedsEscaping(in ReadOnlySpan<char> utf16Source)
            {
                ref char space = ref MemoryMarshal.GetReference(utf16Source);
                int idx = 0;
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

            public static void EscapeString(in ReadOnlySpan<byte> utf8Source, Span<byte> destination, int indexOfFirstByteToEscape, out int written)
            {
                Debug.Assert(indexOfFirstByteToEscape >= 0 && indexOfFirstByteToEscape < utf8Source.Length);

                utf8Source.Slice(0, indexOfFirstByteToEscape).CopyTo(destination);
                written = indexOfFirstByteToEscape;
                int consumed = indexOfFirstByteToEscape;

                ref byte sourceSpace = ref MemoryMarshal.GetReference(utf8Source);
                ref byte destSpace = ref MemoryMarshal.GetReference(destination);
                uint nlen = (uint)utf8Source.Length;
                while ((uint)consumed < nlen)
                {
                    byte val = Unsafe.Add(ref sourceSpace, consumed);
                    if (NeedsEscaping(val))
                    {
                        if (!EscapeNextBytes(ref sourceSpace, ref consumed, nlen - (uint)consumed, destination, ref destSpace, ref written))
                        {
                            ThrowHelper.ThrowArgumentException_InvalidUTF8(utf8Source, consumed);
                        }
                    }
                    else
                    {
                        destination[written] = val;
                        written++;
                        consumed++;
                    }
                }
            }

            public static void EscapeString(in ReadOnlySpan<char> utf16Source, Span<char> destination, int indexOfFirstByteToEscape, out int written)
            {
                Debug.Assert(indexOfFirstByteToEscape >= 0 && indexOfFirstByteToEscape < utf16Source.Length);

                utf16Source.Slice(0, indexOfFirstByteToEscape).CopyTo(destination);
                written = indexOfFirstByteToEscape;
                int consumed = indexOfFirstByteToEscape;

                ref char sourceSpace = ref MemoryMarshal.GetReference(utf16Source);
                ref char destSpace = ref MemoryMarshal.GetReference(destination);
                uint nlen = (uint)utf16Source.Length;
                while ((uint)consumed < nlen)
                {
                    char val = Unsafe.Add(ref sourceSpace, consumed);
                    if (NeedsEscaping(val))
                    {
                        EscapeNextChars(ref sourceSpace, nlen, val, ref destSpace, ref consumed, ref written);
                    }
                    else
                    {
                        Unsafe.Add(ref destSpace, written++) = val;
                    }
                    consumed++;
                }
            }
        }
    }
}
