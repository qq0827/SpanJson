// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using SpanJson.Utilities;

namespace SpanJson.Serialization
{
    [DebuggerDisplay("ClassType.{ClassType}, {Type.Name}")]
    internal sealed partial class JsonClassInfo
    {
        // Return the element type of the IEnumerable or return null if not an IEnumerable.
        public static Type GetElementType(Type propertyType, Type parentType, MemberInfo memberInfo/*, JsonSerializerOptions options*/)
        {
            // We want to handle as the implemented collection type, if applicable.
            Type implementedType = GetImplementedCollectionType(propertyType);

            if (!typeof(IEnumerable).IsAssignableFrom(implementedType))
            {
                return null;
            }

            // Check for Array.
            Type elementType = implementedType.GetElementType();
            if (elementType is object)
            {
                return elementType;
            }

            // Check for Dictionary<TKey, TValue> or IEnumerable<T>
            if (implementedType.IsGenericType)
            {
                Type[] args = implementedType.GetGenericArguments();
                ClassType classType = GetClassType(implementedType/*, options*/);

                if ((classType == ClassType.Dictionary || classType == ClassType.IDictionaryConstructible) &&
                    args.Length >= 2) // It is >= 2 in case there is a IDictionary<TKey, TValue, TSomeExtension>.
                                      //&& args[0].UnderlyingSystemType == typeof(string))
                {
                    return args[1];
                }

                if (classType == ClassType.Enumerable && args.Length >= 1) // It is >= 1 in case there is an IEnumerable<T, TSomeExtension>.
                {
                    return args[0];
                }
            }

            if (implementedType.IsAssignableFrom(typeof(IList)) ||
                implementedType.IsAssignableFrom(typeof(IDictionary)) ||
                IsDeserializedByConstructingWithIList(implementedType) ||
                IsDeserializedByConstructingWithIDictionary(implementedType))
            {
                return typeof(object);
            }

            throw SysJsonThrowHelper.GetNotSupportedException_SerializationNotSupportedCollection(propertyType, parentType, memberInfo);
        }

        public static ClassType GetClassType(Type type/*, JsonSerializerOptions options*/)
        {
            Debug.Assert(type is object);

            // We want to handle as the implemented collection type, if applicable.
            Type implementedType = GetImplementedCollectionType(type);

            if (implementedType.IsGenericType && implementedType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                implementedType = Nullable.GetUnderlyingType(implementedType);
            }

            if (implementedType == typeof(object))
            {
                return ClassType.Unknown;
            }

            if (ConvertUtils.GetTypeCode(implementedType, out _) != PrimitiveTypeCode.Object) // 这儿只检测基元类型
            {
                return ClassType.Value;
            }

            if (/*DefaultImmutableDictionaryConverter.*/IsImmutableDictionary(implementedType) ||
                IsDeserializedByConstructingWithIDictionary(implementedType))
            {
                return ClassType.IDictionaryConstructible;
            }

            if (typeof(IDictionary).IsAssignableFrom(implementedType) || IsDictionaryClassType(implementedType))
            {
                // Special case for immutable dictionaries
                if (type != implementedType && !IsNativelySupportedCollection(type))
                {
                    return ClassType.IDictionaryConstructible;
                }

                return ClassType.Dictionary;
            }

            if (typeof(IEnumerable).IsAssignableFrom(implementedType))
            {
                return ClassType.Enumerable;
            }

            return ClassType.Object;
        }

        public static bool IsDictionaryClassType(Type type)
        {
            return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IDictionary<,>) ||
                type.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)));
        }

        public static bool IsImmutableDictionary(Type type)
        {
            if (!type.IsGenericType)
            {
                return false;
            }

            switch (type.GetGenericTypeDefinition().FullName)
            {
                case ImmutableDictionaryGenericTypeName:
                case ImmutableDictionaryGenericInterfaceTypeName:
                case ImmutableSortedDictionaryGenericTypeName:
                    return true;
                default:
                    return false;
            }
        }
    }
}
