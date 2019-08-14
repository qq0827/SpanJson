using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SpanJson.Dynamic;
using SpanJson.Internal;

namespace SpanJson.Formatters
{
    public sealed class DynamicUtf16ArrayFormatter : DynamicFormatterBase<SpanJsonDynamicArray<char>>
    {
        public static readonly DynamicUtf16ArrayFormatter Default = new DynamicUtf16ArrayFormatter();

        public override void Serialize(ref JsonWriter<byte> writer, SpanJsonDynamicArray<char> value, IJsonFormatterResolver<byte> resolver)
        {
            if (value is null) { writer.WriteUtf8Null(); return; }

            if (value.TryGetRaw(out ArraySegment<char> rawJson))
            {
                ReadOnlySpan<char> utf16Json = rawJson;
                var maxRequired = utf16Json.Length * JsonSharedConstant.MaxExpansionFactorWhileTranscoding;

                byte[] valueArray = null;

                Span<byte> utf8Json = (uint)maxRequired <= JsonSharedConstant.StackallocThreshold ?
                    stackalloc byte[maxRequired] :
                    (valueArray = ArrayPool<byte>.Shared.Rent(maxRequired));
                var written = TextEncodings.Utf8.GetBytes(utf16Json, utf8Json);

#if NETSTANDARD2_0 || NET471 || NET451
                unsafe
                {
                    writer.WriteUtf8Verbatim(new ReadOnlySpan<byte>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(utf8Json)), written));
                }
#else
                writer.WriteUtf8Verbatim(MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(utf8Json), written));
#endif

                if (valueArray is object) { ArrayPool<byte>.Shared.Return(valueArray); }
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

        public override void Serialize(ref JsonWriter<char> writer, SpanJsonDynamicArray<char> value, IJsonFormatterResolver<char> resolver)
        {
            if (value is null) { writer.WriteUtf16Null(); return; }

            if (value.TryGetRaw(out ArraySegment<char> rawJson))
            {
                writer.WriteUtf16Verbatim(rawJson);
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
