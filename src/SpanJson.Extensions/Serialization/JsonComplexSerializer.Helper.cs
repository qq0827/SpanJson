using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using SpanJson.Helpers;

namespace SpanJson.Serialization
{
    partial class JsonComplexSerializer<TResolver, TUtf8Resolver>
    {
        sealed class PolymorphicallyContainer<T>
        {
            public static readonly bool IsPolymorphically;

            static PolymorphicallyContainer()
            {
                IsPolymorphically = IsPolymorphically(typeof(T));
            }
        }

        private static readonly ConcurrentDictionary<Type, bool> s_polymorphicallyTypeCache = new ConcurrentDictionary<Type, bool>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPolymorphically<T>() => PolymorphicallyContainer<T>.IsPolymorphically;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPolymorphically(Type type)
        {
            if (s_polymorphicallyTypeCache.TryGetValue(type, out var result)) { return result; }

            return IsPolymorphicallyImpl(type);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool IsPolymorphicallyImpl(Type type)
        {
            var result = false;

            Type implementedType = null;
            var classType = JsonClassInfo.GetClassType(type);
            switch (classType)
            {
                case ClassType.Enumerable:
                case ClassType.Dictionary:
                case ClassType.IDictionaryConstructible:
                    implementedType = JsonClassInfo.GetElementType(type);
                    break;
            }
            if (null == implementedType) { implementedType = type; }

            if (implementedType.IsGenericType && implementedType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                implementedType = Nullable.GetUnderlyingType(implementedType);
            }

            if (implementedType.HasAttribute<JsonPolymorphicallyAttribute>(false))
            {
                result = true;
            }
            else
            {
                result = implementedType
                    .SerializableMembers()
                    .Any(f => f.HasAttribute<JsonPolymorphicallyAttribute>());
            }

            if (implementedType == type)
            {
                s_polymorphicallyTypeCache.TryAdd(type, result);
            }
            else
            {
                s_polymorphicallyTypeCache.TryAdd(type, result);
                s_polymorphicallyTypeCache.TryAdd(implementedType, result);
            }
            return result;
        }
    }
}
