using SpanJson.Document;

namespace SpanJson.Formatters
{
    public sealed class JsonDocumentFormatter : JsonElementFormatterBase<JsonDocument>
    {
        public static readonly JsonDocumentFormatter Default = new JsonDocumentFormatter();

        public override void Serialize(ref JsonWriter<byte> writer, JsonDocument value, IJsonFormatterResolver<byte> resolver)
        {
            if (value is null) { writer.WriteUtf8Null(); return; }

            JsonElementFormatter.Default.Serialize(ref writer, value.RootElement, resolver);
        }

        public override void Serialize(ref JsonWriter<char> writer, JsonDocument value, IJsonFormatterResolver<char> resolver)
        {
            if (value is null) { writer.WriteUtf16Null(); return; }

            JsonElementFormatter.Default.Serialize(ref writer, value.RootElement, resolver);
        }
    }
}
