using System;
using System.Buffers;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using CuteAnt;
using CuteAnt.Collections;
using CuteAnt.Pool;
using CuteAnt.Reflection;
using NFormatting = Newtonsoft.Json.Formatting;
using NIJsonLineInfo = Newtonsoft.Json.IJsonLineInfo;
using NJsonReader = Newtonsoft.Json.JsonReader;
using NJsonConverter = Newtonsoft.Json.JsonConverter;
using NJsonSerializer = Newtonsoft.Json.JsonSerializer;
using NJsonSerializerSettings = Newtonsoft.Json.JsonSerializerSettings;
using NJsonSerializationException = Newtonsoft.Json.JsonSerializationException;

namespace SpanJson.Serialization
{
    partial class JsonConvertX
    {
        #region @@ Constructors @@

        private static readonly FieldInfo s_checkAdditionalContentField;
        private static readonly MemberGetter<NJsonSerializer> s_checkAdditionalContentGetter;
        private static readonly MemberSetter<NJsonSerializer> s_checkAdditionalContentSetter;

        private static readonly FieldInfo s_formattingField;
        private static readonly MemberGetter<NJsonSerializer> s_formattingGetter;
        private static readonly MemberSetter<NJsonSerializer> s_formattingSetter;

        private static readonly DictionaryCache<NJsonSerializerSettings, ObjectPool<NJsonSerializer>> s_jsonSerializerPoolCache;
        private static readonly ObjectPool<NJsonSerializer> s_defaultJsonSerializerPool;

        public static readonly Newtonsoft.Json.IArrayPool<char> GlobalCharacterArrayPool;

        public static readonly Newtonsoft.Json.Serialization.ISerializationBinder DefaultSerializationBinder;

        static JsonConvertX()
        {
            s_checkAdditionalContentField = typeof(NJsonSerializer).LookupTypeField("_checkAdditionalContent");
            s_checkAdditionalContentGetter = s_checkAdditionalContentField.GetValueGetter<NJsonSerializer>();
            s_checkAdditionalContentSetter = s_checkAdditionalContentField.GetValueSetter<NJsonSerializer>();

            s_formattingField = typeof(NJsonSerializer).LookupTypeField("_formatting");
            s_formattingGetter = s_formattingField.GetValueGetter<NJsonSerializer>();
            s_formattingSetter = s_formattingField.GetValueSetter<NJsonSerializer>();

            s_defaultJsonSerializerPool = _defaultObjectPoolProvider.Create(new JsonSerializerObjectPolicy(null));
            s_jsonSerializerPoolCache = new DictionaryCache<NJsonSerializerSettings, ObjectPool<NJsonSerializer>>(DictionaryCacheConstants.SIZE_SMALL);

            GlobalCharacterArrayPool = new JsonArrayPool<char>(ArrayPool<char>.Shared);

            DefaultSerializationBinder = JsonSerializationBinder.Instance;
        }

        #endregion

        #region -- NJsonSerializer.IsCheckAdditionalContentSetX --

        [MethodImpl(InlineMethod.Value)]
        public static bool IsCheckAdditionalContentSetX(this NJsonSerializer jsonSerializer)
        {
            if (jsonSerializer is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.jsonSerializer); }
            return s_checkAdditionalContentGetter(jsonSerializer) is object;
        }

        [MethodImpl(InlineMethod.Value)]
        public static bool? GetCheckAdditionalContent(this NJsonSerializer jsonSerializer)
        {
            if (jsonSerializer is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.jsonSerializer); }
            return (bool?)s_checkAdditionalContentGetter(jsonSerializer);
        }

        [MethodImpl(InlineMethod.Value)]
        public static void SetCheckAdditionalContent(this NJsonSerializer jsonSerializer, bool? checkAdditionalContent = null)
        {
            if (jsonSerializer is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.jsonSerializer); }
            s_checkAdditionalContentSetter(jsonSerializer, checkAdditionalContent);
        }

        #endregion

        #region -- NJsonSerializer.Formatting --

        [MethodImpl(InlineMethod.Value)]
        public static NFormatting? GetFormatting(this NJsonSerializer jsonSerializer)
        {
            if (jsonSerializer is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.jsonSerializer); }
            return (NFormatting?)s_formattingGetter(jsonSerializer);
        }

        [MethodImpl(InlineMethod.Value)]
        public static void SetFormatting(this NJsonSerializer jsonSerializer, NFormatting? formatting = null)
        {
            if (jsonSerializer is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.jsonSerializer); }
            s_formattingSetter(jsonSerializer, formatting);
        }

        #endregion

        #region == CreateJsonSerializationException ==

        internal static NJsonSerializationException CreateJsonSerializationException(NJsonReader reader, string message)
        {
            return CreateJsonSerializationException(reader, message, null);
        }

        internal static NJsonSerializationException CreateJsonSerializationException(NJsonReader reader, string message, Exception ex)
        {
            return CreateJsonSerializationException(reader as NIJsonLineInfo, reader.Path, message, ex);
        }

        internal static NJsonSerializationException CreateJsonSerializationException(NIJsonLineInfo lineInfo, string path, string message, Exception ex)
        {
            message = JsonPosition.FormatMessage(lineInfo, path, message);

            return new NJsonSerializationException(message, ex);
        }

        #endregion

        #region -- Allocate & Free NJsonSerializer --

        public static ObjectPool<NJsonSerializer> GetJsonSerializerPool(NJsonSerializerSettings jsonSettings)
        {
            return s_jsonSerializerPoolCache.GetItem(jsonSettings, s_getJsonSerializerPoolFunc);
        }

        public static NJsonSerializer AllocateSerializer(NJsonSerializerSettings jsonSettings)
        {
            if (jsonSettings is null) { return s_defaultJsonSerializerPool.Take(); }

            var pool = s_jsonSerializerPoolCache.GetItem(jsonSettings, s_getJsonSerializerPoolFunc);
            return pool.Take();
        }

        public static void FreeSerializer(NJsonSerializerSettings jsonSettings, NJsonSerializer jsonSerializer)
        {
            if (jsonSettings is null) { s_defaultJsonSerializerPool.Return(jsonSerializer); return; }

            if (s_jsonSerializerPoolCache.TryGetValue(jsonSettings, out ObjectPool<NJsonSerializer> pool))
            {
                pool.Return(jsonSerializer);
            }
        }

        private static readonly Func<NJsonSerializerSettings, ObjectPool<NJsonSerializer>> s_getJsonSerializerPoolFunc = GetJsonSerializerPoolInternal;
        private static readonly SynchronizedObjectPoolProvider _defaultObjectPoolProvider = SynchronizedObjectPoolProvider.Default;
        private static ObjectPool<NJsonSerializer> GetJsonSerializerPoolInternal(NJsonSerializerSettings jsonSettings)
        {
            return _defaultObjectPoolProvider.Create(new JsonSerializerObjectPolicy(jsonSettings));
        }

        #endregion

        #region -- Serialize to Byte-Array --

        private const int c_initialBufferSize = 1024 * 64;

        /// <summary>Serializes the specified object to a JSON byte array.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="initialBufferSize">The initial buffer size.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static byte[] SerializeToByteArray(object value, int initialBufferSize = c_initialBufferSize)
        {
            return SerializeToByteArray(value, null, (NJsonSerializerSettings)null, initialBufferSize);
        }

        /// <summary>Serializes the specified object to a JSON byte array using formatting.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <param name="initialBufferSize">The initial buffer size.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static byte[] SerializeToByteArray(object value, NFormatting formatting, int initialBufferSize = c_initialBufferSize)
        {
            return SerializeToByteArray(value, formatting, (NJsonSerializerSettings)null, initialBufferSize);
        }

        /// <summary>Serializes the specified object to a JSON byte array using a collection of <see cref="NJsonConverter"/>.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="converters">A collection of converters used while serializing.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static byte[] SerializeToByteArray(object value, params NJsonConverter[] converters)
        {
            var settings = (converters is object && converters.Length > 0)
                ? new NJsonSerializerSettings { Converters = converters }
                : null;
            return SerializeToByteArray(value, null, settings);
        }

        /// <summary>Serializes the specified object to a JSON byte array using formatting and a collection of <see cref="NJsonConverter"/>.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <param name="converters">A collection of converters used while serializing.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static byte[] SerializeToByteArray(object value, NFormatting formatting, params NJsonConverter[] converters)
        {
            var settings = (converters is object && converters.Length > 0)
                ? new NJsonSerializerSettings { Converters = converters }
                : null;
            return SerializeToByteArray(value, null, formatting, settings);
        }

        /// <summary>Serializes the specified object to a JSON byte array using <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to serialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <param name="initialBufferSize">The initial buffer size.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static byte[] SerializeToByteArray(object value, NJsonSerializerSettings settings, int initialBufferSize = c_initialBufferSize)
        {
            return SerializeToByteArray(value, null, settings, initialBufferSize);
        }

        /// <summary>Serializes the specified object to a JSON byte array using a type, formatting and <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to serialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="Newtonsoft.Json.TypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <param name="initialBufferSize">The initial buffer size.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static byte[] SerializeToByteArray(object value, Type type, NJsonSerializerSettings settings, int initialBufferSize = c_initialBufferSize)
        {
            var jsonSerializer = NJsonSerializer.CreateDefault(settings);

            return jsonSerializer.SerializeToByteArray(value, type, initialBufferSize);
        }

        /// <summary>Serializes the specified object to a JSON byte array using formatting and <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to serialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <param name="initialBufferSize">The initial buffer size.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static byte[] SerializeToByteArray(object value, NFormatting formatting, NJsonSerializerSettings settings, int initialBufferSize = c_initialBufferSize)
        {
            return SerializeToByteArray(value, null, formatting, settings, initialBufferSize);
        }

        /// <summary>Serializes the specified object to a JSON byte array using a type, formatting and <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to serialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="Newtonsoft.Json.TypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <param name="initialBufferSize">The initial buffer size.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static byte[] SerializeToByteArray(object value, Type type, NFormatting formatting, NJsonSerializerSettings settings, int initialBufferSize = c_initialBufferSize)
        {
            var jsonSerializer = NJsonSerializer.CreateDefault(settings);
            jsonSerializer.Formatting = formatting;

            return jsonSerializer.SerializeToByteArray(value, type, initialBufferSize);
        }

        #endregion

        #region -- Serialize to Memory-Pool --

        /// <summary>Serializes the specified object to a JSON byte array.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="initialBufferSize">The initial buffer size.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static ArraySegment<Byte> SerializeToMemoryPool(object value, int initialBufferSize = c_initialBufferSize)
        {
            return SerializeToMemoryPool(value, null, (NJsonSerializerSettings)null, initialBufferSize);
        }

        /// <summary>Serializes the specified object to a JSON byte array using formatting.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <param name="initialBufferSize">The initial buffer size.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static ArraySegment<Byte> SerializeToMemoryPool(object value, NFormatting formatting, int initialBufferSize = c_initialBufferSize)
        {
            return SerializeToMemoryPool(value, formatting, (NJsonSerializerSettings)null, initialBufferSize);
        }

        /// <summary>Serializes the specified object to a JSON byte array using a collection of <see cref="NJsonConverter"/>.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="converters">A collection of converters used while serializing.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static ArraySegment<Byte> SerializeToMemoryPool(object value, params NJsonConverter[] converters)
        {
            var settings = (converters is object && converters.Length > 0)
                ? new NJsonSerializerSettings { Converters = converters }
                : null;

            return SerializeToMemoryPool(value, null, settings);
        }

        /// <summary>Serializes the specified object to a JSON byte array using formatting and a collection of <see cref="NJsonConverter"/>.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <param name="converters">A collection of converters used while serializing.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static ArraySegment<Byte> SerializeToMemoryPool(object value, NFormatting formatting, params NJsonConverter[] converters)
        {
            var settings = (converters is object && converters.Length > 0)
                ? new NJsonSerializerSettings { Converters = converters }
                : null;
            return SerializeToMemoryPool(value, null, formatting, settings);
        }

        /// <summary>Serializes the specified object to a JSON byte array using <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to serialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <param name="initialBufferSize">The initial buffer size.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static ArraySegment<Byte> SerializeToMemoryPool(object value, NJsonSerializerSettings settings, int initialBufferSize = c_initialBufferSize)
        {
            return SerializeToMemoryPool(value, null, settings, initialBufferSize);
        }

        /// <summary>Serializes the specified object to a JSON byte array using a type, formatting and <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to serialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="Newtonsoft.Json.TypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <param name="initialBufferSize">The initial buffer size.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static ArraySegment<Byte> SerializeToMemoryPool(object value, Type type, NJsonSerializerSettings settings, int initialBufferSize = c_initialBufferSize)
        {
            var jsonSerializer = NJsonSerializer.CreateDefault(settings);

            return jsonSerializer.SerializeToMemoryPool(value, type, initialBufferSize);
        }

        /// <summary>Serializes the specified object to a JSON byte array using formatting and <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to serialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <param name="initialBufferSize">The initial buffer size.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static ArraySegment<Byte> SerializeToMemoryPool(object value, NFormatting formatting, NJsonSerializerSettings settings, int initialBufferSize = c_initialBufferSize)
        {
            return SerializeToMemoryPool(value, null, formatting, settings, initialBufferSize);
        }

        /// <summary>Serializes the specified object to a JSON byte array using a type, formatting and <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to serialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="Newtonsoft.Json.TypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <param name="initialBufferSize">The initial buffer size.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static ArraySegment<Byte> SerializeToMemoryPool(object value, Type type, NFormatting formatting, NJsonSerializerSettings settings, int initialBufferSize = c_initialBufferSize)
        {
            var jsonSerializer = NJsonSerializer.CreateDefault(settings);
            jsonSerializer.Formatting = formatting;

            return jsonSerializer.SerializeToMemoryPool(value, type, initialBufferSize);
        }

        #endregion

        #region -- Deserialize from Byte-Array --

        /// <summary>Deserializes the JSON to a .NET object.</summary>
        /// <param name="bytes">The byte array containing the JSON data to read.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromByteArray(byte[] bytes)
        {
            return DeserializeFromByteArray(bytes, null, (NJsonSerializerSettings)null);
        }

        /// <summary>Deserializes the JSON to a .NET object using <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="bytes">The byte array containing the JSON data to read.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to deserialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromByteArray(byte[] bytes, NJsonSerializerSettings settings)
        {
            return DeserializeFromByteArray(bytes, null, settings);
        }

        /// <summary>Deserializes the JSON to the specified .NET type.</summary>
        /// <param name="bytes">The byte array containing the JSON data to read.</param>
        /// <param name="type">The <see cref="Type"/> of object being deserialized.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromByteArray(byte[] bytes, Type type)
        {
            return DeserializeFromByteArray(bytes, type, (NJsonSerializerSettings)null);
        }

        /// <summary>Deserializes the JSON to the specified .NET type.</summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="bytes">The byte array containing the JSON data to read.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static T DeserializeFromByteArray<T>(byte[] bytes)
        {
            return DeserializeFromByteArray<T>(bytes, (NJsonSerializerSettings)null);
        }

        /// <summary>Deserializes the JSON to the specified .NET type using a collection of <see cref="NJsonConverter"/>.</summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="bytes">The byte array containing the JSON data to read.</param>
        /// <param name="converters">Converters to use while deserializing.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static T DeserializeFromByteArray<T>(byte[] bytes, params NJsonConverter[] converters)
        {
            return (T)DeserializeFromByteArray(bytes, typeof(T), converters);
        }

        /// <summary>Deserializes the JSON to the specified .NET type using <see cref="NJsonSerializerSettings"/>.</summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="bytes">The byte array containing the JSON data to read.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to deserialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static T DeserializeFromByteArray<T>(byte[] bytes, NJsonSerializerSettings settings)
        {
            return (T)DeserializeFromByteArray(bytes, typeof(T), settings);
        }

        /// <summary>Deserializes the JSON to the specified .NET type using a collection of <see cref="NJsonConverter"/>.</summary>
        /// <param name="bytes">The byte array containing the JSON data to read.</param>
        /// <param name="type">The type of the object to deserialize.</param>
        /// <param name="converters">Converters to use while deserializing.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromByteArray(byte[] bytes, Type type, params NJsonConverter[] converters)
        {
            var settings = (converters is object && converters.Length > 0)
                ? new NJsonSerializerSettings { Converters = converters }
                : null;
            return DeserializeFromByteArray(bytes, type, settings);
        }

        /// <summary>Deserializes the JSON to the specified .NET type using <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="bytes">The byte array containing the JSON data to read.</param>
        /// <param name="type">The type of the object to deserialize to.</param>
        /// <param name="settings">
        /// The <see cref="NJsonSerializerSettings"/> used to deserialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.
        /// </param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromByteArray(byte[] bytes, Type type, NJsonSerializerSettings settings)
        {
            var jsonSerializer = NJsonSerializer.CreateDefault(settings);

            return jsonSerializer.DeserializeFromByteArray(bytes, type);
        }


        /// <summary>Deserializes the JSON to a .NET object.</summary>
        /// <param name="bytes">The byte array containing the JSON data to read.</param>
        /// <param name="index">The index of the first byte to deserialize.</param>
        /// <param name="count">The number of bytes to deserialize.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromByteArray(byte[] bytes, int index, int count)
        {
            return DeserializeFromByteArray(bytes, index, count, null, (NJsonSerializerSettings)null);
        }

        /// <summary>Deserializes the JSON to a .NET object using <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="bytes">The byte array containing the JSON data to read.</param>
        /// <param name="index">The index of the first byte to deserialize.</param>
        /// <param name="count">The number of bytes to deserialize.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to deserialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromByteArray(byte[] bytes, int index, int count, NJsonSerializerSettings settings)
        {
            return DeserializeFromByteArray(bytes, index, count, null, settings);
        }

        /// <summary>Deserializes the JSON to the specified .NET type.</summary>
        /// <param name="bytes">The byte array containing the JSON data to read.</param>
        /// <param name="index">The index of the first byte to deserialize.</param>
        /// <param name="count">The number of bytes to deserialize.</param>
        /// <param name="type">The <see cref="Type"/> of object being deserialized.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromByteArray(byte[] bytes, int index, int count, Type type)
        {
            return DeserializeFromByteArray(bytes, index, count, type, (NJsonSerializerSettings)null);
        }

        /// <summary>Deserializes the JSON to the specified .NET type.</summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="bytes">The byte array containing the JSON data to read.</param>
        /// <param name="index">The index of the first byte to deserialize.</param>
        /// <param name="count">The number of bytes to deserialize.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static T DeserializeFromByteArray<T>(byte[] bytes, int index, int count)
        {
            return DeserializeFromByteArray<T>(bytes, index, count, (NJsonSerializerSettings)null);
        }

        /// <summary>Deserializes the JSON to the specified .NET type using a collection of <see cref="NJsonConverter"/>.</summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="bytes">The byte array containing the JSON data to read.</param>
        /// <param name="index">The index of the first byte to deserialize.</param>
        /// <param name="count">The number of bytes to deserialize.</param>
        /// <param name="converters">Converters to use while deserializing.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static T DeserializeFromByteArray<T>(byte[] bytes, int index, int count, params NJsonConverter[] converters)
        {
            return (T)DeserializeFromByteArray(bytes, index, count, typeof(T), converters);
        }

        /// <summary>Deserializes the JSON to the specified .NET type using <see cref="NJsonSerializerSettings"/>.</summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="bytes">The byte array containing the JSON data to read.</param>
        /// <param name="index">The index of the first byte to deserialize.</param>
        /// <param name="count">The number of bytes to deserialize.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to deserialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static T DeserializeFromByteArray<T>(byte[] bytes, int index, int count, NJsonSerializerSettings settings)
        {
            return (T)DeserializeFromByteArray(bytes, index, count, typeof(T), settings);
        }

        /// <summary>Deserializes the JSON to the specified .NET type using a collection of <see cref="NJsonConverter"/>.</summary>
        /// <param name="bytes">The byte array containing the JSON data to read.</param>
        /// <param name="index">The index of the first byte to deserialize.</param>
        /// <param name="count">The number of bytes to deserialize.</param>
        /// <param name="type">The type of the object to deserialize.</param>
        /// <param name="converters">Converters to use while deserializing.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromByteArray(byte[] bytes, int index, int count, Type type, params NJsonConverter[] converters)
        {
            var settings = (converters is object && converters.Length > 0)
                ? new NJsonSerializerSettings { Converters = converters }
                : null;
            return DeserializeFromByteArray(bytes, index, count, type, settings);
        }

        /// <summary>Deserializes the JSON to the specified .NET type using <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="bytes">The byte array containing the JSON data to read.</param>
        /// <param name="index">The index of the first byte to deserialize.</param>
        /// <param name="count">The number of bytes to deserialize.</param>
        /// <param name="type">The type of the object to deserialize to.</param>
        /// <param name="settings">
        /// The <see cref="NJsonSerializerSettings"/> used to deserialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.
        /// </param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromByteArray(byte[] bytes, int index, int count, Type type, NJsonSerializerSettings settings)
        {
            var jsonSerializer = NJsonSerializer.CreateDefault(settings);

            return jsonSerializer.DeserializeFromByteArray(bytes, index, count, type);
        }

        #endregion

        #region -- Serialize to Stream --

        /// <summary>Serializes the specified object to a <see cref="Stream"/>.</summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        /// <param name="value">The object to serialize.</param>
        public static void SerializeToStream(Stream stream, object value)
        {
            SerializeToStream(stream, value, null, (NJsonSerializerSettings)null);
        }

        /// <summary>Serializes the specified object to a <see cref="Stream"/> using formatting.</summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        public static void SerializeToStream(Stream stream, object value, NFormatting formatting)
        {
            SerializeToStream(stream, value, formatting, (NJsonSerializerSettings)null);
        }

        /// <summary>Serializes the specified object to a <see cref="Stream"/> using a collection of <see cref="NJsonConverter"/>.</summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="converters">A collection of converters used while serializing.</param>
        public static void SerializeToStream(Stream stream, object value, params NJsonConverter[] converters)
        {
            var settings = (converters is object && converters.Length > 0)
                ? new NJsonSerializerSettings { Converters = converters }
                : null;
            SerializeToStream(stream, value, null, settings);
        }

        /// <summary>Serializes the specified object to a <see cref="Stream"/> using formatting and a collection of <see cref="NJsonConverter"/>.</summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <param name="converters">A collection of converters used while serializing.</param>
        public static void SerializeToStream(Stream stream, object value, NFormatting formatting, params NJsonConverter[] converters)
        {
            var settings = (converters is object && converters.Length > 0)
                ? new NJsonSerializerSettings { Converters = converters }
                : null;
            SerializeToStream(stream, value, null, formatting, settings);
        }

        /// <summary>Serializes the specified object to a <see cref="Stream"/> using <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to serialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        public static void SerializeToStream(Stream stream, object value, NJsonSerializerSettings settings)
        {
            SerializeToStream(stream, value, null, settings);
        }

        /// <summary>Serializes the specified object to a <see cref="Stream"/> using a type, formatting and <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to serialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="Newtonsoft.Json.TypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        public static void SerializeToStream(Stream stream, object value, Type type, NJsonSerializerSettings settings)
        {
            var jsonSerializer = NJsonSerializer.CreateDefault(settings);

            jsonSerializer.SerializeToStream(stream, value, type);
        }

        /// <summary>Serializes the specified object to a <see cref="Stream"/> using formatting and <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to serialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        public static void SerializeToStream(Stream stream, object value, NFormatting formatting, NJsonSerializerSettings settings)
        {
            SerializeToStream(stream, value, null, formatting, settings);
        }

        /// <summary>Serializes the specified object to a <see cref="Stream"/> using a type, formatting and <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="stream">The <see cref="Stream"/> to write to.</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to serialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="Newtonsoft.Json.TypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        public static void SerializeToStream(Stream stream, object value, Type type, NFormatting formatting, NJsonSerializerSettings settings)
        {
            var jsonSerializer = NJsonSerializer.CreateDefault(settings);
            jsonSerializer.Formatting = formatting;

            jsonSerializer.SerializeToStream(stream, value, type);
        }

        #endregion

        #region -- Deserialize from Stream --

        /// <summary>Deserializes the JSON to a .NET object.</summary>
        /// <param name="stream">The <see cref="Stream"/> containing the JSON data to read.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromStream(Stream stream)
        {
            return DeserializeFromStream(stream, null, (NJsonSerializerSettings)null);
        }

        /// <summary>Deserializes the JSON to a .NET object using <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="stream">The <see cref="Stream"/> containing the JSON data to read.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to deserialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromStream(Stream stream, NJsonSerializerSettings settings)
        {
            return DeserializeFromStream(stream, null, settings);
        }

        /// <summary>Deserializes the JSON to the specified .NET type.</summary>
        /// <param name="stream">The <see cref="Stream"/> containing the JSON data to read.</param>
        /// <param name="type">The <see cref="Type"/> of object being deserialized.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromStream(Stream stream, Type type)
        {
            return DeserializeFromStream(stream, type, (NJsonSerializerSettings)null);
        }

        /// <summary>Deserializes the JSON to the specified .NET type.</summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="stream">The <see cref="Stream"/> containing the JSON data to read.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static T DeserializeFromStream<T>(Stream stream)
        {
            return DeserializeFromStream<T>(stream, (NJsonSerializerSettings)null);
        }

        /// <summary>Deserializes the JSON to the specified .NET type using a collection of <see cref="NJsonConverter"/>.</summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="stream">The <see cref="Stream"/> containing the JSON data to read.</param>
        /// <param name="converters">Converters to use while deserializing.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static T DeserializeFromStream<T>(Stream stream, params NJsonConverter[] converters)
        {
            return (T)DeserializeFromStream(stream, typeof(T), converters);
        }

        /// <summary>Deserializes the JSON to the specified .NET type using <see cref="NJsonSerializerSettings"/>.</summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="stream">The <see cref="Stream"/> containing the JSON data to read.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to deserialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static T DeserializeFromStream<T>(Stream stream, NJsonSerializerSettings settings)
        {
            return (T)DeserializeFromStream(stream, typeof(T), settings);
        }

        /// <summary>Deserializes the JSON to the specified .NET type using a collection of <see cref="NJsonConverter"/>.</summary>
        /// <param name="stream">The <see cref="Stream"/> containing the JSON data to read.</param>
        /// <param name="type">The type of the object to deserialize.</param>
        /// <param name="converters">Converters to use while deserializing.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromStream(Stream stream, Type type, params NJsonConverter[] converters)
        {
            var settings = (converters is object && converters.Length > 0)
                ? new NJsonSerializerSettings { Converters = converters }
                : null;
            return DeserializeFromStream(stream, type, settings);
        }

        /// <summary>Deserializes the JSON to the specified .NET type using <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="stream">The <see cref="Stream"/> containing the JSON data to read.</param>
        /// <param name="type">The type of the object to deserialize to.</param>
        /// <param name="settings">
        /// The <see cref="NJsonSerializerSettings"/> used to deserialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.
        /// </param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromStream(Stream stream, Type type, NJsonSerializerSettings settings)
        {
            var jsonSerializer = NJsonSerializer.CreateDefault(settings);
            return jsonSerializer.DeserializeFromStream(stream, type);
        }

        #endregion

        #region -- Serialize to TextWriter --

        /// <summary>Serializes the specified object to a <see cref="TextWriter"/>.</summary>
        /// <param name="textWriter">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="value">The object to serialize.</param>
        public static void SerializeToWriter(TextWriter textWriter, object value)
        {
            SerializeToWriter(textWriter, value, null, (NJsonSerializerSettings)null);
        }

        /// <summary>Serializes the specified object to a <see cref="TextWriter"/> using formatting.</summary>
        /// <param name="textWriter">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        public static void SerializeToWriter(TextWriter textWriter, object value, NFormatting formatting)
        {
            SerializeToWriter(textWriter, value, formatting, (NJsonSerializerSettings)null);
        }

        /// <summary>Serializes the specified object to a <see cref="TextWriter"/> using a collection of <see cref="NJsonConverter"/>.</summary>
        /// <param name="textWriter">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="converters">A collection of converters used while serializing.</param>
        public static void SerializeToWriter(TextWriter textWriter, object value, params NJsonConverter[] converters)
        {
            var settings = (converters is object && converters.Length > 0)
                ? new NJsonSerializerSettings { Converters = converters }
                : null;
            SerializeToWriter(textWriter, value, null, settings);
        }

        /// <summary>Serializes the specified object to a <see cref="TextWriter"/> using formatting and a collection of <see cref="NJsonConverter"/>.</summary>
        /// <param name="textWriter">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <param name="converters">A collection of converters used while serializing.</param>
        public static void SerializeToWriter(TextWriter textWriter, object value, NFormatting formatting, params NJsonConverter[] converters)
        {
            var settings = (converters is object && converters.Length > 0)
                ? new NJsonSerializerSettings { Converters = converters }
                : null;
            SerializeToWriter(textWriter, value, null, formatting, settings);
        }

        /// <summary>Serializes the specified object to a <see cref="TextWriter"/> using <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="textWriter">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to serialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        public static void SerializeToWriter(TextWriter textWriter, object value, NJsonSerializerSettings settings)
        {
            SerializeToWriter(textWriter, value, null, settings);
        }

        /// <summary>Serializes the specified object to a JSON byte array using a type, formatting and <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="textWriter">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to serialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="Newtonsoft.Json.TypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        public static void SerializeToWriter(TextWriter textWriter, object value, Type type, NJsonSerializerSettings settings)
        {
            var jsonSerializer = NJsonSerializer.CreateDefault(settings);

            jsonSerializer.SerializeToWriter(textWriter, value, type);
        }

        /// <summary>Serializes the specified object to a <see cref="TextWriter"/> using formatting and <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="textWriter">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to serialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        public static void SerializeToWriter(TextWriter textWriter, object value, NFormatting formatting, NJsonSerializerSettings settings)
        {
            SerializeToWriter(textWriter, value, null, formatting, settings);
        }

        /// <summary>Serializes the specified object to a <see cref="TextWriter"/> using a type, formatting and <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="textWriter">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to serialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="Newtonsoft.Json.TypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        public static void SerializeToWriter(TextWriter textWriter, object value, Type type, NFormatting formatting, NJsonSerializerSettings settings)
        {
            var jsonSerializer = NJsonSerializer.CreateDefault(settings);
            jsonSerializer.Formatting = formatting;

            jsonSerializer.SerializeToWriter(textWriter, value, type);
        }

        #endregion

        #region -- Deserialize from TextReader --

        /// <summary>Deserializes the JSON to a .NET object.</summary>
        /// <param name="reader">The <see cref="TextReader"/> containing the JSON data to read.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromReader(TextReader reader)
        {
            return DeserializeFromReader(reader, null, (NJsonSerializerSettings)null);
        }

        /// <summary>Deserializes the JSON to a .NET object using <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="reader">The <see cref="TextReader"/> containing the JSON data to read.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to deserialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromReader(TextReader reader, NJsonSerializerSettings settings)
        {
            return DeserializeFromReader(reader, null, settings);
        }

        /// <summary>Deserializes the JSON to the specified .NET type.</summary>
        /// <param name="reader">The <see cref="TextReader"/> containing the JSON data to read.</param>
        /// <param name="type">The <see cref="Type"/> of object being deserialized.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromReader(TextReader reader, Type type)
        {
            return DeserializeFromReader(reader, type, (NJsonSerializerSettings)null);
        }

        /// <summary>Deserializes the JSON to the specified .NET type.</summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="reader">The <see cref="TextReader"/> containing the JSON data to read.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static T DeserializeFromReader<T>(TextReader reader)
        {
            return DeserializeFromReader<T>(reader, (NJsonSerializerSettings)null);
        }

        /// <summary>Deserializes the JSON to the specified .NET type using a collection of <see cref="NJsonConverter"/>.</summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="reader">The <see cref="TextReader"/> containing the JSON data to read.</param>
        /// <param name="converters">Converters to use while deserializing.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static T DeserializeFromReader<T>(TextReader reader, params NJsonConverter[] converters)
        {
            return (T)DeserializeFromReader(reader, typeof(T), converters);
        }

        /// <summary>Deserializes the JSON to the specified .NET type using <see cref="NJsonSerializerSettings"/>.</summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="reader">The <see cref="TextReader"/> containing the JSON data to read.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to deserialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static T DeserializeFromReader<T>(TextReader reader, NJsonSerializerSettings settings)
        {
            return (T)DeserializeFromReader(reader, typeof(T), settings);
        }

        /// <summary>Deserializes the JSON to the specified .NET type using a collection of <see cref="NJsonConverter"/>.</summary>
        /// <param name="reader">The <see cref="TextReader"/> containing the JSON data to read.</param>
        /// <param name="type">The type of the object to deserialize.</param>
        /// <param name="converters">Converters to use while deserializing.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromReader(TextReader reader, Type type, params NJsonConverter[] converters)
        {
            var settings = (converters is object && converters.Length > 0)
                ? new NJsonSerializerSettings { Converters = converters }
                : null;
            return DeserializeFromReader(reader, type, settings);
        }

        /// <summary>Deserializes the JSON to the specified .NET type using <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="reader">The <see cref="TextReader"/> containing the JSON data to read.</param>
        /// <param name="type">The type of the object to deserialize to.</param>
        /// <param name="settings">
        /// The <see cref="NJsonSerializerSettings"/> used to deserialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.
        /// </param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromReader(TextReader reader, Type type, NJsonSerializerSettings settings)
        {
            var jsonSerializer = NJsonSerializer.CreateDefault(settings);
            return jsonSerializer.DeserializeFromReader(reader, type);
        }

        #endregion
    }
}
