using System;
using SpanJson.Document;
using SpanJson.Internal;

namespace SpanJson.Formatters
{
    public sealed class JsonElementFormatter : JsonElementFormatterBase<JsonElement>
    {
        public static readonly JsonElementFormatter Default = new JsonElementFormatter();

        public override void Serialize(ref JsonWriter<byte> writer, JsonElement value, IJsonFormatterResolver<byte> resolver)
        {
            var utf8Writer = new Utf8JsonWriter(1024, new JsonWriterOptions
            {
                Indented = false,
                Encoder = resolver.Encoder,
                EscapeHandling = resolver.EscapeHandling,
                PropertyNamingPolicy = resolver.JsonOptions.PropertyNamingPolicy,
                SkipValidation = false
            });
            try
            {
                value.WriteTo(ref utf8Writer);
                writer.WriteUtf8Verbatim(utf8Writer.WrittenSpan);
            }
            finally
            {
                utf8Writer.Dispose();
            }
        }

        public override void Serialize(ref JsonWriter<char> writer, JsonElement value, IJsonFormatterResolver<char> resolver)
        {
            var utf8Writer = new Utf8JsonWriter(1024, new JsonWriterOptions
            {
                Indented = false,
                Encoder = resolver.Encoder,
                EscapeHandling = resolver.EscapeHandling,
                PropertyNamingPolicy = resolver.JsonOptions.PropertyNamingPolicy,
                SkipValidation = false
            });
            try
            {
                value.WriteTo(ref utf8Writer);
                var utf16Json = TextEncodings.Utf8.GetString(utf8Writer.WrittenSpan);
                writer.WriteUtf16Verbatim(utf16Json.AsSpan());
            }
            finally
            {
                utf8Writer.Dispose();
            }
        }
    }
}
