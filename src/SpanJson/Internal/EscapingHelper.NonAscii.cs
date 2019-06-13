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
    internal static partial class EscapingHelper
    {
        static class NonAscii
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
            private static bool NeedsEscaping(byte value) => 0u >= AllowList[value] ? true : false;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool NeedsEscaping(char value) => (uint)value > byte.MaxValue || 0u >= AllowList[value] ? true : false;

            public static int NeedsEscaping(in ReadOnlySpan<byte> value)
            {
                ref byte space = ref MemoryMarshal.GetReference(value);
                IntPtr offset = (IntPtr)0;
                int idx = 0;
                uint nlen = (uint)value.Length;
                while ((uint)idx < nlen)
                {
                    if (NeedsEscaping(Unsafe.AddByteOffset(ref space, offset + idx))) { goto Return; }
                    idx++;
                }

                idx = -1; // all characters allowed

            Return:
                return idx;
            }

            public static int NeedsEscaping(in ReadOnlySpan<char> value)
            {
                ref char space = ref MemoryMarshal.GetReference(value);
                int idx = 0;
                uint nlen = (uint)value.Length;
                while ((uint)idx < nlen)
                {
                    if (NeedsEscaping(Unsafe.Add(ref space, idx))) { goto Return; }
                    idx++;
                }

                idx = -1; // all characters allowed

            Return:
                return idx;
            }

            public static void EscapeString(in ReadOnlySpan<byte> value, Span<byte> destination, int indexOfFirstByteToEscape, out int written)
            {
                Debug.Assert(indexOfFirstByteToEscape >= 0 && indexOfFirstByteToEscape < value.Length);

                value.Slice(0, indexOfFirstByteToEscape).CopyTo(destination);
                written = indexOfFirstByteToEscape;
                int consumed = indexOfFirstByteToEscape;

                ref byte sourceSpace = ref MemoryMarshal.GetReference(value);
                ref byte destSpace = ref MemoryMarshal.GetReference(destination);
                uint nlen = (uint)value.Length;
                while ((uint)consumed < nlen)
                {
                    byte val = Unsafe.Add(ref sourceSpace, consumed);
                    if (NeedsEscaping(val))
                    {
                        if (!EscapeNextBytes(ref sourceSpace, ref consumed, nlen - (uint)consumed, destination, ref destSpace, ref written))
                        {
                            ThrowHelper.ThrowArgumentException_InvalidUTF8(value, consumed);
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

            public static void EscapeString(in ReadOnlySpan<char> value, Span<char> destination, int indexOfFirstByteToEscape, out int written)
            {
                Debug.Assert(indexOfFirstByteToEscape >= 0 && indexOfFirstByteToEscape < value.Length);

                value.Slice(0, indexOfFirstByteToEscape).CopyTo(destination);
                written = indexOfFirstByteToEscape;
                int consumed = indexOfFirstByteToEscape;

                ref char sourceSpace = ref MemoryMarshal.GetReference(value);
                ref char destSpace = ref MemoryMarshal.GetReference(destination);
                uint nlen = (uint)value.Length;
                while ((uint)consumed < nlen)
                {
                    char val = Unsafe.Add(ref sourceSpace, consumed);
                    if (NeedsEscaping(val))
                    {
                        EscapeNextChars(ref sourceSpace, nlen, val, ref destSpace, ref consumed, ref written);
                    }
                    else
                    {
                        destination[written++] = val;
                    }
                    consumed++;
                }
            }
        }
    }
}
