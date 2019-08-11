namespace SpanJson.Formatters
{
    public class NJArrayFormatter<TArray> : JTokenFormatterBase<TArray>
        where TArray : Newtonsoft.Json.Linq.JArray, new()
    {
        public static readonly NJArrayFormatter<TArray> Default = new NJArrayFormatter<TArray>();

        public override void Serialize(ref JsonWriter<byte> writer, TArray value, IJsonFormatterResolver<byte> resolver)
        {
            if (value == null) { writer.WriteUtf8Null(); return; }

            var valueLength = value.Count;
            writer.IncrementDepth();
            writer.WriteUtf8BeginArray();

            if (valueLength > 0)
            {
                var formatter = resolver.GetRuntimeFormatter();
                formatter.Serialize(ref writer, value[0], resolver);
                for (var i = 1; i < valueLength; i++)
                {
                    writer.WriteUtf8ValueSeparator();
                    formatter.Serialize(ref writer, value[i], resolver);
                }
            }

            writer.DecrementDepth();
            writer.WriteUtf8EndArray();
        }

        public override void Serialize(ref JsonWriter<char> writer, TArray value, IJsonFormatterResolver<char> resolver)
        {
            if (value == null) { writer.WriteUtf16Null(); return; }

            var valueLength = value.Count;
            writer.IncrementDepth();
            writer.WriteUtf16BeginArray();

            if (valueLength > 0)
            {
                var formatter = resolver.GetRuntimeFormatter();
                formatter.Serialize(ref writer, value[0], resolver);
                for (var i = 1; i < valueLength; i++)
                {
                    writer.WriteUtf16ValueSeparator();
                    formatter.Serialize(ref writer, value[i], resolver);
                }
            }

            writer.DecrementDepth();
            writer.WriteUtf16EndArray();
        }
    }
}
