using System;
using System.Net;
using CuteAnt;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Converters
{
    public sealed class IPAddressConverter : JsonConverter
    {
        public static readonly IPAddressConverter Instance = new IPAddressConverter();

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(IPAddress));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IPAddress ip = (IPAddress)value;
            writer.WriteValue(ip.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            return IPAddress.Parse(token.Value<string>());
        }
    }

    public sealed class IPEndPointConverter : JsonConverter
    {
        public static readonly IPEndPointConverter Instance = new IPEndPointConverter();

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(IPEndPoint));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IPEndPoint ep = (IPEndPoint)value;
            writer.WriteStartObject();
            writer.WritePropertyName("Address");
            serializer.Serialize(writer, ep.Address);
            writer.WritePropertyName("Port");
            writer.WriteValue(ep.Port);
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            IPAddress address = jo["Address"].ToObject<IPAddress>(serializer);
            int port = jo["Port"].Value<int>();
            return new IPEndPoint(address, port);
        }
    }

    /// <summary>Converts a <see cref="CombGuid"/> to and from a string.</summary>
    public sealed class CombGuidConverter : JsonConverter
    {
        public static readonly CombGuidConverter Instance = new CombGuidConverter();

        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing property value of the JSON that is being converted.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return CombGuid.Empty;
            }
            else
            {
                //Newtonsoft.Json.Linq.JToken token = Newtonsoft.Json.Linq.JToken.Load(reader);
                var str = reader.ReadAsString();// token.Value<string>();
                if (CombGuid.TryParse(str, CombGuidSequentialSegmentType.Comb, out CombGuid v))
                {
                    return v;
                }
                if (CombGuid.TryParse(str, CombGuidSequentialSegmentType.Guid, out v))
                {
                    return v;
                }
                return CombGuid.Empty;
            }
        }

        /// <summary>Writes the JSON representation of the object.</summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            CombGuid comb = (CombGuid)value;
            writer.WriteValue(comb.ToString());
        }

        /// <summary>Determines whether this instance can convert the specified object type.</summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns><c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.</returns>
        public override Boolean CanConvert(Type objectType)
        {
            return objectType == TypeConstants.CombGuidType || objectType == typeof(CombGuid?);
        }
    }
}
