namespace SpanJson
{
    using System.Runtime.Serialization;
#if !NET451
    using System.Text.Encodings.Web;
#endif

    public class SpanJsonOptions
    {
        public NullOptions NullOption { get; set; }
        public EnumOptions EnumOption { get; set; }

        public StringEscapeHandling StringEscapeHandling { get; set; }

#if !NET451
        /// <summary>The encoder to use when escaping strings, or <see langword="null" /> to use the default encoder.</summary>
        public JavaScriptEncoder Encoder { get; set; }
#endif

        /// <summary>Not yet supported</summary>
        public JsonNamingPolicy DictionaryKeyPolicy { get; set; }
        public JsonNamingPolicy ExtensionDataNamingPolicy { get; set; }
        public JsonNamingPolicy PropertyNamingPolicy { get; set; }
    }
}