using SpanJson.Linq;
using NJObject = Newtonsoft.Json.Linq.JObject;

namespace SpanJson.Formatters
{
    public sealed class NJObjectFormatter : NJObjectFormatter<NJObject>
    {
        public new static readonly NJObjectFormatter Default = new NJObjectFormatter();

        public override NJObject Deserialize(ref JsonReader<byte> reader, IJsonFormatterResolver<byte> resolver)
        {
            var jobj = JObject.Load(ref reader);
            return jobj.ToPolymorphicObject<NJObject>();
        }

        public override NJObject Deserialize(ref JsonReader<char> reader, IJsonFormatterResolver<char> resolver)
        {
            var jobj = JObject.Load(ref reader);
            return jobj.ToPolymorphicObject<NJObject>();
        }
    }

    public class NJObjectFormatter<TObject> : JTokenFormatterBase<TObject>
        where TObject : NJObject, new()
    {
        public static readonly NJObjectFormatter<TObject> Default = new NJObjectFormatter<TObject>();

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
                    writer.WriteUtf8Name(resolver.GetEncodedPropertyName(item.Key));
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
                    writer.WriteUtf16Name(resolver.GetEncodedPropertyName(item.Key));
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
