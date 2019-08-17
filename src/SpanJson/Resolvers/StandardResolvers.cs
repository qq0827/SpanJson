namespace SpanJson.Resolvers
{
    using System;
    using System.ComponentModel;
    using System.Dynamic;
    using System.Runtime.CompilerServices;
    using System.Text.Encodings.Web;
    using System.Threading;
    using SpanJson.Formatters;
    using SpanJson.Internal;

    public static class StandardResolvers
    {
        public static IJsonFormatterResolver<TSymbol, TResolver> GetResolver<TSymbol, TResolver>()
            where TResolver : IJsonFormatterResolver<TSymbol, TResolver>, new()
            where TSymbol : struct
        {
            return StandardResolver<TSymbol, TResolver>.Default;
        }

        public static IJsonFormatter<T, TSymbol> GetFormatter<TSymbol, TResolver, T>()
            where TResolver : IJsonFormatterResolver<TSymbol, TResolver>, new()
            where TSymbol : struct
        {
            return StandardResolver<TSymbol, TResolver>.FormatterCache<T>.Instance;
        }

        public static JsonObjectDescription GetObjectDescription<TSymbol, TResolver, T>()
            where TResolver : IJsonFormatterResolver<TSymbol, TResolver>, new()
            where TSymbol : struct
        {
            return StandardResolver<TSymbol, TResolver>.ObjectDescriptionCache<T>.Instance;
        }

        public static Func<T> GetCreateFunctor<TSymbol, TResolver, T>()
            where TResolver : IJsonFormatterResolver<TSymbol, TResolver>, new()
            where TSymbol : struct
        {
            return StandardResolver<TSymbol, TResolver>.CreateFunctorCache<T>.Instance;
        }

        public static Func<T, TConverted> GetEnumerableConvertFunctor<TSymbol, TResolver, T, TConverted>()
            where TResolver : IJsonFormatterResolver<TSymbol, TResolver>, new()
            where TSymbol : struct
        {
            return StandardResolver<TSymbol, TResolver>.EnumerableConvertFunctorCache<T, TConverted>.Instance;
        }

        sealed class StandardResolver<TSymbol, TResolver> : IJsonFormatterResolver<TSymbol, TResolver>
            where TResolver : IJsonFormatterResolver<TSymbol, TResolver>, new()
            where TSymbol : struct
        {
            public static readonly StandardResolver<TSymbol, TResolver> Default;

            private static readonly TResolver s_innerResolver;

            private readonly JsonEscapeHandling _escapeHandling;
            private readonly JavaScriptEncoder _encoder;
            private readonly JsonNamingPolicy _dictionayKeyPolicy;
            private readonly JsonNamingPolicy _extensionDataPolicy;
            private readonly JsonNamingPolicy _jsonPropertyNamingPolicy;
            private DeserializeDynamicDelegate<TSymbol> _dynamicDeserializer;

            static StandardResolver()
            {
                s_innerResolver = Inner<TSymbol, TResolver>.Default;
                Default = new StandardResolver<TSymbol, TResolver>();
            }

            private StandardResolver()
            {
                _escapeHandling = s_innerResolver.EscapeHandling;
                _encoder = s_innerResolver.Encoder;
                _dictionayKeyPolicy = s_innerResolver.JsonOptions.DictionaryKeyPolicy;
                _extensionDataPolicy = s_innerResolver.JsonOptions.ExtensionDataNamingPolicy;
                _jsonPropertyNamingPolicy = s_innerResolver.JsonOptions.PropertyNamingPolicy;
                _dynamicDeserializer = s_innerResolver.DynamicDeserializer;
            }

            public SpanJsonOptions JsonOptions => s_innerResolver.JsonOptions;

            public JsonEscapeHandling EscapeHandling => _escapeHandling;
            public JavaScriptEncoder Encoder => _encoder;
            public DeserializeDynamicDelegate<TSymbol> DynamicDeserializer
            {
                get => _dynamicDeserializer;
                set
                {
                    if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }
                    _dynamicDeserializer = value;
                    s_innerResolver.DynamicDeserializer = value;
                }
            }

            [EditorBrowsable(EditorBrowsableState.Never)]
            public bool IsSupportedType(Type type)
            {
                return s_innerResolver.IsSupportedType(type);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Func<T> GetCreateFunctor<T>()
            {
                return CreateFunctorCache<T>.Instance;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public JsonObjectDescription GetDynamicObjectDescription(IDynamicMetaObjectProvider provider)
            {
                return s_innerResolver.GetDynamicObjectDescription(provider);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Func<T, TConverted> GetEnumerableConvertFunctor<T, TConverted>()
            {
                return EnumerableConvertFunctorCache<T, TConverted>.Instance;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IJsonFormatter<object, TSymbol> GetRuntimeFormatter() => RuntimeFormatter<TSymbol, TResolver>.Default;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IJsonFormatter<T, TSymbol> GetEnumStringFormatter<T>() where T : struct, Enum
            {
                return EnumStringFormatterCache<T>.Instance;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IJsonFormatter<T, TSymbol> GetEnumIntegerFormatter<T>() where T : struct, Enum
            {
                return EnumIntegerFormatter<T, TSymbol, TResolver>.Default;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IJsonFormatter<T, TSymbol> GetFormatter<T>()
            {
                return FormatterCache<T>.Instance;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IJsonFormatter GetFormatter(Type type)
            {
                return s_innerResolver.GetFormatter(type);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public IJsonFormatter GetFormatter(JsonMemberInfo info, Type overrideMemberType = null)
            {
                return s_innerResolver.GetFormatter(info, overrideMemberType);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public JsonObjectDescription GetObjectDescription<T>()
            {
                return ObjectDescriptionCache<T>.Instance;
            }

            /// <summary>Resolves the key of the dictionary.</summary>
            /// <param name="dictionaryKey">Key of the dictionary.</param>
            /// <returns>Resolved key of the dictionary.</returns>
            public string ResolveDictionaryKey(string dictionaryKey)
            {
                if (_dictionayKeyPolicy is object)
                {
                    return _dictionayKeyPolicy.ConvertName(dictionaryKey);
                }

                return ResolvePropertyName(dictionaryKey);
            }

            /// <summary>Resolves the name of the extension data.</summary>
            /// <param name="extensionDataName">Name of the extension data.</param>
            /// <returns>Resolved name of the extension data.</returns>
            public string ResolveExtensionDataName(string extensionDataName)
            {
                if (_extensionDataPolicy is object)
                {
                    return _extensionDataPolicy.ConvertName(extensionDataName);
                }

                return extensionDataName;
            }

            /// <summary>Resolves the name of the property.</summary>
            /// <param name="propertyName">Name of the property.</param>
            /// <returns>Resolved name of the property.</returns>
            public string ResolvePropertyName(string propertyName)
            {
                if (_jsonPropertyNamingPolicy is object)
                {
                    return _jsonPropertyNamingPolicy.ConvertName(propertyName);
                }

                return propertyName;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public JsonEncodedText GetEncodedDictionaryKey(string dictionaryKey)
            {
                //return JsonEncodedText.Encode(ResolveDictionaryKey(dictionaryKey), JsonEscapeHandling.EscapeNonAscii);
                return EscapingHelper.GetEncodedText(ResolveDictionaryKey(dictionaryKey), _escapeHandling);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public JsonEncodedText GetEncodedExtensionDataName(string extensionDataName)
            {
                return EscapingHelper.GetEncodedText(ResolveExtensionDataName(extensionDataName), _escapeHandling);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public JsonEncodedText GetEncodedPropertyName(string propertyName)
            {
                return EscapingHelper.GetEncodedText(ResolvePropertyName(propertyName), _escapeHandling);
            }

            internal static class FormatterCache<T>
            {
                private static IJsonFormatter<T, TSymbol> s_formatter;

                public static IJsonFormatter<T, TSymbol> Instance
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

            internal static class EnumStringFormatterCache<T> where T : struct, Enum
            {
                private static IJsonFormatter<T, TSymbol> s_formatter;

                public static IJsonFormatter<T, TSymbol> Instance
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get { return s_formatter ?? EnsureFormatterCreated(); }
                }

                [MethodImpl(MethodImplOptions.NoInlining)]
                private static IJsonFormatter<T, TSymbol> EnsureFormatterCreated()
                {
                    Interlocked.Exchange(ref s_formatter, s_innerResolver.GetEnumStringFormatter<T>());
                    return s_formatter;
                }
            }

            internal static class ObjectDescriptionCache<T>
            {
                private static JsonObjectDescription s_objectDescription;

                public static JsonObjectDescription Instance
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get { return s_objectDescription ?? EnsureObjectDescriptionCreated(); }
                }

                [MethodImpl(MethodImplOptions.NoInlining)]
                private static JsonObjectDescription EnsureObjectDescriptionCreated()
                {
                    Interlocked.Exchange(ref s_objectDescription, s_innerResolver.GetObjectDescription<T>());
                    return s_objectDescription;
                }
            }

            internal static class CreateFunctorCache<T>
            {
                private static Func<T> s_createFunctor;

                public static Func<T> Instance
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get { return s_createFunctor ?? EnsureCreateFunctorCreated(); }
                }

                [MethodImpl(MethodImplOptions.NoInlining)]
                private static Func<T> EnsureCreateFunctorCreated()
                {
                    Interlocked.Exchange(ref s_createFunctor, s_innerResolver.GetCreateFunctor<T>());
                    return s_createFunctor;
                }
            }

            internal static class EnumerableConvertFunctorCache<T, TConverted>
            {
                private static Func<T, TConverted> s_convertFunctor;

                public static Func<T, TConverted> Instance
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get { return s_convertFunctor ?? EnsureEnumerableConvertFunctorCreated(); }
                }

                [MethodImpl(MethodImplOptions.NoInlining)]
                private static Func<T, TConverted> EnsureEnumerableConvertFunctorCreated()
                {
                    Interlocked.Exchange(ref s_convertFunctor, s_innerResolver.GetEnumerableConvertFunctor<T, TConverted>());
                    return s_convertFunctor;
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