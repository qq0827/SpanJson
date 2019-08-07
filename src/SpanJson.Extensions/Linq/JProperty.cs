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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace SpanJson.Linq
{
    /// <summary>Represents a JSON property.</summary>
    public partial class JProperty : JContainer
    {
        #region ** JPropertyList **

        private sealed class JPropertyList : IList<JToken>
        {
            internal JToken _token;

            public IEnumerator<JToken> GetEnumerator()
            {
                if (_token != null)
                {
                    yield return _token;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(JToken item)
            {
                _token = item;
            }

            public void Clear()
            {
                _token = null;
            }

            public bool Contains(JToken item)
            {
                return (_token == item) ? true : false;
            }

            public void CopyTo(JToken[] array, int arrayIndex)
            {
                if (_token != null)
                {
                    array[arrayIndex] = _token;
                }
            }

            public bool Remove(JToken item)
            {
                if (_token == item)
                {
                    _token = null;
                    return true;
                }
                return false;
            }

            public int Count => (_token != null) ? 1 : 0;

            public bool IsReadOnly => false;

            public int IndexOf(JToken item)
            {
                return (_token == item) ? 0 : -1;
            }

            public void Insert(int index, JToken item)
            {
                if (0u >= (uint)index) { _token = item; }
            }

            public void RemoveAt(int index)
            {
                if (0u >= (uint)index) { _token = null; }
            }

            public JToken this[int index]
            {
                get => (0u >= (uint)index) ? _token : null;
                set
                {
                    if (0u >= (uint)index) { _token = value; }
                }
            }
        }

        #endregion

        private readonly JPropertyList _content = new JPropertyList();
        private readonly string _name;

        /// <summary>Gets the container's children tokens.</summary>
        /// <value>The container's children tokens.</value>
        protected override IList<JToken> ChildrenTokens => _content;

        /// <summary>Gets the property name.</summary>
        /// <value>The property name.</value>
        public string Name
        {
            [DebuggerStepThrough]
            get { return _name; }
        }

        /// <summary>Gets or sets the property value.</summary>
        /// <value>The property value.</value>
        public JToken Value
        {
            [DebuggerStepThrough]
            get { return _content._token; }
            set
            {
                CheckReentrancy();

                JToken newValue = value ?? JValue.CreateNull();

                if (_content._token == null)
                {
                    InsertItem(0, newValue, false);
                }
                else
                {
                    SetItem(0, newValue);
                }
            }
        }

        /// <summary>Initializes a new instance of the <see cref="JProperty"/> class from another <see cref="JProperty"/> object.</summary>
        /// <param name="other">A <see cref="JProperty"/> object to copy from.</param>
        public JProperty(JProperty other)
            : base(other)
        {
            _name = other.Name;
        }

        internal override JToken GetItem(int index)
        {
            if (index != 0) { ThrowHelper.ThrowArgumentOutOfRangeException(); }

            return Value;
        }

        internal override void SetItem(int index, JToken item)
        {
            if (index != 0) { ThrowHelper.ThrowArgumentOutOfRangeException(); }

            if (IsTokenUnchanged(Value, item)) { return; }

            ((JObject)Parent)?.InternalPropertyChanging(this);

            base.SetItem(0, item);

            ((JObject)Parent)?.InternalPropertyChanged(this);
        }

        internal override bool RemoveItem(JToken item)
        {
            throw ThrowHelper2.GetJsonException_Cannot_add_or_remove_items_from_JProperty();
        }

        internal override void RemoveItemAt(int index)
        {
            throw ThrowHelper2.GetJsonException_Cannot_add_or_remove_items_from_JProperty();
        }

        internal override int IndexOfItem(JToken item)
        {
            return _content.IndexOf(item);
        }

        internal override void InsertItem(int index, JToken item, bool skipParentCheck)
        {
            // don't add comments to JProperty
            if (item != null && item.Type == JTokenType.Comment) { return; }

            if (Value != null) { ThrowHelper2.ThrowJsonException_Cannot_have_multiple_values_JProperty(); }

            base.InsertItem(0, item, false);
        }

        internal override bool ContainsItem(JToken item)
        {
            return (Value == item);
        }

        internal override void MergeItem(object content, JsonMergeSettings settings)
        {
            JToken value = (content as JProperty)?.Value;

            if (value != null && value.Type != JTokenType.Null)
            {
                Value = value;
            }
        }

        internal override void ClearItems()
        {
            throw ThrowHelper2.GetJsonException_Cannot_add_or_remove_items_from_JProperty();
        }

        internal override bool DeepEquals(JToken node)
        {
            return (node is JProperty t && _name == t.Name && ContentsEqual(t));
        }

        internal override JToken CloneToken()
        {
            return new JProperty(this);
        }

        /// <summary>Gets the node type for this <see cref="JToken"/>.</summary>
        /// <value>The type.</value>
        public override JTokenType Type
        {
            [DebuggerStepThrough]
            get { return JTokenType.Property; }
        }

        internal JProperty(string name)
        {
            // called from JTokenWriter
            if (null == name) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.name); }

            _name = name;
        }

        /// <summary>Initializes a new instance of the <see cref="JProperty"/> class.</summary>
        /// <param name="name">The property name.</param>
        /// <param name="content">The property content.</param>
        public JProperty(string name, params object[] content)
            : this(name, CastMultiContent(content))
        {
        }

        /// <summary>Initializes a new instance of the <see cref="JProperty"/> class.</summary>
        /// <param name="name">The property name.</param>
        /// <param name="content">The property content.</param>
        public JProperty(string name, object content)
        {
            if (null == name) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.name); }

            _name = name;

            Value = IsMultiContent(content)
                ? new JArray(content)
                : CreateFromContent(content);
        }

        internal override int GetDeepHashCode()
        {
            return _name.GetHashCode() ^ (Value?.GetDeepHashCode() ?? 0);
        }
    }
}