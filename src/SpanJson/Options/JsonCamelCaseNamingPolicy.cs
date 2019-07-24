namespace SpanJson
{
    using SpanJson.Internal;

    /// <summary>The default naming strategy. Property names are unchanged.</summary>
    public class JsonCamelCaseNamingPolicy : JsonNamingPolicy
    {
        /// <inheritdoc />
        public override string ConvertName(string name) => StringMutator.ToCamelCaseWithCache(name);
    }
}
