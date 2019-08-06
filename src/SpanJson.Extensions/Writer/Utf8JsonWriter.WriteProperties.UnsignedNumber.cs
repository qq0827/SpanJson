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
        /// Writes the pre-encoded property name and <see cref="ulong"/> value (as a JSON number) as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="propertyName">The JSON-encoded name of the property to write.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// Writes the <see cref="ulong"/> using the default <see cref="StandardFormat"/> (that is, 'G'), for example: 32767.
        /// </remarks>
        public void WriteNumber(in JsonEncodedText propertyName, ulong value)
            => WriteNumberHelper(propertyName.EncodedUtf8Bytes, value);

        private void WriteNumberHelper(in ReadOnlySpan<byte> utf8PropertyName, ulong value)
        {
            Debug.Assert(utf8PropertyName.Length <= JsonSharedConstant.MaxUnescapedTokenSize);

            WriteNumberByOptions(utf8PropertyName, value);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = JsonTokenType.Number;
        }

        /// <summary>
        /// Writes the property name and <see cref="ulong"/> value (as a JSON number) as part of a name/value pair of a JSON object.
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
        /// Writes the <see cref="ulong"/> using the default <see cref="StandardFormat"/> (that is, 'G'), for example: 32767.
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteNumber(string propertyName, ulong value)
        {
            if (propertyName == null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.propertyName); }
            WriteNumber(propertyName.AsSpan(), value);
        }

        /// <summary>
        /// Writes the property name and <see cref="ulong"/> value (as a JSON number) as part of a name/value pair of a JSON object.
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
        /// Writes the <see cref="ulong"/> using the default <see cref="StandardFormat"/> (that is, 'G'), for example: 32767.
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteNumber(in ReadOnlySpan<char> propertyName, ulong value)
        {
            JsonWriterHelper.ValidateProperty(propertyName);

            WriteNumberEscape(propertyName, value);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = JsonTokenType.Number;
        }

        /// <summary>
        /// Writes the property name and <see cref="ulong"/> value (as a JSON number) as part of a name/value pair of a JSON object.
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
        /// Writes the <see cref="ulong"/> using the default <see cref="StandardFormat"/> (that is, 'G'), for example: 32767.
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteNumber(in ReadOnlySpan<byte> utf8PropertyName, ulong value)
        {
            JsonWriterHelper.ValidateProperty(utf8PropertyName);

            WriteNumberEscape(utf8PropertyName, value);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = JsonTokenType.Number;
        }

        /// <summary>
        /// Writes the pre-encoded property name and <see cref="uint"/> value (as a JSON number) as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="propertyName">The JSON-encoded name of the property to write.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// Writes the <see cref="uint"/> using the default <see cref="StandardFormat"/> (that is, 'G'), for example: 32767.
        /// </remarks>
        public void WriteNumber(in JsonEncodedText propertyName, uint value)
            => WriteNumber(propertyName, (ulong)value);

        /// <summary>
        /// Writes the property name and <see cref="uint"/> value (as a JSON number) as part of a name/value pair of a JSON object.
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
        /// Writes the <see cref="uint"/> using the default <see cref="StandardFormat"/> (that is, 'G'), for example: 32767.
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteNumber(string propertyName, uint value)
        {
            if (propertyName == null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.propertyName); }
            WriteNumber(propertyName.AsSpan(), (ulong)value);
        }

        /// <summary>
        /// Writes the property name and <see cref="uint"/> value (as a JSON number) as part of a name/value pair of a JSON object.
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
        /// Writes the <see cref="uint"/> using the default <see cref="StandardFormat"/> (that is, 'G'), for example: 32767.
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteNumber(in ReadOnlySpan<char> propertyName, uint value)
            => WriteNumber(propertyName, (ulong)value);

        /// <summary>
        /// Writes the property name and <see cref="uint"/> value (as a JSON number) as part of a name/value pair of a JSON object.
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
        /// Writes the <see cref="uint"/> using the default <see cref="StandardFormat"/> (that is, 'G'), for example: 32767.
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteNumber(in ReadOnlySpan<byte> utf8PropertyName, uint value)
            => WriteNumber(utf8PropertyName, (ulong)value);

        private void WriteNumberEscape(in ReadOnlySpan<char> propertyName, ulong value)
        {
            int propertyIdx = EscapingHelper.NeedsEscaping(propertyName, _options.EscapeHandling, _options.Encoder);

            Debug.Assert(propertyIdx >= -1 && propertyIdx < propertyName.Length);

            if (propertyIdx != -1)
            {
                WriteNumberEscapeProperty(propertyName, value, propertyIdx);
            }
            else
            {
                WriteNumberByOptions(propertyName, value);
            }
        }

        private void WriteNumberEscape(in ReadOnlySpan<byte> utf8PropertyName, ulong value)
        {
            int propertyIdx = EscapingHelper.NeedsEscaping(utf8PropertyName, _options.EscapeHandling, _options.Encoder);

            Debug.Assert(propertyIdx >= -1 && propertyIdx < utf8PropertyName.Length);

            if (propertyIdx != -1)
            {
                WriteNumberEscapeProperty(utf8PropertyName, value, propertyIdx);
            }
            else
            {
                WriteNumberByOptions(utf8PropertyName, value);
            }
        }

        private void WriteNumberEscapeProperty(in ReadOnlySpan<char> propertyName, ulong value, int firstEscapeIndexProp)
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
            WriteNumberByOptions(MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(escapedPropertyName), written), value);
#else
            unsafe
            {
                WriteNumberByOptions(new ReadOnlySpan<char>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(escapedPropertyName)), written), value);
            }
#endif

            if (propertyArray != null)
            {
                ArrayPool<char>.Shared.Return(propertyArray);
            }
        }

        private void WriteNumberEscapeProperty(in ReadOnlySpan<byte> utf8PropertyName, ulong value, int firstEscapeIndexProp)
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
            WriteNumberByOptions(MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(escapedPropertyName), written), value);
#else
            unsafe
            {
                WriteNumberByOptions(new ReadOnlySpan<byte>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(escapedPropertyName)), written), value);
            }
#endif

            if (propertyArray != null)
            {
                ArrayPool<byte>.Shared.Return(propertyArray);
            }
        }

        private void WriteNumberByOptions(in ReadOnlySpan<char> propertyName, ulong value)
        {
            ValidateWritingProperty();
            if (_options.Indented)
            {
                WriteNumberIndented(propertyName, value);
            }
            else
            {
                WriteNumberMinimized(propertyName, value);
            }
        }

        private void WriteNumberByOptions(in ReadOnlySpan<byte> utf8PropertyName, ulong value)
        {
            ValidateWritingProperty();
            if (_options.Indented)
            {
                WriteNumberIndented(utf8PropertyName, value);
            }
            else
            {
                WriteNumberMinimized(utf8PropertyName, value);
            }
        }

        private void WriteNumberMinimized(in ReadOnlySpan<char> escapedPropertyName, ulong value)
        {
            Debug.Assert(escapedPropertyName.Length < (int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileTranscoding) - JsonSharedConstant.MaximumFormatUInt64Length - 4);

            // All ASCII, 2 quotes for property name, and 1 colon => escapedPropertyName.Length + JsonSharedConstant.MaximumFormatUInt64Length + 3
            // Optionally, 1 list separator, and up to 3x growth when transcoding
            int maxRequired = (escapedPropertyName.Length * JsonSharedConstant.MaxExpansionFactorWhileTranscoding) + JsonSharedConstant.MaximumFormatUInt64Length + 4;

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

            bool result = Utf8Formatter.TryFormat(value, FreeSpan, out int bytesWritten);
            Debug.Assert(result);
            pos += bytesWritten;
        }

        private void WriteNumberMinimized(in ReadOnlySpan<byte> escapedPropertyName, ulong value)
        {
            int nameLen = escapedPropertyName.Length;
            Debug.Assert(nameLen < int.MaxValue - JsonSharedConstant.MaximumFormatUInt64Length - 4);

            int minRequired = nameLen + JsonSharedConstant.MaximumFormatUInt64Length + 3; // 2 quotes for property name, and 1 colon
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

            bool result = Utf8Formatter.TryFormat(value, FreeSpan, out int bytesWritten);
            Debug.Assert(result);
            pos += bytesWritten;
        }

        private void WriteNumberIndented(in ReadOnlySpan<char> escapedPropertyName, ulong value)
        {
            int indent = Indentation;
            Debug.Assert(indent <= 2 * JsonSharedConstant.MaxWriterDepth);

            Debug.Assert(escapedPropertyName.Length < (int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileTranscoding) - indent - JsonSharedConstant.MaximumFormatUInt64Length - 5 - JsonWriterHelper.NewLineLength);

            // All ASCII, 2 quotes for property name, 1 colon, and 1 space => escapedPropertyName.Length + JsonSharedConstant.MaximumFormatUInt64Length + 4
            // Optionally, 1 list separator, 1-2 bytes for new line, and up to 3x growth when transcoding
            int maxRequired = indent + (escapedPropertyName.Length * JsonSharedConstant.MaxExpansionFactorWhileTranscoding) + JsonSharedConstant.MaximumFormatUInt64Length + 5 + JsonWriterHelper.NewLineLength;

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

            bool result = Utf8Formatter.TryFormat(value, FreeSpan, out int bytesWritten);
            Debug.Assert(result);
            pos += bytesWritten;
        }

        private void WriteNumberIndented(in ReadOnlySpan<byte> escapedPropertyName, ulong value)
        {
            int indent = Indentation;
            Debug.Assert(indent <= 2 * JsonSharedConstant.MaxWriterDepth);

            int nameLen = escapedPropertyName.Length;
            Debug.Assert(nameLen < int.MaxValue - indent - JsonSharedConstant.MaximumFormatUInt64Length - 5 - JsonWriterHelper.NewLineLength);

            int minRequired = indent + nameLen + JsonSharedConstant.MaximumFormatUInt64Length + 4; // 2 quotes for property name, 1 colon, and 1 space
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

            bool result = Utf8Formatter.TryFormat(value, FreeSpan, out int bytesWritten);
            Debug.Assert(result);
            pos += bytesWritten;
        }
    }
}
