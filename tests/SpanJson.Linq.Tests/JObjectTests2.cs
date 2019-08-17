using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Text;
using SpanJson.Document;
using SpanJson.Linq;
using SpanJson.Resolvers;
using Xunit;
using NJObject = Newtonsoft.Json.Linq.JObject;
using NJArray = Newtonsoft.Json.Linq.JArray;
using NJValue = Newtonsoft.Json.Linq.JValue;

namespace SpanJson.Tests
{
    partial class JObjectTests
    {
        [Fact]
        public void JsonNETTest()
        {
            var o = new NJObject();
            o["id"] = Guid.NewGuid();
            o["age"] = 101;
            o["ddd"] = 101.12d;
            o["eee"] = 202.22M;
            o["bool"] = true;
            o["name"] = "sea";
            o["createDate"] = "sea";
            var json = o.ToString();
            var jObj = NJObject.Parse(json);

            Assert.IsType<string>(((NJValue)jObj["id"]).Value);

            var age = (NJValue)jObj["age"];
            Assert.IsType<long>(age.Value);
            var result = Utf8Parser.TryParse(Encoding.UTF8.GetBytes(age.ToString()), out int iv, out _, 'G');
            Assert.True(result);
            Assert.Equal(101, iv);

            var douV = (NJValue)jObj["ddd"];
            Assert.IsType<double>(douV.Value);
            result = Utf8Parser.TryParse(Encoding.UTF8.GetBytes(douV.ToString()), out decimal lv, out _, 'G');
            Assert.True(result);
            Assert.Equal(101, iv);

            Assert.IsType<double>(((NJValue)jObj["eee"]).Value);
            Assert.IsType<bool>(((NJValue)jObj["bool"]).Value);
            Assert.IsType<string>(((NJValue)jObj["name"]).Value);
            Assert.IsType<string>(((NJValue)jObj["createDate"]).Value);
        }

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
        public void FromDocument()
        {
            using (var doc = JsonDocument.Parse(TestSR.BasicJson))
            {
                var jObj = JObject.FromDocument(doc);
                var basicJson = jObj.ToObject<BasicJson, ExcludeNullsCamelCaseResolver<byte>, ExcludeNullsCamelCaseResolver<char>>();
                Assert.NotNull(basicJson);
                Assert.Equal(30, basicJson.Age);
                Assert.Equal("John", basicJson.First);
                Assert.Equal("Smith", basicJson.Last);
                Assert.NotNull(basicJson.PhoneNumbers);

                Assert.Equal(2, basicJson.PhoneNumbers.Length);
                Assert.Equal("425-000-1212", basicJson.PhoneNumbers[0]);
                Assert.Equal("425-000-1213", basicJson.PhoneNumbers[1]);
                var jArray = (JArray)jObj["phoneNumbers"];
                var phoneNumbers = jArray.ToObject<List<string>>();
                Assert.Equal(2, phoneNumbers.Count);
                Assert.Equal("425-000-1212", phoneNumbers[0]);
                Assert.Equal("425-000-1213", phoneNumbers[1]);

                Assert.NotNull(basicJson.Address);
                Assert.Equal("1 Microsoft Way", basicJson.Address.Street);
                Assert.Equal("Redmond", basicJson.Address.City);
                Assert.Equal(98052, basicJson.Address.Zip);
                var addr = ((JObject)jObj["address"]).ToObject<BasicAddr, ExcludeNullsCamelCaseResolver<byte>, ExcludeNullsCamelCaseResolver<char>>();
                Assert.Equal("1 Microsoft Way", addr.Street);
                Assert.Equal("Redmond", addr.City);
                Assert.Equal(98052, addr.Zip);
            }
        }

        [Fact]
        public void FromDynamicUtf16Object()
        {
            dynamic obj = JsonSerializer.Generic.Utf16.Deserialize<dynamic>(TestSR.BasicJson.ToCharArray());

            var jObj = JObject.FromObject(obj);
            var basicJson = jObj.ToObject<BasicJson, ExcludeNullsCamelCaseResolver<byte>, ExcludeNullsCamelCaseResolver<char>>();
            Assert.NotNull(basicJson);
            Assert.Equal(30, basicJson.Age);
            Assert.Equal("John", basicJson.First);
            Assert.Equal("Smith", basicJson.Last);
            Assert.NotNull(basicJson.PhoneNumbers);

            Assert.Equal(2, basicJson.PhoneNumbers.Length);
            Assert.Equal("425-000-1212", basicJson.PhoneNumbers[0]);
            Assert.Equal("425-000-1213", basicJson.PhoneNumbers[1]);

            var jArray = (JArray)jObj["phoneNumbers"];
            var utf16Json = JsonSerializer.Generic.Utf16.Serialize(jArray);
            Assert.True(jArray.DeepEquals(JArray.Parse(utf16Json)));
            Assert.True(jArray.DeepEquals(JsonSerializer.Generic.Utf16.Deserialize<JArray>(utf16Json)));
            Assert.True(jArray.DeepEquals((JArray)JsonSerializer.NonGeneric.Utf16.Deserialize(utf16Json, typeof(JArray))));

            var phoneNumbers = jArray.ToObject<List<string>>();
            Assert.Equal(2, phoneNumbers.Count);
            Assert.Equal("425-000-1212", phoneNumbers[0]);
            Assert.Equal("425-000-1213", phoneNumbers[1]);

            Assert.NotNull(basicJson.Address);
            Assert.Equal("1 Microsoft Way", basicJson.Address.Street);
            Assert.Equal("Redmond", basicJson.Address.City);
            Assert.Equal(98052, basicJson.Address.Zip);
            var addr = ((JObject)jObj["address"]).ToObject<BasicAddr, ExcludeNullsCamelCaseResolver<byte>, ExcludeNullsCamelCaseResolver<char>>();
            Assert.Equal("1 Microsoft Way", addr.Street);
            Assert.Equal("Redmond", addr.City);
            Assert.Equal(98052, addr.Zip);
        }

        [Fact]
        public void FromDynamicUtf8Object()
        {
            dynamic obj = JsonSerializer.Generic.Utf8.Deserialize<dynamic>(Encoding.UTF8.GetBytes(TestSR.BasicJson));

            var jObj = JObject.FromObject(obj);
            var basicJson = jObj.ToObject<BasicJson, ExcludeNullsCamelCaseResolver<byte>, ExcludeNullsCamelCaseResolver<char>>();
            Assert.NotNull(basicJson);
            Assert.Equal(30, basicJson.Age);
            Assert.Equal("John", basicJson.First);
            Assert.Equal("Smith", basicJson.Last);
            Assert.NotNull(basicJson.PhoneNumbers);

            Assert.Equal(2, basicJson.PhoneNumbers.Length);
            Assert.Equal("425-000-1212", basicJson.PhoneNumbers[0]);
            Assert.Equal("425-000-1213", basicJson.PhoneNumbers[1]);

            var jArray = (JArray)jObj["phoneNumbers"];
            var utf8Json = JsonSerializer.Generic.Utf8.Serialize(jArray);
            Assert.True(jArray.DeepEquals(JArray.Parse(utf8Json)));
            Assert.True(jArray.DeepEquals(JsonSerializer.Generic.Utf8.Deserialize<JArray>(utf8Json)));
            Assert.True(jArray.DeepEquals((JArray)JsonSerializer.NonGeneric.Utf8.Deserialize(utf8Json, typeof(JArray))));

            var phoneNumbers = jArray.ToObject<List<string>>();
            Assert.Equal(2, phoneNumbers.Count);
            Assert.Equal("425-000-1212", phoneNumbers[0]);
            Assert.Equal("425-000-1213", phoneNumbers[1]);

            Assert.NotNull(basicJson.Address);
            Assert.Equal("1 Microsoft Way", basicJson.Address.Street);
            Assert.Equal("Redmond", basicJson.Address.City);
            Assert.Equal(98052, basicJson.Address.Zip);
            var addr = ((JObject)jObj["address"]).ToObject<BasicAddr, ExcludeNullsCamelCaseResolver<byte>, ExcludeNullsCamelCaseResolver<char>>();
            Assert.Equal("1 Microsoft Way", addr.Street);
            Assert.Equal("Redmond", addr.City);
            Assert.Equal(98052, addr.Zip);
        }

        [Fact]
        public void JsonNetObject()
        {
            var jObj = NJObject.Parse(TestSR.BasicJson);
            var basicJson = jObj.ToObject<BasicJson>();
            Assert.NotNull(basicJson);
            Assert.Equal(30, basicJson.Age);
            Assert.Equal("John", basicJson.First);
            Assert.Equal("Smith", basicJson.Last);
            Assert.NotNull(basicJson.PhoneNumbers);

            Assert.Equal(2, basicJson.PhoneNumbers.Length);
            Assert.Equal("425-000-1212", basicJson.PhoneNumbers[0]);
            Assert.Equal("425-000-1213", basicJson.PhoneNumbers[1]);

            var jArray = (NJArray)jObj["phoneNumbers"];

            var phoneNumbers = jArray.ToObject<List<string>>();
            Assert.Equal(2, phoneNumbers.Count);
            Assert.Equal("425-000-1212", phoneNumbers[0]);
            Assert.Equal("425-000-1213", phoneNumbers[1]);

            Assert.NotNull(basicJson.Address);
            Assert.Equal("1 Microsoft Way", basicJson.Address.Street);
            Assert.Equal("Redmond", basicJson.Address.City);
            Assert.Equal(98052, basicJson.Address.Zip);
            var addr = ((NJObject)jObj["address"]).ToObject<BasicAddr>();
            Assert.Equal("1 Microsoft Way", addr.Street);
            Assert.Equal("Redmond", addr.City);
            Assert.Equal(98052, addr.Zip);
        }

        [Fact]
        public void SaveDocumentUtf16()
        {
            using (var doc = JsonDocument.Parse(TestSR.BasicJson))
            {
                var jObj = JObject.FromDocument(doc);
                var utf16Json = JsonSerializer.Generic.Utf16.Serialize(jObj);
                Assert.True(jObj.DeepEquals(JObject.Parse(utf16Json)));
                Assert.True(jObj.DeepEquals(JsonSerializer.Generic.Utf16.Deserialize<JObject>(utf16Json)));
                Assert.True(jObj.DeepEquals((JObject)JsonSerializer.NonGeneric.Utf16.Deserialize(utf16Json, typeof(JObject))));

                var token = JToken.FromDocument(doc);
                utf16Json = JsonSerializer.NonGeneric.Utf16.Serialize(token);
                Assert.True(jObj.DeepEquals(JObject.Parse(utf16Json)));
                utf16Json = JsonSerializer.Generic.Utf16.Serialize(token);
                Assert.True(jObj.DeepEquals(JObject.Parse(utf16Json)));
            }
        }

        [Fact]
        public void SaveDocumentUtf8()
        {
            using (var doc = JsonDocument.Parse(TestSR.BasicJson))
            {
                var jObj = JObject.FromDocument(doc);
                var utf8Json = JsonSerializer.Generic.Utf8.Serialize(jObj);
                Assert.True(jObj.DeepEquals(JObject.Parse(utf8Json)));
                Assert.True(jObj.DeepEquals(JsonSerializer.Generic.Utf8.Deserialize<JObject>(utf8Json)));
                Assert.True(jObj.DeepEquals((JObject)JsonSerializer.NonGeneric.Utf8.Deserialize(utf8Json, typeof(JObject))));

                var token = JToken.FromDocument(doc);
                utf8Json = JsonSerializer.NonGeneric.Utf8.Serialize(jObj);
                Assert.True(jObj.DeepEquals(JObject.Parse(utf8Json)));
                utf8Json = JsonSerializer.Generic.Utf8.Serialize(jObj);
                Assert.True(jObj.DeepEquals(JObject.Parse(utf8Json)));
            }
        }

        [Fact]
        public void SaveDynamicUtf8ObjectToUtf8()
        {
            var jObj = JObject.Parse(Encoding.UTF8.GetBytes(TestSR.BasicJson));
            var utf8Json = JsonSerializer.NonGeneric.Utf8.Serialize(jObj);
            Assert.True(jObj.DeepEquals(JObject.Parse(utf8Json)));

            jObj = JObject.Parse(new ArraySegment<byte>(Encoding.UTF8.GetBytes(TestSR.BasicJson)));
            utf8Json = JsonSerializer.NonGeneric.Utf8.Serialize(jObj);
            Assert.True(jObj.DeepEquals(JObject.Parse(utf8Json)));

            jObj = JObject.Parse(Encoding.UTF8.GetBytes(TestSR.BasicJson).AsSpan());
            utf8Json = JsonSerializer.NonGeneric.Utf8.Serialize(jObj);
            Assert.True(jObj.DeepEquals(JObject.Parse(utf8Json)));

            jObj = JObject.Parse(Encoding.UTF8.GetBytes(TestSR.BasicJson).AsMemory());
            utf8Json = JsonSerializer.NonGeneric.Utf8.Serialize(jObj);
            Assert.True(jObj.DeepEquals(JObject.Parse(utf8Json)));
        }

        [Fact]
        public void SaveDynamicUtf8ObjectToUtf16()
        {
            var jObj = JObject.Parse(Encoding.UTF8.GetBytes(TestSR.BasicJson));
            var utf16Json = JsonSerializer.NonGeneric.Utf16.Serialize(jObj);
            Assert.True(jObj.DeepEquals(JObject.Parse(utf16Json)));
        }

        [Fact]
        public void SaveDynamicUtf16ObjectToUtf8()
        {
            var jObj = JObject.Parse(TestSR.BasicJson);
            var utf8Json = JsonSerializer.NonGeneric.Utf8.Serialize(jObj);
            Assert.True(jObj.DeepEquals(JObject.Parse(utf8Json)));

            jObj = JObject.Parse(TestSR.BasicJson.ToCharArray());
            utf8Json = JsonSerializer.NonGeneric.Utf8.Serialize(jObj);
            Assert.True(jObj.DeepEquals(JObject.Parse(utf8Json)));

            jObj = JObject.Parse(new ArraySegment<char>(TestSR.BasicJson.ToCharArray()));
            utf8Json = JsonSerializer.NonGeneric.Utf8.Serialize(jObj);
            Assert.True(jObj.DeepEquals(JObject.Parse(utf8Json)));

            jObj = JObject.Parse(TestSR.BasicJson.AsSpan());
            utf8Json = JsonSerializer.NonGeneric.Utf8.Serialize(jObj);
            Assert.True(jObj.DeepEquals(JObject.Parse(utf8Json)));

            jObj = JObject.Parse(TestSR.BasicJson.AsMemory());
            utf8Json = JsonSerializer.NonGeneric.Utf8.Serialize(jObj);
            Assert.True(jObj.DeepEquals(JObject.Parse(utf8Json)));

            jObj = JObject.Parse(TestSR.BasicJson.ToCharArray().AsMemory());
            utf8Json = JsonSerializer.NonGeneric.Utf8.Serialize(jObj);
            Assert.True(jObj.DeepEquals(JObject.Parse(utf8Json)));
        }

        [Fact]
        public void SaveDynamicUtf16ObjectToUtf16()
        {
            var jObj = JObject.Parse(TestSR.BasicJson);
            var utf16Json = JsonSerializer.NonGeneric.Utf16.Serialize(jObj);
            Assert.True(jObj.DeepEquals(JObject.Parse(utf16Json)));
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