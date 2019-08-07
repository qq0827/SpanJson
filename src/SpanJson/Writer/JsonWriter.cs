namespace SpanJson
{
    using System;
    using System.Buffers;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using SpanJson.Internal;

    public ref partial struct JsonWriter<TSymbol> where TSymbol : struct
    {
        private ArrayPool<TSymbol> _arrayPool;

        private TSymbol[] _borrowedBuffer;
        internal byte[] _utf8Buffer;
        private Span<byte> _utf8Span;
        internal char[] _utf16Buffer;
        private Span<char> _utf16Span;
        internal int _capacity;

        internal int _pos;
        private int _depth;

        /// <summary>TBD</summary>
        public ref char Utf16PinnableAddress
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref MemoryMarshal.GetReference(_utf16Span);
        }

        /// <summary>TBD</summary>
        public ref byte Utf8PinnableAddress
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref MemoryMarshal.GetReference(_utf8Span);
        }

        public int Position => _pos;

        public TSymbol[] Data => _borrowedBuffer;

        public int Capacity => _capacity;

        public int FreeCapacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _capacity - _pos;
        }

        /// <summary>Unsafe</summary>
        public ReadOnlySpan<char> Utf16WrittenSpan
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _utf16Span.Slice(0, _pos);
        }

        /// <summary>Unsafe</summary>
        public Span<char> Utf16FreeSpan
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _utf16Span.Slice(_pos);
        }

        /// <summary>Unsafe</summary>
        public ReadOnlySpan<byte> Utf8WrittenSpan
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _utf8Span.Slice(0, _pos);
        }

        /// <summary>Unsafe</summary>
        public Span<byte> Utf8FreeSpan
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _utf8Span.Slice(_pos);
        }

        /// <summary>Constructs a new <see cref="JsonWriter{TSymbol}"/> instance.</summary>
        public JsonWriter(bool useThreadLocalBuffer)
        {
            _pos = 0;
            _depth = 0;

            if (useThreadLocalBuffer)
            {
                _arrayPool = null;
                _borrowedBuffer = InternalMemoryPool<TSymbol>.GetBuffer();
            }
            else
            {
                _arrayPool = ArrayPool<TSymbol>.Shared;
                _borrowedBuffer = _arrayPool.Rent(InternalMemoryPool<TSymbol>.InitialCapacity);
            }
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                _utf8Span = _utf8Buffer = _borrowedBuffer as byte[];
                _utf16Span = _utf16Buffer = null;
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                _utf16Span = _utf16Buffer = _borrowedBuffer as char[];
                _utf8Span = _utf8Buffer = null;
            }
            else
            {
                throw ThrowHelper.GetNotSupportedException();
            }
            _capacity = _borrowedBuffer.Length;
        }

        /// <summary>Constructs a new <see cref="JsonWriter{TSymbol}"/> instance.</summary>
        public JsonWriter(int initialCapacity)
        {
            if (((uint)(initialCapacity - 1)) > JsonSharedConstant.TooBigOrNegative) { initialCapacity = InternalMemoryPool<TSymbol>.InitialCapacity; }

            _pos = 0;
            _depth = 0;

            _arrayPool = ArrayPool<TSymbol>.Shared;
            _borrowedBuffer = _arrayPool.Rent(initialCapacity);
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                _utf8Span = _utf8Buffer = _borrowedBuffer as byte[];
                _utf16Span = _utf16Buffer = null;
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                _utf16Span = _utf16Buffer = _borrowedBuffer as char[];
                _utf8Span = _utf8Buffer = null;
            }
            else
            {
                throw ThrowHelper.GetNotSupportedException();
            }
            _capacity = _borrowedBuffer.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Dispose()
        {
            var toReturn = _borrowedBuffer;
            var arrayPool = _arrayPool;
            this = default; // for safety, to avoid using pooled array if this instance is erroneously appended to again
            if (arrayPool != null)
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

            if (_pos > _capacity - count) { ThrowHelper.ThrowInvalidOperationException_AdvancedTooFar(_capacity); }

            _pos += count;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void CheckAndResizeBuffer(int alreadyWritten, int sizeHint)
        {
            Debug.Assert(_borrowedBuffer != null);

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

                var useThreadLocal = null == _arrayPool ? true : false;
                if (useThreadLocal) { _arrayPool = ArrayPool<TSymbol>.Shared; }

                _borrowedBuffer = _arrayPool.Rent(newSize);
                if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
                {
                    _utf8Span = _utf8Buffer = _borrowedBuffer as byte[];
                    _utf16Span = _utf16Buffer = null;
                }
                else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
                {
                    _utf16Span = _utf16Buffer = _borrowedBuffer as char[];
                    _utf8Span = _utf8Buffer = null;
                }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncrementDepth() => _depth++;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DecrementDepth() => _depth--;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AssertDepth()
        {
            if ((uint)_depth > JsonSharedConstant.NestingLimit)
            {
                ThrowHelper.ThrowInvalidOperationException_NestingLimitOfExceeded();
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteEndArray()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8EndArray();
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16EndArray();
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBeginArray()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8BeginArray();
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16BeginArray();
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBeginObject()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8BeginObject();
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16BeginObject();
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteEndObject()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8EndObject();
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16EndObject();
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValueSeparator()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8ValueSeparator();
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16ValueSeparator();
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNull()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8Null();
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16Null();
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChar(char value, JsonEscapeHandling escapeHandling)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8Char(value, escapeHandling);
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16Char(value, escapeHandling);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        /// <summary>The value should already be properly escaped.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteName(in JsonEncodedText name)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8Name(name);
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16Name(name);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteName(string name)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8Name(name);
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16Name(name);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteName(in ReadOnlySpan<char> name)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8Name(name);
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16Name(name);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteName(string name, JsonEscapeHandling escapeHandling)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8Name(name, escapeHandling);
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16Name(name, escapeHandling);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteName(in ReadOnlySpan<char> name, JsonEscapeHandling escapeHandling)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8Name(name, escapeHandling);
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16Name(name, escapeHandling);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteString(in JsonEncodedText value)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8String(value);
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16String(value);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteString(string value, JsonEscapeHandling escapeHandling)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8String(value, escapeHandling);
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16String(value, escapeHandling);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteString(in ReadOnlySpan<char> value)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8String(value);
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16String(value);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteString(in ReadOnlySpan<char> value, JsonEscapeHandling escapeHandling)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8String(value, escapeHandling);
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16String(value, escapeHandling);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteVerbatim(in ReadOnlySpan<TSymbol> values)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8Verbatim(MemoryMarshal.Cast<TSymbol, byte>(values));
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16Verbatim(MemoryMarshal.Cast<TSymbol, char>(values));
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNewLine()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8Verbatim(JsonUtf8Constant.NewLine);
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16Verbatim(JsonUtf16Constant.NewLine);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteIndentation(int count)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8Indentation(count);
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16Indentation(count);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteDoubleQuote()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8DoubleQuote();
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16DoubleQuote();
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNameSeparator()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8NameSeparator();
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16NameSeparator();
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        /// <summary>The value should already be properly escaped.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteVerbatimNameSpan(in ReadOnlySpan<TSymbol> values)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8VerbatimNameSpan(MemoryMarshal.Cast<TSymbol, byte>(values));
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16VerbatimNameSpan(MemoryMarshal.Cast<TSymbol, char>(values));
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteCombGuid(CuteAnt.CombGuid value)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8CombGuid(value);
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16CombGuid(value);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBase64String(in ReadOnlySpan<byte> bytes)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8Base64String(bytes);
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16Base64String(bytes);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }
    }
}
