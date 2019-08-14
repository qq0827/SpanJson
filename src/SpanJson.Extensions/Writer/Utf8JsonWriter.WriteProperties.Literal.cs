// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SpanJson.Internal;

namespace SpanJson
{
    partial struct Utf8JsonWriter
    {
        /// <summary>
        /// Writes the pre-encoded property name and the JSON literal "null" as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="propertyName">The JSON-encoded name of the property to write.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        public void WriteNull(in JsonEncodedText propertyName)
        {
            WriteLiteralHelper(propertyName.EncodedUtf8Bytes, JsonUtf8Constant.NullValue);
            _tokenType = JsonTokenType.Null;
        }

        private void WriteLiteralHelper(in ReadOnlySpan<byte> utf8PropertyName, in ReadOnlySpan<byte> value)
        {
            Debug.Assert(utf8PropertyName.Length <= JsonSharedConstant.MaxUnescapedTokenSize);

            WriteLiteralByOptions(utf8PropertyName, value);

            SetFlagToAddListSeparatorBeforeNextItem();
        }

        /// <summary>
        /// Writes the property name and the JSON literal "null" as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
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
        public void WriteNull(string propertyName)
        {
            if (propertyName is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.propertyName); }
            WriteNull(propertyName.AsSpan());
        }

        /// <summary>
        /// Writes the property name and the JSON literal "null" as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteNull(in ReadOnlySpan<char> propertyName)
        {
            JsonWriterHelper.ValidateProperty(propertyName);

            ReadOnlySpan<byte> span = JsonUtf8Constant.NullValue;

            WriteLiteralEscape(propertyName, span);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = JsonTokenType.Null;
        }

        /// <summary>
        /// Writes the property name and the JSON literal "null" as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="utf8PropertyName">The UTF-8 encoded name of the property to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteNull(in ReadOnlySpan<byte> utf8PropertyName)
        {
            JsonWriterHelper.ValidateProperty(utf8PropertyName);

            ReadOnlySpan<byte> span = JsonUtf8Constant.NullValue;

            WriteLiteralEscape(utf8PropertyName, span);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = JsonTokenType.Null;
        }

        /// <summary>
        /// Writes the pre-encoded property name and <see cref="bool"/> value (as a JSON literal "true" or "false") as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="propertyName">The JSON-encoded name of the property to write.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        public void WriteBoolean(in JsonEncodedText propertyName, bool value)
        {
            if (value)
            {
                WriteLiteralHelper(propertyName.EncodedUtf8Bytes, JsonUtf8Constant.TrueValue);
                _tokenType = JsonTokenType.True;
            }
            else
            {
                WriteLiteralHelper(propertyName.EncodedUtf8Bytes, JsonUtf8Constant.FalseValue);
                _tokenType = JsonTokenType.False;
            }
        }

        /// <summary>
        /// Writes the property name and <see cref="bool"/> value (as a JSON literal "true" or "false") as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <param name="value">The value to write.</param>
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
        public void WriteBoolean(string propertyName, bool value)
        {
            if (propertyName is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.propertyName); }
            WriteBoolean(propertyName.AsSpan(), value);
        }

        /// <summary>
        /// Writes the property name and <see cref="bool"/> value (as a JSON literal "true" or "false") as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteBoolean(in ReadOnlySpan<char> propertyName, bool value)
        {
            JsonWriterHelper.ValidateProperty(propertyName);

            ReadOnlySpan<byte> span = value ? JsonUtf8Constant.TrueValue : JsonUtf8Constant.FalseValue;

            WriteLiteralEscape(propertyName, span);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = value ? JsonTokenType.True : JsonTokenType.False;
        }

        /// <summary>
        /// Writes the property name and <see cref="bool"/> value (as a JSON literal "true" or "false") as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="utf8PropertyName">The UTF-8 encoded name of the property to write.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteBoolean(in ReadOnlySpan<byte> utf8PropertyName, bool value)
        {
            JsonWriterHelper.ValidateProperty(utf8PropertyName);

            ReadOnlySpan<byte> span = value ? JsonUtf8Constant.TrueValue : JsonUtf8Constant.FalseValue;

            WriteLiteralEscape(utf8PropertyName, span);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = value ? JsonTokenType.True : JsonTokenType.False;
        }

        private void WriteLiteralEscape(in ReadOnlySpan<char> propertyName, in ReadOnlySpan<byte> value)
        {
            int propertyIdx = EscapingHelper.NeedsEscaping(propertyName, _options.EscapeHandling, _options.Encoder);

            Debug.Assert(propertyIdx >= -1 && propertyIdx < propertyName.Length);

            if (propertyIdx != -1)
            {
                WriteLiteralEscapeProperty(propertyName, value, propertyIdx);
            }
            else
            {
                WriteLiteralByOptions(propertyName, value);
            }
        }

        private void WriteLiteralEscape(in ReadOnlySpan<byte> utf8PropertyName, in ReadOnlySpan<byte> value)
        {
            int propertyIdx = EscapingHelper.NeedsEscaping(utf8PropertyName, _options.EscapeHandling, _options.Encoder);

            Debug.Assert(propertyIdx >= -1 && propertyIdx < utf8PropertyName.Length);

            if (propertyIdx != -1)
            {
                WriteLiteralEscapeProperty(utf8PropertyName, value, propertyIdx);
            }
            else
            {
                WriteLiteralByOptions(utf8PropertyName, value);
            }
        }

        private void WriteLiteralEscapeProperty(in ReadOnlySpan<char> propertyName, in ReadOnlySpan<byte> value, int firstEscapeIndexProp)
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
            WriteLiteralByOptions(MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(escapedPropertyName), written), value);
#else
            unsafe
            {
                WriteLiteralByOptions(new ReadOnlySpan<char>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(escapedPropertyName)), written), value);
            }
#endif

            if (propertyArray is object)
            {
                ArrayPool<char>.Shared.Return(propertyArray);
            }
        }

        private void WriteLiteralEscapeProperty(in ReadOnlySpan<byte> utf8PropertyName, in ReadOnlySpan<byte> value, int firstEscapeIndexProp)
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
            WriteLiteralByOptions(MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(escapedPropertyName), written), value);
#else
            unsafe
            {
                WriteLiteralByOptions(new ReadOnlySpan<byte>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(escapedPropertyName)), written), value);
            }
#endif

            if (propertyArray is object)
            {
                ArrayPool<byte>.Shared.Return(propertyArray);
            }
        }

        private void WriteLiteralByOptions(in ReadOnlySpan<char> propertyName, in ReadOnlySpan<byte> value)
        {
            ValidateWritingProperty();
            if (_options.Indented)
            {
                WriteLiteralIndented(propertyName, value);
            }
            else
            {
                WriteLiteralMinimized(propertyName, value);
            }
        }

        private void WriteLiteralByOptions(in ReadOnlySpan<byte> utf8PropertyName, in ReadOnlySpan<byte> value)
        {
            ValidateWritingProperty();
            if (_options.Indented)
            {
                WriteLiteralIndented(utf8PropertyName, value);
            }
            else
            {
                WriteLiteralMinimized(utf8PropertyName, value);
            }
        }

        private void WriteLiteralMinimized(in ReadOnlySpan<char> escapedPropertyName, in ReadOnlySpan<byte> value)
        {
            int valueLen = value.Length;
            Debug.Assert(valueLen <= JsonSharedConstant.MaxUnescapedTokenSize);
            Debug.Assert(escapedPropertyName.Length < (int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileTranscoding) - valueLen - 4);

            // All ASCII, 2 quotes for property name, and 1 colon => escapedPropertyName.Length + value.Length + 3
            // Optionally, 1 list separator, and up to 3x growth when transcoding
            int maxRequired = (escapedPropertyName.Length * JsonSharedConstant.MaxExpansionFactorWhileTranscoding) + valueLen + 4;

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

            BinaryUtil.CopyMemory(ref MemoryMarshal.GetReference(value), ref Unsafe.Add(ref output, pos), valueLen);
            pos += valueLen;
        }

        private void WriteLiteralMinimized(in ReadOnlySpan<byte> escapedPropertyName, in ReadOnlySpan<byte> value)
        {
            int nameLen = escapedPropertyName.Length;
            int valueLen = value.Length;
            Debug.Assert(valueLen <= JsonSharedConstant.MaxUnescapedTokenSize);
            Debug.Assert(nameLen < int.MaxValue - valueLen - 4);

            int minRequired = nameLen + valueLen + 3; // 2 quotes for property name, and 1 colon
            int maxRequired = minRequired + 1; // Optionally, 1 list separator

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

            BinaryUtil.CopyMemory(ref MemoryMarshal.GetReference(value), ref Unsafe.Add(ref output, pos), valueLen);
            pos += valueLen;
        }

        private void WriteLiteralIndented(in ReadOnlySpan<char> escapedPropertyName, in ReadOnlySpan<byte> value)
        {
            int indent = Indentation;
            Debug.Assert(indent <= 2 * JsonSharedConstant.MaxWriterDepth);

            int valueLen = value.Length;
            Debug.Assert(valueLen <= JsonSharedConstant.MaxUnescapedTokenSize);
            Debug.Assert(escapedPropertyName.Length < (int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileTranscoding) - indent - valueLen - 5 - JsonWriterHelper.NewLineLength);

            // All ASCII, 2 quotes for property name, 1 colon, and 1 space => escapedPropertyName.Length + value.Length + 4
            // Optionally, 1 list separator, 1-2 bytes for new line, and up to 3x growth when transcoding
            int maxRequired = indent + (escapedPropertyName.Length * JsonSharedConstant.MaxExpansionFactorWhileTranscoding) + valueLen + 5 + JsonWriterHelper.NewLineLength;

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

            BinaryUtil.CopyMemory(ref MemoryMarshal.GetReference(value), ref Unsafe.Add(ref output, pos), valueLen);
            pos += valueLen;
        }

        private void WriteLiteralIndented(in ReadOnlySpan<byte> escapedPropertyName, in ReadOnlySpan<byte> value)
        {
            int indent = Indentation;
            Debug.Assert(indent <= 2 * JsonSharedConstant.MaxWriterDepth);

            int nameLen = escapedPropertyName.Length;
            int valueLen = value.Length;
            Debug.Assert(valueLen <= JsonSharedConstant.MaxUnescapedTokenSize);
            Debug.Assert(nameLen < int.MaxValue - indent - valueLen - 5 - JsonWriterHelper.NewLineLength);

            int minRequired = indent + nameLen + valueLen + 4; // 2 quotes for property name, 1 colon, and 1 space
            int maxRequired = minRequired + 1 + JsonWriterHelper.NewLineLength; // Optionally, 1 list separator and 1-2 bytes for new line

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

            BinaryUtil.CopyMemory(ref MemoryMarshal.GetReference(value), ref Unsafe.Add(ref output, pos), valueLen);
            pos += valueLen;
        }
    }
}
