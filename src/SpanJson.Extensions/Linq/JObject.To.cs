using System;
using System.Collections.Generic;
using SpanJson.Document;
using SpanJson.Dynamic;

namespace SpanJson.Linq
{
    partial class JObject
    {
        protected override T ToObjectInternal<T, TUtf8Resolver, TUtf16Resolver>()
        {
            var value = _dynamicJson;
            if (value == null)
            {
                return base.ToObjectInternal<T, TUtf8Resolver, TUtf16Resolver>();
            }
            switch (value)
            {
                case ArraySegment<byte> utf8Data:
                    return JsonSerializer.Generic.Utf8.Deserialize<T, TUtf8Resolver>(utf8Data);

                case ArraySegment<char> utf16Data:
                    return JsonSerializer.Generic.Utf16.Deserialize<T, TUtf16Resolver>(utf16Data);

                case JsonDocument doc:
                    return JsonSerializer.Generic.Utf8.Deserialize<T, TUtf8Resolver>(doc.RawMemory);

                case JsonElement element:
                    return JsonSerializer.Generic.Utf8.Deserialize<T, TUtf8Resolver>(element.RawMemory);

                case SpanJsonDynamicObject dynamicObject:
                    if (dynamicObject.HasRaw)
                    {
                        if (dynamicObject.IsUtf16)
                        {
                            return JsonSerializer.Generic.Utf16.Deserialize<T, TUtf16Resolver>(dynamicObject.Utf16Raw);
                        }
                        else
                        {
                            return JsonSerializer.Generic.Utf8.Deserialize<T, TUtf8Resolver>(dynamicObject.Utf8Raw);
                        }
                    }
                    else
                    {
                        return base.ToObjectInternal<T, TUtf8Resolver, TUtf16Resolver>();
                    }

                default:
                    return base.ToObjectInternal<T, TUtf8Resolver, TUtf16Resolver>();
            }
        }

        protected override object ToObjectInternal<TUtf8Resolver, TUtf16Resolver>(Type objectType)
        {
            var value = _dynamicJson;
            if (value == null)
            {
                return base.ToObjectInternal<TUtf8Resolver, TUtf16Resolver>(objectType);
            }
            switch (value)
            {
                case JsonDocument doc:
                    return JsonSerializer.NonGeneric.Utf8.Deserialize<TUtf8Resolver>(doc.RawMemory, objectType);

                case JsonElement element:
                    return JsonSerializer.NonGeneric.Utf8.Deserialize<TUtf8Resolver>(element.RawMemory, objectType);

                case SpanJsonDynamicObject dynamicObject:
                    if (dynamicObject.HasRaw)
                    {
                        if (dynamicObject.IsUtf16)
                        {
                            return JsonSerializer.NonGeneric.Utf16.Deserialize<TUtf16Resolver>(dynamicObject.Utf16Raw, objectType);
                        }
                        else
                        {
                            return JsonSerializer.NonGeneric.Utf8.Deserialize<TUtf8Resolver>(dynamicObject.Utf8Raw, objectType);
                        }
                    }
                    else
                    {
                        return base.ToObjectInternal<TUtf8Resolver, TUtf16Resolver>(objectType);
                    }

                default:
                    return base.ToObjectInternal<TUtf8Resolver, TUtf16Resolver>(objectType);
            }
        }

        /// <summary>Writes this token to a <see cref="Newtonsoft.Json.JsonWriter"/>.</summary>
        /// <param name="writer">A <see cref="Newtonsoft.Json.JsonWriter"/> into which this method will write.</param>
        /// <param name="converters">A collection of <see cref="Newtonsoft.Json.JsonConverter"/> which will be used when writing the token.</param>
        public override void WriteTo(Newtonsoft.Json.JsonWriter writer, IList<Newtonsoft.Json.JsonConverter> converters)
        {
            writer.WriteStartObject();

            for (int i = 0; i < _properties.Count; i++)
            {
                _properties[i].WriteTo(writer, converters);
            }

            writer.WriteEndObject();
        }
    }
}
