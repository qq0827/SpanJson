using System;
using SpanJson.Document;

namespace SpanJson.Converters
{
    public class JsonElementConverter : JsonDocumentConverter
    {
        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            switch (value)
            {
                case null:
                    writer.WriteNull();
                    break;

                case JsonElement element:
                    WriteElement(writer, element);
                    break;

                default:
                    throw ThrowHelper2.GetJsonSerializationException<JsonElement>();
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(JsonElement);
        }
    }
}