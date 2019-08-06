using System;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;

namespace SpanJson.Dynamic
{
    public abstract class SpanJsonDynamic<TSymbol> : DynamicObject, ISpanJsonDynamicValue<TSymbol> where TSymbol : struct
    {
        protected SpanJsonDynamic(in ReadOnlySpan<TSymbol> span, bool isFloat)
        {
            Symbols = new ArraySegment<TSymbol>(span.ToArray());
            IsFloat = isFloat;
        }

        protected SpanJsonDynamic(in ArraySegment<TSymbol> data, bool isFloat)
        {
            Symbols = data;
            IsFloat = isFloat;
        }

        [IgnoreDataMember]
        internal readonly bool IsFloat;

        [IgnoreDataMember]
        public ArraySegment<TSymbol> Symbols { get; }

        protected abstract BaseDynamicTypeConverter<TSymbol> Converter { get; }

        public virtual bool TryConvert(Type outputType, out object result)
        {
            return Converter.TryConvertTo(outputType, Symbols, out result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                var jsonRaw = Symbols;
                var temp = jsonRaw.Array;
                var bytes = Unsafe.As<TSymbol[], byte[]>(ref temp);
                return Encoding.UTF8.GetString(bytes, jsonRaw.Offset, jsonRaw.Count);
            }

            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                var jsonRaw = Symbols;
                var temp = jsonRaw.Array;
                var chars = Unsafe.As<TSymbol[], char[]>(ref temp);
                return new string(chars, jsonRaw.Offset, jsonRaw.Count);
            }

            throw ThrowHelper.GetNotSupportedException();
        }

        public virtual string ToJsonValue() => ToString();

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            return TryConvert(binder.ReturnType, out result);
        }
    }
}