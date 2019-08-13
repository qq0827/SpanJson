using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SpanJson.Resolvers;
#if DEBUG
using Utf16OriginalCaseResolver = SpanJson.Resolvers.ExcludeNullsOriginalCaseResolver<char>;
#else
using Utf16OriginalCaseResolver = SpanJson.Resolvers.IncludeNullsOriginalCaseResolver<char>;
#endif

namespace SpanJson
{
    partial class JsonSerializer
    {
        partial class NonGeneric
        {
            /// <summary>Serialize/Deserialize to/from string et al.</summary>
            public static class Utf16
            {
                #region -- Serialize --

                /// <summary>Serialize to string.</summary>
                /// <param name="input">Input</param>
                /// <returns>String</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static string Serialize(object input)
                {
                    return Inner<char, Utf16OriginalCaseResolver>.InnerSerializeToString(input);
                }

                /// <summary>Serialize to string with specific resolver.</summary>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="input">Input</param>
                /// <returns>String</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static string Serialize<TResolver>(object input) where TResolver : IJsonFormatterResolver<char, TResolver>, new()
                {
                    return Inner<char, TResolver>.InnerSerializeToString(input);
                }

                /// <summary>Serialize to string.</summary>
                /// <param name="input">Input</param>
                /// <returns>String</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static char[] SerializeToCharArray(object input)
                {
                    return Inner<char, Utf16OriginalCaseResolver>.InnerSerializeToCharArray(input);
                }

                /// <summary>Serialize to string with specific resolver.</summary>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="input">Input</param>
                /// <returns>String</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static char[] SerializeToCharArray<TResolver>(object input) where TResolver : IJsonFormatterResolver<char, TResolver>, new()
                {
                    return Inner<char, TResolver>.InnerSerializeToCharArray(input);
                }

                /// <summary>Serialize to char buffer from ArrayPool.
                /// The returned ArraySegment's Array needs to be returned to the ArrayPool.</summary>
                /// <param name="input">Input</param>
                /// <returns>Char array from ArrayPool</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static ArraySegment<char> SerializeToArrayPool(object input)
                {
                    return Inner<char, Utf16OriginalCaseResolver>.InnerSerializeToCharArrayPool(input);
                }

                /// <summary>Serialize to char array from Array Pool with specific resolver.
                /// The returned ArraySegment's Array needs to be returned to the ArrayPool.</summary>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="input">Input</param>
                /// <returns>Char array from Array Pool</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static ArraySegment<char> SerializeToArrayPool<TResolver>(object input) where TResolver : IJsonFormatterResolver<char, TResolver>, new()
                {
                    return Inner<char, TResolver>.InnerSerializeToCharArrayPool(input);
                }

                /// <summary>Serialize to TextWriter.</summary>
                /// <param name="input">Input</param>
                /// <param name="writer">TextWriter</param>
                /// <param name="cancellationToken">CancellationToken</param>
                /// <returns>Task</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static ValueTask SerializeAsync(object input, TextWriter writer, CancellationToken cancellationToken = default)
                {
                    return Inner<char, Utf16OriginalCaseResolver>.InnerSerializeAsync(input, writer, cancellationToken);
                }

                /// <summary>Serialize to TextWriter with specific resolver.</summary>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="input">Input</param>
                /// <param name="writer">TextWriter</param>
                /// <param name="cancellationToken">CancellationToken</param>
                /// <returns>Task</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static ValueTask SerializeAsync<TResolver>(object input, TextWriter writer, CancellationToken cancellationToken = default)
                    where TResolver : IJsonFormatterResolver<char, TResolver>, new()
                {
                    return Inner<char, TResolver>.InnerSerializeAsync(input, writer, cancellationToken);
                }

                #endregion

                #region -- Deserialize --

                /// <summary>Deserialize from string.</summary>
                /// <param name="input">Input</param>
                /// <param name="type">Object Type</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static object Deserialize(string input, Type type)
                {
#if NETSTANDARD2_0 || NET471 || NET451
                    return Inner<char, Utf16OriginalCaseResolver>.InnerDeserialize(input.AsSpan(), type);
#else
                    return Inner<char, Utf16OriginalCaseResolver>.InnerDeserialize(input, type);
#endif
                }

                /// <summary>Deserialize from string with specific resolver.</summary>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="input">Input</param>
                /// <param name="type">Object Type</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static object Deserialize<TResolver>(string input, Type type)
                    where TResolver : IJsonFormatterResolver<char, TResolver>, new()
                {
#if NETSTANDARD2_0 || NET471 || NET451
                    return Inner<char, TResolver>.InnerDeserialize(input.AsSpan(), type);
#else
                    return Inner<char, TResolver>.InnerDeserialize(input, type);
#endif
                }

                /// <summary>Deserialize from string.</summary>
                /// <param name="input">Input</param>
                /// <param name="type">Object Type</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static object Deserialize(char[] input, Type type)
                {
                    return Inner<char, Utf16OriginalCaseResolver>.InnerDeserialize(input, type);
                }

                /// <summary>Deserialize from string with specific resolver.</summary>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="input">Input</param>
                /// <param name="type">Object Type</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static object Deserialize<TResolver>(char[] input, Type type)
                    where TResolver : IJsonFormatterResolver<char, TResolver>, new()
                {
                    return Inner<char, TResolver>.InnerDeserialize(input, type);
                }

                /// <summary>Deserialize from string.</summary>
                /// <param name="input">Input</param>
                /// <param name="type">Object Type</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static object Deserialize(in ArraySegment<char> input, Type type)
                {
                    return Inner<char, Utf16OriginalCaseResolver>.InnerDeserialize(input, type);
                }

                /// <summary>Deserialize from string with specific resolver.</summary>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="input">Input</param>
                /// <param name="type">Object Type</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static object Deserialize<TResolver>(in ArraySegment<char> input, Type type)
                    where TResolver : IJsonFormatterResolver<char, TResolver>, new()
                {
                    return Inner<char, TResolver>.InnerDeserialize(input, type);
                }

                /// <summary>Deserialize from string.</summary>
                /// <param name="input">Input</param>
                /// <param name="type">Object Type</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static object Deserialize(in ReadOnlyMemory<char> input, Type type)
                {
                    return Inner<char, Utf16OriginalCaseResolver>.InnerDeserialize(input, type);
                }

                /// <summary>Deserialize from string with specific resolver.</summary>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="input">Input</param>
                /// <param name="type">Object Type</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static object Deserialize<TResolver>(in ReadOnlyMemory<char> input, Type type)
                    where TResolver : IJsonFormatterResolver<char, TResolver>, new()
                {
                    return Inner<char, TResolver>.InnerDeserialize(input, type);
                }

                /// <summary>Deserialize from string.</summary>
                /// <param name="input">Input</param>
                /// <param name="type">Object Type</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static object Deserialize(in ReadOnlySpan<char> input, Type type)
                {
                    return Inner<char, Utf16OriginalCaseResolver>.InnerDeserialize(input, type);
                }

                /// <summary>Deserialize from string with specific resolver.</summary>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="input">Input</param>
                /// <param name="type">Object Type</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static object Deserialize<TResolver>(in ReadOnlySpan<char> input, Type type)
                    where TResolver : IJsonFormatterResolver<char, TResolver>, new()
                {
                    return Inner<char, TResolver>.InnerDeserialize(input, type);
                }

                /// <summary>Deserialize from TextReader.</summary>
                /// <param name="reader">TextReader</param>
                /// <param name="type">Object Type</param>
                /// <param name="cancellationToken">CancellationToken</param>
                /// <returns>Task</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static ValueTask<object> DeserializeAsync(TextReader reader, Type type,
                    CancellationToken cancellationToken = default)
                {
                    return Inner<char, Utf16OriginalCaseResolver>.InnerDeserializeAsync(reader, type, cancellationToken);
                }

                /// <summary>Deserialize from TextReader with specific resolver.</summary>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="reader">TextReader</param>
                /// <param name="type">Object Type</param>
                /// <param name="cancellationToken">CancellationToken</param>
                /// <returns>Task</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static ValueTask<object> DeserializeAsync<TResolver>(TextReader reader, Type type,
                    CancellationToken cancellationToken = default)
                    where TResolver : IJsonFormatterResolver<char, TResolver>, new()
                {
                    return Inner<char, TResolver>.InnerDeserializeAsync(reader, type, cancellationToken);
                }

                #endregion

                /// <summary>This is necessary to convert ValueTask of T to ValueTask of object</summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal static async ValueTask<object> GenericTextReaderObjectWrapper<T, TResolver>(TextReader reader,
                    CancellationToken cancellationToken = default) where TResolver : IJsonFormatterResolver<char, TResolver>, new()
                {
                    return await Generic.Utf16.DeserializeAsync<T, TResolver>(reader, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}