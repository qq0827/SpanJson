using System;
using System.ComponentModel;
using System.Dynamic;
using System.Text.Encodings.Web;
using SpanJson.Resolvers;

namespace SpanJson
{
    public interface IJsonFormatterResolver
    {
        IJsonFormatter GetFormatter(Type type);
        IJsonFormatter GetFormatter(JsonMemberInfo info, Type overrideMemberType = null);
        JsonObjectDescription GetDynamicObjectDescription(IDynamicMetaObjectProvider provider);
    }

    public interface IJsonFormatterResolver<TSymbol> : IJsonFormatterResolver
        where TSymbol : struct
    {
        SpanJsonOptions JsonOptions { get; }

        JsonEscapeHandling EscapeHandling { get; }
        /// <summary>The encoder to use when escaping strings, or <see langword="null" /> to use the default encoder.</summary>
        JavaScriptEncoder Encoder { get; }

        /// <summary>Only support for custom formatters.</summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool IsSupportedType(Type type);

        IJsonFormatter<object, TSymbol> GetRuntimeFormatter();
        IJsonFormatter<T, TSymbol> GetEnumStringFormatter<T>() where T : struct, Enum;
        IJsonFormatter<T, TSymbol> GetEnumIntegerFormatter<T>() where T : struct, Enum;
        IJsonFormatter<T, TSymbol> GetFormatter<T>();
        JsonObjectDescription GetObjectDescription<T>();

        Func<T> GetCreateFunctor<T>();
        Func<T, TConverted> GetEnumerableConvertFunctor<T, TConverted>();

        /// <summary>Resolves the key of the dictionary.</summary>
        /// <param name="dictionaryKey">Key of the dictionary.</param>
        /// <returns>Resolved key of the dictionary.</returns>
        string ResolveDictionaryKey(string dictionaryKey);

        /// <summary>Resolves the name of the extension data.</summary>
        /// <param name="extensionDataName">Name of the extension data.</param>
        /// <returns>Resolved name of the extension data.</returns>
        string ResolveExtensionDataName(string extensionDataName);

        /// <summary>Resolves the name of the property.</summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Resolved name of the property.</returns>
        string ResolvePropertyName(string propertyName);

        /// <summary>TBD</summary>
        /// <param name="dictionaryKey">TBD</param>
        /// <remarks>Note:
        /// The dictionary data is very small
        /// The dictionary key is known
        /// </remarks>
        JsonEncodedText GetEncodedDictionaryKey(string dictionaryKey);
        JsonEncodedText GetEncodedExtensionDataName(string extensionDataName);
        JsonEncodedText GetEncodedPropertyName(string propertyName);
    }

    public interface IJsonFormatterResolver<TSymbol, in TResolver> : IJsonFormatterResolver<TSymbol>
        where TResolver : IJsonFormatterResolver<TSymbol, TResolver>, new()
        where TSymbol : struct
    {
    }

    public interface ICustomJsonFormatterResolver
    {
        bool IsSupportedType(Type type);

        ICustomJsonFormatter GetFormatter(Type type);
        //ICustomJsonFormatter<T> GetFormatter<T>();
    }
}