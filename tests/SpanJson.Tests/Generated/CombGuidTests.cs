using System;
using CuteAnt;
using Xunit;

namespace SpanJson.Tests.Generated
{
    public partial class NullableCombGuidListTests : NullableListTestBase<CombGuid>
    {
    }

    public partial class NullableCombGuidArrayTests : NullableArrayTestBase<CombGuid>
    {
    }
    public partial class CombGuidTests : StructTestBase<CombGuid>
    {

        [Fact]
        public void PrimitiveWrapperUtf8()
        {
            var value = CombGuid.NewComb();
            var writer = new JsonWriter<byte>(16);
            writer.WriteCombGuid(value);
            var output = writer.ToByteArray();

            var reader = new JsonReader<byte>(output);
            var deserialized = reader.ReadCombGuid();
            Assert.Equal(value, deserialized);
        }

        [Fact]
        public void PrimitiveWrapperUtf16()
        {
            var value = CombGuid.NewComb();
            var writer = new JsonWriter<char>(16);
            writer.WriteCombGuid(value);
            var output = writer.ToString();

            var reader = new JsonReader<char>(output.AsSpan());
            var deserialized = reader.ReadCombGuid();
            Assert.Equal(value, deserialized);
        }

        [Fact]
        public void EscapedUtf8()
        {
            var value = CombGuid.NewComb();
            var writer = new JsonWriter<byte>(16);
            writer.WriteCombGuid(value);
            var output = writer.ToByteArray();
            output = EscapeMore(output);
            var reader = new JsonReader<byte>(output);
            var deserialized = reader.ReadCombGuid();
            Assert.Equal(value, deserialized);
        }

        [Fact]
        public void EscapedUtf16()
        {
            var value = CombGuid.NewComb();
            var writer = new JsonWriter<char>(16);
            writer.WriteCombGuid(value);
            var output = writer.ToString();
            output = EscapeMore(output);
            var reader = new JsonReader<char>(output.AsSpan());
            var deserialized = reader.ReadCombGuid();
            Assert.Equal(value, deserialized);
        }
    }
    public partial class CombGuidListTests : ListTestBase<CombGuid>
    {
    }

    public partial class CombGuidArrayTests : ArrayTestBase<CombGuid>
    {
    }
}
