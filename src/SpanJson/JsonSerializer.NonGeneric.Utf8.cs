using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SpanJson.Resolvers;

namespace SpanJson
{
    partial class JsonSerializer
    {
        partial class NonGeneric
        {
            /// <summary>Serialize/Deserialize to/from byte array et al.</summary>
            public static class Utf8
            {
                #region -- Serialize --

                /// <summary>Serialize to byte array.</summary>
                /// <param name="input">Input</param>
                /// <returns>Byte array</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static byte[] Serialize(object input)
                {
                    return Inner<byte, ExcludeNullsOriginalCaseResolver<byte>>.InnerSerializeToByteArray(input);
                }

                /// <summary>Serialize to byte array with specific resolver.</summary>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="input">Input</param>
                /// <returns>Byte array</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static byte[] Serialize<TResolver>(object input) where TResolver : IJsonFormatterResolver<byte, TResolver>, new()
                {
                    return Inner<byte, TResolver>.InnerSerializeToByteArray(input);
                }

                /// <summary>Serialize to byte array from ArrayPool.
                /// The returned ArraySegment's Array needs to be returned to the ArrayPool.</summary>
                /// <param name="input">Input</param>
                /// <returns>Byte array from ArrayPool</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static ArraySegment<byte> SerializeToArrayPool(object input)
                {
                    return Inner<byte, ExcludeNullsOriginalCaseResolver<byte>>.InnerSerializeToByteArrayPool(input);
                }

                /// <summary>Serialize to byte array from ArrayPool with specific resolver.
                /// The returned ArraySegment's Array needs to be returned to the ArrayPool.</summary>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="input">Input</param>
                /// <returns>Byte array</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static ArraySegment<byte> SerializeToArrayPool<TResolver>(object input) where TResolver : IJsonFormatterResolver<byte, TResolver>, new()
                {
                    return Inner<byte, TResolver>.InnerSerializeToByteArrayPool(input);
                }

                /// <summary>Serialize to stream.</summary>
                /// <param name="input">Input</param>
                /// <param name="stream">Stream</param>
                /// <param name="cancellationToken">CancellationToken</param>
                /// <returns>Task</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static ValueTask SerializeAsync(object input, Stream stream, CancellationToken cancellationToken = default)
                {
                    return Inner<byte, ExcludeNullsOriginalCaseResolver<byte>>.InnerSerializeAsync(input, stream, cancellationToken);
                }

                /// <summary>Serialize to stream with specific resolver.</summary>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="input">Input</param>
                /// <param name="stream">Stream</param>
                /// <param name="cancellationToken">CancellationToken</param>
                /// <returns>Task</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static ValueTask SerializeAsync<TResolver>(object input, Stream stream, CancellationToken cancellationToken = default)
                    where TResolver : IJsonFormatterResolver<byte, TResolver>, new()
                {
                    return Inner<byte, TResolver>.InnerSerializeAsync(input, stream, cancellationToken);
                }

                #endregion

                #region -- Deserialize --

                /// <summary>Deserialize from Byte array.</summary>
                /// <param name="input">Input</param>
                /// <param name="type">Object Type</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static object Deserialize(byte[] input, Type type)
                {
                    return Inner<byte, ExcludeNullsOriginalCaseResolver<byte>>.InnerDeserialize(input, type);
                }

                /// <summary>Deserialize from Byte array with specific resolver.</summary>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="input">Input</param>
                /// <param name="type">Object Type</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static object Deserialize<TResolver>(byte[] input, Type type)
                    where TResolver : IJsonFormatterResolver<byte, TResolver>, new()
                {
                    return Inner<byte, TResolver>.InnerDeserialize(input, type);
                }

                /// <summary>Deserialize from Byte array.</summary>
                /// <param name="input">Input</param>
                /// <param name="type">Object Type</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static object Deserialize(in ArraySegment<byte> input, Type type)
                {
                    return Inner<byte, ExcludeNullsOriginalCaseResolver<byte>>.InnerDeserialize(input, type);
                }

                /// <summary>Deserialize from Byte array with specific resolver.</summary>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="input">Input</param>
                /// <param name="type">Object Type</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static object Deserialize<TResolver>(in ArraySegment<byte> input, Type type)
                    where TResolver : IJsonFormatterResolver<byte, TResolver>, new()
                {
                    return Inner<byte, TResolver>.InnerDeserialize(input, type);
                }

                /// <summary>Deserialize from Byte array.</summary>
                /// <param name="input">Input</param>
                /// <param name="type">Object Type</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static object Deserialize(in ReadOnlyMemory<byte> input, Type type)
                {
                    return Inner<byte, ExcludeNullsOriginalCaseResolver<byte>>.InnerDeserialize(input, type);
                }

                /// <summary>Deserialize from Byte array with specific resolver.</summary>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="input">Input</param>
                /// <param name="type">Object Type</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static object Deserialize<TResolver>(in ReadOnlyMemory<byte> input, Type type)
                    where TResolver : IJsonFormatterResolver<byte, TResolver>, new()
                {
                    return Inner<byte, TResolver>.InnerDeserialize(input, type);
                }

                /// <summary>Deserialize from Byte array.</summary>
                /// <param name="input">Input</param>
                /// <param name="type">Object Type</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static object Deserialize(in ReadOnlySpan<byte> input, Type type)
                {
                    return Inner<byte, ExcludeNullsOriginalCaseResolver<byte>>.InnerDeserialize(input, type);
                }

                /// <summary>Deserialize from Byte array with specific resolver.</summary>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="input">Input</param>
                /// <param name="type">Object Type</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static object Deserialize<TResolver>(in ReadOnlySpan<byte> input, Type type)
                    where TResolver : IJsonFormatterResolver<byte, TResolver>, new()
                {
                    return Inner<byte, TResolver>.InnerDeserialize(input, type);
                }

                /// <summary>Deserialize from stream.</summary>
                /// <param name="stream">Stream</param>
                /// <param name="type">Object Type</param>
                /// <param name="cancellationToken">CancellationToken</param>
                /// <returns>Task</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static ValueTask<object> DeserializeAsync(Stream stream, Type type,
                    CancellationToken cancellationToken = default)
                {
                    return Inner<byte, ExcludeNullsOriginalCaseResolver<byte>>.InnerDeserializeAsync(stream, type, cancellationToken);
                }

                /// <summary>Deserialize from stream with specific resolver.</summary>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="stream">Stream</param>
                /// <param name="type">Object Type</param>
                /// <param name="cancellationToken">CancellationToken</param>
                /// <returns>Task</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static ValueTask<object> DeserializeAsync<TResolver>(Stream stream, Type type,
                    CancellationToken cancellationToken = default)
                    where TResolver : IJsonFormatterResolver<byte, TResolver>, new()
                {
                    return Inner<byte, TResolver>.InnerDeserializeAsync(stream, type, cancellationToken);
                }

                #endregion

                /// <summary>This is necessary to convert ValueTask of T to ValueTask of object</summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal static ValueTask<object> GenericStreamObjectWrapper<T, TResolver>(Stream stream, CancellationToken cancellationToken = default)
                    where TResolver : IJsonFormatterResolver<byte, TResolver>, new()
                {
                    var task = Generic.Utf8.DeserializeAsync<T, TResolver>(stream, cancellationToken);
                    if (task.IsCompletedSuccessfully)
                    {
                        return new ValueTask<object>(task.Result);
                    }

                    return AwaitGenericStreamObjectWrapper(task);
                }

                private static async ValueTask<object> AwaitGenericStreamObjectWrapper<T>(ValueTask<T> valueTask)
                {
                    return await valueTask.ConfigureAwait(false);
                }
            }
        }
    }
}