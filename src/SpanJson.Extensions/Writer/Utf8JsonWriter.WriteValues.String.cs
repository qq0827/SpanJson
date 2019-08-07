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
        /// Writes the pre-encoded text value (as a JSON string) as an element of a JSON array.
        /// </summary>
        /// <param name="value">The JSON-encoded value to write.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        public void WriteStringValue(in JsonEncodedText value)
            => WriteStringValueHelper(value.EncodedUtf8Bytes);

        private void WriteStringValueHelper(in ReadOnlySpan<byte> utf8Value)
        {
            Debug.Assert(utf8Value.Length <= JsonSharedConstant.MaxUnescapedTokenSize);

            WriteStringByOptions(utf8Value);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = JsonTokenType.String;
        }

        /// <summary>
        /// Writes the string text value (as a JSON string) as an element of a JSON array.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified value is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// <para>
        /// The value is escaped before writing.</para>
        /// <para>
        /// If <paramref name="value"/> is <see langword="null"/> the JSON null value is written,
        /// as if <see cref="WriteNullValue"/> was called.
        /// </para>
        /// </remarks>
        public void WriteStringValue(string value)
        {
            if (value == null)
            {
                WriteNullValue();
            }
            else
            {
                WriteStringValue(value.AsSpan());
            }
        }

        /// <summary>
        /// Writes the text value (as a JSON string) as an element of a JSON array.
        /// </summary>
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
        public void WriteStringValue(in ReadOnlySpan<char> value)
        {
            JsonWriterHelper.ValidateValue(value);

            WriteStringEscape(value);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = JsonTokenType.String;
        }

        private void WriteStringEscape(in ReadOnlySpan<char> value)
        {
            int valueIdx = EscapingHelper.NeedsEscaping(value, _options.EscapeHandling, _options.Encoder);

            Debug.Assert(valueIdx >= -1 && valueIdx < value.Length);

            if (valueIdx != -1)
            {
                WriteStringEscapeValue(value, valueIdx);
            }
            else
            {
                WriteStringByOptions(value);
            }
        }

        private void WriteStringByOptions(in ReadOnlySpan<char> value)
        {
            ValidateWritingValue();
            if (_options.Indented)
            {
                WriteStringIndented(value);
            }
            else
            {
                WriteStringMinimized(value);
            }
        }

        // TODO: https://github.com/dotnet/corefx/issues/36958
        private void WriteStringMinimized(in ReadOnlySpan<char> escapedValue)
        {
            Debug.Assert(escapedValue.Length < (int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileTranscoding) - 3);

            // All ASCII, 2 quotes => escapedValue.Length + 2
            // Optionally, 1 list separator, and up to 3x growth when transcoding
            int maxRequired = (escapedValue.Length * JsonSharedConstant.MaxExpansionFactorWhileTranscoding) + 3;

            ref var pos = ref _pos;
            EnsureUnsafe(pos, maxRequired);

            ref byte output = ref PinnableAddress;

            if ((uint)_currentDepth > JsonSharedConstant.TooBigOrNegative)
            {
                Unsafe.Add(ref output, pos++) = JsonUtf8Constant.ListSeparator;
            }
            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;

            TranscodeAndWrite(escapedValue, ref output, FreeCapacity, ref pos);

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
        }

        // TODO: https://github.com/dotnet/corefx/issues/36958
        private void WriteStringIndented(in ReadOnlySpan<char> escapedValue)
        {
            int indent = Indentation;
            Debug.Assert(indent <= 2 * JsonSharedConstant.MaxWriterDepth);

            Debug.Assert(escapedValue.Length < (int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileTranscoding) - indent - 3 - JsonWriterHelper.NewLineLength);

            // All ASCII, 2 quotes => indent + escapedValue.Length + 2
            // Optionally, 1 list separator, 1-2 bytes for new line, and up to 3x growth when transcoding
            int maxRequired = indent + (escapedValue.Length * JsonSharedConstant.MaxExpansionFactorWhileTranscoding) + 3 + JsonWriterHelper.NewLineLength;

            ref var pos = ref _pos;
            EnsureUnsafe(pos, maxRequired);

            ref byte output = ref PinnableAddress;

            if ((uint)_currentDepth > JsonSharedConstant.TooBigOrNegative)
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

            TranscodeAndWrite(escapedValue, ref output, FreeCapacity, ref pos);

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
        }

        private void WriteStringEscapeValue(in ReadOnlySpan<char> value, int firstEscapeIndexVal)
        {
            Debug.Assert(int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileEscaping >= value.Length);
            Debug.Assert(firstEscapeIndexVal >= 0 && firstEscapeIndexVal < value.Length);

            char[] valueArray = null;

            int length = EscapingHelper.GetMaxEscapedLength(value.Length, firstEscapeIndexVal);

            Span<char> escapedValue = (uint)length <= JsonSharedConstant.StackallocThreshold ?
                stackalloc char[length] :
                (valueArray = ArrayPool<char>.Shared.Rent(length));

            EscapingHelper.EscapeString(value, escapedValue, _options.EscapeHandling, firstEscapeIndexVal, _options.Encoder, out int written);

#if NETCOREAPP
            WriteStringByOptions(MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(escapedValue), written));
#else
            unsafe
            {
                WriteStringByOptions(new ReadOnlySpan<char>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(escapedValue)), written));
            }
#endif

            if (valueArray != null)
            {
                ArrayPool<char>.Shared.Return(valueArray);
            }
        }

        /// <summary>
        /// Writes the UTF-8 text value (as a JSON string) as an element of a JSON array.
        /// </summary>
        /// <param name="utf8Value">The UTF-8 encoded value to be written as a JSON string element of a JSON array.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified value is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        /// <remarks>
        /// The value is escaped before writing.
        /// </remarks>
        public void WriteStringValue(in ReadOnlySpan<byte> utf8Value)
        {
            JsonWriterHelper.ValidateValue(utf8Value);

            WriteStringEscape(utf8Value);

            SetFlagToAddListSeparatorBeforeNextItem();
            _tokenType = JsonTokenType.String;
        }

        private void WriteStringEscape(in ReadOnlySpan<byte> utf8Value)
        {
            int valueIdx = EscapingHelper.NeedsEscaping(utf8Value, _options.EscapeHandling, _options.Encoder);

            Debug.Assert(valueIdx >= -1 && valueIdx < utf8Value.Length);

            if (valueIdx != -1)
            {
                WriteStringEscapeValue(utf8Value, valueIdx);
            }
            else
            {
                WriteStringByOptions(utf8Value);
            }
        }

        private void WriteStringByOptions(in ReadOnlySpan<byte> utf8Value)
        {
            ValidateWritingValue();
            if (_options.Indented)
            {
                WriteStringIndented(utf8Value);
            }
            else
            {
                WriteStringMinimized(utf8Value);
            }
        }

        // TODO: https://github.com/dotnet/corefx/issues/36958
        private void WriteStringMinimized(in ReadOnlySpan<byte> escapedValue)
        {
            int escapedValueLen = escapedValue.Length;
            Debug.Assert(escapedValueLen < int.MaxValue - 3);

            int minRequired = escapedValueLen + 2; // 2 quotes
            int maxRequired = minRequired + 1; // Optionally, 1 list separator

            ref var pos = ref _pos;
            EnsureUnsafe(pos, maxRequired);

            ref byte output = ref PinnableAddress;

            if ((uint)_currentDepth > JsonSharedConstant.TooBigOrNegative)
            {
                Unsafe.Add(ref output, pos++) = JsonUtf8Constant.ListSeparator;
            }
            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;

            BinaryUtil.CopyMemory(ref MemoryMarshal.GetReference(escapedValue), ref Unsafe.Add(ref output, pos), escapedValueLen);
            pos += escapedValueLen;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
        }

        // TODO: https://github.com/dotnet/corefx/issues/36958
        private void WriteStringIndented(in ReadOnlySpan<byte> escapedValue)
        {
            int indent = Indentation;
            Debug.Assert(indent <= 2 * JsonSharedConstant.MaxWriterDepth);

            int escapedValueLen = escapedValue.Length;
            Debug.Assert(escapedValueLen < int.MaxValue - indent - 3 - JsonWriterHelper.NewLineLength);

            int minRequired = indent + escapedValueLen + 2; // 2 quotes
            int maxRequired = minRequired + 1 + JsonWriterHelper.NewLineLength; // Optionally, 1 list separator and 1-2 bytes for new line

            ref var pos = ref _pos;
            EnsureUnsafe(pos, maxRequired);

            ref byte output = ref PinnableAddress;

            if ((uint)_currentDepth > JsonSharedConstant.TooBigOrNegative)
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

            BinaryUtil.CopyMemory(ref MemoryMarshal.GetReference(escapedValue), ref Unsafe.Add(ref output, pos), escapedValueLen);
            pos += escapedValueLen;

            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.DoubleQuote;
        }

        private void WriteStringEscapeValue(in ReadOnlySpan<byte> utf8Value, int firstEscapeIndexVal)
        {
            Debug.Assert(int.MaxValue / JsonSharedConstant.MaxExpansionFactorWhileEscaping >= utf8Value.Length);
            Debug.Assert(firstEscapeIndexVal >= 0 && firstEscapeIndexVal < utf8Value.Length);

            byte[] valueArray = null;

            int length = EscapingHelper.GetMaxEscapedLength(utf8Value.Length, firstEscapeIndexVal);

            Span<byte> escapedValue = (uint)length <= JsonSharedConstant.StackallocThreshold ?
                stackalloc byte[length] :
                (valueArray = ArrayPool<byte>.Shared.Rent(length));

            EscapingHelper.EscapeString(utf8Value, escapedValue, _options.EscapeHandling, firstEscapeIndexVal, _options.Encoder, out int written);

#if NETCOREAPP
            WriteStringByOptions(MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(escapedValue), written));
#else
            unsafe
            {
                WriteStringByOptions(new ReadOnlySpan<byte>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(escapedValue)), written));
            }
#endif

            if (valueArray != null)
            {
                ArrayPool<byte>.Shared.Return(valueArray);
            }
        }
    }
}
