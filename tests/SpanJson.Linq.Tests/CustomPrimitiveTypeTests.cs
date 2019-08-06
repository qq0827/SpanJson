using System;
using SpanJson.Converters;
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
    }
}
