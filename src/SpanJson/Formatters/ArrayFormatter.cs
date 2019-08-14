using System.Buffers;
using SpanJson.Helpers;
using SpanJson.Internal;
using SpanJson.Resolvers;

namespace SpanJson.Formatters
{
    /// <summary>
    /// Used for types which are not built-in
    /// </summary>
    public sealed class ArrayFormatter<T, TSymbol, TResolver> : BaseFormatter, IJsonFormatter<T[], TSymbol>
        where TResolver : IJsonFormatterResolver<TSymbol, TResolver>, new() where TSymbol : struct
    {
        public static readonly ArrayFormatter<T, TSymbol, TResolver> Default = new ArrayFormatter<T, TSymbol, TResolver>();

        private static readonly IJsonFormatter<T, TSymbol> ElementFormatter =
            StandardResolvers.GetFormatter<TSymbol, TResolver, T>();

        private static readonly bool IsRecursionCandidate = RecursionCandidate<T>.IsRecursionCandidate;

        public T[] Deserialize(ref JsonReader<TSymbol> reader, IJsonFormatterResolver<TSymbol> resolver)
        {
            T[] temp = null;
            T[] result;
            try
            {
                temp = ArrayPool<T>.Shared.Rent(4);
                if (reader.ReadIsNull())
                {
                    return null;
                }
                reader.ReadBeginArrayOrThrow();
                var count = 0;
                while (!reader.TryReadIsEndArrayOrValueSeparator(ref count)) // count is already preincremented, as it counts the separators
                {
                    if (count == temp.Length)
                    {
                        FormatterUtils.GrowArray(ref temp);
                    }

                    temp[count - 1] = ElementFormatter.Deserialize(ref reader, resolver);
                }

                result = 0u >= (uint)count ? JsonHelpers.Empty<T>() : FormatterUtils.CopyArray(temp, count);
            }
            finally
            {
                if (temp is object)
                {
                    ArrayPool<T>.Shared.Return(temp);
                }
            }

            return result;
        }

        public void Serialize(ref JsonWriter<TSymbol> writer, T[] value, IJsonFormatterResolver<TSymbol> resolver)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            if (IsRecursionCandidate)
            {
                writer.IncrementDepth();
            }
            var valueLength = value.Length;
            writer.WriteBeginArray();
            if (valueLength > 0)
            {
                SerializeRuntimeDecisionInternal<T, TSymbol, TResolver>(ref writer, value[0], ElementFormatter, resolver);
                for (var i = 1; i < valueLength; i++)
                {
                    writer.WriteValueSeparator();
                    SerializeRuntimeDecisionInternal<T, TSymbol, TResolver>(ref writer, value[i], ElementFormatter, resolver);
                }
            }
            if (IsRecursionCandidate)
            {
                writer.DecrementDepth();
            }
            writer.WriteEndArray();
        }
    }
}