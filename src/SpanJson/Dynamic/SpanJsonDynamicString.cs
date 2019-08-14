using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using CuteAnt;
using SpanJson.Internal;

namespace SpanJson.Dynamic
{
    public abstract partial class SpanJsonDynamicString<TSymbol> : SpanJsonDynamic<TSymbol> where TSymbol : struct
    {
        private static readonly DynamicTypeConverter DynamicConverter = new DynamicTypeConverter();

        private static readonly HashSet<Type> NumberTypes = new HashSet<Type>
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

        protected SpanJsonDynamicString(in ReadOnlySpan<TSymbol> span, bool isFloat) : base(span, isFloat) { }

        protected SpanJsonDynamicString(in ArraySegment<TSymbol> data, bool isFloat) : base(data, isFloat) { }

        protected override BaseDynamicTypeConverter<TSymbol> Converter => DynamicConverter;

        public override bool TryConvert(Type outputType, out object result)
        {
            var jsonRaw = Symbols;
            if (NumberTypes.Contains(outputType))
            {
                return Converter.TryConvertTo(outputType, jsonRaw.Slice(1, jsonRaw.Count - 2), out result);
            }
            return Converter.TryConvertTo(outputType, jsonRaw, out result);
        }

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

                    if (destinationType.IsEnum || (destinationType = Nullable.GetUnderlyingType(destinationType)) is object)
                    {
                        string data;
                        if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
                        {
                            data = TextEncodings.Utf8.GetStringWithCache(reader.ReadUtf8StringSpan());
                        }
                        else
                        {
                            data = reader.ReadString();
                        }
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
                catch (Exception) { }

                value = default;
                return false;
            }

            public override bool IsSupported(Type type)
            {
                var fix = Converters.ContainsKey(type) || type == typeof(string) || type.IsEnum;
                if (!fix)
                {
                    var nullable = Nullable.GetUnderlyingType(type);
                    if (nullable is object)
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
                    typeof(decimal),

                    typeof(char),
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(TimeSpan),
                    typeof(Guid),
                    typeof(CombGuid),
                    typeof(string),
                    typeof(Version),
                    typeof(Uri)
                };
                return BuildDelegates(allowedTypes);
            }
        }

        public static explicit operator SByte(SpanJsonDynamicString<TSymbol> input)
        {
            var jsonRaw = input.Symbols;
            if (DynamicConverter.TryConvertTo(typeof(SByte), jsonRaw.Slice(1, jsonRaw.Count - 2), out var value))
            {
                return (SByte)value;
            }
            throw ThrowHelper.GetInvalidCastException();
        }

        public static explicit operator Int16(SpanJsonDynamicString<TSymbol> input)
        {
            var jsonRaw = input.Symbols;
            if (DynamicConverter.TryConvertTo(typeof(Int16), jsonRaw.Slice(1, jsonRaw.Count - 2), out var value))
            {
                return (Int16)value;
            }
            throw ThrowHelper.GetInvalidCastException();
        }

        public static explicit operator Int32(SpanJsonDynamicString<TSymbol> input)
        {
            var jsonRaw = input.Symbols;
            if (DynamicConverter.TryConvertTo(typeof(Int32), jsonRaw.Slice(1, jsonRaw.Count - 2), out var value))
            {
                return (Int32)value;
            }
            throw ThrowHelper.GetInvalidCastException();
        }

        public static explicit operator Int64(SpanJsonDynamicString<TSymbol> input)
        {
            var jsonRaw = input.Symbols;
            if (DynamicConverter.TryConvertTo(typeof(Int64), jsonRaw.Slice(1, jsonRaw.Count - 2), out var value))
            {
                return (Int64)value;
            }
            throw ThrowHelper.GetInvalidCastException();
        }

        public static explicit operator Byte(SpanJsonDynamicString<TSymbol> input)
        {
            var jsonRaw = input.Symbols;
            if (DynamicConverter.TryConvertTo(typeof(Byte), jsonRaw.Slice(1, jsonRaw.Count - 2), out var value))
            {
                return (Byte)value;
            }
            throw ThrowHelper.GetInvalidCastException();
        }

        public static explicit operator UInt16(SpanJsonDynamicString<TSymbol> input)
        {
            var jsonRaw = input.Symbols;
            if (DynamicConverter.TryConvertTo(typeof(UInt16), jsonRaw.Slice(1, jsonRaw.Count - 2), out var value))
            {
                return (UInt16)value;
            }
            throw ThrowHelper.GetInvalidCastException();
        }

        public static explicit operator UInt32(SpanJsonDynamicString<TSymbol> input)
        {
            var jsonRaw = input.Symbols;
            if (DynamicConverter.TryConvertTo(typeof(UInt32), jsonRaw.Slice(1, jsonRaw.Count - 2), out var value))
            {
                return (UInt32)value;
            }
            throw ThrowHelper.GetInvalidCastException();
        }

        public static explicit operator UInt64(SpanJsonDynamicString<TSymbol> input)
        {
            var jsonRaw = input.Symbols;
            if (DynamicConverter.TryConvertTo(typeof(UInt64), jsonRaw.Slice(1, jsonRaw.Count - 2), out var value))
            {
                return (UInt64)value;
            }
            throw ThrowHelper.GetInvalidCastException();
        }

        public static explicit operator Single(SpanJsonDynamicString<TSymbol> input)
        {
            var jsonRaw = input.Symbols;
            if (DynamicConverter.TryConvertTo(typeof(Single), jsonRaw.Slice(1, jsonRaw.Count - 2), out var value))
            {
                return (Single)value;
            }
            throw ThrowHelper.GetInvalidCastException();
        }

        public static explicit operator Double(SpanJsonDynamicString<TSymbol> input)
        {
            var jsonRaw = input.Symbols;
            if (DynamicConverter.TryConvertTo(typeof(Double), jsonRaw.Slice(1, jsonRaw.Count - 2), out var value))
            {
                return (Double)value;
            }
            throw ThrowHelper.GetInvalidCastException();
        }

        public static explicit operator Decimal(SpanJsonDynamicString<TSymbol> input)
        {
            var jsonRaw = input.Symbols;
            if (DynamicConverter.TryConvertTo(typeof(Decimal), jsonRaw.Slice(1, jsonRaw.Count - 2), out var value))
            {
                return (Decimal)value;
            }
            throw ThrowHelper.GetInvalidCastException();
        }


        public static explicit operator CombGuid(SpanJsonDynamicString<TSymbol> input)
        {
            if (DynamicConverter.TryConvertTo(typeof(CombGuid), input.Symbols, out var value))
            {
                return (CombGuid)value;
            }
            throw ThrowHelper.GetInvalidCastException();
        }

        public static explicit operator CombGuid?(SpanJsonDynamicString<TSymbol> input)
        {
            if (DynamicConverter.TryConvertTo(typeof(CombGuid?), input.Symbols, out var value))
            {
                return (CombGuid?)value;
            }
            throw ThrowHelper.GetInvalidCastException();
        }

        public static explicit operator String(SpanJsonDynamicString<TSymbol> input)
        {
            return input.ToString();
        }

        public static explicit operator Version(SpanJsonDynamicString<TSymbol> input)
        {
            var strValue = input.ToString();
            if (strValue is null) { return null; }
            return Version.Parse(strValue);
        }

        public static explicit operator Uri(SpanJsonDynamicString<TSymbol> input)
        {
            var strValue = input.ToString();
            if (strValue is null) { return null; }
            if (Uri.TryCreate(strValue, UriKind.RelativeOrAbsolute, out Uri value))
            {
                return value;
            }
            throw ThrowHelper.GetInvalidCastException();
        }

        private string _value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            if (_value is object) { return _value; }
            if (DynamicConverter.TryConvertTo(typeof(string), Symbols, out var value))
            {
                _value = (string)value;
                return _value;
            }

            //if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.ByteSize)
            //{
            //    var jsonRaw = Symbols;
            //    var temp = jsonRaw.Array;
            //    var bytes = Unsafe.As<TSymbol[], byte[]>(ref temp);
            //    _value = Encoding.UTF8.GetString(bytes, jsonRaw.Offset + 1, jsonRaw.Count - 2);
            //    return _value;
            //}

            //if ((uint)Unsafe.SizeOf<TSymbol>() == JsonSharedConstant.CharSize)
            //{
            //    var jsonRaw = Symbols;
            //    var temp = jsonRaw.Array;
            //    var chars = Unsafe.As<TSymbol[], char[]>(ref temp);
            //    _value = new string(chars, jsonRaw.Offset + 1, jsonRaw.Count - 2);
            //    return _value;
            //}

            throw ThrowHelper.GetNotSupportedException();
        }

        public override string ToJsonValue() => base.ToString(); // take the parent version as this ToString removes the double quotes
    }
}