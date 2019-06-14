using System;
using System.Collections.Generic;
using SpanJson.Helpers;
using SpanJson.Resolvers;

namespace SpanJson.Formatters
{
    /// <summary>
    /// Used for types which are not built-in
    /// </summary>
    public sealed class EnumerableFormatter<TEnumerable, T, TSymbol, TResolver> : BaseFormatter, IJsonFormatter<TEnumerable, TSymbol>
        where TResolver : IJsonFormatterResolver<TSymbol, TResolver>, new() where TSymbol : struct where TEnumerable : IEnumerable<T>

    {
        private static readonly Func<T[], TEnumerable> Converter =
            StandardResolvers.GetEnumerableConvertFunctor<TSymbol, TResolver, T[], TEnumerable>();

        public static readonly EnumerableFormatter<TEnumerable, T, TSymbol, TResolver> Default = new EnumerableFormatter<TEnumerable, T, TSymbol, TResolver>();

        private static readonly IJsonFormatter<T, TSymbol> ElementFormatter =
            StandardResolvers.GetFormatter<TSymbol, TResolver, T>();
        private static readonly bool IsRecursionCandidate = RecursionCandidate<T>.IsRecursionCandidate;

        public TEnumerable Deserialize(ref JsonReader<TSymbol> reader, IJsonFormatterResolver<TSymbol> resolver)
        {
            if (reader.ReadIsNull())
            {
                return default;
            }

            var array = ArrayFormatter<T, TSymbol, TResolver>.Default.Deserialize(ref reader, resolver);
            return Converter(array);
        }

        public void Serialize(ref JsonWriter<TSymbol> writer, TEnumerable value, IJsonFormatterResolver<TSymbol> resolver)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            if (IsRecursionCandidate)
            {
                writer.IncrementDepth();
            }
            IEnumerator<T> enumerator = null;
            try
            {
                enumerator = value.GetEnumerator();
                writer.WriteBeginArray();
                if (enumerator.MoveNext())
                {
                    // first one, so we can write the separator prior to every following one
                    SerializeRuntimeDecisionInternal<T, TSymbol, TResolver>(ref writer, enumerator.Current, ElementFormatter, resolver);
                    // write all the other ones
                    while (enumerator.MoveNext())
                    {
                        writer.WriteValueSeparator();
                        SerializeRuntimeDecisionInternal<T, TSymbol, TResolver>(ref writer, enumerator.Current, ElementFormatter, resolver);
                    }
                }
                if (IsRecursionCandidate)
                {
                    writer.DecrementDepth();
                }
                writer.WriteEndArray();
            }
            finally
            {
                enumerator?.Dispose();
            }
        }
    }
}