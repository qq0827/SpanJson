using System.Text;
using System.Runtime.Serialization;
using SpanJson.Resolvers;
using Xunit;

namespace SpanJson.Tests
{
    public class UnicodeEscapeTests2
    {
        public class Person
        {
            /// <summary>
            /// People
            /// </summary>
            [DataMember(Name = "人\r\n才")]
            public string 人 { get; set; }
            /// <summary>
            /// Name
            /// </summary>
            [DataMember(Name = "名\r\n称")]
            public string 名称 { get; set; }
            /// <summary>
            /// Number
            /// </summary>
            [DataMember(Name = "数\r\n字")]
            public Numbers 数 { get; set; }
        }

        public enum Numbers
        {
            一,
            二,
            三,
        }

        [Fact]
        public void SerializeDeserializePersonUtf16()
        {
            var person = new Person
            {
                人 = "自",
                名称 = "男",
                数 = Numbers.三,
            };
            var serialized = JsonSerializer.Generic.Utf16.Serialize(person);
            Assert.Equal("{\"人\\r\\n才\":\"自\",\"名\\r\\n称\":\"男\",\"数\\r\\n字\":\"三\"}", serialized);
            var deserialized = JsonSerializer.Generic.Utf16.Deserialize<Person>(serialized);
            Assert.NotNull(deserialized);
            Assert.Equal(person.人, deserialized.人);
            Assert.Equal(person.名称, deserialized.名称);
            Assert.Equal(person.数, deserialized.数);
        }

        [Fact]
        public void SerializeDeserializePersonUtf8()
        {
            var person = new Person
            {
                人 = "然",
                名称 = "女",
                数 = Numbers.二,
            };
            var serialized = JsonSerializer.Generic.Utf8.Serialize(person);
            Assert.Equal("{\"人\\r\\n才\":\"然\",\"名\\r\\n称\":\"女\",\"数\\r\\n字\":\"二\"}", Encoding.UTF8.GetString(serialized));
            var deserialized = JsonSerializer.Generic.Utf8.Deserialize<Person>(serialized);
            Assert.NotNull(deserialized);
            Assert.Equal(person.人, deserialized.人);
            Assert.Equal(person.名称, deserialized.名称);
            Assert.Equal(person.数, deserialized.数);
        }

        public sealed class NonAsciiEscapeResolver<TSymbol> : ResolverBase<TSymbol, NonAsciiEscapeResolver<TSymbol>> where TSymbol : struct
        {
            public NonAsciiEscapeResolver()
                : base(new SpanJsonOptions { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii })
            {
            }
        }

        [Fact]
        public void SerializeDeserializePersonFullyEncodedUtf16()
        {
            var person = new Person
            {
                人 = "自",
                名称 = "男",
                数 = Numbers.三,
            };
            var serialized = JsonSerializer.Generic.Utf16.Serialize<Person, NonAsciiEscapeResolver<char>>(person);
            Assert.Equal("{\"\\u4eba\\r\\n\\u624d\":\"\\u81ea\",\"\\u540d\\r\\n\\u79f0\":\"\\u7537\",\"\\u6570\\r\\n\\u5b57\":\"三\"}", serialized);
            var deserialized = JsonSerializer.Generic.Utf16.Deserialize<Person, NonAsciiEscapeResolver<char>>(serialized);
            Assert.NotNull(deserialized);
            Assert.Equal(person.人, deserialized.人);
            Assert.Equal(person.名称, deserialized.名称);
            Assert.Equal(person.数, deserialized.数);
        }

        [Fact]
        public void SerializeDeserializePersonFullyEncodedUtf8()
        {
            var person = new Person
            {
                人 = "然",
                名称 = "女",
                数 = Numbers.二,
            };
            var serialized = JsonSerializer.Generic.Utf8.Serialize<Person, NonAsciiEscapeResolver<byte>>(person);
            var utf16Serialized = Encoding.UTF8.GetString(serialized);
            Assert.Equal("{\"\\u4eba\\r\\n\\u624d\":\"\\u7136\",\"\\u540d\\r\\n\\u79f0\":\"\\u5973\",\"\\u6570\\r\\n\\u5b57\":\"二\"}", utf16Serialized);
            var deserialized = JsonSerializer.Generic.Utf8.Deserialize<Person, NonAsciiEscapeResolver<byte>>(serialized);
            Assert.NotNull(deserialized);
            Assert.Equal(person.人, deserialized.人);
            Assert.Equal(person.名称, deserialized.名称);
            Assert.Equal(person.数, deserialized.数);
        }
    }
}
