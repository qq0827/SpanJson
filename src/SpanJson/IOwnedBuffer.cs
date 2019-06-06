namespace SpanJson
{
    using System;
    using System.Buffers;
    using System.Threading;
    using SpanJson.Internal;

    public interface IOwnedBuffer<TSymbol> : IDisposable where TSymbol : struct
    {
        int Count { get; }

        ArraySegment<TSymbol> Buffer { get; }

        ReadOnlyMemory<TSymbol> Memory { get; }

        ReadOnlySpan<TSymbol> Span { get; }
    }

    sealed class ArrayWrittenBuffer<TSymbol> : IOwnedBuffer<TSymbol> where TSymbol : struct
    {
        public static readonly ArrayWrittenBuffer<TSymbol> Empty = new ArrayWrittenBuffer<TSymbol>(null, JsonHelpers.Empty<TSymbol>(), 0);

        private ArrayPool<TSymbol> _arrayPool;
        private TSymbol[] _buffer;
        private readonly int _alreadyWritten;

        public ArrayWrittenBuffer(ArrayPool<TSymbol> arrayPool, TSymbol[] buffer, int alreadyWritten)
        {
            _arrayPool = arrayPool;
            _buffer = buffer;
            _alreadyWritten = alreadyWritten;
        }

        public int Count => _alreadyWritten;

        public ArraySegment<TSymbol> Buffer => new ArraySegment<TSymbol>(_buffer, 0, _alreadyWritten);

        public ReadOnlyMemory<TSymbol> Memory => new ReadOnlyMemory<TSymbol>(_buffer, 0, _alreadyWritten);

        public ReadOnlySpan<TSymbol> Span => new ReadOnlySpan<TSymbol>(_buffer, 0, _alreadyWritten);

        public void Dispose()
        {
            var arrayPool = Interlocked.Exchange(ref _arrayPool, null);
            if (arrayPool != null)
            {
                arrayPool.Return(_buffer);
                _buffer = null;
            }
        }
    }
}
