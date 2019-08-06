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
        /// Writes the pre-encoded property name (as a JSON string) as the first part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="propertyName">The JSON-encoded name of the property to write.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        public void WritePropertyName(in JsonEncodedText propertyName)
            => WritePropertyNameHelper(propertyName.EncodedUtf8Bytes);

        private void WritePropertyNameHelper(in ReadOnlySpan<byte> utf8PropertyName)
        {
            Debug.Assert(utf8PropertyName.Length <= JsonSharedConstant.MaxUnescapedTokenSize);

            WriteStringByOptionsPropertyName(utf8PropertyName);

            _currentDepth &= JsonSharedConstant.RemoveFlagsBitMask;
            _tokenType = JsonTokenType.PropertyName;
        }

        /// <summary>
        /// Writes the property name (as a JSON string) as the first part of a name/value pair of a JSON object.
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
        public void WritePropertyName(string propertyName)
        {
            if (propertyName == null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.propertyName); }
            WritePropertyName(propertyName.AsSpan());
        }

        /// <summary>
        /// Writes the property name (as a JSON string) as the first part of a name/value pair of a JSON object.
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
        public void WritePropertyName(in ReadOnlySpan<char> propertyName)
        {
            JsonWriterHelper.ValidateProperty(propertyName);

            int propertyIdx = EscapingHelper.NeedsEscaping(propertyName, _options.EscapeHandling, _options.Encoder);

            Debug.Assert(propertyIdx >= -1 && propertyIdx < propertyName.Length && propertyIdx < int.MaxValue / 2);

            if (propertyIdx != -1)
            {
                WriteStringEscapeProperty(propertyName, propertyIdx);
            }
            else
            {
                WriteStringByOptionsPropertyName(propertyName);
            }
            _currentDepth &= JsonSharedConstant.RemoveFlagsBitMask;
            _tokenType = JsonTokenType.PropertyName;
        }

        private void WriteStringEscapeProperty(in ReadOnlySpan<char> propertyName, int firstEscapeIndexProp)
        {
            Debug.Assert(int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileEscaping >= propertyName.Length);

            char[] propertyArray = null;

            if (firstEscapeIndexProp != -1)
            {
                int length = EscapingHelper.GetMaxEscapedLength(propertyName.Length, firstEscapeIndexProp);

                Span<char> escapedPropertyName;
                if ((uint)length > JsonSharedConstant.StackallocThreshold)
                {
                    propertyArray = ArrayPool<char>.Shared.Rent(length);
                    escapedPropertyName = propertyArray;
                }
                else
                {
                    // Cannot create a span directly since it gets assigned to parameter and passed down.
                    unsafe
                    {
                        char* ptr = stackalloc char[length];
                        escapedPropertyName = new Span<char>(ptr, length);
                    }
                }

                EscapingHelper.EscapeString(propertyName, escapedPropertyName, _options.EscapeHandling, firstEscapeIndexProp, _options.Encoder, out int written);
                WriteStringByOptionsPropertyName(escapedPropertyName.Slice(0, written));
            }
            else
            {
                WriteStringByOptionsPropertyName(propertyName);
            }

            if (propertyArray != null)
            {
                ArrayPool<char>.Shared.Return(propertyArray);
            }
        }

        private void WriteStringByOptionsPropertyName(in ReadOnlySpan<char> propertyName)
        {
            ValidateWritingProperty();
            if (_options.Indented)
            {
                WriteStringIndentedPropertyName(propertyName);
            }
            else
            {
                WriteStringMinimizedPropertyName(propertyName);
            }
        }

        private void WriteStringMinimizedPropertyName(in ReadOnlySpan<char> escapedPropertyName)
        {
            Debug.Assert(escapedPropertyName.Length <= JsonSharedConstant.MaxEscapedTokenSize);
            Debug.Assert(escapedPropertyName.Length < (int.MaxValue - 4) / JsonSharedConstant.MaxExpansionFactorWhileTranscoding);

            // All ASCII, 2 quotes for property name, and 1 colon => escapedPropertyName.Length + 3
            // Optionally, 1 list separator, and up to 3x growth when transcoding
            int maxRequired = (escapedPropertyName.Length * JsonSharedConstant.MaxExpansionFactorWhileTranscoding) + 4;

            ref var pos = ref _pos;
            EnsureUnsafe(pos, maxRequired);

            ref byte output = ref PinnableAddress;

            if (_currentDepth < 0)
            {
                Unsafe.Add(ref output, pos++) = JsonUtf8Constant.ListSeparator;
            }
            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;

            TranscodeAndWrite(escapedPropertyName, ref output, FreeCapacity, ref pos);

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.KeyValueSeperator;
        }

        private void WriteStringIndentedPropertyName(in ReadOnlySpan<char> escapedPropertyName)
        {
            int indent = Indentation;
            Debug.Assert(indent <= 2 * JsonSharedConstant.MaxWriterDepth);

            Debug.Assert(escapedPropertyName.Length <= JsonSharedConstant.MaxEscapedTokenSize);
            Debug.Assert(escapedPropertyName.Length < (int.MaxValue - 5 - indent - JsonWriterHelper.NewLineLength) / JsonSharedConstant.MaxExpansionFactorWhileTranscoding);

            // All ASCII, 2 quotes for property name, 1 colon, and 1 space => escapedPropertyName.Length + 4
            // Optionally, 1 list separator, 1-2 bytes for new line, and up to 3x growth when transcoding
            int maxRequired = indent + (escapedPropertyName.Length * JsonSharedConstant.MaxExpansionFactorWhileTranscoding) + 5 + JsonWriterHelper.NewLineLength;

            ref var pos = ref _pos;
            EnsureUnsafe(pos, maxRequired);

            ref byte output = ref PinnableAddress;

            if (_currentDepth < 0)
            {
                Unsafe.Add(ref output, pos++) = JsonUtf8Constant.ListSeparator;
            }

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
        }

        /// <summary>
        /// Writes the UTF-8 property name (as a JSON string) as the first part of a name/value pair of a JSON object.
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
        public void WritePropertyName(in ReadOnlySpan<byte> utf8PropertyName)
        {
            JsonWriterHelper.ValidateProperty(utf8PropertyName);

            int propertyIdx = EscapingHelper.NeedsEscaping(utf8PropertyName, _options.EscapeHandling, _options.Encoder);

            Debug.Assert(propertyIdx >= -1 && propertyIdx < utf8PropertyName.Length && propertyIdx < int.MaxValue / 2);

            if (propertyIdx != -1)
            {
                WriteStringEscapeProperty(utf8PropertyName, propertyIdx);
            }
            else
            {
                WriteStringByOptionsPropertyName(utf8PropertyName);
            }
            _currentDepth &= JsonSharedConstant.RemoveFlagsBitMask;
            _tokenType = JsonTokenType.PropertyName;
        }

        private void WriteStringEscapeProperty(in ReadOnlySpan<byte> utf8PropertyName, int firstEscapeIndexProp)
        {
            Debug.Assert(int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileEscaping >= utf8PropertyName.Length);

            byte[] propertyArray = null;

            if (firstEscapeIndexProp != -1)
            {
                int length = EscapingHelper.GetMaxEscapedLength(utf8PropertyName.Length, firstEscapeIndexProp);

                Span<byte> escapedPropertyName;
                if ((uint)length > JsonSharedConstant.StackallocThreshold)
                {
                    propertyArray = ArrayPool<byte>.Shared.Rent(length);
                    escapedPropertyName = propertyArray;
                }
                else
                {
                    // Cannot create a span directly since it gets assigned to parameter and passed down.
                    unsafe
                    {
                        byte* ptr = stackalloc byte[length];
                        escapedPropertyName = new Span<byte>(ptr, length);
                    }
                }

                EscapingHelper.EscapeString(utf8PropertyName, escapedPropertyName, _options.EscapeHandling, firstEscapeIndexProp, _options.Encoder, out int written);
                WriteStringByOptionsPropertyName(escapedPropertyName.Slice(0, written));
            }
            else
            {
                WriteStringByOptionsPropertyName(utf8PropertyName);
            }

            if (propertyArray != null)
            {
                ArrayPool<byte>.Shared.Return(propertyArray);
            }
        }

        private void WriteStringByOptionsPropertyName(in ReadOnlySpan<byte> utf8PropertyName)
        {
            ValidateWritingProperty();
            if (_options.Indented)
            {
                WriteStringIndentedPropertyName(utf8PropertyName);
            }
            else
            {
                WriteStringMinimizedPropertyName(utf8PropertyName);
            }
        }

        private void WriteStringMinimizedPropertyName(in ReadOnlySpan<byte> escapedPropertyName)
        {
            int nameLen = escapedPropertyName.Length;
            Debug.Assert(nameLen <= JsonSharedConstant.MaxEscapedTokenSize);
            Debug.Assert(nameLen < int.MaxValue - 4);

            int minRequired = nameLen + 3; // 2 quotes for property name, and 1 colon
            int maxRequired = minRequired + 1; // Optionally, 1 list separator

            ref var pos = ref _pos;
            EnsureUnsafe(pos, maxRequired);

            ref byte output = ref PinnableAddress;

            if (_currentDepth < 0)
            {
                Unsafe.Add(ref output, pos++) = JsonUtf8Constant.ListSeparator;
            }
            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;

            BinaryUtil.CopyMemory(ref MemoryMarshal.GetReference(escapedPropertyName), ref Unsafe.Add(ref output, pos), nameLen);
            pos += nameLen;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.KeyValueSeperator;
        }

        private void WriteStringIndentedPropertyName(in ReadOnlySpan<byte> escapedPropertyName)
        {
            int indent = Indentation;
            Debug.Assert(indent <= 2 * JsonSharedConstant.MaxWriterDepth);

            int nameLen = escapedPropertyName.Length;
            Debug.Assert(nameLen <= JsonSharedConstant.MaxEscapedTokenSize);
            Debug.Assert(nameLen < int.MaxValue - indent - 5 - JsonWriterHelper.NewLineLength);

            int minRequired = indent + nameLen + 4; // 2 quotes for property name, 1 colon, and 1 space
            int maxRequired = minRequired + 1 + JsonWriterHelper.NewLineLength; // Optionally, 1 list separator and 1-2 bytes for new line

            ref var pos = ref _pos;
            EnsureUnsafe(pos, maxRequired);

            ref byte output = ref PinnableAddress;

            if (_currentDepth < 0)
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
        }

        /// <summary>
        /// Writes the pre-encoded property name and pre-encoded value (as a JSON string) as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="propertyName">The JSON-encoded name of the property to write.</param>
        /// <param name="value">The JSON-encoded value to write.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        public void WriteString(in JsonEncodedText propertyName, in JsonEncodedText value)
            => WriteStringHelper(propertyName.EncodedUtf8Bytes, value.EncodedUtf8Bytes);

        private void WriteStringHelper(in ReadOnlySpan<byte> utf8PropertyName, in ReadOnlySpan<byte> utf8Value)
        {
            Debug.Assert(utf8PropertyName.Length <= JsonSharedConstant.MaxUnescapedTokenSize && utf8Value.Length <= JsonSharedConstant.MaxUnescapedTokenSize);

            WriteStringByOptions(utf8PropertyName, utf8Value);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = JsonTokenType.String;
        }

        /// <summary>
        /// Writes the property name and pre-encoded value (as a JSON string) as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="propertyName">The JSON-encoded name of the property to write.</param>
        /// <param name="value">The JSON-encoded value to write.</param>
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
        public void WriteString(string propertyName, in JsonEncodedText value)
        {
            if (propertyName == null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.propertyName); }
            WriteString(propertyName.AsSpan(), value);
        }

        /// <summary>
        /// Writes the property name and string text value (as a JSON string) as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name or value is too large.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="propertyName"/> parameter is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// <para>
        /// The property name and value is escaped before writing.
        /// </para>
        /// <para>
        /// If <paramref name="value"/> is <see langword="null"/> the JSON null value is written,
        /// as if <see cref="WriteNull(in System.ReadOnlySpan{byte})"/> were called.
        /// </para>
        /// </remarks>
        public void WriteString(string propertyName, string value)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (value == null)
            {
                WriteNull(propertyName.AsSpan());
            }
            else
            {
                WriteString(propertyName.AsSpan(), value.AsSpan());
            }
        }

        /// <summary>
        /// Writes the property name and text value (as a JSON string) as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name or value is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// The property name and value is escaped before writing.
        /// </remarks>
        public void WriteString(in ReadOnlySpan<char> propertyName, in ReadOnlySpan<char> value)
        {
            JsonWriterHelper.ValidatePropertyAndValue(propertyName, value);

            WriteStringEscape(propertyName, value);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = JsonTokenType.String;
        }

        /// <summary>
        /// Writes the UTF-8 property name and UTF-8 text value (as a JSON string) as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="utf8PropertyName">The UTF-8 encoded name of the property to write.</param>
        /// <param name="utf8Value">The UTF-8 encoded value to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name or value is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// The property name and value is escaped before writing.
        /// </remarks>
        public void WriteString(in ReadOnlySpan<byte> utf8PropertyName, in ReadOnlySpan<byte> utf8Value)
        {
            JsonWriterHelper.ValidatePropertyAndValue(utf8PropertyName, utf8Value);

            WriteStringEscape(utf8PropertyName, utf8Value);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = JsonTokenType.String;
        }

        /// <summary>
        /// Writes the pre-encoded property name and string text value (as a JSON string) as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="propertyName">The JSON-encoded name of the property to write.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified value is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// <para>
        /// The value is escaped before writing.
        /// </para>
        /// <para>
        /// If <paramref name="value"/> is <see langword="null"/> the JSON null value is written,
        /// as if <see cref="WriteNull(in JsonEncodedText)"/> was called.
        /// </para>
        /// </remarks>
        public void WriteString(in JsonEncodedText propertyName, string value)
        {
            if (value == null)
            {
                WriteNull(propertyName);
            }
            else
            {
                WriteString(propertyName, value.AsSpan());
            }
        }

        /// <summary>
        /// Writes the pre-encoded property name and text value (as a JSON string) as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="propertyName">The JSON-encoded name of the property to write.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified value is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// The value is escaped before writing.
        /// </remarks>
        public void WriteString(in JsonEncodedText propertyName, in ReadOnlySpan<char> value)
            => WriteStringHelperEscapeValue(propertyName.EncodedUtf8Bytes, value);

        private void WriteStringHelperEscapeValue(in ReadOnlySpan<byte> utf8PropertyName, in ReadOnlySpan<char> value)
        {
            Debug.Assert(utf8PropertyName.Length <= JsonSharedConstant.MaxUnescapedTokenSize);

            JsonWriterHelper.ValidateValue(value);

            int valueIdx = EscapingHelper.NeedsEscaping(value, _options.EscapeHandling, _options.Encoder);

            Debug.Assert(valueIdx >= -1 && valueIdx < value.Length && valueIdx < int.MaxValue / 2);

            if (valueIdx != -1)
            {
                WriteStringEscapeValueOnly(utf8PropertyName, value, valueIdx);
            }
            else
            {
                WriteStringByOptions(utf8PropertyName, value);
            }

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = JsonTokenType.String;
        }

        /// <summary>
        /// Writes the property name and text value (as a JSON string) as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name or value is too large.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="propertyName"/> parameter is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// The property name and value is escaped before writing.
        /// </remarks>
        public void WriteString(string propertyName, in ReadOnlySpan<char> value)
        {
            if (propertyName == null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.propertyName); }
            WriteString(propertyName.AsSpan(), value);
        }

        /// <summary>
        /// Writes the UTF-8 property name and text value (as a JSON string) as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="utf8PropertyName">The UTF-8 encoded name of the property to write.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name or value is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// The property name and value is escaped before writing.
        /// </remarks>
        public void WriteString(in ReadOnlySpan<byte> utf8PropertyName, in ReadOnlySpan<char> value)
        {
            JsonWriterHelper.ValidatePropertyAndValue(utf8PropertyName, value);

            WriteStringEscape(utf8PropertyName, value);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = JsonTokenType.String;
        }

        /// <summary>
        /// Writes the pre-encoded property name and UTF-8 text value (as a JSON string) as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="propertyName">The JSON-encoded name of the property to write.</param>
        /// <param name="utf8Value">The UTF-8 encoded value to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified value is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// The value is escaped before writing.
        /// </remarks>
        public void WriteString(in JsonEncodedText propertyName, in ReadOnlySpan<byte> utf8Value)
            => WriteStringHelperEscapeValue(propertyName.EncodedUtf8Bytes, utf8Value);

        private void WriteStringHelperEscapeValue(in ReadOnlySpan<byte> utf8PropertyName, in ReadOnlySpan<byte> utf8Value)
        {
            Debug.Assert(utf8PropertyName.Length <= JsonSharedConstant.MaxUnescapedTokenSize);

            JsonWriterHelper.ValidateValue(utf8Value);

            int valueIdx = EscapingHelper.NeedsEscaping(utf8Value, _options.EscapeHandling, _options.Encoder);

            Debug.Assert(valueIdx >= -1 && valueIdx < utf8Value.Length && valueIdx < int.MaxValue / 2);

            if (valueIdx != -1)
            {
                WriteStringEscapeValueOnly(utf8PropertyName, utf8Value, valueIdx);
            }
            else
            {
                WriteStringByOptions(utf8PropertyName, utf8Value);
            }

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = JsonTokenType.String;
        }

        /// <summary>
        /// Writes the property name and UTF-8 text value (as a JSON string) as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <param name="utf8Value">The UTF-8 encoded value to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name or value is too large.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="propertyName"/> parameter is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// The property name and value is escaped before writing.
        /// </remarks>
        public void WriteString(string propertyName, in ReadOnlySpan<byte> utf8Value)
        {
            if (propertyName == null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.propertyName); }
            WriteString(propertyName.AsSpan(), utf8Value);
        }

        /// <summary>
        /// Writes the property name and UTF-8 text value (as a JSON string) as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <param name="utf8Value">The UTF-8 encoded value to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name or value is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// The property name and value is escaped before writing.
        /// </remarks>
        public void WriteString(in ReadOnlySpan<char> propertyName, in ReadOnlySpan<byte> utf8Value)
        {
            JsonWriterHelper.ValidatePropertyAndValue(propertyName, utf8Value);

            WriteStringEscape(propertyName, utf8Value);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = JsonTokenType.String;
        }

        /// <summary>
        /// Writes the property name and pre-encoded value (as a JSON string) as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <param name="value">The JSON-encoded value to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteString(in ReadOnlySpan<char> propertyName, in JsonEncodedText value)
            => WriteStringHelperEscapeProperty(propertyName, value.EncodedUtf8Bytes);

        private void WriteStringHelperEscapeProperty(in ReadOnlySpan<char> propertyName, in ReadOnlySpan<byte> utf8Value)
        {
            Debug.Assert(utf8Value.Length <= JsonSharedConstant.MaxUnescapedTokenSize);

            JsonWriterHelper.ValidateProperty(propertyName);

            int propertyIdx = EscapingHelper.NeedsEscaping(propertyName, _options.EscapeHandling, _options.Encoder);

            Debug.Assert(propertyIdx >= -1 && propertyIdx < propertyName.Length && propertyIdx < int.MaxValue / 2);

            if (propertyIdx != -1)
            {
                WriteStringEscapePropertyOnly(propertyName, utf8Value, propertyIdx);
            }
            else
            {
                WriteStringByOptions(propertyName, utf8Value);
            }

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = JsonTokenType.String;
        }

        /// <summary>
        /// Writes the property name and string text value (as a JSON string) as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name or value is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// <para>
        /// The property name and value are escaped before writing.
        /// </para>
        /// <para>
        /// If <paramref name="value"/> is <see langword="null"/> the JSON null value is written,
        /// as if <see cref="WriteNull(in System.ReadOnlySpan{char})"/> was called.
        /// </para>
        /// </remarks>
        public void WriteString(in ReadOnlySpan<char> propertyName, string value)
        {
            if (value == null)
            {
                WriteNull(propertyName);
            }
            else
            {
                WriteString(propertyName, value.AsSpan());
            }
        }

        /// <summary>
        /// Writes the UTF-8 property name and pre-encoded value (as a JSON string) as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="utf8PropertyName">The UTF-8 encoded name of the property to write.</param>
        /// <param name="value">The JSON-encoded value to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteString(in ReadOnlySpan<byte> utf8PropertyName, in JsonEncodedText value)
            => WriteStringHelperEscapeProperty(utf8PropertyName, value.EncodedUtf8Bytes);

        private void WriteStringHelperEscapeProperty(in ReadOnlySpan<byte> utf8PropertyName, in ReadOnlySpan<byte> utf8Value)
        {
            Debug.Assert(utf8Value.Length <= JsonSharedConstant.MaxUnescapedTokenSize);

            JsonWriterHelper.ValidateProperty(utf8PropertyName);

            int propertyIdx = EscapingHelper.NeedsEscaping(utf8PropertyName, _options.EscapeHandling, _options.Encoder);

            Debug.Assert(propertyIdx >= -1 && propertyIdx < utf8PropertyName.Length && propertyIdx < int.MaxValue / 2);

            if (propertyIdx != -1)
            {
                WriteStringEscapePropertyOnly(utf8PropertyName, utf8Value, propertyIdx);
            }
            else
            {
                WriteStringByOptions(utf8PropertyName, utf8Value);
            }

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = JsonTokenType.String;
        }

        /// <summary>
        /// Writes the UTF-8 property name and string text value (as a JSON string) as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="utf8PropertyName">The UTF-8 encoded name of the property to write.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name or value is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// <para>
        /// The property name and value are escaped before writing.
        /// </para>
        /// <para>
        /// If <paramref name="value"/> is <see langword="null"/> the JSON null value is written,
        /// as if <see cref="WriteNull(in System.ReadOnlySpan{byte})"/> was called.
        /// </para>
        /// </remarks>
        public void WriteString(in ReadOnlySpan<byte> utf8PropertyName, string value)
        {
            if (value == null)
            {
                WriteNull(utf8PropertyName);
            }
            else
            {
                WriteString(utf8PropertyName, value.AsSpan());
            }
        }

        private void WriteStringEscapeValueOnly(in ReadOnlySpan<byte> escapedPropertyName, in ReadOnlySpan<byte> utf8Value, int firstEscapeIndex)
        {
            Debug.Assert(int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileEscaping >= utf8Value.Length);
            Debug.Assert(firstEscapeIndex >= 0 && firstEscapeIndex < utf8Value.Length);

            byte[] valueArray = null;

            int length = EscapingHelper.GetMaxEscapedLength(utf8Value.Length, firstEscapeIndex);

            Span<byte> escapedValue = (uint)length <= JsonSharedConstant.StackallocThreshold ?
                stackalloc byte[length] :
                (valueArray = ArrayPool<byte>.Shared.Rent(length));

            EscapingHelper.EscapeString(utf8Value, escapedValue, _options.EscapeHandling, firstEscapeIndex, _options.Encoder, out int written);

#if NETCOREAPP
            WriteStringByOptions(escapedPropertyName, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(escapedValue), written));
#else
            unsafe
            {
                WriteStringByOptions(escapedPropertyName, new ReadOnlySpan<byte>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(escapedValue)), written));
            }
#endif

            if (valueArray != null)
            {
                ArrayPool<byte>.Shared.Return(valueArray);
            }
        }

        private void WriteStringEscapeValueOnly(in ReadOnlySpan<byte> escapedPropertyName, in ReadOnlySpan<char> value, int firstEscapeIndex)
        {
            Debug.Assert(int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileEscaping >= value.Length);
            Debug.Assert(firstEscapeIndex >= 0 && firstEscapeIndex < value.Length);

            char[] valueArray = null;

            int length = EscapingHelper.GetMaxEscapedLength(value.Length, firstEscapeIndex);

            Span<char> escapedValue = (uint)length <= JsonSharedConstant.StackallocThreshold ?
                stackalloc char[length] :
                (valueArray = ArrayPool<char>.Shared.Rent(length));

            EscapingHelper.EscapeString(value, escapedValue, _options.EscapeHandling, firstEscapeIndex, _options.Encoder, out int written);

#if NETCOREAPP
            WriteStringByOptions(escapedPropertyName, MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(escapedValue), written));
#else
            unsafe
            {
                WriteStringByOptions(escapedPropertyName, new ReadOnlySpan<char>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(escapedValue)), written));
            }
#endif

            if (valueArray != null)
            {
                ArrayPool<char>.Shared.Return(valueArray);
            }
        }

        private void WriteStringEscapePropertyOnly(in ReadOnlySpan<char> propertyName, in ReadOnlySpan<byte> escapedValue, int firstEscapeIndex)
        {
            Debug.Assert(int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileEscaping >= propertyName.Length);
            Debug.Assert(firstEscapeIndex >= 0 && firstEscapeIndex < propertyName.Length);

            char[] propertyArray = null;

            int length = EscapingHelper.GetMaxEscapedLength(propertyName.Length, firstEscapeIndex);

            Span<char> escapedPropertyName = (uint)length <= JsonSharedConstant.StackallocThreshold ?
                stackalloc char[length] :
                (propertyArray = ArrayPool<char>.Shared.Rent(length));

            EscapingHelper.EscapeString(propertyName, escapedPropertyName, _options.EscapeHandling, firstEscapeIndex, _options.Encoder, out int written);

#if NETCOREAPP
            WriteStringByOptions(MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(escapedPropertyName), written), escapedValue);
#else
            unsafe
            {
                WriteStringByOptions(new ReadOnlySpan<char>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(escapedPropertyName)), written), escapedValue);
            }
#endif

            if (propertyArray != null)
            {
                ArrayPool<char>.Shared.Return(propertyArray);
            }
        }

        private void WriteStringEscapePropertyOnly(in ReadOnlySpan<byte> utf8PropertyName, in ReadOnlySpan<byte> escapedValue, int firstEscapeIndex)
        {
            Debug.Assert(int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileEscaping >= utf8PropertyName.Length);
            Debug.Assert(firstEscapeIndex >= 0 && firstEscapeIndex < utf8PropertyName.Length);

            byte[] propertyArray = null;

            int length = EscapingHelper.GetMaxEscapedLength(utf8PropertyName.Length, firstEscapeIndex);

            Span<byte> escapedPropertyName = (uint)length <= JsonSharedConstant.StackallocThreshold ?
                stackalloc byte[length] :
                (propertyArray = ArrayPool<byte>.Shared.Rent(length));

            EscapingHelper.EscapeString(utf8PropertyName, escapedPropertyName, _options.EscapeHandling, firstEscapeIndex, _options.Encoder, out int written);

#if NETCOREAPP
            WriteStringByOptions(MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(escapedPropertyName), written), escapedValue);
#else
            unsafe
            {
                WriteStringByOptions(new ReadOnlySpan<byte>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(escapedPropertyName)), written), escapedValue);
            }
#endif

            if (propertyArray != null)
            {
                ArrayPool<byte>.Shared.Return(propertyArray);
            }
        }

        private void WriteStringEscape(in ReadOnlySpan<char> propertyName, in ReadOnlySpan<char> value)
        {
            int valueIdx = EscapingHelper.NeedsEscaping(value, _options.EscapeHandling, _options.Encoder);
            int propertyIdx = EscapingHelper.NeedsEscaping(propertyName, _options.EscapeHandling, _options.Encoder);

            Debug.Assert(valueIdx >= -1 && valueIdx < value.Length && valueIdx < int.MaxValue / 2);
            Debug.Assert(propertyIdx >= -1 && propertyIdx < propertyName.Length && propertyIdx < int.MaxValue / 2);

            // Equivalent to: valueIdx != -1 || propertyIdx != -1
            if (valueIdx + propertyIdx != -2)
            {
                WriteStringEscapePropertyOrValue(propertyName, value, propertyIdx, valueIdx);
            }
            else
            {
                WriteStringByOptions(propertyName, value);
            }
        }

        private void WriteStringEscape(in ReadOnlySpan<byte> utf8PropertyName, in ReadOnlySpan<byte> utf8Value)
        {
            int valueIdx = EscapingHelper.NeedsEscaping(utf8Value, _options.EscapeHandling, _options.Encoder);
            int propertyIdx = EscapingHelper.NeedsEscaping(utf8PropertyName, _options.EscapeHandling, _options.Encoder);

            Debug.Assert(valueIdx >= -1 && valueIdx < utf8Value.Length && valueIdx < int.MaxValue / 2);
            Debug.Assert(propertyIdx >= -1 && propertyIdx < utf8PropertyName.Length && propertyIdx < int.MaxValue / 2);

            // Equivalent to: valueIdx != -1 || propertyIdx != -1
            if (valueIdx + propertyIdx != -2)
            {
                WriteStringEscapePropertyOrValue(utf8PropertyName, utf8Value, propertyIdx, valueIdx);
            }
            else
            {
                WriteStringByOptions(utf8PropertyName, utf8Value);
            }
        }

        private void WriteStringEscape(in ReadOnlySpan<char> propertyName, in ReadOnlySpan<byte> utf8Value)
        {
            int valueIdx = EscapingHelper.NeedsEscaping(utf8Value, _options.EscapeHandling, _options.Encoder);
            int propertyIdx = EscapingHelper.NeedsEscaping(propertyName, _options.EscapeHandling, _options.Encoder);

            Debug.Assert(valueIdx >= -1 && valueIdx < utf8Value.Length && valueIdx < int.MaxValue / 2);
            Debug.Assert(propertyIdx >= -1 && propertyIdx < propertyName.Length && propertyIdx < int.MaxValue / 2);

            // Equivalent to: valueIdx != -1 || propertyIdx != -1
            if (valueIdx + propertyIdx != -2)
            {
                WriteStringEscapePropertyOrValue(propertyName, utf8Value, propertyIdx, valueIdx);
            }
            else
            {
                WriteStringByOptions(propertyName, utf8Value);
            }
        }

        private void WriteStringEscape(in ReadOnlySpan<byte> utf8PropertyName, in ReadOnlySpan<char> value)
        {
            int valueIdx = EscapingHelper.NeedsEscaping(value, _options.EscapeHandling, _options.Encoder);
            int propertyIdx = EscapingHelper.NeedsEscaping(utf8PropertyName, _options.EscapeHandling, _options.Encoder);

            Debug.Assert(valueIdx >= -1 && valueIdx < value.Length && valueIdx < int.MaxValue / 2);
            Debug.Assert(propertyIdx >= -1 && propertyIdx < utf8PropertyName.Length && propertyIdx < int.MaxValue / 2);

            // Equivalent to: valueIdx != -1 || propertyIdx != -1
            if (valueIdx + propertyIdx != -2)
            {
                WriteStringEscapePropertyOrValue(utf8PropertyName, value, propertyIdx, valueIdx);
            }
            else
            {
                WriteStringByOptions(utf8PropertyName, value);
            }
        }

        private void WriteStringEscapePropertyOrValue(in ReadOnlySpan<char> propertyName, in ReadOnlySpan<char> value, int firstEscapeIndexProp, int firstEscapeIndexVal)
        {
            Debug.Assert(int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileEscaping >= value.Length);
            Debug.Assert(int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileEscaping >= propertyName.Length);

            char[] valueArray = null;
            char[] propertyArray = null;

            ReadOnlySpan<char> escapedValue;
            if (firstEscapeIndexVal != -1)
            {
                int length = EscapingHelper.GetMaxEscapedLength(value.Length, firstEscapeIndexVal);

                Span<char> tempValue;
                if ((uint)length > JsonSharedConstant.StackallocThreshold)
                {
                    valueArray = ArrayPool<char>.Shared.Rent(length);
                    tempValue = valueArray;
                }
                else
                {
                    // Cannot create a span directly since it gets assigned to parameter and passed down.
                    unsafe
                    {
                        char* ptr = stackalloc char[length];
                        tempValue = new Span<char>(ptr, length);
                    }
                }

                EscapingHelper.EscapeString(value, tempValue, _options.EscapeHandling, firstEscapeIndexVal, _options.Encoder, out int written);
                escapedValue = tempValue.Slice(0, written);
            }
            else
            {
                escapedValue = value;
            }

            if (firstEscapeIndexProp != -1)
            {
                int length = EscapingHelper.GetMaxEscapedLength(propertyName.Length, firstEscapeIndexProp);

                Span<char> escapedPropertyName;
                if ((uint)length > JsonSharedConstant.StackallocThreshold)
                {
                    propertyArray = ArrayPool<char>.Shared.Rent(length);
                    escapedPropertyName = propertyArray;
                }
                else
                {
                    // Cannot create a span directly since it gets assigned to parameter and passed down.
                    unsafe
                    {
                        char* ptr = stackalloc char[length];
                        escapedPropertyName = new Span<char>(ptr, length);
                    }
                }

                EscapingHelper.EscapeString(propertyName, escapedPropertyName, _options.EscapeHandling, firstEscapeIndexProp, _options.Encoder, out int written);
                WriteStringByOptions(escapedPropertyName.Slice(0, written), escapedValue);
            }
            else
            {
                WriteStringByOptions(propertyName, escapedValue);
            }

            if (valueArray != null)
            {
                ArrayPool<char>.Shared.Return(valueArray);
            }

            if (propertyArray != null)
            {
                ArrayPool<char>.Shared.Return(propertyArray);
            }
        }

        private void WriteStringEscapePropertyOrValue(in ReadOnlySpan<byte> utf8PropertyName, in ReadOnlySpan<byte> utf8Value, int firstEscapeIndexProp, int firstEscapeIndexVal)
        {
            Debug.Assert(int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileEscaping >= utf8Value.Length);
            Debug.Assert(int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileEscaping >= utf8PropertyName.Length);

            byte[] valueArray = null;
            byte[] propertyArray = null;

            ReadOnlySpan<byte> escapedValue;
            if (firstEscapeIndexVal != -1)
            {
                int length = EscapingHelper.GetMaxEscapedLength(utf8Value.Length, firstEscapeIndexVal);

                Span<byte> tempValue;
                if ((uint)length > JsonSharedConstant.StackallocThreshold)
                {
                    valueArray = ArrayPool<byte>.Shared.Rent(length);
                    tempValue = valueArray;
                }
                else
                {
                    // Cannot create a span directly since it gets assigned to parameter and passed down.
                    unsafe
                    {
                        byte* ptr = stackalloc byte[length];
                        tempValue = new Span<byte>(ptr, length);
                    }
                }

                EscapingHelper.EscapeString(utf8Value, tempValue, _options.EscapeHandling, firstEscapeIndexVal, _options.Encoder, out int written);
                escapedValue = tempValue.Slice(0, written);
            }
            else
            {
                escapedValue = utf8Value;
            }

            if (firstEscapeIndexProp != -1)
            {
                int length = EscapingHelper.GetMaxEscapedLength(utf8PropertyName.Length, firstEscapeIndexProp);

                Span<byte> escapedPropertyName;
                if ((uint)length > JsonSharedConstant.StackallocThreshold)
                {
                    propertyArray = ArrayPool<byte>.Shared.Rent(length);
                    escapedPropertyName = propertyArray;
                }
                else
                {
                    // Cannot create a span directly since it gets assigned to parameter and passed down.
                    unsafe
                    {
                        byte* ptr = stackalloc byte[length];
                        escapedPropertyName = new Span<byte>(ptr, length);
                    }
                }

                EscapingHelper.EscapeString(utf8PropertyName, escapedPropertyName, _options.EscapeHandling, firstEscapeIndexProp, _options.Encoder, out int written);
                WriteStringByOptions(escapedPropertyName.Slice(0, written), escapedValue);
            }
            else
            {
                WriteStringByOptions(utf8PropertyName, escapedValue);
            }

            if (valueArray != null)
            {
                ArrayPool<byte>.Shared.Return(valueArray);
            }

            if (propertyArray != null)
            {
                ArrayPool<byte>.Shared.Return(propertyArray);
            }
        }

        private void WriteStringEscapePropertyOrValue(in ReadOnlySpan<char> propertyName, in ReadOnlySpan<byte> utf8Value, int firstEscapeIndexProp, int firstEscapeIndexVal)
        {
            Debug.Assert(int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileEscaping >= utf8Value.Length);
            Debug.Assert(int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileEscaping >= propertyName.Length);

            byte[] valueArray = null;
            char[] propertyArray = null;

            ReadOnlySpan<byte> escapedValue;
            if (firstEscapeIndexVal != -1)
            {
                int length = EscapingHelper.GetMaxEscapedLength(utf8Value.Length, firstEscapeIndexVal);

                Span<byte> tempValue;
                if ((uint)length > JsonSharedConstant.StackallocThreshold)
                {
                    valueArray = ArrayPool<byte>.Shared.Rent(length);
                    tempValue = valueArray;
                }
                else
                {
                    // Cannot create a span directly since it gets assigned to parameter and passed down.
                    unsafe
                    {
                        byte* ptr = stackalloc byte[length];
                        tempValue = new Span<byte>(ptr, length);
                    }
                }

                EscapingHelper.EscapeString(utf8Value, tempValue, _options.EscapeHandling, firstEscapeIndexVal, _options.Encoder, out int written);
                escapedValue = tempValue.Slice(0, written);
            }
            else
            {
                escapedValue = utf8Value;
            }

            if (firstEscapeIndexProp != -1)
            {
                int length = EscapingHelper.GetMaxEscapedLength(propertyName.Length, firstEscapeIndexProp);

                Span<char> escapedPropertyName;
                if ((uint)length > JsonSharedConstant.StackallocThreshold)
                {
                    propertyArray = ArrayPool<char>.Shared.Rent(length);
                    escapedPropertyName = propertyArray;
                }
                else
                {
                    // Cannot create a span directly since it gets assigned to parameter and passed down.
                    unsafe
                    {
                        char* ptr = stackalloc char[length];
                        escapedPropertyName = new Span<char>(ptr, length);
                    }
                }

                EscapingHelper.EscapeString(propertyName, escapedPropertyName, _options.EscapeHandling, firstEscapeIndexProp, _options.Encoder, out int written);
                WriteStringByOptions(escapedPropertyName.Slice(0, written), escapedValue);
            }
            else
            {
                WriteStringByOptions(propertyName, escapedValue);
            }

            if (valueArray != null)
            {
                ArrayPool<byte>.Shared.Return(valueArray);
            }

            if (propertyArray != null)
            {
                ArrayPool<char>.Shared.Return(propertyArray);
            }
        }

        private void WriteStringEscapePropertyOrValue(in ReadOnlySpan<byte> utf8PropertyName, in ReadOnlySpan<char> value, int firstEscapeIndexProp, int firstEscapeIndexVal)
        {
            Debug.Assert(int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileEscaping >= value.Length);
            Debug.Assert(int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileEscaping >= utf8PropertyName.Length);

            char[] valueArray = null;
            byte[] propertyArray = null;

            ReadOnlySpan<char> escapedValue;
            if (firstEscapeIndexVal != -1)
            {
                int length = EscapingHelper.GetMaxEscapedLength(value.Length, firstEscapeIndexVal);

                Span<char> tempValue;
                if ((uint)length > JsonSharedConstant.StackallocThreshold)
                {
                    valueArray = ArrayPool<char>.Shared.Rent(length);
                    tempValue = valueArray;
                }
                else
                {
                    // Cannot create a span directly since it gets assigned to parameter and passed down.
                    unsafe
                    {
                        char* ptr = stackalloc char[length];
                        tempValue = new Span<char>(ptr, length);
                    }
                }

                EscapingHelper.EscapeString(value, tempValue, _options.EscapeHandling, firstEscapeIndexVal, _options.Encoder, out int written);
                escapedValue = tempValue.Slice(0, written);
            }
            else
            {
                escapedValue = value;
            }

            if (firstEscapeIndexProp != -1)
            {
                int length = EscapingHelper.GetMaxEscapedLength(utf8PropertyName.Length, firstEscapeIndexProp);

                Span<byte> escapedPropertyName;
                if ((uint)length > JsonSharedConstant.StackallocThreshold)
                {
                    propertyArray = ArrayPool<byte>.Shared.Rent(length);
                    escapedPropertyName = propertyArray;
                }
                else
                {
                    // Cannot create a span directly since it gets assigned to parameter and passed down.
                    unsafe
                    {
                        byte* ptr = stackalloc byte[length];
                        escapedPropertyName = new Span<byte>(ptr, length);
                    }
                }

                EscapingHelper.EscapeString(utf8PropertyName, escapedPropertyName, _options.EscapeHandling, firstEscapeIndexProp, _options.Encoder, out int written);
                WriteStringByOptions(escapedPropertyName.Slice(0, written), escapedValue);
            }
            else
            {
                WriteStringByOptions(utf8PropertyName, escapedValue);
            }

            if (valueArray != null)
            {
                ArrayPool<char>.Shared.Return(valueArray);
            }

            if (propertyArray != null)
            {
                ArrayPool<byte>.Shared.Return(propertyArray);
            }
        }

        private void WriteStringByOptions(in ReadOnlySpan<char> propertyName, in ReadOnlySpan<char> value)
        {
            ValidateWritingProperty();
            if (_options.Indented)
            {
                WriteStringIndented(propertyName, value);
            }
            else
            {
                WriteStringMinimized(propertyName, value);
            }
        }

        private void WriteStringByOptions(in ReadOnlySpan<byte> utf8PropertyName, in ReadOnlySpan<byte> utf8Value)
        {
            ValidateWritingProperty();
            if (_options.Indented)
            {
                WriteStringIndented(utf8PropertyName, utf8Value);
            }
            else
            {
                WriteStringMinimized(utf8PropertyName, utf8Value);
            }
        }

        private void WriteStringByOptions(in ReadOnlySpan<char> propertyName, in ReadOnlySpan<byte> utf8Value)
        {
            ValidateWritingProperty();
            if (_options.Indented)
            {
                WriteStringIndented(propertyName, utf8Value);
            }
            else
            {
                WriteStringMinimized(propertyName, utf8Value);
            }
        }

        private void WriteStringByOptions(in ReadOnlySpan<byte> utf8PropertyName, in ReadOnlySpan<char> value)
        {
            ValidateWritingProperty();
            if (_options.Indented)
            {
                WriteStringIndented(utf8PropertyName, value);
            }
            else
            {
                WriteStringMinimized(utf8PropertyName, value);
            }
        }

        // TODO: https://github.com/dotnet/corefx/issues/36958
        private void WriteStringMinimized(in ReadOnlySpan<char> escapedPropertyName, in ReadOnlySpan<char> escapedValue)
        {
            Debug.Assert(escapedValue.Length <= JsonSharedConstant.MaxUnescapedTokenSize);
            Debug.Assert(escapedPropertyName.Length < ((int.MaxValue - 6) / JsonSharedConstant.MaxExpansionFactorWhileTranscoding) - escapedValue.Length);

            // All ASCII, 2 quotes for property name, 2 quotes for value, and 1 colon => escapedPropertyName.Length + escapedValue.Length + 5
            // Optionally, 1 list separator, and up to 3x growth when transcoding
            int maxRequired = ((escapedPropertyName.Length + escapedValue.Length) * JsonSharedConstant.MaxExpansionFactorWhileTranscoding) + 6;

            ref var pos = ref _pos;
            EnsureUnsafe(pos, maxRequired);

            ref byte output = ref PinnableAddress;

            if (_currentDepth < 0)
            {
                Unsafe.Add(ref output, pos++) = JsonUtf8Constant.ListSeparator;
            }
            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;

            TranscodeAndWrite(escapedPropertyName, ref output, FreeCapacity, ref pos);

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.KeyValueSeperator;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;

            TranscodeAndWrite(escapedValue, ref output, FreeCapacity, ref pos);

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
        }

        // TODO: https://github.com/dotnet/corefx/issues/36958
        private void WriteStringMinimized(in ReadOnlySpan<byte> escapedPropertyName, in ReadOnlySpan<byte> escapedValue)
        {
            int nameLen = escapedPropertyName.Length;
            int valueLen = escapedValue.Length;
            Debug.Assert(valueLen <= JsonSharedConstant.MaxEscapedTokenSize);
            Debug.Assert(nameLen < int.MaxValue - valueLen - 6);

            int minRequired = nameLen + valueLen + 5; // 2 quotes for property name, 2 quotes for value, and 1 colon
            int maxRequired = minRequired + 1; // Optionally, 1 list separator

            ref var pos = ref _pos;
            EnsureUnsafe(pos, maxRequired);

            ref byte output = ref PinnableAddress;

            if (_currentDepth < 0)
            {
                Unsafe.Add(ref output, pos++) = JsonUtf8Constant.ListSeparator;
            }
            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;

            BinaryUtil.CopyMemory(ref MemoryMarshal.GetReference(escapedPropertyName), ref Unsafe.Add(ref output, pos), nameLen);
            pos += nameLen;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.KeyValueSeperator;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;

            BinaryUtil.CopyMemory(ref MemoryMarshal.GetReference(escapedValue), ref Unsafe.Add(ref output, pos), valueLen);
            pos += valueLen;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
        }

        // TODO: https://github.com/dotnet/corefx/issues/36958
        private void WriteStringMinimized(in ReadOnlySpan<char> escapedPropertyName, in ReadOnlySpan<byte> escapedValue)
        {
            int valueLen = escapedValue.Length;
            Debug.Assert(valueLen <= JsonSharedConstant.MaxEscapedTokenSize);
            Debug.Assert(escapedPropertyName.Length < (int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileTranscoding) - valueLen - 6);

            // All ASCII, 2 quotes for property name, 2 quotes for value, and 1 colon => escapedPropertyName.Length + escapedValue.Length + 5
            // Optionally, 1 list separator, and up to 3x growth when transcoding
            int maxRequired = (escapedPropertyName.Length * JsonSharedConstant.MaxExpansionFactorWhileTranscoding) + valueLen + 6;

            ref var pos = ref _pos;
            EnsureUnsafe(pos, maxRequired);

            ref byte output = ref PinnableAddress;

            if (_currentDepth < 0)
            {
                Unsafe.Add(ref output, pos++) = JsonUtf8Constant.ListSeparator;
            }
            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;

            TranscodeAndWrite(escapedPropertyName, ref output, FreeCapacity, ref pos);

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.KeyValueSeperator;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;

            BinaryUtil.CopyMemory(ref MemoryMarshal.GetReference(escapedValue), ref Unsafe.Add(ref output, pos), valueLen);
            pos += valueLen;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
        }

        // TODO: https://github.com/dotnet/corefx/issues/36958
        private void WriteStringMinimized(in ReadOnlySpan<byte> escapedPropertyName, in ReadOnlySpan<char> escapedValue)
        {
            int nameLen = escapedPropertyName.Length;
            Debug.Assert(escapedValue.Length <= JsonSharedConstant.MaxEscapedTokenSize);
            Debug.Assert(nameLen < (int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileTranscoding) - escapedValue.Length - 6);

            // All ASCII, 2 quotes for property name, 2 quotes for value, and 1 colon => escapedPropertyName.Length + escapedValue.Length + 5
            // Optionally, 1 list separator, and up to 3x growth when transcoding
            int maxRequired = (escapedValue.Length * JsonSharedConstant.MaxExpansionFactorWhileTranscoding) + nameLen + 6;

            ref var pos = ref _pos;
            EnsureUnsafe(pos, maxRequired);

            ref byte output = ref PinnableAddress;

            if (_currentDepth < 0)
            {
                Unsafe.Add(ref output, pos++) = JsonUtf8Constant.ListSeparator;
            }
            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;

            BinaryUtil.CopyMemory(ref MemoryMarshal.GetReference(escapedPropertyName), ref Unsafe.Add(ref output, pos), nameLen);
            pos += nameLen;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.KeyValueSeperator;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;

            TranscodeAndWrite(escapedValue, ref output, FreeCapacity, ref pos);

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
        }

        // TODO: https://github.com/dotnet/corefx/issues/36958
        private void WriteStringIndented(in ReadOnlySpan<char> escapedPropertyName, in ReadOnlySpan<char> escapedValue)
        {
            int indent = Indentation;
            Debug.Assert(indent <= 2 * JsonSharedConstant.MaxWriterDepth);

            Debug.Assert(escapedValue.Length <= JsonSharedConstant.MaxEscapedTokenSize);
            Debug.Assert(escapedPropertyName.Length < ((int.MaxValue - 7 - indent - JsonWriterHelper.NewLineLength) / JsonSharedConstant.MaxExpansionFactorWhileTranscoding) - escapedValue.Length);

            // All ASCII, 2 quotes for property name, 2 quotes for value, 1 colon, and 1 space => escapedPropertyName.Length + escapedValue.Length + 6
            // Optionally, 1 list separator, 1-2 bytes for new line, and up to 3x growth when transcoding
            int maxRequired = indent + ((escapedPropertyName.Length + escapedValue.Length) * JsonSharedConstant.MaxExpansionFactorWhileTranscoding) + 7 + JsonWriterHelper.NewLineLength;

            ref var pos = ref _pos;
            EnsureUnsafe(pos, maxRequired);

            ref byte output = ref PinnableAddress;

            if (_currentDepth < 0)
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

            TranscodeAndWrite(escapedValue, ref output, FreeCapacity, ref pos);

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
        }

        // TODO: https://github.com/dotnet/corefx/issues/36958
        private void WriteStringIndented(in ReadOnlySpan<byte> escapedPropertyName, in ReadOnlySpan<byte> escapedValue)
        {
            int indent = Indentation;
            Debug.Assert(indent <= 2 * JsonSharedConstant.MaxWriterDepth);

            int nameLen = escapedPropertyName.Length;
            int valueLen = escapedValue.Length;
            Debug.Assert(escapedValue.Length <= JsonSharedConstant.MaxEscapedTokenSize);
            Debug.Assert(nameLen < int.MaxValue - indent - valueLen - 7 - JsonWriterHelper.NewLineLength);

            int minRequired = indent + nameLen + valueLen + 6; // 2 quotes for property name, 2 quotes for value, 1 colon, and 1 space
            int maxRequired = minRequired + 1 + JsonWriterHelper.NewLineLength; // Optionally, 1 list separator and 1-2 bytes for new line

            ref var pos = ref _pos;
            EnsureUnsafe(pos, maxRequired);

            ref byte output = ref PinnableAddress;

            if (_currentDepth < 0)
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

            BinaryUtil.CopyMemory(ref MemoryMarshal.GetReference(escapedValue), ref Unsafe.Add(ref output, pos), valueLen);
            pos += valueLen;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
        }

        // TODO: https://github.com/dotnet/corefx/issues/36958
        private void WriteStringIndented(in ReadOnlySpan<char> escapedPropertyName, in ReadOnlySpan<byte> escapedValue)
        {
            int indent = Indentation;
            Debug.Assert(indent <= 2 * JsonSharedConstant.MaxWriterDepth);

            int valueLen = escapedValue.Length;
            Debug.Assert(valueLen <= JsonSharedConstant.MaxEscapedTokenSize);
            Debug.Assert(escapedPropertyName.Length < (int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileTranscoding) - valueLen - 7 - indent - JsonWriterHelper.NewLineLength);

            // All ASCII, 2 quotes for property name, 2 quotes for value, 1 colon, and 1 space => escapedPropertyName.Length + escapedValue.Length + 6
            // Optionally, 1 list separator, 1-2 bytes for new line, and up to 3x growth when transcoding
            int maxRequired = indent + (escapedPropertyName.Length * JsonSharedConstant.MaxExpansionFactorWhileTranscoding) + valueLen + 7 + JsonWriterHelper.NewLineLength;

            ref var pos = ref _pos;
            EnsureUnsafe(pos, maxRequired);

            ref byte output = ref PinnableAddress;

            if (_currentDepth < 0)
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

            BinaryUtil.CopyMemory(ref MemoryMarshal.GetReference(escapedValue), ref Unsafe.Add(ref output, pos), valueLen);
            pos += valueLen;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
        }

        // TODO: https://github.com/dotnet/corefx/issues/36958
        private void WriteStringIndented(in ReadOnlySpan<byte> escapedPropertyName, in ReadOnlySpan<char> escapedValue)
        {
            int indent = Indentation;
            Debug.Assert(indent <= 2 * JsonSharedConstant.MaxWriterDepth);

            int nameLen = escapedPropertyName.Length;
            Debug.Assert(escapedValue.Length <= JsonSharedConstant.MaxEscapedTokenSize);
            Debug.Assert(nameLen < (int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileTranscoding) - escapedValue.Length - 7 - indent - JsonWriterHelper.NewLineLength);

            // All ASCII, 2 quotes for property name, 2 quotes for value, 1 colon, and 1 space => escapedPropertyName.Length + escapedValue.Length + 6
            // Optionally, 1 list separator, 1-2 bytes for new line, and up to 3x growth when transcoding
            int maxRequired = indent + (escapedValue.Length * JsonSharedConstant.MaxExpansionFactorWhileTranscoding) + nameLen + 7 + JsonWriterHelper.NewLineLength;

            ref var pos = ref _pos;
            EnsureUnsafe(pos, maxRequired);

            ref byte output = ref PinnableAddress;

            if (_currentDepth < 0)
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

            TranscodeAndWrite(escapedValue, ref output, FreeCapacity, ref pos);

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
        }
    }
}
