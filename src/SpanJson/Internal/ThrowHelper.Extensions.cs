using System;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Text;

namespace SpanJson
{
    #region -- ExceptionArgument --

    /// <summary>The convention for this enum is using the argument name as the enum name</summary>
    internal enum ExceptionArgument
    {
        count,
        input,
        value,
        source,
        newSize,
    }

    #endregion

    #region -- ExceptionResource --

    /// <summary>The convention for this enum is using the resource name as the enum name</summary>
    internal enum ExceptionResource
    {
    }

    #endregion

    partial class ThrowHelper
    {
        #region -- Exception --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowException_UnreachedCode()
        {
            throw GetException();
            static Exception GetException()
            {
                return new Exception("unreached code.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowException_UnreachableCode()
        {
            throw GetException();
            static Exception GetException()
            {
                return new Exception("Unreachable code.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowException_InvalidMode()
        {
            throw GetException();
            static Exception GetException()
            {
                return new Exception("Invalid Mode.");
            }
        }

        #endregion

        #region -- ArgumentException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static ArgumentException GetArgumentException_Guid_Pattern()
        {
            return new ArgumentException("Invalid Guid Pattern.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_Guid_Pattern()
        {
            throw GetArgumentException_Guid_Pattern();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_Length()
        {
            throw GetException();
            static ArgumentException GetException()
            {
                return new ArgumentException("length < newSize");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_InvalidDoubleValueForJson()
        {
            throw GetException();
            static ArgumentException GetException()
            {
                return new ArgumentException("Invalid double value for JSON", "value");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_InvalidFloatValueForJson()
        {
            throw GetException();
            static ArgumentException GetException()
            {
                return new ArgumentException("Invalid float value for JSON", "value");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_IsNotAnInterface(Type interfaceType)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException($"{interfaceType} is not an interface.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentException_InvalidUTF16(int charAsInt)
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException(string.Format("Cannot encode invalid UTF-16 text as JSON. Invalid surrogate value: '{0}'.", $"0x{charAsInt:X2}"));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowArgumentException_InvalidUTF8(ReadOnlySpan<byte> value, int bytesWritten)
        {
            var builder = new StringBuilder();

            value = value.Slice(bytesWritten);
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

            static bool IsPrintable(byte value) => value >= 0x20 && value < 0x7F;

            throw new ArgumentException($"Cannot encode invalid UTF-8 text as JSON. Invalid input: '{builder}'.");
        }

        public static ArgumentException GetArgumentException_ReadInvalidUTF16(EncoderFallbackException innerException)
        {
            return new ArgumentException("Cannot transcode invalid UTF-16 string to UTF-8 JSON text.", innerException);
        }

        #endregion

        #region -- ArgumentOutOfRangeException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRangeException_Nonnegative(ExceptionArgument argument)
        {
            throw GetException();
            ArgumentOutOfRangeException GetException()
            {
                var argumentName = GetArgumentName(argument);
                return new ArgumentOutOfRangeException(argumentName, $"{argumentName} should be non negative.");
            }
        }

        #endregion

        #region -- InvalidOperationException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException()
        {
            throw GetException();
            static InvalidOperationException GetException()
            {
                return new InvalidOperationException();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_Register_Resolver_Err()
        {
            throw GetException();
            static InvalidOperationException GetException()
            {
                return new InvalidOperationException("Register must call on startup(before use GetFormatter<T>).");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_NotSupport_Value(float value)
        {
            throw GetException();
            InvalidOperationException GetException()
            {
                return new InvalidOperationException("not support float value:" + value);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_NotSupport_Value(double value)
        {
            throw GetException();
            InvalidOperationException GetException()
            {
                return new InvalidOperationException("not support double value:" + value);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_Reached_MaximumSize()
        {
            throw GetException();
            static InvalidOperationException GetException()
            {
                return new InvalidOperationException("byte[] size reached maximum size of array(0x7FFFFFC7), can not write to single byte[]. Details: https://msdn.microsoft.com/en-us/library/system.array");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_NestingLimitOfExceeded()
        {
            throw GetException();
            static InvalidOperationException GetException()
            {
                return new InvalidOperationException($"Nesting Limit of {JsonSharedConstant.NestingLimit} exceeded.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_CanotMapConstructorParameterToAnyMember(ParameterInfo constructorParameter)
        {
            throw GetException();
            InvalidOperationException GetException()
            {
                return new InvalidOperationException($"Can't map constructor parameter {constructorParameter.Name} to any member.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowInvalidOperationException_ReadInvalidUTF16(int charAsInt)
        {
            throw GetException();
            InvalidOperationException GetException()
            {
                return new InvalidOperationException(string.Format("Cannot read invalid UTF-16 JSON text as string. Invalid surrogate value: '{0}'.", $"0x{charAsInt:X2}"));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowInvalidOperationException_ReadInvalidUTF16()
        {
            throw GetException();
            InvalidOperationException GetException()
            {
                return new InvalidOperationException("Cannot read incomplete UTF-16 JSON text as string with missing low surrogate.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowInvalidOperationException_AdvancedTooFar(int capacity)
        {
            throw GetException();
            InvalidOperationException GetException()
            {
                return new InvalidOperationException($"Cannot advance past the end of the buffer, which has a size of {capacity}.");
            }
        }

        #endregion

        #region -- NotSupportedException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static NotSupportedException GetNotSupportedException()
        {
            return new NotSupportedException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowNotSupportedException()
        {
            throw GetNotSupportedException();
        }

        #endregion

        #region -- InvalidCastException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static InvalidCastException GetInvalidCastException()
        {
            return new InvalidCastException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidCastException()
        {
            throw GetInvalidCastException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static InvalidOperationException GetInvalidOperationException_ReadInvalidUTF8(DecoderFallbackException innerException)
        {
            return new InvalidOperationException("Cannot transcode invalid UTF-8 JSON text to UTF-16 string.", innerException);
        }

        #endregion

        #region -- JsonParserException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowJsonParserException(JsonParserException.ParserError error, JsonParserException.ValueType type, int position)
        {
            throw GetJsonParserException(error, type, position);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static JsonParserException GetJsonParserException(JsonParserException.ParserError error, JsonParserException.ValueType type, int position)
        {
            return new JsonParserException(error, type, position);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowJsonParserException(JsonParserException.ParserError error, int position)
        {
            throw GetJsonParserException(error, position);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static JsonParserException GetJsonParserException(JsonParserException.ParserError error, int position)
        {
            return new JsonParserException(error, position);
        }

        #endregion
    }
}
