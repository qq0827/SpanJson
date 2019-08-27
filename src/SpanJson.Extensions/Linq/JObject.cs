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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using SpanJson.Document;
using SpanJson.Dynamic;
using SpanJson.Resolvers;

namespace SpanJson.Linq
{
    /// <summary>Represents a JSON object.</summary>
    public partial class JObject : JContainer
    {
        private readonly JPropertyKeyedCollection _properties = new JPropertyKeyedCollection();
        [IgnoreDataMember]
        internal object _dynamicJson;

        /// <summary>Gets the container's children tokens.</summary>
        /// <value>The container's children tokens.</value>
        protected override IList<JToken> ChildrenTokens => _properties;

        /// <summary>Initializes a new instance of the <see cref="JObject"/> class.</summary>
        public JObject() { }

        /// <summary>Initializes a new instance of the <see cref="JObject"/> class from another <see cref="JObject"/> object.</summary>
        /// <param name="other">A <see cref="JObject"/> object to copy from.</param>
        public JObject(JObject other) : base(other) { }

        /// <summary>Initializes a new instance of the <see cref="JObject"/> class with the specified content.</summary>
        /// <param name="content">The contents of the object.</param>
        public JObject(params object[] content)
        {
            Add(CastMultiContent(content));
        }

        /// <summary>Initializes a new instance of the <see cref="JObject"/> class with the specified content.</summary>
        /// <param name="content">The contents of the object.</param>
        public JObject(object content)
        {
            if (TryReadJsonDynamic(content, out JToken token))
            {
                if (token.Type == JTokenType.Object)
                {
                    AddContainer((JObject)token);
                }
                else
                {
                    Add(token);
                }
            }
            else
            {
                Add(content);
            }
        }

        internal override bool DeepEquals(JToken node)
        {
            if (!(node is JObject t)) { return false; }

            return _properties.Compare(t._properties);
        }

        internal override int IndexOfItem(JToken item)
        {
            return _properties.IndexOfReference(item);
        }

        internal override void InsertItem(int index, JToken item, bool skipParentCheck)
        {
            // don't add comments to JObject, no name to reference comment by
            if (item is object && item.Type == JTokenType.Comment) { return; }

            base.InsertItem(index, item, skipParentCheck);
        }

        internal override void ValidateToken(JToken o, JToken existing)
        {
            if (o is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.o); }

            if (o.Type != JTokenType.Property)
            {
                ThrowHelper2.ThrowArgumentException_CanNotAdd(o);
            }

            JProperty newProperty = (JProperty)o;

            if (existing is object)
            {
                JProperty existingProperty = (JProperty)existing;

                if (newProperty.Name == existingProperty.Name)
                {
                    return;
                }
            }

            if (_properties.TryGetValue(newProperty.Name, out _))
            {
                ThrowHelper2.ThrowArgumentException_CanNotAddProperty(newProperty);
            }
        }

        internal override void MergeItem(object content, JsonMergeSettings settings)
        {
            JObject jobj;
            switch (content)
            {
                case JsonDocument doc when doc.RootElement.ValueKind == JsonValueKind.Object:
                    jobj = (JObject)JToken.FromDocument(doc);
                    break;

                case JsonElement element when element.ValueKind == JsonValueKind.Object:
                    jobj = (JObject)JToken.FromElement(element);
                    break;

                case SpanJsonDynamicObject dynamicObject:
                    jobj = (JObject)FromDynamicInternal<IncludeNullsOriginalCaseResolver<char>>(dynamicObject);
                    break;

                case JObject o: jobj = o; break;

                default: return;
            }

            foreach (KeyValuePair<string, JToken> contentItem in jobj)
            {
                JProperty existingProperty;
                var propertyNameComparison = settings?.PropertyNameComparison;
                if (!propertyNameComparison.HasValue || StringComparison.Ordinal == propertyNameComparison.Value)
                {
                    existingProperty = Property(contentItem.Key);
                }
                else
                {
                    existingProperty = Property(contentItem.Key, propertyNameComparison.Value);
                }

                if (existingProperty is null)
                {
                    Add(contentItem.Key, contentItem.Value);
                }
                else if (contentItem.Value is object)
                {
                    if (!(existingProperty.Value is JContainer existingContainer) || existingContainer.Type != contentItem.Value.Type)
                    {
                        if (!IsNull(contentItem.Value) || settings?.MergeNullValueHandling == MergeNullValueHandling.Merge)
                        {
                            existingProperty.Value = contentItem.Value;
                        }
                    }
                    else
                    {
                        existingContainer.Merge(contentItem.Value, settings);
                    }
                }
            }
        }

        internal override JToken CloneToken()
        {
            return new JObject(this);
        }

        /// <summary>Gets the node type for this <see cref="JToken"/>.</summary>
        /// <value>The type.</value>
        public override JTokenType Type => JTokenType.Object;

        /// <summary>Gets an <see cref="IEnumerable{T}"/> of <see cref="JProperty"/> of this object's properties.</summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="JProperty"/> of this object's properties.</returns>
        public IEnumerable<JProperty> Properties()
        {
            return _properties.Cast<JProperty>();
        }

        /// <summary>Gets a <see cref="JProperty"/> with the specified name.</summary>
        /// <param name="name">The property name.</param>
        /// <returns>A <see cref="JProperty"/> with the specified name or <c>null</c>.</returns>
        public JProperty Property(string name)
        {
            if (name is null) { return null; }

            if (_properties.TryGetValue(name, out JToken property))
            {
                return (JProperty)property;
            }

            return null;
        }

        /// <summary>Gets the <see cref="JProperty"/> with the specified name.
        /// The exact name will be searched for first and if no matching property is found then
        /// the <see cref="StringComparison"/> will be used to match a property.</summary>
        /// <param name="name">The property name.</param>
        /// <param name="comparison">One of the enumeration values that specifies how the strings will be compared.</param>
        /// <returns>A <see cref="JProperty"/> matched with the specified name or <c>null</c>.</returns>
        public JProperty Property(string name, StringComparison comparison)
        {
            if (name is null) { return null; }

            if (_properties.TryGetValue(name, out JToken property))
            {
                return (JProperty)property;
            }

            // test above already uses this comparison so no need to repeat
            if (comparison != StringComparison.Ordinal)
            {
                for (int i = 0; i < _properties.Count; i++)
                {
                    JProperty p = (JProperty)_properties[i];
                    if (string.Equals(p.Name, name, comparison))
                    {
                        return p;
                    }
                }
            }

            return null;
        }

        /// <summary>Gets a <see cref="JEnumerable{T}"/> of <see cref="JToken"/> of this object's property values.</summary>
        /// <returns>A <see cref="JEnumerable{T}"/> of <see cref="JToken"/> of this object's property values.</returns>
        public JEnumerable<JToken> PropertyValues()
        {
            return new JEnumerable<JToken>(Properties().Select(p => p.Value));
        }

        /// <summary>Gets the <see cref="JToken"/> with the specified key.</summary>
        /// <value>The <see cref="JToken"/> with the specified key.</value>
        public override JToken this[object key]
        {
            get
            {
                switch (key)
                {
                    case null:
                        throw ThrowHelper.GetArgumentNullException(ExceptionArgument.key);

                    case string propertyName:
                        return this[propertyName];

                    default:
                        throw ThrowHelper2.GetArgumentException_Accessed_JObject_values_with_invalid_key_value(key);
                }
            }
            set
            {
                switch (key)
                {
                    case null:
                        throw ThrowHelper.GetArgumentNullException(ExceptionArgument.key);

                    case string propertyName:
                        this[propertyName] = value;
                        break;

                    default:
                        throw ThrowHelper2.GetArgumentException_Set_JObject_values_with_invalid_key_value(key);
                }
            }
        }

        /// <summary>Gets or sets the <see cref="JToken"/> with the specified property name.</summary>
        /// <value></value>
        public JToken this[string propertyName]
        {
            get
            {
                if (propertyName is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.propertyName); }

                JProperty property = Property(propertyName);

                return property?.Value;
            }
            set
            {
                JProperty property = Property(propertyName);
                if (property is object)
                {
                    property.Value = value;
                }
                else
                {
                    OnPropertyChanging(propertyName);
                    Add(new JProperty(propertyName, value));
                    OnPropertyChanged(propertyName);
                }
            }
        }

        /// <summary>Gets the <see cref="JToken"/> with the specified property name.</summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The <see cref="JToken"/> with the specified property name.</returns>
        public JToken GetValue(string propertyName)
        {
            //if (propertyName is null) { return null; } // Property 也会对 propertyName 进行判断

            // attempt to get value via dictionary first for performance
            var property = Property(propertyName);

            return property?.Value;
        }

        /// <summary>Gets the <see cref="JToken"/> with the specified property name.
        /// The exact property name will be searched for first and if no matching property is found then
        /// the <see cref="StringComparison"/> will be used to match a property.</summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="comparison">One of the enumeration values that specifies how the strings will be compared.</param>
        /// <returns>The <see cref="JToken"/> with the specified property name.</returns>
        public JToken GetValue(string propertyName, StringComparison comparison)
        {
            //if (propertyName is null) { return null; } // Property 也会对 propertyName 进行判断

            // attempt to get value via dictionary first for performance
            var property = Property(propertyName, comparison);

            return property?.Value;
        }

        /// <summary>Tries to get the <see cref="JToken"/> with the specified property name.
        /// The exact property name will be searched for first and if no matching property is found then
        /// the <see cref="StringComparison"/> will be used to match a property.</summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        /// <param name="comparison">One of the enumeration values that specifies how the strings will be compared.</param>
        /// <returns><c>true</c> if a value was successfully retrieved; otherwise, <c>false</c>.</returns>
        public bool TryGetValue(string propertyName, StringComparison comparison, out JToken value)
        {
            value = GetValue(propertyName, comparison);
            return (value is object);
        }

        internal override int GetDeepHashCode()
        {
            return ContentsHashCode();
        }
    }
}
