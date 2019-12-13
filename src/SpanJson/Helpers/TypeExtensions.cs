using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SpanJson.Helpers
{
    public static class TypeExtensions
    {
        public static bool TryGetNullableUnderlyingType(this Type type, out Type underlingType)
        {
            if (type.IsValueType)
            {
                underlingType = Nullable.GetUnderlyingType(type);
                return underlingType is object;
            }

            underlingType = default;
            return false;
        }

        public static bool IsNullable(this Type type)
        {
            return type.IsClass || Nullable.GetUnderlyingType(type) is object;
        }

        public static bool TryGetTypeOfGenericInterface(this Type type, Type interfaceType, out Type[] argumentTypes)
        {
            if (!interfaceType.IsInterface)
            {
                ThrowHelper.ThrowArgumentException_IsNotAnInterface(interfaceType);
            }

            if (type.IsGenericType && interfaceType.IsAssignableFrom(type.GetGenericTypeDefinition()))
            {
                argumentTypes = type.GetGenericArguments();
                return true;
            }

            var interfaces = type.GetInterfaces();
            var match = interfaces.FirstOrDefault(a => a.IsGenericType && interfaceType.IsAssignableFrom(a.GetGenericTypeDefinition()));
            if (match is object)
            {
                argumentTypes = match.GetGenericArguments();
                return true;
            }

            argumentTypes = default;
            return false;
        }

        // https://stackoverflow.com/questions/358835/getproperties-to-return-all-properties-for-an-interface-inheritance-hierarchy
        public static PropertyInfo[] GetPublicProperties(this Type type)
        {
            if (type.IsInterface)
            {
                var propertyInfos = new List<PropertyInfo>();

                var considered = new List<Type>();
                var queue = new Queue<Type>();
                considered.Add(type);
                queue.Enqueue(type);
                while (queue.Count > 0)
                {
                    var subType = queue.Dequeue();
                    foreach (var subInterface in subType.GetInterfaces())
                    {
                        if (considered.Contains(subInterface)) continue;

                        considered.Add(subInterface);
                        queue.Enqueue(subInterface);
                    }

                    var typeProperties = subType.GetProperties(
                        BindingFlags.FlattenHierarchy
                        | BindingFlags.Public
                        | BindingFlags.Instance);

                    var newPropertyInfos = typeProperties
                        .Where(x => !propertyInfos.Contains(x));

                    propertyInfos.InsertRange(0, newPropertyInfos);
                }

                return propertyInfos.ToArray();
            }

            return type.GetProperties(BindingFlags.FlattenHierarchy
                                      | BindingFlags.Public | BindingFlags.Instance);
        }

        public static IEnumerable<MemberInfo> SerializableMembers(this Type type)
        {
            var publicMembers = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(a => !a.IsLiteral && !a.FieldType.IsByRefLike()).Cast<MemberInfo>().Concat(
                    type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(a => !a.PropertyType.IsByRefLike() && 0u >= (uint)a.GetIndexParameters().Length));
            return publicMembers;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsByRefLike(this Type type)
        {
#if NETSTANDARD2_0 || NET471 || NET451
            if (!type.IsValueType)
            {
                return false;
            }

            // IsByRefLike flag on type is not available in netstandard2.0
            Attribute[] attributes = type.GetCustomAttributes(false).Cast<Attribute>().ToArray();
            for (int i = 0; i < attributes.Length; i++)
            {
                if (string.Equals(attributes[i].GetType().FullName, "System.Runtime.CompilerServices.IsByRefLikeAttribute"))
                {
                    return true;
                }
            }

            return false;
#else
            return type.IsByRefLike;
#endif
        }

        public static bool IsInteger(this Type type)
        {
            if (type.IsEnum)
            {
                return false;
            }
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return true;
                default:
                    return false;
            }
        }
    }
}