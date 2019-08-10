using System;
using System.Runtime.InteropServices;
using SpanJson.Document;
using SpanJson.Dynamic;
using SpanJson.Serialization;
using NJsonSerializer = Newtonsoft.Json.JsonSerializer;
using NJsonWriter = Newtonsoft.Json.JsonWriter;

namespace SpanJson.Linq
{
    partial class JObject
    {
        protected override T ToObjectInternal<T, TUtf8Resolver, TUtf16Resolver>()
        {
            var value = _dynamicJson;
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
            switch (value)
            {
                case ArraySegment<byte> utf8Data:
                    return JsonSerializer.NonGeneric.Utf8.Deserialize<TUtf8Resolver>(utf8Data, objectType);

                case ArraySegment<char> utf16Data:
                    return JsonSerializer.NonGeneric.Utf16.Deserialize<TUtf16Resolver>(utf16Data, objectType);

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

        protected override T ToObjectInternal<T, TUtf8Resolver, TUtf16Resolver>(NJsonSerializer jsonSerializer)
        {
            var value = _dynamicJson;
            switch (value)
            {
                case ArraySegment<byte> utf8Data:
                    return (T)jsonSerializer.DeserializeFromByteArray(utf8Data.Array, utf8Data.Offset, utf8Data.Count, typeof(T));

                case ArraySegment<char> utf16Data:
                    return (T)jsonSerializer.DeserializeObject(utf16Data.AsSpan().ToString(), typeof(T));

                case JsonDocument doc:
                    {
                        var rawMemory = doc.RawMemory;
                        if (MemoryMarshal.TryGetArray(rawMemory, out ArraySegment<byte> utf8Json))
                        {
                            return (T)jsonSerializer.DeserializeFromByteArray(utf8Json.Array, utf8Json.Offset, utf8Json.Count, typeof(T));
                        }
                        else
                        {
                            return (T)jsonSerializer.DeserializeFromByteArray(rawMemory.ToArray(), typeof(T));
                        }
                    }

                case JsonElement element:
                    {
                        var rawMemory = element.RawMemory;
                        if (MemoryMarshal.TryGetArray(rawMemory, out ArraySegment<byte> utf8Json))
                        {
                            return (T)jsonSerializer.DeserializeFromByteArray(utf8Json.Array, utf8Json.Offset, utf8Json.Count, typeof(T));
                        }
                        else
                        {
                            return (T)jsonSerializer.DeserializeFromByteArray(rawMemory.ToArray(), typeof(T));
                        }
                    }

                case SpanJsonDynamicObject dynamicObject:
                    if (dynamicObject.HasRaw)
                    {
                        if (dynamicObject.IsUtf16)
                        {
                            return (T)jsonSerializer.DeserializeObject(dynamicObject.Utf16Raw.AsSpan().ToString(), typeof(T));
                        }
                        else
                        {
                            var utf8Json = dynamicObject.Utf8Raw;
                            return (T)jsonSerializer.DeserializeFromByteArray(utf8Json.Array, utf8Json.Offset, utf8Json.Count, typeof(T));
                        }
                    }
                    else
                    {
                        return base.ToObjectInternal<T, TUtf8Resolver, TUtf16Resolver>(jsonSerializer);
                    }

                default:
                    return base.ToObjectInternal<T, TUtf8Resolver, TUtf16Resolver>(jsonSerializer);
            }
        }

        protected override object ToObjectInternal<TUtf8Resolver, TUtf16Resolver>(Type objectType, NJsonSerializer jsonSerializer)
        {
            var value = _dynamicJson;
            switch (value)
            {
                case ArraySegment<byte> utf8Data:
                    return jsonSerializer.DeserializeFromByteArray(utf8Data.Array, utf8Data.Offset, utf8Data.Count, objectType);

                case ArraySegment<char> utf16Data:
                    return jsonSerializer.DeserializeObject(utf16Data.AsSpan().ToString(), objectType);

                case JsonDocument doc:
                    {
                        var rawMemory = doc.RawMemory;
                        if (MemoryMarshal.TryGetArray(rawMemory, out ArraySegment<byte> utf8Json))
                        {
                            return jsonSerializer.DeserializeFromByteArray(utf8Json.Array, utf8Json.Offset, utf8Json.Count, objectType);
                        }
                        else
                        {
                            return jsonSerializer.DeserializeFromByteArray(rawMemory.ToArray(), objectType);
                        }
                    }

                case JsonElement element:
                    {
                        var rawMemory = element.RawMemory;
                        if (MemoryMarshal.TryGetArray(rawMemory, out ArraySegment<byte> utf8Json))
                        {
                            return jsonSerializer.DeserializeFromByteArray(utf8Json.Array, utf8Json.Offset, utf8Json.Count, objectType);
                        }
                        else
                        {
                            return jsonSerializer.DeserializeFromByteArray(rawMemory.ToArray(), objectType);
                        }
                    }

                case SpanJsonDynamicObject dynamicObject:
                    if (dynamicObject.HasRaw)
                    {
                        if (dynamicObject.IsUtf16)
                        {
                            return jsonSerializer.DeserializeObject(dynamicObject.Utf16Raw.AsSpan().ToString(), objectType);
                        }
                        else
                        {
                            var utf8Json = dynamicObject.Utf8Raw;
                            return jsonSerializer.DeserializeFromByteArray(utf8Json.Array, utf8Json.Offset, utf8Json.Count, objectType);
                        }
                    }
                    else
                    {
                        return base.ToObjectInternal<TUtf8Resolver, TUtf16Resolver>(objectType, jsonSerializer);
                    }

                default:
                    return base.ToObjectInternal<TUtf8Resolver, TUtf16Resolver>(objectType, jsonSerializer);
            }
        }

        /// <summary>Writes this token to a <see cref="NJsonWriter"/>.</summary>
        /// <param name="writer">A <see cref="NJsonWriter"/> into which this method will write.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteTo(NJsonWriter writer, NJsonSerializer serializer)
        {
            writer.WriteStartObject();

            for (int i = 0; i < _properties.Count; i++)
            {
                _properties[i].WriteTo(writer, serializer);
            }

            writer.WriteEndObject();
        }
    }
}
