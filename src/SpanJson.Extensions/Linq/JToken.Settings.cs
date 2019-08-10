using System;
using System.Collections.Generic;
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

        private static readonly NJsonSerializerSettings _defaultDeserializerSettings;
        private static ObjectPool<NJsonSerializer> _defaultDeserializerPool;

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

        public static NJsonSerializerSettings DefaultDeserializerSettings => _defaultDeserializerSettings;
        public static ObjectPool<NJsonSerializer> DefaultDeserializerPool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _defaultDeserializerPool ?? EnsureDeserializerPoolCreated();
        }

        static JToken()
        {
            _defaultSerializerSettings = new NJsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.None,

                DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
                MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore,
            };
            _polymorphicSerializerSettings = new NJsonSerializerSettings
            {
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = Newtonsoft.Json.TypeNameAssemblyFormatHandling.Simple,
                ConstructorHandling = Newtonsoft.Json.ConstructorHandling.AllowNonPublicDefaultConstructor,

                Formatting = Newtonsoft.Json.Formatting.None,

                DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
                MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore,

                SerializationBinder = JsonSerializationBinder.Instance
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

                new SpanJson.Converters.JTokenConverter(),

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

            _defaultDeserializerSettings = new NJsonSerializerSettings
            {
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = Newtonsoft.Json.TypeNameAssemblyFormatHandling.Simple,
                ConstructorHandling = Newtonsoft.Json.ConstructorHandling.AllowNonPublicDefaultConstructor,

                DateParseHandling = Newtonsoft.Json.DateParseHandling.None,
                DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore,
                MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,

                ObjectCreationHandling = Newtonsoft.Json.ObjectCreationHandling.Replace,
                FloatParseHandling = Newtonsoft.Json.FloatParseHandling.Double,

                SerializationBinder = JsonSerializationBinder.Instance
            };
            _defaultDeserializerSettings.Converters.Add(Newtonsoft.Json.Converters.IPAddressConverter.Instance);
            _defaultDeserializerSettings.Converters.Add(Newtonsoft.Json.Converters.IPEndPointConverter.Instance);
            _defaultDeserializerSettings.Converters.Add(Newtonsoft.Json.Converters.CombGuidConverter.Instance);
        }

        public static NJsonSerializerSettings CreateSerializerSettings(Action<NJsonSerializerSettings> configSettings)
        {
            if (null == configSettings) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.configSettings); }

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
            if (null == configSettings) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.configSettings); }

            var serializerSettings = new NJsonSerializerSettings();
            var converters = serializerSettings.Converters;
            foreach (var item in _defaultDeserializerSettings.Converters)
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
        private static ObjectPool<NJsonSerializer> EnsureDeserializerPoolCreated()
        {
            Interlocked.CompareExchange(ref _defaultDeserializerPool, JsonConvertX.GetJsonSerializerPool(_defaultDeserializerSettings), null);
            return _defaultDeserializerPool;
        }
    }
}