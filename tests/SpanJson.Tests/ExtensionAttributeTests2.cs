using System.Collections.Generic;
using SpanJson.Resolvers;
using Xunit;

namespace SpanJson.Tests
{
    public partial class ExtensionAttributeTests
    {
        [Fact]
        public void SerializeDeserializeSnakeCase()
        {
            var dto = new ExtensionTestDTO { Key = "Hello", Value = "World", AdditionalValues = new Dictionary<string, object> { { "TestValue", 1.0m } } };
            var output = JsonSerializer.Generic.Utf16.Serialize<ExtensionTestDTO, ExcludeNullsSnakeCaseResolver<char>>(dto);
            Assert.Contains("key", output);
            Assert.Contains("test_value", output);
            var deserialized = JsonSerializer.Generic.Utf16.Deserialize<ExtensionTestDTO, ExcludeNullsSnakeCaseResolver<char>>(output);
            Assert.True(deserialized.AdditionalValues.ContainsKey("test_value"));
        }
    }
}