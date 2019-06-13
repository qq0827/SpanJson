namespace SpanJson.Formatters
{
    /// <summary>
    /// Used for types which are not built-in
    /// </summary>
    public sealed class ComplexStructFormatter<T, TSymbol, TResolver> : ComplexFormatter, IJsonFormatter<T, TSymbol>
        where T : struct where TResolver : IJsonFormatterResolver<TSymbol, TResolver>, new() where TSymbol : struct
    {
        public static readonly ComplexStructFormatter<T, TSymbol, TResolver> Default = new ComplexStructFormatter<T, TSymbol, TResolver>();
        private static readonly DeserializeDelegate<T, TSymbol> Deserializer = BuildDeserializeDelegate<T, TSymbol, TResolver>();
        private static readonly SerializeDelegate<T, TSymbol> Serializer = BuildSerializeDelegate<T, TSymbol, TResolver>();

        public T Deserialize(ref JsonReader<TSymbol> reader, IJsonFormatterResolver<TSymbol> resolver)
        {
            return Deserializer(ref reader, resolver);
        }

        public void Serialize(ref JsonWriter<TSymbol> writer, T value, IJsonFormatterResolver<TSymbol> resolver)
        {
            Serializer(ref writer, value, resolver);
        }
    }
}