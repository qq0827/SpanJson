using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using SpanJson.Resolvers;
using Xunit;

namespace SpanJson.Tests
{
    partial class DynamicTests
    {
        [Fact]
        public void DynamicObjectWithKnownMembersUtf16SnakeCase()
        {
            var list = new List<string> { "Hello", "World" };
            dynamic dynamicObject = new DynamicObjectWithKnownMembers2();
            dynamicObject.NumValue = 5;
            dynamicObject.GoodText = "Hello World";
            dynamicObject.JsonSupported = list;
            dynamicObject.DynamicValue = "Hello Universe";

            var serialized = JsonSerializer.Generic.Utf16.Serialize<DynamicObjectWithKnownMembers2, ExcludeNullsSnakeCaseResolver<char>>(dynamicObject);
            Assert.NotNull(serialized);
            Assert.Contains("\"good_text\":", serialized);
            Assert.Contains("\"num_value\":", serialized);
            Assert.Contains("\"json_supported\":", serialized);
            Assert.Contains("\"dynamic_value\":", serialized);
            var deserialized = JsonSerializer.Generic.Utf16.Deserialize<DynamicObjectWithKnownMembers2, ExcludeNullsSnakeCaseResolver<char>>(serialized);
            Assert.NotNull(deserialized);
            Assert.Equal(5, (int)deserialized.NumValue);
            var supported = (List<string>)deserialized.JsonSupported;
            Assert.NotEmpty(supported);
            Assert.Equal(list, supported);
            Assert.Equal("Hello World", (string)deserialized.good_text);
            Assert.Equal("Hello Universe", (string)deserialized.dynamic_value);
        }

        [Fact]
        public void DynamicObjectWithKnownMembersUtf8SnakeCase()
        {
            var list = new List<string> { "Hello", "World" };
            dynamic dynamicObject = new DynamicObjectWithKnownMembers2();
            dynamicObject.NumValue = 5;
            dynamicObject.GoodText = "Hello World";
            dynamicObject.JsonSupported = list;
            dynamicObject.DynamicValue = "Hello Universe";
            var serialized = JsonSerializer.Generic.Utf8.Serialize<DynamicObjectWithKnownMembers2, ExcludeNullsSnakeCaseResolver<byte>>(dynamicObject);
            Assert.NotNull(serialized);
            var serializedText = Encoding.UTF8.GetString(serialized);
            Assert.Contains("\"good_text\":", serializedText);
            Assert.Contains("\"num_value\":", serializedText);
            Assert.Contains("\"json_supported\":", serializedText);
            Assert.Contains("\"dynamic_value\":", serializedText);
            var deserialized = JsonSerializer.Generic.Utf8.Deserialize<DynamicObjectWithKnownMembers2, ExcludeNullsSnakeCaseResolver<byte>>(serialized);
            Assert.NotNull(deserialized);
            Assert.Equal(5, (int)deserialized.NumValue);
            var supported = (List<string>)deserialized.JsonSupported;
            Assert.NotEmpty(supported);
            Assert.Equal(list, supported);
            Assert.Equal("Hello World", (string)deserialized.good_text);
            Assert.Equal("Hello Universe", (string)deserialized.dynamic_value);
        }

        public class DynamicObjectWithKnownMembers2 : DynamicObject
        {
            private readonly Dictionary<string, object> _dictionary = new Dictionary<string, object>();

            public override IEnumerable<string> GetDynamicMemberNames()
            {
                return _dictionary.Keys;
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                if (_dictionary.TryGetValue(binder.Name, out result))
                {
                    return true;
                }

                return base.TryGetMember(binder, out result);
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                _dictionary[binder.Name] = value;
                return true;
            }

            public int NumValue { get; set; }

            public int ReadOnly { get; } = 8;

            public IList<string> JsonSupported { get; set; }

            public AbstractMember NotSupported { get; set; }
        }
    }
}