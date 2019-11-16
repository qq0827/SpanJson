using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using CuteAnt.Pool;
using SpanJson.Serialization;
using NJsonSerializer = Newtonsoft.Json.JsonSerializer;
using NJsonSerializerSettings = Newtonsoft.Json.JsonSerializerSettings;

namespace SpanJson.Linq
{
    partial class JToken
    {
        private static readonly NJsonSerializerSettings _defaultSerializerSettings;
        private static ObjectPool<NJsonSerializer> _defaultSerializerPool;

        private static readonly NJsonSerializerSettings _polymorphicSerializerSettings;
        private static ObjectPool<NJsonSerializer> _polymorphicSerializerPool;

        private static readonly NJsonSerializerSettings _polymorphicDeserializerSettings;
        private static ObjectPool<NJsonSerializer> _polymorphicDeserializerPool;

        public static NJsonSerializerSettings DefaultSerializerSettings => _defaultSerializerSettings;
        public static ObjectPool<NJsonSerializer> DefaultSerializerPool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _defaultSerializerPool ?? EnsureSerializerPoolCreated();
        }

        public static NJsonSerializerSettings PolymorphicSerializerSettings => _polymorphicSerializerSettings;
        public static ObjectPool<NJsonSerializer> PolymorphicSerializerPool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _polymorphicSerializerPool ?? EnsurePolymorphicSerializerPoolCreated();
        }

        [Obsolete("=> PolymorphicDeserializerSettings")]
        public static NJsonSerializerSettings DefaultDeserializerSettings => _polymorphicDeserializerSettings;
        [Obsolete("=> PolymorphicDeserializerPool")]
        public static ObjectPool<NJsonSerializer> DefaultDeserializerPool => PolymorphicDeserializerPool;

        public static NJsonSerializerSettings PolymorphicDeserializerSettings => _polymorphicDeserializerSettings;
        public static ObjectPool<NJsonSerializer> PolymorphicDeserializerPool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _polymorphicDeserializerPool ?? EnsurePolymorphicDeserializerPoolCreated();
        }

        static JToken()
        {
            _defaultSerializerSettings = new NJsonSerializerSettings();
            _polymorphicSerializerSettings = new NJsonSerializerSettings
            {
                SerializationBinder = JsonSerializationBinder.Instance,
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto
            };
            var jonsConverters = new List<Newtonsoft.Json.JsonConverter>
            {
                new SpanJson.Converters.DynamicObjectConverter(),
                new SpanJson.Converters.DynamicUtf16ArrayConverter(),
                new SpanJson.Converters.DynamicUtf16NumberConverter(),
                new SpanJson.Converters.DynamicUtf16StringConverter(),
                new SpanJson.Converters.DynamicUtf8ArrayConverter(),
                new SpanJson.Converters.DynamicUtf8NumberConverter(),
                new SpanJson.Converters.DynamicUtf8StringConverter(),

                new SpanJson.Converters.JsonDocumentConverter(),
                new SpanJson.Converters.JsonElementConverter(),

                SpanJson.Converters.JTokenConverter.Instance,

                new SpanJson.Converters.CombGuidJTokenConverter(),
                Newtonsoft.Json.Converters.IPAddressConverter.Instance,
                Newtonsoft.Json.Converters.IPEndPointConverter.Instance,
            };
            _defaultSerializerSettings.Converters = jonsConverters;
            var converters = _polymorphicSerializerSettings.Converters;
            foreach (var item in jonsConverters)
            {
                converters.Add(item);
            }

            _polymorphicDeserializerSettings = new NJsonSerializerSettings
            {
                ConstructorHandling = Newtonsoft.Json.ConstructorHandling.AllowNonPublicDefaultConstructor,

                DateParseHandling = Newtonsoft.Json.DateParseHandling.None,

                SerializationBinder = JsonSerializationBinder.Instance,
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto
            };
            _polymorphicDeserializerSettings.Converters.Add(Newtonsoft.Json.Converters.IPAddressConverter.Instance);
            _polymorphicDeserializerSettings.Converters.Add(Newtonsoft.Json.Converters.IPEndPointConverter.Instance);
            _polymorphicDeserializerSettings.Converters.Add(Newtonsoft.Json.Converters.CombGuidConverter.Instance);
            _polymorphicDeserializerSettings.Converters.Add(SpanJson.Converters.JTokenConverter.Instance);
        }

        public static NJsonSerializerSettings CreateSerializerSettings(Action<NJsonSerializerSettings> configSettings)
        {
            if (configSettings is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.configSettings); }

            var serializerSettings = new NJsonSerializerSettings();
            var converters = serializerSettings.Converters;
            foreach (var item in _defaultSerializerSettings.Converters)
            {
                converters.Add(item);
            }
            configSettings.Invoke(serializerSettings);
            return serializerSettings;
        }

        public static NJsonSerializerSettings CreateDeserializerSettings(Action<NJsonSerializerSettings> configSettings)
        {
            if (configSettings is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.configSettings); }

            var serializerSettings = new NJsonSerializerSettings();
            var converters = serializerSettings.Converters;
            foreach (var item in _polymorphicDeserializerSettings.Converters)
            {
                converters.Add(item);
            }
            configSettings.Invoke(serializerSettings);
            return serializerSettings;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ObjectPool<NJsonSerializer> EnsureSerializerPoolCreated()
        {
            Interlocked.CompareExchange(ref _defaultSerializerPool, JsonConvertX.GetJsonSerializerPool(_defaultSerializerSettings), null);
            return _defaultSerializerPool;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ObjectPool<NJsonSerializer> EnsurePolymorphicSerializerPoolCreated()
        {
            Interlocked.CompareExchange(ref _polymorphicSerializerPool, JsonConvertX.GetJsonSerializerPool(_polymorphicSerializerSettings), null);
            return _polymorphicSerializerPool;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ObjectPool<NJsonSerializer> EnsurePolymorphicDeserializerPoolCreated()
        {
            Interlocked.CompareExchange(ref _polymorphicDeserializerPool, JsonConvertX.GetJsonSerializerPool(_polymorphicDeserializerSettings), null);
            return _polymorphicDeserializerPool;
        }
    }
}