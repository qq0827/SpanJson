namespace SpanJson.Resolvers
{
    public sealed class IncludeNullsOriginalCaseResolver<TSymbol> : ResolverBase<TSymbol, IncludeNullsOriginalCaseResolver<TSymbol>> where TSymbol : struct
    {
        public IncludeNullsOriginalCaseResolver() : base(new SpanJsonOptions
        {
            NullOption = NullOptions.IncludeNulls,
            EnumOption = EnumOptions.String
        })
        {
        }
    }
}