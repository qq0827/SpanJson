using SpanJson.Linq;

namespace SpanJson.Formatters
{
    public sealed class JTokenFormatter : JTokenFormatterBase<JToken>
    {
        public static readonly JTokenFormatter Default = new JTokenFormatter();

        public override void Serialize(ref JsonWriter<byte> writer, JToken value, IJsonFormatterResolver<byte> resolver)
        {
            if (null == value) { return; }

            switch (value.Type)
            {
                case JTokenType.Object:
                    JObjectFormatter<JObject>.Default.Serialize(ref writer, (JObject)value, resolver);
                    break;
                case JTokenType.Array:
                    JArrayFormatter<JArray>.Default.Serialize(ref writer, (JArray)value, resolver);
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
            if (null == value) { return; }

            switch (value.Type)
            {
                case JTokenType.Object:
                    JObjectFormatter<JObject>.Default.Serialize(ref writer, (JObject)value, resolver);
                    break;
                case JTokenType.Array:
                    JArrayFormatter<JArray>.Default.Serialize(ref writer, (JArray)value, resolver);
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
