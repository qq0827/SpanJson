namespace SpanJson
{
    using System.Runtime.Serialization;

    public class SpanJsonOptions
    {
        public NullOptions NullOption { get; set; }
        public EnumOptions EnumOption { get; set; }

        public StringEscapeHandling StringEscapeHandling { get; set; }

        public JsonNamingPolicy DictionaryKeyPolicy { get; set; }
        public JsonNamingPolicy ExtensionDataNamingPolicy { get; set; }
        public JsonNamingPolicy PropertyNamingPolicy { get; set; }
    }
}