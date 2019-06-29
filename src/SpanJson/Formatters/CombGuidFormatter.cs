using System;
using System.Buffers;
using System.Collections.Generic;
using CuteAnt;
using SpanJson.Helpers;
using SpanJson.Internal;

namespace SpanJson.Formatters
{
    public sealed class CombGuidUtf16Formatter : IJsonFormatter<CombGuid, Char>
    {
        public static readonly CombGuidUtf16Formatter Default = new CombGuidUtf16Formatter();

        public void Serialize(ref JsonWriter<Char> writer, CombGuid value, IJsonFormatterResolver<Char> resolver)
        {
            writer.WriteUtf16CombGuid(value);
        }

        public CombGuid Deserialize(ref JsonReader<Char> reader, IJsonFormatterResolver<Char> resolver)
        {
            return reader.ReadUtf16CombGuid();
        }
    }

    public sealed class NullableCombGuidUtf16Formatter : IJsonFormatter<CombGuid?, Char>
    {
        public static readonly NullableCombGuidUtf16Formatter Default = new NullableCombGuidUtf16Formatter();
        private static readonly CombGuidUtf16Formatter ElementFormatter = CombGuidUtf16Formatter.Default;
        public void Serialize(ref JsonWriter<Char> writer, CombGuid? value, IJsonFormatterResolver<Char> resolver)
        {
            if (value is null)
            {
                writer.WriteUtf16Null();
                return;
            }

            ElementFormatter.Serialize(ref writer, value.GetValueOrDefault(), resolver);
        }

        public CombGuid? Deserialize(ref JsonReader<Char> reader, IJsonFormatterResolver<Char> resolver)
        {
            if (reader.ReadUtf16IsNull())
            {
                return null;
            }

            return ElementFormatter.Deserialize(ref reader, resolver);
        }
    }

    public sealed class NullableCombGuidUtf16ArrayFormatter : IJsonFormatter<CombGuid?[], Char>
    {
        public static readonly NullableCombGuidUtf16ArrayFormatter Default = new NullableCombGuidUtf16ArrayFormatter();
        private static readonly NullableCombGuidUtf16Formatter ElementFormatter = NullableCombGuidUtf16Formatter.Default;
        public void Serialize(ref JsonWriter<Char> writer, CombGuid?[] value, IJsonFormatterResolver<Char> resolver)
        {
            if (value is null)
            {
                writer.WriteUtf16Null();
                return;
            }
            var valueLength = value.Length;
            writer.WriteUtf16BeginArray();
            if (valueLength > 0)
            {
                ElementFormatter.Serialize(ref writer, value[0], resolver);
                for (var i = 1; i < valueLength; i++)
                {
                    writer.WriteUtf16ValueSeparator();
                    ElementFormatter.Serialize(ref writer, value[i], resolver);
                }
            }

            writer.WriteUtf16EndArray();
        }

        public CombGuid?[] Deserialize(ref JsonReader<Char> reader, IJsonFormatterResolver<Char> resolver)
        {
            CombGuid?[] temp = null;
            CombGuid?[] result;
            try
            {
                if (reader.ReadUtf16IsNull())
                {
                    return null;
                }
                temp = ArrayPool<CombGuid?>.Shared.Rent(4);
                reader.ReadUtf16BeginArrayOrThrow();
                var count = 0;
                while (!reader.TryReadUtf16IsEndArrayOrValueSeparator(ref count)) // count is already preincremented, as it counts the separators
                {
                    if (count == temp.Length)
                    {
                        FormatterUtils.GrowArray(ref temp);
                    }

                    temp[count - 1] = ElementFormatter.Deserialize(ref reader, resolver);
                }

                if (0u >= (uint)count)
                {
                    result = JsonHelpers.Empty<CombGuid?>();
                }
                else
                {
                    result = FormatterUtils.CopyArray(temp, count);
                }
            }
            finally
            {
                if (temp != null)
                {
                    ArrayPool<CombGuid?>.Shared.Return(temp);
                }
            }

            return result;
        }
    }

    public sealed class NullableCombGuidUtf16ListFormatter : IJsonFormatter<List<CombGuid?>, Char>
    {
        public static readonly NullableCombGuidUtf16ListFormatter Default = new NullableCombGuidUtf16ListFormatter();
        private static readonly NullableCombGuidUtf16Formatter ElementFormatter = NullableCombGuidUtf16Formatter.Default;

        public void Serialize(ref JsonWriter<Char> writer, List<CombGuid?> value, IJsonFormatterResolver<Char> resolver)
        {
            if (value is null)
            {
                writer.WriteUtf16Null();
                return;
            }
            var valueLength = value.Count;
            writer.WriteUtf16BeginArray();
            if (valueLength > 0)
            {
                ElementFormatter.Serialize(ref writer, value[0], resolver);
                for (var i = 1; i < valueLength; i++)
                {
                    writer.WriteUtf16ValueSeparator();
                    ElementFormatter.Serialize(ref writer, value[i], resolver);
                }
            }

            writer.WriteUtf16EndArray();
        }

        public List<CombGuid?> Deserialize(ref JsonReader<Char> reader, IJsonFormatterResolver<Char> resolver)
        {
            if (reader.ReadUtf16IsNull())
            {
                return null;
            }
            reader.ReadUtf16BeginArrayOrThrow();
            var list = new List<CombGuid?>();
            var count = 0;
            while (!reader.TryReadUtf16IsEndArrayOrValueSeparator(ref count))
            {
                list.Add(ElementFormatter.Deserialize(ref reader, resolver));
            }

            return list;
        }
    }

    public sealed class CombGuidUtf16ArrayFormatter : IJsonFormatter<CombGuid[], Char>
    {
        public static readonly CombGuidUtf16ArrayFormatter Default = new CombGuidUtf16ArrayFormatter();
        private static readonly CombGuidUtf16Formatter ElementFormatter = CombGuidUtf16Formatter.Default;
        public void Serialize(ref JsonWriter<Char> writer, CombGuid[] value, IJsonFormatterResolver<Char> resolver)
        {
            if (value is null)
            {
                writer.WriteUtf16Null();
                return;
            }
            var valueLength = value.Length;
            writer.WriteUtf16BeginArray();
            if (valueLength > 0)
            {
                ElementFormatter.Serialize(ref writer, value[0], resolver);
                for (var i = 1; i < valueLength; i++)
                {
                    writer.WriteUtf16ValueSeparator();
                    ElementFormatter.Serialize(ref writer, value[i], resolver);
                }
            }

            writer.WriteUtf16EndArray();
        }

        public CombGuid[] Deserialize(ref JsonReader<Char> reader, IJsonFormatterResolver<Char> resolver)
        {
            CombGuid[] temp = null;
            CombGuid[] result;
            try
            {
                if (reader.ReadUtf16IsNull())
                {
                    return null;
                }
                temp = ArrayPool<CombGuid>.Shared.Rent(4);
                reader.ReadUtf16BeginArrayOrThrow();
                var count = 0;
                while (!reader.TryReadUtf16IsEndArrayOrValueSeparator(ref count)) // count is already preincremented, as it counts the separators
                {
                    if (count == temp.Length)
                    {
                        FormatterUtils.GrowArray(ref temp);
                    }

                    temp[count - 1] = ElementFormatter.Deserialize(ref reader, resolver);
                }

                if (0u >= (uint)count)
                {
                    result = JsonHelpers.Empty<CombGuid>();
                }
                else
                {
                    result = FormatterUtils.CopyArray(temp, count);
                }
            }
            finally
            {
                if (temp != null)
                {
                    ArrayPool<CombGuid>.Shared.Return(temp);
                }
            }

            return result;
        }
    }

    public sealed class CombGuidUtf16ListFormatter : IJsonFormatter<List<CombGuid>, Char>
    {
        public static readonly CombGuidUtf16ListFormatter Default = new CombGuidUtf16ListFormatter();
        private static readonly CombGuidUtf16Formatter ElementFormatter = CombGuidUtf16Formatter.Default;

        public void Serialize(ref JsonWriter<Char> writer, List<CombGuid> value, IJsonFormatterResolver<Char> resolver)
        {
            if (value is null)
            {
                writer.WriteUtf16Null();
                return;
            }
            var valueLength = value.Count;
            writer.WriteUtf16BeginArray();
            if (valueLength > 0)
            {
                ElementFormatter.Serialize(ref writer, value[0], resolver);
                for (var i = 1; i < valueLength; i++)
                {
                    writer.WriteUtf16ValueSeparator();
                    ElementFormatter.Serialize(ref writer, value[i], resolver);
                }
            }

            writer.WriteUtf16EndArray();
        }

        public List<CombGuid> Deserialize(ref JsonReader<Char> reader, IJsonFormatterResolver<Char> resolver)
        {
            if (reader.ReadUtf16IsNull())
            {
                return null;
            }
            reader.ReadUtf16BeginArrayOrThrow();
            var list = new List<CombGuid>();
            var count = 0;
            while (!reader.TryReadUtf16IsEndArrayOrValueSeparator(ref count))
            {
                list.Add(ElementFormatter.Deserialize(ref reader, resolver));
            }

            return list;
        }
    }

    public sealed class CombGuidUtf8Formatter : IJsonFormatter<CombGuid, Byte>
    {
        public static readonly CombGuidUtf8Formatter Default = new CombGuidUtf8Formatter();

        public void Serialize(ref JsonWriter<Byte> writer, CombGuid value, IJsonFormatterResolver<Byte> resolver)
        {
            writer.WriteUtf8CombGuid(value);
        }

        public CombGuid Deserialize(ref JsonReader<Byte> reader, IJsonFormatterResolver<Byte> resolver)
        {
            return reader.ReadUtf8CombGuid();
        }
    }

    public sealed class NullableCombGuidUtf8Formatter : IJsonFormatter<CombGuid?, Byte>
    {
        public static readonly NullableCombGuidUtf8Formatter Default = new NullableCombGuidUtf8Formatter();
        private static readonly CombGuidUtf8Formatter ElementFormatter = CombGuidUtf8Formatter.Default;
        public void Serialize(ref JsonWriter<Byte> writer, CombGuid? value, IJsonFormatterResolver<Byte> resolver)
        {
            if (value is null)
            {
                writer.WriteUtf8Null();
                return;
            }

            ElementFormatter.Serialize(ref writer, value.GetValueOrDefault(), resolver);
        }

        public CombGuid? Deserialize(ref JsonReader<Byte> reader, IJsonFormatterResolver<Byte> resolver)
        {
            if (reader.ReadUtf8IsNull())
            {
                return null;
            }

            return ElementFormatter.Deserialize(ref reader, resolver);
        }
    }

    public sealed class NullableCombGuidUtf8ArrayFormatter : IJsonFormatter<CombGuid?[], Byte>
    {
        public static readonly NullableCombGuidUtf8ArrayFormatter Default = new NullableCombGuidUtf8ArrayFormatter();
        private static readonly NullableCombGuidUtf8Formatter ElementFormatter = NullableCombGuidUtf8Formatter.Default;
        public void Serialize(ref JsonWriter<Byte> writer, CombGuid?[] value, IJsonFormatterResolver<Byte> resolver)
        {
            if (value is null)
            {
                writer.WriteUtf8Null();
                return;
            }
            var valueLength = value.Length;
            writer.WriteUtf8BeginArray();
            if (valueLength > 0)
            {
                ElementFormatter.Serialize(ref writer, value[0], resolver);
                for (var i = 1; i < valueLength; i++)
                {
                    writer.WriteUtf8ValueSeparator();
                    ElementFormatter.Serialize(ref writer, value[i], resolver);
                }
            }

            writer.WriteUtf8EndArray();
        }

        public CombGuid?[] Deserialize(ref JsonReader<Byte> reader, IJsonFormatterResolver<Byte> resolver)
        {
            CombGuid?[] temp = null;
            CombGuid?[] result;
            try
            {
                if (reader.ReadUtf8IsNull())
                {
                    return null;
                }
                temp = ArrayPool<CombGuid?>.Shared.Rent(4);
                reader.ReadUtf8BeginArrayOrThrow();
                var count = 0;
                while (!reader.TryReadUtf8IsEndArrayOrValueSeparator(ref count)) // count is already preincremented, as it counts the separators
                {
                    if (count == temp.Length)
                    {
                        FormatterUtils.GrowArray(ref temp);
                    }

                    temp[count - 1] = ElementFormatter.Deserialize(ref reader, resolver);
                }

                if (0u >= (uint)count)
                {
                    result = JsonHelpers.Empty<CombGuid?>();
                }
                else
                {
                    result = FormatterUtils.CopyArray(temp, count);
                }
            }
            finally
            {
                if (temp != null)
                {
                    ArrayPool<CombGuid?>.Shared.Return(temp);
                }
            }

            return result;
        }
    }

    public sealed class NullableCombGuidUtf8ListFormatter : IJsonFormatter<List<CombGuid?>, Byte>
    {
        public static readonly NullableCombGuidUtf8ListFormatter Default = new NullableCombGuidUtf8ListFormatter();
        private static readonly NullableCombGuidUtf8Formatter ElementFormatter = NullableCombGuidUtf8Formatter.Default;

        public void Serialize(ref JsonWriter<Byte> writer, List<CombGuid?> value, IJsonFormatterResolver<Byte> resolver)
        {
            if (value is null)
            {
                writer.WriteUtf8Null();
                return;
            }
            var valueLength = value.Count;
            writer.WriteUtf8BeginArray();
            if (valueLength > 0)
            {
                ElementFormatter.Serialize(ref writer, value[0], resolver);
                for (var i = 1; i < valueLength; i++)
                {
                    writer.WriteUtf8ValueSeparator();
                    ElementFormatter.Serialize(ref writer, value[i], resolver);
                }
            }

            writer.WriteUtf8EndArray();
        }

        public List<CombGuid?> Deserialize(ref JsonReader<Byte> reader, IJsonFormatterResolver<Byte> resolver)
        {
            if (reader.ReadUtf8IsNull())
            {
                return null;
            }
            reader.ReadUtf8BeginArrayOrThrow();
            var list = new List<CombGuid?>();
            var count = 0;
            while (!reader.TryReadUtf8IsEndArrayOrValueSeparator(ref count))
            {
                list.Add(ElementFormatter.Deserialize(ref reader, resolver));
            }

            return list;
        }
    }

    public sealed class CombGuidUtf8ArrayFormatter : IJsonFormatter<CombGuid[], Byte>
    {
        public static readonly CombGuidUtf8ArrayFormatter Default = new CombGuidUtf8ArrayFormatter();
        private static readonly CombGuidUtf8Formatter ElementFormatter = CombGuidUtf8Formatter.Default;
        public void Serialize(ref JsonWriter<Byte> writer, CombGuid[] value, IJsonFormatterResolver<Byte> resolver)
        {
            if (value is null)
            {
                writer.WriteUtf8Null();
                return;
            }
            var valueLength = value.Length;
            writer.WriteUtf8BeginArray();
            if (valueLength > 0)
            {
                ElementFormatter.Serialize(ref writer, value[0], resolver);
                for (var i = 1; i < valueLength; i++)
                {
                    writer.WriteUtf8ValueSeparator();
                    ElementFormatter.Serialize(ref writer, value[i], resolver);
                }
            }

            writer.WriteUtf8EndArray();
        }

        public CombGuid[] Deserialize(ref JsonReader<Byte> reader, IJsonFormatterResolver<Byte> resolver)
        {
            CombGuid[] temp = null;
            CombGuid[] result;
            try
            {
                if (reader.ReadUtf8IsNull())
                {
                    return null;
                }
                temp = ArrayPool<CombGuid>.Shared.Rent(4);
                reader.ReadUtf8BeginArrayOrThrow();
                var count = 0;
                while (!reader.TryReadUtf8IsEndArrayOrValueSeparator(ref count)) // count is already preincremented, as it counts the separators
                {
                    if (count == temp.Length)
                    {
                        FormatterUtils.GrowArray(ref temp);
                    }

                    temp[count - 1] = ElementFormatter.Deserialize(ref reader, resolver);
                }

                if (0u >= (uint)count)
                {
                    result = JsonHelpers.Empty<CombGuid>();
                }
                else
                {
                    result = FormatterUtils.CopyArray(temp, count);
                }
            }
            finally
            {
                if (temp != null)
                {
                    ArrayPool<CombGuid>.Shared.Return(temp);
                }
            }

            return result;
        }
    }

    public sealed class CombGuidUtf8ListFormatter : IJsonFormatter<List<CombGuid>, Byte>
    {
        public static readonly CombGuidUtf8ListFormatter Default = new CombGuidUtf8ListFormatter();
        private static readonly CombGuidUtf8Formatter ElementFormatter = CombGuidUtf8Formatter.Default;

        public void Serialize(ref JsonWriter<Byte> writer, List<CombGuid> value, IJsonFormatterResolver<Byte> resolver)
        {
            if (value is null)
            {
                writer.WriteUtf8Null();
                return;
            }
            var valueLength = value.Count;
            writer.WriteUtf8BeginArray();
            if (valueLength > 0)
            {
                ElementFormatter.Serialize(ref writer, value[0], resolver);
                for (var i = 1; i < valueLength; i++)
                {
                    writer.WriteUtf8ValueSeparator();
                    ElementFormatter.Serialize(ref writer, value[i], resolver);
                }
            }

            writer.WriteUtf8EndArray();
        }

        public List<CombGuid> Deserialize(ref JsonReader<Byte> reader, IJsonFormatterResolver<Byte> resolver)
        {
            if (reader.ReadUtf8IsNull())
            {
                return null;
            }
            reader.ReadUtf8BeginArrayOrThrow();
            var list = new List<CombGuid>();
            var count = 0;
            while (!reader.TryReadUtf8IsEndArrayOrValueSeparator(ref count))
            {
                list.Add(ElementFormatter.Deserialize(ref reader, resolver));
            }

            return list;
        }
    }
}