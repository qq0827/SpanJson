namespace SpanJson.Formatters
{
    public sealed class Base64StringFormatter : ICustomJsonFormatter<byte[]>
    {
        public static readonly Base64StringFormatter Default = new Base64StringFormatter();

        public object Arguments { get; set; }

        public byte[] Deserialize(ref JsonReader<byte> reader, IJsonFormatterResolver<byte> resolver)
        {
            var token = reader.ReadUtf8NextToken();
            if (token == JsonTokenType.String)
            {
                return reader.ReadUtf8BytesFromBase64();
            }
            else
            {
                return ByteUtf8ArrayFormatter.Default.Deserialize(ref reader, resolver);
            }
        }

        public void Serialize(ref JsonWriter<byte> writer, byte[] value, IJsonFormatterResolver<byte> resolver)
        {
            if (null == value) { writer.WriteUtf8Null(); return; }

            writer.WriteUtf8Base64String(value);
        }


        public byte[] Deserialize(ref JsonReader<char> reader, IJsonFormatterResolver<char> resolver)
        {
            var token = reader.ReadUtf16NextToken();
            if (token == JsonTokenType.String)
            {
                return reader.ReadUtf16BytesFromBase64();
            }
            else
            {
                return ByteUtf16ArrayFormatter.Default.Deserialize(ref reader, resolver);
            }
        }

        public void Serialize(ref JsonWriter<char> writer, byte[] value, IJsonFormatterResolver<char> resolver)
        {
            if (null == value) { writer.WriteUtf16Null(); return; }

            writer.WriteUtf16Base64String(value);
        }
    }
}
