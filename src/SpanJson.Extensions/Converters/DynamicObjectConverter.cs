using System;
using System.Collections.Generic;
using SpanJson.Dynamic;

namespace SpanJson.Converters
{
    public class DynamicObjectConverter : Newtonsoft.Json.JsonConverter
    {
        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            switch (value)
            {
                case null:
                    writer.WriteNull();
                    break;

                case SpanJsonDynamicObject _:
                    WriteDynamicObject(writer, value);
                    break;

                default:
                    throw ThrowHelper2.GetJsonSerializationException<SpanJsonDynamicObject>();
            }
        }

        protected void WriteDynamicObject(Newtonsoft.Json.JsonWriter writer, object value)
        {
            switch (value)
            {
                case SpanJsonDynamicObject dynamicObject:
                    var dict = (IDictionary<string, object>)(dynamic)dynamicObject;
                    writer.WriteStartObject();
                    foreach (var item in dict)
                    {
                        writer.WritePropertyName(item.Key);

                        WriteDynamicObject(writer, item.Value);
                    }
                    writer.WriteEndObject();
                    break;

                case ISpanJsonDynamicArray dynamicArray:
                    writer.WriteStartArray();
                    foreach (var item in dynamicArray)
                    {
                        WriteDynamicObject(writer, item);
                    }
                    writer.WriteEndArray();
                    break;

                case SpanJsonDynamicUtf16Number _:
                case SpanJsonDynamicUtf16String _:
                case SpanJsonDynamicUtf8Number _:
                case SpanJsonDynamicUtf8String _:
                    writer.WriteValue(value);
                    break;

                default:
                    throw ThrowHelper.GetNotSupportedException();
            }
        }

        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SpanJsonDynamicObject);
        }
    }
}
