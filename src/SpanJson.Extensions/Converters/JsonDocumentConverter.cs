using System;
using SpanJson.Document;

namespace SpanJson.Converters
{
    public class JsonDocumentConverter : Newtonsoft.Json.JsonConverter
    {
        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            switch (value)
            {
                case null:
                    writer.WriteNull();
                    break;

                case JsonDocument doc:
                    WriteElement(writer, doc.RootElement);
                    break;

                default:
                    throw ThrowHelper2.GetJsonSerializationException<JsonDocument>();
            }
        }

        protected void WriteElement(Newtonsoft.Json.JsonWriter writer, JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    writer.WriteValue(element);
                    break;
                case JsonValueKind.Number:
                    writer.WriteValue(element);
                    break;
                case JsonValueKind.True:
                    writer.WriteValue(true);
                    break;
                case JsonValueKind.False:
                    writer.WriteValue(false);
                    break;
                case JsonValueKind.Null:
                    writer.WriteNull();
                    break;
                case JsonValueKind.Object:
                    writer.WriteStartObject();
                    foreach (var item in element.EnumerateObject())
                    {
                        writer.WritePropertyName(item.Name);

                        WriteElement(writer, item.Value);
                    }
                    writer.WriteEndObject();
                    break;
                case JsonValueKind.Array:
                    writer.WriteStartArray();
                    foreach (var item in element.EnumerateArray())
                    {
                        WriteElement(writer, item);
                    }
                    writer.WriteEndArray();
                    break;
                case JsonValueKind.Undefined:
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
            return objectType == typeof(JsonDocument);
        }
    }
}