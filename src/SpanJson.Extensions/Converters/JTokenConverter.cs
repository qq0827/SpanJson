using System;
using SpanJson.Linq;
using NJsonConverter = Newtonsoft.Json.JsonConverter;
using NJsonReader = Newtonsoft.Json.JsonReader;
using NJsonSerializer = Newtonsoft.Json.JsonSerializer;
using NJsonWriter = Newtonsoft.Json.JsonWriter;
using NJToken = Newtonsoft.Json.Linq.JToken;
using NJContainer = Newtonsoft.Json.Linq.JContainer;
using NJTokenType = Newtonsoft.Json.Linq.JTokenType;

namespace SpanJson.Converters
{
    public sealed class JTokenConverter : NJsonConverter
    {
        public static readonly JTokenConverter Instance = new JTokenConverter();

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
            var token = NJToken.ReadFrom(reader, null);
            if (token.Type == NJTokenType.Null && typeof(NJContainer).IsAssignableFrom(objectType))
            {
                return null;
            }
            return JToken.FromObject(token);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(JToken).IsAssignableFrom(objectType);
        }
    }
}