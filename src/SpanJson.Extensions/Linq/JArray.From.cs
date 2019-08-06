using System.Runtime.CompilerServices;
using SpanJson.Document;
using SpanJson.Resolvers;

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

            return ToJArray(token);
        }

        /// <summary>Creates a <see cref="JArray"/> from an object.</summary>
        /// <param name="input">The object that will be used to create <see cref="JArray"/>.</param>
        /// <returns>A <see cref="JArray"/> with the value of the specified object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JArray From<T, TResolver>(T input)
            where TResolver : IJsonFormatterResolver<char, TResolver>, new()
        {
            JToken token = FromInternal<T, TResolver>(input);

            return ToJArray(token);
        }

        /// <summary>Creates a <see cref="JArray"/> from an object.</summary>
        /// <param name="o">The object that will be used to create <see cref="JArray"/>.</param>
        /// <returns>A <see cref="JArray"/> with the values of the specified object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JArray FromDynamic(object o)
        {
            JToken token = FromDynamicInternal<IncludeNullsOriginalCaseResolver<char>>(o);

            return ToJArray(token);
        }

        /// <summary>Creates a <see cref="JArray"/> from an object.</summary>
        /// <param name="o">The object that will be used to create <see cref="JArray"/>.</param>
        /// <returns>A <see cref="JArray"/> with the values of the specified object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public new static JArray FromDynamic<TResolver>(object o)
            where TResolver : IJsonFormatterResolver<char, TResolver>, new()
        {
            JToken token = FromDynamicInternal<TResolver>(o);

            return ToJArray(token);
        }

        /// <summary>Creates a <see cref="JArray"/> from an object.</summary>
        /// <param name="o">The object that will be used to create <see cref="JArray"/>.</param>
        /// <returns>A <see cref="JArray"/> with the value of the specified object.</returns>
        public new static JArray FromObject(object o)
        {
            JToken token = FromObjectInternal(o, Newtonsoft.Json.JsonSerializer.Create(DefaultSettings));

            return ToJArray(token);
        }

        /// <summary>Creates a <see cref="JArray"/> from an object using the specified <see cref="Newtonsoft.Json.JsonSerializer"/>.</summary>
        /// <param name="o">The object that will be used to create <see cref="JArray"/>.</param>
        /// <param name="jsonSerializer">The <see cref="Newtonsoft.Json.JsonSerializer"/> that will be used when reading the object.</param>
        /// <returns>A <see cref="JArray"/> with the value of the specified object.</returns>
        public new static JArray FromObject(object o, Newtonsoft.Json.JsonSerializer jsonSerializer)
        {
            JToken token = FromObjectInternal(o, jsonSerializer);

            return ToJArray(token);
        }

        public new static JArray FromDocument(JsonDocument doc)
        {
            JToken token = JToken.FromDocument(doc);

            return ToJArray(token);
        }

        public new static JArray FromElement(in JsonElement element)
        {
            JToken token = JToken.FromElement(element);

            return ToJArray(token);
        }

        #region ** ToJArray **

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static JArray ToJArray(JToken token)
        {
            if (token.Type != JTokenType.Array)
            {
                ThrowHelper2.ThrowArgumentException_Object_serialized_to_JArray_instance_expected(token.Type);
            }

            return (JArray)token;
        }

        #endregion
    }
}
