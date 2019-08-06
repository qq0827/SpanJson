namespace SpanJson.Formatters
{
    public abstract class JTokenFormatterBase<T> : ICustomJsonFormatter<T>
    {
        public object Arguments { get; set; }

        public virtual T Deserialize(ref JsonReader<byte> reader, IJsonFormatterResolver<byte> resolver)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        public virtual T Deserialize(ref JsonReader<char> reader, IJsonFormatterResolver<char> resolver)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        public virtual void Serialize(ref JsonWriter<byte> writer, T value, IJsonFormatterResolver<byte> resolver)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        public virtual void Serialize(ref JsonWriter<char> writer, T value, IJsonFormatterResolver<char> resolver)
        {
            throw ThrowHelper.GetNotSupportedException();
        }
    }
}
