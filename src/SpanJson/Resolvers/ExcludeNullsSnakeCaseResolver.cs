namespace SpanJson.Resolvers
{
    public sealed class ExcludeNullsSnakeCaseResolver<TSymbol> : ResolverBase<TSymbol, ExcludeNullsSnakeCaseResolver<TSymbol>> where TSymbol : struct
    {
        public ExcludeNullsSnakeCaseResolver() : base(new SpanJsonOptions
        {
            NullOption = NullOptions.ExcludeNulls,
            NamingConvention = NamingConventions.SnakeCase,
            EnumOption = EnumOptions.String
        })
        {
        }
    }
}