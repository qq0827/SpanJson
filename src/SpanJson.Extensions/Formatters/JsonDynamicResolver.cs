using System;
using System.Collections.Generic;
using System.Reflection;
using SpanJson.Document;
using SpanJson.Dynamic;
using SpanJson.Linq;

namespace SpanJson.Formatters
{
    public sealed class JsonDynamicResolver : ICustomJsonFormatterResolver
    {
        // Resolver should be singleton.
        public static readonly JsonDynamicResolver Instance = new JsonDynamicResolver();

        JsonDynamicResolver() { }

        public ICustomJsonFormatter GetFormatter(Type type)
        {
            if (formatterMap.TryGetValue(type, out var formatter))
            {
                return formatter;
            }

            // JObject
            if (typeof(JObject).IsAssignableFrom(type))
            {
                var formatterType = typeof(JObjectFormatter<>).MakeGenericType(type);
                return GetDefaultOrCreate(formatterType);
            }

            // JArray
            if (typeof(JArray).IsAssignableFrom(type))
            {
                var formatterType = typeof(JArrayFormatter<>).MakeGenericType(type);
                return GetDefaultOrCreate(formatterType);
            }

            // JProperty
            if (typeof(JProperty).IsAssignableFrom(type))
            {
                var formatterType = typeof(JPropertyFormatter<>).MakeGenericType(type);
                return GetDefaultOrCreate(formatterType);
            }

            // JRaw
            if (typeof(JRaw).IsAssignableFrom(type))
            {
                var formatterType = typeof(JRawFormatter<>).MakeGenericType(type);
                return GetDefaultOrCreate(formatterType);
            }

            // JValue
            if (typeof(JValue).IsAssignableFrom(type))
            {
                var formatterType = typeof(JValueFormatter<>).MakeGenericType(type);
                return GetDefaultOrCreate(formatterType);
            }

            // If type can not get, must return null for fallback mechanism.
            return null;
        }

        public bool IsSupportedType(Type type)
        {
            if (formatterMap.ContainsKey(type)) { return true; }

            if (typeof(JObject).IsAssignableFrom(type)) { return true; }
            if (typeof(JArray).IsAssignableFrom(type)) { return true; }
            if (typeof(JProperty).IsAssignableFrom(type)) { return true; }
            if (typeof(JRaw).IsAssignableFrom(type)) { return true; }
            if (typeof(JValue).IsAssignableFrom(type)) { return true; }

            return false;
        }

        public static ICustomJsonFormatter GetDefaultOrCreate(Type type)
        {
            return (ICustomJsonFormatter)(type.GetField("Default", BindingFlags.Public | BindingFlags.Static)
                                        ?.GetValue(null) ?? Activator.CreateInstance(type)); // leave the createinstance here, this helps with recursive types
        }

        static readonly Dictionary<Type, ICustomJsonFormatter> formatterMap = new Dictionary<Type, ICustomJsonFormatter>()
        {
            { typeof(JsonDocument), JsonDocumentFormatter.Default },
            { typeof(JsonElement), JsonElementFormatter.Default },
            { typeof(JsonProperty), JsonPropertyFormatter.Default },

            { typeof(SpanJsonDynamicObject), DynamicObjectFormatter.Default },
            { typeof(SpanJsonDynamicArray<char>), DynamicUtf16ArrayFormatter.Default },
            { typeof(SpanJsonDynamicArray<byte>), DynamicUtf8ArrayFormatter.Default },
            { typeof(SpanJsonDynamicUtf16Number), DynamicUtf16NumberFormatter.Default },
            { typeof(SpanJsonDynamicUtf16String), DynamicUtf16StringFormatter.Default },
            { typeof(SpanJsonDynamicUtf8Number), DynamicUtf8NumberFormatter.Default },
            { typeof(SpanJsonDynamicUtf8String), DynamicUtf8StringFormatter.Default },

            { typeof(JToken), JTokenFormatter.Default },
            { typeof(JObject), JObjectFormatter.Default },
            { typeof(JArray), JArrayFormatter.Default },
        };
    }
}
