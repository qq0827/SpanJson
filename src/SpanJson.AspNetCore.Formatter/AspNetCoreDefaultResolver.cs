using SpanJson.Resolvers;

namespace SpanJson.AspNetCore.Formatter
{
    public class AspNetCoreDefaultResolver<TSymbol> : ResolverBase<TSymbol, AspNetCoreDefaultResolver<TSymbol>> where TSymbol : struct
    {
        public AspNetCoreDefaultResolver() : base(new SpanJsonOptions
        {
            NullOption = NullOptions.IncludeNulls,
            EnumOption = EnumOptions.Integer,
            ExtensionDataNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        })
        {
        }
    }
}