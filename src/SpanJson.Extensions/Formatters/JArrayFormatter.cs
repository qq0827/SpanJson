using SpanJson.Linq;

namespace SpanJson.Formatters
{
    public sealed class JArrayFormatter : JArrayFormatter<JArray>
    {
        public new static readonly JArrayFormatter Default = new JArrayFormatter();

        public override JArray Deserialize(ref JsonReader<byte> reader, IJsonFormatterResolver<byte> resolver)
        {
            var token = JToken.ParseCore(ref reader, 0);

            return JArray.ToJArray(token);
        }

        public override JArray Deserialize(ref JsonReader<char> reader, IJsonFormatterResolver<char> resolver)
        {
            var token = JToken.ParseCore(ref reader, 0);

            return JArray.ToJArray(token);
        }

    }

    public class JArrayFormatter<TArray> : JTokenFormatterBase<TArray>
        where TArray : JArray, new()
    {
        public static readonly JArrayFormatter<TArray> Default = new JArrayFormatter<TArray>();

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
