namespace SpanJson
{
    public interface IJsonFormatter
    {
    }

    public interface ICustomJsonFormatter : IJsonFormatter
    {
        object Arguments { get; set; }
    }

    public interface ICustomJsonFormatter<T> : IJsonFormatter<T, byte>, IJsonFormatter<T, char>, ICustomJsonFormatter
    {
    }

    public interface IJsonFormatter<T, TSymbol> : IJsonFormatter where TSymbol : struct
    {
        void Serialize(ref JsonWriter<TSymbol> writer, T value, IJsonFormatterResolver<TSymbol> resolver);
        T Deserialize(ref JsonReader<TSymbol> reader, IJsonFormatterResolver<TSymbol> resolver);
    }
}