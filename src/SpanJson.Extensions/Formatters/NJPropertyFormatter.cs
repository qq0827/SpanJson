namespace SpanJson.Formatters
{
    public sealed class NJPropertyFormatter<TProperty> : JTokenFormatterBase<TProperty>
        where TProperty : Newtonsoft.Json.Linq.JProperty
    {
        public static readonly NJPropertyFormatter<TProperty> Default = new NJPropertyFormatter<TProperty>();

        public override void Serialize(ref JsonWriter<byte> writer, TProperty value, IJsonFormatterResolver<byte> resolver)
        {
            if (null == value) { return; }

            var encodedName = resolver.GetEncodedPropertyName(value.Name);
            writer.WriteUtf8Name(encodedName);
            var pv = value.Value;
            if (pv != null)
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
            if (null == value) { return; }

            var encodedName = resolver.GetEncodedPropertyName(value.Name);
            writer.WriteUtf16Name(encodedName);
            var pv = value.Value;
            if (pv != null)
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
