using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NJsonSerializerSettings = Newtonsoft.Json.JsonSerializerSettings;

namespace SpanJson.Serialization
{
    public interface IJsonSerializer
    {
        NJsonSerializerSettings DefaultSerializerSettings { get; }
        NJsonSerializerSettings DefaultDeserializerSettings { get; }

        string Serialize<T>(T input);
        char[] SerializeToCharArray<T>(T input);
        ArraySegment<char> SerializeToArrayPool<T>(T input);
        ValueTask SerializeAsync<T>(T input, TextWriter writer, CancellationToken cancellationToken = default);

        T Deserialize<T>(string input);
        T Deserialize<T>(char[] input);
        T Deserialize<T>(in ArraySegment<char> input);
        T Deserialize<T>(in ReadOnlyMemory<char> input);
        T Deserialize<T>(in ReadOnlySpan<char> input);
        ValueTask<T> DeserializeAsync<T>(TextReader reader, CancellationToken cancellationToken = default);

        byte[] SerializeToUtf8Bytes<T>(T input);
        ArraySegment<byte> SerializeToUtf8ArrayPool<T>(T input);
        ValueTask SerializeAsync<T>(T input, Stream stream, CancellationToken cancellationToken = default);

        T Deserialize<T>(byte[] input);
        T Deserialize<T>(in ArraySegment<byte> input);
        T Deserialize<T>(in ReadOnlyMemory<byte> input);
        T Deserialize<T>(in ReadOnlySpan<byte> input);
        ValueTask<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default);

        string SerializeObject(object input);
        char[] SerializeObjectToCharArray(object input);
        ArraySegment<char> SerializeObjectToArrayPool(object input);
        ValueTask SerializeObjectAsync(object input, TextWriter writer, CancellationToken cancellationToken = default);

        object Deserialize(string input, Type type);
        object Deserialize(char[] input, Type type);
        object Deserialize(in ArraySegment<char> input, Type type);
        object Deserialize(in ReadOnlyMemory<char> input, Type type);
        object Deserialize(in ReadOnlySpan<char> input, Type type);
        ValueTask<object> DeserializeAsync(TextReader reader, Type type, CancellationToken cancellationToken = default);

        byte[] SerializeObjectToUtf8Bytes(object input);
        ArraySegment<byte> SerializeObjectToUtf8ArrayPool(object input);
        ValueTask SerializeObjectAsync(object input, Stream stream, CancellationToken cancellationToken = default);

        object Deserialize(byte[] input, Type type);
        object Deserialize(in ArraySegment<byte> input, Type type);
        object Deserialize(in ReadOnlyMemory<byte> input, Type type);
        object Deserialize(in ReadOnlySpan<byte> input, Type type);
        ValueTask<object> DeserializeAsync(Stream stream, Type type, CancellationToken cancellationToken = default);
    }
}
