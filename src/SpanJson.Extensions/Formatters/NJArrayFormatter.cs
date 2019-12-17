using SpanJson.Linq;
using NJArray = Newtonsoft.Json.Linq.JArray;

namespace SpanJson.Formatters
{
    public sealed class NJArrayFormatter : NJArrayFormatter<NJArray>
    {
        public new static readonly NJArrayFormatter Default = new NJArrayFormatter();

        public override NJArray Deserialize(ref JsonReader<byte> reader, IJsonFormatterResolver<byte> resolver)
        {
            if (reader.ReadUtf8IsNull()) { return null; }

            var jary = JArray.Load(ref reader);
            return jary.ToPolymorphicObject<NJArray>();
        }

        public override NJArray Deserialize(ref JsonReader<char> reader, IJsonFormatterResolver<char> resolver)
        {
            if (reader.ReadUtf16IsNull()) { return null; }

            var jary = JArray.Load(ref reader);
            return jary.ToPolymorphicObject<NJArray>();
        }
    }

    public class NJArrayFormatter<TArray> : JTokenFormatterBase<TArray>
        where TArray : NJArray, new()
    {
        public static readonly NJArrayFormatter<TArray> Default = new NJArrayFormatter<TArray>();

        public override void Serialize(ref JsonWriter<byte> writer, TArray value, IJsonFormatterResolver<byte> resolver)
        {
            if (value is null) { writer.WriteUtf8Null(); return; }

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
            if (value is null) { writer.WriteUtf16Null(); return; }

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
