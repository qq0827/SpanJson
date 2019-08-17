using System;
using System.Collections.Generic;
using CuteAnt;
using SpanJson.Linq;
using SpanJson.Resolvers;
using Xunit;

namespace SpanJson.Tests
{
    public class DynamicDeserializerTests
    {
        sealed class DynamicDeserializerResolver : ResolverBase<char, DynamicDeserializerResolver>
        {
            public DynamicDeserializerResolver() : base(new SpanJsonOptions()) { }

            public override DeserializeDynamicDelegate<char> DynamicDeserializer => ReadDynamic;

            private static object ReadDynamic(ref JsonReader<char> reader)
            {
                return JToken.Load(ref reader);
            }
        }

        [Fact]
        public void ReadDynamic()
        {
            var jsonStr = TestSR.BasicJson;
            dynamic obj = JsonSerializer.Generic.Utf16.Deserialize<object, DynamicDeserializerResolver>(jsonStr);
            Assert.NotNull(obj);
            Assert.IsType<JObject>(obj);
            Assert.Equal(30, (int)obj["age"]);

            obj = JsonSerializer.Generic.Utf16.Deserialize<dynamic, DynamicDeserializerResolver>(jsonStr);
            Assert.NotNull(obj);
            Assert.IsType<JObject>(obj);
            Assert.Equal(30, (int)obj["age"]);
        }

        [Fact]
        public void JsonObjectTypeDeserializerTest()
        {
            var dict = new Dictionary<string, object>
            {
                { "KeyA", 101 },
                { "KeyB", Guid.NewGuid() },
                { "KeyC", CombGuid.NewComb() },
            };

            var json = JsonSerializer.Generic.Utf16.Serialize(dict);
            var newDict = JsonSerializer.Generic.Utf16.Deserialize<Dictionary<string, object>, DynamicDeserializerResolver>(json);

            Assert.NotNull(newDict);
            Assert.Equal(3, newDict.Count);
            var utf16Num = newDict["KeyA"] as JValue;
            Assert.NotNull(utf16Num);
            Assert.Equal(dict["KeyA"], (int)utf16Num);

            var utf16Str = newDict["KeyB"] as JValue;
            Assert.NotNull(utf16Str);
            Assert.Equal(dict["KeyB"], (Guid)utf16Str);

            utf16Str = newDict["KeyC"] as JValue;
            Assert.NotNull(utf16Str);
            Assert.Equal(dict["KeyC"], (CombGuid)utf16Str);
        }
    }
}
