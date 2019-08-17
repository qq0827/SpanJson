using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using SpanJson.Document;
using SpanJson.Internal;

namespace SpanJson.Linq
{
    partial class JObject
    {
        #region -- Utf8 --

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JObject Parse(byte[] utf8Json)
        {
            var jsonReader = new JsonReader<byte>(utf8Json);
            return Load(ref jsonReader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JObject Parse(in ArraySegment<byte> utf8Json)
        {
            if (utf8Json.IsEmpty()) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.utf8Json); }

            var jsonReader = new JsonReader<byte>(utf8Json);
            return Load(ref jsonReader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JObject Parse(in ReadOnlySpan<byte> utf8Json)
        {
            if (utf8Json.IsEmpty) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.utf8Json); }

            var jsonReader = new JsonReader<byte>(utf8Json);
            return Load(ref jsonReader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JObject Parse(in ReadOnlyMemory<byte> utf8Json)
        {
            if (utf8Json.IsEmpty) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.utf8Json); }

            var jsonReader = new JsonReader<byte>(utf8Json);
            return Load(ref jsonReader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JObject Parse(in ReadOnlySequence<byte> utf8Json)
        {
            using (var doc = JsonDocument.Parse(utf8Json, options: JsonDocumentOptions.CreateDefault()))
            {
                var token = FromElement(doc.RootElement.Clone());
                return AsObject(token);
            }
        }

        public new static JObject Load(ref JsonReader<byte> reader)
        {
            reader.EnsureUtf8InnerBufferCreated();

            var nextToken = reader.ReadUtf8NextToken();

            switch (nextToken)
            {
                case JsonTokenType.BeginObject:
                    return (JObject)ParseCore(ref reader, 0);

                case JsonTokenType.Null:
                    return null;

                case JsonTokenType.None:
                default:
                    throw ThrowHelper2.GetJsonReaderException_Error_reading_JObject_from_JsonReader();
            }
        }

        #endregion

        #region -- Utf16 --

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JObject Parse(string utf16Json)
        {
            if (utf16Json is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.utf16Json); }

            return Parse(utf16Json.ToCharArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JObject Parse(char[] utf16Json)
        {
            var jsonReader = new JsonReader<char>(utf16Json);
            return Load(ref jsonReader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JObject Parse(ArraySegment<char> utf16Json)
        {
            if (utf16Json.IsEmpty()) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.utf16Json); }

            var jsonReader = new JsonReader<char>(utf16Json);
            return Load(ref jsonReader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JObject Parse(in ReadOnlyMemory<char> utf16Json)
        {
            if (utf16Json.IsEmpty) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.utf16Json); }

            var jsonReader = new JsonReader<char>(utf16Json);
            return Load(ref jsonReader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JObject Parse(in ReadOnlySpan<char> utf16Json)
        {
            if (utf16Json.IsEmpty) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.utf16Json); }

            var jsonReader = new JsonReader<char>(utf16Json);
            return Load(ref jsonReader);
        }

        public new static JObject Load(ref JsonReader<char> reader)
        {
            reader.EnsureUtf16InnerBufferCreated();

            var nextToken = reader.ReadUtf16NextToken();

            switch (nextToken)
            {
                case JsonTokenType.BeginObject:
                    return (JObject)ParseCore(ref reader, 0);

                case JsonTokenType.Null:
                    return null;

                case JsonTokenType.None:
                default:
                    throw ThrowHelper2.GetJsonReaderException_Error_reading_JObject_from_JsonReader();
            }
        }

        #endregion
    }
}
