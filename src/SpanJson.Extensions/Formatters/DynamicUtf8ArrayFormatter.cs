using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SpanJson.Dynamic;
using SpanJson.Internal;

namespace SpanJson.Formatters
{
    public sealed class DynamicUtf8ArrayFormatter : DynamicFormatterBase<SpanJsonDynamicArray<byte>>
    {
        public static readonly DynamicUtf8ArrayFormatter Default = new DynamicUtf8ArrayFormatter();

        public override void Serialize(ref JsonWriter<byte> writer, SpanJsonDynamicArray<byte> value, IJsonFormatterResolver<byte> resolver)
        {
            if (value is null) { writer.WriteUtf8Null(); return; }

            if (value.TryGetRaw(out ArraySegment<byte> rawJson))
            {
                writer.WriteUtf8Verbatim(rawJson);
            }
            else
            {
                var valueLength = value.Length;
                writer.IncrementDepth();
                writer.WriteUtf8BeginArray();

                if (valueLength > 0)
                {
                    WriteComplexElement(ref writer, value[0], resolver);
                    for (var i = 1; i < valueLength; i++)
                    {
                        writer.WriteUtf8ValueSeparator();
                        WriteComplexElement(ref writer, value[i], resolver);
                    }
                }

                writer.DecrementDepth();
                writer.WriteUtf8EndArray();
            }
        }

        public override void Serialize(ref JsonWriter<char> writer, SpanJsonDynamicArray<byte> value, IJsonFormatterResolver<char> resolver)
        {
            if (value is null) { writer.WriteUtf16Null(); return; }

            if (value.TryGetRaw(out ArraySegment<byte> rawJson))
            {
                ReadOnlySpan<byte> utf8Json = rawJson;
                var maxRequired = TextEncodings.Utf8.GetCharCount(utf8Json);

                char[] valueArray = null;

                Span<char> utf16Json = (uint)maxRequired <= JsonSharedConstant.StackallocThreshold ?
                    stackalloc char[maxRequired] :
                    (valueArray = ArrayPool<char>.Shared.Rent(maxRequired));
                var written = TextEncodings.Utf8.GetChars(utf8Json, utf16Json);

#if NETSTANDARD2_0 || NET471 || NET451
                unsafe
                {
                    writer.WriteUtf16Verbatim(new ReadOnlySpan<char>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(utf16Json)), written));
                }
#else
                writer.WriteUtf16Verbatim(MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(utf16Json), written));
#endif

                if (valueArray is object) { ArrayPool<char>.Shared.Return(valueArray); }
            }
            else
            {
                var valueLength = value.Length;
                writer.IncrementDepth();
                writer.WriteUtf16BeginArray();

                if (valueLength > 0)
                {
                    WriteComplexElement(ref writer, value[0], resolver);
                    for (var i = 1; i < valueLength; i++)
                    {
                        writer.WriteUtf16ValueSeparator();
                        WriteComplexElement(ref writer, value[i], resolver);
                    }
                }

                writer.DecrementDepth();
                writer.WriteUtf16EndArray();
            }
        }
    }
}
