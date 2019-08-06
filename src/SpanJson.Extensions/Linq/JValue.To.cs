using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using SpanJson.Document;
using SpanJson.Dynamic;
using SpanJson.Utilities;

namespace SpanJson.Linq
{
    partial class JValue
    {
        protected override T ToObjectInternal<T, TUtf8Resolver, TUtf16Resolver>()
        {
            var value = this.Value;
            switch (value)
            {
                case null:
                    return default;

                case JsonElement element:
                    return JsonSerializer.Generic.Utf8.Deserialize<T, TUtf8Resolver>(element.RawMemory);

                case SpanJsonDynamicUtf16Number utf16Number:
                    return JsonSerializer.Generic.Utf16.Deserialize<T, TUtf16Resolver>(utf16Number.Symbols);

                case SpanJsonDynamicUtf16String utf16String:
                    return JsonSerializer.Generic.Utf16.Deserialize<T, TUtf16Resolver>(utf16String.Symbols);

                case SpanJsonDynamicUtf8Number utf8Number:
                    return JsonSerializer.Generic.Utf8.Deserialize<T, TUtf8Resolver>(utf8Number.Symbols);

                case SpanJsonDynamicUtf8String utf8String:
                    return JsonSerializer.Generic.Utf8.Deserialize<T, TUtf8Resolver>(utf8String.Symbols);

                default:
                    if (typeof(T).IsAssignableFrom(value.GetType()))
                    {
                        return (T)value;
                    }
                    else
                    {
                        switch (_valueType)
                        {
                            case JTokenType.String:
                                return JsonSerializer.Generic.Utf16.Deserialize<T, TUtf16Resolver>($"\"{_value.ToString()}\"");
                            case JTokenType.Raw when _value is string utf16Json:
                                return JsonSerializer.Generic.Utf16.Deserialize<T, TUtf16Resolver>(utf16Json);
                            case JTokenType.Raw when _value is byte[] utf8Json:
                                return JsonSerializer.Generic.Utf8.Deserialize<T, TUtf8Resolver>(utf8Json);
                            default:
                                return base.ToObjectInternal<T, TUtf8Resolver, TUtf16Resolver>();
                        }
                    }
            }
        }

        protected override object ToObjectInternal<TUtf8Resolver, TUtf16Resolver>(Type objectType)
        {
            var value = this.Value;
            switch (value)
            {
                case null:
                    return default;

                case JsonElement element:
                    return JsonSerializer.NonGeneric.Utf8.Deserialize<TUtf8Resolver>(element.RawMemory, objectType);

                case SpanJsonDynamicUtf16Number utf16Number:
                    return JsonSerializer.NonGeneric.Utf16.Deserialize<TUtf16Resolver>(utf16Number.Symbols, objectType);

                case SpanJsonDynamicUtf16String utf16String:
                    return JsonSerializer.NonGeneric.Utf16.Deserialize<TUtf16Resolver>(utf16String.Symbols, objectType);

                case SpanJsonDynamicUtf8Number utf8Number:
                    return JsonSerializer.NonGeneric.Utf8.Deserialize<TUtf8Resolver>(utf8Number.Symbols, objectType);

                case SpanJsonDynamicUtf8String utf8String:
                    return JsonSerializer.NonGeneric.Utf8.Deserialize<TUtf8Resolver>(utf8String.Symbols, objectType);

                default:
                    if (objectType.IsAssignableFrom(value.GetType()))
                    {
                        return value;
                    }
                    else
                    {
                        switch (_valueType)
                        {
                            case JTokenType.String:
                                return JsonSerializer.NonGeneric.Utf16.Deserialize<TUtf16Resolver>($"\"{_value.ToString()}\"", objectType);
                            case JTokenType.Raw when _value is string utf16Json:
                                return JsonSerializer.NonGeneric.Utf16.Deserialize<TUtf16Resolver>(utf16Json, objectType);
                            case JTokenType.Raw when _value is byte[] utf8Json:
                                return JsonSerializer.NonGeneric.Utf8.Deserialize<TUtf8Resolver>(utf8Json, objectType);
                            default:
                                return base.ToObjectInternal<TUtf8Resolver, TUtf16Resolver>(objectType);
                        }
                    }
            }
        }

        /// <summary>Writes this token to a <see cref="Newtonsoft.Json.JsonWriter"/>.</summary>
        /// <param name="writer">A <see cref="Newtonsoft.Json.JsonWriter"/> into which this method will write.</param>
        /// <param name="converters">A collection of <see cref="Newtonsoft.Json.JsonConverter"/> which will be used when writing the token.</param>
        public override void WriteTo(Newtonsoft.Json.JsonWriter writer, IList<Newtonsoft.Json.JsonConverter> converters)
        {
            if (converters != null && converters.Count > 0 && _value != null)
            {
                var matchingConverter = GetMatchingConverter(converters, _value.GetType());
                if (matchingConverter != null && matchingConverter.CanWrite)
                {
                    matchingConverter.WriteJson(writer, _value, Newtonsoft.Json.JsonSerializer.Create(DefaultSettings));
                    return;
                }
            }

            switch (_valueType)
            {
                case JTokenType.Comment:
                    writer.WriteComment(_value?.ToString());
                    return;
                case JTokenType.Raw:
                    writer.WriteRawValue(_value?.ToString());
                    return;
                case JTokenType.Null:
                    writer.WriteNull();
                    return;
                case JTokenType.Undefined:
                    if (writer is JTokenWriter tokenWriter)
                    {
                        tokenWriter.WriteUndefined(_value);
                    }
                    else
                    {
                        writer.WriteUndefined();
                    }
                    return;
                case JTokenType.Integer:
                    switch (_value)
                    {
                        case int iv:
                            writer.WriteValue(iv);
                            break;

                        case long lv:
                            writer.WriteValue(lv);
                            break;

                        case ulong ulv:
                            writer.WriteValue(ulv);
                            break;

                        case BigInteger integer:
                            writer.WriteValue(integer);
                            break;

                        case SpanJsonDynamicUtf16Number _:
                        case SpanJsonDynamicUtf8Number _:
                        case JsonElement _:
                            writer.WriteValue(_value);
                            break;

                        default:
                            writer.WriteValue(Convert.ToInt64(_value, CultureInfo.InvariantCulture));
                            break;
                    }
                    return;
                case JTokenType.Float:
                    switch (_value)
                    {
                        case decimal dec:
                            writer.WriteValue(dec);
                            break;

                        case double d:
                            writer.WriteValue(d);
                            break;

                        case float f:
                            writer.WriteValue(f);
                            break;

                        case SpanJsonDynamicUtf16Number _:
                        case SpanJsonDynamicUtf8Number _:
                        case JsonElement _:
                            writer.WriteValue(_value);
                            break;

                        default:
                            writer.WriteValue(Convert.ToDouble(_value, CultureInfo.InvariantCulture));
                            break;
                    }
                    return;
                case JTokenType.String:
                    writer.WriteValue(_value?.ToString());
                    return;
                case JTokenType.Boolean:
                    writer.WriteValue(Convert.ToBoolean(_value, CultureInfo.InvariantCulture));
                    return;
                case JTokenType.Date:
                    if (_value is DateTimeOffset offset)
                    {
                        writer.WriteValue(offset);
                    }
                    else
                    {
                        writer.WriteValue(Convert.ToDateTime(_value, CultureInfo.InvariantCulture));
                    }
                    return;
                case JTokenType.Bytes:
                    writer.WriteValue((byte[])_value);
                    return;
                case JTokenType.Guid:
                    writer.WriteValue((_value != null) ? (Guid?)_value : null);
                    return;
                case JTokenType.TimeSpan:
                    writer.WriteValue((_value != null) ? (TimeSpan?)_value : null);
                    return;
                case JTokenType.Uri:
                    writer.WriteValue((Uri)_value);
                    return;
                case JTokenType.Dynamic:
                    writer.WriteValue(_value);
                    return;
                case JTokenType.CombGuid:
                    (writer as JTokenWriter)?.WriteValue((_value != null) ? (CuteAnt.CombGuid?)_value : null);
                    return;
            }

            throw MiscellaneousUtils.CreateArgumentOutOfRangeException(nameof(Type), _valueType, "Unexpected token type.");
        }

        private static Newtonsoft.Json.JsonConverter GetMatchingConverter(IList<Newtonsoft.Json.JsonConverter> converters, Type objectType)
        {
            if (converters != null)
            {
                for (int i = 0; i < converters.Count; i++)
                {
                    var converter = converters[i];

                    if (converter.CanConvert(objectType))
                    {
                        return converter;
                    }
                }
            }

            return null;
        }
    }
}
