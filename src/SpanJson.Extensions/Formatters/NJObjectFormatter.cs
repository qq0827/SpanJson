namespace SpanJson.Formatters
{
    public class NJObjectFormatter<TObject> : JTokenFormatterBase<TObject>
        where TObject : Newtonsoft.Json.Linq.JObject, new()
    {
        public static readonly NJObjectFormatter<TObject> Default = new NJObjectFormatter<TObject>();

        public override void Serialize(ref JsonWriter<byte> writer, TObject value, IJsonFormatterResolver<byte> resolver)
        {
            if (value == null) { writer.WriteUtf8Null(); return; }

            var valueLength = value.Count;

            writer.IncrementDepth();
            writer.WriteUtf8BeginObject();

            if (valueLength > 0)
            {
                var counter = 0;
                var formatter = resolver.GetRuntimeFormatter();
                foreach (var item in value)
                {
                    writer.IncrementDepth();
                    writer.WriteUtf8Name(resolver.GetEncodedPropertyName(item.Key));
                    var propertyValue = item.Value;
                    if (propertyValue != null)
                    {
                        formatter.Serialize(ref writer, propertyValue, resolver);
                    }
                    else
                    {
                        writer.WriteUtf8Null();
                    }
                    writer.DecrementDepth();
                    if (counter++ < valueLength - 1)
                    {
                        writer.WriteUtf8ValueSeparator();
                    }
                }
            }

            writer.DecrementDepth();
            writer.WriteUtf8EndObject();
        }

        public override void Serialize(ref JsonWriter<char> writer, TObject value, IJsonFormatterResolver<char> resolver)
        {
            if (value == null) { writer.WriteUtf16Null(); return; }

            var valueLength = value.Count;

            writer.IncrementDepth();
            writer.WriteUtf16BeginObject();

            if (valueLength > 0)
            {
                var counter = 0;
                var formatter = resolver.GetRuntimeFormatter();
                foreach (var item in value)
                {
                    writer.IncrementDepth();
                    writer.WriteUtf16Name(resolver.GetEncodedPropertyName(item.Key));
                    var propertyValue = item.Value;
                    if (propertyValue != null)
                    {
                        formatter.Serialize(ref writer, propertyValue, resolver);
                    }
                    else
                    {
                        writer.WriteUtf16Null();
                    }
                    writer.DecrementDepth();
                    if (counter++ < valueLength - 1)
                    {
                        writer.WriteUtf16ValueSeparator();
                    }
                }
            }

            writer.DecrementDepth();
            writer.WriteUtf16EndObject();
        }
    }
}
