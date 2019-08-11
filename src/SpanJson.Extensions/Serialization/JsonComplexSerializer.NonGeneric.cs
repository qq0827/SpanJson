using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SpanJson.Linq;

namespace SpanJson.Serialization
{
    partial class JsonComplexSerializer<TResolver, TUtf8Resolver>
    {
        #region -- Utf16 Serialize --

        /// <summary>Serialize to string.</summary>
        /// <param name="input">Input</param>
        /// <returns>String</returns>
        public string SerializeObject(object input)
        {
            if (input == null) { return JsonSerializer.Generic.Utf16.Serialize<object>(null); }

            var inputType = input.GetType();
            if (IsPolymorphically(inputType))
            {
                return SerializerPool.SerializeObject(input, inputType);
            }
            var invoker = Utf16Invokers.GetOrAdd(inputType, Utf16InvokerFactory);
            return invoker.ToStringSerializer(input);
        }

        /// <summary>Serialize to string.</summary>
        /// <param name="input">Input</param>
        /// <returns>String</returns>
        public char[] SerializeObjectToCharArray(object input)
        {
            if (input == null) { return JsonSerializer.Generic.Utf16.SerializeToCharArray<object>(null); }

            var inputType = input.GetType();
            if (IsPolymorphically(inputType))
            {
                return SerializerPool.SerializeObject(input, inputType).ToCharArray();
            }
            var invoker = Utf16Invokers.GetOrAdd(inputType, Utf16InvokerFactory);
            return invoker.ToCharArraySerializer(input);
        }

        /// <summary>Serialize to char buffer from ArrayPool.
        /// The returned ArraySegment's Array needs to be returned to the ArrayPool.</summary>
        /// <param name="input">Input</param>
        /// <returns>Char array from ArrayPool</returns>
        public ArraySegment<char> SerializeObjectToArrayPool(object input)
        {
            if (input == null) { return JsonSerializer.Generic.Utf16.SerializeToArrayPool<object>(null); }

            var inputType = input.GetType();
            if (IsPolymorphically(inputType))
            {
                var token = JToken.FromPolymorphicObject(input);
                return JsonSerializer.Generic.Inner<JToken, char, TResolver>.InnerSerializeToCharArrayPool(token);
            }
            var invoker = Utf16Invokers.GetOrAdd(inputType, Utf16InvokerFactory);
            return invoker.ToCharArrayPoolSerializer(input);
        }

        /// <summary>Serialize to TextWriter.</summary>
        /// <param name="input">Input</param>
        /// <param name="writer">TextWriter</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>Task</returns>
        public ValueTask SerializeObjectAsync(object input, TextWriter writer, CancellationToken cancellationToken = default)
        {
            if (input == null) { return JsonSerializer.Generic.Utf16.SerializeAsync<object>(null, writer, cancellationToken); }

            var inputType = input.GetType();
            if (IsPolymorphically(inputType))
            {
                SerializerPool.SerializeToWriter(writer, input, inputType);
                return default;
            }
            var invoker = Utf16Invokers.GetOrAdd(inputType, Utf16InvokerFactory);
            return invoker.ToTextWriterSerializerAsync(input, writer, cancellationToken);
        }

        #endregion

        #region -- Utf16 Deserialize --

        /// <summary>Deserialize from string.</summary>
        /// <param name="input">Input</param>
        /// <param name="type">Object Type</param>
        /// <returns>Deserialized object</returns>
        public object Deserialize(string input, Type type)
        {
            if (null == type) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

            if (IsPolymorphically(type))
            {
                return DeserializerPool.DeserializeObject(input, type);
            }
            var invoker = Utf16Invokers.GetOrAdd(type, Utf16InvokerFactory);
#if NETSTANDARD2_0 || NET471 || NET451
            return invoker.Deserializer(input.AsSpan());
#else
            return invoker.Deserializer(input);
#endif
        }

        /// <summary>Deserialize from string.</summary>
        /// <param name="input">Input</param>
        /// <param name="type">Object Type</param>
        /// <returns>Deserialized object</returns>
        public object Deserialize(char[] input, Type type)
        {
            if (null == type) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

            if (IsPolymorphically(type))
            {
                return DeserializerPool.DeserializeObject(input.AsSpan().ToString(), type);
            }
            var invoker = Utf16Invokers.GetOrAdd(type, Utf16InvokerFactory);
            return invoker.FromByteArrayDeserializer(input);
        }

        /// <summary>Deserialize from string.</summary>
        /// <param name="input">Input</param>
        /// <param name="type">Object Type</param>
        /// <returns>Deserialized object</returns>
        public object Deserialize(in ArraySegment<char> input, Type type)
        {
            if (null == type) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

            if (IsPolymorphically(type))
            {
                return DeserializerPool.DeserializeObject(input.AsSpan().ToString(), type);
            }
            var invoker = Utf16Invokers.GetOrAdd(type, Utf16InvokerFactory);
            return invoker.FromBufferDeserializer(input);
        }

        /// <summary>Deserialize from string.</summary>
        /// <param name="input">Input</param>
        /// <param name="type">Object Type</param>
        /// <returns>Deserialized object</returns>
        public object Deserialize(in ReadOnlyMemory<char> input, Type type)
        {
            if (null == type) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

            if (IsPolymorphically(type))
            {
                return DeserializerPool.DeserializeObject(input.ToString(), type);
            }
            var invoker = Utf16Invokers.GetOrAdd(type, Utf16InvokerFactory);
            return invoker.FromMemoryDeserializer(input);
        }

        /// <summary>Deserialize from string.</summary>
        /// <param name="input">Input</param>
        /// <param name="type">Object Type</param>
        /// <returns>Deserialized object</returns>
        public object Deserialize(in ReadOnlySpan<char> input, Type type)
        {
            if (null == type) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

            if (IsPolymorphically(type))
            {
                return DeserializerPool.DeserializeObject(input.ToString(), type);
            }
            var invoker = Utf16Invokers.GetOrAdd(type, Utf16InvokerFactory);
            return invoker.Deserializer(input);
        }

        /// <summary>Deserialize from TextReader.</summary>
        /// <param name="reader">TextReader</param>
        /// <param name="type">Object Type</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>Task</returns>
        public ValueTask<object> DeserializeAsync(TextReader reader, Type type, CancellationToken cancellationToken = default)
        {
            if (null == type) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

            if (IsPolymorphically(type))
            {
                var result = DeserializerPool.DeserializeFromReader(reader, type);
                return new ValueTask<object>(result);
            }
            var invoker = Utf16Invokers.GetOrAdd(type, Utf16InvokerFactory);
            return invoker.FromTextReaderDeserializerAsync(reader, cancellationToken);
        }

        #endregion

        #region -- Utf8 Serialize --

        /// <summary>Serialize to byte array.</summary>
        /// <param name="input">Input</param>
        /// <returns>Byte array</returns>
        public byte[] SerializeObjectToUtf8Bytes(object input)
        {
            if (input == null) { return JsonSerializer.Generic.Utf8.Serialize<object>(null); }

            var inputType = input.GetType();
            if (IsPolymorphically(inputType))
            {
                return SerializerPool.SerializeToByteArray(input, inputType);
            }
            var invoker = Utf8Invokers.GetOrAdd(inputType, Utf8InvokerFactory);
            return invoker.ToByteArraySerializer(input);
        }

        /// <summary>Serialize to byte array from ArrayPool.
        /// The returned ArraySegment's Array needs to be returned to the ArrayPool.</summary>
        /// <param name="input">Input</param>
        /// <returns>Byte array from ArrayPool</returns>
        public ArraySegment<byte> SerializeObjectToUtf8ArrayPool(object input)
        {
            if (input == null) { return JsonSerializer.Generic.Utf8.SerializeToArrayPool<object>(null); }

            var inputType = input.GetType();
            if (IsPolymorphically(inputType))
            {
                return SerializerPool.SerializeToMemoryPool(input, inputType);
            }
            var invoker = Utf8Invokers.GetOrAdd(inputType, Utf8InvokerFactory);
            return invoker.ToByteArrayPoolSerializer(input);
        }

        /// <summary>Serialize to stream.</summary>
        /// <param name="input">Input</param>
        /// <param name="stream">Stream</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>Task</returns>
        public ValueTask SerializeObjectAsync(object input, Stream stream, CancellationToken cancellationToken = default)
        {
            if (input == null) { return JsonSerializer.Generic.Utf8.SerializeAsync<object>(null, stream, cancellationToken); }

            var inputType = input.GetType();
            if (IsPolymorphically(inputType))
            {
                SerializerPool.SerializeToStream(stream, input, inputType);
                return default;
            }
            var invoker = Utf8Invokers.GetOrAdd(inputType, Utf8InvokerFactory);
            return invoker.ToStreamSerializerAsync(input, stream, cancellationToken);
        }
        #endregion

        #region -- Utf8 Deserialize --

        /// <summary>Deserialize from Byte array.</summary>
        /// <param name="input">Input</param>
        /// <param name="type">Object Type</param>
        /// <returns>Deserialized object</returns>
        public object Deserialize(byte[] input, Type type)
        {
            if (null == type) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

            if (IsPolymorphically(type))
            {
                return DeserializerPool.DeserializeFromByteArray(input, type);
            }
            var invoker = Utf8Invokers.GetOrAdd(type, Utf8InvokerFactory);
            return invoker.FromByteArrayDeserializer(input);
        }

        /// <summary>Deserialize from Byte array.</summary>
        /// <param name="input">Input</param>
        /// <param name="type">Object Type</param>
        /// <returns>Deserialized object</returns>
        public object Deserialize(in ArraySegment<byte> input, Type type)
        {
            if (null == type) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

            if (IsPolymorphically(type))
            {
                return DeserializerPool.DeserializeFromByteArray(input.Array, input.Offset, input.Count, type);
            }
            var invoker = Utf8Invokers.GetOrAdd(type, Utf8InvokerFactory);
            return invoker.FromBufferDeserializer(input);
        }

        /// <summary>Deserialize from Byte array.</summary>
        /// <param name="input">Input</param>
        /// <param name="type">Object Type</param>
        /// <returns>Deserialized object</returns>
        public object Deserialize(in ReadOnlyMemory<byte> input, Type type)
        {
            if (null == type) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

            if (IsPolymorphically(type))
            {
                if (MemoryMarshal.TryGetArray(input, out ArraySegment<byte> segment))
                {
                    return DeserializerPool.DeserializeFromByteArray(segment.Array, segment.Offset, segment.Count, type);
                }
                else
                {
                    return DeserializerPool.DeserializeFromByteArray(input.ToArray(), type);
                }
            }
            var invoker = Utf8Invokers.GetOrAdd(type, Utf8InvokerFactory);
            return invoker.FromMemoryDeserializer(input);
        }

        /// <summary>Deserialize from Byte array.</summary>
        /// <param name="input">Input</param>
        /// <param name="type">Object Type</param>
        /// <returns>Deserialized object</returns>
        public object Deserialize(in ReadOnlySpan<byte> input, Type type)
        {
            if (null == type) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

            if (IsPolymorphically(type))
            {
                return DeserializerPool.DeserializeFromByteArray(input.ToArray(), type);
            }
            var invoker = Utf8Invokers.GetOrAdd(type, Utf8InvokerFactory);
            return invoker.Deserializer(input);
        }

        /// <summary>Deserialize from stream.</summary>
        /// <param name="stream">Stream</param>
        /// <param name="type">Object Type</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>Task</returns>
        public ValueTask<object> DeserializeAsync(Stream stream, Type type, CancellationToken cancellationToken = default)
        {
            if (null == type) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.type); }

            if (IsPolymorphically(type))
            {
                var result = DeserializerPool.DeserializeFromStream(stream, type);
                return new ValueTask<object>(result);
            }
            var invoker = Utf8Invokers.GetOrAdd(type, Utf8InvokerFactory);
            return invoker.FromStreamDeserializerAsync(stream, cancellationToken);
        }

        #endregion
    }
}
