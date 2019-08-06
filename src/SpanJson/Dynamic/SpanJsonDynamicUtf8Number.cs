using System;
using System.ComponentModel;

namespace SpanJson.Dynamic
{
    [TypeConverter(typeof(DynamicTypeConverter))]
    public sealed class SpanJsonDynamicUtf8Number : SpanJsonDynamicNumber<byte>
    {
        public SpanJsonDynamicUtf8Number(in ReadOnlySpan<byte> span) : base(span, span.IndexOf(JsonUtf8Constant.Period) != -1) { }
        internal SpanJsonDynamicUtf8Number(in ReadOnlySpan<byte> span, bool isFloat) : base(span, isFloat) { }

        public SpanJsonDynamicUtf8Number(in ArraySegment<byte> data) : base(data, data.AsSpan().IndexOf(JsonUtf8Constant.Period) != -1) { }
        internal SpanJsonDynamicUtf8Number(in ArraySegment<byte> data, bool isFloat) : base(data, isFloat) { }
    }
}