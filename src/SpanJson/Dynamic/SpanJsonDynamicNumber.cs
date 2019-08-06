using System;
using System.Collections.Generic;

namespace SpanJson.Dynamic
{
    public abstract partial class SpanJsonDynamicNumber<TSymbol> : SpanJsonDynamic<TSymbol> where TSymbol : struct
    {
        private static readonly DynamicTypeConverter DynamicConverter = new DynamicTypeConverter();

        protected SpanJsonDynamicNumber(in ReadOnlySpan<TSymbol> span, bool isFloat) : base(span, isFloat) { }

        protected SpanJsonDynamicNumber(in ArraySegment<TSymbol> data, bool isFloat) : base(data, isFloat) { }

        protected override BaseDynamicTypeConverter<TSymbol> Converter => DynamicConverter;

        public sealed class DynamicTypeConverter : BaseDynamicTypeConverter<TSymbol>
        {
            private static readonly Dictionary<Type, ConvertDelegate> Converters = BuildDelegates();


            public override bool TryConvertTo(Type destinationType, ReadOnlySpan<TSymbol> span, out object value)
            {
                try
                {
                    if (Converters.TryGetValue(destinationType, out var del))
                    {
                        var reader = new JsonReader<TSymbol>(span);
                        value = del(ref reader);
                        return true;
                    }
                    if (destinationType == typeof(string))
                    {
                        value = this.ToString();
                        return true;
                    }
                }
                catch { }

                value = default;
                return false;
            }

            public override bool IsSupported(Type type)
            {
                var fix = Converters.ContainsKey(type);
                if (!fix)
                {
                    var nullable = Nullable.GetUnderlyingType(type);
                    if (nullable != null)
                    {
                        fix |= IsSupported(nullable);
                    }
                }

                return fix;
            }

            private static Dictionary<Type, ConvertDelegate> BuildDelegates()
            {
                var allowedTypes = new[]
                {
                    typeof(sbyte),
                    typeof(short),
                    typeof(int),
                    typeof(long),
                    typeof(byte),
                    typeof(ushort),
                    typeof(uint),
                    typeof(ulong),
                    typeof(float),
                    typeof(double),
                    typeof(decimal)
                };
                return BuildDelegates(allowedTypes);
            }
        }
    }
}