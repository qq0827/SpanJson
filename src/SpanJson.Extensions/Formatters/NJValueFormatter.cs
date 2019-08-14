using SpanJson.Internal;
using NJTokenType = Newtonsoft.Json.Linq.JTokenType;

namespace SpanJson.Formatters
{
    public sealed class NJValueFormatter<TValue> : JTokenFormatterBase<TValue>
        where TValue : Newtonsoft.Json.Linq.JValue
    {
        public static readonly NJValueFormatter<TValue> Default = new NJValueFormatter<TValue>();

        public override void Serialize(ref JsonWriter<byte> writer, TValue value, IJsonFormatterResolver<byte> resolver)
        {
            if (value is null || value.Value is null)
            {
                writer.WriteUtf8Null();
                return;
            }

            switch (value.Type)
            {
                case NJTokenType.Null:
                    writer.WriteUtf8Null();
                    break;
                case NJTokenType.Raw:
                    if (value.Value is byte[] utf8Json)
                    {
                        writer.WriteUtf8Verbatim(utf8Json);
                    }
                    else
                    {
                        writer.WriteUtf8Verbatim(TextEncodings.UTF8NoBOM.GetBytes(value.Value.ToString()));
                    }
                    break;
                default:
                    var formatter = resolver.GetRuntimeFormatter();
                    formatter.Serialize(ref writer, value.Value, resolver);
                    break;
            }
        }

        public override void Serialize(ref JsonWriter<char> writer, TValue value, IJsonFormatterResolver<char> resolver)
        {
            if (value is null || value.Value is null)
            {
                writer.WriteUtf16Null();
                return;
            }

            switch (value.Type)
            {
                case NJTokenType.Null:
                    writer.WriteUtf16Null();
                    break;
                case NJTokenType.Raw:
                    if (value.Value is byte[] utf8Json)
                    {
                        writer.WriteUtf16Verbatim(TextEncodings.Utf8.ToString(utf8Json));
                    }
                    else
                    {
                        writer.WriteUtf16Verbatim(value.Value.ToString());
                    }
                    break;
                default:
                    var formatter = resolver.GetRuntimeFormatter();
                    formatter.Serialize(ref writer, value.Value, resolver);
                    break;
            }
        }
    }
}
