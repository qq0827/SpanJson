namespace SpanJson.Resolvers
{
    public sealed class IncludeNullsSnakeCaseResolver<TSymbol> : ResolverBase<TSymbol, IncludeNullsSnakeCaseResolver<TSymbol>> where TSymbol : struct
    {
        public IncludeNullsSnakeCaseResolver() : base(new SpanJsonOptions
        {
            NullOption = NullOptions.IncludeNulls,
#if DEBUG
            EnumOption = EnumOptions.String,
#else
            EnumOption = EnumOptions.Integer,
#endif
            ExtensionDataNamingPolicy = JsonNamingPolicy.SnakeCase,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCase,
        })
        {
        }
    }
}