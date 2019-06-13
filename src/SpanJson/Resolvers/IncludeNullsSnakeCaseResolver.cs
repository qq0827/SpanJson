namespace SpanJson.Resolvers
{
    public sealed class IncludeNullsSnakeCaseResolver<TSymbol> : ResolverBase<TSymbol, IncludeNullsSnakeCaseResolver<TSymbol>> where TSymbol : struct
    {
        public IncludeNullsSnakeCaseResolver() : base(new SpanJsonOptions
        {
            NullOption = NullOptions.IncludeNulls,
            NamingConvention = NamingConventions.SnakeCase,
            EnumOption = EnumOptions.String
        })
        {
        }
    }
}