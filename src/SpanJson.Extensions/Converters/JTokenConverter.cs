using System;
using SpanJson.Linq;

namespace SpanJson.Converters
{
    public class JTokenConverter : Newtonsoft.Json.JsonConverter
    {
        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            switch (value)
            {
                case null:
                    writer.WriteNull();
                    break;

                case JToken jToken:
                    jToken.WriteTo(writer, serializer.Converters);
                    break;

                default:
                    throw ThrowHelper2.GetJsonSerializationException<JToken>();
            }
        }

        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(JToken).IsAssignableFrom(objectType);
        }
    }
}