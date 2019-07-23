using System;
using System.Text;
using System.Runtime.Serialization;
using SpanJson.Resolvers;
using Xunit;

namespace SpanJson.Tests
{
    public class UnicodeEscapeTests2
    {
        public class Person
        {
            /// <summary>
            /// People
            /// </summary>
            [DataMember(Name = "人\r\n才")]
            public string 人 { get; set; }
            /// <summary>
            /// Name
            /// </summary>
            [DataMember(Name = "名\r\n称")]
            public string 名称 { get; set; }
            /// <summary>
            /// Number
            /// </summary>
            [DataMember(Name = "数\r\n字")]
            public Numbers 数 { get; set; }
        }

        public enum Numbers
        {
            一,
            二,
            三,
        }

        [Fact]
        public void SerializeDeserializePersonUtf16()
        {
            var person = new Person
            {
                人 = "自",
                名称 = "男",
                数 = Numbers.三,
            };
            var serialized = JsonSerializer.Generic.Utf16.Serialize(person);
            Assert.Equal("{\"人\\r\\n才\":\"自\",\"名\\r\\n称\":\"男\",\"数\\r\\n字\":\"三\"}", serialized);
            var deserialized = JsonSerializer.Generic.Utf16.Deserialize<Person>(serialized);
            Assert.NotNull(deserialized);
            Assert.Equal(person.人, deserialized.人);
            Assert.Equal(person.名称, deserialized.名称);
            Assert.Equal(person.数, deserialized.数);
        }

        [Fact]
        public void SerializeDeserializePersonUtf8()
        {
            var person = new Person
            {
                人 = "然",
                名称 = "女",
                数 = Numbers.二,
            };
            var serialized = JsonSerializer.Generic.Utf8.Serialize(person);
            Assert.Equal("{\"人\\r\\n才\":\"然\",\"名\\r\\n称\":\"女\",\"数\\r\\n字\":\"二\"}", Encoding.UTF8.GetString(serialized));
            var deserialized = JsonSerializer.Generic.Utf8.Deserialize<Person>(serialized);
            Assert.NotNull(deserialized);
            Assert.Equal(person.人, deserialized.人);
            Assert.Equal(person.名称, deserialized.名称);
            Assert.Equal(person.数, deserialized.数);
        }

        public sealed class NonAsciiEscapeResolver<TSymbol> : ResolverBase<TSymbol, NonAsciiEscapeResolver<TSymbol>> where TSymbol : struct
        {
            public NonAsciiEscapeResolver()
                : base(new SpanJsonOptions { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii })
            {
            }
        }

        [Fact]
        public void SerializeDeserializePersonFullyEncodedUtf16()
        {
            var person = new Person
            {
                人 = "自",
                名称 = "男",
                数 = Numbers.三,
            };
            var serialized = JsonSerializer.Generic.Utf16.Serialize<Person, NonAsciiEscapeResolver<char>>(person);
            Assert.Equal("{\"\\u4eba\\r\\n\\u624d\":\"\\u81ea\",\"\\u540d\\r\\n\\u79f0\":\"\\u7537\",\"\\u6570\\r\\n\\u5b57\":\"三\"}", serialized, ignoreCase: true);
            var deserialized = JsonSerializer.Generic.Utf16.Deserialize<Person, NonAsciiEscapeResolver<char>>(serialized);
            Assert.NotNull(deserialized);
            Assert.Equal(person.人, deserialized.人);
            Assert.Equal(person.名称, deserialized.名称);
            Assert.Equal(person.数, deserialized.数);
        }

        [Fact]
        public void SerializeDeserializePersonFullyEncodedUtf8()
        {
            var person = new Person
            {
                人 = "然",
                名称 = "女",
                数 = Numbers.二,
            };
            var serialized = JsonSerializer.Generic.Utf8.Serialize<Person, NonAsciiEscapeResolver<byte>>(person);
            var utf16Serialized = Encoding.UTF8.GetString(serialized);
            Assert.Equal("{\"\\u4eba\\r\\n\\u624d\":\"\\u7136\",\"\\u540d\\r\\n\\u79f0\":\"\\u5973\",\"\\u6570\\r\\n\\u5b57\":\"二\"}", utf16Serialized, ignoreCase: true);
            var deserialized = JsonSerializer.Generic.Utf8.Deserialize<Person, NonAsciiEscapeResolver<byte>>(serialized);
            Assert.NotNull(deserialized);
            Assert.Equal(person.人, deserialized.人);
            Assert.Equal(person.名称, deserialized.名称);
            Assert.Equal(person.数, deserialized.数);
        }

        [Fact]
        public void HtmlStringEscapeHandling()
        {
            var writer = new JsonWriter<char>(16);

            //var temp = "a/b";
            //var je = JsonEncodedText.Encode(temp, StringEscapeHandling.EscapeNonAscii);
            //var utf16Text = je.EncodedValue;

            string script = @"<script type=""text/javascript"">alert('hi');</script>";

            writer.WriteString(script, StringEscapeHandling.EscapeHtml);

            string json = writer.ToString();

            Assert.Equal(@"""\u003cscript type=\u0022text/javascript\u0022\u003ealert(\u0027hi\u0027);\u003c/script\u003e""", json);

            var reader = new JsonReader<char>(json.AsSpan());

            Assert.Equal(script, reader.ReadString());
        }

        [Fact]
        public void NonAsciiStringEscapeHandling()
        {
            var writer = new JsonWriter<char>(16);

            string unicode = "\u5f20";

            writer.WriteString(unicode, StringEscapeHandling.EscapeNonAscii);

            string json = writer.ToString();

            Assert.Equal(8, json.Length);
            Assert.Equal(@"""\u5f20""", json);

            var reader = new JsonReader<char>(json.AsSpan());

            Assert.Equal(unicode, reader.ReadString());

            writer = new JsonWriter<char>(16);

            writer.WriteString(unicode);

            json = writer.ToString();

            Assert.Equal(3, json.Length);
            Assert.Equal("\"\u5f20\"", json);
        }

        [Fact]
        public void ToStringStringEscapeHandling()
        {
            string v = "<b>hi " + '\u20AC' + "</b>";

            var json = JsonEncodedText.Encode(v);
            Assert.Equal(@"<b>hi " + '\u20AC' + @"</b>", json.ToString());
            Assert.Equal(@"<b>hi " + '\u20AC' + @"</b>", Internal.EscapingHelper.EscapeString(v));

            json = JsonEncodedText.Encode(v, StringEscapeHandling.EscapeHtml);
            Assert.Equal(@"\u003cb\u003ehi " + '\u20AC' + @"\u003c/b\u003e", json.ToString());
            Assert.Equal(@"\u003cb\u003ehi " + '\u20AC' + @"\u003c/b\u003e", Internal.EscapingHelper.EscapeString(v, StringEscapeHandling.EscapeHtml));

            json = JsonEncodedText.Encode(v, StringEscapeHandling.EscapeNonAscii);
            Assert.Equal(@"\u003cb\u003ehi \u20ac\u003c/b\u003e", json.ToString(), ignoreCase: true);
            Assert.Equal(@"\u003cb\u003ehi \u20ac\u003c/b\u003e", Internal.EscapingHelper.EscapeString(v, StringEscapeHandling.EscapeNonAscii), ignoreCase: true);
        }

        [Fact]
        public void EscapeJavaScriptString_UnicodeLinefeeds()
        {
            var text0085 = "before" + '\u0085' + "after";
            var text2028 = "before" + '\u2028' + "after";
            var text2029 = "before" + '\u2029' + "after";

            var escapedString = Internal.EscapingHelper.EscapeString(text0085, StringEscapeHandling.Default);
            Assert.Equal(@"before\u0085after", escapedString);

            escapedString = Internal.EscapingHelper.EscapeString(text2028, StringEscapeHandling.Default);
            Assert.Equal(@"before\u2028after", escapedString);

            escapedString = Internal.EscapingHelper.EscapeString(text2029, StringEscapeHandling.Default);
            Assert.Equal(@"before\u2029after", escapedString);

            var result = JsonEncodedText.Encode(text0085, StringEscapeHandling.EscapeNonAscii);
            Assert.Equal(@"before\u0085after", result.ToString());

            result = JsonEncodedText.Encode(text2028, StringEscapeHandling.EscapeNonAscii);
            Assert.Equal(@"before\u2028after", result.ToString());

            result = JsonEncodedText.Encode(text2029, StringEscapeHandling.EscapeNonAscii);
            Assert.Equal(@"before\u2029after", result.ToString());

            result = JsonEncodedText.Encode(text0085, StringEscapeHandling.Default);
            Assert.Equal(@"before\u0085after", result.ToString());

            result = JsonEncodedText.Encode(text2028, StringEscapeHandling.Default);
            Assert.Equal(@"before\u2028after", result.ToString());

            result = JsonEncodedText.Encode(text2029, StringEscapeHandling.Default);
            Assert.Equal(@"before\u2029after", result.ToString());
        }

        [Fact]
        public void EscapeJavaScriptString()
        {
            var escapedString = Internal.EscapingHelper.EscapeString("How now brown cow?",  StringEscapeHandling.Default);
            Assert.Equal(@"How now brown cow?", escapedString);

            escapedString = Internal.EscapingHelper.EscapeString("How now 'brown' cow?",  StringEscapeHandling.Default);
            Assert.Equal(@"How now 'brown' cow?", escapedString);

            escapedString = Internal.EscapingHelper.EscapeString("How now <brown> cow?",  StringEscapeHandling.Default);
            Assert.Equal(@"How now <brown> cow?", escapedString);

            escapedString = Internal.EscapingHelper.EscapeString("How \r\nnow brown cow?",  StringEscapeHandling.Default);
            Assert.Equal(@"How \r\nnow brown cow?", escapedString);

            escapedString = Internal.EscapingHelper.EscapeString("\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007",  StringEscapeHandling.Default);
            Assert.Equal(@"\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007", escapedString);

            escapedString =
                Internal.EscapingHelper.EscapeString("\b\t\n\u000b\f\r\u000e\u000f\u0010\u0011\u0012\u0013", StringEscapeHandling.Default);
            Assert.Equal(@"\b\t\n\u000b\f\r\u000e\u000f\u0010\u0011\u0012\u0013", escapedString);

            escapedString =
                Internal.EscapingHelper.EscapeString(
                    "\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f ", StringEscapeHandling.Default);
            Assert.Equal(@"\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f ", escapedString);

            escapedString =
                Internal.EscapingHelper.EscapeString(
                    "!\"#$%&\u0027()*+,-./0123456789:;\u003c=\u003e?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]", StringEscapeHandling.Default);
            Assert.Equal(@"!\""#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]", escapedString);

            escapedString = Internal.EscapingHelper.EscapeString("^_`abcdefghijklmnopqrstuvwxyz{|}~", StringEscapeHandling.Default);
            Assert.Equal(@"^_`abcdefghijklmnopqrstuvwxyz{|}~", escapedString);

            string data =
                "\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\b\t\n\u000b\f\r\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f !\"#$%&\u0027()*+,-./0123456789:;\u003c=\u003e?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
            string expected =
                @"\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\b\t\n\u000b\f\r\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f !\""#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

            escapedString = Internal.EscapingHelper.EscapeString(data, StringEscapeHandling.Default);
            Assert.Equal(expected, escapedString);

            escapedString = Internal.EscapingHelper.EscapeString("Fred's cat.", StringEscapeHandling.Default);
            Assert.Equal(@"Fred's cat.", escapedString);

            escapedString = Internal.EscapingHelper.EscapeString(@"""How are you gentlemen?"" said Cats.", StringEscapeHandling.Default);
            Assert.Equal(@"\""How are you gentlemen?\"" said Cats.", escapedString);

            escapedString = Internal.EscapingHelper.EscapeString(@"""How are' you gentlemen?"" said Cats.", StringEscapeHandling.Default);
            Assert.Equal(@"\""How are' you gentlemen?\"" said Cats.", escapedString);

            escapedString = Internal.EscapingHelper.EscapeString(@"Fred's ""cat"".", StringEscapeHandling.Default);
            Assert.Equal(@"Fred's \""cat\"".", escapedString);

            escapedString = Internal.EscapingHelper.EscapeString("\u001farray\u003caddress", StringEscapeHandling.Default);
            Assert.Equal(@"\u001farray<address", escapedString);

            var result = JsonEncodedText.Encode("How now brown cow?", StringEscapeHandling.Default);
            Assert.Equal(@"How now brown cow?", result.ToString());

            result = JsonEncodedText.Encode("How now 'brown' cow?", StringEscapeHandling.Default);
            Assert.Equal(@"How now 'brown' cow?", result.ToString());

            result = JsonEncodedText.Encode("How now <brown> cow?", StringEscapeHandling.Default);
            Assert.Equal(@"How now <brown> cow?", result.ToString());

            result = JsonEncodedText.Encode("How \r\nnow brown cow?", StringEscapeHandling.Default);
            Assert.Equal(@"How \r\nnow brown cow?", result.ToString());

            result = JsonEncodedText.Encode("\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007", StringEscapeHandling.Default);
            Assert.Equal(@"\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007", result.ToString());

            result =
                JsonEncodedText.Encode("\b\t\n\u000b\f\r\u000e\u000f\u0010\u0011\u0012\u0013", StringEscapeHandling.Default);
            Assert.Equal(@"\b\t\n\u000b\f\r\u000e\u000f\u0010\u0011\u0012\u0013", result.ToString());

            result =
                JsonEncodedText.Encode(
                    "\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f ", StringEscapeHandling.Default);
            Assert.Equal(@"\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f ", result.ToString());

            result =
                JsonEncodedText.Encode(
                    "!\"#$%&\u0027()*+,-./0123456789:;\u003c=\u003e?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]", StringEscapeHandling.Default);
            Assert.Equal(@"!\""#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]", result.ToString());

            result = JsonEncodedText.Encode("^_`abcdefghijklmnopqrstuvwxyz{|}~", StringEscapeHandling.Default);
            Assert.Equal(@"^_`abcdefghijklmnopqrstuvwxyz{|}~", result.ToString());

            data =
                "\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\b\t\n\u000b\f\r\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f !\"#$%&\u0027()*+,-./0123456789:;\u003c=\u003e?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
            expected =
                @"\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\b\t\n\u000b\f\r\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f !\""#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";

            result = JsonEncodedText.Encode(data, StringEscapeHandling.Default);
            Assert.Equal(expected, result.ToString());

            result = JsonEncodedText.Encode("Fred's cat.", StringEscapeHandling.Default);
            Assert.Equal(@"Fred's cat.", result.ToString());

            result = JsonEncodedText.Encode(@"""How are you gentlemen?"" said Cats.", StringEscapeHandling.Default);
            Assert.Equal(@"\""How are you gentlemen?\"" said Cats.", result.ToString());

            result = JsonEncodedText.Encode(@"""How are' you gentlemen?"" said Cats.", StringEscapeHandling.Default);
            Assert.Equal(@"\""How are' you gentlemen?\"" said Cats.", result.ToString());

            result = JsonEncodedText.Encode(@"Fred's ""cat"".", StringEscapeHandling.Default);
            Assert.Equal(@"Fred's \""cat\"".", result.ToString());

            result = JsonEncodedText.Encode("\u001farray\u003caddress", StringEscapeHandling.Default);
            Assert.Equal(@"\u001farray<address", result.ToString());
        }
    }
}
