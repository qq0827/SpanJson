using System;

namespace SpanJson.Converters
{
    public abstract class CustomPrimitiveValueConverter<T> : Newtonsoft.Json.JsonConverter
    {
        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            switch (value)
            {
                case null:
                    writer.WriteNull();
                    break;

                case T pa:
                    writer.WriteValue(pa);
                    break;

                default:
                    throw ThrowHelper2.GetJsonSerializationException<T>();
            }
        }

        public sealed override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(T);
        }
    }
}
