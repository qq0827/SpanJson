// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;
using System;
using System.Buffers;
using System.IO;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
//using System.IO.Pipelines;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace SpanJson.Tests
{
    public class Utf8JsonWriterTests
    {
        private const int MaxExpansionFactorWhileEscaping = 6;
        private const int MaxEscapedTokenSize = 1_000_000_000;   // Max size for already escaped value.
        private const int MaxUnescapedTokenSize = MaxEscapedTokenSize / MaxExpansionFactorWhileEscaping;  // 166_666_666 bytes

        public static bool IsX64 { get; } = IntPtr.Size >= 8;

        private delegate void WriteAction(ref Utf8JsonWriter writer);

        private delegate void WriteStringAction(
            ref Utf8JsonWriter writer,
            string text);

        private delegate void WriteStringAction<T>(
            ref Utf8JsonWriter writer,
            string text,
            T value);

        private delegate void WriteValueSpanAction<T>(
            ref Utf8JsonWriter writer,
            ReadOnlySpan<T> value);

        private delegate void WritePropertySpanAction<T>(
            ref Utf8JsonWriter writer,
            ReadOnlySpan<T> propertyName);

        private delegate void WritePropertySpanAction<T1, T2>(
            ref Utf8JsonWriter writer,
            ReadOnlySpan<T1> propertyName,
            T2 value);

        private static void WriteNullValue_InObject(
            string wireValue,
            string nullValue,
            WriteStringAction stringAction,
            WriteValueSpanAction<char> charSpanAction,
            WriteValueSpanAction<byte> byteSpanAction)
        {
            string nullString = null;

            var writer = new Utf8JsonWriter(1024);
            writer.WriteStartObject();

            stringAction(ref writer, nullString);

            ReadOnlySpan<char> nullStringSpan = nullString.AsSpan();
            charSpanAction(ref writer, nullStringSpan);

            byteSpanAction(ref writer, ReadOnlySpan<byte>.Empty);

            writer.WriteEndObject();

            AssertContents($"{{{nullValue},{wireValue},{wireValue}}}", writer.ToByteArray());
        }

        private static void WriteNullValue_InArray(
            string wireValue,
            string nullValue,
            WriteStringAction stringAction,
            WriteValueSpanAction<char> charSpanAction,
            WriteValueSpanAction<byte> byteSpanAction)
        {
            string nullString = null;

            var writer = new Utf8JsonWriter(1024);
            writer.WriteStartArray();

            stringAction(ref writer, nullString);

            ReadOnlySpan<char> nullStringSpan = nullString.AsSpan();
            charSpanAction(ref writer, nullStringSpan);

            byteSpanAction(ref writer, ReadOnlySpan<byte>.Empty);

            writer.WriteEndArray();

            AssertContents($"[{nullValue},{wireValue},{wireValue}]", writer.ToByteArray());
        }

        private static string GetHelloWorldExpectedString(bool prettyPrint, string propertyName, string value)
        {
            var ms = new MemoryStream();
            TextWriter streamWriter = new StreamWriter(ms, new UTF8Encoding(false), 1024, true);

            var json = new JsonTextWriter(streamWriter)
            {
                Formatting = prettyPrint ? Formatting.Indented : Formatting.None,
                StringEscapeHandling = StringEscapeHandling.EscapeHtml
            };

            json.WriteStartObject();
            json.WritePropertyName(propertyName);
            json.WriteValue(value);
            json.WritePropertyName(propertyName);
            json.WriteValue(value);
            json.WriteEnd();

            json.Flush();

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private static string GetBase64ExpectedString(bool prettyPrint, string propertyName, byte[] value)
        {
            var ms = new MemoryStream();
            TextWriter streamWriter = new StreamWriter(ms, new UTF8Encoding(false), 1024, true);

            var json = new JsonTextWriter(streamWriter)
            {
                Formatting = prettyPrint ? Formatting.Indented : Formatting.None,
                StringEscapeHandling = StringEscapeHandling.EscapeHtml
            };

            json.WriteStartObject();
            json.WritePropertyName(propertyName);
            json.WriteValue(value);
            json.WritePropertyName(propertyName);
            json.WriteValue(value);
            json.WriteEnd();

            json.Flush();

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private static string GetCommentExpectedString(bool prettyPrint, string comment)
        {
            var ms = new MemoryStream();
            TextWriter streamWriter = new StreamWriter(ms, new UTF8Encoding(false), 1024, true);

            var json = new JsonTextWriter(streamWriter)
            {
                Formatting = prettyPrint ? Formatting.Indented : Formatting.None,
                StringEscapeHandling = StringEscapeHandling.EscapeHtml,
            };

            json.WriteStartArray();
            for (int j = 0; j < 10; j++)
                json.WriteComment(comment);
            json.WriteValue(comment);
            json.WriteComment(comment);
            json.WriteEnd();

            json.Flush();

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private static string GetStringsExpectedString(bool prettyPrint, string value)
        {
            var ms = new MemoryStream();
            TextWriter streamWriter = new StreamWriter(ms, new UTF8Encoding(false), 1024, true);

            var json = new JsonTextWriter(streamWriter)
            {
                Formatting = prettyPrint ? Formatting.Indented : Formatting.None
            };

            json.WriteStartArray();
            for (int j = 0; j < 10; j++)
                json.WriteValue(value);
            json.WriteEnd();

            json.Flush();

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private static string GetEscapedExpectedString(bool prettyPrint, string propertyName, string value, StringEscapeHandling escaping, bool escape = true)
        {
            using (TextWriter stringWriter = new StringWriter())
            using (var json = new JsonTextWriter(stringWriter)
            {
                Formatting = prettyPrint ? Formatting.Indented : Formatting.None,
                StringEscapeHandling = escaping
            })
            {
                json.WriteStartObject();
                json.WritePropertyName(propertyName, escape);
                json.WriteValue(value);
                json.WriteEnd();

                json.Flush();
                return stringWriter.ToString();
            }
        }

        private static string GetCustomExpectedString(bool prettyPrint)
        {
            var ms = new MemoryStream();
            TextWriter streamWriter = new StreamWriter(ms, new UTF8Encoding(false), 1024, true);

            var json = new JsonTextWriter(streamWriter)
            {
                Formatting = prettyPrint ? Formatting.Indented : Formatting.None
            };

            json.WriteStartObject();
            for (int i = 0; i < 1_000; i++)
            {
                json.WritePropertyName("message");
                json.WriteValue("Hello, World!");
            }
            json.WriteEnd();

            json.Flush();

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private static string GetStartEndExpectedString(bool prettyPrint)
        {
            var ms = new MemoryStream();
            TextWriter streamWriter = new StreamWriter(ms, new UTF8Encoding(false), 1024, true);

            var json = new JsonTextWriter(streamWriter)
            {
                Formatting = prettyPrint ? Formatting.Indented : Formatting.None
            };

            json.WriteStartArray();
            json.WriteStartObject();
            json.WriteEnd();
            json.WriteEnd();

            json.Flush();

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private static string GetStartEndWithPropertyArrayExpectedString(bool prettyPrint)
        {
            var ms = new MemoryStream();
            TextWriter streamWriter = new StreamWriter(ms, new UTF8Encoding(false), 1024, true);

            var json = new JsonTextWriter(streamWriter)
            {
                Formatting = prettyPrint ? Formatting.Indented : Formatting.None
            };

            json.WriteStartObject();
            json.WritePropertyName("property name");
            json.WriteStartArray();
            json.WriteEnd();
            json.WriteEnd();

            json.Flush();

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private static string GetStartEndWithPropertyArrayExpectedString(string key, bool prettyPrint, bool escape = false)
        {
            var ms = new MemoryStream();
            TextWriter streamWriter = new StreamWriter(ms, new UTF8Encoding(false), 1024, true);

            var json = new JsonTextWriter(streamWriter)
            {
                Formatting = prettyPrint ? Formatting.Indented : Formatting.None,
                StringEscapeHandling = StringEscapeHandling.EscapeHtml
            };

            json.WriteStartObject();
            json.WritePropertyName(key, escape);
            json.WriteStartArray();
            json.WriteEnd();
            json.WriteEnd();

            json.Flush();

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private static string GetStartEndWithPropertyObjectExpectedString(bool prettyPrint)
        {
            var ms = new MemoryStream();
            TextWriter streamWriter = new StreamWriter(ms, new UTF8Encoding(false), 1024, true);

            var json = new JsonTextWriter(streamWriter)
            {
                Formatting = prettyPrint ? Formatting.Indented : Formatting.None
            };

            json.WriteStartObject();
            json.WritePropertyName("property name");
            json.WriteStartObject();
            json.WriteEnd();
            json.WriteEnd();

            json.Flush();

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private static string GetStartEndWithPropertyObjectExpectedString(string key, bool prettyPrint, bool escape = false)
        {
            var ms = new MemoryStream();
            TextWriter streamWriter = new StreamWriter(ms, new UTF8Encoding(false), 1024, true);

            var json = new JsonTextWriter(streamWriter)
            {
                Formatting = prettyPrint ? Formatting.Indented : Formatting.None,
                StringEscapeHandling = StringEscapeHandling.EscapeHtml
            };

            json.WriteStartObject();
            json.WritePropertyName(key, escape);
            json.WriteStartObject();
            json.WriteEnd();
            json.WriteEnd();

            json.Flush();

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private static string GetArrayWithPropertyExpectedString(bool prettyPrint)
        {
            var ms = new MemoryStream();
            TextWriter streamWriter = new StreamWriter(ms, new UTF8Encoding(false), 1024, true);

            var json = new JsonTextWriter(streamWriter)
            {
                Formatting = prettyPrint ? Formatting.Indented : Formatting.None
            };

            json.WriteStartObject();
            json.WritePropertyName("message");
            json.WriteStartArray();
            json.WriteEndArray();
            json.WriteEndObject();
            json.Flush();

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private static string GetBooleanExpectedString(bool prettyPrint, string keyString, bool value, bool escape = false)
        {
            var ms = new MemoryStream();
            TextWriter streamWriter = new StreamWriter(ms, new UTF8Encoding(false), 1024, true);

            var json = new JsonTextWriter(streamWriter)
            {
                Formatting = prettyPrint ? Formatting.Indented : Formatting.None,
                StringEscapeHandling = StringEscapeHandling.EscapeHtml,
            };

            json.WriteStartObject();
            json.WritePropertyName(keyString, escape);
            json.WriteValue(value);

            json.WritePropertyName("temp");
            json.WriteStartArray();
            json.WriteValue(true);
            json.WriteValue(true);
            json.WriteValue(false);
            json.WriteValue(false);
            json.WriteEnd();

            json.WriteEnd();

            json.Flush();

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private static string GetNullExpectedString(bool prettyPrint, string keyString, bool escape = false)
        {
            var ms = new MemoryStream();
            TextWriter streamWriter = new StreamWriter(ms, new UTF8Encoding(false), 1024, true);

            var json = new JsonTextWriter(streamWriter)
            {
                Formatting = prettyPrint ? Formatting.Indented : Formatting.None,
                StringEscapeHandling = StringEscapeHandling.EscapeHtml,
            };

            json.WriteStartObject();
            json.WritePropertyName(keyString, escape);
            json.WriteNull();
            json.WritePropertyName(keyString, escape);
            json.WriteNull();

            json.WritePropertyName("temp");
            json.WriteStartArray();
            json.WriteValue((string)null);
            json.WriteValue((string)null);
            json.WriteEnd();

            json.WriteEnd();

            json.Flush();

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private static string GetPropertyExpectedString<T>(bool prettyPrint, T value)
        {
            var sb = new StringBuilder();
            StringWriter stringWriter = new StringWriter(sb);

            var json = new JsonTextWriter(stringWriter)
            {
                Formatting = prettyPrint ? Formatting.Indented : Formatting.None
            };

            json.WriteStartObject();
            json.WritePropertyName("message");
            json.WriteValue(value);
            json.WriteEnd();

            json.Flush();

            return sb.ToString();
        }

        private static string GetNumbersExpectedString(bool prettyPrint, string keyString, int[] ints, uint[] uints, long[] longs, ulong[] ulongs, float[] floats, double[] doubles, decimal[] decimals, bool escape = false)
        {
            var ms = new MemoryStream();
            TextWriter streamWriter = new StreamWriter(ms, new UTF8Encoding(false), 1024, true);

            var json = new JsonTextWriter(streamWriter)
            {
                Formatting = prettyPrint ? Formatting.Indented : Formatting.None
            };

            json.WriteStartObject();

            for (int i = 0; i < floats.Length; i++)
            {
                json.WritePropertyName(keyString, escape);
                json.WriteValue(floats[i]);
            }
            for (int i = 0; i < ints.Length; i++)
            {
                json.WritePropertyName(keyString, escape);
                json.WriteValue(ints[i]);
            }
            for (int i = 0; i < uints.Length; i++)
            {
                json.WritePropertyName(keyString, escape);
                json.WriteValue(uints[i]);
            }
            for (int i = 0; i < doubles.Length; i++)
            {
                json.WritePropertyName(keyString, escape);
                json.WriteValue(doubles[i]);
            }
            for (int i = 0; i < longs.Length; i++)
            {
                json.WritePropertyName(keyString, escape);
                json.WriteValue(longs[i]);
            }
            for (int i = 0; i < ulongs.Length; i++)
            {
                json.WritePropertyName(keyString, escape);
                json.WriteValue(ulongs[i]);
            }
            for (int i = 0; i < decimals.Length; i++)
            {
                json.WritePropertyName(keyString, escape);
                json.WriteValue(decimals[i]);
            }

            json.WritePropertyName(keyString, escape);
            json.WriteStartArray();
            json.WriteValue(floats[0]);
            json.WriteValue(ints[0]);
            json.WriteValue(uints[0]);
            json.WriteValue(doubles[0]);
            json.WriteValue(longs[0]);
            json.WriteValue(ulongs[0]);
            json.WriteValue(decimals[0]);
            json.WriteEndArray();

            json.WriteEnd();

            json.Flush();

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private static string GetGuidsExpectedString(bool prettyPrint, string keyString, Guid[] guids, bool escape = false)
        {
            var ms = new MemoryStream();
            TextWriter streamWriter = new StreamWriter(ms, new UTF8Encoding(false), 1024, true);

            var json = new JsonTextWriter(streamWriter)
            {
                Formatting = prettyPrint ? Formatting.Indented : Formatting.None,
                StringEscapeHandling = StringEscapeHandling.EscapeHtml
            };

            json.WriteStartObject();

            for (int i = 0; i < guids.Length; i++)
            {
                json.WritePropertyName(keyString, escape);
                json.WriteValue(guids[i]);
            }

            json.WritePropertyName(keyString, escape);
            json.WriteStartArray();
            json.WriteValue(guids[0]);
            json.WriteValue(guids[1]);
            json.WriteEnd();

            json.WriteEnd();

            json.Flush();

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private static string GetNumbersExpectedString<T>(bool prettyPrint, int numberOfElements, T value)
        {
            var sb = new StringBuilder();
            StringWriter stringWriter = new StringWriter(sb);

            var json = new JsonTextWriter(stringWriter)
            {
                Formatting = prettyPrint ? Formatting.Indented : Formatting.None,
            };

            json.WriteStartArray();
            for (int i = 0; i < numberOfElements; i++)
            {
                json.WriteValue(value);
            }
            json.WriteEnd();

            json.Flush();

            return sb.ToString();
        }

        private static string GetDatesExpectedString(bool prettyPrint, string keyString, DateTime[] dates, bool escape = false)
        {
            var ms = new MemoryStream();
            TextWriter streamWriter = new StreamWriter(ms, new UTF8Encoding(false), 1024, true);

            var json = new JsonTextWriter(streamWriter)
            {
                Formatting = prettyPrint ? Formatting.Indented : Formatting.None,
                StringEscapeHandling = StringEscapeHandling.EscapeHtml,
            };

            json.WriteStartObject();

            for (int i = 0; i < dates.Length; i++)
            {
                json.WritePropertyName(keyString, escape);
                json.WriteValue(dates[i]);
            }

            json.WritePropertyName(keyString, escape);
            json.WriteStartArray();
            json.WriteValue(dates[0]);
            json.WriteValue(dates[1]);
            json.WriteEnd();

            json.WriteEnd();

            json.Flush();

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private static string GetDatesExpectedString(bool prettyPrint, string keyString, DateTimeOffset[] dates, bool escape = false)
        {
            var ms = new MemoryStream();
            TextWriter streamWriter = new StreamWriter(ms, new UTF8Encoding(false), 1024, true);

            var json = new JsonTextWriter(streamWriter)
            {
                Formatting = prettyPrint ? Formatting.Indented : Formatting.None,
                StringEscapeHandling = StringEscapeHandling.EscapeHtml,
            };

            json.WriteStartObject();

            for (int i = 0; i < dates.Length; i++)
            {
                json.WritePropertyName(keyString, escape);
                json.WriteValue(dates[i]);
            }

            json.WritePropertyName(keyString, escape);
            json.WriteStartArray();
            json.WriteValue(dates[0]);
            json.WriteValue(dates[1]);
            json.WriteEnd();

            json.WriteEnd();

            json.Flush();

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        private static void AssertContents(string expectedValue, byte[] buffer)
        {
            string value = Encoding.UTF8.GetString(buffer);

            // Temporary hack until we can use the same escape algorithm on both sides and make sure we want uppercase hex.
            Assert.Equal(expectedValue.NormalizeToJsonNetFormat(), value.NormalizeToJsonNetFormat());
        }

        public static IEnumerable<object[]> JsonEncodedTextStrings
        {
            get
            {
                return new List<object[]>
                {
                    new object[] {"", "\"\"" },
                    new object[] { "message", "\"message\"" },
                    new object[] { "mess\"age", "\"mess\\u0022age\"" },
                    new object[] { "mess\\u0022age", "\"mess\\\\u0022age\"" },
                    new object[] { ">>>>>", "\"\\u003E\\u003E\\u003E\\u003E\\u003E\"" },
                    new object[] { "\\u003E\\u003E\\u003E\\u003E\\u003E", "\"\\\\u003E\\\\u003E\\\\u003E\\\\u003E\\\\u003E\"" },
                };
            }
        }
    }
}
