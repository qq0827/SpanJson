using System.Collections.Generic;
using SpanJson.Converters;
using SpanJson.Document;
using SpanJson.Linq;
using Xunit;

namespace SpanJson.Tests
{
    public struct PrimitiveA
    {
        public int Value { get; set; }
    }

    public class PrimitiveAConverter : CustomPrimitiveValueConverter<PrimitiveA> { }

    public class CustomPrimitiveTypeTests : TestFixtureBase
    {
        [Fact]
        public void FromObject()
        {
            var pa = new PrimitiveA { Value = 101 };

            var token = JToken.FromObject(pa);
            Assert.IsType<JValue>(token);
            var jv = (JValue)token;
            Assert.Equal(101, ((PrimitiveA)jv.Value).Value);
            var newPa = token.ToObject<PrimitiveA>();
            Assert.Equal(101, newPa.Value);
        }

        [Fact]
        public void FromObject0()
        {
            var ipa = new IncludePrimitiveA
            {
                Id = 10,
                PA = new PrimitiveA { Value = 101 }
            };

            var jObj = JObject.FromObject(ipa);
            var id = jObj["Id"];
            Assert.IsType<JValue>(id);
            Assert.IsType<int>(id.Value<int>());
            var pa = jObj["PA"];
            Assert.IsType<JValue>(pa);
            Assert.Equal(101, pa.Value<PrimitiveA>().Value);
        }

        [Fact]
        public void FromObject1()
        {
            var ipa = new IncludePrimitiveB
            {
                Id = 10,
                PA = new JValue(new PrimitiveA { Value = 101 }, JTokenType.Undefined)
            };

            var jObj = JObject.FromObject(ipa);
            var id = jObj["Id"];
            Assert.IsType<JValue>(id);
            Assert.IsType<int>(id.Value<int>());
            var pa = jObj["PA"];
            Assert.IsType<JValue>(pa);
            Assert.Equal(101, pa.Value<PrimitiveA>().Value);
        }

        [Fact]
        public void FromObject2()
        {
            var doc = JsonDocument.Parse(TestSR.BasicJson);
            var jObj = JObject.FromObject(doc);
            Assert.Equal(30, (int)jObj["age"]);

            var dict = new Dictionary<string, object>();
            dict["fromDoc"] = doc;
            dict["fromDynamic"] = JsonSerializer.Generic.Utf16.Deserialize<dynamic>(TestSR.BasicJson);
            dict["fromJObject"] = jObj;

            var co = JObject.FromObject(dict);
            Assert.True(jObj.DeepEquals((JObject)co["fromDoc"]));
            Assert.True(jObj.DeepEquals((JObject)co["fromDynamic"]));
            Assert.True(jObj.DeepEquals((JObject)co["fromJObject"]));
        }

        public class IncludePrimitiveA
        {
            public int Id { get; set; }

            public PrimitiveA PA { get; set; }
        }

        public class IncludePrimitiveB
        {
            public int Id { get; set; }

            public JValue PA { get; set; }
        }
    }
}
