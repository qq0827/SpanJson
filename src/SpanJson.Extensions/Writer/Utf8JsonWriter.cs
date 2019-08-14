// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SpanJson.Internal;

namespace SpanJson
{
    /// <summary>
    /// Provides a high-performance API for forward-only, non-cached writing of UTF-8 encoded JSON text.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     It writes the text sequentially with no caching and adheres to the JSON RFC
    ///     by default (https://tools.ietf.org/html/rfc8259), with the exception of writing comments.
    ///   </para>
    ///   <para>
    ///     When the user attempts to write invalid JSON and validation is enabled, it throws
    ///     an <see cref="InvalidOperationException"/> with a context specific error message.
    ///   </para>
    ///   <para>
    ///     To be able to format the output with indentation and whitespace OR to skip validation, create an instance of 
    ///     <see cref="JsonWriterOptions"/> and pass that in to the writer.
    ///   </para>
    /// </remarks>
    public ref partial struct Utf8JsonWriter
    {
        private bool _inObject;
        private JsonTokenType _tokenType;
        private BitStack _bitStack;

        // The highest order bit of _currentDepth is used to discern whether we are writing the first item in a list or not.
        // if (_currentDepth >> 31) == 1, add a list separator before writing the item
        // else, no list separator is needed since we are writing the first item.
        private int _currentDepth;

        private JsonWriterOptions _options; // Since JsonWriterOptions is a struct, use a field to avoid a copy for internal code.

        private ArrayPool<byte> _arrayPool;
        private byte[] _borrowedBuffer;
        private Span<byte> _utf8Span;
        private int _capacity;
        private int _pos;

        /// <summary>
        /// Gets the custom behavior when writing JSON using
        /// the <see cref="Utf8JsonWriter"/> which indicates whether to format the output
        /// while writing and whether to skip structural JSON validation or not.
        /// </summary>
        public JsonWriterOptions Options => _options;

        private int Indentation => CurrentDepth * JsonSharedConstant.SpacesPerIndent;

        /// <summary>
        /// Tracks the recursive depth of the nested objects / arrays within the JSON text
        /// written so far. This provides the depth of the current token.
        /// </summary>
        public int CurrentDepth => _currentDepth & JsonSharedConstant.RemoveFlagsBitMask;

        /// <summary>TBD</summary>
        public ref byte PinnableAddress
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref MemoryMarshal.GetReference(_utf8Span);
        }

        public int WrittenCount => _pos;

        public int Capacity => _capacity;

        public int FreeCapacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _capacity - _pos;
        }

        /// <summary>Unsafe</summary>
        public ReadOnlySpan<byte> WrittenSpan
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _utf8Span.Slice(0, _pos);
        }

        /// <summary>Unsafe</summary>
        public Span<byte> FreeSpan
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _utf8Span.Slice(_pos);
        }

        /// <summary>Constructs a new <see cref="Utf8JsonWriter"/> instance.</summary>
        /// <param name="useThreadLocalBuffer">TBD</param>
        /// <param name="options">Defines the customized behavior of the <see cref="Utf8JsonWriter"/>
        /// By default, the <see cref="Utf8JsonWriter"/> writes JSON minimized (that is, with no extra whitespace)
        /// and validates that the JSON being written is structurally valid according to JSON RFC.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the instance of <see cref="IBufferWriter{Byte}" /> that is passed in is null.
        /// </exception>
        public Utf8JsonWriter(bool useThreadLocalBuffer, JsonWriterOptions options = default)
        {
            _inObject = default;
            _tokenType = default;
            _currentDepth = default;
            _options = options;

            // Only allocate if the user writes a JSON payload beyond the depth that the _allocationFreeContainer can handle.
            // This way we avoid allocations in the common, default cases, and allocate lazily.
            _bitStack = default;

            _pos = 0;
            if (useThreadLocalBuffer)
            {
                _arrayPool = null;
                _utf8Span = _borrowedBuffer = InternalMemoryPool.GetBuffer();
            }
            else
            {
                _arrayPool = ArrayPool<byte>.Shared;
                _utf8Span = _borrowedBuffer = _arrayPool.Rent(InternalMemoryPool.InitialCapacity);
            }
            _capacity = _borrowedBuffer.Length;
        }

        /// <summary>Constructs a new <see cref="Utf8JsonWriter"/> instance.</summary>
        /// <param name="initialCapacity">TBD</param>
        /// <param name="options">Defines the customized behavior of the <see cref="Utf8JsonWriter"/>
        /// By default, the <see cref="Utf8JsonWriter"/> writes JSON minimized (that is, with no extra whitespace)
        /// and validates that the JSON being written is structurally valid according to JSON RFC.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the instance of <see cref="Stream" /> that is passed in is null.
        /// </exception>
        public Utf8JsonWriter(int initialCapacity, JsonWriterOptions options = default)
        {
            if (((uint)(initialCapacity - 1)) > JsonSharedConstant.TooBigOrNegative) { initialCapacity = InternalMemoryPool.InitialCapacity; }

            _inObject = default;
            _tokenType = default;
            _currentDepth = default;
            _options = options;

            // Only allocate if the user writes a JSON payload beyond the depth that the _allocationFreeContainer can handle.
            // This way we avoid allocations in the common, default cases, and allocate lazily.
            _bitStack = default;

            _pos = 0;
            _arrayPool = ArrayPool<byte>.Shared;
            _utf8Span = _borrowedBuffer = _arrayPool.Rent(initialCapacity);
            _capacity = _borrowedBuffer.Length;
        }

        /// <summary>Clears the data written to the underlying buffer.</summary>
        public void Clear()
        {
            if (_utf8Span.IsEmpty) { return; }
            _utf8Span.Slice(0, _pos).Clear();
            _pos = 0;
        }

        public byte[] ToByteArray()
        {
            ref var alreadyWritten = ref _pos;
            if (0u >= (uint)alreadyWritten) { return JsonHelpers.Empty<byte>(); }

            var borrowedBuffer = _borrowedBuffer;
            if (borrowedBuffer is null) { return JsonHelpers.Empty<byte>(); }

            var destination = new byte[alreadyWritten];
            BinaryUtil.CopyMemory(borrowedBuffer, 0, destination, 0, alreadyWritten);
            Dispose();
            return destination;
        }

        public void Dispose()
        {
            var toReturn = _borrowedBuffer;
            var arrayPool = _arrayPool;
            this = default; // for safety, to avoid using pooled array if this instance is erroneously appended to again
            if (arrayPool is object)
            {
                arrayPool.Return(toReturn);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Ensure(int sizeHintt)
        {
            var alreadyWritten = _pos;
            if ((uint)sizeHintt >= (uint)(_capacity - alreadyWritten)) { CheckAndResizeBuffer(alreadyWritten, sizeHintt); }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureUnsafe(int alreadyWritten, int sizeHintt)
        {
            if ((uint)sizeHintt >= (uint)(_capacity - alreadyWritten)) { CheckAndResizeBuffer(alreadyWritten, sizeHintt); }
        }

        public void Advance(int count)
        {
            if ((uint)count > JsonSharedConstant.TooBigOrNegative) { ThrowHelper.ThrowArgumentOutOfRangeException_Nonnegative(ExceptionArgument.count); }

            if ((uint)_pos > (uint)(_capacity - count)) { ThrowHelper.ThrowInvalidOperationException_AdvancedTooFar(_capacity); }

            _pos += count;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void CheckAndResizeBuffer(int alreadyWritten, int sizeHint)
        {
            Debug.Assert(_borrowedBuffer is object);

            const int MinimumBufferSize = 256;

            //if ((uint)sizeHint > JsonSharedConstant.TooBigOrNegative) ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.sizeHint);
            //if (sizeHint == 0)
            if (unchecked((uint)(sizeHint - 1)) > JsonSharedConstant.TooBigOrNegative)
            {
                sizeHint = MinimumBufferSize;
            }

            int availableSpace = _capacity - alreadyWritten;

            if ((uint)sizeHint > (uint)availableSpace)
            {
                int growBy = Math.Max(sizeHint, _capacity);

                int newSize = checked(_capacity + growBy);

                var oldBuffer = _borrowedBuffer;

                var useThreadLocal = _arrayPool is null ? true : false;
                if (useThreadLocal) { _arrayPool = ArrayPool<byte>.Shared; }

                _utf8Span = _borrowedBuffer = _arrayPool.Rent(newSize);

                Debug.Assert(oldBuffer.Length >= alreadyWritten);
                Debug.Assert(_borrowedBuffer.Length >= alreadyWritten);

                var previousBuffer = oldBuffer.AsSpan(0, alreadyWritten);
                previousBuffer.CopyTo(_borrowedBuffer);
                //previousBuffer.Clear();

                //BinaryUtil.CopyMemory(oldBuffer, 0, _borrowedBuffer, 0, alreadyWritten);

                _capacity = _borrowedBuffer.Length;

                if (!useThreadLocal)
                {
                    _arrayPool.Return(oldBuffer);
                }
            }
        }

        /// <summary>
        /// Writes the beginning of a JSON array.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the depth of the JSON has exceeded the maximum depth of 1000 
        /// OR if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        public void WriteStartArray()
        {
            WriteStart(JsonUtf8Constant.OpenBracket);
            _tokenType = JsonTokenType.BeginArray;
        }

        /// <summary>
        /// Writes the beginning of a JSON object.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the depth of the JSON has exceeded the maximum depth of 1000 
        /// OR if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        public void WriteStartObject()
        {
            WriteStart(JsonUtf8Constant.OpenBrace);
            _tokenType = JsonTokenType.BeginObject;
        }

        private void WriteStart(byte token)
        {
            if (CurrentDepth >= JsonSharedConstant.MaxWriterDepth)
                SysJsonThrowHelper.ThrowInvalidOperationException(ExceptionResource.DepthTooLarge, _currentDepth, token: default, tokenType: default);

            if (_options.IndentedOrNotSkipValidation)
            {
                WriteStartSlow(token);
            }
            else
            {
                WriteStartMinimized(token);
            }

            _currentDepth &= JsonSharedConstant.RemoveFlagsBitMask;
            _currentDepth++;
        }

        private void WriteStartMinimized(byte token)
        {
            ref var pos = ref _pos;
            EnsureUnsafe(pos, 2); // 1 start token, and optionally, 1 list separator


            ref byte output = ref PinnableAddress;
            if ((uint)_currentDepth > JsonSharedConstant.TooBigOrNegative)
            {
                Unsafe.AddByteOffset(ref output, (IntPtr)pos++) = JsonUtf8Constant.ListSeparator;
            }
            Unsafe.AddByteOffset(ref output, (IntPtr)pos++) = token;
        }

        private void WriteStartSlow(byte token)
        {
            Debug.Assert(_options.Indented || !_options.SkipValidation);

            if (_options.Indented)
            {
                if (!_options.SkipValidation)
                {
                    ValidateStart();
                    UpdateBitStackOnStart(token);
                }
                WriteStartIndented(token);
            }
            else
            {
                Debug.Assert(!_options.SkipValidation);
                ValidateStart();
                UpdateBitStackOnStart(token);
                WriteStartMinimized(token);
            }
        }

        private void ValidateStart()
        {
            if (_inObject)
            {
                if (_tokenType != JsonTokenType.PropertyName)
                {
                    Debug.Assert(_tokenType != JsonTokenType.None && _tokenType != JsonTokenType.BeginArray);
                    SysJsonThrowHelper.ThrowInvalidOperationException(ExceptionResource.CannotStartObjectArrayWithoutProperty, currentDepth: default, token: default, _tokenType);
                }
            }
            else
            {
                Debug.Assert(_tokenType != JsonTokenType.PropertyName);
                Debug.Assert(_tokenType != JsonTokenType.BeginObject);

                // It is more likely for CurrentDepth to not equal 0 when writing valid JSON, so check that first to rely on short-circuiting and return quickly.
                if (0u >= (uint)CurrentDepth && _tokenType != JsonTokenType.None)
                {
                    SysJsonThrowHelper.ThrowInvalidOperationException(ExceptionResource.CannotStartObjectArrayAfterPrimitiveOrClose, currentDepth: default, token: default, _tokenType);
                }
            }
        }

        private void WriteStartIndented(byte token)
        {
            int indent = Indentation;
            Debug.Assert(indent <= 2 * JsonSharedConstant.MaxWriterDepth);

            int minRequired = indent + 1;   // 1 start token
            int maxRequired = minRequired + 3; // Optionally, 1 list separator and 1-2 bytes for new line

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

            Unsafe.Add(ref output, pos++) = token;
        }

        /// <summary>
        /// Writes the beginning of a JSON array with a pre-encoded property name as the key.
        /// </summary>
        /// <param name="propertyName">The JSON-encoded name of the property to write.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the depth of the JSON has exceeded the maximum depth of 1000 
        /// OR if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        public void WriteStartArray(in JsonEncodedText propertyName)
        {
            WriteStartHelper(propertyName.EncodedUtf8Bytes, JsonUtf8Constant.OpenBracket);
            _tokenType = JsonTokenType.BeginArray;
        }

        /// <summary>
        /// Writes the beginning of a JSON object with a pre-encoded property name as the key.
        /// </summary>
        /// <param name="propertyName">The JSON-encoded name of the property to write.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the depth of the JSON has exceeded the maximum depth of 1000 
        /// OR if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        public void WriteStartObject(in JsonEncodedText propertyName)
        {
            WriteStartHelper(propertyName.EncodedUtf8Bytes, JsonUtf8Constant.OpenBrace);
            _tokenType = JsonTokenType.BeginObject;
        }

        private void WriteStartHelper(in ReadOnlySpan<byte> utf8PropertyName, byte token)
        {
            Debug.Assert(utf8PropertyName.Length <= JsonSharedConstant.MaxUnescapedTokenSize);

            ValidateDepth();

            WriteStartByOptions(utf8PropertyName, token);

            _currentDepth &= JsonSharedConstant.RemoveFlagsBitMask;
            _currentDepth++;
        }

        /// <summary>
        /// Writes the beginning of a JSON array with a property name as the key.
        /// </summary>
        /// <param name="utf8PropertyName">The UTF-8 encoded property name of the JSON array to be written.</param>
        /// <remarks>
        /// The property name is escaped before writing.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the depth of the JSON has exceeded the maximum depth of 1000 
        /// OR if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        public void WriteStartArray(in ReadOnlySpan<byte> utf8PropertyName)
        {
            ValidatePropertyNameAndDepth(utf8PropertyName);

            WriteStartEscape(utf8PropertyName, JsonUtf8Constant.OpenBracket);

            _currentDepth &= JsonSharedConstant.RemoveFlagsBitMask;
            _currentDepth++;
            _tokenType = JsonTokenType.BeginArray;
        }

        /// <summary>
        /// Writes the beginning of a JSON object with a property name as the key.
        /// </summary>
        /// <param name="utf8PropertyName">The UTF-8 encoded property name of the JSON object to be written.</param>
        /// <remarks>
        /// The property name is escaped before writing.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the depth of the JSON has exceeded the maximum depth of 1000 
        /// OR if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        public void WriteStartObject(in ReadOnlySpan<byte> utf8PropertyName)
        {
            ValidatePropertyNameAndDepth(utf8PropertyName);

            WriteStartEscape(utf8PropertyName, JsonUtf8Constant.OpenBrace);

            _currentDepth &= JsonSharedConstant.RemoveFlagsBitMask;
            _currentDepth++;
            _tokenType = JsonTokenType.BeginObject;
        }

        private void WriteStartEscape(in ReadOnlySpan<byte> utf8PropertyName, byte token)
        {
            int propertyIdx = EscapingHelper.NeedsEscaping(utf8PropertyName, _options.EscapeHandling, _options.Encoder);

            Debug.Assert(propertyIdx >= -1 && propertyIdx < utf8PropertyName.Length);

            if (propertyIdx != -1)
            {
                WriteStartEscapeProperty(utf8PropertyName, token, propertyIdx);
            }
            else
            {
                WriteStartByOptions(utf8PropertyName, token);
            }
        }

        private void WriteStartByOptions(in ReadOnlySpan<byte> utf8PropertyName, byte token)
        {
            ValidateWritingProperty(token);

            if (_options.Indented)
            {
                WritePropertyNameIndented(utf8PropertyName, token);
            }
            else
            {
                WritePropertyNameMinimized(utf8PropertyName, token);
            }
        }

        private void WriteStartEscapeProperty(in ReadOnlySpan<byte> utf8PropertyName, byte token, int firstEscapeIndexProp)
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
            WriteStartByOptions(MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(escapedPropertyName), written), token);
#else
            unsafe
            {
                WriteStartByOptions(new ReadOnlySpan<byte>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(escapedPropertyName)), written), token);
            }
#endif

            if (propertyArray is object)
            {
                ArrayPool<byte>.Shared.Return(propertyArray);
            }
        }

        /// <summary>
        /// Writes the beginning of a JSON array with a property name as the key.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <remarks>
        /// The property name is escaped before writing.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="propertyName"/> parameter is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the depth of the JSON has exceeded the maximum depth of 1000 
        /// OR if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        public void WriteStartArray(string propertyName)
        {
            if (propertyName is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.propertyName); }
            WriteStartArray(propertyName.AsSpan());
        }

        /// <summary>
        /// Writes the beginning of a JSON object with a property name as the key.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <remarks>
        /// The property name is escaped before writing.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="propertyName"/> parameter is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the depth of the JSON has exceeded the maximum depth of 1000 
        /// OR if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        public void WriteStartObject(string propertyName)
        {
            if (propertyName is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.propertyName); }
            WriteStartObject(propertyName.AsSpan());
        }

        /// <summary>
        /// Writes the beginning of a JSON array with a property name as the key.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <remarks>
        /// The property name is escaped before writing.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the depth of the JSON has exceeded the maximum depth of 1000 
        /// OR if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        public void WriteStartArray(in ReadOnlySpan<char> propertyName)
        {
            ValidatePropertyNameAndDepth(propertyName);

            WriteStartEscape(propertyName, JsonUtf8Constant.OpenBracket);

            _currentDepth &= JsonSharedConstant.RemoveFlagsBitMask;
            _currentDepth++;
            _tokenType = JsonTokenType.BeginArray;
        }

        /// <summary>
        /// Writes the beginning of a JSON object with a property name as the key.
        /// </summary>
        /// <param name="propertyName">The name of the property to write.</param>
        /// <remarks>
        /// The property name is escaped before writing.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when the specified property name is too large.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the depth of the JSON has exceeded the maximum depth of 1000 
        /// OR if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        public void WriteStartObject(in ReadOnlySpan<char> propertyName)
        {
            ValidatePropertyNameAndDepth(propertyName);

            WriteStartEscape(propertyName, JsonUtf8Constant.OpenBrace);

            _currentDepth &= JsonSharedConstant.RemoveFlagsBitMask;
            _currentDepth++;
            _tokenType = JsonTokenType.BeginObject;
        }

        private void WriteStartEscape(in ReadOnlySpan<char> propertyName, byte token)
        {
            int propertyIdx = EscapingHelper.NeedsEscaping(propertyName, _options.EscapeHandling, _options.Encoder);

            Debug.Assert(propertyIdx >= -1 && propertyIdx < propertyName.Length);

            if (propertyIdx != -1)
            {
                WriteStartEscapeProperty(propertyName, token, propertyIdx);
            }
            else
            {
                WriteStartByOptions(propertyName, token);
            }
        }

        private void WriteStartByOptions(in ReadOnlySpan<char> propertyName, byte token)
        {
            ValidateWritingProperty(token);

            if (_options.Indented)
            {
                WritePropertyNameIndented(propertyName, token);
            }
            else
            {
                WritePropertyNameMinimized(propertyName, token);
            }
        }

        private void WriteStartEscapeProperty(in ReadOnlySpan<char> propertyName, byte token, int firstEscapeIndexProp)
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
            WriteStartByOptions(MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(escapedPropertyName), written), token);
#else
            unsafe
            {
                WriteStartByOptions(new ReadOnlySpan<char>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(escapedPropertyName)), written), token);
            }
#endif

            if (propertyArray is object)
            {
                ArrayPool<char>.Shared.Return(propertyArray);
            }
        }

        /// <summary>
        /// Writes the end of a JSON array.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        public void WriteEndArray()
        {
            WriteEnd(JsonUtf8Constant.CloseBracket);
            _tokenType = JsonTokenType.EndArray;
        }

        /// <summary>
        /// Writes the end of a JSON object.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this would result in invalid JSON being written (while validation is enabled).
        /// </exception>
        public void WriteEndObject()
        {
            WriteEnd(JsonUtf8Constant.CloseBrace);
            _tokenType = JsonTokenType.EndObject;
        }

        private void WriteEnd(byte token)
        {
            if (_options.IndentedOrNotSkipValidation)
            {
                WriteEndSlow(token);
            }
            else
            {
                WriteEndMinimized(token);
            }

            SetFlagToAddListSeparatorBeforeNextItem();
            // Necessary if WriteEndX is called without a corresponding WriteStartX first.
            if (CurrentDepth != 0)
            {
                _currentDepth--;
            }
        }

        private void WriteEndMinimized(byte token)
        {
            ref var pos = ref _pos;
            EnsureUnsafe(pos, 1); // 1 end token

            ref byte output = ref PinnableAddress;
            Unsafe.Add(ref output, pos++) = token;
        }

        private void WriteEndSlow(byte token)
        {
            Debug.Assert(_options.Indented || !_options.SkipValidation);

            if (_options.Indented)
            {
                if (!_options.SkipValidation)
                {
                    ValidateEnd(token);
                }
                WriteEndIndented(token);
            }
            else
            {
                Debug.Assert(!_options.SkipValidation);
                ValidateEnd(token);
                WriteEndMinimized(token);
            }
        }

        private void ValidateEnd(byte token)
        {
            if (_bitStack.CurrentDepth <= 0 || _tokenType == JsonTokenType.PropertyName)
                SysJsonThrowHelper.ThrowInvalidOperationException(ExceptionResource.MismatchedObjectArray, currentDepth: default, token, _tokenType);

            if (token == JsonUtf8Constant.CloseBracket)
            {
                if (_inObject)
                {
                    Debug.Assert(_tokenType != JsonTokenType.None);
                    SysJsonThrowHelper.ThrowInvalidOperationException(ExceptionResource.MismatchedObjectArray, currentDepth: default, token, _tokenType);
                }
            }
            else
            {
                Debug.Assert(token == JsonUtf8Constant.CloseBrace);

                if (!_inObject)
                {
                    SysJsonThrowHelper.ThrowInvalidOperationException(ExceptionResource.MismatchedObjectArray, currentDepth: default, token, _tokenType);
                }
            }

            _inObject = _bitStack.Pop();
        }

        private void WriteEndIndented(byte token)
        {
            // Do not format/indent empty JSON object/array.
            if (_tokenType == JsonTokenType.BeginObject || _tokenType == JsonTokenType.BeginArray)
            {
                WriteEndMinimized(token);
            }
            else
            {
                int indent = Indentation;

                // Necessary if WriteEndX is called without a corresponding WriteStartX first.
                if (indent != 0)
                {
                    // The end token should be at an outer indent and since we haven't updated
                    // current depth yet, explicitly subtract here.
                    indent -= JsonSharedConstant.SpacesPerIndent;
                }

                Debug.Assert(indent <= 2 * JsonSharedConstant.MaxWriterDepth);
                Debug.Assert(_options.SkipValidation || _tokenType != JsonTokenType.None);

                int maxRequired = indent + 3; // 1 end token, 1-2 bytes for new line

                ref var pos = ref _pos;
                EnsureUnsafe(pos, maxRequired);

                ref byte output = ref PinnableAddress;

                WriteNewLine(ref output, ref pos);

                JsonWriterHelper.WriteIndentation(ref output, indent, ref pos);

                Unsafe.Add(ref output, pos++) = token;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteNewLine(ref byte output, ref int pos)
        {
            // Write '\r\n' OR '\n', depending on OS
            if (JsonWriterHelper.NewLineLength == 2)
            {
                Unsafe.Add(ref output, pos++) = JsonUtf8Constant.CarriageReturn;
            }
            Unsafe.Add(ref output, pos++) = JsonUtf8Constant.LineFeed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateBitStackOnStart(byte token)
        {
            if (token == JsonUtf8Constant.OpenBracket)
            {
                _bitStack.PushFalse();
                _inObject = false;
            }
            else
            {
                Debug.Assert(token == JsonUtf8Constant.OpenBrace);
                _bitStack.PushTrue();
                _inObject = true;
            }
        }

        private void SetFlagToAddListSeparatorBeforeNextItem()
        {
            _currentDepth |= 1 << 31;
        }
    }
}
