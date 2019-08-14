using System.Runtime.CompilerServices;
using SpanJson.Document;
using SpanJson.Resolvers;
using SpanJson.Serialization;
using NJsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace SpanJson.Linq
{
    partial class JObject
    {
        /// <summary>Creates a <see cref="JObject"/> from an object.</summary>
        /// <param name="input">The object that will be used to create <see cref="JObject"/>.</param>
        /// <returns>A <see cref="JObject"/> with the value of the specified object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JObject From<T>(T input)
        {
            JToken token = FromInternal<T, IncludeNullsOriginalCaseResolver<char>>(input);

            return ToJObject(token);
        }

        /// <summary>Creates a <see cref="JObject"/> from an object.</summary>
        /// <param name="input">The object that will be used to create <see cref="JObject"/>.</param>
        /// <returns>A <see cref="JObject"/> with the value of the specified object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JObject From<T, TResolver>(T input)
            where TResolver : IJsonFormatterResolver<char, TResolver>, new()
        {
            JToken token = FromInternal<T, TResolver>(input);

            return ToJObject(token);
        }

        /// <summary>Creates a <see cref="JObject"/> from an object.</summary>
        /// <param name="o">The object that will be used to create <see cref="JObject"/>.</param>
        /// <returns>A <see cref="JObject"/> with the values of the specified object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JObject FromDynamic(object o)
        {
            JToken token = FromDynamicInternal<IncludeNullsOriginalCaseResolver<char>>(o);

            return ToJObject(token);
        }

        /// <summary>Creates a <see cref="JObject"/> from an object.</summary>
        /// <param name="o">The object that will be used to create <see cref="JObject"/>.</param>
        /// <returns>A <see cref="JObject"/> with the values of the specified object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JObject FromDynamic<TResolver>(object o)
            where TResolver : IJsonFormatterResolver<char, TResolver>, new()
        {
            JToken token = FromDynamicInternal<TResolver>(o);

            return ToJObject(token);
        }

        /// <summary>Creates a <see cref="JObject"/> from an object.</summary>
        /// <param name="o">The object that will be used to create <see cref="JObject"/>.</param>
        /// <returns>A <see cref="JObject"/> with the value of the specified object.</returns>
        public new static JObject FromObject(object o)
        {
            var jsonSerializer = DefaultSerializerPool.Take();
            try
            {
                JToken token = FromObjectInternal(o, jsonSerializer);

                return ToJObject(token);
            }
            finally
            {
                DefaultSerializerPool.Return(jsonSerializer);
            }
        }

        /// <summary>Creates a <see cref="JObject"/> from an object.</summary>
        /// <param name="o">The object that will be used to create <see cref="JObject"/>.</param>
        /// <returns>A <see cref="JObject"/> with the value of the specified object.</returns>
        public new static JObject FromPolymorphicObject(object o)
        {
            var jsonSerializer = PolymorphicSerializerPool.Take();
            try
            {
                JToken token = FromObjectInternal(o, jsonSerializer);

                return ToJObject(token);
            }
            finally
            {
                PolymorphicSerializerPool.Return(jsonSerializer);
            }
        }

        /// <summary>Creates a <see cref="JObject"/> from an object using the specified <see cref="NJsonSerializer"/>.</summary>
        /// <param name="o">The object that will be used to create <see cref="JObject"/>.</param>
        /// <param name="jsonSerializer">The <see cref="NJsonSerializer"/> that will be used when reading the object.</param>
        /// <returns>A <see cref="JObject"/> with the value of the specified object.</returns>
        public new static JObject FromObject(object o, NJsonSerializer jsonSerializer)
        {
            JToken token = FromObjectInternal(o, jsonSerializer);

            return ToJObject(token);
        }

        public new static JObject FromDocument(JsonDocument doc)
        {
            JToken token = JToken.FromDocument(doc);

            return ToJObject(token);
        }

        public new static JObject FromElement(in JsonElement element)
        {
            JToken token = JToken.FromElement(element);

            return ToJObject(token);
        }

        #region ** ToJObject **

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static JObject ToJObject(JToken token)
        {
            if (token is object && token.Type != JTokenType.Object)
            {
                ThrowHelper2.ThrowArgumentException_Object_serialized_to_JObject_instance_expected(token.Type);
            }

            return (JObject)token;
        }

        #endregion
    }
}
