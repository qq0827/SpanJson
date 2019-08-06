// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.Linq;
using System.Text;
using SpanJson.Document;
using Xunit;

namespace SpanJson.Tests
{
    public static class JsonPropertyTests
    {

        //[Fact]
        //public static void CheckByPassingNullWriter()
        //{
        //    using (JsonDocument doc = JsonDocument.Parse("{\"First\":1}", default))
        //    {
        //        foreach (JsonProperty property in doc.RootElement.EnumerateObject())
        //        {
        //            Assert.Throws<ArgumentNullException>("writer", () => property.WriteTo(null));
        //        }
        //    }
        //}

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public static void WriteObjectValidations(bool skipValidation)
        {
            using (JsonDocument doc = JsonDocument.Parse("{\"First\":1}", default))
            {
                JsonElement root = doc.RootElement;
                var options = new JsonWriterOptions
                {
                    SkipValidation = skipValidation,
                };
                var writer = new Utf8JsonWriter(16, options);
                if (skipValidation)
                {
                    foreach (JsonProperty property in root.EnumerateObject())
                    {
                        property.WriteTo(ref writer);
                    }
                    AssertContents("\"First\":1", writer.ToByteArray());
                }
                else
                {
                    foreach (JsonProperty property in root.EnumerateObject())
                    {
                        try
                        {
                            property.WriteTo(ref writer);
                            Assert.True(false);
                        }
                        catch(Exception ex)
                        {
                            Assert.IsType<InvalidOperationException>(ex);
                        }
                    }
                    AssertContents("", writer.ToByteArray());
                }
            }
        }

        [Fact]
        public static void WriteSimpleObject()
        {
            using (JsonDocument doc = JsonDocument.Parse("{\"First\":1, \"Number\":1e400}"))
            {
                var writer = new Utf8JsonWriter(16);
                writer.WriteStartObject();
                foreach (JsonProperty prop in doc.RootElement.EnumerateObject())
                {
                    prop.WriteTo(ref writer);
                }
                writer.WriteEndObject();

                AssertContents("{\"First\":1,\"Number\":1e400}", writer.ToByteArray());
            }
        }

        private static void AssertContents(string expectedValue, byte[] buffer)
        {
            Assert.Equal(expectedValue, Encoding.UTF8.GetString(buffer));
        }

        [Theory]
        [InlineData("hello")]
        [InlineData("")]
        [InlineData(null)]
        public static void NameEquals_InvalidInstance_Throws(string text)
        {
#if DESKTOPCLR
            const string ErrorMessage = "对象的当前状态使该操作无效。";
#else
            const string ErrorMessage = "Operation is not valid due to the current state of the object.";
#endif
            JsonProperty prop = default;
            AssertExtensions.Throws<InvalidOperationException>(() => prop.NameEquals(text), ErrorMessage);
            AssertExtensions.Throws<InvalidOperationException>(() => prop.NameEquals(text.AsSpan()), ErrorMessage);
            byte[] expectedGetBytes = text == null ? null : Encoding.UTF8.GetBytes(text);
            AssertExtensions.Throws<InvalidOperationException>(() => prop.NameEquals(expectedGetBytes), ErrorMessage);
        }

        [Theory]
        [InlineData("conne\\u0063tionId", "connectionId")]
        [InlineData("connectionId", "connectionId")]
        [InlineData("123", "123")]
        [InlineData("My name is \\\"Ahson\\\"", "My name is \"Ahson\"")]
        public static void NameEquals_UseGoodMatches_True(string propertyName, string otherText)
        {
            string jsonString = $"{{ \"{propertyName}\" : \"itsValue\" }}";
            using (JsonDocument doc = JsonDocument.Parse(jsonString))
            {
                JsonElement jElement = doc.RootElement;
                JsonProperty property = jElement.EnumerateObject().First();
                byte[] expectedGetBytes = Encoding.UTF8.GetBytes(otherText);
                Assert.True(property.NameEquals(otherText));
                Assert.True(property.NameEquals(otherText.AsSpan()));
                Assert.True(property.NameEquals(expectedGetBytes));
            }
        }

        [Fact]
        public static void NameEquals_GivenPropertyAndValue_TrueForPropertyName()
        {
            string jsonString = $"{{ \"aPropertyName\" : \"itsValue\" }}";
            using (JsonDocument doc = JsonDocument.Parse(jsonString))
            {
                JsonElement jElement = doc.RootElement;
                JsonProperty property = jElement.EnumerateObject().First();

                string text = "aPropertyName";
                byte[] expectedGetBytes = Encoding.UTF8.GetBytes(text);
                Assert.True(property.NameEquals(text));
                Assert.True(property.NameEquals(text.AsSpan()));
                Assert.True(property.NameEquals(expectedGetBytes));

                text = "itsValue";
                expectedGetBytes = Encoding.UTF8.GetBytes(text);
                Assert.False(property.NameEquals(text));
                Assert.False(property.NameEquals(text.AsSpan()));
                Assert.False(property.NameEquals(expectedGetBytes));
            }
        }
    }
}
