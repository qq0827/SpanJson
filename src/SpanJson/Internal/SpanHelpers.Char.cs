// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace SpanJson.Internal
{
    using System;
    using System.Diagnostics;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    internal static partial class SpanHelpers
    {
        public static unsafe bool Contains(ref char searchSpace, char value, int length)
        {
            Debug.Assert(length >= 0);

            fixed (char* pChars = &searchSpace)
            {
                char* pCh = pChars;
                char* pEndCh = pCh + length;

                if (Vector.IsHardwareAccelerated && length >= Vector<ushort>.Count * 2)
                {
                    // Figure out how many characters to read sequentially until we are vector aligned
                    // This is equivalent to:
                    //         unaligned = ((int)pCh % Unsafe.SizeOf<Vector<ushort>>()) / elementsPerByte
                    //         length = (Vector<ushort>.Count - unaligned) % Vector<ushort>.Count
                    const int elementsPerByte = sizeof(ushort) / sizeof(byte);
                    int unaligned = ((int)pCh & (Unsafe.SizeOf<Vector<ushort>>() - 1)) / elementsPerByte;
                    length = (Vector<ushort>.Count - unaligned) & (Vector<ushort>.Count - 1);
                }

            SequentialScan:
                while (length >= 4)
                {
                    length -= 4;

                    if (value == *pCh ||
                        value == *(pCh + 1) ||
                        value == *(pCh + 2) ||
                        value == *(pCh + 3))
                    {
                        goto Found;
                    }

                    pCh += 4;
                }

                while (length > 0)
                {
                    length -= 1;

                    if (value == *pCh)
                        goto Found;

                    pCh += 1;
                }

                // We get past SequentialScan only if IsHardwareAccelerated is true. However, we still have the redundant check to allow
                // the JIT to see that the code is unreachable and eliminate it when the platform does not have hardware accelerated.
                if (Vector.IsHardwareAccelerated && pCh < pEndCh)
                {
                    // Get the highest multiple of Vector<ushort>.Count that is within the search space.
                    // That will be how many times we iterate in the loop below.
                    // This is equivalent to: length = Vector<ushort>.Count * ((int)(pEndCh - pCh) / Vector<ushort>.Count)
                    length = (int)((pEndCh - pCh) & ~(Vector<ushort>.Count - 1));

                    // Get comparison Vector
                    Vector<ushort> vComparison = new Vector<ushort>(value);

                    while (length > 0)
                    {
                        // Using Unsafe.Read instead of ReadUnaligned since the search space is pinned and pCh is always vector aligned
                        Debug.Assert(((int)pCh & (Unsafe.SizeOf<Vector<ushort>>() - 1)) == 0);
                        Vector<ushort> vMatches = Vector.Equals(vComparison, Unsafe.Read<Vector<ushort>>(pCh));
                        if (Vector<ushort>.Zero.Equals(vMatches))
                        {
                            pCh += Vector<ushort>.Count;
                            length -= Vector<ushort>.Count;
                            continue;
                        }

                        goto Found;
                    }

                    if (pCh < pEndCh)
                    {
                        length = (int)(pEndCh - pCh);
                        goto SequentialScan;
                    }
                }

                return false;

            Found:
                return true;
            }
        }

        public static unsafe int IndexOf(ref char searchSpace, char value, int length)
        {
            Debug.Assert(length >= 0);

            fixed (char* pChars = &searchSpace)
            {
                char* pCh = pChars;
                char* pEndCh = pCh + length;

                if (Vector.IsHardwareAccelerated && length >= Vector<ushort>.Count * 2)
                {
                    // Figure out how many characters to read sequentially until we are vector aligned
                    // This is equivalent to:
                    //         unaligned = ((int)pCh % Unsafe.SizeOf<Vector<ushort>>()) / elementsPerByte
                    //         length = (Vector<ushort>.Count - unaligned) % Vector<ushort>.Count
                    const int elementsPerByte = sizeof(ushort) / sizeof(byte);
                    int unaligned = ((int)pCh & (Unsafe.SizeOf<Vector<ushort>>() - 1)) / elementsPerByte;
                    length = (Vector<ushort>.Count - unaligned) & (Vector<ushort>.Count - 1);
                }

            SequentialScan:
                while (length >= 4)
                {
                    length -= 4;

                    if (pCh[0] == value)
                        goto Found;
                    if (pCh[1] == value)
                        goto Found1;
                    if (pCh[2] == value)
                        goto Found2;
                    if (pCh[3] == value)
                        goto Found3;

                    pCh += 4;
                }

                while (length > 0)
                {
                    length--;

                    if (pCh[0] == value)
                        goto Found;

                    pCh++;
                }

                // We get past SequentialScan only if IsHardwareAccelerated is true. However, we still have the redundant check to allow
                // the JIT to see that the code is unreachable and eliminate it when the platform does not have hardware accelerated.
                if (Vector.IsHardwareAccelerated && pCh < pEndCh)
                {
                    // Get the highest multiple of Vector<ushort>.Count that is within the search space.
                    // That will be how many times we iterate in the loop below.
                    // This is equivalent to: length = Vector<ushort>.Count * ((int)(pEndCh - pCh) / Vector<ushort>.Count)
                    length = (int)((pEndCh - pCh) & ~(Vector<ushort>.Count - 1));

                    // Get comparison Vector
                    Vector<ushort> vComparison = new Vector<ushort>(value);

                    while (length > 0)
                    {
                        // Using Unsafe.Read instead of ReadUnaligned since the search space is pinned and pCh is always vector aligned
                        Debug.Assert(((int)pCh & (Unsafe.SizeOf<Vector<ushort>>() - 1)) == 0);
                        Vector<ushort> vMatches = Vector.Equals(vComparison, Unsafe.Read<Vector<ushort>>(pCh));
                        if (Vector<ushort>.Zero.Equals(vMatches))
                        {
                            pCh += Vector<ushort>.Count;
                            length -= Vector<ushort>.Count;
                            continue;
                        }
                        // Find offset of first match
                        return (int)(pCh - pChars) + LocateFirstFoundChar(vMatches);
                    }

                    if (pCh < pEndCh)
                    {
                        length = (int)(pEndCh - pCh);
                        goto SequentialScan;
                    }
                }

                return -1;
            Found3:
                pCh++;
            Found2:
                pCh++;
            Found1:
                pCh++;
            Found:
                return (int)(pCh - pChars);
            }
        }

        public static unsafe int IndexOfAny(ref char searchSpace, char value0, char value1, int length)
        {
            Debug.Assert(length >= 0);

            fixed (char* pChars = &searchSpace)
            {
                char* pCh = pChars;
                char* pEndCh = pCh + length;

                if (Vector.IsHardwareAccelerated && length >= Vector<ushort>.Count * 2)
                {
                    // Figure out how many characters to read sequentially until we are vector aligned
                    // This is equivalent to:
                    //         unaligned = ((int)pCh % Unsafe.SizeOf<Vector<ushort>>()) / elementsPerByte
                    //         length = (Vector<ushort>.Count - unaligned) % Vector<ushort>.Count
                    const int elementsPerByte = sizeof(ushort) / sizeof(byte);
                    int unaligned = ((int)pCh & (Unsafe.SizeOf<Vector<ushort>>() - 1)) / elementsPerByte;
                    length = (Vector<ushort>.Count - unaligned) & (Vector<ushort>.Count - 1);
                }

            SequentialScan:
                while (length >= 4)
                {
                    length -= 4;

                    if (pCh[0] == value0 || pCh[0] == value1)
                        goto Found;
                    if (pCh[1] == value0 || pCh[1] == value1)
                        goto Found1;
                    if (pCh[2] == value0 || pCh[2] == value1)
                        goto Found2;
                    if (pCh[3] == value0 || pCh[3] == value1)
                        goto Found3;

                    pCh += 4;
                }

                while (length > 0)
                {
                    length--;

                    if (pCh[0] == value0 || pCh[0] == value1)
                        goto Found;

                    pCh++;
                }

                // We get past SequentialScan only if IsHardwareAccelerated is true. However, we still have the redundant check to allow
                // the JIT to see that the code is unreachable and eliminate it when the platform does not have hardware accelerated.
                if (Vector.IsHardwareAccelerated && pCh < pEndCh)
                {
                    // Get the highest multiple of Vector<ushort>.Count that is within the search space.
                    // That will be how many times we iterate in the loop below.
                    // This is equivalent to: length = Vector<ushort>.Count * ((int)(pEndCh - pCh) / Vector<ushort>.Count)
                    length = (int)((pEndCh - pCh) & ~(Vector<ushort>.Count - 1));

                    // Get comparison Vector
                    Vector<ushort> values0 = new Vector<ushort>(value0);
                    Vector<ushort> values1 = new Vector<ushort>(value1);

                    while (length > 0)
                    {
                        // Using Unsafe.Read instead of ReadUnaligned since the search space is pinned and pCh is always vector aligned
                        Debug.Assert(((int)pCh & (Unsafe.SizeOf<Vector<ushort>>() - 1)) == 0);
                        Vector<ushort> vData = Unsafe.Read<Vector<ushort>>(pCh);
                        var vMatches = Vector.BitwiseOr(
                                        Vector.Equals(vData, values0),
                                        Vector.Equals(vData, values1));
                        if (Vector<ushort>.Zero.Equals(vMatches))
                        {
                            pCh += Vector<ushort>.Count;
                            length -= Vector<ushort>.Count;
                            continue;
                        }
                        // Find offset of first match
                        return (int)(pCh - pChars) + LocateFirstFoundChar(vMatches);
                    }

                    if (pCh < pEndCh)
                    {
                        length = (int)(pEndCh - pCh);
                        goto SequentialScan;
                    }
                }

                return -1;
            Found3:
                pCh++;
            Found2:
                pCh++;
            Found1:
                pCh++;
            Found:
                return (int)(pCh - pChars);
            }
        }

        // Vector sub-search adapted from https://github.com/aspnet/KestrelHttpServer/pull/1138
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateFirstFoundChar(Vector<ushort> match)
        {
            var vector64 = Vector.AsVectorUInt64(match);
            ulong candidate = 0;
            int i = 0;
            // Pattern unrolled by jit https://github.com/dotnet/coreclr/pull/8001
            for (; i < Vector<ulong>.Count; i++)
            {
                candidate = vector64[i];
                if (candidate != 0)
                {
                    break;
                }
            }

            // Single LEA instruction with jitted const (using function result)
            return i * 4 + LocateFirstFoundChar(candidate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateFirstFoundChar(ulong match)
        {
            //// TODO: Arm variants
            //if (Bmi1.X64.IsSupported)
            //{
            //    return (int)(Bmi1.X64.TrailingZeroCount(match) >> 4);
            //}
            //else
            //{
                unchecked
                {
                    // Flag least significant power of two bit
                    var powerOfTwoFlag = match ^ (match - 1);
                    // Shift all powers of two into the high byte and extract
                    return (int)((powerOfTwoFlag * XorPowerOfTwoToHighChar) >> 49);
                }
            //}
        }

        private const ulong XorPowerOfTwoToHighChar = (0x03ul |
                                                       0x02ul << 16 |
                                                       0x01ul << 32) + 1;

        // Vector sub-search adapted from https://github.com/aspnet/KestrelHttpServer/pull/1138
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateLastFoundChar(Vector<ushort> match)
        {
            var vector64 = Vector.AsVectorUInt64(match);
            ulong candidate = 0;
            int i = Vector<ulong>.Count - 1;
            // Pattern unrolled by jit https://github.com/dotnet/coreclr/pull/8001
            for (; i >= 0; i--)
            {
                candidate = vector64[i];
                if (candidate != 0)
                {
                    break;
                }
            }

            // Single LEA instruction with jitted const (using function result)
            return i * 4 + LocateLastFoundChar(candidate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int LocateLastFoundChar(ulong match)
        {
            //return 3 - (BitOperations.LeadingZeroCount(match) >> 4);

            // Find the most significant char that has its highest bit set
            int index = 3;
            while ((long)match > 0)
            {
                match = match << 16;
                index--;
            }
            return index;
        }
    }
}
