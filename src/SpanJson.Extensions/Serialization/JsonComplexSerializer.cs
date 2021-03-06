﻿using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using CuteAnt.Pool;
using Newtonsoft.Json;
using SpanJson.Resolvers;
using NJsonSerializer = Newtonsoft.Json.JsonSerializer;
using NJsonSerializerSettings = Newtonsoft.Json.JsonSerializerSettings;

namespace SpanJson.Serialization
{
    public sealed class JsonComplexSerializer : JsonComplexSerializer<IncludeNullsOriginalCaseResolver<char>, IncludeNullsOriginalCaseResolver<byte>>
    {
        public static readonly JsonComplexSerializer Instance = new JsonComplexSerializer();
    }

    public partial class JsonComplexSerializer<TUtf16Resolver, TUtf8Resolver> : IJsonSerializer
        where TUtf16Resolver : IJsonFormatterResolver<char, TUtf16Resolver>, new()
        where TUtf8Resolver : IJsonFormatterResolver<byte, TUtf8Resolver>, new()
    {
        static readonly ConcurrentDictionary<Type, JsonSerializer.NonGeneric.Inner<char, TUtf16Resolver>.Invoker> Utf16Invokers =
            JsonSerializer.NonGeneric.Inner<char, TUtf16Resolver>.Invokers;
        static readonly Func<Type, JsonSerializer.NonGeneric.Inner<char, TUtf16Resolver>.Invoker> Utf16InvokerFactory =
            JsonSerializer.NonGeneric.Inner<char, TUtf16Resolver>.InvokerFactory;

        static readonly ConcurrentDictionary<Type, JsonSerializer.NonGeneric.Inner<byte, TUtf8Resolver>.Invoker> Utf8Invokers =
            JsonSerializer.NonGeneric.Inner<byte, TUtf8Resolver>.Invokers;
        static readonly Func<Type, JsonSerializer.NonGeneric.Inner<byte, TUtf8Resolver>.Invoker> Utf8InvokerFactory =
            JsonSerializer.NonGeneric.Inner<byte, TUtf8Resolver>.InvokerFactory;

        private readonly NJsonSerializerSettings _serializerSettings;
        private readonly NJsonSerializerSettings _deserializerSettings;
        private ObjectPool<NJsonSerializer> _serializerPool;
        private ObjectPool<NJsonSerializer> _deserializerPool;

        public JsonComplexSerializer()
        {
            _serializerSettings = new NJsonSerializerSettings
            {
                //NullValueHandling = NullValueHandling.Ignore,

                SerializationBinder = JsonSerializationBinder.Instance,
                TypeNameHandling = TypeNameHandling.Auto
            };
            _serializerSettings.Converters.Add(Newtonsoft.Json.Converters.IPAddressConverter.Instance);
            _serializerSettings.Converters.Add(Newtonsoft.Json.Converters.IPEndPointConverter.Instance);
            _serializerSettings.Converters.Add(Newtonsoft.Json.Converters.CombGuidConverter.Instance);
            _serializerSettings.Converters.Add(SpanJson.Converters.JTokenConverter.Instance);

            _deserializerSettings = new NJsonSerializerSettings
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,

                DateParseHandling = DateParseHandling.None,

                SerializationBinder = JsonSerializationBinder.Instance,
                TypeNameHandling = TypeNameHandling.Auto
            };
            _deserializerSettings.Converters.Add(Newtonsoft.Json.Converters.IPAddressConverter.Instance);
            _deserializerSettings.Converters.Add(Newtonsoft.Json.Converters.IPEndPointConverter.Instance);
            _deserializerSettings.Converters.Add(Newtonsoft.Json.Converters.CombGuidConverter.Instance);
            _deserializerSettings.Converters.Add(SpanJson.Converters.JTokenConverter.Instance);
        }

        public JsonComplexSerializer(NJsonSerializerSettings serializerSettings, NJsonSerializerSettings deserializerSettings)
        {
            if (serializerSettings is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.serializerSettings); }
            if (deserializerSettings is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.deserializerSettings); }
            if (ReferenceEquals(serializerSettings, serializerSettings)) { ThrowHelper.ThrowArgumentException_SerializerSettings_same_instance(); }

            _serializerSettings = serializerSettings;
            _deserializerSettings = deserializerSettings;
        }

        /// <summary>Gets or sets the default <see cref="NJsonSerializerSettings"/> used to configure the <see cref="NJsonSerializer"/>.</summary>
        public NJsonSerializerSettings SerializerSettings => _serializerSettings;
        public ObjectPool<NJsonSerializer> SerializerPool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _serializerPool ?? EnsureSerializerPoolCreated();
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private ObjectPool<NJsonSerializer> EnsureSerializerPoolCreated()
        {
            Interlocked.CompareExchange(ref _serializerPool, JsonConvertX.GetJsonSerializerPool(_serializerSettings), null);
            return _serializerPool;
        }

        /// <summary>Gets or sets the default <see cref="NJsonSerializerSettings"/> used to configure the <see cref="NJsonSerializer"/>.</summary>
        public NJsonSerializerSettings DeserializerSettings => _deserializerSettings;
        public ObjectPool<NJsonSerializer> DeserializerPool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _deserializerPool ?? EnsureDeserializerPoolCreated();
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private ObjectPool<NJsonSerializer> EnsureDeserializerPoolCreated()
        {
            Interlocked.CompareExchange(ref _deserializerPool, JsonConvertX.GetJsonSerializerPool(_deserializerSettings), null);
            return _deserializerPool;
        }

        public NJsonSerializerSettings CreateSerializerSettings(Action<NJsonSerializerSettings> configSettings)
        {
            if (configSettings is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.configSettings); }

            var serializerSettings = new NJsonSerializerSettings();
            var converters = serializerSettings.Converters;
            foreach (var item in _serializerSettings.Converters)
            {
                converters.Add(item);
            }
            configSettings.Invoke(serializerSettings);
            return serializerSettings;
        }

        public NJsonSerializerSettings CreateDeserializerSettings(Action<NJsonSerializerSettings> configSettings)
        {
            if (configSettings is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.configSettings); }

            var serializerSettings = new NJsonSerializerSettings();
            var converters = serializerSettings.Converters;
            foreach (var item in _deserializerSettings.Converters)
            {
                converters.Add(item);
            }
            configSettings.Invoke(serializerSettings);
            return serializerSettings;
        }
    }
}
