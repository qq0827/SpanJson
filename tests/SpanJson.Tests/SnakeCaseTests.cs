using System;
using System.Text;
using SpanJson.Resolvers;
using Xunit;

namespace SpanJson.Tests
{
    public class SnakeCaseTests
    {
        [Fact]
        public void SerializeDeserializeUtf16()
        {
            var input = new TestObject { SnakeCaseText = "Hello World"};
            var serialized = JsonSerializer.Generic.Utf16.Serialize<TestObject, ExcludeNullsSnakeCaseResolver<char>>(input);
            Assert.Contains("\"snake_case_text\":", serialized);
            var deserialized = JsonSerializer.Generic.Utf16.Deserialize<TestObject, ExcludeNullsSnakeCaseResolver<char>>(serialized);
            Assert.Equal(input, deserialized);
        }

        [Fact]
        public void SerializeDeserializeUtf8()
        {
            var input = new TestObject { SnakeCaseText = "Hello World"};
            var serialized = JsonSerializer.Generic.Utf8.Serialize<TestObject, ExcludeNullsSnakeCaseResolver<byte>>(input);
            Assert.Contains("\"snake_case_text\":", Encoding.UTF8.GetString(serialized));
            var deserialized = JsonSerializer.Generic.Utf8.Deserialize<TestObject, ExcludeNullsSnakeCaseResolver<byte>>(serialized);
            Assert.Equal(input, deserialized);
        }

        public class TestObject : IEquatable<TestObject>
        {
            public string SnakeCaseText { get; set; }

            public bool Equals(TestObject other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return string.Equals(SnakeCaseText, other.SnakeCaseText);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((TestObject) obj);
            }

            public override int GetHashCode()
            {
                // ReSharper disable once NonReadonlyMemberInGetHashCode
                return SnakeCaseText?.GetHashCode() ?? 0;
            }
        }
    }
}