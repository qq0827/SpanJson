using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
#if NETSTANDARD2_0 || NET471 || NET451
using SpanJson.Internal;
#endif

namespace SpanJson.Formatters
{
    public abstract class BaseFormatter
    {
        protected static MethodInfo FindPublicInstanceMethod(Type type, string name, params Type[] args)
        {
            return (null == args || 0u >= (uint)args.Length) ? type.GetMethod(name) : type.GetMethod(name, args);
        }

        protected static MethodInfo FindGenericMethod(Type type, string name, BindingFlags bindingFlags, Type genericType, Type parameterType)
        {
            var myMethod = type
                .GetMethods(bindingFlags)
                .Single(m => m.Name == name && m.IsGenericMethodDefinition && m.GetParameters().Single().ParameterType.GetGenericTypeDefinition() == parameterType);
            return myMethod.MakeGenericMethod(genericType);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void SerializeRuntimeDecisionInternal<T, TSymbol, TResolver>(ref JsonWriter<TSymbol> writer,
            T value, IJsonFormatter<T, TSymbol> formatter)
            where TResolver : IJsonFormatterResolver<TSymbol, TResolver>, new() where TSymbol : struct
        {
            // The first check is to get around the runtime check for primitive types and structs without references, i.e most of the blc types from the bclformatter.tt
            // Then we specifically check for string as it is very common
            // A null value can be serialized by both (doesn't matter, but we need to handle null for the runtime type check)
            // Checking for things like IsValueType and/or IsSealed is actually several times more expensive than the type comparison
#if NETSTANDARD2_0 || NET471 || NET451
            if (!JsonHelpers.IsReferenceOrContainsReferences<T>() || typeof(T) == typeof(string) || value == null || value.GetType() == typeof(T))
#else
            if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>() || typeof(T) == typeof(string) || value == null || value.GetType() == typeof(T))
#endif
            {
                formatter.Serialize(ref writer, value);
            }
            else
            {
                RuntimeFormatter<TSymbol, TResolver>.Default.Serialize(ref writer, value);
            }
        }
    }
}