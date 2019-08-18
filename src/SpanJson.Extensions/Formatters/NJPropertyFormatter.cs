using SpanJson.Internal;

namespace SpanJson.Formatters
{
    public sealed class NJPropertyFormatter<TProperty> : JTokenFormatterBase<TProperty>
        where TProperty : Newtonsoft.Json.Linq.JProperty
    {
        public static readonly NJPropertyFormatter<TProperty> Default = new NJPropertyFormatter<TProperty>();

        public override void Serialize(ref JsonWriter<byte> writer, TProperty value, IJsonFormatterResolver<byte> resolver)
        {
            if (value is null) { return; }

            var encodedName = EscapingHelper.GetEncodedText(value.Name, resolver.EscapeHandling);
            writer.WriteUtf8Name(encodedName);
            var pv = value.Value;
            if (pv is object)
            {
                var formatter = resolver.GetRuntimeFormatter();
                formatter.Serialize(ref writer, pv, resolver);
            }
            else
            {
                writer.WriteUtf8Null();
            }
        }

        public override void Serialize(ref JsonWriter<char> writer, TProperty value, IJsonFormatterResolver<char> resolver)
        {
            if (value is null) { return; }

            var encodedName = EscapingHelper.GetEncodedText(value.Name, resolver.EscapeHandling);
            writer.WriteUtf16Name(encodedName);
            var pv = value.Value;
            if (pv is object)
            {
                var formatter = resolver.GetRuntimeFormatter();
                formatter.Serialize(ref writer, pv, resolver);
            }
            else
            {
                writer.WriteUtf16Null();
            }
        }
    }
}
