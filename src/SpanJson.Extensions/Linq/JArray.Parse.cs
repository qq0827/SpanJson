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
            var token = ParseCore(ref jsonReader, 0);
            return AsJArray(token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JArray Parse(in ArraySegment<byte> utf8Json)
        {
            if (utf8Json.IsEmpty()) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.utf8Json); }

            var jsonReader = new JsonReader<byte>(utf8Json);
            var token = ParseCore(ref jsonReader, 0);
            return AsJArray(token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JArray Parse(in ReadOnlySpan<byte> utf8Json)
        {
            if (utf8Json.IsEmpty) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.utf8Json); }

            var jsonReader = new JsonReader<byte>(utf8Json);
            var token = ParseCore(ref jsonReader, 0);
            return AsJArray(token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JArray Parse(in ReadOnlyMemory<byte> utf8Json)
        {
            if (utf8Json.IsEmpty) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.utf8Json); }

            var jsonReader = new JsonReader<byte>(utf8Json);
            var token = ParseCore(ref jsonReader, 0);
            return AsJArray(token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JArray Parse(in ReadOnlySequence<byte> utf8Json)
        {
            var doc = JsonDocument.Parse(utf8Json, options: JsonDocumentOptions.CreateDefault(), useArrayPools: false);
            var token = FromElement(doc.RootElement);
            return AsJArray(token);
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
            var token = ParseCore(ref jsonReader, 0);
            return AsJArray(token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JArray Parse(ArraySegment<char> utf16Json)
        {
            if (utf16Json.IsEmpty()) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.utf16Json); }

            var jsonReader = new JsonReader<char>(utf16Json);
            var token = ParseCore(ref jsonReader, 0);
            return AsJArray(token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JArray Parse(in ReadOnlyMemory<char> utf16Json)
        {
            if (utf16Json.IsEmpty) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.utf16Json); }

            var jsonReader = new JsonReader<char>(utf16Json);
            var token = ParseCore(ref jsonReader, 0);
            return AsJArray(token);
        }

        public new static JArray Parse(in ReadOnlySpan<char> utf16Json)
        {
            if (utf16Json.IsEmpty) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.utf16Json); }

            var jsonReader = new JsonReader<char>(utf16Json);
            var token = ParseCore(ref jsonReader, 0);
            return AsJArray(token);
        }

        #endregion

        #region ** AsJArray **

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static JArray AsJArray(JToken token)
        {
            if (token is null || token.Type != JTokenType.Array)
            {
                ThrowHelper2.ThrowJsonReaderException_Error_reading_JArray_from_JsonReader();
            }
            return (JArray)(token);
        }

        #endregion
    }
}
