using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SpanJson.Internal;

namespace SpanJson
{
    public ref partial struct JsonReader<TSymbol> where TSymbol : struct
    {
        internal ArraySegment<char> _utf16Json;
        internal ArraySegment<byte> _utf8Json;
        internal readonly ReadOnlySpan<char> _utf16Span;
        internal readonly ReadOnlySpan<byte> _utf8Span;
        internal readonly uint _length;

        internal int _pos;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JsonReader(TSymbol[] input)
        {
            if (input is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.input); }

            _length = (uint)input.Length;
            _pos = 0;

            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                var utf8Json = Unsafe.As<TSymbol[], byte[]>(ref input);
                _utf8Span = new ReadOnlySpan<byte>(utf8Json);
                _utf8Json = new ArraySegment<byte>(utf8Json);
                _utf16Span = null;
                _utf16Json = default;
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                var utf16Json = Unsafe.As<TSymbol[], char[]>(ref input);
                _utf16Span = new ReadOnlySpan<char>(utf16Json);
                _utf16Json = new ArraySegment<char>(utf16Json);
                _utf8Json = default;
                _utf8Span = null;
            }
            else
            {
                throw ThrowHelper.GetNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JsonReader(ArraySegment<TSymbol> input)
        {
            _length = (uint)input.Count;
            _pos = 0;

            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                var utf8Json = Unsafe.As<ArraySegment<TSymbol>, ArraySegment<byte>>(ref input);
                _utf8Span = new ReadOnlySpan<byte>(utf8Json.Array, utf8Json.Offset, utf8Json.Count);
                _utf8Json = utf8Json;
                _utf16Span = null;
                _utf16Json = default;
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                var utf16Json = Unsafe.As<ArraySegment<TSymbol>, ArraySegment<char>>(ref input);
                _utf16Span = new ReadOnlySpan<char>(utf16Json.Array, utf16Json.Offset, utf16Json.Count);
                _utf16Json = utf16Json;
                _utf8Json = default;
                _utf8Span = null;
            }
            else
            {
                throw ThrowHelper.GetNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JsonReader(in ReadOnlyMemory<TSymbol> input)
        {
            _length = (uint)input.Length;
            _pos = 0;

            MemoryMarshal.TryGetArray(input, out ArraySegment<TSymbol> tmp);

            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                _utf8Json = Unsafe.As<ArraySegment<TSymbol>, ArraySegment<byte>>(ref tmp);
                _utf8Span = MemoryMarshal.Cast<TSymbol, byte>(input.Span);
                _utf16Json = default;
                _utf16Span = null;
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                _utf16Json = Unsafe.As<ArraySegment<TSymbol>, ArraySegment<char>>(ref tmp);
                _utf16Span = MemoryMarshal.Cast<TSymbol, char>(input.Span);
                _utf8Json = default;
                _utf8Span = null;
            }
            else
            {
                throw ThrowHelper.GetNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JsonReader(in ReadOnlySpan<TSymbol> input)
        {
            _length = (uint)input.Length;
            _pos = 0;
            _utf16Json = default;
            _utf8Json = default;

            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                _utf8Span = MemoryMarshal.Cast<TSymbol, byte>(input);
                _utf16Span = null;
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                _utf16Span = MemoryMarshal.Cast<TSymbol, char>(input);
                _utf8Span = null;
            }
            else
            {
                throw ThrowHelper.GetNotSupportedException();
            }
        }

        public int Position => _pos;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadBeginArrayOrThrow()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                ReadUtf8BeginArrayOrThrow();
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                ReadUtf16BeginArrayOrThrow();
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReadIsEndArrayOrValueSeparator(ref int count)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                return TryReadUtf8IsEndArrayOrValueSeparator(ref count);
            }

            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                return TryReadUtf16IsEndArrayOrValueSeparator(ref count);
            }

            throw ThrowHelper.GetNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object ReadDynamic()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                return ReadUtf8Dynamic();
            }

            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                return ReadUtf16Dynamic();
            }

            throw ThrowHelper.GetNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadIsNull()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                return ReadUtf8IsNull();
            }

            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                return ReadUtf16IsNull();
            }

            throw ThrowHelper.GetNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadEscapedName()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                return ReadUtf8EscapedName();
            }

            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                return ReadUtf16EscapedName();
            }

            throw ThrowHelper.GetNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TSymbol> ReadEscapedNameSpan()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                return MemoryMarshal.Cast<byte, TSymbol>(ReadUtf8EscapedNameSpan());
            }

            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                return MemoryMarshal.Cast<char, TSymbol>(ReadUtf16EscapedNameSpan());
            }

            throw ThrowHelper.GetNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TSymbol> ReadVerbatimNameSpan()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                //ref var pos = ref _pos;
                //ref byte bStart = ref MemoryMarshal.GetReference(_bytes);
                //SkipWhitespaceUtf8(ref bStart, ref pos, _nLength);
                return MemoryMarshal.Cast<byte, TSymbol>(ReadUtf8VerbatimNameSpan());
            }

            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                //SkipWhitespaceUtf16();
                return MemoryMarshal.Cast<char, TSymbol>(ReadUtf16VerbatimNameSpan());
            }

            throw ThrowHelper.GetNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReadIsEndObjectOrValueSeparator(ref int count)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                return TryReadUtf8IsEndObjectOrValueSeparator(ref count);
            }

            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                return TryReadUtf16IsEndObjectOrValueSeparator(ref count);
            }

            throw ThrowHelper.GetNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadBeginObjectOrThrow()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                ReadUtf8BeginObjectOrThrow();
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                ReadUtf16BeginObjectOrThrow();
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadEndObjectOrThrow()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                ReadUtf8EndObjectOrThrow();
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                ReadUtf16EndObjectOrThrow();
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadEndArrayOrThrow()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                ReadUtf8EndArrayOrThrow();
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                ReadUtf16EndArrayOrThrow();
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TSymbol> ReadStringSpan()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                return MemoryMarshal.Cast<byte, TSymbol>(ReadUtf8StringSpan());
            }

            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                return MemoryMarshal.Cast<char, TSymbol>(ReadUtf16StringSpan());
            }

            throw ThrowHelper.GetNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TSymbol> ReadVerbatimStringSpan()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                return MemoryMarshal.Cast<byte, TSymbol>(ReadUtf8VerbatimStringSpan());
            }

            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                return MemoryMarshal.Cast<char, TSymbol>(ReadUtf16VerbatimStringSpan());
            }

            throw ThrowHelper.GetNotSupportedException();
        }

        /// <summary>Doesn't skip whitespace, just for copying around in a token loop</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TSymbol> ReadVerbatimStringSpanUnsafe()
        {
            ref var pos = ref _pos;
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                ref byte bStart = ref MemoryMarshal.GetReference(_utf8Span);
                return MemoryMarshal.Cast<byte, TSymbol>(ReadUtf8StringSpanInternal(ref bStart, ref pos, _length, out _));
            }

            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                ref var cStart = ref MemoryMarshal.GetReference(_utf16Span);
                return MemoryMarshal.Cast<char, TSymbol>(ReadUtf16StringSpanInternal(ref cStart, ref pos, _length, out _));
            }

            throw ThrowHelper.GetNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SkipNextSegment()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                SkipNextUtf8Segment();
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                SkipNextUtf16Segment();
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SkipNextValue(JsonTokenType tokenType)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                SkipNextUtf8Value(tokenType);
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                SkipNextUtf16Value(tokenType);
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JsonTokenType ReadNextToken()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                return ReadUtf8NextToken();
            }

            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                return ReadUtf16NextToken();
            }

            throw ThrowHelper.GetNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TSymbol> ReadNumberSpan()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                return MemoryMarshal.Cast<byte, TSymbol>(ReadUtf8NumberSpan());
            }

            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                return MemoryMarshal.Cast<char, TSymbol>(ReadUtf16NumberSpan());
            }

            throw ThrowHelper.GetNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadSymbolOrThrow(TSymbol symbol)
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                ReadUtf8SymbolOrThrow(Unsafe.As<TSymbol, byte>(ref symbol));
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                ReadUtf16SymbolOrThrow(Unsafe.As<TSymbol, char>(ref symbol));
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CuteAnt.CombGuid ReadCombGuid()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                return ReadUtf8CombGuid();
            }

            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                return ReadUtf16CombGuid();
            }

            throw ThrowHelper.GetNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ReadBytesFromBase64()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                return ReadUtf8BytesFromBase64();
            }

            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                return ReadUtf16BytesFromBase64();
            }

            throw ThrowHelper.GetNotSupportedException();
        }
    }
}