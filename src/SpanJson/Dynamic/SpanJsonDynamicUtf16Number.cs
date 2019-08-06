using System;
using System.ComponentModel;

namespace SpanJson.Dynamic
{
    [TypeConverter(typeof(DynamicTypeConverter))]
    public sealed class SpanJsonDynamicUtf16Number : SpanJsonDynamicNumber<char>
    {
        public SpanJsonDynamicUtf16Number(in ReadOnlySpan<char> span) : base(span, span.IndexOf(JsonUtf16Constant.Period) != -1) { }
        internal SpanJsonDynamicUtf16Number(in ReadOnlySpan<char> span, bool isFloat) : base(span, isFloat) { }

        public SpanJsonDynamicUtf16Number(in ArraySegment<char> data) : base(data, data.AsSpan().IndexOf(JsonUtf16Constant.Period) != -1) { }
        internal SpanJsonDynamicUtf16Number(in ArraySegment<char> data, bool isFloat) : base(data, isFloat) { }
    }
}