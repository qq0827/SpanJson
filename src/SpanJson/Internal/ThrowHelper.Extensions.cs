using System;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace SpanJson
{
    #region -- ExceptionArgument --

    /// <summary>The convention for this enum is using the argument name as the enum name</summary>
    internal enum ExceptionArgument
    {
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
            Exception GetException()
            {
                return new Exception("unreached code.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowException_UnreachableCode()
        {
            throw GetException();
            Exception GetException()
            {
                return new Exception("Unreachable code.");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowException_InvalidMode()
        {
            throw GetException();
            Exception GetException()
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
            ArgumentException GetException()
            {
                return new ArgumentException("length < newSize");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_InvalidDoubleValueForJson()
        {
            throw GetException();
            ArgumentException GetException()
            {
                return new ArgumentException("Invalid double value for JSON", "value");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentException_InvalidFloatValueForJson()
        {
            throw GetException();
            ArgumentException GetException()
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

        #endregion

        #region -- InvalidOperationException --

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException()
        {
            throw GetException();
            InvalidOperationException GetException()
            {
                return new InvalidOperationException();
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
            InvalidOperationException GetException()
            {
                return new InvalidOperationException("byte[] size reached maximum size of array(0x7FFFFFC7), can not write to single byte[]. Details: https://msdn.microsoft.com/en-us/library/system.array");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidOperationException_NestingLimitOfExceeded()
        {
            throw GetException();
            InvalidOperationException GetException()
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

        #endregion
    }
}
