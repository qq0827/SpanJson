namespace SpanJson.Resolvers
{
    public sealed class IncludeNullsCamelCaseResolver<TSymbol> : ResolverBase<TSymbol, IncludeNullsCamelCaseResolver<TSymbol>> where TSymbol : struct
    {
        public IncludeNullsCamelCaseResolver() : base(new SpanJsonOptions
        {
            NullOption = NullOptions.IncludeNulls,
#if DEBUG
            EnumOption = EnumOptions.String,
#else
            EnumOption = EnumOptions.Integer,
#endif
            ExtensionDataNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        })
        {
        }
    }
}