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
using System.Collections.Specialized;
using System.Runtime.Serialization;
using SpanJson.Utilities;

namespace SpanJson.Linq
{
    /// <summary>Represents a JSON array.</summary>
    public partial class JArray : JContainer, IList<JToken>
    {
        private readonly List<JToken> _values = new List<JToken>();
        [IgnoreDataMember]
        internal object _dynamicJson;

        /// <summary>Gets the container's children tokens.</summary>
        /// <value>The container's children tokens.</value>
        protected override IList<JToken> ChildrenTokens => _values;

        /// <summary>Gets the node type for this <see cref="JToken"/>.</summary>
        /// <value>The type.</value>
        public override JTokenType Type => JTokenType.Array;

        /// <summary>Initializes a new instance of the <see cref="JArray"/> class.</summary>
        public JArray() { }

        /// <summary>Initializes a new instance of the <see cref="JArray"/> class from another <see cref="JArray"/> object.</summary>
        /// <param name="other">A <see cref="JArray"/> object to copy from.</param>
        public JArray(JArray other) : base(other) { }

        /// <summary>Initializes a new instance of the <see cref="JArray"/> class with the specified content.</summary>
        /// <param name="content">The contents of the array.</param>
        public JArray(params object[] content)
        {
            Add(CastMultiContent(content));
        }

        /// <summary>Initializes a new instance of the <see cref="JArray"/> class with the specified content.</summary>
        /// <param name="content">The contents of the array.</param>
        public JArray(object content)
        {
            if (TryReadJsonDynamic(content, out JToken token))
            {
                if (token.Type == JTokenType.Array)
                {
                    AddContainer((JArray)token);
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
            return (node is JArray t && ContentsEqual(t));
        }

        internal override JToken CloneToken()
        {
            return new JArray(this);
        }

        /// <summary>Gets the <see cref="JToken"/> with the specified key.</summary>
        /// <value>The <see cref="JToken"/> with the specified key.</value>
        public override JToken this[object key]
        {
            get
            {
                return key switch
                {
                    null => throw ThrowHelper.GetArgumentNullException(ExceptionArgument.key),
                    int idx => GetItem(idx),
                    _ => throw ThrowHelper2.GetArgumentException_Accessed_JArray_values_with_invalid_key_value(key),
                };
            }
            set
            {
                switch (key)
                {
                    case null:
                        throw ThrowHelper.GetArgumentNullException(ExceptionArgument.key);

                    case int idx:
                        SetItem(idx, value);
                        break;

                    default:
                        throw ThrowHelper2.GetArgumentException_Set_JArray_values_with_invalid_key_value(key);
                }
            }
        }

        /// <summary>Gets or sets the <see cref="JToken"/> at the specified index.</summary>
        /// <value></value>
        public JToken this[int index]
        {
            get => GetItem(index);
            set => SetItem(index, value);
        }

        internal override int IndexOfItem(JToken item)
        {
            if (item is null) { return -1; }

            return _values.IndexOfReference(item);
        }

        internal override void MergeItem(object content, JsonMergeSettings settings)
        {
            IEnumerable a = null;
            if (IsMultiContent(content))
            {
                a = CastMultiContent((IEnumerable)content);
            }
            else if (content is JArray)
            {
                a = (IEnumerable)content;
            }

            if (a is null) { return; }

            MergeEnumerableContent(this, a, settings);
        }

        internal override int GetDeepHashCode()
        {
            return ContentsHashCode();
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_dynamicJson is object) { _dynamicJson = null; }
            base.OnCollectionChanged(e);
        }
    }
}