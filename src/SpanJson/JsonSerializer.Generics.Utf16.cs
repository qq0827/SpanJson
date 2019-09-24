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
        partial class Generic
        {
            /// <summary>Serialize/Deserialize to/from string et al.</summary>
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
                    return Inner<T, char, ExcludeNullsOriginalCaseResolver<char>>.InnerSerializeToString(input);
                }

                /// <summary>Serialize to string with specific resolver.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="input">Input</param>
                /// <returns>String</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static string Serialize<T, TResolver>(T input)
                    where TResolver : IJsonFormatterResolver<char, TResolver>, new()
                {
                    return Inner<T, char, TResolver>.InnerSerializeToString(input);
                }

                /// <summary>Serialize to string.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <param name="input">Input</param>
                /// <returns>String</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static char[] SerializeToCharArray<T>(T input)
                {
                    return Inner<T, char, ExcludeNullsOriginalCaseResolver<char>>.InnerSerializeToCharArray(input);
                }

                /// <summary>Serialize to string with specific resolver.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="input">Input</param>
                /// <returns>String</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static char[] SerializeToCharArray<T, TResolver>(T input)
                    where TResolver : IJsonFormatterResolver<char, TResolver>, new()
                {
                    return Inner<T, char, TResolver>.InnerSerializeToCharArray(input);
                }

                /// <summary>Serialize to char buffer from ArrayPool
                /// The returned ArraySegment's Array needs to be returned to the ArrayPool.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <param name="input">Input</param>
                /// <returns>Char array from ArrayPool</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static ArraySegment<char> SerializeToArrayPool<T>(T input)
                {
                    return Inner<T, char, ExcludeNullsOriginalCaseResolver<char>>.InnerSerializeToCharArrayPool(input);
                }


                /// <summary>Serialize to string with specific resolver.
                /// The returned ArraySegment's Array needs to be returned to the ArrayPool.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="input">Input</param>
                /// <returns>String</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static ArraySegment<char> SerializeToArrayPool<T, TResolver>(T input)
                    where TResolver : IJsonFormatterResolver<char, TResolver>, new()
                {
                    return Inner<T, char, TResolver>.InnerSerializeToCharArrayPool(input);
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
                    return Inner<T, char, ExcludeNullsOriginalCaseResolver<char>>.InnerSerializeAsync(input, writer, cancellationToken);
                }

                /// <summary>Serialize to TextWriter with specific resolver.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="input">Input</param>
                /// <param name="writer">Writer</param>
                /// <param name="cancellationToken">CancellationToken</param>
                /// <returns>Task</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static ValueTask SerializeAsync<T, TResolver>(T input, TextWriter writer, CancellationToken cancellationToken = default)
                    where TResolver : IJsonFormatterResolver<char, TResolver>, new()
                {
                    return Inner<T, char, TResolver>.InnerSerializeAsync(input, writer, cancellationToken);
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
                    return Inner<T, char, ExcludeNullsOriginalCaseResolver<char>>.InnerDeserialize(input.AsSpan());
#else
                    return Inner<T, char, ExcludeNullsOriginalCaseResolver<char>>.InnerDeserialize(input);
#endif
                }

                /// <summary>Deserialize from string with specific resolver.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="input">Input</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static T Deserialize<T, TResolver>(string input)
                    where TResolver : IJsonFormatterResolver<char, TResolver>, new()
                {
#if NETSTANDARD2_0 || NET471 || NET451
                    return Inner<T, char, TResolver>.InnerDeserialize(input.AsSpan());
#else
                    return Inner<T, char, TResolver>.InnerDeserialize(input);
#endif
                }

                /// <summary>Deserialize from string.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <param name="input">Input</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static T Deserialize<T>(char[] input)
                {
                    return Inner<T, char, ExcludeNullsOriginalCaseResolver<char>>.InnerDeserialize(input);
                }

                /// <summary>Deserialize from string with specific resolver.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="input">Input</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static T Deserialize<T, TResolver>(char[] input)
                    where TResolver : IJsonFormatterResolver<char, TResolver>, new()
                {
                    return Inner<T, char, TResolver>.InnerDeserialize(input);
                }

                /// <summary>Deserialize from string.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <param name="input">Input</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static T Deserialize<T>(in ArraySegment<char> input)
                {
                    return Inner<T, char, ExcludeNullsOriginalCaseResolver<char>>.InnerDeserialize(input);
                }

                /// <summary>Deserialize from string with specific resolver.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="input">Input</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static T Deserialize<T, TResolver>(in ArraySegment<char> input)
                    where TResolver : IJsonFormatterResolver<char, TResolver>, new()
                {
                    return Inner<T, char, TResolver>.InnerDeserialize(input);
                }

                /// <summary>Deserialize from string.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <param name="input">Input</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static T Deserialize<T>(in ReadOnlyMemory<char> input)
                {
                    return Inner<T, char, ExcludeNullsOriginalCaseResolver<char>>.InnerDeserialize(input);
                }

                /// <summary>Deserialize from string with specific resolver.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="input">Input</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static T Deserialize<T, TResolver>(in ReadOnlyMemory<char> input)
                    where TResolver : IJsonFormatterResolver<char, TResolver>, new()
                {
                    return Inner<T, char, TResolver>.InnerDeserialize(input);
                }

                /// <summary>Deserialize from string.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <param name="input">Input</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static T Deserialize<T>(in ReadOnlySpan<char> input)
                {
                    return Inner<T, char, ExcludeNullsOriginalCaseResolver<char>>.InnerDeserialize(input);
                }

                /// <summary>Deserialize from string with specific resolver.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="input">Input</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static T Deserialize<T, TResolver>(in ReadOnlySpan<char> input)
                    where TResolver : IJsonFormatterResolver<char, TResolver>, new()
                {
                    return Inner<T, char, TResolver>.InnerDeserialize(input);
                }

                /// <summary>Deserialize from TextReader.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <param name="reader">TextReader</param>
                /// <param name="cancellationToken">CancellationToken</param>
                /// <returns>Deserialized object</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static ValueTask<T> DeserializeAsync<T>(TextReader reader, CancellationToken cancellationToken = default)
                {
                    return Inner<T, char, ExcludeNullsOriginalCaseResolver<char>>.InnerDeserializeAsync(reader, cancellationToken);
                }

                /// <summary>Deserialize from TextReader with specific resolver.</summary>
                /// <typeparam name="T">Type</typeparam>
                /// <typeparam name="TResolver">Resolver</typeparam>
                /// <param name="reader">TextReader</param>
                /// <param name="cancellationToken">CancellationToken</param>
                /// <returns>Task</returns>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static ValueTask<T> DeserializeAsync<T, TResolver>(TextReader reader, CancellationToken cancellationToken = default)
                    where TResolver : IJsonFormatterResolver<char, TResolver>, new()
                {
                    return Inner<T, char, TResolver>.InnerDeserializeAsync(reader, cancellationToken);
                }

                #endregion
            }
        }
    }
}