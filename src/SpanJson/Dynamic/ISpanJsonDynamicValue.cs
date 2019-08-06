using System;

namespace SpanJson.Dynamic
{
    public interface ISpanJsonDynamicValue<TSymbol> : ISpanJsonDynamic where TSymbol : struct
    {
        ArraySegment<TSymbol> Symbols { get; }
        bool TryConvert(Type outputType, out object result);
    }
}