using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using CuteAnt;
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

            return IsPolymorphicallyImpl(type, parentType: null, memberInfo: null);
        }

        private static bool IsPolymorphicallyInternal(Type type, Type parentType, MemberInfo memberInfo)
        {
            if (s_polymorphicallyTypeCache.TryGetValue(type, out var result)) { return result; }

            return IsPolymorphicallyImpl(type, parentType: null, memberInfo: null);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool IsPolymorphicallyImpl(Type type, Type parentType, MemberInfo memberInfo)
        {
            static Type GetUnderlyingTypeLocal(Type t, Type pt, MemberInfo mi)
            {
                Type underlyingType = null;
                var classType = JsonClassInfo.GetClassType(t);
                switch (classType)
                {
                    case ClassType.Enumerable:
                    case ClassType.Dictionary:
                    case ClassType.IDictionaryConstructible:
                        underlyingType = JsonClassInfo.GetElementType(t, pt, mi);
                        break;
                }
                if (underlyingType is null) { underlyingType = t; }

                if (underlyingType.IsGenericType && underlyingType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    underlyingType = Nullable.GetUnderlyingType(underlyingType);
                }

                return underlyingType;
            }

            var result = false;

            Type implementedType = GetUnderlyingTypeLocal(type, parentType, memberInfo);

            if (implementedType == TypeConstants.ObjectType)
            {
                result = true;
            }
            else if (implementedType.HasAttribute<JsonPolymorphicallyAttribute>(false))
            {
                result = true;
            }
            else
            {
                foreach (var item in implementedType.SerializableMembers())
                {
                    if (item is FieldInfo fi)
                    {
                        if (fi.HasAttribute<JsonPolymorphicallyAttribute>()) { result = true; break; }
                        if (IsPolymorphicallyInternal(fi.FieldType, implementedType, fi)) { result = true; break; }
                    }

                    if (item is PropertyInfo pi)
                    {
                        if (pi.HasAttribute<JsonPolymorphicallyAttribute>()) { result = true; break; }
                        if (IsPolymorphicallyInternal(pi.PropertyType, implementedType, pi)) { result = true; break; }
                    }
                }
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
