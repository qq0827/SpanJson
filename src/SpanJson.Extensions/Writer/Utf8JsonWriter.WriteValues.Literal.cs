// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SpanJson.Internal;

namespace SpanJson
{
    partial struct Utf8JsonWriter
    {
        /// <summary>
        /// Writes the JSON literal "null" as an element of a JSON array.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        public void WriteNullValue()
        {
            WriteLiteralByOptions(JsonUtf8Constant.NullValue);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = JsonTokenType.Null;
        }

        /// <summary>
        /// Writes the <see cref="bool"/> value (as a JSON literal "true" or "false") as an element of a JSON array.
        /// </summary>
        /// <param name="value">The value write.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        public void WriteBooleanValue(bool value)
        {
            if (value)
            {
                WriteLiteralByOptions(JsonUtf8Constant.TrueValue);
                _tokenType = JsonTokenType.True;
            }
            else
            {
                WriteLiteralByOptions(JsonUtf8Constant.FalseValue);
                _tokenType = JsonTokenType.False;
            }

            SetFlagToAddListSeparatorBeforeNextItem();
        }

        private void WriteLiteralByOptions(in ReadOnlySpan<byte> utf8Value)
        {
            ValidateWritingValue();
            if (_options.Indented)
            {
                WriteLiteralIndented(utf8Value);
            }
            else
            {
                WriteLiteralMinimized(utf8Value);
            }
        }

        private void WriteLiteralMinimized(in ReadOnlySpan<byte> utf8Value)
        {
            int valueLen = utf8Value.Length;
            Debug.Assert(valueLen <= 5);

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

        private void WriteLiteralIndented(in ReadOnlySpan<byte> utf8Value)
        {
            int indent = Indentation;
            int valueLen = utf8Value.Length;
            Debug.Assert(indent <= 2 * JsonSharedConstant.MaxWriterDepth);
            Debug.Assert(valueLen <= 5);

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
