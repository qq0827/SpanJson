using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using CuteAnt.Reflection;
using SpanJson.Helpers;
using SpanJson.Resolvers;

namespace SpanJson.Formatters
{
    /// <summary>
    /// Used for types which are not built-in
    /// </summary>
    public sealed class DictionaryFormatter<TDictionary, TWritableDictionary, TKey, TValue, TSymbol, TResolver> : BaseFormatter,
        IJsonFormatter<TDictionary, TSymbol>
        where TResolver : IJsonFormatterResolver<TSymbol, TResolver>, new() where TSymbol : struct where TDictionary : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private static readonly Func<TWritableDictionary> CreateFunctor =
            StandardResolvers.GetCreateFunctor<TSymbol, TResolver, TWritableDictionary>();

        private static readonly Func<TDictionary, int> CountFunctor = BuildCountFunctor();
        private static readonly WriteKeyDelegate WriteKeyFunctor = BuildKeyToNameDelegate();
        private static readonly ReadKeyDelegate ReadKeyFunctor = BuildNameToKeyDelegate();

        private static readonly AssignKvpDelegate AssignKvpFunctor = BuildAssignKvpFunctor();

        public static readonly DictionaryFormatter<TDictionary, TWritableDictionary, TKey, TValue, TSymbol, TResolver> Default =
            new DictionaryFormatter<TDictionary, TWritableDictionary, TKey, TValue, TSymbol, TResolver>();

        private static readonly IJsonFormatter<TKey, TSymbol> KeyFormatter = GetKeyFormatter();

        private static readonly IJsonFormatter<TValue, TSymbol> ValueFormatter = StandardResolvers.GetFormatter<TSymbol, TResolver, TValue>();

        private static readonly Func<TWritableDictionary, TDictionary> Converter =
            StandardResolvers.GetEnumerableConvertFunctor<TSymbol, TResolver, TWritableDictionary, TDictionary>();

        private static readonly bool IsRecursionCandidate = RecursionCandidate<TValue>.IsRecursionCandidate;


        private static IJsonFormatter<TKey, TSymbol> GetKeyFormatter()
        {
            if (typeof(TKey).IsEnum)
            {
                return (IJsonFormatter<TKey, TSymbol>) ResolverBase.GetDefaultOrCreate(
                    typeof(EnumStringFormatter<,,>).GetCachedGenericType(typeof(TKey), typeof(TSymbol), typeof(TResolver)));
            }

            return StandardResolvers.GetFormatter<TSymbol, TResolver, TKey>();
        }


        private static Func<TDictionary, int> BuildCountFunctor()
        {
            var paramExpression = Expression.Parameter(typeof(TDictionary), "input");
            var propertyInfo = paramExpression.Type.GetPublicProperties().First(x => x.Name == nameof(ICollection.Count));
            var propertyExpression = Expression.Property(paramExpression, propertyInfo);
            return Expression.Lambda<Func<TDictionary, int>>(propertyExpression, paramExpression).Compile();
        }

        private static AssignKvpDelegate BuildAssignKvpFunctor()
        {
            var dictionaryParameterExpression = Expression.Parameter(typeof(TWritableDictionary), "dictionary");
            var keyParameterExpression = Expression.Parameter(typeof(TKey), "key");
            var valueParameterExpression = Expression.Parameter(typeof(TValue), "value");

            var indexerProperty = dictionaryParameterExpression.Type.GetProperty("Item", valueParameterExpression.Type, new[] {keyParameterExpression.Type});

            var lambda = Expression.Lambda<AssignKvpDelegate>(
                Expression.Assign(Expression.Property(dictionaryParameterExpression, indexerProperty, keyParameterExpression), valueParameterExpression),
                dictionaryParameterExpression, keyParameterExpression, valueParameterExpression);
            return lambda.Compile();
        }

        private static ReadKeyDelegate BuildNameToKeyDelegate()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                if (typeof(TKey).IsInteger()) // the integer values are quoted
                {
                    static TKey ReadIntegerKey(ref JsonReader<TSymbol> reader, IJsonFormatterResolver<TSymbol> resolver)
                    {
                        var separator = JsonUtf16Constant.NameSeparator;
                        var doubleQuote = JsonUtf16Constant.DoubleQuote;
                        reader.ReadSymbolOrThrow(Unsafe.As<char, TSymbol>(ref doubleQuote));
                        var key = KeyFormatter.Deserialize(ref reader, resolver);
                        reader.ReadSymbolOrThrow(Unsafe.As<char, TSymbol>(ref doubleQuote));
                        reader.ReadSymbolOrThrow(Unsafe.As<char, TSymbol>(ref separator));
                        return key;
                    }
                    return ReadIntegerKey;
                }

                static TKey ReadStringKey(ref JsonReader<TSymbol> reader, IJsonFormatterResolver<TSymbol> resolver)
                {
                    var separator = JsonUtf16Constant.NameSeparator;
                    var key = KeyFormatter.Deserialize(ref reader, resolver);
                    reader.ReadSymbolOrThrow(Unsafe.As<char, TSymbol>(ref separator));
                    return key;
                }

                return ReadStringKey;
            }

            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                if (typeof(TKey).IsInteger()) // the integer values need to be quoted
                {
                    static TKey ReadIntegerKey(ref JsonReader<TSymbol> reader, IJsonFormatterResolver<TSymbol> resolver)
                    {
                        var separator = JsonUtf8Constant.NameSeparator;
                        var doubleQuote = JsonUtf8Constant.DoubleQuote;
                        reader.ReadSymbolOrThrow(Unsafe.As<byte, TSymbol>(ref doubleQuote));
                        var key = KeyFormatter.Deserialize(ref reader, resolver);
                        reader.ReadSymbolOrThrow(Unsafe.As<byte, TSymbol>(ref doubleQuote));
                        reader.ReadSymbolOrThrow(Unsafe.As<byte, TSymbol>(ref separator));
                        return key;
                    }
                    return ReadIntegerKey;
                }

                static TKey ReadStringKey(ref JsonReader<TSymbol> reader, IJsonFormatterResolver<TSymbol> resolver)
                {
                    var separator = JsonUtf8Constant.NameSeparator;
                    var key = KeyFormatter.Deserialize(ref reader, resolver);
                    reader.ReadSymbolOrThrow(Unsafe.As<byte, TSymbol>(ref separator));
                    return key;
                }

                return ReadStringKey;
            }
            throw new NotSupportedException();
        }

        private static WriteKeyDelegate BuildKeyToNameDelegate()
        {
            if (typeof(TKey).IsInteger()) // the integer values need to be quoted
            {
                static void WriteIntegerKey(ref JsonWriter<TSymbol> writer, TKey value, IJsonFormatterResolver<TSymbol> resolver)
                {
                    writer.WriteDoubleQuote();
                    KeyFormatter.Serialize(ref writer, value, resolver);
                    writer.WriteDoubleQuote();
                    writer.WriteNameSeparator();
                }

                return WriteIntegerKey;
            }

            static void WriteStringKey(ref JsonWriter<TSymbol> writer, TKey value, IJsonFormatterResolver<TSymbol> resolver)
            {
                KeyFormatter.Serialize(ref writer, value, resolver);
                writer.WriteNameSeparator();
            }

            return WriteStringKey;
        }

        public TDictionary Deserialize(ref JsonReader<TSymbol> reader, IJsonFormatterResolver<TSymbol> resolver)
        {
            if (reader.ReadIsNull())
            {
                return default;
            }

            reader.ReadBeginObjectOrThrow();
            var result = CreateFunctor(); // using new T() is 5-10 times slower
            var count = 0;
            while (!reader.TryReadIsEndObjectOrValueSeparator(ref count))
            {
                var key = ReadKeyFunctor(ref reader, resolver);
                var value = ValueFormatter.Deserialize(ref reader, resolver);
                AssignKvpFunctor(result, key, value); // No shared interface for IReadOnlyDictionary and IDictionary to set the value via indexer (to make sure that for duplicated keys, we use the last one)
            }

            return Converter(result);
        }

        public void Serialize(ref JsonWriter<TSymbol> writer, TDictionary value, IJsonFormatterResolver<TSymbol> resolver)
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

            var valueLength = CountFunctor(value); // IReadOnlyDictionary and IDictionary don't share the same interface with .Count, use expression trees to optimize it
            writer.WriteBeginObject();
            if (valueLength > 0)
            {
                var counter = 0;
                foreach (var kvp in value)
                {
                    WriteKeyFunctor(ref writer, kvp.Key, resolver);
                    SerializeRuntimeDecisionInternal<TValue, TSymbol, TResolver>(ref writer, kvp.Value, ValueFormatter, resolver);
                    if (counter++ < valueLength - 1)
                    {
                        writer.WriteValueSeparator();
                    }
                }
            }

            if (IsRecursionCandidate)
            {
                writer.DecrementDepth();
            }

            writer.WriteEndObject();
        }

        private delegate void WriteKeyDelegate(ref JsonWriter<TSymbol> writer, TKey input, IJsonFormatterResolver<TSymbol> resolver);

        private delegate TKey ReadKeyDelegate(ref JsonReader<TSymbol> reader, IJsonFormatterResolver<TSymbol> resolver);

        private delegate void AssignKvpDelegate(TWritableDictionary dictionary, TKey key, TValue value);
    }
}