#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Reflection;
using CuteAnt;
using CuteAnt.Reflection;
using SpanJson.Document;
using SpanJson.Dynamic;

namespace SpanJson.Utilities
{
    #region == enum PrimitiveTypeCode ==

    internal enum PrimitiveTypeCode
    {
        Empty = 0,
        Object = 1,
        Char = 2,
        CharNullable = 3,
        Boolean = 4,
        BooleanNullable = 5,
        SByte = 6,
        SByteNullable = 7,
        Int16 = 8,
        Int16Nullable = 9,
        UInt16 = 10,
        UInt16Nullable = 11,
        Int32 = 12,
        Int32Nullable = 13,
        Byte = 14,
        ByteNullable = 15,
        UInt32 = 16,
        UInt32Nullable = 17,
        Int64 = 18,
        Int64Nullable = 19,
        UInt64 = 20,
        UInt64Nullable = 21,
        Single = 22,
        SingleNullable = 23,
        Double = 24,
        DoubleNullable = 25,
        DateTime = 26,
        DateTimeNullable = 27,
        DateTimeOffset = 28,
        DateTimeOffsetNullable = 29,
        Decimal = 30,
        DecimalNullable = 31,
        Guid = 32,
        GuidNullable = 33,
        TimeSpan = 34,
        TimeSpanNullable = 35,
        BigInteger = 36,
        BigIntegerNullable = 37,
        Uri = 38,
        String = 39,
        Bytes = 40,
        DBNull = 41,

        CombGuid = 100,
        CombGuidNullable = 101,

        JToken = 102,
    }

    #endregion

    public static class ConvertUtils
    {
        private static readonly Dictionary<Type, PrimitiveTypeCode> TypeCodeMap =
            new Dictionary<Type, PrimitiveTypeCode>
            {
                { typeof(char), PrimitiveTypeCode.Char },
                { typeof(char?), PrimitiveTypeCode.CharNullable },
                { typeof(bool), PrimitiveTypeCode.Boolean },
                { typeof(bool?), PrimitiveTypeCode.BooleanNullable },
                { typeof(sbyte), PrimitiveTypeCode.SByte },
                { typeof(sbyte?), PrimitiveTypeCode.SByteNullable },
                { typeof(short), PrimitiveTypeCode.Int16 },
                { typeof(short?), PrimitiveTypeCode.Int16Nullable },
                { typeof(ushort), PrimitiveTypeCode.UInt16 },
                { typeof(ushort?), PrimitiveTypeCode.UInt16Nullable },
                { typeof(int), PrimitiveTypeCode.Int32 },
                { typeof(int?), PrimitiveTypeCode.Int32Nullable },
                { typeof(byte), PrimitiveTypeCode.Byte },
                { typeof(byte?), PrimitiveTypeCode.ByteNullable },
                { typeof(uint), PrimitiveTypeCode.UInt32 },
                { typeof(uint?), PrimitiveTypeCode.UInt32Nullable },
                { typeof(long), PrimitiveTypeCode.Int64 },
                { typeof(long?), PrimitiveTypeCode.Int64Nullable },
                { typeof(ulong), PrimitiveTypeCode.UInt64 },
                { typeof(ulong?), PrimitiveTypeCode.UInt64Nullable },
                { typeof(float), PrimitiveTypeCode.Single },
                { typeof(float?), PrimitiveTypeCode.SingleNullable },
                { typeof(double), PrimitiveTypeCode.Double },
                { typeof(double?), PrimitiveTypeCode.DoubleNullable },
                { typeof(DateTime), PrimitiveTypeCode.DateTime },
                { typeof(DateTime?), PrimitiveTypeCode.DateTimeNullable },
                { typeof(DateTimeOffset), PrimitiveTypeCode.DateTimeOffset },
                { typeof(DateTimeOffset?), PrimitiveTypeCode.DateTimeOffsetNullable },
                { typeof(decimal), PrimitiveTypeCode.Decimal },
                { typeof(decimal?), PrimitiveTypeCode.DecimalNullable },
                { typeof(Guid), PrimitiveTypeCode.Guid },
                { typeof(Guid?), PrimitiveTypeCode.GuidNullable },
                { typeof(TimeSpan), PrimitiveTypeCode.TimeSpan },
                { typeof(TimeSpan?), PrimitiveTypeCode.TimeSpanNullable },
                { typeof(BigInteger), PrimitiveTypeCode.BigInteger },
                { typeof(BigInteger?), PrimitiveTypeCode.BigIntegerNullable },
                { typeof(Uri), PrimitiveTypeCode.Uri },
                { typeof(string), PrimitiveTypeCode.String },
                { typeof(byte[]), PrimitiveTypeCode.Bytes },
                { typeof(DBNull), PrimitiveTypeCode.DBNull },

                { typeof(CombGuid), PrimitiveTypeCode.CombGuid },
                { typeof(CombGuid?), PrimitiveTypeCode.CombGuidNullable },

                { typeof(SpanJson.Linq.JToken), PrimitiveTypeCode.JToken },
                { typeof(SpanJson.Linq.JContainer), PrimitiveTypeCode.JToken },
                { typeof(SpanJson.Linq.JObject), PrimitiveTypeCode.JToken },
                { typeof(SpanJson.Linq.JArray), PrimitiveTypeCode.JToken },
                { typeof(SpanJson.Linq.JProperty), PrimitiveTypeCode.JToken },
                { typeof(SpanJson.Linq.JValue), PrimitiveTypeCode.JToken },
                { typeof(SpanJson.Linq.JRaw), PrimitiveTypeCode.JToken },
            };

        internal static PrimitiveTypeCode GetTypeCode(Type t)
        {
            return GetTypeCode(t, out _);
        }

        internal static PrimitiveTypeCode GetTypeCode(Type t, out bool isEnum)
        {
            if (TypeCodeMap.TryGetValue(t, out PrimitiveTypeCode typeCode))
            {
                isEnum = false;
                return typeCode;
            }

            if (t.IsEnum)
            {
                isEnum = true;
                return GetTypeCode(Enum.GetUnderlyingType(t));
            }

            // performance?
            if (ReflectionUtils.IsNullableType(t))
            {
                Type nonNullable = Nullable.GetUnderlyingType(t);
                if (nonNullable.IsEnum)
                {
                    Type nullableUnderlyingType = typeof(Nullable<>).MakeGenericType(Enum.GetUnderlyingType(nonNullable));
                    isEnum = true;
                    return GetTypeCode(nullableUnderlyingType);
                }
            }

            isEnum = false;
            return PrimitiveTypeCode.Object;
        }

        internal static bool IsConvertible(Type t)
        {
            return typeof(IConvertible).IsAssignableFrom(t);
        }

        internal static TimeSpan ParseTimeSpan(string input)
        {
            return TimeSpan.Parse(input, CultureInfo.InvariantCulture);
        }

        private static readonly ConcurrentDictionary<StructMultiKey<Type, Type>, Func<object, object>> CastConverters =
            new ConcurrentDictionary<StructMultiKey<Type, Type>, Func<object, object>>();

        private static Func<object, object> CreateCastConverter(StructMultiKey<Type, Type> t)
        {
            Type initialType = t.Value1;
            Type targetType = t.Value2;
            MethodInfo castMethodInfo = targetType.GetMethod("op_Implicit", new[] { initialType })
                ?? targetType.GetMethod("op_Explicit", new[] { initialType });

            if (castMethodInfo is null)
            {
                return null;
            }

            MethodCaller<object, object> call = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(castMethodInfo);

            return o => call(null, new[] { o });
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)] // .NET Core 3.0 Release模式下，会抛出异常 Internal CLR error. (0x80131506)
        internal static string ToString(object value)
        {
            switch (value)
            {
                case JsonElement element:
                    return element.ToString();

                case SpanJsonDynamicUtf16Number utf16Number:
                    return utf16Number.ToString();

                case SpanJsonDynamicUtf16String utf16String:
                    return utf16String.ToString();

                case SpanJsonDynamicUtf8Number utf8Number:
                    return utf8Number.ToString();

                case SpanJsonDynamicUtf8String utf8String:
                    return utf8String.ToString();

                default:
                    return System.Convert.ToString(value, CultureInfo.InvariantCulture); ;
            }
        }

        internal static BigInteger ToBigInteger(object value)
        {
            switch (value)
            {
                case BigInteger integer:
                    return integer;

                case string s:
                    return BigInteger.Parse(s, CultureInfo.InvariantCulture);

                case float f:
                    return new BigInteger(f);

                case double d:
                    return new BigInteger(d);

                case decimal @decimal:
                    return new BigInteger(@decimal);

                case int i:
                    return new BigInteger(i);

                case long l:
                    return new BigInteger(l);

                case uint u:
                    return new BigInteger(u);

                case ulong @ulong:
                    return new BigInteger(@ulong);

                case byte[] bytes:
                    return new BigInteger(bytes);

                case SpanJsonDynamicUtf16Number utf16Number:
                    return new BigInteger((decimal)utf16Number);

                case SpanJsonDynamicUtf8Number utf8Number:
                    return new BigInteger((decimal)utf8Number);

                case JsonElement jsonElement when jsonElement.ValueKind == JsonValueKind.Number:
                    return new BigInteger(jsonElement.GetDecimal());

                default:
                    throw new InvalidCastException("Cannot convert {0} to BigInteger.".FormatWith(CultureInfo.InvariantCulture, value.GetType()));
            }
        }

        internal static object FromBigInteger(BigInteger i, Type targetType)
        {
            if (targetType == typeof(decimal))
            {
                return (decimal)i;
            }
            if (targetType == typeof(double))
            {
                return (double)i;
            }
            if (targetType == typeof(float))
            {
                return (float)i;
            }
            if (targetType == typeof(ulong))
            {
                return (ulong)i;
            }
            if (targetType == typeof(bool))
            {
                return i != 0;
            }

            try
            {
                return System.Convert.ChangeType((long)i, targetType, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Can not convert from BigInteger to {0}.".FormatWith(CultureInfo.InvariantCulture, targetType), ex);
            }
        }

        #region -- TryConvert --

        internal enum ConvertResult
        {
            Success = 0,
            CannotConvertNull = 1,
            NotInstantiableType = 2,
            NoValidConversion = 3
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Convert<T>(object initialValue)
        {
            return (T)Convert(initialValue, CultureInfo.InvariantCulture, typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Convert<T>(object initialValue, CultureInfo culture)
        {
            return (T)Convert(initialValue, culture, typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Convert(object initialValue, Type targetType)
        {
            return Convert(initialValue, CultureInfo.InvariantCulture, targetType);
        }

        public static object Convert(object initialValue, CultureInfo culture, Type targetType)
        {
            switch (TryConvertInternal(initialValue, culture, targetType, out object value))
            {
                case ConvertResult.Success:
                    return value;
                case ConvertResult.CannotConvertNull:
                    throw ThrowHelper2.GetException_Can_not_convert_null_into_non_nullable(initialValue, targetType);
                case ConvertResult.NotInstantiableType:
                    throw ThrowHelper2.GetArgumentException_Target_type_is_not_a_value_type_or_a_non_abstract_class(targetType);
                case ConvertResult.NoValidConversion:
                    throw ThrowHelper2.GetInvalidOperationException_Can_not_convert_from_to(initialValue, targetType);
                default:
                    throw ThrowHelper2.GetInvalidOperationException_Unexpected_conversion_result();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryConvert<T>(object initialValue, out T value)
        {
            return TryConvert(initialValue, CultureInfo.InvariantCulture, out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryConvert<T>(object initialValue, CultureInfo culture, out T value)
        {
            var result = TryConvert(initialValue, culture, typeof(T), out object tmp);
            value = result ? (T)tmp : default;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryConvert(object initialValue, Type targetType, out object value)
        {
            return TryConvert(initialValue, CultureInfo.InvariantCulture, targetType, out value);
        }

        public static bool TryConvert(object initialValue, CultureInfo culture, Type targetType, out object value)
        {
            try
            {
                if (TryConvertInternal(initialValue, culture, targetType, out value) == ConvertResult.Success)
                {
                    return true;
                }
            }
            catch { }
            value = null;
            return false;
        }

        private static ConvertResult TryConvertInternal(object initialValue, CultureInfo culture, Type targetType, out object value)
        {
            if (initialValue is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.initialValue); }

            if (ReflectionUtils.IsNullableType(targetType))
            {
                targetType = Nullable.GetUnderlyingType(targetType);
            }

            Type initialType = initialValue.GetType();

            if (targetType == initialType)
            {
                value = initialValue;
                return ConvertResult.Success;
            }

            // use Convert.ChangeType if both types are IConvertible
            if (IsConvertible(initialValue.GetType()) && IsConvertible(targetType))
            {
                if (targetType.IsEnum)
                {
                    if (initialValue is string)
                    {
                        value = Enum.Parse(targetType, initialValue.ToString(), true);
                        return ConvertResult.Success;
                    }
                    else if (IsInteger(initialValue))
                    {
                        value = Enum.ToObject(targetType, initialValue);
                        return ConvertResult.Success;
                    }
                }

                value = System.Convert.ChangeType(initialValue, targetType, culture);
                return ConvertResult.Success;
            }

            switch (initialValue)
            {
                case DateTime dt when targetType == typeof(DateTimeOffset):
                    value = new DateTimeOffset(dt);
                    return ConvertResult.Success;

                case byte[] bytes when targetType == typeof(Guid):
                    value = new Guid(bytes);
                    return ConvertResult.Success;

                case byte[] cbytes when targetType == typeof(CombGuid):
                    value = new CombGuid(cbytes, CombGuidSequentialSegmentType.Guid);
                    return ConvertResult.Success;

                case Guid guid when targetType == typeof(byte[]):
                    value = guid.ToByteArray();
                    return ConvertResult.Success;

                case CombGuid comb when targetType == typeof(byte[]):
                    value = comb.GetByteArray(CombGuidSequentialSegmentType.Guid);
                    return ConvertResult.Success;

                case string s:
                    if (targetType == typeof(Guid))
                    {
                        value = new Guid(s);
                        return ConvertResult.Success;
                    }
                    if (targetType == typeof(CombGuid))
                    {
                        value = new CombGuid(s, CombGuidSequentialSegmentType.Comb);
                        return ConvertResult.Success;
                    }
                    if (targetType == typeof(Uri))
                    {
                        value = new Uri(s, UriKind.RelativeOrAbsolute);
                        return ConvertResult.Success;
                    }
                    if (targetType == typeof(TimeSpan))
                    {
                        value = ParseTimeSpan(s);
                        return ConvertResult.Success;
                    }
                    if (targetType == typeof(byte[]))
                    {
                        value = System.Convert.FromBase64String(s);
                        return ConvertResult.Success;
                    }
                    if (targetType == typeof(Version))
                    {
                        if (VersionTryParse(s, out Version result))
                        {
                            value = result;
                            return ConvertResult.Success;
                        }
                        value = null;
                        return ConvertResult.NoValidConversion;
                    }
                    if (typeof(Type).IsAssignableFrom(targetType))
                    {
                        value = TypeUtils.ResolveType(s);
                        return ConvertResult.Success;
                    }
                    break;

                case BigInteger integer:
                    value = FromBigInteger(integer, targetType);
                    return ConvertResult.Success;

                case JsonElement jsonElement:
                    try
                    {
                        value = jsonElement.ToObject(targetType);
                        return ConvertResult.Success;
                    }
                    catch
                    {
                        value = null;
                        return ConvertResult.NoValidConversion;
                    }

                case SpanJsonDynamicUtf16Number utf16Number:
                    if (utf16Number.TryConvert(targetType, out value))
                    {
                        return ConvertResult.Success;
                    }
                    return ConvertResult.NoValidConversion;

                case SpanJsonDynamicUtf8Number utf8Number:
                    if (utf8Number.TryConvert(targetType, out value))
                    {
                        return ConvertResult.Success;
                    }
                    return ConvertResult.NoValidConversion;

                case SpanJsonDynamicUtf16String utf16Str:
                    if (utf16Str.TryConvert(targetType, out value))
                    {
                        return ConvertResult.Success;
                    }
                    break;

                case SpanJsonDynamicUtf8String utf8Str:
                    if (utf8Str.TryConvert(targetType, out value))
                    {
                        return ConvertResult.Success;
                    }
                    break;
            }

            if (targetType == typeof(BigInteger))
            {
                value = ToBigInteger(initialValue);
                return ConvertResult.Success;
            }

            // see if source or target types have a TypeConverter that converts between the two
            TypeConverter toConverter = TypeDescriptor.GetConverter(initialType);

            if (toConverter is object && toConverter.CanConvertTo(targetType))
            {
                value = toConverter.ConvertTo(null, culture, initialValue, targetType);
                return ConvertResult.Success;
            }

            TypeConverter fromConverter = TypeDescriptor.GetConverter(targetType);

            if (fromConverter is object && fromConverter.CanConvertFrom(initialType))
            {
                value = fromConverter.ConvertFrom(null, culture, initialValue);
                return ConvertResult.Success;
            }
            // handle DBNull
            if (initialValue == DBNull.Value)
            {
                if (ReflectionUtils.IsNullable(targetType))
                {
                    value = EnsureTypeAssignable(null, initialType, targetType);
                    return ConvertResult.Success;
                }

                // cannot convert null to non-nullable
                value = null;
                return ConvertResult.CannotConvertNull;
            }

            if (targetType.IsInterface || targetType.IsGenericTypeDefinition || targetType.IsAbstract)
            {
                value = null;
                return ConvertResult.NotInstantiableType;
            }

            value = null;
            return ConvertResult.NoValidConversion;
        }

        private static bool VersionTryParse(string input, out Version result)
        {
            return Version.TryParse(input, out result);
        }

        private static bool IsInteger(object value)
        {
            switch (GetTypeCode(value.GetType()))
            {
                case PrimitiveTypeCode.SByte:
                case PrimitiveTypeCode.Byte:
                case PrimitiveTypeCode.Int16:
                case PrimitiveTypeCode.UInt16:
                case PrimitiveTypeCode.Int32:
                case PrimitiveTypeCode.UInt32:
                case PrimitiveTypeCode.Int64:
                case PrimitiveTypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        #region -- ConvertOrCast --

        /// <summary>Converts the value to the specified type. If the value is unable to be converted, the
        /// value is checked whether it assignable to the specified type.</summary>
        /// <param name="initialValue">The value to convert.</param>
        /// <returns>The converted type. If conversion was unsuccessful, the initial value
        /// is returned if assignable to the target type.</returns>
        public static T ConvertOrCast<T>(object initialValue)
        {
            return (T)ConvertOrCast(initialValue, CultureInfo.InvariantCulture, typeof(T));
        }

        /// <summary>Converts the value to the specified type. If the value is unable to be converted, the
        /// value is checked whether it assignable to the specified type.</summary>
        /// <param name="initialValue">The value to convert.</param>
        /// <param name="culture">The culture to use when converting.</param>
        /// <returns>The converted type. If conversion was unsuccessful, the initial value
        /// is returned if assignable to the target type.</returns>
        public static T ConvertOrCast<T>(object initialValue, CultureInfo culture)
        {
            return (T)ConvertOrCast(initialValue, culture, typeof(T));
        }

        /// <summary>Converts the value to the specified type. If the value is unable to be converted, the
        /// value is checked whether it assignable to the specified type.</summary>
        /// <param name="initialValue">The value to convert.</param>
        /// <param name="targetType">The type to convert or cast the value to.</param>
        /// <returns>The converted type. If conversion was unsuccessful, the initial value
        /// is returned if assignable to the target type.</returns>
        public static object ConvertOrCast(object initialValue, Type targetType)
        {
            return ConvertOrCast(initialValue, CultureInfo.InvariantCulture, targetType);
        }

        /// <summary>Converts the value to the specified type. If the value is unable to be converted, the
        /// value is checked whether it assignable to the specified type.</summary>
        /// <param name="initialValue">The value to convert.</param>
        /// <param name="culture">The culture to use when converting.</param>
        /// <param name="targetType">The type to convert or cast the value to.</param>
        /// <returns>The converted type. If conversion was unsuccessful, the initial value
        /// is returned if assignable to the target type.</returns>
        public static object ConvertOrCast(object initialValue, CultureInfo culture, Type targetType)
        {
            if (targetType == typeof(object)) { return initialValue; }

            if (initialValue is null && ReflectionUtils.IsNullable(targetType)) { return null; }

            if (TryConvert(initialValue, culture, targetType, out object convertedValue))
            {
                return convertedValue;
            }

            return EnsureTypeAssignable(initialValue, initialValue.GetType(), targetType);
        }

        private static object EnsureTypeAssignable(object value, Type initialType, Type targetType)
        {
            Type valueType = value?.GetType();

            if (value is object)
            {
                if (targetType.IsAssignableFrom(valueType))
                {
                    return value;
                }

                Func<object, object> castConverter = CastConverters.GetOrAdd(new StructMultiKey<Type, Type>(valueType, targetType), k => CreateCastConverter(k));
                if (castConverter is object)
                {
                    return castConverter(value);
                }
            }
            else
            {
                if (ReflectionUtils.IsNullable(targetType))
                {
                    return null;
                }
            }

            throw ThrowHelper2.GetArgumentException_Could_not_cast_or_convert_from(initialType, targetType);
        }

        #endregion
    }
}