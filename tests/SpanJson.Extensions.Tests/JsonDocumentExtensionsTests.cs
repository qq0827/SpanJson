using System;
using System.Collections.Generic;
using System.Text;
using SpanJson.Resolvers;
using SpanJson.Document;
using Xunit;

namespace SpanJson.Tests
{
    public class JsonDocumentExtensionsTests
    {
        [Fact]
        public void DocumentToObject()
        {
            var json = TestSR.BasicJson;
            using (var doc = JsonDocument.Parse(json))
            {
                var basicJson = doc.ToObject<BasicJson, ExcludeNullsCamelCaseResolver<byte>>();
                Assert.NotNull(basicJson);
                Assert.Equal(30, basicJson.Age);
                Assert.Equal("John", basicJson.First);
                Assert.Equal("Smith", basicJson.Last);
                Assert.NotNull(basicJson.PhoneNumbers);
                Assert.Equal(2, basicJson.PhoneNumbers.Length);
                Assert.Equal("425-000-1212", basicJson.PhoneNumbers[0]);
                Assert.Equal("425-000-1213", basicJson.PhoneNumbers[1]);
                Assert.NotNull(basicJson.Address);
                Assert.Equal("1 Microsoft Way", basicJson.Address.Street);
                Assert.Equal("Redmond", basicJson.Address.City);
                Assert.Equal(98052, basicJson.Address.Zip);
            }
        }

        [Fact]
        public void RootElementToObject()
        {
            var json = TestSR.BasicJson;
            using (var doc = JsonDocument.Parse(json))
            {
                var rootElement = doc.RootElement;
                var basicJson = rootElement.ToObject<BasicJson, ExcludeNullsCamelCaseResolver<byte>>();
                Assert.NotNull(basicJson);
                Assert.Equal(30, basicJson.Age);
                Assert.Equal("John", basicJson.First);
                Assert.Equal("Smith", basicJson.Last);
                Assert.NotNull(basicJson.PhoneNumbers);
                Assert.Equal(2, basicJson.PhoneNumbers.Length);
                Assert.Equal("425-000-1212", basicJson.PhoneNumbers[0]);
                Assert.Equal("425-000-1213", basicJson.PhoneNumbers[1]);
                Assert.NotNull(basicJson.Address);
                Assert.Equal("1 Microsoft Way", basicJson.Address.Street);
                Assert.Equal("Redmond", basicJson.Address.City);
                Assert.Equal(98052, basicJson.Address.Zip);
            }
        }

        [Fact]
        public void ElementToObject()
        {
            var json = TestSR.BasicJson;
            using (var doc = JsonDocument.Parse(json))
            {
                var rootElement = doc.RootElement;
                var ageElement = rootElement.GetProperty("age");
                Assert.Equal(ageElement.GetInt32(), ageElement.ToObject<int>());
                var nums = rootElement.GetProperty("phoneNumbers").ToObject<List<string>, ExcludeNullsCamelCaseResolver<byte>>();
                Assert.Equal(2, nums.Count);
                Assert.Equal("425-000-1212", nums[0]);
                Assert.Equal("425-000-1213", nums[1]);
                var addr = rootElement.GetProperty("address").ToObject<BasicAddr, ExcludeNullsCamelCaseResolver<byte>>();
                Assert.NotNull(addr);
                Assert.Equal("1 Microsoft Way", addr.Street);
                Assert.Equal("Redmond", addr.City);
                Assert.Equal(98052, addr.Zip);
            }
        }

        [Fact]
        public void GetArrayLength()
        {
            var json = TestSR.BasicJson;
            using (var doc = JsonDocument.Parse(json))
            {
                var rootElement = doc.RootElement;

                Assert.Throws<InvalidOperationException>(() => rootElement.GetArrayLength());
            }
        }

        public class BasicJson
        {
            public int Age { get; set; }
            public string First { get; set; }
            public string Last { get; set; }
            public string[] PhoneNumbers { get; set; }
            public BasicAddr Address { get; set; }
        }

        public class BasicAddr
        {
            public string Street { get; set; }
            public string City { get; set; }
            public int Zip { get; set; }
        }
    }
}
