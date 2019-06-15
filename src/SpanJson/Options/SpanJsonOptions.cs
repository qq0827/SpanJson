namespace SpanJson
{
    public class SpanJsonOptions
    {
        public NamingConventions NamingConvention { get; set; }
        public NullOptions NullOption { get; set; }
        public EnumOptions EnumOption { get; set; }

        public StringEscapeHandling StringEscapeHandling { get; set; }
    }
}