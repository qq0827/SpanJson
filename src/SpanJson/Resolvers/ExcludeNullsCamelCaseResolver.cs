namespace SpanJson.Resolvers
{
    public sealed class ExcludeNullsCamelCaseResolver<TSymbol> : ResolverBase<TSymbol, ExcludeNullsCamelCaseResolver<TSymbol>> where TSymbol : struct
    {
        public ExcludeNullsCamelCaseResolver() : base(new SpanJsonOptions
        {
            NullOption = NullOptions.ExcludeNulls,
            EnumOption = EnumOptions.String,
            ExtensionDataNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        })
        {
        }
    }
}