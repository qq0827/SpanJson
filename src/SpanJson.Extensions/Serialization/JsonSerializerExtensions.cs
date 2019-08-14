using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using CuteAnt.Buffers;
using CuteAnt.IO;
using CuteAnt.Pool;
using CuteAnt.Text;
using NJsonReader = Newtonsoft.Json.JsonReader;
using NJsonSerializer = Newtonsoft.Json.JsonSerializer;
using NJsonTextReader = Newtonsoft.Json.JsonTextReader;
using NJsonTextWriter = Newtonsoft.Json.JsonTextWriter;
using NJsonToken = Newtonsoft.Json.JsonToken;
using NFormatting = Newtonsoft.Json.Formatting;
using NTypeNameHandling = Newtonsoft.Json.TypeNameHandling;

namespace SpanJson.Serialization
{
    public static class JsonSerializerExtensions
    {
        private static readonly Encoding UTF8NoBOM = StringHelper.UTF8NoBOM;
        private static readonly ArrayPool<byte> s_sharedBufferPool = ArrayPool<byte>.Shared;
        private const int c_initialBufferSize = 1024 * 64;

        #region -- SerializeObject --

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializer">The <see cref="NJsonSerializer"/> used to serialize the object</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="NTypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string SerializeObject(this NJsonSerializer jsonSerializer, object value, Type type = null)
        {
            using (var pooledStringWriter = StringWriterManager.Create())
            {
                var sw = pooledStringWriter.Object;

                return SerializeInternal(jsonSerializer, sw, value, type);
            }
        }

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializer">The <see cref="NJsonSerializer"/> used to serialize the object</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="writeIndented">Defines whether JSON should pretty print which includes:
        /// indenting nested JSON tokens, adding new lines, and adding white space between property names and values.
        /// By default, the JSON is serialized without any extra white space.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="NTypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string SerializeObject(this NJsonSerializer jsonSerializer, object value, bool writeIndented, Type type = null)
        {
            var sw = StringWriterManager.Allocate();

            var previousFormatting = jsonSerializer.GetFormatting();
            jsonSerializer.Formatting = writeIndented ? NFormatting.Indented : NFormatting.None;

            try
            {
                return SerializeInternal(jsonSerializer, sw, value, type);
            }
            finally
            {
                StringWriterManager.Free(sw);
                jsonSerializer.SetFormatting(previousFormatting);
            }
        }

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializerPool">The <see cref="NJsonSerializer"/> pool used to serialize the object</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="NTypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string SerializeObject(this ObjectPool<NJsonSerializer> jsonSerializerPool, object value, Type type = null)
        {
            var sw = StringWriterManager.Allocate();
            var jsonSerializer = jsonSerializerPool.Take();

            try
            {
                return SerializeInternal(jsonSerializer, sw, value, type);
            }
            finally
            {
                StringWriterManager.Free(sw);
                jsonSerializerPool.Return(jsonSerializer);
            }
        }

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializerPool">The <see cref="NJsonSerializer"/> pool used to serialize the object</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="writeIndented">Defines whether JSON should pretty print which includes:
        /// indenting nested JSON tokens, adding new lines, and adding white space between property names and values.
        /// By default, the JSON is serialized without any extra white space.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="NTypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string SerializeObject(this ObjectPool<NJsonSerializer> jsonSerializerPool, object value, bool writeIndented, Type type = null)
        {
            var sw = StringWriterManager.Allocate();
            var jsonSerializer = jsonSerializerPool.Take();

            var previousFormatting = jsonSerializer.GetFormatting();
            jsonSerializer.Formatting = writeIndented ? NFormatting.Indented : NFormatting.None;

            try
            {
                return SerializeInternal(jsonSerializer, sw, value, type);
            }
            finally
            {
                StringWriterManager.Free(sw);
                jsonSerializer.SetFormatting(previousFormatting);
                jsonSerializerPool.Return(jsonSerializer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string SerializeInternal(NJsonSerializer jsonSerializer, StringWriterX sw, object value, Type type)
        {
            using (NJsonTextWriter jsonWriter = new NJsonTextWriter(sw))
            {
                jsonWriter.ArrayPool = JsonConvertX.GlobalCharacterArrayPool;
                jsonWriter.CloseOutput = false;
                jsonWriter.Formatting = jsonSerializer.Formatting;

                jsonSerializer.Serialize(jsonWriter, value, type);
                jsonWriter.Flush();
            }
            return sw.ToString();
        }

        #endregion

        #region -- DeserializeObject --

        /// <summary>Deserializes the JSON to the specified .NET type using <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializer">The <see cref="NJsonSerializer"/> used to deserialize the object.</param>
        /// <param name="value">The JSON to deserialize.</param>
        /// <param name="type">The type of the object to deserialize to.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeObject(this NJsonSerializer jsonSerializer, string value, Type type = null)
        {
            if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }

            var isCheckAdditionalContentNoSet = !jsonSerializer.IsCheckAdditionalContentSetX();
            try
            {
                // by default DeserializeObject should check for additional content
                if (isCheckAdditionalContentNoSet)
                {
                    jsonSerializer.CheckAdditionalContent = true;
                }

                using (var reader = new NJsonTextReader(new StringReader(value)))
                {
                    reader.ArrayPool = JsonConvertX.GlobalCharacterArrayPool;
                    reader.CloseInput = false;

                    return jsonSerializer.Deserialize(reader, type);
                }
            }
            finally
            {
                if (isCheckAdditionalContentNoSet) { jsonSerializer.SetCheckAdditionalContent(); }
            }
        }

        /// <summary>Deserializes the JSON to the specified .NET type using <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializerPool">The <see cref="NJsonSerializer"/> pool used to deserialize the object.</param>
        /// <param name="value">The JSON to deserialize.</param>
        /// <param name="type">The type of the object to deserialize to.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeObject(this ObjectPool<NJsonSerializer> jsonSerializerPool, string value, Type type = null)
        {
            if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }

            var jsonSerializer = jsonSerializerPool.Take();
            var isCheckAdditionalContentNoSet = !jsonSerializer.IsCheckAdditionalContentSetX();
            try
            {
                // by default DeserializeObject should check for additional content
                if (isCheckAdditionalContentNoSet)
                {
                    jsonSerializer.CheckAdditionalContent = true;
                }

                using (var reader = new NJsonTextReader(new StringReader(value)))
                {
                    reader.ArrayPool = JsonConvertX.GlobalCharacterArrayPool;
                    reader.CloseInput = false;

                    return jsonSerializer.Deserialize(reader, type);
                }
            }
            finally
            {
                if (isCheckAdditionalContentNoSet) { jsonSerializer.SetCheckAdditionalContent(); }
                jsonSerializerPool.Return(jsonSerializer);
            }
        }

        #endregion

        #region -- Serialize to Byte-Array --

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializer">The <see cref="NJsonSerializer"/> used to serialize the object</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="NTypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <param name="initialBufferSize"></param>
        /// <returns>A JSON string representation of the object.</returns>
        public static byte[] SerializeToByteArray(this NJsonSerializer jsonSerializer, object value, Type type = null, int initialBufferSize = c_initialBufferSize)
        {
            using (var pooledOutputStream = BufferManagerOutputStreamManager.Create())
            {
                var outputStream = pooledOutputStream.Object;
                outputStream.Reinitialize(initialBufferSize, s_sharedBufferPool);

                return SerializeToByteArrayInternal(jsonSerializer, outputStream, value, type);
            }
        }

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializer">The <see cref="NJsonSerializer"/> used to serialize the object</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="writeIndented">Defines whether JSON should pretty print which includes:
        /// indenting nested JSON tokens, adding new lines, and adding white space between property names and values.
        /// By default, the JSON is serialized without any extra white space.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="NTypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <param name="initialBufferSize"></param>
        /// <returns>A JSON string representation of the object.</returns>
        public static byte[] SerializeToByteArray(this NJsonSerializer jsonSerializer, object value, bool writeIndented, Type type = null, int initialBufferSize = c_initialBufferSize)
        {
            var outputStream = BufferManagerOutputStreamManager.Take();
            outputStream.Reinitialize(initialBufferSize, s_sharedBufferPool);

            var previousFormatting = jsonSerializer.GetFormatting();
            jsonSerializer.Formatting = writeIndented ? NFormatting.Indented : NFormatting.None;

            try
            {
                return SerializeToByteArrayInternal(jsonSerializer, outputStream, value, type);
            }
            finally
            {
                BufferManagerOutputStreamManager.Return(outputStream);
                jsonSerializer.SetFormatting(previousFormatting);
            }
        }

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializerPool">The <see cref="NJsonSerializer"/> pool used to serialize the object</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="NTypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <param name="initialBufferSize"></param>
        /// <returns>A JSON string representation of the object.</returns>
        public static byte[] SerializeToByteArray(this ObjectPool<NJsonSerializer> jsonSerializerPool, object value, Type type = null, int initialBufferSize = c_initialBufferSize)
        {
            var jsonSerializer = jsonSerializerPool.Take();
            var outputStream = BufferManagerOutputStreamManager.Take();
            outputStream.Reinitialize(initialBufferSize, s_sharedBufferPool);

            try
            {
                return SerializeToByteArrayInternal(jsonSerializer, outputStream, value, type);
            }
            finally
            {
                BufferManagerOutputStreamManager.Return(outputStream);
                jsonSerializerPool.Return(jsonSerializer);
            }
        }

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializerPool">The <see cref="NJsonSerializer"/> pool used to serialize the object</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="writeIndented">Defines whether JSON should pretty print which includes:
        /// indenting nested JSON tokens, adding new lines, and adding white space between property names and values.
        /// By default, the JSON is serialized without any extra white space.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="NTypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <param name="initialBufferSize"></param>
        /// <returns>A JSON string representation of the object.</returns>
        public static byte[] SerializeToByteArray(this ObjectPool<NJsonSerializer> jsonSerializerPool, object value, bool writeIndented, Type type = null, int initialBufferSize = c_initialBufferSize)
        {
            var jsonSerializer = jsonSerializerPool.Take();

            var previousFormatting = jsonSerializer.GetFormatting();
            jsonSerializer.Formatting = writeIndented ? NFormatting.Indented : NFormatting.None;

            var outputStream = BufferManagerOutputStreamManager.Take();
            outputStream.Reinitialize(initialBufferSize, s_sharedBufferPool);

            try
            {
                return SerializeToByteArrayInternal(jsonSerializer, outputStream, value, type);
            }
            finally
            {
                BufferManagerOutputStreamManager.Return(outputStream);
                jsonSerializer.SetFormatting(previousFormatting);
                jsonSerializerPool.Return(jsonSerializer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte[] SerializeToByteArrayInternal(NJsonSerializer jsonSerializer, BufferManagerOutputStream outputStream, object value, Type type)
        {
            using (NJsonTextWriter jsonWriter = new NJsonTextWriter(new StreamWriterX(outputStream, UTF8NoBOM)))
            {
                jsonWriter.ArrayPool = JsonConvertX.GlobalCharacterArrayPool;
                jsonWriter.CloseOutput = false;
                jsonWriter.Formatting = jsonSerializer.Formatting;

                jsonSerializer.Serialize(jsonWriter, value, type);
                jsonWriter.Flush();
            }

            return outputStream.ToByteArray();
        }

        #endregion

        #region -- Serialize ot Memory-Pool --

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializer">The <see cref="NJsonSerializer"/> used to serialize the object</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="NTypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <param name="initialBufferSize"></param>
        /// <returns>A JSON string representation of the object.</returns>
        public static ArraySegment<byte> SerializeToMemoryPool(this NJsonSerializer jsonSerializer, object value, Type type = null, int initialBufferSize = c_initialBufferSize)
        {
            using (var pooledOutputStream = BufferManagerOutputStreamManager.Create())
            {
                var outputStream = pooledOutputStream.Object;
                outputStream.Reinitialize(initialBufferSize, s_sharedBufferPool);

                return SerializeToMemoryPoolInternal(jsonSerializer, outputStream, value, type);
            }
        }

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializer">The <see cref="NJsonSerializer"/> used to serialize the object</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="writeIndented">Defines whether JSON should pretty print which includes:
        /// indenting nested JSON tokens, adding new lines, and adding white space between property names and values.
        /// By default, the JSON is serialized without any extra white space.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="NTypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <param name="initialBufferSize"></param>
        /// <returns>A JSON string representation of the object.</returns>
        public static ArraySegment<byte> SerializeToMemoryPool(this NJsonSerializer jsonSerializer, object value, bool writeIndented, Type type = null, int initialBufferSize = c_initialBufferSize)
        {
            var previousFormatting = jsonSerializer.GetFormatting();
            jsonSerializer.Formatting = writeIndented ? NFormatting.Indented : NFormatting.None;

            var outputStream = BufferManagerOutputStreamManager.Take();
            outputStream.Reinitialize(initialBufferSize, s_sharedBufferPool);

            try
            {
                return SerializeToMemoryPoolInternal(jsonSerializer, outputStream, value, type);
            }
            finally
            {
                BufferManagerOutputStreamManager.Return(outputStream);
                jsonSerializer.SetFormatting(previousFormatting);
            }
        }

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializerPool">The <see cref="NJsonSerializer"/> pool used to serialize the object</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="NTypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <param name="initialBufferSize"></param>
        /// <returns>A JSON string representation of the object.</returns>
        public static ArraySegment<byte> SerializeToMemoryPool(this ObjectPool<NJsonSerializer> jsonSerializerPool, object value, Type type = null, int initialBufferSize = c_initialBufferSize)
        {
            var jsonSerializer = jsonSerializerPool.Take();

            var outputStream = BufferManagerOutputStreamManager.Take();
            outputStream.Reinitialize(initialBufferSize, s_sharedBufferPool);

            try
            {
                return SerializeToMemoryPoolInternal(jsonSerializer, outputStream, value, type);
            }
            finally
            {
                BufferManagerOutputStreamManager.Return(outputStream);
                jsonSerializerPool.Return(jsonSerializer);
            }
        }

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializerPool">The <see cref="NJsonSerializer"/> pool used to serialize the object</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="writeIndented">Defines whether JSON should pretty print which includes:
        /// indenting nested JSON tokens, adding new lines, and adding white space between property names and values.
        /// By default, the JSON is serialized without any extra white space.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="NTypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <param name="initialBufferSize"></param>
        /// <returns>A JSON string representation of the object.</returns>
        public static ArraySegment<byte> SerializeToMemoryPool(this ObjectPool<NJsonSerializer> jsonSerializerPool, object value, bool writeIndented, Type type = null, int initialBufferSize = c_initialBufferSize)
        {
            var jsonSerializer = jsonSerializerPool.Take();

            var previousFormatting = jsonSerializer.GetFormatting();
            jsonSerializer.Formatting = writeIndented ? NFormatting.Indented : NFormatting.None;

            var outputStream = BufferManagerOutputStreamManager.Take();
            outputStream.Reinitialize(initialBufferSize, s_sharedBufferPool);

            try
            {
                return SerializeToMemoryPoolInternal(jsonSerializer, outputStream, value, type);
            }
            finally
            {
                BufferManagerOutputStreamManager.Return(outputStream);
                jsonSerializer.SetFormatting(previousFormatting);
                jsonSerializerPool.Return(jsonSerializer);
            }
        }

        private static ArraySegment<byte> SerializeToMemoryPoolInternal(NJsonSerializer jsonSerializer, BufferManagerOutputStream outputStream, object value, Type type)
        {
            using (NJsonTextWriter jsonWriter = new NJsonTextWriter(new StreamWriterX(outputStream, UTF8NoBOM)))
            {
                jsonWriter.ArrayPool = JsonConvertX.GlobalCharacterArrayPool;
                jsonWriter.CloseOutput = false;
                jsonWriter.Formatting = jsonSerializer.Formatting;

                jsonSerializer.Serialize(jsonWriter, value, type);
                jsonWriter.Flush();
            }

            return outputStream.ToArraySegment();
        }

        #endregion

        #region -- Deserialize from Byte-Array --

        /// <summary>Deserializes the JSON to the specified .NET type using <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializer">The <see cref="NJsonSerializer"/> used to deserialize the object.</param>
        /// <param name="value">The JSON to deserialize.</param>
        /// <param name="type">The type of the object to deserialize to.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromByteArray(this NJsonSerializer jsonSerializer, byte[] value, Type type = null)
        {
            if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }

            return DeserializeFromByteArray(jsonSerializer, value, 0, value.Length, type);
        }

        /// <summary>Deserializes the JSON to the specified .NET type using <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializer">The <see cref="NJsonSerializer"/> used to deserialize the object.</param>
        /// <param name="value">The JSON to deserialize.</param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="type">The type of the object to deserialize to.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromByteArray(this NJsonSerializer jsonSerializer, byte[] value, int offset, int count, Type type = null)
        {
            var isCheckAdditionalContentNoSet = !jsonSerializer.IsCheckAdditionalContentSetX();
            try
            {
                // by default DeserializeObject should check for additional content
                if (isCheckAdditionalContentNoSet)
                {
                    jsonSerializer.CheckAdditionalContent = true;
                }

                using (var reader = new NJsonTextReader(new StreamReaderX(new MemoryStream(value, offset, count), Encoding.UTF8)))
                {
                    reader.ArrayPool = JsonConvertX.GlobalCharacterArrayPool;
                    reader.CloseInput = false;

                    return jsonSerializer.Deserialize(reader, type);
                }
            }
            finally
            {
                if (isCheckAdditionalContentNoSet) { jsonSerializer.SetCheckAdditionalContent(); }
            }
        }

        /// <summary>Deserializes the JSON to the specified .NET type using <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializerPool">The <see cref="NJsonSerializer"/> pool used to deserialize the object.</param>
        /// <param name="value">The JSON to deserialize.</param>
        /// <param name="type">The type of the object to deserialize to.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromByteArray(this ObjectPool<NJsonSerializer> jsonSerializerPool, byte[] value, Type type = null)
        {
            if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }

            return DeserializeFromByteArray(jsonSerializerPool, value, 0, value.Length, type);
        }

        /// <summary>Deserializes the JSON to the specified .NET type using <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializerPool">The <see cref="NJsonSerializer"/> pool used to deserialize the object.</param>
        /// <param name="value">The JSON to deserialize.</param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="type">The type of the object to deserialize to.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromByteArray(this ObjectPool<NJsonSerializer> jsonSerializerPool, byte[] value, int offset, int count, Type type = null)
        {
            if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }

            var jsonSerializer = jsonSerializerPool.Take();
            var isCheckAdditionalContentNoSet = !jsonSerializer.IsCheckAdditionalContentSetX();
            try
            {
                // by default DeserializeObject should check for additional content
                if (isCheckAdditionalContentNoSet)
                {
                    jsonSerializer.CheckAdditionalContent = true;
                }

                using (var reader = new NJsonTextReader(new StreamReaderX(new MemoryStream(value, offset, count), Encoding.UTF8)))
                {
                    reader.ArrayPool = JsonConvertX.GlobalCharacterArrayPool;
                    reader.CloseInput = false;

                    return jsonSerializer.Deserialize(reader, type);
                }
            }
            finally
            {
                if (isCheckAdditionalContentNoSet) { jsonSerializer.SetCheckAdditionalContent(); }
                jsonSerializerPool.Return(jsonSerializer);
            }
        }

        #endregion

        #region -- Serialize to Stream --

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializer">The <see cref="NJsonSerializer"/> used to serialize the object</param>
        /// <param name="outputStream"></param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="NTypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static void SerializeToStream(this NJsonSerializer jsonSerializer, Stream outputStream, object value, Type type = null)
        {
            using (NJsonTextWriter jsonWriter = new NJsonTextWriter(new StreamWriterX(outputStream, UTF8NoBOM)))
            {
                jsonWriter.ArrayPool = JsonConvertX.GlobalCharacterArrayPool;
                jsonWriter.CloseOutput = false;
                jsonWriter.Formatting = jsonSerializer.Formatting;

                jsonSerializer.Serialize(jsonWriter, value, type);
                jsonWriter.Flush();
            }
        }

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializer">The <see cref="NJsonSerializer"/> used to serialize the object</param>
        /// <param name="outputStream"></param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="writeIndented">Defines whether JSON should pretty print which includes:
        /// indenting nested JSON tokens, adding new lines, and adding white space between property names and values.
        /// By default, the JSON is serialized without any extra white space.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="NTypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static void SerializeToStream(this NJsonSerializer jsonSerializer, Stream outputStream, object value, bool writeIndented, Type type = null)
        {
            var previousFormatting = jsonSerializer.GetFormatting();
            jsonSerializer.Formatting = writeIndented ? NFormatting.Indented : NFormatting.None;

            try
            {
                SerializeToStream(jsonSerializer, outputStream, value, type);
            }
            finally
            {
                jsonSerializer.SetFormatting(previousFormatting);
            }
        }

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializerPool">The <see cref="NJsonSerializer"/> pool used to serialize the object</param>
        /// <param name="outputStream"></param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="NTypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static void SerializeToStream(this ObjectPool<NJsonSerializer> jsonSerializerPool, Stream outputStream, object value, Type type = null)
        {
            var jsonSerializer = jsonSerializerPool.Take();
            try
            {
                SerializeToStream(jsonSerializer, outputStream, value, type);
            }
            finally
            {
                jsonSerializerPool.Return(jsonSerializer);
            }
        }

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializerPool">The <see cref="NJsonSerializer"/> pool used to serialize the object</param>
        /// <param name="outputStream"></param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="writeIndented">Defines whether JSON should pretty print which includes:
        /// indenting nested JSON tokens, adding new lines, and adding white space between property names and values.
        /// By default, the JSON is serialized without any extra white space.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="NTypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static void SerializeToStream(this ObjectPool<NJsonSerializer> jsonSerializerPool, Stream outputStream, object value, bool writeIndented, Type type = null)
        {
            var jsonSerializer = jsonSerializerPool.Take();

            var previousFormatting = jsonSerializer.GetFormatting();
            jsonSerializer.Formatting = writeIndented ? NFormatting.Indented : NFormatting.None;

            try
            {
                SerializeToStream(jsonSerializer, outputStream, value, type);
            }
            finally
            {
                jsonSerializer.SetFormatting(previousFormatting);
                jsonSerializerPool.Return(jsonSerializer);
            }
        }

        #endregion

        #region -- Deserialize from Stream --

        /// <summary>Deserializes the JSON to the specified .NET type using <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializer">The <see cref="NJsonSerializer"/> used to deserialize the object.</param>
        /// <param name="inputStream">The JSON to deserialize.</param>
        /// <param name="type">The type of the object to deserialize to.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromStream(this NJsonSerializer jsonSerializer, Stream inputStream, Type type = null)
        {
            var isCheckAdditionalContentNoSet = !jsonSerializer.IsCheckAdditionalContentSetX();
            try
            {
                // by default DeserializeObject should check for additional content
                if (isCheckAdditionalContentNoSet)
                {
                    jsonSerializer.CheckAdditionalContent = true;
                }

                using (var reader = new NJsonTextReader(new StreamReaderX(inputStream, Encoding.UTF8)))
                {
                    reader.ArrayPool = JsonConvertX.GlobalCharacterArrayPool;
                    reader.CloseInput = false;

                    return jsonSerializer.Deserialize(reader, type);
                }
            }
            finally
            {
                if (isCheckAdditionalContentNoSet) { jsonSerializer.SetCheckAdditionalContent(); }
            }
        }

        /// <summary>Deserializes the JSON to the specified .NET type using <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializerPool">The <see cref="NJsonSerializer"/> pool used to deserialize the object.</param>
        /// <param name="inputStream">The JSON to deserialize.</param>
        /// <param name="type">The type of the object to deserialize to.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromStream(this ObjectPool<NJsonSerializer> jsonSerializerPool, Stream inputStream, Type type = null)
        {
            var jsonSerializer = jsonSerializerPool.Take();
            try
            {
                return DeserializeFromStream(jsonSerializer, inputStream, type);
            }
            finally
            {
                jsonSerializerPool.Return(jsonSerializer);
            }
        }

        #endregion

        #region -- Serialize to TextWriter --

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializer">The <see cref="NJsonSerializer"/> used to serialize the object</param>
        /// <param name="textWriter"></param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="NTypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static void SerializeToWriter(this NJsonSerializer jsonSerializer, TextWriter textWriter, object value, Type type = null)
        {
            using (NJsonTextWriter jsonWriter = new NJsonTextWriter(textWriter))
            {
                jsonWriter.ArrayPool = JsonConvertX.GlobalCharacterArrayPool;
                jsonWriter.CloseOutput = false;
                jsonWriter.Formatting = jsonSerializer.Formatting;

                jsonSerializer.Serialize(jsonWriter, value, type);
                jsonWriter.Flush();
            }
        }

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializer">The <see cref="NJsonSerializer"/> used to serialize the object</param>
        /// <param name="textWriter"></param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="writeIndented">Defines whether JSON should pretty print which includes:
        /// indenting nested JSON tokens, adding new lines, and adding white space between property names and values.
        /// By default, the JSON is serialized without any extra white space.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="NTypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static void SerializeToWriter(this NJsonSerializer jsonSerializer, TextWriter textWriter, object value, bool writeIndented, Type type = null)
        {
            var previousFormatting = jsonSerializer.GetFormatting();
            jsonSerializer.Formatting = writeIndented ? NFormatting.Indented : NFormatting.None;

            try
            {
                SerializeToWriter(jsonSerializer, textWriter, value, type);
            }
            finally
            {
                jsonSerializer.SetFormatting(previousFormatting);
            }
        }

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializerPool">The <see cref="NJsonSerializer"/> pool used to serialize the object</param>
        /// <param name="textWriter"></param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="NTypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static void SerializeToWriter(this ObjectPool<NJsonSerializer> jsonSerializerPool, TextWriter textWriter, object value, Type type = null)
        {
            var jsonSerializer = jsonSerializerPool.Take();
            try
            {
                SerializeToWriter(jsonSerializer, textWriter, value, type);
            }
            finally
            {
                jsonSerializerPool.Return(jsonSerializer);
            }
        }

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializerPool">The <see cref="NJsonSerializer"/> pool used to serialize the object</param>
        /// <param name="textWriter"></param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="writeIndented">Defines whether JSON should pretty print which includes:
        /// indenting nested JSON tokens, adding new lines, and adding white space between property names and values.
        /// By default, the JSON is serialized without any extra white space.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="NTypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static void SerializeToWriter(this ObjectPool<NJsonSerializer> jsonSerializerPool, TextWriter textWriter, object value, bool writeIndented, Type type = null)
        {
            var jsonSerializer = jsonSerializerPool.Take();

            var previousFormatting = jsonSerializer.GetFormatting();
            jsonSerializer.Formatting = writeIndented ? NFormatting.Indented : NFormatting.None;

            try
            {
                SerializeToWriter(jsonSerializer, textWriter, value, type);
            }
            finally
            {
                jsonSerializer.SetFormatting(previousFormatting);
                jsonSerializerPool.Return(jsonSerializer);
            }
        }

        #endregion

        #region -- Deserialize from TextReader --

        /// <summary>Deserializes the JSON to the specified .NET type using <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializer">The <see cref="NJsonSerializer"/> used to deserialize the object.</param>
        /// <param name="textReader">The JSON to deserialize.</param>
        /// <param name="type">The type of the object to deserialize to.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromReader(this NJsonSerializer jsonSerializer, TextReader textReader, Type type = null)
        {
            var isCheckAdditionalContentNoSet = !jsonSerializer.IsCheckAdditionalContentSetX();
            try
            {
                // by default DeserializeObject should check for additional content
                if (isCheckAdditionalContentNoSet)
                {
                    jsonSerializer.CheckAdditionalContent = true;
                }

                using (var reader = new NJsonTextReader(textReader))
                {
                    reader.ArrayPool = JsonConvertX.GlobalCharacterArrayPool;
                    reader.CloseInput = false;

                    return jsonSerializer.Deserialize(reader, type);
                }
            }
            finally
            {
                if (isCheckAdditionalContentNoSet) { jsonSerializer.SetCheckAdditionalContent(); }
            }
        }

        /// <summary>Deserializes the JSON to the specified .NET type using <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializerPool">The <see cref="NJsonSerializer"/> pool used to deserialize the object.</param>
        /// <param name="textReader">The JSON to deserialize.</param>
        /// <param name="type">The type of the object to deserialize to.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeFromReader(this ObjectPool<NJsonSerializer> jsonSerializerPool, TextReader textReader, Type type = null)
        {
            var jsonSerializer = jsonSerializerPool.Take();
            try
            {
                return DeserializeFromReader(jsonSerializer, textReader, type);
            }
            finally
            {
                jsonSerializerPool.Return(jsonSerializer);
            }
        }

        #endregion

        #region -- PopulateObject --

        /// <summary>Populates the object with values from the JSON string using <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializer">The <see cref="NJsonSerializer"/> used to deserialize the object.</param>
        /// <param name="value">The JSON to populate values from.</param>
        /// <param name="target">The target object to populate values onto.</param>
        public static void PopulateObject(this NJsonSerializer jsonSerializer, object target, string value)
        {
            using (var jsonReader = new NJsonTextReader(new StringReader(value)))
            {
                jsonReader.ArrayPool = JsonConvertX.GlobalCharacterArrayPool;
                jsonReader.CloseInput = false;

                jsonSerializer.Populate(jsonReader, target);

                if (jsonSerializer.CheckAdditionalContent)
                {
                    while (jsonReader.Read())
                    {
                        if (jsonReader.TokenType != NJsonToken.Comment)
                        {
                            ThrowJsonSerializationException(jsonReader);
                        }
                    }
                }
            }
        }

        /// <summary>Populates the object with values from the JSON string using <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializerPool">The <see cref="NJsonSerializer"/> pool used to deserialize the object.</param>
        /// <param name="value">The JSON to populate values from.</param>
        /// <param name="target">The target object to populate values onto.</param>
        public static void PopulateObject(this ObjectPool<NJsonSerializer> jsonSerializerPool, object target, string value)
        {
            var jsonSerializer = jsonSerializerPool.Take();
            try
            {
                jsonSerializer.PopulateObject(target, value);
            }
            finally
            {
                jsonSerializerPool.Return(jsonSerializer);
            }
        }

        /// <summary>Populates the object with values from the JSON string using <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializer">The <see cref="NJsonSerializer"/> used to deserialize the object.</param>
        /// <param name="value">The JSON to populate values from.</param>
        /// <param name="target">The target object to populate values onto.</param>
        public static void PopulateObject(this NJsonSerializer jsonSerializer, object target, byte[] value)
        {
            if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }

            PopulateObject(jsonSerializer, target, value, 0, value.Length);
        }

        /// <summary>Populates the object with values from the JSON string using <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializer">The <see cref="NJsonSerializer"/> used to deserialize the object.</param>
        /// <param name="value">The JSON to populate values from.</param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="target">The target object to populate values onto.</param>
        public static void PopulateObject(this NJsonSerializer jsonSerializer, object target, byte[] value, int offset, int count)
        {
            using (var jsonReader = new NJsonTextReader(new StreamReaderX(new MemoryStream(value, offset, count), Encoding.UTF8)))
            {
                jsonReader.ArrayPool = JsonConvertX.GlobalCharacterArrayPool;
                jsonReader.CloseInput = false;

                jsonSerializer.Populate(jsonReader, target);

                if (jsonSerializer.CheckAdditionalContent)
                {
                    while (jsonReader.Read())
                    {
                        if (jsonReader.TokenType != NJsonToken.Comment)
                        {
                            ThrowJsonSerializationException(jsonReader);
                        }
                    }
                }
            }
        }

        /// <summary>Populates the object with values from the JSON string using <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializerPool">The <see cref="NJsonSerializer"/> pool used to deserialize the object.</param>
        /// <param name="value">The JSON to populate values from.</param>
        /// <param name="target">The target object to populate values onto.</param>
        public static void PopulateObject(this ObjectPool<NJsonSerializer> jsonSerializerPool, object target, byte[] value)
        {
            if (value is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }

            PopulateObject(jsonSerializerPool, target, value, 0, value.Length);
        }

        /// <summary>Populates the object with values from the JSON string using <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializerPool">The <see cref="NJsonSerializer"/> pool used to deserialize the object.</param>
        /// <param name="value">The JSON to populate values from.</param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="target">The target object to populate values onto.</param>
        public static void PopulateObject(this ObjectPool<NJsonSerializer> jsonSerializerPool, object target, byte[] value, int offset, int count)
        {
            var jsonSerializer = jsonSerializerPool.Take();
            try
            {
                jsonSerializer.PopulateObject(target, value, offset, count);
            }
            finally
            {
                jsonSerializerPool.Return(jsonSerializer);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowJsonSerializationException(NJsonReader jsonReader)
        {
            throw JsonConvertX.CreateJsonSerializationException(jsonReader, "Additional text found in JSON string after finishing deserializing object.");
        }

        #endregion
    }
}
