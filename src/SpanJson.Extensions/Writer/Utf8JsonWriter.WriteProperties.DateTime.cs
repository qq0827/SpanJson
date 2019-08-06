// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SpanJson.Helpers;
using SpanJson.Internal;

namespace SpanJson
{
    partial struct Utf8JsonWriter
    {
        /// <summary>
        /// Writes the pre-encoded property name and <see cref="DateTime"/> value (as a JSON string) as part of a name/value pair of a JSON object.
        /// </summary>
        /// <param name="propertyName">The JSON-encoded name of the property to write.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// Writes the <see cref="DateTime"/> using the round-trippable ('O') <see cref="StandardFormat"/> , for example: 2017-06-12T05:30:45.7680000.
        /// The property name should already be escaped when the instance of <see cref="JsonEncodedText"/> was created.
        /// </remarks>
        public void WriteString(in JsonEncodedText propertyName, DateTime value)
            => WriteStringHelper(propertyName.EncodedUtf8Bytes, value);

        private void WriteStringHelper(in ReadOnlySpan<byte> utf8PropertyName, DateTime value)
        {
            Debug.Assert(utf8PropertyName.Length <= JsonSharedConstant.MaxUnescapedTokenSize);

            WriteStringByOptions(utf8PropertyName, value);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = JsonTokenType.String;
        }

        /// <summary>
        /// Writes the property name and <see cref="DateTime"/> value (as a JSON string) as part of a name/value pair of a JSON object.
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
        /// Writes the <see cref="DateTime"/> using the round-trippable ('O') <see cref="StandardFormat"/> , for example: 2017-06-12T05:30:45.7680000.
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteString(string propertyName, DateTime value)
        {
            if (propertyName == null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.propertyName); }
            WriteString(propertyName.AsSpan(), value);
        }

        /// <summary>
        /// Writes the property name and <see cref="DateTime"/> value (as a JSON string) as part of a name/value pair of a JSON object.
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
        /// Writes the <see cref="DateTime"/> using the round-trippable ('O') <see cref="StandardFormat"/> , for example: 2017-06-12T05:30:45.7680000.
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteString(in ReadOnlySpan<char> propertyName, DateTime value)
        {
            JsonWriterHelper.ValidateProperty(propertyName);

            WriteStringEscape(propertyName, value);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = JsonTokenType.String;
        }

        /// <summary>
        /// Writes the property name and <see cref="DateTime"/> value (as a JSON string) as part of a name/value pair of a JSON object.
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
        /// Writes the <see cref="DateTime"/> using the round-trippable ('O') <see cref="StandardFormat"/> , for example: 2017-06-12T05:30:45.7680000.
        /// The property name is escaped before writing.
        /// </remarks>
        public void WriteString(in ReadOnlySpan<byte> utf8PropertyName, DateTime value)
        {
            JsonWriterHelper.ValidateProperty(utf8PropertyName);

            WriteStringEscape(utf8PropertyName, value);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = JsonTokenType.String;
        }

        private void WriteStringEscape(in ReadOnlySpan<char> propertyName, DateTime value)
        {
            int propertyIdx = EscapingHelper.NeedsEscaping(propertyName, _options.EscapeHandling, _options.Encoder);

            Debug.Assert(propertyIdx >= -1 && propertyIdx < propertyName.Length);

            if (propertyIdx != -1)
            {
                WriteStringEscapeProperty(propertyName, value, propertyIdx);
            }
            else
            {
                WriteStringByOptions(propertyName, value);
            }
        }

        private void WriteStringEscape(in ReadOnlySpan<byte> utf8PropertyName, DateTime value)
        {
            int propertyIdx = EscapingHelper.NeedsEscaping(utf8PropertyName, _options.EscapeHandling, _options.Encoder);

            Debug.Assert(propertyIdx >= -1 && propertyIdx < utf8PropertyName.Length);

            if (propertyIdx != -1)
            {
                WriteStringEscapeProperty(utf8PropertyName, value, propertyIdx);
            }
            else
            {
                WriteStringByOptions(utf8PropertyName, value);
            }
        }

        private void WriteStringEscapeProperty(in ReadOnlySpan<char> propertyName, DateTime value, int firstEscapeIndexProp)
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
            WriteStringByOptions(MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(escapedPropertyName), written), value);
#else
            unsafe
            {
                WriteStringByOptions(new ReadOnlySpan<char>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(escapedPropertyName)), written), value);
            }
#endif

            if (propertyArray != null)
            {
                ArrayPool<char>.Shared.Return(propertyArray);
            }
        }

        private void WriteStringEscapeProperty(in ReadOnlySpan<byte> utf8PropertyName, DateTime value, int firstEscapeIndexProp)
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
            WriteStringByOptions(MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(escapedPropertyName), written), value);
#else
            unsafe
            {
                WriteStringByOptions(new ReadOnlySpan<byte>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(escapedPropertyName)), written), value);
            }
#endif

            if (propertyArray != null)
            {
                ArrayPool<byte>.Shared.Return(propertyArray);
            }
        }

        private void WriteStringByOptions(in ReadOnlySpan<char> propertyName, DateTime value)
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

        private void WriteStringByOptions(in ReadOnlySpan<byte> utf8PropertyName, DateTime value)
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

        private void WriteStringMinimized(in ReadOnlySpan<char> escapedPropertyName, DateTime value)
        {
            Debug.Assert(escapedPropertyName.Length < (int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileTranscoding) - JsonSharedConstant.MaximumFormatDateTimeOffsetLength - 6);

            // All ASCII, 2 quotes for property name, 2 quotes for date, and 1 colon => escapedPropertyName.Length + JsonSharedConstant.MaximumFormatDateTimeOffsetLength + 5
            // Optionally, 1 list separator, and up to 3x growth when transcoding
            int maxRequired = (escapedPropertyName.Length * JsonSharedConstant.MaxExpansionFactorWhileTranscoding) + JsonSharedConstant.MaximumFormatDateTimeOffsetLength + 6;

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

            Span<byte> tempSpan = stackalloc byte[JsonSharedConstant.MaximumFormatDateTimeOffsetLength];
            bool result = Utf8Formatter.TryFormat(value, tempSpan, out int bytesWritten, s_dateTimeStandardFormat);
            Debug.Assert(result);
            DateTimeFormatter.TrimDateTimeOffset(tempSpan.Slice(0, bytesWritten), out bytesWritten);
            BinaryUtil.CopyMemory(ref MemoryMarshal.GetReference(tempSpan), ref Unsafe.Add(ref output, pos), bytesWritten);
            pos += bytesWritten;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
        }

        private void WriteStringMinimized(in ReadOnlySpan<byte> escapedPropertyName, DateTime value)
        {
            int nameLen = escapedPropertyName.Length;
            Debug.Assert(nameLen < int.MaxValue - JsonSharedConstant.MaximumFormatDateTimeOffsetLength - 6);

            int minRequired = nameLen + JsonSharedConstant.MaximumFormatDateTimeOffsetLength + 5; // 2 quotes for property name, 2 quotes for date, and 1 colon
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

            Span<byte> tempSpan = stackalloc byte[JsonSharedConstant.MaximumFormatDateTimeOffsetLength];
            bool result = Utf8Formatter.TryFormat(value, tempSpan, out int bytesWritten, s_dateTimeStandardFormat);
            Debug.Assert(result);
            DateTimeFormatter.TrimDateTimeOffset(tempSpan.Slice(0, bytesWritten), out bytesWritten);
            BinaryUtil.CopyMemory(ref MemoryMarshal.GetReference(tempSpan), ref Unsafe.Add(ref output, pos), bytesWritten);
            pos += bytesWritten;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
        }

        private void WriteStringIndented(in ReadOnlySpan<char> escapedPropertyName, DateTime value)
        {
            int indent = Indentation;
            Debug.Assert(indent <= 2 * JsonSharedConstant.MaxWriterDepth);

            Debug.Assert(escapedPropertyName.Length < (int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileTranscoding) - indent - JsonSharedConstant.MaximumFormatDateTimeOffsetLength - 7 - JsonWriterHelper.NewLineLength);

            // All ASCII, 2 quotes for property name, 2 quotes for date, 1 colon, and 1 space => escapedPropertyName.Length + JsonSharedConstant.MaximumFormatDateTimeOffsetLength + 6
            // Optionally, 1 list separator, 1-2 bytes for new line, and up to 3x growth when transcoding
            int maxRequired = indent + (escapedPropertyName.Length * JsonSharedConstant.MaxExpansionFactorWhileTranscoding) + JsonSharedConstant.MaximumFormatDateTimeOffsetLength + 7 + JsonWriterHelper.NewLineLength;

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

            Span<byte> tempSpan = stackalloc byte[JsonSharedConstant.MaximumFormatDateTimeOffsetLength];
            bool result = Utf8Formatter.TryFormat(value, tempSpan, out int bytesWritten, s_dateTimeStandardFormat);
            Debug.Assert(result);
            DateTimeFormatter.TrimDateTimeOffset(tempSpan.Slice(0, bytesWritten), out bytesWritten);
            BinaryUtil.CopyMemory(ref MemoryMarshal.GetReference(tempSpan), ref Unsafe.Add(ref output, pos), bytesWritten);
            pos += bytesWritten;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
        }

        private void WriteStringIndented(in ReadOnlySpan<byte> escapedPropertyName, DateTime value)
        {
            int indent = Indentation;
            Debug.Assert(indent <= 2 * JsonSharedConstant.MaxWriterDepth);
            int namelen = escapedPropertyName.Length;
            Debug.Assert(namelen < int.MaxValue - indent - JsonSharedConstant.MaximumFormatDateTimeOffsetLength - 7 - JsonWriterHelper.NewLineLength);

            int minRequired = indent + namelen + JsonSharedConstant.MaximumFormatDateTimeOffsetLength + 6; // 2 quotes for property name, 2 quotes for date, 1 colon, and 1 space
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

            BinaryUtil.CopyMemory(ref MemoryMarshal.GetReference(escapedPropertyName), ref Unsafe.Add(ref output, pos), namelen);
            namelen += namelen;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.KeyValueSeperator;
            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.Space;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;

            Span<byte> tempSpan = stackalloc byte[JsonSharedConstant.MaximumFormatDateTimeOffsetLength];
            bool result = Utf8Formatter.TryFormat(value, tempSpan, out int bytesWritten, s_dateTimeStandardFormat);
            Debug.Assert(result);
            DateTimeFormatter.TrimDateTimeOffset(tempSpan.Slice(0, bytesWritten), out bytesWritten);
            BinaryUtil.CopyMemory(ref MemoryMarshal.GetReference(tempSpan), ref Unsafe.Add(ref output, pos), bytesWritten);
            pos += bytesWritten;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
        }
    }
}
