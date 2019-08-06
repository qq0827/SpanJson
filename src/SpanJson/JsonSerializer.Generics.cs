using System;
using System.Runtime.CompilerServices;

namespace SpanJson
{
    /// <summary>Main Type for SpanJson Serializer</summary>
    public static partial class JsonSerializer
    {
        /// <summary>Generic part</summary>
        public static partial class Generic
        {
            /// <summary>This method is used for the nongeneric deserialize calls.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static T DeserializeFromByteArrayInternal<T, TSymbol, TResolver>(TSymbol[] input)
                where TResolver : IJsonFormatterResolver<TSymbol, TResolver>, new() where TSymbol : struct
            {
                return Inner<T, TSymbol, TResolver>.InnerDeserialize(input);
            }

            /// <summary>This method is used for the nongeneric deserialize calls.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static T DeserializeFromBufferInternal<T, TSymbol, TResolver>(in ArraySegment<TSymbol> input)
                where TResolver : IJsonFormatterResolver<TSymbol, TResolver>, new() where TSymbol : struct
            {
                return Inner<T, TSymbol, TResolver>.InnerDeserialize(input);
            }

            /// <summary>This method is used for the nongeneric deserialize calls.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static T DeserializeFromMemoryInternal<T, TSymbol, TResolver>(in ReadOnlyMemory<TSymbol> input)
                where TResolver : IJsonFormatterResolver<TSymbol, TResolver>, new() where TSymbol : struct
            {
                return Inner<T, TSymbol, TResolver>.InnerDeserialize(input);
            }

            /// <summary>This method is used for the nongeneric deserialize calls.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static T DeserializeInternal<T, TSymbol, TResolver>(in ReadOnlySpan<TSymbol> input)
                where TResolver : IJsonFormatterResolver<TSymbol, TResolver>, new() where TSymbol : struct
            {
                return Inner<T, TSymbol, TResolver>.InnerDeserialize(input);
            }
        }
    }
}