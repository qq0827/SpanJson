using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace SpanJson.Serialization
{
    partial class JsonComplexSerializer<TResolver, TUtf8Resolver>
    {
        #region -- Utf16 Serialize --

        /// <summary>Serialize to string.</summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="input">Input</param>
        /// <returns>String</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Serialize<T>(T input)
        {
            if (IsPolymorphically<T>())
            {
                return SerializerPool.SerializeObject(input, input.GetType());
            }
            return JsonSerializer.Generic.Inner<T, char, TResolver>.InnerSerializeToString(input);
        }

        /// <summary>Serialize to string.</summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="input">Input</param>
        /// <returns>String</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public char[] SerializeToCharArray<T>(T input)
        {
            if (IsPolymorphically<T>())
            {
                return SerializerPool.SerializeObject(input, input.GetType()).ToCharArray();
            }
            return JsonSerializer.Generic.Inner<T, char, TResolver>.InnerSerializeToCharArray(input);
        }

        /// <summary>Serialize to char buffer from ArrayPool
        /// The returned ArraySegment's Array needs to be returned to the ArrayPool.</summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="input">Input</param>
        /// <returns>Char array from ArrayPool</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArraySegment<char> SerializeToArrayPool<T>(T input)
        {
            if (IsPolymorphically<T>())
            {
                ThrowHelper.ThrowNotSupportedException();
            }
            return JsonSerializer.Generic.Inner<T, char, TResolver>.InnerSerializeToCharArrayPool(input);
        }

        /// <summary>Serialize to TextWriter.</summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="input">Input</param>
        /// <param name="writer">Writer</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask SerializeAsync<T>(T input, TextWriter writer, CancellationToken cancellationToken = default)
        {
            if (IsPolymorphically<T>())
            {
                SerializerPool.SerializeToWriter(writer, input, input.GetType());
                return default;
            }
            return JsonSerializer.Generic.Inner<T, char, TResolver>.InnerSerializeAsync(input, writer, cancellationToken);
        }

        #endregion

        #region -- Utf16 Deserialize --

        /// <summary>Deserialize from string.</summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="input">Input</param>
        /// <returns>Deserialized object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Deserialize<T>(string input)
        {
            if (IsPolymorphically<T>())
            {
                return (T)DeserializerPool.DeserializeObject(input, typeof(T));
            }
#if NETSTANDARD2_0 || NET471 || NET451
            return JsonSerializer.Generic.Inner<T, char, TResolver>.InnerDeserialize(input.AsSpan());
#else
            return JsonSerializer.Generic.Inner<T, char, TResolver>.InnerDeserialize(input);
#endif
        }

        /// <summary>Deserialize from string.</summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="input">Input</param>
        /// <returns>Deserialized object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Deserialize<T>(char[] input)
        {
            if (IsPolymorphically<T>())
            {
                return (T)DeserializerPool.DeserializeObject(input.AsSpan().ToString(), typeof(T));
            }
            return JsonSerializer.Generic.Inner<T, char, TResolver>.InnerDeserialize(input);
        }

        /// <summary>Deserialize from string.</summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="input">Input</param>
        /// <returns>Deserialized object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Deserialize<T>(in ArraySegment<char> input)
        {
            if (IsPolymorphically<T>())
            {
                return (T)DeserializerPool.DeserializeObject(input.AsSpan().ToString(), typeof(T));
            }
            return JsonSerializer.Generic.Inner<T, char, TResolver>.InnerDeserialize(input);
        }

        /// <summary>Deserialize from string.</summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="input">Input</param>
        /// <returns>Deserialized object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Deserialize<T>(in ReadOnlyMemory<char> input)
        {
            if (IsPolymorphically<T>())
            {
                return (T)DeserializerPool.DeserializeObject(input.ToString(), typeof(T));
            }
            return JsonSerializer.Generic.Inner<T, char, TResolver>.InnerDeserialize(input);
        }

        /// <summary>Deserialize from string.</summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="input">Input</param>
        /// <returns>Deserialized object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Deserialize<T>(in ReadOnlySpan<char> input)
        {
            if (IsPolymorphically<T>())
            {
                return (T)DeserializerPool.DeserializeObject(input.ToString(), typeof(T));
            }
            return JsonSerializer.Generic.Inner<T, char, TResolver>.InnerDeserialize(input);
        }

        /// <summary>Deserialize from TextReader.</summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="reader">TextReader</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>Deserialized object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<T> DeserializeAsync<T>(TextReader reader, CancellationToken cancellationToken = default)
        {
            if (IsPolymorphically<T>())
            {
                var result = (T)DeserializerPool.DeserializeFromReader(reader, typeof(T));
                return new ValueTask<T>(result);
            }
            return JsonSerializer.Generic.Inner<T, char, TResolver>.InnerDeserializeAsync(reader, cancellationToken);
        }

        #endregion

        #region -- Utf8 Serialize --

        /// <summary>Serialize to byte array.</summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="input">Input</param>
        /// <returns>Byte array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] SerializeToUtf8Bytes<T>(T input)
        {
            if (IsPolymorphically<T>())
            {
                return SerializerPool.SerializeToByteArray(input);
            }
            return JsonSerializer.Generic.Inner<T, byte, TUtf8Resolver>.InnerSerializeToByteArray(input);
        }

        /// <summary>Serialize to byte array from ArrayPool.
        /// The returned ArraySegment's Array needs to be returned to the ArrayPool.</summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="input">Input</param>
        /// <returns>Byte array from ArrayPool</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArraySegment<byte> SerializeToUtf8ArrayPool<T>(T input)
        {
            if (IsPolymorphically<T>())
            {
                return SerializerPool.SerializeToMemoryPool(input);
            }
            return JsonSerializer.Generic.Inner<T, byte, TUtf8Resolver>.InnerSerializeToByteArrayPool(input);
        }

        /// <summary>Serialize to stream.</summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="input">Input</param>
        /// <param name="stream">Stream</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask SerializeAsync<T>(T input, Stream stream, CancellationToken cancellationToken = default)
        {
            if (IsPolymorphically<T>())
            {
                SerializerPool.SerializeToStream(stream, input);
                return default;
            }
            return JsonSerializer.Generic.Inner<T, byte, TUtf8Resolver>.InnerSerializeAsync(input, stream, cancellationToken);
        }

        #endregion

        #region -- Utf8 Deserialize --

        /// <summary>Deserialize from byte array.</summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="input">Input</param>
        /// <returns>Deserialized object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Deserialize<T>(byte[] input)
        {
            if (IsPolymorphically<T>())
            {
                return (T)DeserializerPool.DeserializeFromByteArray(input, typeof(T));
            }
            return JsonSerializer.Generic.Inner<T, byte, TUtf8Resolver>.InnerDeserialize(input);
        }

        /// <summary>Deserialize from byte array.</summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="input">Input</param>
        /// <returns>Deserialized object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Deserialize<T>(in ArraySegment<byte> input)
        {
            if (IsPolymorphically<T>())
            {
                return (T)DeserializerPool.DeserializeFromByteArray(input.Array, input.Offset, input.Count, typeof(T));
            }
            return JsonSerializer.Generic.Inner<T, byte, TUtf8Resolver>.InnerDeserialize(input);
        }

        /// <summary>Deserialize from byte array.</summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="input">Input</param>
        /// <returns>Deserialized object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Deserialize<T>(in ReadOnlyMemory<byte> input)
        {
            if (IsPolymorphically<T>())
            {
                if(MemoryMarshal.TryGetArray(input, out ArraySegment<byte> segment))
                {
                    return (T)DeserializerPool.DeserializeFromByteArray(segment.Array, segment.Offset, segment.Count, typeof(T));
                }
                else
                {
                    return (T)DeserializerPool.DeserializeFromByteArray(input.ToArray(), typeof(T));
                }
            }
            return JsonSerializer.Generic.Inner<T, byte, TUtf8Resolver>.InnerDeserialize(input);
        }

        /// <summary>Deserialize from byte array.</summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="input">Input</param>
        /// <returns>Deserialized object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Deserialize<T>(in ReadOnlySpan<byte> input)
        {
            if (IsPolymorphically<T>())
            {
                return (T)DeserializerPool.DeserializeFromByteArray(input.ToArray(), typeof(T));
            }
            return JsonSerializer.Generic.Inner<T, byte, TUtf8Resolver>.InnerDeserialize(input);
        }

        /// <summary>Deserialize from stream.</summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>Task</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
        {
            if (IsPolymorphically<T>())
            {
                var result = (T)DeserializerPool.DeserializeFromStream(stream, typeof(T));
                return new ValueTask<T>(result);
            }
            return JsonSerializer.Generic.Inner<T, byte, TUtf8Resolver>.InnerDeserializeAsync(stream, cancellationToken);
        }

        #endregion
    }
}
