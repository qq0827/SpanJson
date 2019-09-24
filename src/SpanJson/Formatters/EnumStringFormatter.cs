using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SpanJson.Formatters
{
    public sealed class EnumStringFormatter<T, TSymbol, TResolver> : BaseEnumStringFormatter<T, TSymbol>, IJsonFormatter<T, TSymbol> where T : struct, Enum
        where TResolver : IJsonFormatterResolver<TSymbol, TResolver>, new()
        where TSymbol : struct
    {
        private static readonly SerializeDelegate Serializer = BuildSerializeDelegate(s => "\"" + s + "\"");
        private static readonly DeserializeDelegate Deserializer = BuildDeserializeDelegate();
        public static readonly EnumStringFormatter<T, TSymbol, TResolver> Default = new EnumStringFormatter<T, TSymbol, TResolver>();

        public T Deserialize(ref JsonReader<TSymbol> reader, IJsonFormatterResolver<TSymbol> resolver)
        {
            return Deserializer(ref reader);
        }

        public void Serialize(ref JsonWriter<TSymbol> writer, T value, IJsonFormatterResolver<TSymbol> resolver)
        {
            Serializer(ref writer, value);
        }

        private static DeserializeDelegate BuildDeserializeDelegate()
        {
            var readerParameter = Expression.Parameter(typeof(JsonReader<TSymbol>).MakeByRefType(), "reader");
            MethodInfo nameSpanMethodInfo = null;
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                nameSpanMethodInfo = FindPublicInstanceMethod(readerParameter.Type, nameof(JsonReader<TSymbol>.ReadUtf8StringSpan));
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                nameSpanMethodInfo = FindPublicInstanceMethod(readerParameter.Type, nameof(JsonReader<TSymbol>.ReadUtf16StringSpan));
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
            Expression nameSpanExpression = Expression.Call(readerParameter, nameSpanMethodInfo);
            return BuildDeserializeDelegateExpressions<DeserializeDelegate, T>(readerParameter, nameSpanExpression);
        }

        private delegate T DeserializeDelegate(ref JsonReader<TSymbol> reader);
    }

    public sealed class EnumStringFormatter<T> : ICustomJsonFormatter<T> where T : struct, Enum
    {
        public static readonly EnumStringFormatter<T> Default = new EnumStringFormatter<T>();

        public object Arguments { get; set; }

        public T Deserialize(ref JsonReader<byte> reader, IJsonFormatterResolver<byte> resolver)
        {
            return resolver.GetEnumStringFormatter<T>().Deserialize(ref reader, resolver);
        }

        public T Deserialize(ref JsonReader<char> reader, IJsonFormatterResolver<char> resolver)
        {
            return resolver.GetEnumStringFormatter<T>().Deserialize(ref reader, resolver);
        }

        public void Serialize(ref JsonWriter<byte> writer, T value, IJsonFormatterResolver<byte> resolver)
        {
            resolver.GetEnumStringFormatter<T>().Serialize(ref writer, value, resolver);
        }

        public void Serialize(ref JsonWriter<char> writer, T value, IJsonFormatterResolver<char> resolver)
        {
            resolver.GetEnumStringFormatter<T>().Serialize(ref writer, value, resolver);
        }
    }
}