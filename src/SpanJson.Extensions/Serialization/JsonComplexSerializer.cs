using System;
using System.Collections.Concurrent;
using CuteAnt.Pool;
using Newtonsoft.Json;
using SpanJson.Resolvers;
using NJsonSerializer = Newtonsoft.Json.JsonSerializer;
using NJsonSerializerSettings = Newtonsoft.Json.JsonSerializerSettings;

namespace SpanJson.Serialization
{
    public sealed class JsonComplexSerializer : JsonComplexSerializer<ExcludeNullsOriginalCaseResolver<char>, ExcludeNullsOriginalCaseResolver<byte>>
    {
        public static readonly JsonComplexSerializer Instance = new JsonComplexSerializer();
    }

    public partial class JsonComplexSerializer<TResolver, TUtf8Resolver> : IJsonSerializer
        where TResolver : IJsonFormatterResolver<char, TResolver>, new()
        where TUtf8Resolver : IJsonFormatterResolver<byte, TUtf8Resolver>, new()
    {
        static readonly ConcurrentDictionary<Type, JsonSerializer.NonGeneric.Inner<char, TResolver>.Invoker> Utf16Invokers =
            JsonSerializer.NonGeneric.Inner<char, TResolver>.Invokers;
        static readonly Func<Type, JsonSerializer.NonGeneric.Inner<char, TResolver>.Invoker> Utf16InvokerFactory =
            JsonSerializer.NonGeneric.Inner<char, TResolver>.InvokerFactory;

        static readonly ConcurrentDictionary<Type, JsonSerializer.NonGeneric.Inner<byte, TUtf8Resolver>.Invoker> Utf8Invokers =
            JsonSerializer.NonGeneric.Inner<byte, TUtf8Resolver>.Invokers;
        static readonly Func<Type, JsonSerializer.NonGeneric.Inner<byte, TUtf8Resolver>.Invoker> Utf8InvokerFactory =
            JsonSerializer.NonGeneric.Inner<byte, TUtf8Resolver>.InvokerFactory;

        private readonly NJsonSerializerSettings _defaultSerializerSettings;
        private readonly NJsonSerializerSettings _defaultDeserializerSettings;
        private readonly ObjectPool<NJsonSerializer> _outputJsonSerializerPool;
        private readonly ObjectPool<NJsonSerializer> _inputJsonSerializerPool;

        public JsonComplexSerializer()
        {
            _defaultSerializerSettings = new NJsonSerializerSettings
            {
                //PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,

                Formatting = Formatting.None,

                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,

                ObjectCreationHandling = ObjectCreationHandling.Replace,
                FloatParseHandling = FloatParseHandling.Double,

                SerializationBinder = JsonSerializationBinder.Instance
            };
            _defaultSerializerSettings.Converters.Add(Newtonsoft.Json.Converters.IPAddressConverter.Instance);
            _defaultSerializerSettings.Converters.Add(Newtonsoft.Json.Converters.IPEndPointConverter.Instance);
            _defaultSerializerSettings.Converters.Add(Newtonsoft.Json.Converters.CombGuidConverter.Instance);

            _outputJsonSerializerPool = JsonConvertX.GetJsonSerializerPool(_defaultSerializerSettings);

            _defaultDeserializerSettings = new NJsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,

                DefaultValueHandling = DefaultValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,

                ObjectCreationHandling = ObjectCreationHandling.Replace,
                FloatParseHandling = FloatParseHandling.Double,

                SerializationBinder = JsonSerializationBinder.Instance
            };
            _defaultDeserializerSettings.Converters.Add(Newtonsoft.Json.Converters.IPAddressConverter.Instance);
            _defaultDeserializerSettings.Converters.Add(Newtonsoft.Json.Converters.IPEndPointConverter.Instance);
            _defaultDeserializerSettings.Converters.Add(Newtonsoft.Json.Converters.CombGuidConverter.Instance);

            _inputJsonSerializerPool = JsonConvertX.GetJsonSerializerPool(_defaultDeserializerSettings);
        }

        public JsonComplexSerializer(NJsonSerializerSettings serializerSettings, NJsonSerializerSettings deserializerSettings)
        {
            if (null == serializerSettings) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.serializerSettings); }
            if (null == deserializerSettings) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.deserializerSettings); }
            if (ReferenceEquals(serializerSettings, serializerSettings)) { ThrowHelper.ThrowArgumentException_SerializerSettings_same_instance(); }

            _defaultSerializerSettings = serializerSettings;
            _outputJsonSerializerPool = JsonConvertX.GetJsonSerializerPool(_defaultSerializerSettings);

            _defaultDeserializerSettings = deserializerSettings;
            _inputJsonSerializerPool = JsonConvertX.GetJsonSerializerPool(_defaultDeserializerSettings);
        }

        /// <summary>Gets or sets the default <see cref="NJsonSerializerSettings"/> used to configure the <see cref="NJsonSerializer"/>.</summary>
        public NJsonSerializerSettings DefaultSerializerSettings => _defaultSerializerSettings;

        /// <summary>Gets or sets the default <see cref="NJsonSerializerSettings"/> used to configure the <see cref="NJsonSerializer"/>.</summary>
        public NJsonSerializerSettings DefaultDeserializerSettings => _defaultDeserializerSettings;
    }
}
