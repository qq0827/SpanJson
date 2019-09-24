namespace SpanJson.Resolvers
{
    public sealed class ExcludeNullsSnakeCaseResolver<TSymbol> : ResolverBase<TSymbol, ExcludeNullsSnakeCaseResolver<TSymbol>> where TSymbol : struct
    {
        public ExcludeNullsSnakeCaseResolver() : base(new SpanJsonOptions
        {
            NullOption = NullOptions.ExcludeNulls,
            EnumOption = EnumOptions.String,
            ExtensionDataNamingPolicy = JsonNamingPolicy.SnakeCase,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCase,
        })
        {
        }
    }
}