using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SpanJson.Resolvers;

namespace SpanJson
{
    /// <summary>Main Type for SpanJson Serializer</summary>
    public static partial class JsonCamelCaseSerializer
    {
        /// <summary>Generic part</summary>
        public static class Generic
        {
            public static class Utf16
            {
                #region -- Serialize --

                /// <summary>Serialize to string.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <param name="input">Input</param>
                /// <returns>String</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static string Serialize<T>(T input)
                {
                    return JsonSerializer.Generic.Inner<T, char, ExcludeNullsCamelCaseResolver<char>>.InnerSerializeToString(input);
                }

                /// <summary>Serialize to string.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <param name="input">Input</param>
                /// <returns>String</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static char[] SerializeToCharArray<T>(T input)
                {
                    return JsonSerializer.Generic.Inner<T, char, ExcludeNullsCamelCaseResolver<char>>.InnerSerializeToCharArray(input);
                }

                /// <summary>Serialize to char buffer from ArrayPool
                /// The returned ArraySegment's Array needs to be returned to the ArrayPool.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <param name="input">Input</param>
                /// <returns>Char array from ArrayPool</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static ArraySegment<char> SerializeToArrayPool<T>(T input)
                {
                    return JsonSerializer.Generic.Inner<T, char, ExcludeNullsCamelCaseResolver<char>>.InnerSerializeToCharArrayPool(input);
                }

                /// <summary>Serialize to TextWriter.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <param name="input">Input</param>
                /// <param name="writer">Writer</param>
                /// <param name="cancellationToken">CancellationToken</param>
                /// <returns>Task</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static ValueTask SerializeAsync<T>(T input, TextWriter writer, CancellationToken cancellationToken = default)
                {
                    return JsonSerializer.Generic.Inner<T, char, ExcludeNullsCamelCaseResolver<char>>.InnerSerializeAsync(input, writer, cancellationToken);
                }

                #endregion

                #region -- Deserialize --

                /// <summary>Deserialize from string.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <param name="input">Input</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static T Deserialize<T>(string input)
                {
#if NETSTANDARD2_0 || NET471 || NET451
                    return JsonSerializer.Generic.Inner<T, char, ExcludeNullsCamelCaseResolver<char>>.InnerDeserialize(input.AsSpan());
#else
                    return JsonSerializer.Generic.Inner<T, char, ExcludeNullsCamelCaseResolver<char>>.InnerDeserialize(input);
#endif
                }

                /// <summary>Deserialize from string.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <param name="input">Input</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static T Deserialize<T>(char[] input)
                {
                    return JsonSerializer.Generic.Inner<T, char, ExcludeNullsCamelCaseResolver<char>>.InnerDeserialize(input);
                }

                /// <summary>Deserialize from string.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <param name="input">Input</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static T Deserialize<T>(in ArraySegment<char> input)
                {
                    return JsonSerializer.Generic.Inner<T, char, ExcludeNullsCamelCaseResolver<char>>.InnerDeserialize(input);
                }

                /// <summary>Deserialize from string.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <param name="input">Input</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static T Deserialize<T>(in ReadOnlyMemory<char> input)
                {
                    return JsonSerializer.Generic.Inner<T, char, ExcludeNullsCamelCaseResolver<char>>.InnerDeserialize(input);
                }

                /// <summary>Deserialize from string.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <param name="input">Input</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static T Deserialize<T>(in ReadOnlySpan<char> input)
                {
                    return JsonSerializer.Generic.Inner<T, char, ExcludeNullsCamelCaseResolver<char>>.InnerDeserialize(input);
                }

                /// <summary>Deserialize from TextReader.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <param name="reader">TextReader</param>
                /// <param name="cancellationToken">CancellationToken</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static ValueTask<T> DeserializeAsync<T>(TextReader reader, CancellationToken cancellationToken = default)
                {
                    return JsonSerializer.Generic.Inner<T, char, ExcludeNullsCamelCaseResolver<char>>.InnerDeserializeAsync(reader, cancellationToken);
                }

                #endregion
            }

            public static class Utf8
            {
                #region -- Serialize --

                /// <summary>Serialize to byte array.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <param name="input">Input</param>
                /// <returns>Byte array</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static byte[] Serialize<T>(T input)
                {
                    return JsonSerializer.Generic.Inner<T, byte, ExcludeNullsCamelCaseResolver<byte>>.InnerSerializeToByteArray(input);
                }

                /// <summary>Serialize to byte array from ArrayPool.
                /// The returned ArraySegment's Array needs to be returned to the ArrayPool.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <param name="input">Input</param>
                /// <returns>Byte array from ArrayPool</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static ArraySegment<byte> SerializeToArrayPool<T>(T input)
                {
                    return JsonSerializer.Generic.Inner<T, byte, ExcludeNullsCamelCaseResolver<byte>>.InnerSerializeToByteArrayPool(input);
                }

                /// <summary>Serialize to stream.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <param name="input">Input</param>
                /// <param name="stream">Stream</param>
                /// <param name="cancellationToken">CancellationToken</param>
                /// <returns>Task</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static ValueTask SerializeAsync<T>(T input, Stream stream, CancellationToken cancellationToken = default)
                {
                    return JsonSerializer.Generic.Inner<T, byte, ExcludeNullsCamelCaseResolver<byte>>.InnerSerializeAsync(input, stream, cancellationToken);
                }

                #endregion

                #region -- Deserialize --

                /// <summary>Deserialize from byte array.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <param name="input">Input</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static T Deserialize<T>(byte[] input)
                {
                    return JsonSerializer.Generic.Inner<T, byte, ExcludeNullsCamelCaseResolver<byte>>.InnerDeserialize(input);
                }

                /// <summary>Deserialize from byte array.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <param name="input">Input</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static T Deserialize<T>(in ArraySegment<byte> input)
                {
                    return JsonSerializer.Generic.Inner<T, byte, ExcludeNullsCamelCaseResolver<byte>>.InnerDeserialize(input);
                }

                /// <summary>Deserialize from byte array.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <param name="input">Input</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static T Deserialize<T>(in ReadOnlyMemory<byte> input)
                {
                    return JsonSerializer.Generic.Inner<T, byte, ExcludeNullsCamelCaseResolver<byte>>.InnerDeserialize(input);
                }

                /// <summary>Deserialize from byte array.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <param name="input">Input</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static T Deserialize<T>(in ReadOnlySpan<byte> input)
                {
                    return JsonSerializer.Generic.Inner<T, byte, ExcludeNullsCamelCaseResolver<byte>>.InnerDeserialize(input);
                }

                /// <summary>Deserialize from stream.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <param name="stream">Stream</param>
                /// <param name="cancellationToken">CancellationToken</param>
                /// <returns>Task</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static ValueTask<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
                {
                    return JsonSerializer.Generic.Inner<T, byte, ExcludeNullsCamelCaseResolver<byte>>.InnerDeserializeAsync(stream, cancellationToken);
                }

                #endregion
            }
        }
    }
}
