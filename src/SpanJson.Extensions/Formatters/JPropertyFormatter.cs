using SpanJson.Linq;

namespace SpanJson.Formatters
{
    public sealed class JPropertyFormatter<TProperty> : JTokenFormatterBase<TProperty>
        where TProperty : JProperty
    {
        public static readonly JPropertyFormatter<TProperty> Default = new JPropertyFormatter<TProperty>();

        public override void Serialize(ref JsonWriter<byte> writer, TProperty value, IJsonFormatterResolver<byte> resolver)
        {
            if (null == value) { return; }

            writer.WriteUtf8Name(value.Name);
            var formatter = resolver.GetRuntimeFormatter();
            formatter.Serialize(ref writer, value.Value, resolver);
        }

        public override void Serialize(ref JsonWriter<char> writer, TProperty value, IJsonFormatterResolver<char> resolver)
        {
            if (null == value) { return; }

            writer.WriteUtf16Name(value.Name);
            var formatter = resolver.GetRuntimeFormatter();
            formatter.Serialize(ref writer, value.Value, resolver);
        }
    }
}
