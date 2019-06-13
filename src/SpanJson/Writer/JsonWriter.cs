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
        private const uint c_maximumBufferSize = int.MaxValue;
        private const int c_minimumBufferSize = 256;
        private const uint c_stackallocThreshold = 256u;

        private static readonly int c_defaultBufferSize = 1 + ((64 * 1024 - 1) / Unsafe.SizeOf<TSymbol>());
        private static readonly ArrayPool<TSymbol> s_sharedPool = ArrayPool<TSymbol>.Shared;

        private bool _useThreadLocal;

        private TSymbol[] _borrowedBuffer;
        internal byte[] _utf8Buffer;
        internal char[] _utf16Buffer;
        internal int _capacity;

        internal int _pos;
        private int _depth;

        /// <summary>TBD</summary>
        public ref TSymbol PinnableAddress
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _borrowedBuffer[0];
        }

        /// <summary>TBD</summary>
        internal ref char PinnableUtf16Address
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _utf16Buffer[0];
        }

        /// <summary>TBD</summary>
        internal ref byte PinnableUtf8Address
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _utf8Buffer[0];
        }

        public int Position => _pos;

        public TSymbol[] Data => _borrowedBuffer;

        public int Capacity => _capacity;

        public int FreeCapacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _capacity - _pos;
        }

        public Span<TSymbol> FreeSpan
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _borrowedBuffer.AsSpan(_pos);
        }

        internal Span<char> Utf16Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _utf16Buffer.AsSpan(_pos);
        }

        internal Span<byte> Utf8Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _utf8Buffer.AsSpan(_pos);
        }

        /// <summary>Constructs a new <see cref="JsonWriter{TSymbol}"/> instance.</summary>
        public JsonWriter(bool useThreadLocalBuffer)
        {
            _pos = 0;
            _depth = 0;

            if (useThreadLocalBuffer)
            {
                _useThreadLocal = true;
                _borrowedBuffer = InternalMemoryPool<TSymbol>.GetBuffer();
            }
            else
            {
                _useThreadLocal = false; ;
                _borrowedBuffer = s_sharedPool.Rent(c_defaultBufferSize);
            }
            _utf8Buffer = _borrowedBuffer as byte[];
            _utf16Buffer = _borrowedBuffer as char[];
            _capacity = _borrowedBuffer.Length;
        }

        /// <summary>Constructs a new <see cref="JsonWriter{TSymbol}"/> instance.</summary>
        public JsonWriter(int initialCapacity)
        {
            if (((uint)(initialCapacity - 1)) > c_maximumBufferSize) { initialCapacity = c_defaultBufferSize; }

            _pos = 0;
            _depth = 0;

            _useThreadLocal = false;
            _borrowedBuffer = s_sharedPool.Rent(c_defaultBufferSize);
            _utf8Buffer = _borrowedBuffer as byte[];
            _utf16Buffer = _borrowedBuffer as char[];
            _capacity = _borrowedBuffer.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Dispose()
        {
            var toReturn = _borrowedBuffer;
            this = default; // for safety, to avoid using pooled array if this instance is erroneously appended to again
            if (toReturn != null)
            {
                s_sharedPool.Return(toReturn);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Ensure(int sizeHintt)
        {
            var alreadyWritten = _pos;
            if ((uint)sizeHintt >= (uint)(_capacity - alreadyWritten)) { CheckAndResizeBuffer(alreadyWritten, sizeHintt); }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Ensure(int alreadyWritten, int sizeHintt)
        {
            if ((uint)sizeHintt >= (uint)(_capacity - alreadyWritten)) { CheckAndResizeBuffer(alreadyWritten, sizeHintt); }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void CheckAndResizeBuffer(int alreadyWritten, int sizeHint)
        {
            Debug.Assert(_borrowedBuffer != null);

            //if (sizeHint < 0) ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.sizeHint);
            //if (sizeHint == 0)
            if (unchecked((uint)(sizeHint - 1)) > c_maximumBufferSize)
            {
                sizeHint = c_minimumBufferSize;
            }

            int availableSpace = _capacity - alreadyWritten;

            if (sizeHint > availableSpace)
            {
                int growBy = Math.Max(sizeHint, _capacity);

                int newSize = checked(_capacity + growBy);

                var oldBuffer = _borrowedBuffer;

                _borrowedBuffer = s_sharedPool.Rent(newSize);
                _utf8Buffer = _borrowedBuffer as byte[];
                _utf16Buffer = _borrowedBuffer as char[];

                Debug.Assert(oldBuffer.Length >= alreadyWritten);
                Debug.Assert(_borrowedBuffer.Length >= alreadyWritten);

                var previousBuffer = oldBuffer.AsSpan(0, alreadyWritten);
                previousBuffer.CopyTo(_borrowedBuffer);
                //previousBuffer.Clear();

                //BinaryUtil.CopyMemory(oldBuffer, 0, _borrowedBuffer, 0, alreadyWritten);

                _capacity = _borrowedBuffer.Length;

                if (_useThreadLocal)
                {
                    _useThreadLocal = false;
                }
                else
                {
                    s_sharedPool.Return(oldBuffer);
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
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16EndArray();
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8EndArray();
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBeginArray()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16BeginArray();
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8BeginArray();
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBeginObject()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16BeginObject();
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8BeginObject();
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteEndObject()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16EndObject();
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8EndObject();
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteValueSeparator()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16ValueSeparator();
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8ValueSeparator();
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNull()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16Null();
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8Null();
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteChar(char value, StringEscapeHandling escapeHandling)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16Char(value, escapeHandling);
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8Char(value, escapeHandling);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        /// <summary>The value should already be properly escaped.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteName(string name)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16Name(name.AsSpan());
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8Name(name.AsSpan());
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        /// <summary>The value should already be properly escaped.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteName(in ReadOnlySpan<char> name)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16Name(name);
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8Name(name);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteName(string name, StringEscapeHandling escapeHandling)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16Name(name.AsSpan(), escapeHandling);
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8Name(name.AsSpan(), escapeHandling);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteName(in ReadOnlySpan<char> name, StringEscapeHandling escapeHandling)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16Name(name, escapeHandling);
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8Name(name, escapeHandling);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteString(string value, StringEscapeHandling escapeHandling)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16String(value.AsSpan(), escapeHandling);
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8String(value.AsSpan(), escapeHandling);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteString(in ReadOnlySpan<char> value)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16String(value);
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8String(value);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteString(in ReadOnlySpan<char> value, StringEscapeHandling escapeHandling)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16String(value, escapeHandling);
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8String(value, escapeHandling);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteVerbatim(in ReadOnlySpan<TSymbol> values)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16Verbatim(MemoryMarshal.Cast<TSymbol, char>(values));
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8Verbatim(MemoryMarshal.Cast<TSymbol, byte>(values));
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNewLine()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16Verbatim(JsonUtf16Constant.NewLine);
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8Verbatim(JsonUtf8Constant.NewLine);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteIndentation(int count)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16Indentation(count);
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8Indentation(count);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteDoubleQuote()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16DoubleQuote();
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8DoubleQuote();
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNameSeparator()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16NameSeparator();
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8NameSeparator();
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
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16VerbatimNameSpan(MemoryMarshal.Cast<TSymbol, char>(values));
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8VerbatimNameSpan(MemoryMarshal.Cast<TSymbol, byte>(values));
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteVerbatimNameSpan(in ReadOnlySpan<TSymbol> values, StringEscapeHandling escapeHandling)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                WriteUtf16VerbatimNameSpan(MemoryMarshal.Cast<TSymbol, char>(values), escapeHandling);
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                WriteUtf8VerbatimNameSpan(MemoryMarshal.Cast<TSymbol, byte>(values), escapeHandling);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }
    }
}
