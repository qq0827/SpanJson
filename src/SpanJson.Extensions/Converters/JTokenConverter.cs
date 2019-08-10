using System;
using SpanJson.Linq;
using NJsonConverter = Newtonsoft.Json.JsonConverter;
using NJsonReader = Newtonsoft.Json.JsonReader;
using NJsonSerializer = Newtonsoft.Json.JsonSerializer;
using NJsonWriter = Newtonsoft.Json.JsonWriter;

namespace SpanJson.Converters
{
    public class JTokenConverter : NJsonConverter
    {
        public override void WriteJson(NJsonWriter writer, object value, NJsonSerializer serializer)
        {
            switch (value)
            {
                case null:
                    writer.WriteNull();
                    break;

                case JToken jToken:
                    jToken.WriteTo(writer, serializer);
                    break;

                default:
                    throw ThrowHelper2.GetJsonSerializationException<JToken>();
            }
        }

        public override object ReadJson(NJsonReader reader, Type objectType, object existingValue, NJsonSerializer serializer)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(JToken).IsAssignableFrom(objectType);
        }
    }
}