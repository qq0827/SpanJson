using SpanJson.Linq;
using NJArray = Newtonsoft.Json.Linq.JArray;
using NJProperty = Newtonsoft.Json.Linq.JProperty;
using NJObject = Newtonsoft.Json.Linq.JObject;
using NJToken = Newtonsoft.Json.Linq.JToken;
using NJValue = Newtonsoft.Json.Linq.JValue;
using NJTokenType = Newtonsoft.Json.Linq.JTokenType;

namespace SpanJson.Formatters
{
    public sealed class NJTokenFormatter : JTokenFormatterBase<NJToken>
    {
        public static readonly NJTokenFormatter Default = new NJTokenFormatter();

        public override NJToken Deserialize(ref JsonReader<byte> reader, IJsonFormatterResolver<byte> resolver)
        {
            var token = JToken.Load(ref reader);
            return token.ToPolymorphicObject<NJToken>();
        }

        public override NJToken Deserialize(ref JsonReader<char> reader, IJsonFormatterResolver<char> resolver)
        {
            var token = JToken.Load(ref reader);
            return token.ToPolymorphicObject<NJToken>();
        }

        public override void Serialize(ref JsonWriter<byte> writer, NJToken value, IJsonFormatterResolver<byte> resolver)
        {
            if (value is null) { return; }

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
                    NJValueFormatter<NJValue>.Default.Serialize(ref writer, (NJValue)value, resolver);
                    break;
            }
        }

        public override void Serialize(ref JsonWriter<char> writer, NJToken value, IJsonFormatterResolver<char> resolver)
        {
            if (value is null) { return; }

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
                    NJValueFormatter<NJValue>.Default.Serialize(ref writer, (NJValue)value, resolver);
                    break;
            }
        }
    }
}
