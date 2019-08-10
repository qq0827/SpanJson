using System;
using SpanJson.Serialization;
using Xunit;
using NFormatting = Newtonsoft.Json.Formatting;
using NJsonSerializer = Newtonsoft.Json.JsonSerializer;
using NJsonSerializerSettings = Newtonsoft.Json.JsonSerializerSettings;

namespace SpanJson.Tests
{
    public class JsonSerializerReflectionTests
    {
        [Fact]
        public void JsonSerializer_IsCheckAdditionalContentSet()
        {
            NJsonSerializer jsonSerializer = null;
            Assert.Throws<ArgumentNullException>("jsonSerializer", () => jsonSerializer.IsCheckAdditionalContentSetX());

            jsonSerializer = NJsonSerializer.CreateDefault();
            Assert.False(jsonSerializer.IsCheckAdditionalContentSetX());

            jsonSerializer = NJsonSerializer.CreateDefault(new NJsonSerializerSettings { CheckAdditionalContent = true });
            Assert.True(jsonSerializer.IsCheckAdditionalContentSetX());

            jsonSerializer = NJsonSerializer.CreateDefault(new NJsonSerializerSettings { CheckAdditionalContent = false });
            Assert.True(jsonSerializer.IsCheckAdditionalContentSetX());

            jsonSerializer.SetCheckAdditionalContent(null);
            Assert.Null(jsonSerializer.GetCheckAdditionalContent());

            jsonSerializer.SetCheckAdditionalContent(true);
            Assert.True(jsonSerializer.GetCheckAdditionalContent());

            jsonSerializer.SetCheckAdditionalContent(false);
            Assert.False(jsonSerializer.GetCheckAdditionalContent());
        }

        [Fact]
        public void JsonSerializer_FormattingField_Test()
        {
            NJsonSerializer jsonSerializer = null;
            Assert.Throws<ArgumentNullException>("jsonSerializer", () => jsonSerializer.GetFormatting());

            jsonSerializer = NJsonSerializer.CreateDefault();

            jsonSerializer.SetFormatting(null);
            Assert.Null(jsonSerializer.GetFormatting());

            jsonSerializer.SetFormatting(NFormatting.None);
            Assert.Equal(NFormatting.None, jsonSerializer.GetFormatting());

            jsonSerializer.SetFormatting(NFormatting.Indented);
            Assert.Equal(NFormatting.Indented, jsonSerializer.GetFormatting());
        }
    }
}
