using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using CuteAnt;
using CuteAnt.Pool;
using SpanJson.Document;
using SpanJson.Dynamic;
using SpanJson.Resolvers;
using SpanJson.Serialization;
using SpanJson.Utilities;
using NJsonWriter = Newtonsoft.Json.JsonWriter;
using NJsonSerializer = Newtonsoft.Json.JsonSerializer;
using NJsonTextWriter = Newtonsoft.Json.JsonTextWriter;
using NFormatting = Newtonsoft.Json.Formatting;

namespace SpanJson.Linq
{
    partial class JToken
    {
        /// <summary>Creates an instance of the specified .NET type from the <see cref="JToken"/>.</summary>
        /// <typeparam name="T">The object type that the token will be deserialized to.</typeparam>
        /// <returns>The new object created from the JSON value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ToObject<T>()
        {
            return ToObject<T, IncludeNullsOriginalCaseResolver<byte>, IncludeNullsOriginalCaseResolver<char>>();
        }

        /// <summary>Creates an instance of the specified .NET type from the <see cref="JToken"/>.</summary>
        /// <typeparam name="T">The object type that the token will be deserialized to.</typeparam>
        /// <typeparam name="TUtf8Resolver">The Utf8 resolver.</typeparam>
        /// <typeparam name="TUtf16Resolver">The Utf16 resolver.</typeparam>
        /// <returns>The new object created from the JSON value.</returns>
        public T ToObject<T, TUtf8Resolver, TUtf16Resolver>()
            where TUtf8Resolver : IJsonFormatterResolver<byte, TUtf8Resolver>, new()
            where TUtf16Resolver : IJsonFormatterResolver<char, TUtf16Resolver>, new()
        {
            if (TryConvertOrCast(typeof(T), out object result)) { return (T)result; }

            return ToObjectInternal<T, TUtf8Resolver, TUtf16Resolver>();
        }

        /// <summary>Creates an instance of the specified .NET type from the <see cref="JToken"/>.</summary>
        /// <returns>The new object created from the JSON value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object ToObject(Type objectType)
        {
            return ToObject<IncludeNullsOriginalCaseResolver<byte>, IncludeNullsOriginalCaseResolver<char>>(objectType);
        }

        /// <summary>Creates an instance of the specified .NET type from the <see cref="JToken"/>.</summary>
        /// <typeparam name="TUtf8Resolver">The Utf8 resolver.</typeparam>
        /// <typeparam name="TUtf16Resolver">The Utf16 resolver.</typeparam>
        /// <param name="objectType">The object type that the token will be deserialized to.</param>
        /// <returns>The new object created from the JSON value.</returns>
        public object ToObject<TUtf8Resolver, TUtf16Resolver>(Type objectType)
            where TUtf8Resolver : IJsonFormatterResolver<byte, TUtf8Resolver>, new()
            where TUtf16Resolver : IJsonFormatterResolver<char, TUtf16Resolver>, new()
        {
            if (objectType is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.objectType); }

            if (TryConvertOrCast(objectType, out object result)) { return result; }

            return ToObjectInternal<TUtf8Resolver, TUtf16Resolver>(objectType);
        }

        /// <summary>Creates an instance of the specified .NET type from the <see cref="JToken"/>.</summary>
        /// <typeparam name="T">The object type that the token will be deserialized to.</typeparam>
        /// <returns>The new object created from the JSON value.</returns>
        public T ToPolymorphicObject<T>()
        {
            if (TryConvertOrCast(typeof(T), out object result)) { return (T)result; }

            var jsonSerializer = PolymorphicDeserializerPool.Take();
            try
            {
                return ToObjectInternal<T, IncludeNullsOriginalCaseResolver<byte>, IncludeNullsOriginalCaseResolver<char>>(jsonSerializer);
            }
            finally
            {
                PolymorphicDeserializerPool.Return(jsonSerializer);
            }
        }

        /// <summary>Creates an instance of the specified .NET type from the <see cref="JToken"/>
        /// using the specified <see cref="NJsonSerializer"/>.</summary>
        /// <typeparam name="T">The object type that the token will be deserialized to.</typeparam>
        /// <param name="jsonSerializer">The <see cref="NJsonSerializer"/> that will be used when creating the object.</param>
        /// <returns>The new object created from the JSON value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ToPolymorphicObject<T>(NJsonSerializer jsonSerializer)
        {
            return ToPolymorphicObject<T, IncludeNullsOriginalCaseResolver<byte>, IncludeNullsOriginalCaseResolver<char>>(jsonSerializer);
        }

        /// <summary>Creates an instance of the specified .NET type from the <see cref="JToken"/>
        /// using the specified <see cref="NJsonSerializer"/>.</summary>
        /// <typeparam name="T">The object type that the token will be deserialized to.</typeparam>
        /// <typeparam name="TUtf8Resolver">The Utf8 resolver.</typeparam>
        /// <typeparam name="TUtf16Resolver">The Utf16 resolver.</typeparam>
        /// <param name="jsonSerializer">The <see cref="NJsonSerializer"/> that will be used when creating the object.</param>
        /// <returns>The new object created from the JSON value.</returns>
        public T ToPolymorphicObject<T, TUtf8Resolver, TUtf16Resolver>(NJsonSerializer jsonSerializer)
            where TUtf8Resolver : IJsonFormatterResolver<byte, TUtf8Resolver>, new()
            where TUtf16Resolver : IJsonFormatterResolver<char, TUtf16Resolver>, new()
        {
            if (TryConvertOrCast(typeof(T), out object result)) { return (T)result; }

            if (jsonSerializer is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.jsonSerializer); }

            return ToObjectInternal<T, TUtf8Resolver, TUtf16Resolver>(jsonSerializer);
        }

        /// <summary>Creates an instance of the specified .NET type from the <see cref="JToken"/>.</summary>
        /// <param name="objectType">The object type that the token will be deserialized to.</param>
        /// <returns>The new object created from the JSON value.</returns>
        public object ToPolymorphicObject(Type objectType)
        {
            if (objectType is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.objectType); }

            if (TryConvertOrCast(objectType, out object result)) { return result; }

            var jsonSerializer = PolymorphicDeserializerPool.Take();
            try
            {
                return ToObjectInternal<IncludeNullsOriginalCaseResolver<byte>, IncludeNullsOriginalCaseResolver<char>>(objectType, jsonSerializer);
            }
            finally
            {
                PolymorphicDeserializerPool.Return(jsonSerializer);
            }
        }

        /// <summary>Creates an instance of the specified .NET type from the <see cref="JToken"/>
        /// using the specified <see cref="NJsonSerializer"/>.</summary>
        /// <param name="objectType">The object type that the token will be deserialized to.</param>
        /// <param name="jsonSerializer">The <see cref="NJsonSerializer"/> that will be used when creating the object.</param>
        /// <returns>The new object created from the JSON value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object ToPolymorphicObject(Type objectType, NJsonSerializer jsonSerializer)
        {
            return ToPolymorphicObject<IncludeNullsOriginalCaseResolver<byte>, IncludeNullsOriginalCaseResolver<char>>(objectType, jsonSerializer);
        }

        /// <summary>Creates an instance of the specified .NET type from the <see cref="JToken"/>
        /// using the specified <see cref="NJsonSerializer"/>.</summary>
        /// <typeparam name="TUtf8Resolver">The Utf8 resolver.</typeparam>
        /// <typeparam name="TUtf16Resolver">The Utf16 resolver.</typeparam>
        /// <param name="objectType">The object type that the token will be deserialized to.</param>
        /// <param name="jsonSerializer">The <see cref="NJsonSerializer"/> that will be used when creating the object.</param>
        /// <returns>The new object created from the JSON value.</returns>
        public object ToPolymorphicObject<TUtf8Resolver, TUtf16Resolver>(Type objectType, NJsonSerializer jsonSerializer)
            where TUtf8Resolver : IJsonFormatterResolver<byte, TUtf8Resolver>, new()
            where TUtf16Resolver : IJsonFormatterResolver<char, TUtf16Resolver>, new()
        {
            if (objectType is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.objectType); }

            if (TryConvertOrCast(objectType, out object result)) { return result; }

            if (jsonSerializer is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.jsonSerializer); }

            return ToObjectInternal<TUtf8Resolver, TUtf16Resolver>(objectType, jsonSerializer);
        }

        #region ** TryConvertOrCast **

        sealed class InnerEnumStringResolver<TSymbol> : ResolverBase<TSymbol, InnerEnumStringResolver<TSymbol>> where TSymbol : struct
        {
            public InnerEnumStringResolver()
                : base(new SpanJsonOptions
                {
                    NullOption = NullOptions.ExcludeNulls,
                    EnumOption = EnumOptions.String
                })
            {
            }
        }

        sealed class InnerEnumIntegerResolver<TSymbol> : ResolverBase<TSymbol, InnerEnumIntegerResolver<TSymbol>> where TSymbol : struct
        {
            public InnerEnumIntegerResolver()
                : base(new SpanJsonOptions
                {
                    NullOption = NullOptions.ExcludeNulls,
                    EnumOption = EnumOptions.Integer
                })
            {
            }
        }

        private bool TryConvertOrCast(Type objectType, out object result)
        {
            PrimitiveTypeCode typeCode = ConvertUtils.GetTypeCode(objectType, out bool isEnum);

            if (isEnum)
            {
                static bool TryParseQuotedInteger(Type type, JValue jv, out object v)
                {
                    try
                    {
                        Type enumType = type.IsEnum ? type : Nullable.GetUnderlyingType(type);
                        var underlyingType = Enum.GetUnderlyingType(enumType);
                        v = Enum.ToObject(enumType, jv.ToObject(underlyingType));
                        return true;
                    }
                    catch { }
                    v = null;
                    return false;
                }
                switch (Type)
                {
                    case JTokenType.Dynamic:
                        {
                            try
                            {
                                result = ToObjectInternal<InnerEnumStringResolver<byte>, InnerEnumStringResolver<char>>(objectType);
                                return true;
                            }
                            catch (Exception ex)
                            {
                                // Try parse quoted number
                                if (TryParseQuotedInteger(objectType, (JValue)this, out object ev))
                                {
                                    result = ev;
                                    return true;
                                }
                                throw ThrowHelper2.GetArgumentException_Could_not_convert(objectType, this, ex);
                            }
                        }
                    case JTokenType.String:
                        {
                            try
                            {
                                result = JsonSerializer.NonGeneric.Utf16.Deserialize<InnerEnumStringResolver<char>>($"\"{this.ToString()}\"", objectType);
                                return true;
                            }
                            catch (Exception ex)
                            {
                                // Try parse quoted number
                                if (TryParseQuotedInteger(objectType, (JValue)this, out object ev))
                                {
                                    result = ev;
                                    return true;
                                }
                                throw ThrowHelper2.GetArgumentException_Could_not_convert(objectType, this, ex);
                            }
                        }
                    case JTokenType.Integer:
                        {
                            var innerValue = ((JValue)this).Value;
                            switch (innerValue)
                            {
                                case SpanJsonDynamicUtf8Number _:
                                case SpanJsonDynamicUtf16Number _:
                                case JsonElement _:
                                    try
                                    {
                                        result = ToObjectInternal<InnerEnumIntegerResolver<byte>, InnerEnumIntegerResolver<char>>(objectType);
                                        return true;
                                    }
                                    catch (Exception ex)
                                    {
                                        throw ThrowHelper2.GetArgumentException_Could_not_convert(objectType, this, ex);
                                    }

                                default:
                                    Type enumType = objectType.IsEnum ? objectType : Nullable.GetUnderlyingType(objectType);
                                    result = Enum.ToObject(enumType, ((JValue)this).Value);
                                    return true;
                            }
                        }
                }
            }

            // 基元类型交由 JValue 处理
            switch (typeCode)
            {
                case PrimitiveTypeCode.BooleanNullable:
                    result = (bool?)this;
                    return true;
                case PrimitiveTypeCode.Boolean:
                    result = (bool)this;
                    return true;
                case PrimitiveTypeCode.CharNullable:
                    result = (char?)this;
                    return true;
                case PrimitiveTypeCode.Char:
                    result = (char)this;
                    return true;
                case PrimitiveTypeCode.SByte:
                    result = (sbyte)this;
                    return true;
                case PrimitiveTypeCode.SByteNullable:
                    result = (sbyte?)this;
                    return true;
                case PrimitiveTypeCode.ByteNullable:
                    result = (byte?)this;
                    return true;
                case PrimitiveTypeCode.Byte:
                    result = (byte)this;
                    return true;
                case PrimitiveTypeCode.Int16Nullable:
                    result = (short?)this;
                    return true;
                case PrimitiveTypeCode.Int16:
                    result = (short)this;
                    return true;
                case PrimitiveTypeCode.UInt16Nullable:
                    result = (ushort?)this;
                    return true;
                case PrimitiveTypeCode.UInt16:
                    result = (ushort)this;
                    return true;
                case PrimitiveTypeCode.Int32Nullable:
                    result = (int?)this;
                    return true;
                case PrimitiveTypeCode.Int32:
                    result = (int)this;
                    return true;
                case PrimitiveTypeCode.UInt32Nullable:
                    result = (uint?)this;
                    return true;
                case PrimitiveTypeCode.UInt32:
                    result = (uint)this;
                    return true;
                case PrimitiveTypeCode.Int64Nullable:
                    result = (long?)this;
                    return true;
                case PrimitiveTypeCode.Int64:
                    result = (long)this;
                    return true;
                case PrimitiveTypeCode.UInt64Nullable:
                    result = (ulong?)this;
                    return true;
                case PrimitiveTypeCode.UInt64:
                    result = (ulong)this;
                    return true;
                case PrimitiveTypeCode.SingleNullable:
                    result = (float?)this;
                    return true;
                case PrimitiveTypeCode.Single:
                    result = (float)this;
                    return true;
                case PrimitiveTypeCode.DoubleNullable:
                    result = (double?)this;
                    return true;
                case PrimitiveTypeCode.Double:
                    result = (double)this;
                    return true;
                case PrimitiveTypeCode.DecimalNullable:
                    result = (decimal?)this;
                    return true;
                case PrimitiveTypeCode.Decimal:
                    result = (decimal)this;
                    return true;
                case PrimitiveTypeCode.DateTimeNullable:
                    result = (DateTime?)this;
                    return true;
                case PrimitiveTypeCode.DateTime:
                    result = (DateTime)this;
                    return true;
                case PrimitiveTypeCode.DateTimeOffsetNullable:
                    result = (DateTimeOffset?)this;
                    return true;
                case PrimitiveTypeCode.DateTimeOffset:
                    result = (DateTimeOffset)this;
                    return true;
                case PrimitiveTypeCode.String:
                    result = (string)this;
                    return true;
                case PrimitiveTypeCode.GuidNullable:
                    result = (Guid?)this;
                    return true;
                case PrimitiveTypeCode.Guid:
                    result = (Guid)this;
                    return true;
                case PrimitiveTypeCode.CombGuidNullable:
                    result = (CombGuid?)this;
                    return true;
                case PrimitiveTypeCode.CombGuid:
                    result = (CombGuid)this;
                    return true;
                case PrimitiveTypeCode.Uri:
                    result = (Uri)this;
                    return true;
                case PrimitiveTypeCode.TimeSpanNullable:
                    result = (TimeSpan?)this;
                    return true;
                case PrimitiveTypeCode.TimeSpan:
                    result = (TimeSpan)this;
                    return true;
                case PrimitiveTypeCode.BigIntegerNullable:
                    result = ToBigIntegerNullable(this);
                    return true;
                case PrimitiveTypeCode.BigInteger:
                    result = ToBigInteger(this);
                    return true;
                case PrimitiveTypeCode.JToken:
                    result = this;
                    return true;
                default:
                    result = null;
                    return false;
            }
        }

        #endregion

        protected virtual T ToObjectInternal<T, TUtf8Resolver, TUtf16Resolver>()
            where TUtf8Resolver : IJsonFormatterResolver<byte, TUtf8Resolver>, new()
            where TUtf16Resolver : IJsonFormatterResolver<char, TUtf16Resolver>, new()
        {
            var utf8Json = JsonSerializer.NonGeneric.Utf8.Serialize<TUtf8Resolver>(this); // 反序列化的对象不排除动态对象，这儿禁用缓存
            return JsonSerializer.Generic.Utf8.Deserialize<T, TUtf8Resolver>(utf8Json);
        }

        protected virtual object ToObjectInternal<TUtf8Resolver, TUtf16Resolver>(Type objectType)
            where TUtf8Resolver : IJsonFormatterResolver<byte, TUtf8Resolver>, new()
            where TUtf16Resolver : IJsonFormatterResolver<char, TUtf16Resolver>, new()
        {
            var utf8Json = JsonSerializer.NonGeneric.Utf8.Serialize<TUtf8Resolver>(this); // 反序列化的对象不排除动态对象，这儿禁用缓存
            return JsonSerializer.NonGeneric.Utf8.Deserialize<TUtf8Resolver>(utf8Json, objectType);
        }

        protected virtual T ToObjectInternal<T, TUtf8Resolver, TUtf16Resolver>(NJsonSerializer jsonSerializer)
            where TUtf8Resolver : IJsonFormatterResolver<byte, TUtf8Resolver>, new()
            where TUtf16Resolver : IJsonFormatterResolver<char, TUtf16Resolver>, new()
        {
            var utf8Json = JsonSerializer.NonGeneric.Utf8.SerializeToArrayPool<TUtf8Resolver>(this);
            try
            {
                return (T)jsonSerializer.DeserializeFromByteArray(utf8Json.Array, utf8Json.Offset, utf8Json.Count, typeof(T));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(utf8Json.Array);
            }
        }

        protected virtual object ToObjectInternal<TUtf8Resolver, TUtf16Resolver>(Type objectType, NJsonSerializer jsonSerializer)
            where TUtf8Resolver : IJsonFormatterResolver<byte, TUtf8Resolver>, new()
            where TUtf16Resolver : IJsonFormatterResolver<char, TUtf16Resolver>, new()
        {
            var utf8Json = JsonSerializer.NonGeneric.Utf8.SerializeToArrayPool<TUtf8Resolver>(this);
            try
            {
                return jsonSerializer.DeserializeFromByteArray(utf8Json.Array, utf8Json.Offset, utf8Json.Count, objectType);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(utf8Json.Array);
            }
        }

        /// <summary>Writes this token to a <see cref="NJsonWriter"/>.</summary>
        /// <param name="writer">A <see cref="NJsonWriter"/> into which this method will write.</param>
        /// <param name="serializer">The calling serializer.</param>
        public abstract void WriteTo(NJsonWriter writer, NJsonSerializer serializer);

        /// <summary>Returns the JSON for this token.</summary>
        /// <returns>The JSON for this token.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return ToString(true);
        }

        /// <summary>Returns the JSON for this token using the given formatting and converters.</summary>
        /// <param name="writeIndented">Indicates how the output should be formatted.</param>
        /// <returns>The JSON for this token using the given formatting and converters.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(bool writeIndented)
        {
            if (writeIndented)
            {
                return ToStringInternal(this);
            }
            else
            {
                return JsonSerializer.NonGeneric.Utf16.Serialize<IncludeNullsOriginalCaseResolver<char>>(this);
            }
        }

        /// <summary>Returns the JSON for this token using the given resolver.</summary>
        /// <returns>The JSON for this token using the given resolver.</returns>
        public string ToString<TResolver>(bool writeIndented)
            where TResolver : IJsonFormatterResolver<char, TResolver>, new()
        {
            if (writeIndented)
            {
                var utf16Json = JsonSerializer.NonGeneric.Utf16.Serialize<TResolver>(this);
                return JsonSerializer.PrettyPrinter.Print(utf16Json);
            }
            else
            {
                return JsonSerializer.NonGeneric.Utf16.Serialize<TResolver>(this);
            }
        }

        private static string ToStringInternal(JToken token)
        {
            var sw = StringWriterManager.Allocate();
            var jsonSerializer = DefaultSerializerPool.Take();

            try
            {
                using (NJsonTextWriter jsonWriter = new NJsonTextWriter(sw))
                {
                    jsonWriter.ArrayPool = JsonConvertX.GlobalCharacterArrayPool;
                    jsonWriter.CloseOutput = false;
                    jsonWriter.Formatting = NFormatting.Indented;

                    token.WriteTo(jsonWriter, jsonSerializer);

                    jsonWriter.Flush();
                }
                return sw.ToString();
            }
            finally
            {
                StringWriterManager.Free(sw);
                DefaultSerializerPool.Return(jsonSerializer);
            }
        }
    }
}