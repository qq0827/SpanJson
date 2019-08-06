using System;

namespace SpanJson.Document
{
    public static class JsonDocumentExtensions
    {
        /// <summary>Creates an instance of the specified .NET type from the <see cref="JsonDocument"/>.</summary>
        /// <typeparam name="T">The object type that the token will be deserialized to.</typeparam>
        /// <returns>The new object created from the JSON value.</returns>
        public static T ToObject<T>(this JsonDocument jsonDocument)
        {
            var rawSpan = jsonDocument.RawSpan;
            return JsonSerializer.Generic.Utf8.Deserialize<T>(rawSpan);
        }

        /// <summary>Creates an instance of the specified .NET type from the <see cref="JsonDocument"/>.</summary>
        /// <typeparam name="T">The object type that the token will be deserialized to.</typeparam>
        /// <typeparam name="TResolver">Resolver</typeparam>
        /// <returns>The new object created from the JSON value.</returns>
        public static T ToObject<T, TResolver>(this JsonDocument jsonDocument)
            where TResolver : IJsonFormatterResolver<byte, TResolver>, new()
        {
            var rawSpan = jsonDocument.RawSpan;
            return JsonSerializer.Generic.Utf8.Deserialize<T, TResolver>(rawSpan);
        }

        /// <summary>Creates an instance of the specified .NET type from the <see cref="JsonDocument"/>.</summary>
        /// <param name="jsonDocument"></param>
        /// <param name="objectType">The object type that the token will be deserialized to.</param>
        /// <returns>The new object created from the JSON value.</returns>
        public static object ToObject(this JsonDocument jsonDocument, Type objectType)
        {
            var rawSpan = jsonDocument.RawSpan;
            return JsonSerializer.NonGeneric.Utf8.Deserialize(rawSpan, objectType);
        }

        /// <summary>Creates an instance of the specified .NET type from the <see cref="JsonDocument"/>.</summary>
        /// <typeparam name="TResolver">Resolver</typeparam>
        /// <param name="jsonDocument"></param>
        /// <param name="objectType">The object type that the token will be deserialized to.</param>
        /// <returns>The new object created from the JSON value.</returns>
        public static object ToObject<TResolver>(this JsonDocument jsonDocument, Type objectType)
            where TResolver : IJsonFormatterResolver<byte, TResolver>, new()
        {
            var rawSpan = jsonDocument.RawSpan;
            return JsonSerializer.NonGeneric.Utf8.Deserialize<TResolver>(rawSpan, objectType);
        }

        /// <summary>Creates an instance of the specified .NET type from the <see cref="JsonElement"/>.</summary>
        /// <typeparam name="T">The object type that the token will be deserialized to.</typeparam>
        /// <returns>The new object created from the JSON value.</returns>
        public static T ToObject<T>(in this JsonElement jsonElement)
        {
            var rawSpan = jsonElement.RawSpan;
            return JsonSerializer.Generic.Utf8.Deserialize<T>(rawSpan);
        }

        /// <summary>Creates an instance of the specified .NET type from the <see cref="JsonElement"/>.</summary>
        /// <typeparam name="T">The object type that the token will be deserialized to.</typeparam>
        /// <typeparam name="TResolver">Resolver</typeparam>
        /// <returns>The new object created from the JSON value.</returns>
        public static T ToObject<T, TResolver>(in this JsonElement jsonElement)
            where TResolver : IJsonFormatterResolver<byte, TResolver>, new()
        {
            var rawSpan = jsonElement.RawSpan;
            return JsonSerializer.Generic.Utf8.Deserialize<T, TResolver>(rawSpan);
        }

        /// <summary>Creates an instance of the specified .NET type from the <see cref="JsonElement"/>.</summary>
        /// <param name="jsonElement"></param>
        /// <param name="objectType">The object type that the token will be deserialized to.</param>
        /// <returns>The new object created from the JSON value.</returns>
        public static object ToObject(in this JsonElement jsonElement, Type objectType)
        {
            var rawSpan = jsonElement.RawSpan;
            return JsonSerializer.NonGeneric.Utf8.Deserialize(rawSpan, objectType);
        }

        /// <summary>Creates an instance of the specified .NET type from the <see cref="JsonElement"/>.</summary>
        /// <typeparam name="TResolver">Resolver</typeparam>
        /// <param name="jsonElement"></param>
        /// <param name="objectType">The object type that the token will be deserialized to.</param>
        /// <returns>The new object created from the JSON value.</returns>
        public static object ToObject<TResolver>(in this JsonElement jsonElement, Type objectType)
            where TResolver : IJsonFormatterResolver<byte, TResolver>, new()
        {
            var rawSpan = jsonElement.RawSpan;
            return JsonSerializer.NonGeneric.Utf8.Deserialize<TResolver>(rawSpan, objectType);
        }
    }
}
