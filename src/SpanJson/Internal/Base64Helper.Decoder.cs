// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpanJson.Internal
{
    public static partial class Base64Helper
    {
        /// <summary>Converts the specified string, which encodes binary data as Base64 digits, to the equivalent byte array.</summary>
        /// <param name="s">The string to convert</param>
        /// <returns>The array of bytes represented by the specified Base64 string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] FromBase64String(string s)
        {
            // "s" is an unfortunate parameter name, but we need to keep it for backward compat.
            if (s is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.s); }

            unsafe
            {
                fixed (char* sPtr = s)
                {
                    return FromBase64CharPtr(sPtr, s.Length);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFromBase64String(string s, Span<byte> bytes, out int bytesWritten)
        {
            if (s is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.s); }

            return TryFromBase64Chars(s.AsSpan(), bytes, out bytesWritten);
        }

        public static bool TryFromBase64Chars(ReadOnlySpan<char> chars, Span<byte> bytes, out int bytesWritten)
        {
            // This is actually local to one of the nested blocks but is being declared at the top as we don't want multiple stackallocs
            // for each iteraton of the loop. 
            Span<char> tempBuffer = stackalloc char[4];  // Note: The tempBuffer size could be made larger than 4 but the size must be a multiple of 4.

            bytesWritten = 0;

            while (chars.Length != 0)
            {
                // Attempt to decode a segment that doesn't contain whitespace.
                bool complete = TryDecodeFromUtf16(chars, bytes, out int consumedInThisIteration, out int bytesWrittenInThisIteration);
                bytesWritten += bytesWrittenInThisIteration;
                if (complete) { return true; }

                chars = chars.Slice(consumedInThisIteration);
                bytes = bytes.Slice(bytesWrittenInThisIteration);

                Debug.Assert(chars.Length != 0); // If TryDecodeFromUtf16() consumed the entire buffer, it could not have returned false.
                if (chars[0].IsSpace())
                {
                    // If we got here, the very first character not consumed was a whitespace. We can skip past any consecutive whitespace, then continue decoding.

                    int indexOfFirstNonSpace = 1;
                    for (; ; )
                    {
                        if (indexOfFirstNonSpace == chars.Length) { break; }
                        if (!chars[indexOfFirstNonSpace].IsSpace()) { break; }
                        indexOfFirstNonSpace++;
                    }

                    chars = chars.Slice(indexOfFirstNonSpace);

                    if ((bytesWrittenInThisIteration % 3) != 0 && chars.Length != 0)
                    {
                        // If we got here, the last successfully decoded block encountered an end-marker, yet we have trailing non-whitespace characters.
                        // That is not allowed.
                        bytesWritten = default;
                        return false;
                    }

                    // We now loop again to decode the next run of non-space characters. 
                }
                else
                {
                    Debug.Assert(chars.Length != 0 && !chars[0].IsSpace());

                    // If we got here, it is possible that there is whitespace that occurred in the middle of a 4-byte chunk. That is, we still have
                    // up to three Base64 characters that were left undecoded by the fast-path helper because they didn't form a complete 4-byte chunk.
                    // This is hopefully the rare case (multiline-formatted base64 message with a non-space character width that's not a multiple of 4.)
                    // We'll filter out whitespace and copy the remaining characters into a temporary buffer.
                    CopyToTempBufferWithoutWhiteSpace(chars, tempBuffer, out int consumedFromChars, out int charsWritten);
                    if ((charsWritten & 0x3) != 0)
                    {
                        // Even after stripping out whitespace, the number of characters is not divisible by 4. This cannot be a legal Base64 string.
                        bytesWritten = default;
                        return false;
                    }

                    tempBuffer = tempBuffer.Slice(0, charsWritten);
                    if (!TryDecodeFromUtf16(tempBuffer, bytes, out int consumedFromTempBuffer, out int bytesWrittenFromTempBuffer))
                    {
                        bytesWritten = default;
                        return false;
                    }
                    bytesWritten += bytesWrittenFromTempBuffer;
                    chars = chars.Slice(consumedFromChars);
                    bytes = bytes.Slice(bytesWrittenFromTempBuffer);

                    if ((bytesWrittenFromTempBuffer % 3) != 0)
                    {
                        // If we got here, this decode contained one or more padding characters ('='). We can accept trailing whitespace after this
                        // but nothing else.
                        for (int i = 0; i < chars.Length; i++)
                        {
                            if (!chars[i].IsSpace())
                            {
                                bytesWritten = default;
                                return false;
                            }
                        }
                        return true;
                    }

                    // We now loop again to decode the next run of non-space characters. 
                }
            }

            return true;
        }

        private static void CopyToTempBufferWithoutWhiteSpace(ReadOnlySpan<char> chars, Span<char> tempBuffer, out int consumed, out int charsWritten)
        {
            Debug.Assert(tempBuffer.Length != 0); // We only bound-check after writing a character to the tempBuffer.

            charsWritten = 0;
            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];
                if (!c.IsSpace())
                {
                    tempBuffer[charsWritten++] = c;
                    if (charsWritten == tempBuffer.Length)
                    {
                        consumed = i + 1;
                        return;
                    }
                }
            }
            consumed = chars.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsSpace(this char c)
        {
            switch (c)
            {
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>Converts the specified range of a Char array, which encodes binary data as Base64 digits, to the equivalent byte array.</summary>
        /// <param name="inArray">Chars representing Base64 encoding characters</param>
        /// <param name="offset">A position within the input array.</param>
        /// <param name="length">Number of element to convert.</param>
        /// <returns>The array of bytes represented by the specified Base64 encoding characters.</returns>
        public static byte[] FromBase64CharArray(char[] inArray, int offset, int length)
        {
            if (inArray is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.inArray); }
            if ((uint)length > JsonSharedConstant.TooBigOrNegative) { ThrowHelper.ThrowArgumentOutOfRangeException_Index(ExceptionArgument.length); }
            if ((uint)offset > JsonSharedConstant.TooBigOrNegative) { ThrowHelper.ThrowArgumentOutOfRangeException_GenericPositive(ExceptionArgument.offset); }
            if ((uint)offset > (uint)(inArray.Length - length)) { ThrowHelper.ThrowArgumentOutOfRangeException_OffsetLength(ExceptionArgument.offset); }

            if (0u >= (uint)inArray.Length)
            {
                return JsonHelpers.Empty<byte>();
            }

            unsafe
            {
                fixed (char* inArrayPtr = &inArray[0])
                {
                    return FromBase64CharPtr(inArrayPtr + offset, length);
                }
            }
        }

        /// <summary>Convert Base64 encoding characters to bytes:
        ///  - Compute result length exactly by actually walking the input;
        ///  - Allocate new result array based on computation;
        ///  - Decode input into the new array;</summary>
        /// <param name="inputPtr">Pointer to the first input char</param>
        /// <param name="inputLength">Number of input chars</param>
        /// <returns></returns>
        internal static unsafe byte[] FromBase64CharPtr(char* inputPtr, int inputLength)
        {
            // The validity of parameters much be checked by callers, thus we are Critical here.

            Debug.Assert(0 <= inputLength);

            // We need to get rid of any trailing white spaces.
            // Otherwise we would be rejecting input such as "abc= ":
            while (inputLength > 0)
            {
                int lastChar = inputPtr[inputLength - 1];
                if (lastChar != (int)' ' && lastChar != (int)'\n' && lastChar != (int)'\r' && lastChar != (int)'\t') { break; }
                inputLength--;
            }

            // Compute the output length:
            int resultLength = FromBase64_ComputeResultLength(inputPtr, inputLength);

            Debug.Assert(0 <= resultLength);

            // resultLength can be zero. We will still enter FromBase64_Decode and process the input.
            // It may either simply write no bytes (e.g. input = " ") or throw (e.g. input = "ab").

            // Create result byte blob:
            byte[] decodedBytes = new byte[resultLength];

            // Convert Base64 chars into bytes:
            if (!TryFromBase64Chars(new ReadOnlySpan<char>(inputPtr, inputLength), decodedBytes, out int _))
            {
                ThrowHelper.ThrowFormatException_BadBase64Char();
            }

            // Note that the number of bytes written can differ from resultLength if the caller is modifying the array
            // as it is being converted. Silently ignore the failure.
            // Consider throwing exception in an non in-place release.

            // We are done:
            return decodedBytes;
        }

        /// <summary>Compute the number of bytes encoded in the specified Base 64 char array:
        /// Walk the entire input counting white spaces and padding chars, then compute result length
        /// based on 3 bytes per 4 chars.</summary>
        internal static unsafe int FromBase64_ComputeResultLength(char* inputPtr, int inputLength)
        {
            const uint intEq = (uint)'=';
            const uint intSpace = (uint)' ';

            Debug.Assert(0 <= inputLength);

            char* inputEndPtr = inputPtr + inputLength;
            int usefulInputLength = inputLength;
            int padding = 0;

            while (inputPtr < inputEndPtr)
            {
                uint c = (uint)(*inputPtr);
                inputPtr++;

                // We want to be as fast as possible and filter out spaces with as few comparisons as possible.
                // We end up accepting a number of illegal chars as legal white-space chars.
                // This is ok: as soon as we hit them during actual decode we will recognise them as illegal and throw.
                if (c <= intSpace) { usefulInputLength--; }

                else if (c == intEq)
                {
                    usefulInputLength--;
                    padding++;
                }
            }

            Debug.Assert(0 <= usefulInputLength);

            // For legal input, we can assume that 0 <= padding < 3. But it may be more for illegal input.
            // We will notice it at decode when we see a '=' at the wrong place.
            Debug.Assert(0 <= padding);

            // Perf: reuse the variable that stored the number of '=' to store the number of bytes encoded by the
            // last group that contains the '=':
            if (padding != 0)
            {
                if (padding == 1)
                {
                    padding = 2;
                }
                else if (padding == 2)
                {
                    padding = 1;
                }
                else
                {
                    ThrowHelper.ThrowFormatException_BadBase64Char();
                }
            }

            // Done:
            return (usefulInputLength / 4) * 3 + padding;
        }

        /// <summary>Decode the span of UTF-16 encoded text represented as base 64 into binary data.
        /// If the input is not a multiple of 4, or contains illegal characters, it will decode as much as it can, to the largest possible multiple of 4.
        /// This invariant allows continuation of the parse with a slower, whitespace-tolerant algorithm.
        ///
        /// <param name="utf16">The input span which contains UTF-16 encoded text in base 64 that needs to be decoded.</param>
        /// <param name="bytes">The output span which contains the result of the operation, i.e. the decoded binary data.</param>
        /// <param name="consumed">The number of input bytes consumed during the operation. This can be used to slice the input for subsequent calls, if necessary.</param>
        /// <param name="written">The number of bytes written into the output span. This can be used to slice the output for subsequent calls, if necessary.</param>
        /// </summary> 
        /// <returns>Returns:
        /// - true  - The entire input span was successfully parsed.
        /// - false - Only a part of the input span was successfully parsed. Failure causes may include embedded or trailing whitespace, 
        ///           other illegal Base64 characters, trailing characters after an encoding pad ('='), an input span whose length is not divisible by 4
        ///           or a destination span that's too small. <paramref name="consumed"/> and <paramref name="written"/> are set so that 
        ///           parsing can continue with a slower whitespace-tolerant algorithm.
        ///           
        /// Note: This is a cut down version of the implementation of Base64.DecodeFromUtf8(), modified the accept UTF16 chars and act as a fast-path
        /// helper for the Convert routines when the input string contains no whitespace.
        /// </returns>
        private static bool TryDecodeFromUtf16(ReadOnlySpan<char> utf16, Span<byte> bytes, out int consumed, out int written)
        {
            ref char srcChars = ref MemoryMarshal.GetReference(utf16);
            ref byte destBytes = ref MemoryMarshal.GetReference(bytes);

            int srcLength = utf16.Length & ~0x3;  // only decode input up to the closest multiple of 4.
            int destLength = bytes.Length;

            int sourceIndex = 0;
            int destIndex = 0;

            if (0u >= (uint)utf16.Length) { goto DoneExit; }

            ref sbyte decodingMap = ref s_decodingMap[0];

            // Last bytes could have padding characters, so process them separately and treat them as valid.
            const int skipLastChunk = 4;

            int maxSrcLength;
            if (destLength >= (srcLength >> 2) * 3)
            {
                maxSrcLength = srcLength - skipLastChunk;
            }
            else
            {
                // This should never overflow since destLength here is less than int.MaxValue / 4 * 3 (i.e. 1610612733)
                // Therefore, (destLength / 3) * 4 will always be less than 2147483641
                maxSrcLength = (destLength / 3) * 4;
            }

            while (sourceIndex < maxSrcLength)
            {
                int result = Decode(ref Unsafe.Add(ref srcChars, sourceIndex), ref decodingMap);
                if ((uint)result > JsonSharedConstant.TooBigOrNegative) { goto InvalidExit; }
                WriteThreeLowOrderBytes(ref Unsafe.Add(ref destBytes, destIndex), result);
                destIndex += 3;
                sourceIndex += 4;
            }

            if (maxSrcLength != srcLength - skipLastChunk) { goto InvalidExit; }

            // If input is less than 4 bytes, srcLength == sourceIndex == 0
            // If input is not a multiple of 4, sourceIndex == srcLength != 0
            if (sourceIndex == srcLength) { goto InvalidExit; }

            int i0 = Unsafe.Add(ref srcChars, srcLength - 4);
            int i1 = Unsafe.Add(ref srcChars, srcLength - 3);
            int i2 = Unsafe.Add(ref srcChars, srcLength - 2);
            int i3 = Unsafe.Add(ref srcChars, srcLength - 1);
            if (((i0 | i1 | i2 | i3) & 0xffffff00) != 0) { goto InvalidExit; }

            i0 = Unsafe.Add(ref decodingMap, i0);
            i1 = Unsafe.Add(ref decodingMap, i1);

            i0 <<= 18;
            i1 <<= 12;

            i0 |= i1;

            if (i3 != EncodingPad)
            {
                i2 = Unsafe.Add(ref decodingMap, i2);
                i3 = Unsafe.Add(ref decodingMap, i3);

                i2 <<= 6;

                i0 |= i3;
                i0 |= i2;

                if ((uint)i0 > JsonSharedConstant.TooBigOrNegative) { goto InvalidExit; }
                if (destIndex > destLength - 3) { goto InvalidExit; }
                WriteThreeLowOrderBytes(ref Unsafe.Add(ref destBytes, destIndex), i0);
                destIndex += 3;
            }
            else if (i2 != EncodingPad)
            {
                i2 = Unsafe.Add(ref decodingMap, i2);

                i2 <<= 6;

                i0 |= i2;

                if ((uint)i0 > JsonSharedConstant.TooBigOrNegative) { goto InvalidExit; }
                if (destIndex > destLength - 2) { goto InvalidExit; }
                Unsafe.Add(ref destBytes, destIndex) = (byte)(i0 >> 16);
                Unsafe.Add(ref destBytes, destIndex + 1) = (byte)(i0 >> 8);
                destIndex += 2;
            }
            else
            {
                if ((uint)i0 > JsonSharedConstant.TooBigOrNegative) { goto InvalidExit; }
                if (destIndex > destLength - 1) { goto InvalidExit; }
                Unsafe.Add(ref destBytes, destIndex) = (byte)(i0 >> 16);
                destIndex += 1;
            }

            sourceIndex += 4;

            if (srcLength != utf16.Length) { goto InvalidExit; }

        DoneExit:
            consumed = sourceIndex;
            written = destIndex;
            return true;

        InvalidExit:
            consumed = sourceIndex;
            written = destIndex;
            Debug.Assert((consumed % 4) == 0);
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Decode(ref char encodedChars, ref sbyte decodingMap)
        {
            int i0 = encodedChars;
            int i1 = Unsafe.Add(ref encodedChars, 1);
            int i2 = Unsafe.Add(ref encodedChars, 2);
            int i3 = Unsafe.Add(ref encodedChars, 3);

            if (((i0 | i1 | i2 | i3) & 0xffffff00) != 0)
            {
                return -1; // One or more chars falls outside the 00..ff range. This cannot be a valid Base64 character.
            }

            i0 = Unsafe.Add(ref decodingMap, i0);
            i1 = Unsafe.Add(ref decodingMap, i1);
            i2 = Unsafe.Add(ref decodingMap, i2);
            i3 = Unsafe.Add(ref decodingMap, i3);

            i0 <<= 18;
            i1 <<= 12;
            i2 <<= 6;

            i0 |= i3;
            i1 |= i2;

            i0 |= i1;
            return i0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteThreeLowOrderBytes(ref byte destination, int value)
        {
            destination = (byte)(value >> 16);
            Unsafe.Add(ref destination, 1) = (byte)(value >> 8);
            Unsafe.Add(ref destination, 2) = (byte)value;
        }

        // Pre-computing this table using a custom string(s_characters) and GenerateDecodingMapAndVerify (found in tests)
        private static readonly sbyte[] s_decodingMap = {
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 62, -1, -1, -1, 63,         //62 is placed at index 43 (for +), 63 at index 47 (for /)
            52, 53, 54, 55, 56, 57, 58, 59, 60, 61, -1, -1, -1, -1, -1, -1,         //52-61 are placed at index 48-57 (for 0-9), 64 at index 61 (for =)
            -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14,
            15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, -1, -1, -1, -1, -1,         //0-25 are placed at index 65-90 (for A-Z)
            -1, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40,
            41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, -1, -1, -1, -1, -1,         //26-51 are placed at index 97-122 (for a-z)
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,         // Bytes over 122 ('z') are invalid and cannot be decoded
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,         // Hence, padding the map with 255, which indicates invalid input
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        };

        private const byte EncodingPad = (byte)'='; // '=', for padding
    }
}
