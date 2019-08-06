using System;
using System.Collections.Generic;
using SpanJson.Document;
using SpanJson.Dynamic;

namespace SpanJson.Linq
{
    partial class JArray
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

                case SpanJsonDynamicArray<byte> utf8Array:
                    if (utf8Array.TryGetRaw(out ArraySegment<byte> utf8Json))
                    {
                        return JsonSerializer.Generic.Utf8.Deserialize<T, TUtf8Resolver>(utf8Json);
                    }
                    else
                    {
                        return base.ToObjectInternal<T, TUtf8Resolver, TUtf16Resolver>();
                    }

                case SpanJsonDynamicArray<char> utf16Array:
                    if (utf16Array.TryGetRaw(out ArraySegment<char> utf16Json))
                    {
                        return JsonSerializer.Generic.Utf16.Deserialize<T, TUtf16Resolver>(utf16Json);
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

                case SpanJsonDynamicArray<byte> utf8Array:
                    if (utf8Array.TryGetRaw(out ArraySegment<byte> utf8Json))
                    {
                        return JsonSerializer.NonGeneric.Utf8.Deserialize<TUtf8Resolver>(utf8Json, objectType);
                    }
                    else
                    {
                        return base.ToObjectInternal<TUtf8Resolver, TUtf16Resolver>(objectType);
                    }

                case SpanJsonDynamicArray<char> utf16Array:
                    if (utf16Array.TryGetRaw(out ArraySegment<char> utf16Json))
                    {
                        return JsonSerializer.NonGeneric.Utf16.Deserialize<TUtf16Resolver>(utf16Json, objectType);
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
            writer.WriteStartArray();

            for (int i = 0; i < _values.Count; i++)
            {
                _values[i].WriteTo(writer, converters);
            }

            writer.WriteEndArray();
        }
    }
}
