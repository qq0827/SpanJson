using System;
using System.Buffers;
using System.IO;
using System.Text;
using CuteAnt.Buffers;
using CuteAnt.Pool;
using CuteAnt.Text;
using NJsonSerializer = Newtonsoft.Json.JsonSerializer;
using JsonTextWriter = Newtonsoft.Json.JsonTextWriter;
using JsonTextReader = Newtonsoft.Json.JsonTextReader;
using TypeNameHandling = Newtonsoft.Json.TypeNameHandling;

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
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="TypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string SerializeObject(this NJsonSerializer jsonSerializer, object value, Type type = null)
        {
            using (var pooledStringWriter = StringWriterManager.Create())
            {
                var sw = pooledStringWriter.Object;

                using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
                {
                    jsonWriter.ArrayPool = JsonConvertX.GlobalCharacterArrayPool;
                    jsonWriter.CloseOutput = false;
                    jsonWriter.Formatting = jsonSerializer.Formatting;

                    jsonSerializer.Serialize(jsonWriter, value, type);
                    jsonWriter.Flush();
                }
                return sw.ToString();
            }
        }

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializerPool">The <see cref="NJsonSerializer"/> pool used to serialize the object</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="TypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string SerializeObject(this ObjectPool<NJsonSerializer> jsonSerializerPool, object value, Type type = null)
        {
            var sw = StringWriterManager.Allocate();
            var jsonSerializer = jsonSerializerPool.Take();

            try
            {
                using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
                {
                    jsonWriter.ArrayPool = JsonConvertX.GlobalCharacterArrayPool;
                    jsonWriter.CloseOutput = false;
                    jsonWriter.Formatting = jsonSerializer.Formatting;

                    jsonSerializer.Serialize(jsonWriter, value, type);
                    jsonWriter.Flush();
                }
                return sw.ToString();
            }
            finally
            {
                StringWriterManager.Free(sw);
                jsonSerializerPool.Return(jsonSerializer);
            }
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
            if (null == value) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }

            var isCheckAdditionalContentNoSet = !jsonSerializer.IsCheckAdditionalContentSetX();
            try
            {
                // by default DeserializeObject should check for additional content
                if (isCheckAdditionalContentNoSet)
                {
                    jsonSerializer.CheckAdditionalContent = true;
                }

                using (var reader = new JsonTextReader(new StringReader(value)))
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
            if (null == value) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }

            var jsonSerializer = jsonSerializerPool.Take();
            var isCheckAdditionalContentNoSet = !jsonSerializer.IsCheckAdditionalContentSetX();
            try
            {
                // by default DeserializeObject should check for additional content
                if (isCheckAdditionalContentNoSet)
                {
                    jsonSerializer.CheckAdditionalContent = true;
                }

                using (var reader = new JsonTextReader(new StringReader(value)))
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
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="TypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <param name="initialBufferSize"></param>
        /// <returns>A JSON string representation of the object.</returns>
        public static byte[] SerializeToByteArray(this NJsonSerializer jsonSerializer, object value, Type type = null, int initialBufferSize = c_initialBufferSize)
        {
            using (var pooledOutputStream = BufferManagerOutputStreamManager.Create())
            {
                var outputStream = pooledOutputStream.Object;
                outputStream.Reinitialize(initialBufferSize, s_sharedBufferPool);

                using (JsonTextWriter jsonWriter = new JsonTextWriter(new StreamWriterX(outputStream, UTF8NoBOM)))
                {
                    jsonWriter.ArrayPool = JsonConvertX.GlobalCharacterArrayPool;
                    jsonWriter.CloseOutput = false;
                    jsonWriter.Formatting = jsonSerializer.Formatting;

                    jsonSerializer.Serialize(jsonWriter, value, type);
                    jsonWriter.Flush();
                }
                return outputStream.ToByteArray();
            }
        }

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializerPool">The <see cref="NJsonSerializer"/> pool used to serialize the object</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="TypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
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
                using (JsonTextWriter jsonWriter = new JsonTextWriter(new StreamWriterX(outputStream, UTF8NoBOM)))
                {
                    jsonWriter.ArrayPool = JsonConvertX.GlobalCharacterArrayPool;
                    jsonWriter.CloseOutput = false;
                    jsonWriter.Formatting = jsonSerializer.Formatting;

                    jsonSerializer.Serialize(jsonWriter, value, type);
                    jsonWriter.Flush();
                }
                return outputStream.ToByteArray();
            }
            finally
            {
                BufferManagerOutputStreamManager.Return(outputStream);
                jsonSerializerPool.Return(jsonSerializer);
            }
        }

        #endregion

        #region -- Serialize ot Memory-Pool --

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializer">The <see cref="NJsonSerializer"/> used to serialize the object</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="TypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <param name="initialBufferSize"></param>
        /// <returns>A JSON string representation of the object.</returns>
        public static ArraySegment<byte> SerializeToMemoryPool(this NJsonSerializer jsonSerializer, object value, Type type = null, int initialBufferSize = c_initialBufferSize)
        {
            using (var pooledOutputStream = BufferManagerOutputStreamManager.Create())
            {
                var outputStream = pooledOutputStream.Object;
                outputStream.Reinitialize(initialBufferSize, s_sharedBufferPool);

                using (JsonTextWriter jsonWriter = new JsonTextWriter(new StreamWriterX(outputStream, UTF8NoBOM)))
                {
                    jsonWriter.ArrayPool = JsonConvertX.GlobalCharacterArrayPool;
                    jsonWriter.CloseOutput = false;
                    jsonWriter.Formatting = jsonSerializer.Formatting;

                    jsonSerializer.Serialize(jsonWriter, value, type);
                    jsonWriter.Flush();
                }
                return outputStream.ToArraySegment();
            }
        }

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializerPool">The <see cref="NJsonSerializer"/> pool used to serialize the object</param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="TypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
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
                using (JsonTextWriter jsonWriter = new JsonTextWriter(new StreamWriterX(outputStream, UTF8NoBOM)))
                {
                    jsonWriter.ArrayPool = JsonConvertX.GlobalCharacterArrayPool;
                    jsonWriter.CloseOutput = false;
                    jsonWriter.Formatting = jsonSerializer.Formatting;

                    jsonSerializer.Serialize(jsonWriter, value, type);
                    jsonWriter.Flush();
                }
                return outputStream.ToArraySegment();
            }
            finally
            {
                BufferManagerOutputStreamManager.Return(outputStream);
                jsonSerializerPool.Return(jsonSerializer);
            }
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
            if (null == value) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }

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

                using (var reader = new JsonTextReader(new StreamReaderX(new MemoryStream(value, offset, count), Encoding.UTF8)))
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
            if (null == value) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }

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
            if (null == value) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value); }

            var jsonSerializer = jsonSerializerPool.Take();
            var isCheckAdditionalContentNoSet = !jsonSerializer.IsCheckAdditionalContentSetX();
            try
            {
                // by default DeserializeObject should check for additional content
                if (isCheckAdditionalContentNoSet)
                {
                    jsonSerializer.CheckAdditionalContent = true;
                }

                using (var reader = new JsonTextReader(new StreamReaderX(new MemoryStream(value, offset, count), Encoding.UTF8)))
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
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="TypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static void SerializeToStream(this NJsonSerializer jsonSerializer, Stream outputStream, object value, Type type = null)
        {
            using (JsonTextWriter jsonWriter = new JsonTextWriter(new StreamWriterX(outputStream, UTF8NoBOM)))
            {
                jsonWriter.ArrayPool = JsonConvertX.GlobalCharacterArrayPool;
                jsonWriter.CloseOutput = false;
                jsonWriter.Formatting = jsonSerializer.Formatting;

                jsonSerializer.Serialize(jsonWriter, value, type);
                jsonWriter.Flush();
            }
        }

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializerPool">The <see cref="NJsonSerializer"/> pool used to serialize the object</param>
        /// <param name="outputStream"></param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="TypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
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

                using (var reader = new JsonTextReader(new StreamReaderX(inputStream, Encoding.UTF8)))
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
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="TypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static void SerializeToWriter(this NJsonSerializer jsonSerializer, TextWriter textWriter, object value, Type type = null)
        {
            using (JsonTextWriter jsonWriter = new JsonTextWriter(textWriter))
            {
                jsonWriter.ArrayPool = JsonConvertX.GlobalCharacterArrayPool;
                jsonWriter.CloseOutput = false;
                jsonWriter.Formatting = jsonSerializer.Formatting;

                jsonSerializer.Serialize(jsonWriter, value, type);
                jsonWriter.Flush();
            }
        }

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializer"/>.</summary>
        /// <param name="jsonSerializerPool">The <see cref="NJsonSerializer"/> pool used to serialize the object</param>
        /// <param name="textWriter"></param>
        /// <param name="value">The object to serialize.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="TypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
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

                using (var reader = new JsonTextReader(textReader))
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
    }
}
