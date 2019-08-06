using System;
using SpanJson.Dynamic;

namespace SpanJson.Converters
{
    public class DynamicUtf16ArrayConverter : DynamicObjectConverter
    {
        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            switch (value)
            {
                case null:
                    writer.WriteNull();
                    break;

                case SpanJsonDynamicArray<char> _:
                    WriteDynamicObject(writer, value);
                    break;

                default:
                    throw ThrowHelper2.GetJsonSerializationException<SpanJsonDynamicArray<char>>();
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SpanJsonDynamicArray<char>);
        }
    }
}
