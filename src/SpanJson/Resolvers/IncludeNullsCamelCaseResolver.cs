namespace SpanJson.Resolvers
{
    public sealed class IncludeNullsCamelCaseResolver<TSymbol> : ResolverBase<TSymbol, IncludeNullsCamelCaseResolver<TSymbol>> where TSymbol : struct
    {
        public IncludeNullsCamelCaseResolver() : base(new SpanJsonOptions
        {
            NullOption = NullOptions.IncludeNulls,
            EnumOption = EnumOptions.String,
            ExtensionDataNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        })
        {
        }
    }
}