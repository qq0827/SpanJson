using SpanJson.Linq;

namespace SpanJson.Formatters
{
    public sealed class JTokenFormatter : JTokenFormatterBase<JToken>
    {
        public static readonly JTokenFormatter Default = new JTokenFormatter();

        public override JToken Deserialize(ref JsonReader<byte> reader, IJsonFormatterResolver<byte> resolver)
        {
            return JToken.Load(ref reader);
        }

        public override JToken Deserialize(ref JsonReader<char> reader, IJsonFormatterResolver<char> resolver)
        {
            return JToken.Load(ref reader);
        }

        public override void Serialize(ref JsonWriter<byte> writer, JToken value, IJsonFormatterResolver<byte> resolver)
        {
            if (value is null) { return; }

            switch (value.Type)
            {
                case JTokenType.Object:
                    JObjectFormatter.Default.Serialize(ref writer, (JObject)value, resolver);
                    break;
                case JTokenType.Array:
                    JArrayFormatter.Default.Serialize(ref writer, (JArray)value, resolver);
                    break;
                case JTokenType.Property:
                    JPropertyFormatter<JProperty>.Default.Serialize(ref writer, (JProperty)value, resolver);
                    break;
                default:
                    JValueFormatter<JValue>.Default.Serialize(ref writer, (JValue)value, resolver);
                    break;
            }
        }

        public override void Serialize(ref JsonWriter<char> writer, JToken value, IJsonFormatterResolver<char> resolver)
        {
            if (value is null) { return; }

            switch (value.Type)
            {
                case JTokenType.Object:
                    JObjectFormatter.Default.Serialize(ref writer, (JObject)value, resolver);
                    break;
                case JTokenType.Array:
                    JArrayFormatter.Default.Serialize(ref writer, (JArray)value, resolver);
                    break;
                case JTokenType.Property:
                    JPropertyFormatter<JProperty>.Default.Serialize(ref writer, (JProperty)value, resolver);
                    break;
                default:
                    JValueFormatter<JValue>.Default.Serialize(ref writer, (JValue)value, resolver);
                    break;
            }
        }
    }
}
