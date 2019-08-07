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
        private const int base64LineBreakPosition = 76;

        private static readonly char[] base64Table =
        {
            'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O',
            'P','Q','R','S','T','U','V','W','X','Y','Z','a','b','c','d',
            'e','f','g','h','i','j','k','l','m','n','o','p','q','r','s',
            't','u','v','w','x','y','z','0','1','2','3','4','5','6','7',
            '8','9','+','/','='
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToBase64String(byte[] inArray)
        {
            if (inArray == null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.inArray); }
            return ToBase64String(new ReadOnlySpan<byte>(inArray), Base64FormattingOptions.None);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToBase64String(byte[] inArray, Base64FormattingOptions options)
        {
            if (inArray == null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.inArray); }
            return ToBase64String(new ReadOnlySpan<byte>(inArray), options);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToBase64String(byte[] inArray, int offset, int length)
        {
            return ToBase64String(inArray, offset, length, Base64FormattingOptions.None);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToBase64String(byte[] inArray, int offset, int length, Base64FormattingOptions options)
        {
            if (inArray == null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.inArray); }
            if ((uint)length > JsonSharedConstant.TooBigOrNegative) { ThrowHelper.ThrowArgumentOutOfRangeException_Index(ExceptionArgument.length); }
            if ((uint)offset > JsonSharedConstant.TooBigOrNegative) { ThrowHelper.ThrowArgumentOutOfRangeException_GenericPositive(ExceptionArgument.offset); }
            if ((uint)offset > (uint)(inArray.Length - length)) { ThrowHelper.ThrowArgumentOutOfRangeException_OffsetLength(ExceptionArgument.offset); }

            return ToBase64String(new ReadOnlySpan<byte>(inArray, offset, length), options);
        }

        public static string ToBase64String(in ReadOnlySpan<byte> bytes, Base64FormattingOptions options = Base64FormattingOptions.None)
        {
            if (options < Base64FormattingOptions.None || options > Base64FormattingOptions.InsertLineBreaks)
            {
                ThrowHelper.ThrowArgumentException_EnumIllegalVal(options);
            }

            if (0u >= (uint)bytes.Length) { return string.Empty; }

            bool insertLineBreaks = (options == Base64FormattingOptions.InsertLineBreaks);
            string result = /*string.FastAllocateString*/new string('\0', ToBase64_CalculateAndValidateOutputLength(bytes.Length, insertLineBreaks));

            unsafe
            {
                fixed (byte* bytesPtr = &MemoryMarshal.GetReference(bytes))
                fixed (char* charsPtr = result)
                {
                    int charsWritten = ConvertToBase64Array(charsPtr, bytesPtr, 0, bytes.Length, insertLineBreaks);
                    Debug.Assert(result.Length == charsWritten, $"Expected {result.Length} == {charsWritten}");
                }
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToBase64CharArray(byte[] inArray, int offsetIn, int length, char[] outArray, int offsetOut)
        {
            return ToBase64CharArray(inArray, offsetIn, length, outArray, offsetOut, Base64FormattingOptions.None);
        }

        public static unsafe int ToBase64CharArray(byte[] inArray, int offsetIn, int length, char[] outArray, int offsetOut, Base64FormattingOptions options)
        {
            //Do data verfication
            if (inArray == null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.inArray); }
            if (outArray == null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.outArray); }
            if ((uint)length > JsonSharedConstant.TooBigOrNegative) { ThrowHelper.ThrowArgumentOutOfRangeException_Index(ExceptionArgument.length); }
            if ((uint)offsetIn > JsonSharedConstant.TooBigOrNegative) { ThrowHelper.ThrowArgumentOutOfRangeException_GenericPositive(ExceptionArgument.offsetIn); }
            if ((uint)offsetOut > JsonSharedConstant.TooBigOrNegative) { ThrowHelper.ThrowArgumentOutOfRangeException_GenericPositive(ExceptionArgument.offsetOut); }
            if (options < Base64FormattingOptions.None || options > Base64FormattingOptions.InsertLineBreaks)
            {
                ThrowHelper.ThrowArgumentException_EnumIllegalVal(options);
            }


            int retVal;

            int inArrayLength;
            int outArrayLength;
            int numElementsToCopy;

            inArrayLength = inArray.Length;

            if (offsetIn > (int)(inArrayLength - length)) { ThrowHelper.ThrowArgumentOutOfRangeException_OffsetLength(ExceptionArgument.offsetIn); }

            if (0u >= (uint)inArrayLength) { return 0; }

            bool insertLineBreaks = (options == Base64FormattingOptions.InsertLineBreaks);
            //This is the maximally required length that must be available in the char array
            outArrayLength = outArray.Length;

            // Length of the char buffer required
            numElementsToCopy = ToBase64_CalculateAndValidateOutputLength(length, insertLineBreaks);

            if (offsetOut > (int)(outArrayLength - numElementsToCopy)) { ThrowHelper.ThrowArgumentOutOfRangeException_OffsetOut(ExceptionArgument.offsetOut); }

            fixed (char* outChars = &outArray[offsetOut])
            {
                fixed (byte* inData = &inArray[0])
                {
                    retVal = ConvertToBase64Array(outChars, inData, offsetIn, length, insertLineBreaks);
                }
            }

            return retVal;
        }

        public static unsafe bool TryToBase64Chars(in ReadOnlySpan<byte> bytes, Span<char> chars, out int charsWritten, Base64FormattingOptions options = Base64FormattingOptions.None)
        {
            if (options < Base64FormattingOptions.None || options > Base64FormattingOptions.InsertLineBreaks)
            {
                ThrowHelper.ThrowArgumentException_EnumIllegalVal(options);
            }

            if (0u >= (uint)bytes.Length)
            {
                charsWritten = 0;
                return true;
            }

            bool insertLineBreaks = (options == Base64FormattingOptions.InsertLineBreaks);

            int charLengthRequired = ToBase64_CalculateAndValidateOutputLength(bytes.Length, insertLineBreaks);
            if (charLengthRequired > chars.Length)
            {
                charsWritten = 0;
                return false;
            }

            fixed (char* outChars = &MemoryMarshal.GetReference(chars))
            fixed (byte* inData = &MemoryMarshal.GetReference(bytes))
            {
                charsWritten = ConvertToBase64Array(outChars, inData, 0, bytes.Length, insertLineBreaks);
                return true;
            }
        }

        internal static unsafe int ConvertToBase64Array(char* outChars, byte* inData, int offset, int length, bool insertLineBreaks)
        {
            int lengthmod3 = length % 3;
            int calcLength = offset + (length - lengthmod3);
            int j = 0;
            int charcount = 0;
            //Convert three bytes at a time to base64 notation.  This will consume 4 chars.
            int i;

            // get a pointer to the base64Table to avoid unnecessary range checking
            fixed (char* base64 = &base64Table[0])
            {
                for (i = offset; i < calcLength; i += 3)
                {
                    if (insertLineBreaks)
                    {
                        if (charcount == base64LineBreakPosition)
                        {
                            outChars[j++] = '\r';
                            outChars[j++] = '\n';
                            charcount = 0;
                        }
                        charcount += 4;
                    }
                    outChars[j] = base64[(inData[i] & 0xfc) >> 2];
                    outChars[j + 1] = base64[((inData[i] & 0x03) << 4) | ((inData[i + 1] & 0xf0) >> 4)];
                    outChars[j + 2] = base64[((inData[i + 1] & 0x0f) << 2) | ((inData[i + 2] & 0xc0) >> 6)];
                    outChars[j + 3] = base64[(inData[i + 2] & 0x3f)];
                    j += 4;
                }

                //Where we left off before
                i = calcLength;

                if (insertLineBreaks && (lengthmod3 != 0) && (charcount == base64LineBreakPosition))
                {
                    outChars[j++] = '\r';
                    outChars[j++] = '\n';
                }

                switch (lengthmod3)
                {
                    case 2: //One character padding needed
                        outChars[j] = base64[(inData[i] & 0xfc) >> 2];
                        outChars[j + 1] = base64[((inData[i] & 0x03) << 4) | ((inData[i + 1] & 0xf0) >> 4)];
                        outChars[j + 2] = base64[(inData[i + 1] & 0x0f) << 2];
                        outChars[j + 3] = base64[64]; //Pad
                        j += 4;
                        break;
                    case 1: // Two character padding needed
                        outChars[j] = base64[(inData[i] & 0xfc) >> 2];
                        outChars[j + 1] = base64[(inData[i] & 0x03) << 4];
                        outChars[j + 2] = base64[64]; //Pad
                        outChars[j + 3] = base64[64]; //Pad
                        j += 4;
                        break;
                }
            }

            return j;
        }

        internal static int ToBase64_CalculateAndValidateOutputLength(int inputLength, bool insertLineBreaks)
        {
            long outlen = ((long)inputLength) / 3 * 4;          // the base length - we want integer division here. 
            outlen += ((inputLength % 3) != 0) ? 4 : 0;         // at most 4 more chars for the remainder

            if (0ul >= (ulong)outlen) { return 0; }

            if (insertLineBreaks)
            {
                long newLines = outlen / base64LineBreakPosition;
                if ((outlen % base64LineBreakPosition) == 0)
                {
                    --newLines;
                }
                outlen += newLines * 2;              // the number of line break chars we'll add, "\r\n"
            }

            // If we overflow an int then we cannot allocate enough
            // memory to output the value so throw
            if (outlen > int.MaxValue) { ThrowHelper.ThrowOutOfMemoryException(); }

            return (int)outlen;
        }
    }
}
