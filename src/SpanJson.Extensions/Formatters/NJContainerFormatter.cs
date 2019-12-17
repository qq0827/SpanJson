using SpanJson.Linq;
using NJArray = Newtonsoft.Json.Linq.JArray;
using NJContainer = Newtonsoft.Json.Linq.JContainer;
using NJProperty = Newtonsoft.Json.Linq.JProperty;
using NJObject = Newtonsoft.Json.Linq.JObject;
using NJTokenType = Newtonsoft.Json.Linq.JTokenType;

namespace SpanJson.Formatters
{
    public sealed class NJContainerFormatter : JTokenFormatterBase<NJContainer>
    {
        public static readonly NJContainerFormatter Default = new NJContainerFormatter();

        public override NJContainer Deserialize(ref JsonReader<byte> reader, IJsonFormatterResolver<byte> resolver)
        {
            if (reader.ReadUtf8IsNull()) { return null; }

            var token = JToken.Load(ref reader);
            switch (token.Type)
            {
                case JTokenType.Object:
                case JTokenType.Array:
                    return token.ToPolymorphicObject<NJContainer>();

                default:
                    throw ThrowHelper2.GetJsonReaderException_Error_reading_JContainer_from_JsonReader();
            }
        }

        public override NJContainer Deserialize(ref JsonReader<char> reader, IJsonFormatterResolver<char> resolver)
        {
            if (reader.ReadUtf16IsNull()) { return null; }

            var token = JToken.Load(ref reader);
            switch (token.Type)
            {
                case JTokenType.Object:
                case JTokenType.Array:
                    return token.ToPolymorphicObject<NJContainer>();

                default:
                    throw ThrowHelper2.GetJsonReaderException_Error_reading_JContainer_from_JsonReader();
            }
        }

        public override void Serialize(ref JsonWriter<byte> writer, NJContainer value, IJsonFormatterResolver<byte> resolver)
        {
            if (value is null) { writer.WriteUtf8Null(); return; }

            switch (value.Type)
            {
                case NJTokenType.Object:
                    NJObjectFormatter<NJObject>.Default.Serialize(ref writer, (NJObject)value, resolver);
                    break;
                case NJTokenType.Array:
                    NJArrayFormatter<NJArray>.Default.Serialize(ref writer, (NJArray)value, resolver);
                    break;
                case NJTokenType.Property:
                    NJPropertyFormatter<NJProperty>.Default.Serialize(ref writer, (NJProperty)value, resolver);
                    break;
                default:
                    throw ThrowHelper.GetNotSupportedException();
            }
        }

        public override void Serialize(ref JsonWriter<char> writer, NJContainer value, IJsonFormatterResolver<char> resolver)
        {
            if (value is null) { writer.WriteUtf16Null(); return; }

            switch (value.Type)
            {
                case NJTokenType.Object:
                    NJObjectFormatter<NJObject>.Default.Serialize(ref writer, (NJObject)value, resolver);
                    break;
                case NJTokenType.Array:
                    NJArrayFormatter<NJArray>.Default.Serialize(ref writer, (NJArray)value, resolver);
                    break;
                case NJTokenType.Property:
                    NJPropertyFormatter<NJProperty>.Default.Serialize(ref writer, (NJProperty)value, resolver);
                    break;
                default:
                    throw ThrowHelper.GetNotSupportedException();
            }
        }
    }
}
