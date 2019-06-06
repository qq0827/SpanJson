using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SpanJson.Internal
{
    internal static partial class JsonHelpers
    {
#if NETSTANDARD2_0 || NET471 || NET451

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source) => source.ToHashSet(comparer: null);

        public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer)
        {
            if (source == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.source);
            }

            // Don't pre-allocate based on knowledge of size, as potentially many elements will be dropped.
            return new HashSet<TSource>(source, comparer);
        }

        internal sealed class ReferenceContainer<T>
        {
            public static readonly bool IsReferenceOrContainsReferences;

            static ReferenceContainer()
            {
                IsReferenceOrContainsReferences = IsReferenceOrContainsReferencesImpl(typeof(T));
            }
        }

        private static readonly ConcurrentDictionary<Type, bool> s_referenceTypeCache = new ConcurrentDictionary<Type, bool>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsReferenceOrContainsReferences<T>() => ReferenceContainer<T>.IsReferenceOrContainsReferences;

        // https://stackoverflow.com/questions/53968920/how-do-i-check-if-a-type-fits-the-unmanaged-constraint-in-c
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool IsReferenceOrContainsReferencesImpl(Type type)
        {
            if (s_referenceTypeCache.TryGetValue(type, out var result)) { return result; }

            if (!type.IsValueType)
            {
                result = true;
            }
            else if (type.IsPrimitive || type.IsPointer || type.IsEnum)
            {
                result = false;
            }
            else
            {
                // otherwise check recursively
                result = type
                    .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .All(f => IsReferenceOrContainsReferencesImpl(f.FieldType));
            }

            s_referenceTypeCache.TryAdd(type, result);
            return result;
        }

#endif
    }
}
