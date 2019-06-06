using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace SpanJson.Formatters.Dynamic
{
    public abstract partial class SpanJsonDynamicString<TSymbol> : SpanJsonDynamic<TSymbol> where TSymbol : struct
    {
        private static readonly DynamicTypeConverter DynamicConverter = new DynamicTypeConverter();

        protected SpanJsonDynamicString(in ReadOnlySpan<TSymbol> span) : base(span)
        {
        }

        protected override BaseDynamicTypeConverter<TSymbol> Converter => DynamicConverter;

        public sealed class DynamicTypeConverter : BaseDynamicTypeConverter<TSymbol>
        {
            private static readonly Dictionary<Type, ConvertDelegate> Converters = BuildDelegates();

            public override bool TryConvertTo(Type destinationType, ReadOnlySpan<TSymbol> span, out object value)
            {
                try
                {
                    var reader = new JsonReader<TSymbol>(span);
                    if (Converters.TryGetValue(destinationType, out var del))
                    {
                        value = del(ref reader);
                        return true;
                    }

                    if (destinationType == typeof(string))
                    {
                        value = reader.ReadString();
                        return true;
                    }

                    if (destinationType.IsEnum || (destinationType = Nullable.GetUnderlyingType(destinationType)) != null)
                    {
                        var data = reader.ReadString();
#if NETSTANDARD2_0 || NET471 || NET451
                        value = Enum.Parse(destinationType, data, false);
                        return true;
#else
                        if (Enum.TryParse(destinationType, data, out var enumValue))
                        {
                            value = enumValue;
                            return true;
                        }
#endif
                    }
                }
                catch (Exception)
                {
                }

                value = default;
                return false;
            }

            public override bool IsSupported(Type type)
            {
                var fix = Converters.ContainsKey(type) || type == typeof(string) || type.IsEnum;
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
                    typeof(char),
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(TimeSpan),
                    typeof(Guid),
                    typeof(string),
                    typeof(Version),
                    typeof(Uri)
                };
                return BuildDelegates(allowedTypes);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            {
                var temp = Symbols;
                var chars = Unsafe.As<TSymbol[], char[]>(ref temp);
                return new string(chars, 1, chars.Length - 2);
            }

            if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            {
                var temp = Symbols;
                var bytes = Unsafe.As<TSymbol[], byte[]>(ref temp);
                return Encoding.UTF8.GetString(bytes, 1, bytes.Length - 2);
            }

            throw ThrowHelper.GetNotSupportedException();
        }

        public override string ToJsonValue() => base.ToString(); // take the parent version as this ToString removes the double quotes
    }
}