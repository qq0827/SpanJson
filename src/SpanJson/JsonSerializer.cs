using System;

namespace SpanJson
{
    public static partial class JsonSerializer
    {
        /// <summary>Very primitive JSON pretty printer</summary>
        public static class PrettyPrinter
        {
            /// <summary>Pretty prints a json input with 2 space indentation.</summary>
            /// <param name="input">Input</param>
            /// <returns>String</returns>
            public static string Print(string input)
            {
                return Print(input.AsSpan());
            }

            /// <summary>Pretty prints a json input with 2 space indentation.</summary>
            /// <param name="input">Input</param>
            /// <returns>String</returns>
            public static string Print(in ReadOnlySpan<char> input)
            {
                var reader = new JsonReader<char>(input);
#if DEBUG
                var writer = new JsonWriter<char>(16);
#else
                var writer = new JsonWriter<char>(true);
#endif
                Print(ref reader, ref writer);
                return writer.ToString();
            }

            /// <summary>Pretty prints a json input with 2 space indentation.</summary>
            /// <param name="input">Input</param>
            /// <returns>Byte array</returns>
            public static byte[] Print(byte[] input)
            {
                var reader = new JsonReader<byte>(input);
#if DEBUG
                var writer = new JsonWriter<byte>(16);
#else
                var writer = new JsonWriter<byte>(true);
#endif
                Print(ref reader, ref writer);
                return writer.ToByteArray();
            }

            /// <summary>Pretty prints a json input with 2 space indentation.</summary>
            /// <param name="input">Input</param>
            /// <returns>Byte array</returns>
            public static byte[] Print(in ReadOnlySpan<byte> input)
            {
                var reader = new JsonReader<byte>(input);
#if DEBUG
                var writer = new JsonWriter<byte>(16);
#else
                var writer = new JsonWriter<byte>(true);
#endif
                Print(ref reader, ref writer);
                return writer.ToByteArray();
            }

            private static void Print<TSymbol>(ref JsonReader<TSymbol> reader, ref JsonWriter<TSymbol> writer) where TSymbol : struct
            {
                var token = reader.ReadNextToken();
                if (token == JsonTokenType.String)
                {
                    var span = reader.ReadVerbatimStringSpanUnsafe();
                    writer.WriteDoubleQuote();
                    writer.WriteVerbatim(span);
                    writer.WriteDoubleQuote();

                    var nextToken = reader.ReadNextToken();
                    if (nextToken == JsonTokenType.NameSeparator)
                    {
                        reader.SkipNextValue(nextToken);
                        writer.WriteNameSeparator();
                        writer.WriteIndentation(1);
                    }
                }
                Print(ref reader, ref writer, 0);
            }

            private static void Print<TSymbol>(ref JsonReader<TSymbol> reader, ref JsonWriter<TSymbol> writer, int indent) where TSymbol : struct
            {
                var token = reader.ReadNextToken();
                switch (token)
                {
                    case JsonTokenType.BeginObject:
                        {
                            reader.ReadBeginObjectOrThrow();
                            writer.WriteBeginObject();
                            writer.WriteNewLine();
                            var c = 0;
                            while (!reader.TryReadIsEndObjectOrValueSeparator(ref c))
                            {
                                if (c != 1)
                                {
                                    writer.WriteValueSeparator();
                                    writer.WriteNewLine();
                                }

                                writer.WriteIndentation(indent + 2);
                                writer.WriteVerbatimNameSpan(reader.ReadVerbatimNameSpan());
                                writer.WriteIndentation(1);
                                Print(ref reader, ref writer, indent + 2);
                            }
                            if (c > 0)
                            {
                                writer.WriteNewLine();
                                writer.WriteIndentation(indent);
                            }
                            else
                            {
                                writer._pos -= 2;
                            }
                            writer.WriteEndObject();
                            break;
                        }
                    case JsonTokenType.BeginArray:
                        {
                            reader.ReadBeginArrayOrThrow();
                            writer.WriteBeginArray();
                            writer.WriteNewLine();
                            var c = 0;
                            while (!reader.TryReadIsEndArrayOrValueSeparator(ref c))
                            {
                                if (c != 1)
                                {
                                    writer.WriteValueSeparator();
                                    writer.WriteNewLine();
                                }

                                writer.WriteIndentation(indent + 2);
                                Print(ref reader, ref writer, indent + 2);
                            }
                            if (c > 0)
                            {
                                writer.WriteNewLine();
                                writer.WriteIndentation(indent);
                            }
                            else
                            {
                                writer._pos -= 2;
                            }
                            writer.WriteEndArray();
                            break;
                        }
                    case JsonTokenType.Number:
                        {
                            var span = reader.ReadNumberSpan();
                            writer.WriteVerbatim(span);
                            break;
                        }
                    case JsonTokenType.String:
                        {
                            var span = reader.ReadVerbatimStringSpanUnsafe();
                            writer.WriteDoubleQuote();
                            writer.WriteVerbatim(span);
                            writer.WriteDoubleQuote();
                            break;
                        }
                    case JsonTokenType.True:
                    case JsonTokenType.False:
                        {
                            var value = reader.ReadBoolean();
                            writer.WriteBoolean(value);
                            break;
                        }
                    case JsonTokenType.Null:
                        {
                            reader.ReadIsNull();
                            writer.WriteNull();
                            break;
                        }
                }
            }
        }

        /// <summary>Minifies JSON</summary>
        public static class Minifier
        {
            /// <summary>Minifies the input</summary>
            /// <param name="input">Input</param>
            /// <returns>String</returns>
            public static string Minify(string input)
            {
                return Minify(input.AsSpan());
            }

            /// <summary>Minifies the input</summary>
            /// <param name="input">Input</param>
            /// <returns>String</returns>
            public static string Minify(in ReadOnlySpan<char> input)
            {
                var reader = new JsonReader<char>(input);
#if DEBUG
                var writer = new JsonWriter<char>(16);
#else
                var writer = new JsonWriter<char>(true);
#endif
                Minify(ref reader, ref writer);
                return writer.ToString();
            }

            /// <summary>Minifies the input</summary>
            /// <param name="input">Input</param>
            /// <returns>Byte array</returns>
            public static byte[] Minify(byte[] input)
            {
                var reader = new JsonReader<byte>(input);
#if DEBUG
                var writer = new JsonWriter<byte>(16);
#else
                var writer = new JsonWriter<byte>(true);
#endif
                Minify(ref reader, ref writer);
                return writer.ToByteArray();
            }

            /// <summary>Minifies the input</summary>
            /// <param name="input">Input</param>
            /// <returns>Byte array</returns>
            public static byte[] Minify(in ReadOnlySpan<byte> input)
            {
                var reader = new JsonReader<byte>(input);
#if DEBUG
                var writer = new JsonWriter<byte>(16);
#else
                var writer = new JsonWriter<byte>(true);
#endif
                Minify(ref reader, ref writer);
                return writer.ToByteArray();
            }

            private static void Minify<TSymbol>(ref JsonReader<TSymbol> reader, ref JsonWriter<TSymbol> writer) where TSymbol : struct
            {
                var token = reader.ReadNextToken();
                switch (token)
                {
                    case JsonTokenType.BeginObject:
                        {
                            reader.ReadBeginObjectOrThrow();
                            writer.WriteBeginObject();
                            var c = 0;
                            while (!reader.TryReadIsEndObjectOrValueSeparator(ref c))
                            {
                                if (c != 1)
                                {
                                    writer.WriteValueSeparator();
                                }

                                writer.WriteVerbatimNameSpan(reader.ReadVerbatimNameSpan());

                                Minify(ref reader, ref writer);
                            }
                            writer.WriteEndObject();
                            break;
                        }
                    case JsonTokenType.BeginArray:
                        {
                            reader.ReadBeginArrayOrThrow();
                            writer.WriteBeginArray();
                            var c = 0;
                            while (!reader.TryReadIsEndArrayOrValueSeparator(ref c))
                            {
                                if (c != 1)
                                {
                                    writer.WriteValueSeparator();
                                }

                                Minify(ref reader, ref writer);
                            }

                            writer.WriteEndArray();
                            break;
                        }
                    case JsonTokenType.Number:
                        {
                            var span = reader.ReadNumberSpan();
                            writer.WriteVerbatim(span);
                            break;
                        }
                    case JsonTokenType.String:
                        {
                            var span = reader.ReadVerbatimStringSpanUnsafe();
                            writer.WriteDoubleQuote();
                            writer.WriteVerbatim(span);
                            writer.WriteDoubleQuote();
                            break;
                        }
                    case JsonTokenType.True:
                    case JsonTokenType.False:
                        {
                            var value = reader.ReadBoolean();
                            writer.WriteBoolean(value);
                            break;
                        }
                    case JsonTokenType.Null:
                        {
                            reader.ReadIsNull();
                            writer.WriteNull();
                            break;
                        }
                }
            }
        }
    }
}