#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Xml;
using System.Xml.Linq;
using NFormatting = Newtonsoft.Json.Formatting;
using NTypeNameHandling = Newtonsoft.Json.TypeNameHandling;
using NJsonConverter = Newtonsoft.Json.JsonConverter;
using NJsonSerializer = Newtonsoft.Json.JsonSerializer;
using NJsonSerializerSettings = Newtonsoft.Json.JsonSerializerSettings;
using NXmlNodeConverter = Newtonsoft.Json.Converters.XmlNodeConverter;

namespace SpanJson.Serialization
{
    /// <summary>Provides methods for converting between .NET types and JSON types.</summary>
    public static partial class JsonConvertX
    {
        #region -- Serialize --

        /// <summary>Serializes the specified object to a JSON string.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string SerializeObject(object value)
        {
            return SerializeObject(value, null, (NJsonSerializerSettings)null);
        }

        /// <summary>Serializes the specified object to a JSON string using formatting.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string SerializeObject(object value, NFormatting formatting)
        {
            return SerializeObject(value, formatting, (NJsonSerializerSettings)null);
        }

        /// <summary>Serializes the specified object to a JSON string using a collection of <see cref="NJsonConverter"/>.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="converters">A collection of converters used while serializing.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string SerializeObject(object value, params NJsonConverter[] converters)
        {
            var settings = (converters != null && converters.Length > 0)
                ? new NJsonSerializerSettings { Converters = converters }
                : null;

            return SerializeObject(value, null, settings);
        }

        /// <summary>Serializes the specified object to a JSON string using formatting and a collection of <see cref="NJsonConverter"/>.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <param name="converters">A collection of converters used while serializing.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string SerializeObject(object value, NFormatting formatting, params NJsonConverter[] converters)
        {
            var settings = (converters != null && converters.Length > 0)
                ? new NJsonSerializerSettings { Converters = converters }
                : null;

            return SerializeObject(value, null, formatting, settings);
        }

        /// <summary>Serializes the specified object to a JSON string using <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to serialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string SerializeObject(object value, NJsonSerializerSettings settings)
        {
            return SerializeObject(value, null, settings);
        }

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to serialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="NTypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string SerializeObject(object value, Type type, NJsonSerializerSettings settings)
        {
            var jsonSerializer = NJsonSerializer.CreateDefault(settings);

            return jsonSerializer.SerializeObject(value, type);
        }

        /// <summary>Serializes the specified object to a JSON string using formatting and <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to serialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string SerializeObject(object value, NFormatting formatting, NJsonSerializerSettings settings)
        {
            return SerializeObject(value, null, formatting, settings);
        }

        /// <summary>Serializes the specified object to a JSON string using a type, formatting and <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to serialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <param name="type">The type of the value being serialized.
        /// This parameter is used when <see cref="NJsonSerializer.TypeNameHandling"/> is <see cref="NTypeNameHandling.Auto"/> to write out the type name if the type of the value does not match.
        /// Specifying the type is optional.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string SerializeObject(object value, Type type, NFormatting formatting, NJsonSerializerSettings settings)
        {
            var jsonSerializer = NJsonSerializer.CreateDefault(settings);
            jsonSerializer.Formatting = formatting;

            return jsonSerializer.SerializeObject(value, type);
        }

        #endregion

        #region -- Deserialize --

        /// <summary>Deserializes the JSON to a .NET object.</summary>
        /// <param name="value">The JSON to deserialize.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeObject(string value)
        {
            return DeserializeObject(value, null, (NJsonSerializerSettings)null);
        }

        /// <summary>Deserializes the JSON to a .NET object using <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="value">The JSON to deserialize.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to deserialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeObject(string value, NJsonSerializerSettings settings)
        {
            return DeserializeObject(value, null, settings);
        }

        /// <summary>Deserializes the JSON to the specified .NET type.</summary>
        /// <param name="value">The JSON to deserialize.</param>
        /// <param name="type">The <see cref="Type"/> of object being deserialized.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeObject(string value, Type type)
        {
            return DeserializeObject(value, type, (NJsonSerializerSettings)null);
        }

        /// <summary>Deserializes the JSON to the specified .NET type.</summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="value">The JSON to deserialize.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static T DeserializeObject<T>(string value)
        {
            return DeserializeObject<T>(value, (NJsonSerializerSettings)null);
        }

        /// <summary>Deserializes the JSON to the given anonymous type.</summary>
        /// <typeparam name="T">The anonymous type to deserialize to. This can't be specified
        /// traditionally and must be inferred from the anonymous type passed as a parameter.</typeparam>
        /// <param name="value">The JSON to deserialize.</param>
        /// <param name="anonymousTypeObject">The anonymous type object.</param>
        /// <returns>The deserialized anonymous type from the JSON string.</returns>
        public static T DeserializeAnonymousType<T>(string value, T anonymousTypeObject)
        {
            return DeserializeObject<T>(value);
        }

        /// <summary>Deserializes the JSON to the given anonymous type using <see cref="NJsonSerializerSettings"/>.</summary>
        /// <typeparam name="T">The anonymous type to deserialize to. This can't be specified
        /// traditionally and must be inferred from the anonymous type passed as a parameter.</typeparam>
        /// <param name="value">The JSON to deserialize.</param>
        /// <param name="anonymousTypeObject">The anonymous type object.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to deserialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <returns>The deserialized anonymous type from the JSON string.</returns>
        public static T DeserializeAnonymousType<T>(string value, T anonymousTypeObject, NJsonSerializerSettings settings)
        {
            return DeserializeObject<T>(value, settings);
        }

        /// <summary>Deserializes the JSON to the specified .NET type using a collection of <see cref="NJsonConverter"/>.</summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="value">The JSON to deserialize.</param>
        /// <param name="converters">Converters to use while deserializing.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static T DeserializeObject<T>(string value, params NJsonConverter[] converters)
        {
            return (T)DeserializeObject(value, typeof(T), converters);
        }

        /// <summary>Deserializes the JSON to the specified .NET type using <see cref="NJsonSerializerSettings"/>.</summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="value">The object to deserialize.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to deserialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static T DeserializeObject<T>(string value, NJsonSerializerSettings settings)
        {
            return (T)DeserializeObject(value, typeof(T), settings);
        }

        /// <summary>Deserializes the JSON to the specified .NET type using a collection of <see cref="NJsonConverter"/>.</summary>
        /// <param name="value">The JSON to deserialize.</param>
        /// <param name="type">The type of the object to deserialize.</param>
        /// <param name="converters">Converters to use while deserializing.</param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeObject(string value, Type type, params NJsonConverter[] converters)
        {
            var settings = (converters != null && converters.Length > 0)
                ? new NJsonSerializerSettings { Converters = converters }
                : null;
            return DeserializeObject(value, type, settings);
        }

        /// <summary>Deserializes the JSON to the specified .NET type using <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="value">The JSON to deserialize.</param>
        /// <param name="type">The type of the object to deserialize to.</param>
        /// <param name="settings">
        /// The <see cref="NJsonSerializerSettings"/> used to deserialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.
        /// </param>
        /// <returns>The deserialized object from the JSON string.</returns>
        public static object DeserializeObject(string value, Type type, NJsonSerializerSettings settings)
        {
            var jsonSerializer = NJsonSerializer.CreateDefault(settings);
            return jsonSerializer.DeserializeObject(value, type);
        }

        #endregion

        #region -- Populate --

        /// <summary>Populates the object with values from the JSON string.</summary>
        /// <param name="value">The JSON to populate values from.</param>
        /// <param name="target">The target object to populate values onto.</param>
        public static void PopulateObject(string value, object target)
        {
            PopulateObject(value, target, null);
        }

        /// <summary>Populates the object with values from the JSON string using <see cref="NJsonSerializerSettings"/>.</summary>
        /// <param name="value">The JSON to populate values from.</param>
        /// <param name="target">The target object to populate values onto.</param>
        /// <param name="settings">The <see cref="NJsonSerializerSettings"/> used to deserialize the object.
        /// If this is <c>null</c>, default serialization settings will be used.</param>
        public static void PopulateObject(string value, object target, NJsonSerializerSettings settings)
        {
            var jsonSerializer = NJsonSerializer.CreateDefault(settings);

            jsonSerializer.PopulateObject(target, value);
        }

        #endregion

        #region -- Xml --

        /// <summary>Serializes the <see cref="XmlNode"/> to a JSON string.</summary>
        /// <param name="node">The node to serialize.</param>
        /// <returns>A JSON string of the <see cref="XmlNode"/>.</returns>
        public static string SerializeXmlNode(XmlNode node)
        {
            return SerializeXmlNode(node, NFormatting.None);
        }

        /// <summary>Serializes the <see cref="XmlNode"/> to a JSON string using formatting.</summary>
        /// <param name="node">The node to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <returns>A JSON string of the <see cref="XmlNode"/>.</returns>
        public static string SerializeXmlNode(XmlNode node, NFormatting formatting)
        {
            var converter = new NXmlNodeConverter();

            return SerializeObject(node, formatting, converter);
        }

        /// <summary>Serializes the <see cref="XmlNode"/> to a JSON string using formatting and omits the root object if <paramref name="omitRootObject"/> is <c>true</c>.</summary>
        /// <param name="node">The node to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <param name="omitRootObject">Omits writing the root object.</param>
        /// <returns>A JSON string of the <see cref="XmlNode"/>.</returns>
        public static string SerializeXmlNode(XmlNode node, NFormatting formatting, bool omitRootObject)
        {
            var converter = new NXmlNodeConverter { OmitRootObject = omitRootObject };

            return SerializeObject(node, formatting, converter);
        }

        /// <summary>Deserializes the <see cref="XmlNode"/> from a JSON string.</summary>
        /// <param name="value">The JSON string.</param>
        /// <returns>The deserialized <see cref="XmlNode"/>.</returns>
        public static XmlDocument DeserializeXmlNode(string value)
        {
            return DeserializeXmlNode(value, null);
        }

        /// <summary>Deserializes the <see cref="XmlNode"/> from a JSON string nested in a root element specified by <paramref name="deserializeRootElementName"/>.</summary>
        /// <param name="value">The JSON string.</param>
        /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
        /// <returns>The deserialized <see cref="XmlNode"/>.</returns>
        public static XmlDocument DeserializeXmlNode(string value, string deserializeRootElementName)
        {
            return DeserializeXmlNode(value, deserializeRootElementName, false);
        }

        /// <summary>Deserializes the <see cref="XmlNode"/> from a JSON string nested in a root element specified by <paramref name="deserializeRootElementName"/>
        /// and writes a Json.NET array attribute for collections.</summary>
        /// <param name="value">The JSON string.</param>
        /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
        /// <param name="writeArrayAttribute">A flag to indicate whether to write the Json.NET array attribute.
        /// This attribute helps preserve arrays when converting the written XML back to JSON.</param>
        /// <returns>The deserialized <see cref="XmlNode"/>.</returns>
        public static XmlDocument DeserializeXmlNode(string value, string deserializeRootElementName, bool writeArrayAttribute)
        {
            var converter = new NXmlNodeConverter()
            {
                DeserializeRootElementName = deserializeRootElementName,
                WriteArrayAttribute = writeArrayAttribute
            };
            return (XmlDocument)DeserializeObject(value, typeof(XmlDocument), converter);
        }

        /// <summary>Serializes the <see cref="XNode"/> to a JSON string.</summary>
        /// <param name="node">The node to convert to JSON.</param>
        /// <returns>A JSON string of the <see cref="XNode"/>.</returns>
        public static string SerializeXNode(XObject node)
        {
            return SerializeXNode(node, NFormatting.None);
        }

        /// <summary>Serializes the <see cref="XNode"/> to a JSON string using formatting.</summary>
        /// <param name="node">The node to convert to JSON.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <returns>A JSON string of the <see cref="XNode"/>.</returns>
        public static string SerializeXNode(XObject node, NFormatting formatting)
        {
            return SerializeXNode(node, formatting, false);
        }

        /// <summary>Serializes the <see cref="XNode"/> to a JSON string using formatting and omits the root object if <paramref name="omitRootObject"/> is <c>true</c>.</summary>
        /// <param name="node">The node to serialize.</param>
        /// <param name="formatting">Indicates how the output should be formatted.</param>
        /// <param name="omitRootObject">Omits writing the root object.</param>
        /// <returns>A JSON string of the <see cref="XNode"/>.</returns>
        public static string SerializeXNode(XObject node, NFormatting formatting, bool omitRootObject)
        {
            var converter = new NXmlNodeConverter { OmitRootObject = omitRootObject };

            return SerializeObject(node, formatting, converter);
        }

        /// <summary>Deserializes the <see cref="XNode"/> from a JSON string.</summary>
        /// <param name="value">The JSON string.</param>
        /// <returns>The deserialized <see cref="XNode"/>.</returns>
        public static XDocument DeserializeXNode(string value)
        {
            return DeserializeXNode(value, null);
        }

        /// <summary>Deserializes the <see cref="XNode"/> from a JSON string nested in a root element specified by <paramref name="deserializeRootElementName"/>.</summary>
        /// <param name="value">The JSON string.</param>
        /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
        /// <returns>The deserialized <see cref="XNode"/>.</returns>
        public static XDocument DeserializeXNode(string value, string deserializeRootElementName)
        {
            return DeserializeXNode(value, deserializeRootElementName, false);
        }

        /// <summary>Deserializes the <see cref="XNode"/> from a JSON string nested in a root element specified by <paramref name="deserializeRootElementName"/>
        /// and writes a Json.NET array attribute for collections.</summary>
        /// <param name="value">The JSON string.</param>
        /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
        /// <param name="writeArrayAttribute">A flag to indicate whether to write the Json.NET array attribute.
        /// This attribute helps preserve arrays when converting the written XML back to JSON.</param>
        /// <returns>The deserialized <see cref="XNode"/>.</returns>
        public static XDocument DeserializeXNode(string value, string deserializeRootElementName, bool writeArrayAttribute)
        {
            var converter = new NXmlNodeConverter()
            {
                DeserializeRootElementName = deserializeRootElementName,
                WriteArrayAttribute = writeArrayAttribute
            };
            return (XDocument)DeserializeObject(value, typeof(XDocument), converter);
        }

        #endregion
    }
}