namespace SpanJson.Resolvers
{
    public sealed class ExcludeNullsSnakeCaseResolver<TSymbol> : ResolverBase<TSymbol, ExcludeNullsSnakeCaseResolver<TSymbol>> where TSymbol : struct
    {
        public ExcludeNullsSnakeCaseResolver() : base(new SpanJsonOptions
        {
            NullOption = NullOptions.ExcludeNulls,
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