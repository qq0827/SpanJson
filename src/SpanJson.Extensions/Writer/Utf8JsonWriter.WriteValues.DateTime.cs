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
        /// Writes the <see cref="DateTime"/> value (as a JSON string) as an element of a JSON array.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// Writes the <see cref="DateTime"/> using the round-trippable ('O') <see cref="StandardFormat"/> , for example: 2017-06-12T05:30:45.7680000.
        /// </remarks>
        public void WriteStringValue(DateTime value)
        {
            ValidateWritingValue();
            if (_options.Indented)
            {
                WriteStringValueIndented(value);
            }
            else
            {
                WriteStringValueMinimized(value);
            }

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = JsonTokenType.String;
        }

        private void WriteStringValueMinimized(DateTime value)
        {
            int maxRequired = JsonSharedConstant.MaximumFormatDateTimeOffsetLength + 3; // 2 quotes, and optionally, 1 list separator

            ref var pos = ref _pos;
            EnsureUnsafe(pos, maxRequired);

            ref byte output = ref PinnableAddress;

            if (_currentDepth < 0)
            {
                Unsafe.Add(ref output, pos++) = JsonUtf8Constant.ListSeparator;
            }

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;

            Span<byte> tempSpan = stackalloc byte[JsonSharedConstant.MaximumFormatDateTimeOffsetLength];
            bool result = Utf8Formatter.TryFormat(value, tempSpan, out int bytesWritten, s_dateTimeStandardFormat);
            Debug.Assert(result);
            DateTimeFormatter.TrimDateTimeOffset(tempSpan.Slice(0, bytesWritten), out bytesWritten);
            BinaryUtil.CopyMemory(ref MemoryMarshal.GetReference(tempSpan), ref Unsafe.Add(ref output, pos), bytesWritten);
            pos += bytesWritten;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
        }

        private void WriteStringValueIndented(DateTime value)
        {
            int indent = Indentation;
            Debug.Assert(indent <= 2 * JsonSharedConstant.MaxWriterDepth);

            // 2 quotes, and optionally, 1 list separator and 1-2 bytes for new line
            int maxRequired = indent + JsonSharedConstant.MaximumFormatDateTimeOffsetLength + 3 + JsonWriterHelper.NewLineLength;

            ref var pos = ref _pos;
            EnsureUnsafe(pos, maxRequired);

            ref byte output = ref PinnableAddress;

            if (_currentDepth < 0)
            {
                Unsafe.Add(ref output, pos++) = JsonUtf8Constant.ListSeparator;
            }

            if (_tokenType != JsonTokenType.PropertyName)
            {
                if (_tokenType != JsonTokenType.None)
                {
                    WriteNewLine(ref output, ref pos);
                }
                JsonWriterHelper.WriteIndentation(ref output, indent, ref pos);
            }

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;

            Span<byte> tempSpan = stackalloc byte[JsonSharedConstant.MaximumFormatDateTimeOffsetLength];
            bool result = Utf8Formatter.TryFormat(value, tempSpan, out int bytesWritten, s_dateTimeStandardFormat);
            Debug.Assert(result);
            DateTimeFormatter.TrimDateTimeOffset(tempSpan.Slice(0, bytesWritten), out bytesWritten);
            BinaryUtil.CopyMemory(ref MemoryMarshal.GetReference(tempSpan), ref Unsafe.Add(ref output, pos), bytesWritten);
            pos += bytesWritten;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
        }

        private static readonly StandardFormat s_dateTimeStandardFormat = new StandardFormat('O');
    }
}
