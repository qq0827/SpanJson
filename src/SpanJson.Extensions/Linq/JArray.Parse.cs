using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using SpanJson.Document;
using SpanJson.Internal;

namespace SpanJson.Linq
{
    partial class JArray
    {
        #region -- Utf8 --

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JArray Parse(byte[] utf8Json)
        {
            var jsonReader = new JsonReader<byte>(utf8Json);
            return Load(ref jsonReader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JArray Parse(in ArraySegment<byte> utf8Json)
        {
            if (utf8Json.IsEmpty()) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.utf8Json); }

            var jsonReader = new JsonReader<byte>(utf8Json);
            return Load(ref jsonReader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JArray Parse(in ReadOnlySpan<byte> utf8Json)
        {
            if (utf8Json.IsEmpty) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.utf8Json); }

            var jsonReader = new JsonReader<byte>(utf8Json);
            return Load(ref jsonReader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JArray Parse(in ReadOnlyMemory<byte> utf8Json)
        {
            if (utf8Json.IsEmpty) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.utf8Json); }

            var jsonReader = new JsonReader<byte>(utf8Json);
            return Load(ref jsonReader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JArray Parse(in ReadOnlySequence<byte> utf8Json)
        {
            using (var doc = JsonDocument.Parse(utf8Json, options: JsonDocumentOptions.CreateDefault()))
            {
                var token = FromElement(doc.RootElement.Clone());
                return AsJArray(token);
            }
        }

        public new static JArray Load(ref JsonReader<byte> reader)
        {
            reader.EnsureUtf8InnerBufferCreated();

            var nextToken = reader.ReadUtf8NextToken();

            switch (nextToken)
            {
                case JsonTokenType.BeginArray:
                    return (JArray)ParseCore(ref reader, 0);

                case JsonTokenType.Null:
                    return null;

                case JsonTokenType.None:
                default:
                    throw ThrowHelper2.GetJsonReaderException_Error_reading_JArray_from_JsonReader();
            }
        }

        #endregion

        #region -- Utf16 --

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JArray Parse(string utf16Json)
        {
            if (utf16Json is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.utf16Json); }

            return Parse(utf16Json.ToCharArray());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JArray Parse(char[] utf16Json)
        {
            var jsonReader = new JsonReader<char>(utf16Json);
            return Load(ref jsonReader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JArray Parse(ArraySegment<char> utf16Json)
        {
            if (utf16Json.IsEmpty()) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.utf16Json); }

            var jsonReader = new JsonReader<char>(utf16Json);
            return Load(ref jsonReader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JArray Parse(in ReadOnlyMemory<char> utf16Json)
        {
            if (utf16Json.IsEmpty) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.utf16Json); }

            var jsonReader = new JsonReader<char>(utf16Json);
            return Load(ref jsonReader);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JArray Parse(in ReadOnlySpan<char> utf16Json)
        {
            if (utf16Json.IsEmpty) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.utf16Json); }

            var jsonReader = new JsonReader<char>(utf16Json);
            return Load(ref jsonReader);
        }

        public new static JArray Load(ref JsonReader<char> reader)
        {
            reader.EnsureUtf16InnerBufferCreated();

            var nextToken = reader.ReadUtf16NextToken();

            switch (nextToken)
            {
                case JsonTokenType.BeginArray:
                    return (JArray)ParseCore(ref reader, 0);

                case JsonTokenType.Null:
                    return null;

                case JsonTokenType.None:
                default:
                    throw ThrowHelper2.GetJsonReaderException_Error_reading_JArray_from_JsonReader();
            }
        }

        #endregion
    }
}
