// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SpanJson.Internal;

namespace SpanJson
{
    partial struct Utf8JsonWriter
    {
        /// <summary>
        /// Writes the pre-encoded property name and raw bytes value (as a base 64 encoded JSON string) as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="propertyName">The JSON-encoded name of the property to write.</param>
        /// <param name="bytes">The Base64-encoded data to write.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        public void WriteBase64String(in JsonEncodedText propertyName, in ReadOnlySpan<byte> bytes)
            => WriteBase64StringHelper(propertyName.EncodedUtf8Bytes, bytes);

        private void WriteBase64StringHelper(in ReadOnlySpan<byte> utf8PropertyName, in ReadOnlySpan<byte> bytes)
        {
            Debug.Assert(utf8PropertyName.Length <= JsonSharedConstant.MaxUnescapedTokenSize);

            JsonWriterHelper.ValidateBytes(bytes);

            WriteBase64ByOptions(utf8PropertyName, bytes);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = JsonTokenType.String;
        }

        /// <summary>
        /// Writes the property name and raw bytes value (as a Base64 encoded JSON string) as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <param name="bytes">The Base64-encoded data to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="propertyName"/> parameter is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteBase64String(string propertyName, in ReadOnlySpan<byte> bytes)
        {
            if (propertyName is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.propertyName); }
            WriteBase64String(propertyName.AsSpan(), bytes);
        }

        /// <summary>
        /// Writes the property name and raw bytes value (as a base 64 encoded JSON string) as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <param name="bytes">The Base64-encoded data to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteBase64String(in ReadOnlySpan<char> propertyName, in ReadOnlySpan<byte> bytes)
        {
            JsonWriterHelper.ValidatePropertyAndBytes(propertyName, bytes);

            WriteBase64Escape(propertyName, bytes);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = JsonTokenType.String;
        }

        /// <summary>
        /// Writes the property name and raw bytes value (as a base 64 encoded JSON string) as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="utf8PropertyName">The UTF-8 encoded name of the property to write.</param>
        /// <param name="bytes">The Base64-encoded data to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteBase64String(in ReadOnlySpan<byte> utf8PropertyName, in ReadOnlySpan<byte> bytes)
        {
            JsonWriterHelper.ValidatePropertyAndBytes(utf8PropertyName, bytes);

            WriteBase64Escape(utf8PropertyName, bytes);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = JsonTokenType.String;
        }

        private void WriteBase64Escape(in ReadOnlySpan<char> propertyName, in ReadOnlySpan<byte> bytes)
        {
            int propertyIdx = EscapingHelper.NeedsEscaping(propertyName, _options.EscapeHandling, _options.Encoder);

            Debug.Assert(propertyIdx >= -1 && propertyIdx < propertyName.Length);

            if (propertyIdx != -1)
            {
                WriteBase64EscapeProperty(propertyName, bytes, propertyIdx);
            }
            else
            {
                WriteBase64ByOptions(propertyName, bytes);
            }
        }

        private void WriteBase64Escape(in ReadOnlySpan<byte> utf8PropertyName, in ReadOnlySpan<byte> bytes)
        {
            int propertyIdx = EscapingHelper.NeedsEscaping(utf8PropertyName, _options.EscapeHandling, _options.Encoder);

            Debug.Assert(propertyIdx >= -1 && propertyIdx < utf8PropertyName.Length);

            if (propertyIdx != -1)
            {
                WriteBase64EscapeProperty(utf8PropertyName, bytes, propertyIdx);
            }
            else
            {
                WriteBase64ByOptions(utf8PropertyName, bytes);
            }
        }

        private void WriteBase64EscapeProperty(in ReadOnlySpan<char> propertyName, in ReadOnlySpan<byte> bytes, int firstEscapeIndexProp)
        {
            Debug.Assert(int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileEscaping >= propertyName.Length);
            Debug.Assert(firstEscapeIndexProp >= 0 && firstEscapeIndexProp < propertyName.Length);

            char[] propertyArray = null;

            int length = EscapingHelper.GetMaxEscapedLength(propertyName.Length, firstEscapeIndexProp);

            Span<char> escapedPropertyName = (uint)length <= JsonSharedConstant.StackallocThreshold ?
                stackalloc char[length] :
                (propertyArray = ArrayPool<char>.Shared.Rent(length));

            EscapingHelper.EscapeString(propertyName, escapedPropertyName, _options.EscapeHandling, firstEscapeIndexProp, _options.Encoder, out int written);

#if NETCOREAPP
            WriteBase64ByOptions(MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(escapedPropertyName), written), bytes);
#else
            unsafe
            {
                WriteBase64ByOptions(new ReadOnlySpan<char>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(escapedPropertyName)), written), bytes);
            }
#endif

            if (propertyArray is object)
            {
                ArrayPool<char>.Shared.Return(propertyArray);
            }
        }

        private void WriteBase64EscapeProperty(in ReadOnlySpan<byte> utf8PropertyName, in ReadOnlySpan<byte> bytes, int firstEscapeIndexProp)
        {
            Debug.Assert(int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileEscaping >= utf8PropertyName.Length);
            Debug.Assert(firstEscapeIndexProp >= 0 && firstEscapeIndexProp < utf8PropertyName.Length);

            byte[] propertyArray = null;

            int length = EscapingHelper.GetMaxEscapedLength(utf8PropertyName.Length, firstEscapeIndexProp);

            Span<byte> escapedPropertyName = (uint)length <= JsonSharedConstant.StackallocThreshold ?
                stackalloc byte[length] :
                (propertyArray = ArrayPool<byte>.Shared.Rent(length));

            EscapingHelper.EscapeString(utf8PropertyName, escapedPropertyName, _options.EscapeHandling, firstEscapeIndexProp, _options.Encoder, out int written);

#if NETCOREAPP
            WriteBase64ByOptions(MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(escapedPropertyName), written), bytes);
#else
            unsafe
            {
                WriteBase64ByOptions(new ReadOnlySpan<byte>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(escapedPropertyName)), written), bytes);
            }
#endif

            if (propertyArray is object)
            {
                ArrayPool<byte>.Shared.Return(propertyArray);
            }
        }

        private void WriteBase64ByOptions(in ReadOnlySpan<char> propertyName, in ReadOnlySpan<byte> bytes)
        {
            ValidateWritingProperty();
            if (_options.Indented)
            {
                WriteBase64Indented(propertyName, bytes);
            }
            else
            {
                WriteBase64Minimized(propertyName, bytes);
            }
        }

        private void WriteBase64ByOptions(in ReadOnlySpan<byte> utf8PropertyName, in ReadOnlySpan<byte> bytes)
        {
            ValidateWritingProperty();
            if (_options.Indented)
            {
                WriteBase64Indented(utf8PropertyName, bytes);
            }
            else
            {
                WriteBase64Minimized(utf8PropertyName, bytes);
            }
        }

        private void WriteBase64Minimized(in ReadOnlySpan<char> escapedPropertyName, in ReadOnlySpan<byte> bytes)
        {
            int encodedLength = Base64.GetMaxEncodedToUtf8Length(bytes.Length);
            int nameLen = escapedPropertyName.Length;

            Debug.Assert(nameLen * JsonSharedConstant.MaxExpansionFactorWhileTranscoding < int.MaxValue - encodedLength - 6);

            // All ASCII, 2 quotes for property name, 2 quotes to surround the base-64 encoded string value, and 1 colon => escapedPropertyName.Length + encodedLength + 5
            // Optionally, 1 list separator, and up to 3x growth when transcoding.
            int maxRequired = (nameLen * JsonSharedConstant.MaxExpansionFactorWhileTranscoding) + encodedLength + 6;

            ref var pos = ref _pos;
            EnsureUnsafe(pos, maxRequired);

            ref byte output = ref PinnableAddress;

            if ((uint)_currentDepth > JsonSharedConstant.TooBigOrNegative)
            {
                Unsafe.Add(ref output, pos++) = JsonUtf8Constant.ListSeparator;
            }
            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;

            TranscodeAndWrite(escapedPropertyName, ref output, FreeCapacity, ref pos);

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.KeyValueSeperator;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;

            Base64EncodeAndWrite(bytes, ref output, encodedLength, ref pos);

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
        }

        private void WriteBase64Minimized(in ReadOnlySpan<byte> escapedPropertyName, in ReadOnlySpan<byte> bytes)
        {
            int encodedLength = Base64.GetMaxEncodedToUtf8Length(bytes.Length);

            int nameLen = escapedPropertyName.Length;
            Debug.Assert(nameLen < int.MaxValue - encodedLength - 6);

            // 2 quotes for property name, 2 quotes to surround the base-64 encoded string value, and 1 colon => escapedPropertyName.Length + encodedLength + 5
            // Optionally, 1 list separator.
            int maxRequired = nameLen + encodedLength + 6;

            ref var pos = ref _pos;
            EnsureUnsafe(pos, maxRequired);

            ref byte output = ref PinnableAddress;

            if ((uint)_currentDepth > JsonSharedConstant.TooBigOrNegative)
            {
                Unsafe.Add(ref output, pos++) = JsonUtf8Constant.ListSeparator;
            }
            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;

            BinaryUtil.CopyMemory(ref MemoryMarshal.GetReference(escapedPropertyName), ref Unsafe.Add(ref output, pos), nameLen);
            pos += nameLen;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.KeyValueSeperator;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;

            Base64EncodeAndWrite(bytes, ref output, encodedLength, ref pos);

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
        }

        private void WriteBase64Indented(in ReadOnlySpan<char> escapedPropertyName, in ReadOnlySpan<byte> bytes)
        {
            int indent = Indentation;
            Debug.Assert(indent <= 2 * JsonSharedConstant.MaxWriterDepth);

            int encodedLength = Base64.GetMaxEncodedToUtf8Length(bytes.Length);
            int nameLen = escapedPropertyName.Length;

            Debug.Assert(nameLen * JsonSharedConstant.MaxExpansionFactorWhileTranscoding < int.MaxValue - indent - encodedLength - 7 - JsonWriterHelper.NewLineLength);

            // All ASCII, 2 quotes for property name, 2 quotes to surround the base-64 encoded string value, 1 colon, and 1 space => indent + escapedPropertyName.Length + encodedLength + 6
            // Optionally, 1 list separator, 1-2 bytes for new line, and up to 3x growth when transcoding.
            int maxRequired = indent + (nameLen * JsonSharedConstant.MaxExpansionFactorWhileTranscoding) + encodedLength + 7 + JsonWriterHelper.NewLineLength;

            ref var pos = ref _pos;
            EnsureUnsafe(pos, maxRequired);

            ref byte output = ref PinnableAddress;

            if ((uint)_currentDepth > JsonSharedConstant.TooBigOrNegative)
            {
                Unsafe.Add(ref output, pos++) = JsonUtf8Constant.ListSeparator;
            }

            Debug.Assert(_options.SkipValidation || _tokenType != JsonTokenType.PropertyName);

            if (_tokenType != JsonTokenType.None)
            {
                WriteNewLine(ref output, ref pos);
            }

            JsonWriterHelper.WriteIndentation(ref output, indent, ref pos);

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;

            TranscodeAndWrite(escapedPropertyName, ref output, FreeCapacity, ref pos);

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.KeyValueSeperator;
            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.Space;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;

            Base64EncodeAndWrite(bytes, ref output, encodedLength, ref pos);

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
        }

        private void WriteBase64Indented(in ReadOnlySpan<byte> escapedPropertyName, in ReadOnlySpan<byte> bytes)
        {
            int indent = Indentation;
            Debug.Assert(indent <= 2 * JsonSharedConstant.MaxWriterDepth);

            int encodedLength = Base64.GetMaxEncodedToUtf8Length(bytes.Length);
            int nameLen = escapedPropertyName.Length;

            Debug.Assert(nameLen < int.MaxValue - indent - encodedLength - 7 - JsonWriterHelper.NewLineLength);

            // 2 quotes for property name, 2 quotes to surround the base-64 encoded string value, 1 colon, and 1 space => indent + escapedPropertyName.Length + encodedLength + 6
            // Optionally, 1 list separator, and 1-2 bytes for new line.
            int maxRequired = indent + nameLen + encodedLength + 7 + JsonWriterHelper.NewLineLength;

            ref var pos = ref _pos;
            EnsureUnsafe(pos, maxRequired);

            ref byte output = ref PinnableAddress;

            if ((uint)_currentDepth > JsonSharedConstant.TooBigOrNegative)
            {
                Unsafe.Add(ref output, pos++) = JsonUtf8Constant.ListSeparator;
            }

            Debug.Assert(_options.SkipValidation || _tokenType != JsonTokenType.PropertyName);

            if (_tokenType != JsonTokenType.None)
            {
                WriteNewLine(ref output, ref pos);
            }

            JsonWriterHelper.WriteIndentation(ref output, indent, ref pos);

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;

            BinaryUtil.CopyMemory(ref MemoryMarshal.GetReference(escapedPropertyName), ref Unsafe.Add(ref output, pos), nameLen);
            pos += nameLen;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.KeyValueSeperator;
            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.Space;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;

            Base64EncodeAndWrite(bytes, ref output, encodedLength, ref pos);

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
        }
    }
}
