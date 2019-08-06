using System;
using System.Linq;
using Xunit;

namespace SpanJson.Tests.Generated
{
    public partial class CharArrayTests
    {
        [Fact]
        public void SerializeDeserializeNullCharUtf16()
        {
            var chars = new char[5];
#if DESKTOPCLR
            for (var idx = 0; idx < chars.Length; idx++)
            {
                chars[idx] = '\0';
            }
#else
            Array.Fill(chars, '\0');
#endif
            var serialized = JsonSerializer.Generic.Utf16.Serialize(chars);
            Assert.NotNull(serialized);
            var deserialized = JsonSerializer.Generic.Utf16.Deserialize<char[]>(serialized);
            Assert.NotNull(deserialized);
            Assert.Equal(chars, deserialized);
            deserialized = JsonSerializer.Generic.Utf16.Deserialize<char[]>(new ArraySegment<char>(serialized.ToCharArray()));
            Assert.NotNull(deserialized);
            Assert.Equal(chars, deserialized);
            deserialized = JsonSerializer.Generic.Utf16.Deserialize<char[]>(serialized.ToCharArray());
            Assert.NotNull(deserialized);
            Assert.Equal(chars, deserialized);
            deserialized = JsonSerializer.Generic.Utf16.Deserialize<char[]>(serialized.AsMemory());
            Assert.NotNull(deserialized);
            Assert.Equal(chars, deserialized);
            deserialized = JsonSerializer.Generic.Utf16.Deserialize<char[]>(serialized.AsSpan());
            Assert.NotNull(deserialized);
            Assert.Equal(chars, deserialized);
        }

        [Fact]
        public void SerializeDeserializeNullCharUtf8()
        {
            var chars = new char[5];
#if DESKTOPCLR
            for (var idx = 0; idx < chars.Length; idx++)
            {
                chars[idx] = '\0';
            }
#else
            Array.Fill(chars, '\0');
#endif
            var serialized = JsonSerializer.Generic.Utf8.Serialize(chars);
            Assert.NotNull(serialized);
            var deserialized = JsonSerializer.Generic.Utf8.Deserialize<char[]>(serialized);
            Assert.NotNull(deserialized);
            Assert.Equal(chars, deserialized);
            deserialized = JsonSerializer.Generic.Utf8.Deserialize<char[]>(new ArraySegment<byte>(serialized));
            Assert.NotNull(deserialized);
            Assert.Equal(chars, deserialized);
            deserialized = JsonSerializer.Generic.Utf8.Deserialize<char[]>(serialized.AsMemory());
            Assert.NotNull(deserialized);
            Assert.Equal(chars, deserialized);
            deserialized = JsonSerializer.Generic.Utf8.Deserialize<char[]>(serialized.AsSpan());
            Assert.NotNull(deserialized);
            Assert.Equal(chars, deserialized);
        }

        [Fact]
        public void SerializeDeserializeAllAsciiUtf8()
        {
            var chars = Enumerable.Range(0, 0x80).Select(a => (char)a).ToArray();
            var serialized = JsonSerializer.Generic.Utf8.Serialize(chars);
            Assert.NotNull(serialized);
            var deserialized = JsonSerializer.Generic.Utf8.Deserialize<char[]>(serialized);
            Assert.NotNull(deserialized);
            Assert.Equal(chars, deserialized);
            deserialized = JsonSerializer.Generic.Utf8.Deserialize<char[]>(new ArraySegment<byte>(serialized));
            Assert.NotNull(deserialized);
            Assert.Equal(chars, deserialized);
            deserialized = JsonSerializer.Generic.Utf8.Deserialize<char[]>(serialized.AsSpan());
            Assert.NotNull(deserialized);
            Assert.Equal(chars, deserialized);
            deserialized = JsonSerializer.Generic.Utf8.Deserialize<char[]>(serialized.AsMemory());
            Assert.NotNull(deserialized);
            Assert.Equal(chars, deserialized);
        }

        [Fact]
        public void SerializeDeserializeAllAsciiUtf16()
        {
            var chars = Enumerable.Range(0, 0x80).Select(a => (char)a).ToArray();
            var serialized = JsonSerializer.Generic.Utf16.Serialize(chars);
            Assert.NotNull(serialized);
            var deserialized = JsonSerializer.Generic.Utf16.Deserialize<char[]>(serialized);
            Assert.NotNull(deserialized);
            Assert.Equal(chars, deserialized);
            deserialized = JsonSerializer.Generic.Utf16.Deserialize<char[]>(new ArraySegment<char>(serialized.ToCharArray()));
            Assert.NotNull(deserialized);
            Assert.Equal(chars, deserialized);
            deserialized = JsonSerializer.Generic.Utf16.Deserialize<char[]>(serialized.ToCharArray());
            Assert.NotNull(deserialized);
            Assert.Equal(chars, deserialized);
            deserialized = JsonSerializer.Generic.Utf16.Deserialize<char[]>(serialized.AsMemory());
            Assert.NotNull(deserialized);
            Assert.Equal(chars, deserialized);
            deserialized = JsonSerializer.Generic.Utf16.Deserialize<char[]>(serialized.AsSpan());
            Assert.NotNull(deserialized);
            Assert.Equal(chars, deserialized);
        }
    }
}