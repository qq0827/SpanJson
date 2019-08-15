using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SpanJson.Document;
using SpanJson.Dynamic;
using SpanJson.Helpers;
using SpanJson.Internal;

namespace SpanJson.Linq
{
    partial class JToken
    {
        #region -- Utf8 --

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JToken Parse(byte[] utf8Json)
        {
            var jsonReader = new JsonReader<byte>(utf8Json);
            return ParseCore(ref jsonReader, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JToken Parse(in ArraySegment<byte> utf8Json)
        {
            if (utf8Json.IsEmpty()) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.utf8Json); }

            var jsonReader = new JsonReader<byte>(utf8Json);
            return ParseCore(ref jsonReader, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JToken Parse(in ReadOnlySpan<byte> utf8Json)
        {
            if (utf8Json.IsEmpty) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.utf8Json); }

            var jsonReader = new JsonReader<byte>(utf8Json);
            return ParseCore(ref jsonReader, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JToken Parse(in ReadOnlyMemory<byte> utf8Json)
        {
            if (utf8Json.IsEmpty) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.utf8Json); }

            var jsonReader = new JsonReader<byte>(utf8Json);
            return ParseCore(ref jsonReader, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JToken Parse(in ReadOnlySequence<byte> utf8Json)
        {
            var doc = JsonDocument.Parse(utf8Json, options: JsonDocumentOptions.CreateDefault(), useArrayPools: false);
            return FromElement(doc.RootElement);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JToken Load(ref JsonReader<byte> reader)
        {
            return ParseCore(ref reader, 0);
        }

        internal protected static JToken ParseCore(ref JsonReader<byte> reader, int stack)
        {
            ref var pos = ref reader._pos;
            var nextToken = reader.ReadUtf8NextToken();
            if ((uint)stack > JsonSharedConstant.NestingLimit)
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.NestingTooDeep, pos);
            }

            switch (nextToken)
            {
                case JsonTokenType.Null:
                    reader.ReadUtf8Null();
                    return null;

                case JsonTokenType.False:
                case JsonTokenType.True:
                    return reader.ReadUtf8Boolean();

                case JsonTokenType.Number:
                    if (reader._utf8Json.NonEmpty())
                    {
                        return new SpanJsonDynamicUtf8Number(reader.ReadUtf8RawNumber());
                    }
                    else
                    {
                        return new SpanJsonDynamicUtf8Number(reader.ReadUtf8NumberSpan());
                    }

                case JsonTokenType.String:
                    if (reader._utf8Json.NonEmpty())
                    {
                        reader.ReadUtf8StringSpanWithQuotes(ref MemoryMarshal.GetReference(reader._utf8Span), ref pos, reader._length, out ArraySegment<byte> result);
                        return new SpanJsonDynamicUtf8String(result);
                    }
                    else
                    {
                        var span = JsonReader<byte>.ReadUtf8StringSpanWithQuotes(ref MemoryMarshal.GetReference(reader._utf8Span), ref pos, reader._length);
                        return new SpanJsonDynamicUtf8String(span);
                    }

                case JsonTokenType.BeginObject:
                    {
                        var startOffset = pos;
                        pos++;
                        var count = 0;
                        var jObj = new JObject();
                        while (!reader.TryReadUtf8IsEndObjectOrValueSeparator(ref count))
                        {
                            var name = reader.ReadUtf8EscapedName();
                            var token = ParseCore(ref reader, stack + 1);
                            jObj[name] = token; // take last one
                        }

                        var utf8Json = reader._utf8Json;
                        if (utf8Json.NonEmpty())
                        {
                            jObj._dynamicJson = utf8Json.Slice(startOffset, pos - startOffset);
                        }
                        return jObj;
                    }
                case JsonTokenType.BeginArray:
                    {
                        var startOffset = pos;
                        pos++;
                        var count = 0;
                        JToken[] temp = null;
                        try
                        {
                            temp = ArrayPool<JToken>.Shared.Rent(4);
                            while (!reader.TryReadUtf8IsEndArrayOrValueSeparator(ref count))
                            {
                                if (count == temp.Length)
                                {
                                    FormatterUtils.GrowArray(ref temp);
                                }

                                temp[count - 1] = ParseCore(ref reader, stack + 1);
                            }

                            JToken[] result;
                            if (0u >= (uint)count)
                            {
                                result = JsonHelpers.Empty<JToken>();
                            }
                            else
                            {
                                result = FormatterUtils.CopyArray(temp, count);
                            }

                            var jAry = new JArray(result);
                            var utf8Json = reader._utf8Json;
                            if (utf8Json.NonEmpty())
                            {
                                jAry._dynamicJson = utf8Json.Slice(startOffset, pos - startOffset);
                            }
                            return jAry;
                        }
                        finally
                        {
                            if (temp is object)
                            {
                                ArrayPool<JToken>.Shared.Return(temp);
                            }
                        }
                    }
                default:
                    ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.EndOfData, pos);
                    return default;
            }
        }

        #endregion

        #region -- Utf16 --

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JToken Parse(string json)
        {
            if (json is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.json); }

            return Parse(json.ToCharArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JToken Parse(char[] json)
        {
            var jsonReader = new JsonReader<char>(json);
            return ParseCore(ref jsonReader, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JToken Parse(ArraySegment<char> json)
        {
            if (json.IsEmpty()) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.json); }

            var jsonReader = new JsonReader<char>(json);
            return ParseCore(ref jsonReader, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JToken Parse(in ReadOnlyMemory<char> json)
        {
            if (json.IsEmpty) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.json); }

            var jsonReader = new JsonReader<char>(json);
            return ParseCore(ref jsonReader, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JToken Parse(in ReadOnlySpan<char> json)
        {
            if (json.IsEmpty) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.json); }

            var jsonReader = new JsonReader<char>(json);
            return ParseCore(ref jsonReader, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JToken Load(ref JsonReader<char> reader)
        {
            return ParseCore(ref reader, 0);
        }

        internal protected static JToken ParseCore(ref JsonReader<char> reader, int stack)
        {
            ref var pos = ref reader._pos;
            var nextToken = reader.ReadUtf16NextToken();
            if ((uint)stack > JsonSharedConstant.NestingLimit)
            {
                ThrowHelper.ThrowJsonParserException(JsonParserException.ParserError.NestingTooDeep, pos);
            }

            switch (nextToken)
            {
                case JsonTokenType.Null:
                    reader.ReadUtf16Null();
                    return null;

                case JsonTokenType.False:
                case JsonTokenType.True:
                    return reader.ReadUtf16Boolean();

                case JsonTokenType.Number:
                    if (reader._utf16Json.NonEmpty())
                    {
                        return new SpanJsonDynamicUtf16Number(reader.ReadUtf16RawNumber());
                    }
                    else
                    {
                        return new SpanJsonDynamicUtf16Number(reader.ReadUtf16NumberSpan());
                    }

                case JsonTokenType.String:
                    if (reader._utf16Json.NonEmpty())
                    {
                        reader.ReadUtf16StringSpanWithQuotes(ref MemoryMarshal.GetReference(reader._utf16Span), ref pos, reader._length, out ArraySegment<char> result);
                        return new SpanJsonDynamicUtf16String(result);
                    }
                    else
                    {
                        var span = JsonReader<char>.ReadUtf16StringSpanWithQuotes(ref MemoryMarshal.GetReference(reader._utf16Span), ref pos, reader._length);
                        return new SpanJsonDynamicUtf16String(span);
                    }

                case JsonTokenType.BeginObject:
                    {
                        var startOffset = pos;
                        pos++;
                        var count = 0;
                        var jObj = new JObject();
                        while (!reader.TryReadUtf16IsEndObjectOrValueSeparator(ref count))
                        {
                            var name = reader.ReadUtf16EscapedName();
                            var token = ParseCore(ref reader, stack + 1);
                            jObj[name] = token; // take last one
                        }

                        var utf16Json = reader._utf16Json;
                        if (utf16Json.NonEmpty())
                        {
                            jObj._dynamicJson = utf16Json.Slice(startOffset, pos - startOffset);
                        }
                        return jObj;
                    }
                case JsonTokenType.BeginArray:
                    {
                        var startOffset = pos;
                        pos++;
                        var count = 0;
                        JToken[] temp = null;
                        try
                        {
                            temp = ArrayPool<JToken>.Shared.Rent(4);
                            while (!reader.TryReadUtf16IsEndArrayOrValueSeparator(ref count))
                            {
                                if (count == temp.Length)
                                {
                                    FormatterUtils.GrowArray(ref temp);
                                }

                                temp[count - 1] = ParseCore(ref reader, stack + 1);
                            }

                            JToken[] result;
                            if (0u >= (uint)count)
                            {
                                result = JsonHelpers.Empty<JToken>();
                            }
                            else
                            {
                                result = FormatterUtils.CopyArray(temp, count);
                            }

                            var jAry = new JArray(result);
                            var utf16Json = reader._utf16Json;
                            if (utf16Json.NonEmpty())
                            {
                                jAry._dynamicJson = utf16Json.Slice(startOffset, pos - startOffset);
                            }
                            return jAry;
                        }
                        finally
                        {
                            if (temp is object)
                            {
                                ArrayPool<JToken>.Shared.Return(temp);
                            }
                        }
                    }
                default:
                    throw ThrowHelper.GetJsonParserException(JsonParserException.ParserError.EndOfData, pos);
            }
        }

        #endregion
    }
}