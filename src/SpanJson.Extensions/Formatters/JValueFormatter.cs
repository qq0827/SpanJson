using SpanJson.Linq;

namespace SpanJson.Formatters
{
    public sealed class JValueFormatter<TValue> : JTokenFormatterBase<TValue>
        where TValue : JValue
    {
        public static readonly JValueFormatter<TValue> Default = new JValueFormatter<TValue>();

        public override void Serialize(ref JsonWriter<byte> writer, TValue value, IJsonFormatterResolver<byte> resolver)
        {
            if (null == value || value.Type == JTokenType.Null || null == value.Value)
            {
                writer.WriteUtf8Null();
                return;
            }

            var formatter = resolver.GetRuntimeFormatter();
            formatter.Serialize(ref writer, value.Value, resolver);
        }

        public override void Serialize(ref JsonWriter<char> writer, TValue value, IJsonFormatterResolver<char> resolver)
        {
            if (null == value || value.Type == JTokenType.Null || null == value.Value)
            {
                writer.WriteUtf16Null();
                return;
            }

            var formatter = resolver.GetRuntimeFormatter();
            formatter.Serialize(ref writer, value.Value, resolver);
        }
    }
}
