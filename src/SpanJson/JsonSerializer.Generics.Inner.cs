using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SpanJson.Helpers;
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
                    if (null == input) { return default; }

                    _lastDeserializationSizeEstimate = input.Length;
                    var jsonReader = new JsonReader<TSymbol>(input);
                    return Formatter.Deserialize(ref jsonReader, Resolver);
                }

                public static T InnerDeserialize(in ArraySegment<TSymbol> input)
                {
                    if (input.IsEmpty()) { return default; }

                    _lastDeserializationSizeEstimate = input.Count;
                    var jsonReader = new JsonReader<TSymbol>(input);
                    return Formatter.Deserialize(ref jsonReader, Resolver);
                }

                public static T InnerDeserialize(in ReadOnlyMemory<TSymbol> input)
                {
                    if (input.IsEmpty) { return default; }

                    _lastDeserializationSizeEstimate = input.Length;
                    var jsonReader = new JsonReader<TSymbol>(input);
                    return Formatter.Deserialize(ref jsonReader, Resolver);
                }

                public static T InnerDeserialize(in ReadOnlySpan<TSymbol> input)
                {
                    if (input.IsEmpty) { return default; }

                    _lastDeserializationSizeEstimate = input.Length;
                    var jsonReader = new JsonReader<TSymbol>(input);
                    return Formatter.Deserialize(ref jsonReader, Resolver);
                }

                #endregion

                #region -- Utf16 Deserialize --

                public static ValueTask<T> InnerDeserializeAsync(TextReader reader, CancellationToken cancellationToken = default)
                {
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

                public static ValueTask<T> InnerDeserializeAsync(Stream stream, CancellationToken cancellationToken = default)
                {
#if !NET451
                    if (stream is MemoryStream ms && ms.TryGetBuffer(out var buffer))
                    {
                        return new ValueTask<T>(InnerDeserialize((ArraySegment<TSymbol>)(object)buffer));
                    }
#endif

                    var input = stream.CanSeek
                        ? ReadStreamFullAsync(stream, cancellationToken)
                        : ReadStreamAsync(stream, _lastDeserializationSizeEstimate, cancellationToken);
                    if (input.IsCompletedSuccessfully)
                    {
                        var memory = input.Result;
                        return new ValueTask<T>(Utf8InnerDeserialize(memory));
                    }

                    return AwaitDeserializeAsync(input);
                }

                private static async ValueTask<ArraySegment<byte>> ReadStreamFullAsync(Stream stream, CancellationToken cancellationToken = default)
                {
                    var buffer = ArrayPool<byte>.Shared.Rent((int)stream.Length);
                    var read = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
                    return new ArraySegment<byte>(buffer, 0, read);
                }

                private static T Utf8InnerDeserialize(in ArraySegment<byte> memory)
                {
                    var result = InnerDeserialize((ArraySegment<TSymbol>)(object)memory);

                    ArrayPool<byte>.Shared.Return(memory.Array);

                    return result;
                }

                private static async ValueTask<ArraySegment<byte>> ReadStreamAsync(Stream stream, int sizeHint, CancellationToken cancellationToken = default)
                {
                    var totalSize = 0;
                    var buffer = ArrayPool<byte>.Shared.Rent(sizeHint);
                    int read;
                    while ((read = await stream.ReadAsync(buffer, totalSize, buffer.Length - totalSize, cancellationToken).ConfigureAwait(false)) > 0)
                    {
                        if (totalSize + read == buffer.Length)
                        {
                            FormatterUtils.GrowArray(ref buffer);
                        }

                        totalSize += read;
                    }

                    return new ArraySegment<byte>(buffer, 0, totalSize);
                }

                private static async ValueTask<T> AwaitDeserializeAsync(ValueTask<ArraySegment<byte>> task)
                {
                    var input = await task.ConfigureAwait(false);
                    return Utf8InnerDeserialize(input);
                }

                #endregion

                // ReSharper disable StaticMemberInGenericType
                private static int _lastSerializationSizeEstimate = 256; // initial size, get's updated with each serialization

                private static int _lastDeserializationSizeEstimate = 256; // initial size, get's updated with each deserialization
                // ReSharper restore StaticMemberInGenericType
            }
        }
    }
}