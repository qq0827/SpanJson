using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SpanJson.Internal;
using SpanJson.Resolvers;

namespace SpanJson
{
    partial class JsonSerializer
    {
        partial class Generic
        {
            public static class Inner<T, TSymbol, TResolver> where TResolver : IJsonFormatterResolver<TSymbol, TResolver>, new() where TSymbol : struct
            {
                private static readonly IJsonFormatterResolver<TSymbol> Resolver;
                private static readonly IJsonFormatter<T, TSymbol> Formatter;

                static Inner()
                {
                    Resolver = StandardResolvers.GetResolver<TSymbol, TResolver>();
                    Formatter = Resolver.GetFormatter<T>();
                }

                #region -- Utf16 Serialize --

                public static string InnerSerializeToString(T input)
                {
                    var jsonWriter = new JsonWriter<TSymbol>(true);
                    Formatter.Serialize(ref jsonWriter, input, Resolver);
                    _lastSerializationSizeEstimate = jsonWriter.Data.Length;
                    var result = jsonWriter.ToString(); // includes Dispose
                    return result;
                }

                public static char[] InnerSerializeToCharArray(T input)
                {
                    var jsonWriter = new JsonWriter<TSymbol>(true);
                    Formatter.Serialize(ref jsonWriter, input, Resolver);
                    _lastSerializationSizeEstimate = jsonWriter.Data.Length;
                    return jsonWriter.ToCharArray(); // includes Dispose
                }

                public static ArraySegment<char> InnerSerializeToCharArrayPool(T input)
                {
                    var jsonWriter = new JsonWriter<TSymbol>(_lastSerializationSizeEstimate);
                    Formatter.Serialize(ref jsonWriter, input, Resolver);
                    var data = jsonWriter._utf16Buffer;
                    _lastSerializationSizeEstimate = data.Length;
                    return new ArraySegment<char>(data, 0, jsonWriter.Position);
                }

                public static ValueTask InnerSerializeAsync(T input, TextWriter writer, CancellationToken cancellationToken = default)
                {
                    if (writer is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.writer); }

                    var jsonWriter = new JsonWriter<TSymbol>(_lastSerializationSizeEstimate);
                    Formatter.Serialize(ref jsonWriter, input, Resolver);
                    var data = jsonWriter._utf16Buffer;
                    _lastSerializationSizeEstimate = data.Length;
                    var result = writer.WriteAsync(data, 0, jsonWriter.Position);
                    if (result.IsCompletedSuccessfully())
                    {
                        // This is a bit ugly, as we use the arraypool outside of the jsonwriter, but ref can't be use in async
                        ArrayPool<char>.Shared.Return(data);
                        return new ValueTask();
                    }

                    return AwaitSerializeAsync(result, data);
                }

                // This is a bit ugly, as we use the arraypool outside of the jsonwriter, but ref can't be use in async
                private static async ValueTask AwaitSerializeAsync(Task result, char[] data)
                {
                    await result.ConfigureAwait(false);
                    ArrayPool<char>.Shared.Return(data);
                }

                #endregion

                #region -- Utf8 Serialize --

                public static byte[] InnerSerializeToByteArray(T input)
                {
                    var jsonWriter = new JsonWriter<TSymbol>(true);
                    Formatter.Serialize(ref jsonWriter, input, Resolver);
                    _lastSerializationSizeEstimate = jsonWriter.Data.Length;
                    var result = jsonWriter.ToByteArray();
                    return result;
                }

                public static ArraySegment<byte> InnerSerializeToByteArrayPool(T input)
                {
                    var jsonWriter = new JsonWriter<TSymbol>(_lastSerializationSizeEstimate);
                    Formatter.Serialize(ref jsonWriter, input, Resolver);
                    var data = jsonWriter._utf8Buffer;
                    _lastSerializationSizeEstimate = data.Length;
                    return new ArraySegment<byte>(data, 0, jsonWriter.Position);
                }

                public static ValueTask InnerSerializeAsync(T input, Stream stream, CancellationToken cancellationToken = default)
                {
                    if (stream is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.stream); }

                    var jsonWriter = new JsonWriter<TSymbol>(_lastSerializationSizeEstimate);
                    Formatter.Serialize(ref jsonWriter, input, Resolver);
                    var data = jsonWriter._utf8Buffer;
                    _lastSerializationSizeEstimate = data.Length;
                    var result = stream.WriteAsync(data, 0, jsonWriter.Position, cancellationToken);
                    if (result.IsCompletedSuccessfully())
                    {
                        // This is a bit ugly, as we use the arraypool outside of the jsonwriter, but ref can't be use in async
                        ArrayPool<byte>.Shared.Return(data);
                        return new ValueTask();
                    }

                    return AwaitSerializeAsync(result, data);
                }

                // This is a bit ugly, as we use the arraypool outside of the jsonwriter, but ref can't be use in async
                private static async ValueTask AwaitSerializeAsync(Task result, byte[] data)
                {
                    await result.ConfigureAwait(false);
                    ArrayPool<byte>.Shared.Return(data);
                }

                #endregion

                #region -- Deserialize --

                public static T InnerDeserialize(TSymbol[] input)
                {
                    if (input is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.input); }

                    var jsonReader = new JsonReader<TSymbol>(input);
                    return Formatter.Deserialize(ref jsonReader, Resolver);
                }

                public static T InnerDeserialize(in ArraySegment<TSymbol> input)
                {
                    var jsonReader = new JsonReader<TSymbol>(input);
                    return Formatter.Deserialize(ref jsonReader, Resolver);
                }

                public static T InnerDeserialize(in ReadOnlyMemory<TSymbol> input)
                {
                    var jsonReader = new JsonReader<TSymbol>(input);
                    return Formatter.Deserialize(ref jsonReader, Resolver);
                }

                public static T InnerDeserialize(in ReadOnlySpan<TSymbol> input)
                {
                    var jsonReader = new JsonReader<TSymbol>(input);
                    return Formatter.Deserialize(ref jsonReader, Resolver);
                }

                #endregion

                #region -- Utf16 Deserialize --

                public static ValueTask<T> InnerDeserializeAsync(TextReader reader, CancellationToken cancellationToken = default)
                {
                    if (reader is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.reader); }

                    var input = reader.ReadToEndAsync();
                    if (input.IsCompletedSuccessfully())
                    {
#if NETSTANDARD2_0 || NET471 || NET451
                        return new ValueTask<T>(InnerDeserialize(MemoryMarshal.Cast<char, TSymbol>(input.Result.AsSpan())));
#else
                        return new ValueTask<T>(InnerDeserialize(MemoryMarshal.Cast<char, TSymbol>(input.Result)));
#endif
                    }

                    return AwaitDeserializeAsync(input);
                }

                private static async ValueTask<T> AwaitDeserializeAsync(Task<string> task)
                {
                    var input = await task.ConfigureAwait(false);
#if NETSTANDARD2_0 || NET471 || NET451
                    return InnerDeserialize(MemoryMarshal.Cast<char, TSymbol>(input.AsSpan()));
#else
                    return InnerDeserialize(MemoryMarshal.Cast<char, TSymbol>(input));
#endif
                }

                #endregion

                #region -- Utf8 Deserialize --

                public static async ValueTask<T> InnerDeserializeAsync(Stream stream, CancellationToken cancellationToken = default)
                {
                    if (stream is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.stream); }

#if !NET451
                    if (stream is MemoryStream ms && ms.TryGetBuffer(out var buffer))
                    {
                        return InnerDeserialize((ArraySegment<TSymbol>)(object)buffer);
                    }
#endif

                    ArraySegment<byte> drained = await stream.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
                    try
                    {
                        return InnerDeserialize(((ArraySegment<TSymbol>)(object)drained).AsSpan()); // 这儿需要要使用 ReadOnlySpan<TSymbol>，确保不被 JsonReader 直接用来解析动态类型
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(drained.Array);
                    }
                }

                #endregion

                // ReSharper disable StaticMemberInGenericType
                private static int _lastSerializationSizeEstimate = 256; // initial size, get's updated with each serialization
            }
        }
    }
}