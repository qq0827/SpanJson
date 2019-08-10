using System;
using System.Collections.Generic;
using System.Text;
using SpanJson.Serialization;
using Xunit;

namespace SpanJson.Tests
{
    public class JsonClassInfoTests
    {
        [Fact]
        public void GetElementType()
        {
            var type = typeof(Dictionary<int, string>);
            var elementType = JsonClassInfo.GetElementType(type);
            Assert.Equal(typeof(string), elementType);
        }
    }
}
