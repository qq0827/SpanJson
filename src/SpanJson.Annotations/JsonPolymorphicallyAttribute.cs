using System;

namespace SpanJson
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
    public class JsonPolymorphicallyAttribute : Attribute { }
}
