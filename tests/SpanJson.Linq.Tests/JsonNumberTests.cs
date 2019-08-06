using System;
using SpanJson.Document;
using SpanJson.Dynamic;
using SpanJson.Linq;
using Xunit;

namespace SpanJson.Tests
{
    public class JsonNumberTests
    {
        [Fact]
        public void ParseNumber()
        {
            var json = "123";
            var num = JsonSerializer.Generic.Utf16.Deserialize<int>(json);
            Assert.Equal(123, num);
            var dyNum = JsonSerializer.Generic.Utf16.Deserialize<dynamic>(json);
            Assert.IsType<SpanJsonDynamicUtf16Number>(dyNum);
            Assert.Equal(123, (int)dyNum);
            var jv = new JValue((SpanJsonDynamicUtf16Number)dyNum);
            Assert.Equal(123, (int)jv);
        }

        [Fact]
        public void ParseNumberWithQuote()
        {
            var json = "\"123\"";
            Assert.Throws<JsonParserException>(() => JsonSerializer.Generic.Utf16.Deserialize<int>(json));
            var str = JsonSerializer.Generic.Utf16.Deserialize<string>(json);
            Assert.Equal(123, int.Parse(str));
            var dyNum = JsonSerializer.Generic.Utf16.Deserialize<dynamic>(json);
            Assert.IsType<SpanJsonDynamicUtf16String>(dyNum);
            var jv = new JValue((SpanJsonDynamicUtf16String)dyNum);
            Assert.Equal(123, (int)jv);
        }

        [Fact]
        public void ParseNumber_Doc()
        {
            var json = "123";
            using (var doc = JsonDocument.Parse(json))
            {
                var num = doc.RootElement.GetInt32();
                Assert.Equal(123, num);
                var jv = new JValue(doc.RootElement);
                Assert.Equal(123, (int)jv);
            }
        }

        [Fact]
        public void ParseNumberWithQuote_Doc()
        {
            var json = "\"123\"";
            using (var doc = JsonDocument.Parse(json))
            {
                Assert.Throws<InvalidOperationException>(() => doc.RootElement.GetInt32());
                var num = doc.RootElement.ToString(); // ToString == GetString
                Assert.Equal(123, int.Parse(num));
                var jv = new JValue(doc.RootElement);
                Assert.Equal(123, (int)jv);
            }
        }
    }
}
