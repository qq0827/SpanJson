using SpanJson.Linq;
using SpanJson.Internal;

namespace SpanJson.Formatters
{
    public sealed class JObjectFormatter : JObjectFormatter<JObject>
    {
        public new static readonly JObjectFormatter Default = new JObjectFormatter();

        public override JObject Deserialize(ref JsonReader<byte> reader, IJsonFormatterResolver<byte> resolver)
        {
            return JObject.Load(ref reader);
        }

        public override JObject Deserialize(ref JsonReader<char> reader, IJsonFormatterResolver<char> resolver)
        {
            return JObject.Load(ref reader);
        }
    }

    public class JObjectFormatter<TObject> : JTokenFormatterBase<TObject>
        where TObject : JObject, new()
    {
        public static readonly JObjectFormatter<TObject> Default = new JObjectFormatter<TObject>();

        public override void Serialize(ref JsonWriter<byte> writer, TObject value, IJsonFormatterResolver<byte> resolver)
        {
            if (value is null) { writer.WriteUtf8Null(); return; }

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
                    writer.WriteUtf8Name(EscapingHelper.GetEncodedText(item.Key, resolver.EscapeHandling));
                    var propertyValue = item.Value;
                    if (propertyValue is object)
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
            if (value is null) { writer.WriteUtf16Null(); return; }

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
                    writer.WriteUtf16Name(EscapingHelper.GetEncodedText(item.Key, resolver.EscapeHandling));
                    var propertyValue = item.Value;
                    if (propertyValue is object)
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
