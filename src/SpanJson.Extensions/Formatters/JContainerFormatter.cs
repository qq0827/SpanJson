using SpanJson.Linq;

namespace SpanJson.Formatters
{
    public sealed class JContainerFormatter : JTokenFormatterBase<JContainer>
    {
        public static readonly JContainerFormatter Default = new JContainerFormatter();

        public override void Serialize(ref JsonWriter<byte> writer, JContainer value, IJsonFormatterResolver<byte> resolver)
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
                    throw ThrowHelper.GetNotSupportedException();
            }
        }

        public override void Serialize(ref JsonWriter<char> writer, JContainer value, IJsonFormatterResolver<char> resolver)
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
                    throw ThrowHelper.GetNotSupportedException();
            }
        }
    }
}
