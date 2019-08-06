using System;
using System.ComponentModel;

namespace SpanJson.Dynamic
{
    [TypeConverter(typeof(DynamicTypeConverter))]
    public sealed class SpanJsonDynamicUtf8String : SpanJsonDynamicString<byte>
    {
        public SpanJsonDynamicUtf8String(in ReadOnlySpan<byte> span) : base(span, false) { }
        internal SpanJsonDynamicUtf8String(in ReadOnlySpan<byte> span, bool isFloat) : base(span, isFloat) { }

        public SpanJsonDynamicUtf8String(in ArraySegment<byte> data) : base(data, false) { }
        internal SpanJsonDynamicUtf8String(in ArraySegment<byte> data, bool isFloat) : base(data, isFloat) { }
    }
}