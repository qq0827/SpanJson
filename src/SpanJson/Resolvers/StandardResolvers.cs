using System;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SpanJson.Resolvers
{
    public static class StandardResolvers
    {
        public static IJsonFormatterResolver<TSymbol, TResolver> GetResolver<TSymbol, TResolver>()
            where TResolver : IJsonFormatterResolver<TSymbol, TResolver>, new()
            where TSymbol : struct
        {
            return StandardResolver<TSymbol, TResolver>.Default;
        }

        sealed class StandardResolver<TSymbol, TResolver> : IJsonFormatterResolver<TSymbol, TResolver>
            where TResolver : IJsonFormatterResolver<TSymbol, TResolver>, new()
            where TSymbol : struct
        {
            public static readonly StandardResolver<TSymbol, TResolver> Default;

            private static readonly TResolver s_innerResolver;

            static StandardResolver()
            {
                s_innerResolver = Inner<TSymbol, TResolver>.Default;
                Default = new StandardResolver<TSymbol, TResolver>();
            }

            private StandardResolver() { }

            public SpanJsonOptions JsonOptions => s_innerResolver.JsonOptions;

            public Func<T> GetCreateFunctor<T>()
            {
                return s_innerResolver.GetCreateFunctor<T>();
            }

            public JsonObjectDescription GetDynamicObjectDescription(IDynamicMetaObjectProvider provider)
            {
                return s_innerResolver.GetDynamicObjectDescription(provider);
            }

            public Func<T, TConverted> GetEnumerableConvertFunctor<T, TConverted>()
            {
                return s_innerResolver.GetEnumerableConvertFunctor<T, TConverted>();
            }

            public IJsonFormatter<T, TSymbol> GetFormatter<T>()
            {
                return FormatterCache<T>.Formatter;
            }

            public IJsonFormatter GetFormatter(Type type)
            {
                return s_innerResolver.GetFormatter(type);
            }

            public IJsonFormatter GetFormatter(JsonMemberInfo info, Type overrideMemberType = null)
            {
                return s_innerResolver.GetFormatter(info, overrideMemberType);
            }

            public JsonObjectDescription GetObjectDescription<T>()
            {
                return s_innerResolver.GetObjectDescription<T>();
            }

            static class FormatterCache<T>
            {
                private static IJsonFormatter<T, TSymbol> s_formatter;

                public static IJsonFormatter<T, TSymbol> Formatter
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get { return s_formatter ?? EnsureFormatterCreated(); }
                }

                [MethodImpl(MethodImplOptions.NoInlining)]
                private static IJsonFormatter<T, TSymbol> EnsureFormatterCreated()
                {
                    Interlocked.Exchange(ref s_formatter, s_innerResolver.GetFormatter<T>());
                    return s_formatter;
                }
            }
        }

        private static class Inner<TSymbol, TResolver>
            where TResolver : IJsonFormatterResolver<TSymbol, TResolver>, new()
            where TSymbol : struct
        {
            public static readonly TResolver Default;

            static Inner() { Default = CreateResolver(); }

            private static TResolver CreateResolver()
            {
                var result = new TResolver();

                return result;
            }
        }
    }
}