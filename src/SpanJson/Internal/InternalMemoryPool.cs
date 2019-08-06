namespace SpanJson.Internal
{
    using System;
    using System.Runtime.CompilerServices;

    internal static class InternalMemoryPool<TSymbol> where TSymbol : struct
    {
        internal static readonly int InitialCapacity;

        static InternalMemoryPool()
        {
            InitialCapacity = 1 + ((64 * 1024 - 1) / Unsafe.SizeOf<TSymbol>());
        }

        [ThreadStatic]
        static TSymbol[] s_buffer = null;

        public static TSymbol[] GetBuffer()
        {
            if (s_buffer == null) { s_buffer = new TSymbol[InitialCapacity]; }
            return s_buffer;
        }
    }

    internal static class TinyMemoryPool<TSymbol> where TSymbol : struct
    {
        [ThreadStatic]
        static TSymbol[] s_buffer = null;

        public static TSymbol[] GetBuffer()
        {
            if (s_buffer == null) { s_buffer = new TSymbol[256]; }
            return s_buffer;
        }
    }
}
