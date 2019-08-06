namespace SpanJson.Resolvers
{
    public sealed class ExcludeNullsOriginalCaseResolver<TSymbol> : ResolverBase<TSymbol, ExcludeNullsOriginalCaseResolver<TSymbol>> where TSymbol : struct
    {
        public ExcludeNullsOriginalCaseResolver() : base(new SpanJsonOptions
        {
            NullOption = NullOptions.ExcludeNulls,
#if DEBUG
            EnumOption = EnumOptions.String
#else
            EnumOption = EnumOptions.Integer
#endif
        })
        {
        }
    }
}