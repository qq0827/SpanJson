using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SpanJson.Dynamic;
using SpanJson.Internal;

namespace SpanJson.Formatters
{
    public sealed class DynamicObjectFormatter : DynamicFormatterBase<SpanJsonDynamicObject>
    {
        public static readonly DynamicObjectFormatter Default = new DynamicObjectFormatter();

        public override void Serialize(ref JsonWriter<byte> writer, SpanJsonDynamicObject value, IJsonFormatterResolver<byte> resolver)
        {
            if (value is null) { writer.WriteUtf8Null(); return; }

            if (value.HasRaw)
            {
                if (value.IsUtf16)
                {
                    ReadOnlySpan<char> utf16Json = value.Utf16Raw;
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
                    writer.WriteUtf8Verbatim(value.Utf8Raw);
                }
            }
            else
            {
                var dict = (IDictionary<string, object>)(dynamic)value;
                var valueLength = dict.Count;

                writer.IncrementDepth();
                writer.WriteUtf8BeginObject();

                if (valueLength > 0)
                {
                    var counter = 0;
                    foreach (var item in dict)
                    {
                        writer.IncrementDepth();
                        writer.WriteUtf8Name(resolver.GetEncodedPropertyName(item.Key));
                        WriteComplexElement(ref writer, item.Value, resolver);
                        writer.DecrementDepth();
                        if (counter++ < valueLength - 1)
                        {
                            writer.WriteUtf8ValueSeparator();
                        }
                    }
                }

                writer.DecrementDepth();
                writer.WriteUtf8EndObject();
            }
        }

        public override void Serialize(ref JsonWriter<char> writer, SpanJsonDynamicObject value, IJsonFormatterResolver<char> resolver)
        {
            if (value is null) { writer.WriteUtf16Null(); return; }

            if (value.HasRaw)
            {
                if (value.IsUtf16)
                {
                    writer.WriteUtf16Verbatim(value.Utf16Raw);
                }
                else
                {
                    ReadOnlySpan<byte> utf8Json = value.Utf8Raw;
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
            }
            else
            {
                var dict = (IDictionary<string, object>)(dynamic)value;
                var valueLength = dict.Count;

                writer.IncrementDepth();
                writer.WriteUtf16BeginObject();

                if (valueLength > 0)
                {
                    var counter = 0;
                    foreach (var item in dict)
                    {
                        writer.IncrementDepth();
                        writer.WriteUtf16Name(resolver.GetEncodedPropertyName(item.Key));
                        WriteComplexElement(ref writer, item.Value, resolver);
                        writer.DecrementDepth();
                        if (counter++ < valueLength - 1)
                        {
                            writer.WriteUtf16ValueSeparator();
                        }
                    }
                }

                writer.DecrementDepth();
                writer.WriteUtf16EndObject();
            }
        }
    }
}
