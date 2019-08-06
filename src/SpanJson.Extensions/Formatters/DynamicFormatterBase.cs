using SpanJson.Dynamic;

namespace SpanJson.Formatters
{
    public abstract class DynamicFormatterBase<T> : ICustomJsonFormatter<T>
    {
        public object Arguments { get; set; }

        public virtual T Deserialize(ref JsonReader<byte> reader, IJsonFormatterResolver<byte> resolver)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        public virtual T Deserialize(ref JsonReader<char> reader, IJsonFormatterResolver<char> resolver)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        public virtual void Serialize(ref JsonWriter<byte> writer, T value, IJsonFormatterResolver<byte> resolver)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        public virtual void Serialize(ref JsonWriter<char> writer, T value, IJsonFormatterResolver<char> resolver)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        protected static void WriteComplexElement(ref JsonWriter<byte> writer, object value, IJsonFormatterResolver<byte> resolver)
        {
            switch (value)
            {
                case SpanJsonDynamicObject dynamicObject:
                    DynamicObjectFormatter.Default.Serialize(ref writer, dynamicObject, resolver);
                    break;

                case SpanJsonDynamicArray<byte> utf8Array:
                    DynamicUtf8ArrayFormatter.Default.Serialize(ref writer, utf8Array, resolver);
                    break;

                case SpanJsonDynamicUtf8Number utf8Number:
                    DynamicUtf8NumberFormatter.Default.Serialize(ref writer, utf8Number, resolver);
                    break;

                case SpanJsonDynamicUtf8String utf8String:
                    DynamicUtf8StringFormatter.Default.Serialize(ref writer, utf8String, resolver);
                    break;

                case SpanJsonDynamicArray<char> utf16Array:
                    DynamicUtf16ArrayFormatter.Default.Serialize(ref writer, utf16Array, resolver);
                    break;

                case SpanJsonDynamicUtf16Number utf16Number:
                    DynamicUtf16NumberFormatter.Default.Serialize(ref writer, utf16Number, resolver);
                    break;

                case SpanJsonDynamicUtf16String utf16String:
                    DynamicUtf16StringFormatter.Default.Serialize(ref writer, utf16String, resolver);
                    break;

                default:
                    throw ThrowHelper.GetNotSupportedException();
            }
        }

        protected static void WriteComplexElement(ref JsonWriter<char> writer, object value, IJsonFormatterResolver<char> resolver)
        {
            switch (value)
            {
                case SpanJsonDynamicObject dynamicObject:
                    DynamicObjectFormatter.Default.Serialize(ref writer, dynamicObject, resolver);
                    break;

                case SpanJsonDynamicArray<byte> utf8Array:
                    DynamicUtf8ArrayFormatter.Default.Serialize(ref writer, utf8Array, resolver);
                    break;

                case SpanJsonDynamicUtf8Number utf8Number:
                    DynamicUtf8NumberFormatter.Default.Serialize(ref writer, utf8Number, resolver);
                    break;

                case SpanJsonDynamicUtf8String utf8String:
                    DynamicUtf8StringFormatter.Default.Serialize(ref writer, utf8String, resolver);
                    break;

                case SpanJsonDynamicArray<char> utf16Array:
                    DynamicUtf16ArrayFormatter.Default.Serialize(ref writer, utf16Array, resolver);
                    break;

                case SpanJsonDynamicUtf16Number utf16Number:
                    DynamicUtf16NumberFormatter.Default.Serialize(ref writer, utf16Number, resolver);
                    break;

                case SpanJsonDynamicUtf16String utf16String:
                    DynamicUtf16StringFormatter.Default.Serialize(ref writer, utf16String, resolver);
                    break;

                default:
                    throw ThrowHelper.GetNotSupportedException();
            }
        }
    }
}
