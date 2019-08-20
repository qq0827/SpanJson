using System;

namespace SpanJson
{
    /// <summary>Annotate constructors with this attribute to use the constructor instead of members directly for deserialization.</summary>
    [AttributeUsage(AttributeTargets.Constructor)]
    public class JsonConstructorAttribute : Attribute
    {
        /// <summary>The names of the constructor's parameters need to match the member names of the type (case-insensitive), the order is not relevant.</summary>
        public JsonConstructorAttribute() { }

        /// <summary>Override the constructor's parameter names to match the member names of the type, the order is relevant.</summary>
        public JsonConstructorAttribute(params string[] parameterNames)
        {
            ParameterNames = parameterNames;
        }

        /// <summary>Override the constructor's parameter names to match the member names of the type, the order is relevant.</summary>
        public JsonConstructorAttribute(Type[] parameterTypes)
        {
            ParameterTypes = parameterTypes;
        }

        /// <summary>Override the constructor's parameter names to match the member names of the type, the order is relevant.</summary>
        public JsonConstructorAttribute(string[] parameterNames, Type[] parameterTypes)
        {
            ParameterNames = parameterNames;
            ParameterTypes = parameterTypes;
        }

        public string[] ParameterNames { get; }
        public Type[] ParameterTypes { get; }
    }
}