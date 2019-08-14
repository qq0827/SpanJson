// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using SpanJson.Internal;

namespace SpanJson
{
    internal static partial class SysJsonThrowHelper
    {
        // If the exception source is this value, the serializer will re-throw as JsonException.
        public const string ExceptionSourceValueToRethrowAsJsonException = "System.Text.Json.Rethrowable";

        public static ArgumentOutOfRangeException GetArgumentOutOfRangeException_MaxDepthMustBePositive(string parameterName)
        {
            return GetArgumentOutOfRangeException(parameterName, SR.MaxDepthMustBePositive);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ArgumentOutOfRangeException GetArgumentOutOfRangeException(string parameterName, string message)
        {
            return new ArgumentOutOfRangeException(parameterName, message);
        }

        public static ArgumentOutOfRangeException GetArgumentOutOfRangeException_CommentEnumMustBeInRange(string parameterName)
        {
            return GetArgumentOutOfRangeException(parameterName, SR.CommentHandlingMustBeValid);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ArgumentException GetArgumentException(string message)
        {
            return new ArgumentException(message);
        }

        public static void ThrowArgumentException(string message)
        {
            throw GetArgumentException(message);
        }

        public static InvalidOperationException GetInvalidOperationException_CallFlushFirst(int _buffered)
        {
            return GetInvalidOperationException(SR.Format(SR.CallFlushToAvoidDataLoss, _buffered));
        }

        public static void ThrowArgumentException_PropertyNameTooLarge(int tokenLength)
        {
            throw GetArgumentException(SR.Format(SR.PropertyNameTooLarge, tokenLength));
        }

        public static void ThrowArgumentException_ValueTooLarge(int tokenLength)
        {
            throw GetArgumentException(SR.Format(SR.ValueTooLarge, tokenLength));
        }

        public static void ThrowArgumentException_ValueNotSupported()
        {
            throw GetArgumentException(SR.SpecialNumberValuesNotSupported);
        }

        public static void ThrowInvalidOperationException_NeedLargerSpan()
        {
            throw GetInvalidOperationException(SR.FailedToGetLargerSpan);
        }

        public static void ThrowArgumentException(in ReadOnlySpan<byte> propertyName, in ReadOnlySpan<byte> value)
        {
            if (propertyName.Length > JsonSharedConstant.MaxUnescapedTokenSize)
            {
                ThrowArgumentException(SR.Format(SR.PropertyNameTooLarge, propertyName.Length));
            }
            else
            {
                Debug.Assert(value.Length > JsonSharedConstant.MaxUnescapedTokenSize);
                ThrowArgumentException(SR.Format(SR.ValueTooLarge, value.Length));
            }
        }

        public static void ThrowArgumentException(in ReadOnlySpan<byte> propertyName, in ReadOnlySpan<char> value)
        {
            if (propertyName.Length > JsonSharedConstant.MaxUnescapedTokenSize)
            {
                ThrowArgumentException(SR.Format(SR.PropertyNameTooLarge, propertyName.Length));
            }
            else
            {
                Debug.Assert(value.Length > JsonSharedConstant.MaxCharacterTokenSize);
                ThrowArgumentException(SR.Format(SR.ValueTooLarge, value.Length));
            }
        }

        public static void ThrowArgumentException(in ReadOnlySpan<char> propertyName, in ReadOnlySpan<byte> value)
        {
            if (propertyName.Length > JsonSharedConstant.MaxCharacterTokenSize)
            {
                ThrowArgumentException(SR.Format(SR.PropertyNameTooLarge, propertyName.Length));
            }
            else
            {
                Debug.Assert(value.Length > JsonSharedConstant.MaxUnescapedTokenSize);
                ThrowArgumentException(SR.Format(SR.ValueTooLarge, value.Length));
            }
        }

        public static void ThrowArgumentException(in ReadOnlySpan<char> propertyName, in ReadOnlySpan<char> value)
        {
            if (propertyName.Length > JsonSharedConstant.MaxCharacterTokenSize)
            {
                ThrowArgumentException(SR.Format(SR.PropertyNameTooLarge, propertyName.Length));
            }
            else
            {
                Debug.Assert(value.Length > JsonSharedConstant.MaxCharacterTokenSize);
                ThrowArgumentException(SR.Format(SR.ValueTooLarge, value.Length));
            }
        }

        public static void ThrowInvalidOperationOrArgumentException(in ReadOnlySpan<byte> propertyName, int currentDepth)
        {
            currentDepth &= JsonSharedConstant.RemoveFlagsBitMask;
            if (currentDepth >= JsonSharedConstant.MaxWriterDepth)
            {
                ThrowInvalidOperationException(SR.Format(SR.DepthTooLarge, currentDepth, JsonSharedConstant.MaxWriterDepth));
            }
            else
            {
                Debug.Assert(propertyName.Length > JsonSharedConstant.MaxCharacterTokenSize);
                ThrowArgumentException(SR.Format(SR.PropertyNameTooLarge, propertyName.Length));
            }
        }

        public static void ThrowInvalidOperationException(int currentDepth)
        {
            currentDepth &= JsonSharedConstant.RemoveFlagsBitMask;
            Debug.Assert(currentDepth >= JsonSharedConstant.MaxWriterDepth);
            ThrowInvalidOperationException(SR.Format(SR.DepthTooLarge, currentDepth, JsonSharedConstant.MaxWriterDepth));
        }

        public static void ThrowInvalidOperationException(string message)
        {
            throw GetInvalidOperationException(message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static InvalidOperationException GetInvalidOperationException(string message)
        {
            var ex = new InvalidOperationException(message);
            ex.Source = ExceptionSourceValueToRethrowAsJsonException;
            return ex;
        }

        public static void ThrowInvalidOperationException_DepthNonZeroOrEmptyJson(int currentDepth)
        {
            throw GetInvalidOperationException(currentDepth);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static InvalidOperationException GetInvalidOperationException(int currentDepth)
        {
            currentDepth &= JsonSharedConstant.RemoveFlagsBitMask;
            if (currentDepth != 0)
            {
                return GetInvalidOperationException(SR.Format(SR.ZeroDepthAtEnd, currentDepth));
            }
            else
            {
                return GetInvalidOperationException(SR.EmptyJsonIsInvalid);
            }
        }

        public static void ThrowInvalidOperationOrArgumentException(in ReadOnlySpan<char> propertyName, int currentDepth)
        {
            currentDepth &= JsonSharedConstant.RemoveFlagsBitMask;
            if (currentDepth >= JsonSharedConstant.MaxWriterDepth)
            {
                ThrowInvalidOperationException(SR.Format(SR.DepthTooLarge, currentDepth, JsonSharedConstant.MaxWriterDepth));
            }
            else
            {
                Debug.Assert(propertyName.Length > JsonSharedConstant.MaxCharacterTokenSize);
                ThrowArgumentException(SR.Format(SR.PropertyNameTooLarge, propertyName.Length));
            }
        }

        public static InvalidOperationException GetInvalidOperationException_ExpectedNumber(JsonTokenType tokenType)
        {
            return GetInvalidOperationException("number", tokenType);
        }

        public static InvalidOperationException GetInvalidOperationException_ExpectedBoolean(JsonTokenType tokenType)
        {
            return GetInvalidOperationException("boolean", tokenType);
        }

        public static InvalidOperationException GetInvalidOperationException_ExpectedString(JsonTokenType tokenType)
        {
            return GetInvalidOperationException("string", tokenType);
        }

        public static InvalidOperationException GetInvalidOperationException_ExpectedStringComparison(JsonTokenType tokenType)
        {
            return GetInvalidOperationException(tokenType);
        }

        public static InvalidOperationException GetInvalidOperationException_ExpectedComment(JsonTokenType tokenType)
        {
            return GetInvalidOperationException("comment", tokenType);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static InvalidOperationException GetInvalidOperationException_CannotSkipOnPartial()
        {
            return GetInvalidOperationException(SR.CannotSkip);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static InvalidOperationException GetInvalidOperationException(string message, JsonTokenType tokenType)
        {
            return GetInvalidOperationException(SR.Format(SR.InvalidCast, tokenType, message));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static InvalidOperationException GetInvalidOperationException(JsonTokenType tokenType)
        {
            return GetInvalidOperationException(SR.Format(SR.InvalidComparison, tokenType));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static InvalidOperationException GetJsonElementWrongTypeException(
            JsonTokenType expectedType,
            JsonTokenType actualType)
        {
            return GetInvalidOperationException(
                SR.Format(SR.JsonElementHasWrongType, expectedType.ToValueKind(), actualType.ToValueKind()));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static InvalidOperationException GetJsonElementWrongTypeException(
            string expectedTypeName,
            JsonTokenType actualType)
        {
            return GetInvalidOperationException(
                SR.Format(SR.JsonElementHasWrongType, expectedTypeName, actualType.ToValueKind()));
        }

        public static void ThrowJsonReaderException(ref Utf8JsonReader json, ExceptionResource resource, byte nextByte = default, in ReadOnlySpan<byte> bytes = default)
        {
            throw GetJsonReaderException(ref json, resource, nextByte, bytes);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static JsonException GetJsonReaderException(ref Utf8JsonReader json, ExceptionResource resource, byte nextByte, in ReadOnlySpan<byte> bytes)
        {
            string message = GetResourceString(ref json, resource, nextByte, TextEncodings.Utf8.GetString(bytes));

            long lineNumber = json.CurrentState._lineNumber;
            long bytePositionInLine = json.CurrentState._bytePositionInLine;

            message += $" LineNumber: {lineNumber} | BytePositionInLine: {bytePositionInLine}.";
            return new JsonReaderException(message, lineNumber, bytePositionInLine);
        }

        private static bool IsPrintable(byte value) => value >= 0x20 && value < 0x7F;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetPrintableString(byte value)
        {
            return IsPrintable(value) ? ((char)value).ToString() : $"0x{value:X2}";
        }

        // This function will convert an ExceptionResource enum value to the resource string.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetResourceString(ref Utf8JsonReader json, ExceptionResource resource, byte nextByte, string characters)
        {
            string character = GetPrintableString(nextByte);

            string message = "";
            switch (resource)
            {
                case ExceptionResource.ArrayDepthTooLarge:
                    message = SR.Format(SR.ArrayDepthTooLarge, json.CurrentState.Options.MaxDepth);
                    break;
                case ExceptionResource.MismatchedObjectArray:
                    message = SR.Format(SR.MismatchedObjectArray, character);
                    break;
                case ExceptionResource.TrailingCommaNotAllowedBeforeArrayEnd:
                    message = SR.TrailingCommaNotAllowedBeforeArrayEnd;
                    break;
                case ExceptionResource.TrailingCommaNotAllowedBeforeObjectEnd:
                    message = SR.TrailingCommaNotAllowedBeforeObjectEnd;
                    break;
                case ExceptionResource.EndOfStringNotFound:
                    message = SR.EndOfStringNotFound;
                    break;
                case ExceptionResource.RequiredDigitNotFoundAfterSign:
                    message = SR.Format(SR.RequiredDigitNotFoundAfterSign, character);
                    break;
                case ExceptionResource.RequiredDigitNotFoundAfterDecimal:
                    message = SR.Format(SR.RequiredDigitNotFoundAfterDecimal, character);
                    break;
                case ExceptionResource.RequiredDigitNotFoundEndOfData:
                    message = SR.RequiredDigitNotFoundEndOfData;
                    break;
                case ExceptionResource.ExpectedEndAfterSingleJson:
                    message = SR.Format(SR.ExpectedEndAfterSingleJson, character);
                    break;
                case ExceptionResource.ExpectedEndOfDigitNotFound:
                    message = SR.Format(SR.ExpectedEndOfDigitNotFound, character);
                    break;
                case ExceptionResource.ExpectedNextDigitEValueNotFound:
                    message = SR.Format(SR.ExpectedNextDigitEValueNotFound, character);
                    break;
                case ExceptionResource.ExpectedSeparatorAfterPropertyNameNotFound:
                    message = SR.Format(SR.ExpectedSeparatorAfterPropertyNameNotFound, character);
                    break;
                case ExceptionResource.ExpectedStartOfPropertyNotFound:
                    message = SR.Format(SR.ExpectedStartOfPropertyNotFound, character);
                    break;
                case ExceptionResource.ExpectedStartOfPropertyOrValueNotFound:
                    message = SR.ExpectedStartOfPropertyOrValueNotFound;
                    break;
                case ExceptionResource.ExpectedStartOfPropertyOrValueAfterComment:
                    message = SR.Format(SR.ExpectedStartOfPropertyOrValueAfterComment, character);
                    break;
                case ExceptionResource.ExpectedStartOfValueNotFound:
                    message = SR.Format(SR.ExpectedStartOfValueNotFound, character);
                    break;
                case ExceptionResource.ExpectedValueAfterPropertyNameNotFound:
                    message = SR.ExpectedValueAfterPropertyNameNotFound;
                    break;
                case ExceptionResource.FoundInvalidCharacter:
                    message = SR.Format(SR.FoundInvalidCharacter, character);
                    break;
                case ExceptionResource.InvalidEndOfJsonNonPrimitive:
                    message = SR.Format(SR.InvalidEndOfJsonNonPrimitive, json.TokenType);
                    break;
                case ExceptionResource.ObjectDepthTooLarge:
                    message = SR.Format(SR.ObjectDepthTooLarge, json.CurrentState.Options.MaxDepth);
                    break;
                case ExceptionResource.ExpectedFalse:
                    message = SR.Format(SR.ExpectedFalse, characters);
                    break;
                case ExceptionResource.ExpectedNull:
                    message = SR.Format(SR.ExpectedNull, characters);
                    break;
                case ExceptionResource.ExpectedTrue:
                    message = SR.Format(SR.ExpectedTrue, characters);
                    break;
                case ExceptionResource.InvalidCharacterWithinString:
                    message = SR.Format(SR.InvalidCharacterWithinString, character);
                    break;
                case ExceptionResource.InvalidCharacterAfterEscapeWithinString:
                    message = SR.Format(SR.InvalidCharacterAfterEscapeWithinString, character);
                    break;
                case ExceptionResource.InvalidHexCharacterWithinString:
                    message = SR.Format(SR.InvalidHexCharacterWithinString, character);
                    break;
                case ExceptionResource.EndOfCommentNotFound:
                    message = SR.EndOfCommentNotFound;
                    break;
                case ExceptionResource.ZeroDepthAtEnd:
                    message = SR.Format(SR.ZeroDepthAtEnd);
                    break;
                case ExceptionResource.ExpectedJsonTokens:
                    message = SR.ExpectedJsonTokens;
                    break;
                case ExceptionResource.NotEnoughData:
                    message = SR.NotEnoughData;
                    break;
                case ExceptionResource.ExpectedOneCompleteToken:
                    message = SR.ExpectedOneCompleteToken;
                    break;
                case ExceptionResource.InvalidCharacterAtStartOfComment:
                    message = SR.Format(SR.InvalidCharacterAtStartOfComment, character);
                    break;
                case ExceptionResource.UnexpectedEndOfDataWhileReadingComment:
                    message = SR.Format(SR.UnexpectedEndOfDataWhileReadingComment);
                    break;
                case ExceptionResource.UnexpectedEndOfLineSeparator:
                    message = SR.Format(SR.UnexpectedEndOfLineSeparator);
                    break;
                default:
                    Debug.Fail($"The ExceptionResource enum value: {resource} is not part of the switch. Add the appropriate case and exception message.");
                    break;
            }

            return message;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowInvalidOperationException(ExceptionResource resource, int currentDepth, byte token, JsonTokenType tokenType)
        {
            throw GetInvalidOperationException(resource, currentDepth, token, tokenType);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentException_InvalidCommentValue()
        {
            throw new ArgumentException(SR.CannotWriteCommentWithEmbeddedDelimiter);
        }

        public static void ThrowArgumentException_InvalidUTF8(in ReadOnlySpan<byte> value)
        {
            var builder = new StringBuilder();

            int printFirst10 = Math.Min(value.Length, 10);

            for (int i = 0; i < printFirst10; i++)
            {
                byte nextByte = value[i];
                if (IsPrintable(nextByte))
                {
                    builder.Append((char)nextByte);
                }
                else
                {
                    builder.Append($"0x{nextByte:X2}");
                }
            }

            if (printFirst10 < value.Length)
            {
                builder.Append("...");
            }

            throw new ArgumentException(SR.Format(SR.CannotEncodeInvalidUTF8, builder));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentException_InvalidUTF16(int charAsInt)
        {
            throw new ArgumentException(SR.Format(SR.CannotEncodeInvalidUTF16, $"0x{charAsInt:X2}"));
        }

        public static void ThrowInvalidOperationException_ReadInvalidUTF16(int charAsInt)
        {
            throw GetInvalidOperationException(SR.Format(SR.CannotReadInvalidUTF16, $"0x{charAsInt:X2}"));
        }

        public static void ThrowInvalidOperationException_ReadInvalidUTF16()
        {
            throw GetInvalidOperationException(SR.CannotReadIncompleteUTF16);
        }

        public static InvalidOperationException GetInvalidOperationException_ReadInvalidUTF8(DecoderFallbackException innerException)
        {
            return GetInvalidOperationException(SR.CannotTranscodeInvalidUtf8, innerException);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ArgumentException GetArgumentException_ReadInvalidUTF16(EncoderFallbackException innerException)
        {
            return new ArgumentException(SR.CannotTranscodeInvalidUtf16, innerException);
        }

        public static InvalidOperationException GetInvalidOperationException(string message, Exception innerException)
        {
            InvalidOperationException ex = new InvalidOperationException(message, innerException);
            ex.Source = ExceptionSourceValueToRethrowAsJsonException;
            return ex;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static InvalidOperationException GetInvalidOperationException(ExceptionResource resource, int currentDepth, byte token, JsonTokenType tokenType)
        {
            string message = GetResourceString(resource, currentDepth, token, tokenType);
            InvalidOperationException ex = GetInvalidOperationException(message);
            ex.Source = ExceptionSourceValueToRethrowAsJsonException;
            return ex;
        }

        // This function will convert an ExceptionResource enum value to the resource string.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetResourceString(ExceptionResource resource, int currentDepth, byte token, JsonTokenType tokenType)
        {
            string message = "";
            switch (resource)
            {
                case ExceptionResource.MismatchedObjectArray:
                    Debug.Assert(token == JsonUtf8Constant.CloseBracket || token == JsonUtf8Constant.CloseBrace);
                    message = (tokenType == JsonTokenType.PropertyName) ?
                        SR.Format(SR.CannotWriteEndAfterProperty, (char)token) :
                        SR.Format(SR.MismatchedObjectArray, (char)token);
                    break;
                case ExceptionResource.DepthTooLarge:
                    message = SR.Format(SR.DepthTooLarge, currentDepth & JsonSharedConstant.RemoveFlagsBitMask, JsonSharedConstant.MaxWriterDepth);
                    break;
                case ExceptionResource.CannotStartObjectArrayWithoutProperty:
                    message = SR.Format(SR.CannotStartObjectArrayWithoutProperty, tokenType);
                    break;
                case ExceptionResource.CannotStartObjectArrayAfterPrimitiveOrClose:
                    message = SR.Format(SR.CannotStartObjectArrayAfterPrimitiveOrClose, tokenType);
                    break;
                case ExceptionResource.CannotWriteValueWithinObject:
                    message = SR.Format(SR.CannotWriteValueWithinObject, tokenType);
                    break;
                case ExceptionResource.CannotWritePropertyWithinArray:
                    message = (tokenType == JsonTokenType.PropertyName) ?
                        SR.Format(SR.CannotWritePropertyAfterProperty) :
                        SR.Format(SR.CannotWritePropertyWithinArray, tokenType);
                    break;
                case ExceptionResource.CannotWriteValueAfterPrimitiveOrClose:
                    message = SR.Format(SR.CannotWriteValueAfterPrimitiveOrClose, tokenType);
                    break;
                default:
                    Debug.Fail($"The ExceptionResource enum value: {resource} is not part of the switch. Add the appropriate case and exception message.");
                    break;
            }

            return message;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static FormatException GetFormatException()
        {
            var ex = new FormatException();
            ex.Source = ExceptionSourceValueToRethrowAsJsonException;
            return ex;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static FormatException GetFormatException(NumericType numericType)
        {
            string message = "";

            switch (numericType)
            {
                case NumericType.Byte:
                    message = SR.FormatByte;
                    break;
                case NumericType.SByte:
                    message = SR.FormatSByte;
                    break;
                case NumericType.Int16:
                    message = SR.FormatInt16;
                    break;
                case NumericType.Int32:
                    message = SR.FormatInt32;
                    break;
                case NumericType.Int64:
                    message = SR.FormatInt64;
                    break;
                case NumericType.UInt16:
                    message = SR.FormatUInt16;
                    break;
                case NumericType.UInt32:
                    message = SR.FormatUInt32;
                    break;
                case NumericType.UInt64:
                    message = SR.FormatUInt64;
                    break;
                case NumericType.Single:
                    message = SR.FormatSingle;
                    break;
                case NumericType.Double:
                    message = SR.FormatDouble;
                    break;
                case NumericType.Decimal:
                    message = SR.FormatDecimal;
                    break;
                default:
                    Debug.Fail($"The NumericType enum value: {numericType} is not part of the switch. Add the appropriate case and exception message.");
                    break;
            }

            var ex = new FormatException(message);
            ex.Source = ExceptionSourceValueToRethrowAsJsonException;
            return ex;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static FormatException GetFormatException(DateType dateType)
        {
            string message = "";

            switch (dateType)
            {
                case DateType.DateTime:
                    message = SR.FormatDateTime;
                    break;
                case DateType.DateTimeOffset:
                    message = SR.FormatDateTimeOffset;
                    break;
                case DateType.Base64String:
                    message = SR.CannotDecodeInvalidBase64;
                    break;
                default:
                    Debug.Fail($"The DateType enum value: {dateType} is not part of the switch. Add the appropriate case and exception message.");
                    break;
            }

            var ex = new FormatException(message);
            ex.Source = ExceptionSourceValueToRethrowAsJsonException;
            return ex;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static NotSupportedException GetNotSupportedException_SerializationNotSupportedCollection(Type propertyType, Type parentType, MemberInfo memberInfo)
        {
            if (parentType is object && parentType != typeof(object) && memberInfo is object)
            {
                return new NotSupportedException(SR.Format(SR.SerializationNotSupportedCollection, propertyType, $"{parentType}.{memberInfo.Name}"));
            }

            return new NotSupportedException(SR.Format(SR.SerializationNotSupportedCollectionType, propertyType));
        }
    }

    internal enum NumericType
    {
        Byte,
        SByte,
        Int16,
        Int32,
        Int64,
        UInt16,
        UInt32,
        UInt64,
        Single,
        Double,
        Decimal
    }

    internal enum DateType
    {
        DateTime,
        DateTimeOffset,
        Base64String
    }
}
