using System;
using SpanJson.Dynamic;

namespace SpanJson.Converters
{
    public class DynamicUtf8ArrayConverter : DynamicObjectConverter
    {
        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            switch (value)
            {
                case null:
                    writer.WriteNull();
                    break;

                case SpanJsonDynamicArray<byte> _:
                    WriteDynamicObject(writer, value);
                    break;

                default:
                    throw ThrowHelper2.GetJsonSerializationException<SpanJsonDynamicArray<byte>>();
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SpanJsonDynamicArray<byte>);
        }
    }
}
