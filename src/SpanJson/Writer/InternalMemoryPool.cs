namespace SpanJson
{
    using System;
    using System.Runtime.CompilerServices;

    static class InternalMemoryPool<TSymbol> where TSymbol : struct
    {
        private static readonly int s_initialCapacity;
        internal static readonly uint InitialCapacity;

        static InternalMemoryPool()
        {
            s_initialCapacity = 1 + ((64 * 1024 - 1) / Unsafe.SizeOf<TSymbol>());
            InitialCapacity = (uint)s_initialCapacity;
        }

        [ThreadStatic]
        static TSymbol[] s_buffer = null;

        public static TSymbol[] GetBuffer()
        {
            if (s_buffer == null) { s_buffer = new TSymbol[s_initialCapacity]; }
            return s_buffer;
        }
    }

    static class TinyMemoryPool<TSymbol> where TSymbol : struct
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
