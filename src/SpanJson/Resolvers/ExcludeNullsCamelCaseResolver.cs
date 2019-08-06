namespace SpanJson.Resolvers
{
    public sealed class ExcludeNullsCamelCaseResolver<TSymbol> : ResolverBase<TSymbol, ExcludeNullsCamelCaseResolver<TSymbol>> where TSymbol : struct
    {
        public ExcludeNullsCamelCaseResolver() : base(new SpanJsonOptions
        {
            NullOption = NullOptions.ExcludeNulls,
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