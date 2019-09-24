using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using CuteAnt;
using SpanJson.Helpers;
using SpanJson.Resolvers;

namespace SpanJson.Serialization
{
    partial class JsonComplexSerializer<TUtf16Resolver, TUtf8Resolver>
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

            return IsPolymorphicallyImpl(type, parentType: null, memberInfo: null, parentTypes: new HashSet<Type>());
        }

        private static bool IsPolymorphicallyInternal(Type type, Type parentType, MemberInfo memberInfo, HashSet<Type> parentTypes)
        {
            if (s_polymorphicallyTypeCache.TryGetValue(type, out var result)) { return result; }

            return IsPolymorphicallyImpl(type, parentType, memberInfo, parentTypes);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool IsPolymorphicallyImpl(Type type, Type parentType, MemberInfo memberInfo, HashSet<Type> parentTypes)
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
            // 只判断是否已经注册自定义 Formatter，IncludeNullsOriginalCaseResolver 作为默认的 Resolver，会确保应用程序所用的 custom formatter
            else if (StandardResolvers.GetResolver<char, IncludeNullsOriginalCaseResolver<char>>().IsSupportedType(implementedType))
            {
                result = false;
            }
            else
            {
                foreach (var item in implementedType.SerializableMembers())
                {
                    if (item is FieldInfo fi)
                    {
                        if (fi.HasAttribute<JsonPolymorphicallyAttribute>()) { result = true; break; }

                        var fieldType = GetUnderlyingTypeLocal(fi.FieldType, type, fi);

                        if (StandardResolvers.GetResolver<char, IncludeNullsOriginalCaseResolver<char>>().IsSupportedType(fieldType)) { continue; }

                        if (fieldType.IsAbstract || fieldType.IsInterface) { result = true; break; }

                        if (fi.FieldType == type) { continue; }
                        if (fieldType == type) { continue; }
                        if (fieldType == implementedType) { continue; }
                        if (parentTypes.Contains(fieldType)) { continue; }
                        parentTypes.Add(type);
                        parentTypes.Add(implementedType);

                        if (IsPolymorphicallyInternal(fieldType, type, fi, parentTypes)) { result = true; break; }
                    }

                    if (item is PropertyInfo pi)
                    {
                        if (pi.HasAttribute<JsonPolymorphicallyAttribute>()) { result = true; break; }

                        var propertyType = GetUnderlyingTypeLocal(pi.PropertyType, type, pi);

                        if (StandardResolvers.GetResolver<char, IncludeNullsOriginalCaseResolver<char>>().IsSupportedType(propertyType)) { continue; }

                        if (propertyType.IsAbstract || propertyType.IsInterface) { result = true; break; }

                        if (pi.PropertyType == type) { continue; }
                        if (propertyType == type) { continue; }
                        if (propertyType == implementedType) { continue; }
                        if (parentTypes.Contains(propertyType)) { continue; }
                        parentTypes.Add(type);
                        parentTypes.Add(implementedType);

                        if (IsPolymorphicallyInternal(propertyType, type, pi, parentTypes)) { result = true; break; }
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
