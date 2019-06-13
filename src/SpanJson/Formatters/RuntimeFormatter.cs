using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SpanJson.Resolvers;

namespace SpanJson.Formatters
{
    public sealed class RuntimeFormatter<TSymbol, TResolver> : BaseFormatter, IJsonFormatter<object, TSymbol>
        where TResolver : IJsonFormatterResolver<TSymbol, TResolver>, new()
        where TSymbol : struct
    {
        public static readonly RuntimeFormatter<TSymbol, TResolver> Default = new RuntimeFormatter<TSymbol, TResolver>();

        private static readonly ConcurrentDictionary<Type, SerializeDelegate> RuntimeSerializerDictionary =
            new ConcurrentDictionary<Type, SerializeDelegate>();

        public object Deserialize(ref JsonReader<TSymbol> reader, IJsonFormatterResolver<TSymbol> resolver)
        {
            return reader.ReadDynamic();
        }

        public void Serialize(ref JsonWriter<TSymbol> writer, object value, IJsonFormatterResolver<TSymbol> resolver)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }


            // ReSharper disable ConvertClosureToMethodGroup
            var serializer = RuntimeSerializerDictionary.GetOrAdd(value.GetType(), x => BuildSerializeDelegate(x));
            serializer(ref writer, value, resolver);
            // ReSharper restore ConvertClosureToMethodGroup
        }

        private static SerializeDelegate BuildSerializeDelegate(Type type)
        {
            var writerParameter = Expression.Parameter(typeof(JsonWriter<TSymbol>).MakeByRefType(), "writer");
            var valueParameter = Expression.Parameter(typeof(object), "value");
            var resolverParameter = Expression.Parameter(typeof(IJsonFormatterResolver<TSymbol>), "resolver");
            if (type == typeof(object)) // if it's an object we can't do anything about so we write an empty object
            {
                return (ref JsonWriter<TSymbol> writer, object value, IJsonFormatterResolver<TSymbol> resolver) =>
                {
                    writer.WriteBeginObject();
                    writer.WriteEndObject();
                };
            }

            var formatterType = StandardResolvers.GetResolver<TSymbol, TResolver>().GetFormatter(type).GetType();
            var fieldInfo = formatterType.GetField("Default", BindingFlags.Static | BindingFlags.Public);
            var serializeMethodInfo = formatterType.GetMethods()
                .Where(_ => MatchSerializeMethod(_)).Single();
            var lambda = Expression.Lambda<SerializeDelegate>(
                Expression.Call(Expression.Field(null, fieldInfo), serializeMethodInfo, writerParameter,
                    Expression.Convert(valueParameter, type), resolverParameter),
                writerParameter, valueParameter, resolverParameter);
            return lambda.Compile();
        }

        private static bool MatchSerializeMethod(MethodInfo mi)
        {
            if (!string.Equals("Serialize", mi.Name, StringComparison.Ordinal)) { return false; }

            var parameters = mi.GetParameters();
            if (parameters.Length != 3) { return false; }
            var firstParamType = parameters[0].ParameterType;
            if (!firstParamType.IsByRef) { return false; }
            if (firstParamType.GetElementType() != typeof(JsonWriter<TSymbol>)) { return false; }
            if (parameters[2].ParameterType != typeof(IJsonFormatterResolver<TSymbol>)) { return false; }
            return true;
        }

        private delegate void SerializeDelegate(ref JsonWriter<TSymbol> writer, object value, IJsonFormatterResolver<TSymbol> resolver);
    }
}