using System.Runtime.CompilerServices;
using SpanJson.Document;
using SpanJson.Resolvers;
using SpanJson.Serialization;
using NJsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace SpanJson.Linq
{
    partial class JArray
    {
        /// <summary>Creates a <see cref="JArray"/> from an object.</summary>
        /// <param name="input">The object that will be used to create <see cref="JArray"/>.</param>
        /// <returns>A <see cref="JArray"/> with the value of the specified object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JArray From<T>(T input)
        {
            JToken token = FromInternal<T, IncludeNullsOriginalCaseResolver<char>>(input);

            return AsJArray(token);
        }

        /// <summary>Creates a <see cref="JArray"/> from an object.</summary>
        /// <param name="input">The object that will be used to create <see cref="JArray"/>.</param>
        /// <returns>A <see cref="JArray"/> with the value of the specified object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JArray From<T, TResolver>(T input)
            where TResolver : IJsonFormatterResolver<char, TResolver>, new()
        {
            JToken token = FromInternal<T, TResolver>(input);

            return AsJArray(token);
        }

        /// <summary>Creates a <see cref="JArray"/> from an object.</summary>
        /// <param name="o">The object that will be used to create <see cref="JArray"/>.</param>
        /// <returns>A <see cref="JArray"/> with the values of the specified object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JArray FromDynamic(object o)
        {
            JToken token = FromDynamicInternal<IncludeNullsOriginalCaseResolver<char>>(o);

            return AsJArray(token);
        }

        /// <summary>Creates a <see cref="JArray"/> from an object.</summary>
        /// <param name="o">The object that will be used to create <see cref="JArray"/>.</param>
        /// <returns>A <see cref="JArray"/> with the values of the specified object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JArray FromDynamic<TResolver>(object o)
            where TResolver : IJsonFormatterResolver<char, TResolver>, new()
        {
            JToken token = FromDynamicInternal<TResolver>(o);

            return AsJArray(token);
        }

        /// <summary>Creates a <see cref="JArray"/> from an object.</summary>
        /// <param name="o">The object that will be used to create <see cref="JArray"/>.</param>
        /// <returns>A <see cref="JArray"/> with the value of the specified object.</returns>
        public new static JArray FromObject(object o)
        {
            var jsonSerializer = DefaultSerializerPool.Take();
            try
            {
                JToken token = FromObjectInternal(o, jsonSerializer);

                return AsJArray(token);
            }
            finally
            {
                DefaultSerializerPool.Return(jsonSerializer);
            }
        }

        /// <summary>Creates a <see cref="JArray"/> from an object.</summary>
        /// <param name="o">The object that will be used to create <see cref="JArray"/>.</param>
        /// <returns>A <see cref="JArray"/> with the value of the specified object.</returns>
        public new static JArray FromPolymorphicObject(object o)
        {
            var jsonSerializer = PolymorphicSerializerPool.Take();
            try
            {
                JToken token = FromObjectInternal(o, jsonSerializer);

                return AsJArray(token);
            }
            finally
            {
                PolymorphicSerializerPool.Return(jsonSerializer);
            }
        }

        /// <summary>Creates a <see cref="JArray"/> from an object using the specified <see cref="NJsonSerializer"/>.</summary>
        /// <param name="o">The object that will be used to create <see cref="JArray"/>.</param>
        /// <param name="jsonSerializer">The <see cref="NJsonSerializer"/> that will be used when reading the object.</param>
        /// <returns>A <see cref="JArray"/> with the value of the specified object.</returns>
        public new static JArray FromObject(object o, NJsonSerializer jsonSerializer)
        {
            JToken token = FromObjectInternal(o, jsonSerializer);

            return AsJArray(token);
        }

        public new static JArray FromDocument(JsonDocument doc)
        {
            JToken token = JToken.FromDocument(doc);

            return AsJArray(token);
        }

        public new static JArray FromElement(in JsonElement element)
        {
            JToken token = JToken.FromElement(element);

            return AsJArray(token);
        }

        #region == AsJArray ==

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static JArray AsJArray(JToken token)
        {
            if (token is null || token.Type != JTokenType.Array)
            {
                ThrowHelper2.ThrowArgumentException_Object_serialized_to_JArray_instance_expected(token.Type);
            }

            return (JArray)token;
        }

        #endregion
    }
}
