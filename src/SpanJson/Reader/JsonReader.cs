using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpanJson
{
    public ref partial struct JsonReader<TSymbol> where TSymbol : struct
    {
        private readonly ReadOnlySpan<char> _chars;
        private readonly ReadOnlySpan<byte> _bytes;
        private readonly uint _length;

        private int _pos;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JsonReader(TSymbol[] input)
        {
            if (null == input) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.input); }

            _length = (uint)input.Length;
            _pos = 0;

            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                _bytes = new ReadOnlySpan<byte>((byte[])(object)input);
                _chars = null;
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                _chars = new ReadOnlySpan<char>((char[])(object)input);
                _bytes = null;
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

            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                _bytes = MemoryMarshal.Cast<TSymbol, byte>(input);
                _chars = null;
            }
            else if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                _chars = MemoryMarshal.Cast<TSymbol, char>(input);
                _bytes = null;
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
                ref byte bStart = ref MemoryMarshal.GetReference(_bytes);
                return MemoryMarshal.Cast<byte, TSymbol>(ReadUtf8StringSpanInternal(ref bStart, ref pos, _length, out _));
            }

            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                ref var cStart = ref MemoryMarshal.GetReference(_chars);
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
        public JsonToken ReadNextToken()
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
                return MemoryMarshal.Cast<byte, TSymbol>(ReadUtf8NumberInternal());
            }

            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                return MemoryMarshal.Cast<char, TSymbol>(ReadUtf16NumberInternal());
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
    }
}