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
        /// Writes the value (as a JSON number) as an element of a JSON array.
        /// </summary>
        /// <param name="utf8FormattedNumber">The value to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="utf8FormattedNumber"/> does not represent a valid JSON number.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// Writes the <see cref="int"/> using the default <see cref="StandardFormat"/> (that is, 'G'), for example: 32767.
        /// </remarks>
        internal void WriteNumberValue(in ReadOnlySpan<byte> utf8FormattedNumber)
        {
            JsonWriterHelper.ValidateValue(utf8FormattedNumber);
            JsonWriterHelper.ValidateNumber(utf8FormattedNumber);
            ValidateWritingValue();

            if (_options.Indented)
            {
                WriteNumberValueIndented(utf8FormattedNumber);
            }
            else
            {
                WriteNumberValueMinimized(utf8FormattedNumber);
            }

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = JsonTokenType.Number;
        }

        private void WriteNumberValueMinimized(in ReadOnlySpan<byte> utf8Value)
        {
            int valueLen = utf8Value.Length;
            int maxRequired = valueLen + 1; // Optionally, 1 list separator

            ref var pos = ref _pos;
            EnsureUnsafe(pos, maxRequired);

            ref byte output = ref PinnableAddress;

            if (_currentDepth < 0)
            {
                Unsafe.Add(ref output, pos++) = JsonUtf8Constant.ListSeparator;
            }

            BinaryUtil.CopyMemory(ref MemoryMarshal.GetReference(utf8Value), ref Unsafe.Add(ref output, pos), valueLen);
            pos += valueLen;
        }

        private void WriteNumberValueIndented(in ReadOnlySpan<byte> utf8Value)
        {
            int indent = Indentation;
            Debug.Assert(indent <= 2 * JsonSharedConstant.MaxWriterDepth);

            int valueLen = utf8Value.Length;
            Debug.Assert(valueLen < int.MaxValue - indent - 1 - JsonWriterHelper.NewLineLength);

            int maxRequired = indent + valueLen + 1 + JsonWriterHelper.NewLineLength; // Optionally, 1 list separator and 1-2 bytes for new line

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

            BinaryUtil.CopyMemory(ref MemoryMarshal.GetReference(utf8Value), ref Unsafe.Add(ref output, pos), valueLen);
            pos += valueLen;
        }
    }
}
