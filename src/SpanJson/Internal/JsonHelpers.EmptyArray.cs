using System;
using System.Runtime.CompilerServices;

namespace SpanJson.Internal
{
    static partial class JsonHelpers
    {
        internal sealed class EmptyArray<T>
        {
            public static readonly T[] Instance;

            static EmptyArray()
            {
#if NET451
                Instance = new T[0];
#else
                Instance = Array.Empty<T>();
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Empty<T>() => EmptyArray<T>.Instance;
    }
}
