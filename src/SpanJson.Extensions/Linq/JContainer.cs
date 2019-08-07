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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace SpanJson.Linq
{
    /// <summary>Represents a token that can contain other tokens.</summary>
    public abstract partial class JContainer : JToken
    {
        /// <summary>Gets the container's children tokens.</summary>
        /// <value>The container's children tokens.</value>
        protected abstract IList<JToken> ChildrenTokens { get; }

        internal JContainer() { }

        internal JContainer(JContainer other)
            : this()
        {
            AddContainer(other);
        }

        internal void AddContainer(JContainer other)
        {
            if (null == other) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.other); }

            int i = 0;
            foreach (JToken child in other)
            {
                AddInternal(i, child, false);
                i++;
            }
        }

        internal static IEnumerable CastMultiContent(IEnumerable content)
        {
            if (null == content) { return null; }

            if (!HasJsonDynamic(content)) { return content; }

            var list = new List<object>();
            foreach (var item in content)
            {
                if (TryReadJsonDynamic(item, out JToken token))
                {
                    list.Add(token);
                }
                else
                {
                    list.Add(item);
                }
            }
            return list;
        }

        internal virtual IList<JToken> CreateChildrenCollection()
        {
            return new List<JToken>();
        }

        /// <summary>Gets a value indicating whether this token has child tokens.</summary>
        /// <value><c>true</c> if this token has child values; otherwise, <c>false</c>.</value>
        public override bool HasValues => ChildrenTokens.Count > 0;

        internal bool ContentsEqual(JContainer container)
        {
            if (container == this)
            {
                return true;
            }

            IList<JToken> t1 = ChildrenTokens;
            IList<JToken> t2 = container.ChildrenTokens;

            if (t1.Count != t2.Count)
            {
                return false;
            }

            for (int i = 0; i < t1.Count; i++)
            {
                if (!t1[i].DeepEquals(t2[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Get the first child token of this token.</summary>
        /// <value>A <see cref="JToken"/> containing the first child token of the <see cref="JToken"/>.</value>
        public override JToken First
        {
            get
            {
                IList<JToken> children = ChildrenTokens;
                return (children.Count > 0) ? children[0] : null;
            }
        }

        /// <summary>Get the last child token of this token.</summary>
        /// <value>A <see cref="JToken"/> containing the last child token of the <see cref="JToken"/>.</value>
        public override JToken Last
        {
            get
            {
                IList<JToken> children = ChildrenTokens;
                int count = children.Count;
                return (count > 0) ? children[count - 1] : null;
            }
        }

        /// <summary>Returns a collection of the child tokens of this token, in document order.</summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="JToken"/> containing the child tokens of this <see cref="JToken"/>, in document order.</returns>
        public override JEnumerable<JToken> Children()
        {
            return new JEnumerable<JToken>(ChildrenTokens);
        }

        /// <summary>Returns a collection of the child values of this token, in document order.</summary>
        /// <typeparam name="T">The type to convert the values to.</typeparam>
        /// <returns>A <see cref="IEnumerable{T}"/> containing the child values of this <see cref="JToken"/>, in document order.</returns>
        public override IEnumerable<T> Values<T>()
        {
            return ChildrenTokens.Convert<JToken, T>();
        }

        /// <summary>Returns a collection of the descendant tokens for this token in document order.</summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="JToken"/> containing the descendant tokens of the <see cref="JToken"/>.</returns>
        public IEnumerable<JToken> Descendants()
        {
            return GetDescendants(false);
        }

        /// <summary>Returns a collection of the tokens that contain this token,
        /// and all descendant tokens of this token, in document order.</summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="JToken"/> containing this token,
        /// and all the descendant tokens of the <see cref="JToken"/>.</returns>
        public IEnumerable<JToken> DescendantsAndSelf()
        {
            return GetDescendants(true);
        }

        internal IEnumerable<JToken> GetDescendants(bool self)
        {
            if (self)
            {
                yield return this;
            }

            foreach (JToken o in ChildrenTokens)
            {
                yield return o;
                if (o is JContainer c)
                {
                    foreach (JToken d in c.Descendants())
                    {
                        yield return d;
                    }
                }
            }
        }

        internal bool IsMultiContent(object content)
        {
            switch (content)
            {
                case string _:
                    return false;
                case JToken _:
                    return false;
                case byte[] _:
                    return false;
                case IEnumerable _:
                    return true;
                default:
                    return false;
            }
        }

        internal JToken EnsureParentToken(JToken item, bool skipParentCheck)
        {
            if (item == null)
            {
                return JValue.CreateNull();
            }

            if (skipParentCheck)
            {
                return item;
            }

            // to avoid a token having multiple parents or creating a recursive loop, create a copy if...
            // the item already has a parent
            // the item is being added to itself
            // the item is being added to the root parent of itself
            if (item.Parent != null || item == this || (item.HasValues && Root == item))
            {
                item = item.CloneToken();
            }

            return item;
        }

        internal abstract int IndexOfItem(JToken item);

        internal virtual void InsertItem(int index, JToken item, bool skipParentCheck)
        {
            IList<JToken> children = ChildrenTokens;

            if (index > children.Count)
            {
                ThrowHelper2.ThrowArgumentOutOfRangeException_Index_must_be_within_the_bounds_of_the_List(ExceptionArgument.index);
            }

            CheckReentrancy();

            item = EnsureParentToken(item, skipParentCheck);

            JToken previous = (0u >= (uint)index) ? null : children[index - 1];
            // haven't inserted new token yet so next token is still at the inserting index
            JToken next = (index == children.Count) ? null : children[index];

            ValidateToken(item, null);

            item.Parent = this;

            item.Previous = previous;
            if (previous != null)
            {
                previous.Next = item;
            }

            item.Next = next;
            if (next != null)
            {
                next.Previous = item;
            }

            children.Insert(index, item);

            if (_listChanged != null)
            {
                OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
            }
            if (_collectionChanged != null)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            }
        }

        internal virtual void RemoveItemAt(int index)
        {
            IList<JToken> children = ChildrenTokens;

            var uidx = (uint)index;
            if (uidx > JsonSharedConstant.TooBigOrNegative) { ThrowHelper2.ThrowArgumentOutOfRangeException_Index(); }
            if (uidx >= (uint)children.Count)
            {
                ThrowHelper2.ThrowArgumentOutOfRangeException_Index_is_equal_to_or_greater_than_Count();
            }

            CheckReentrancy();

            JToken item = children[index];
            JToken previous = (0u >= (uint)index) ? null : children[index - 1];
            JToken next = (index == children.Count - 1) ? null : children[index + 1];

            if (previous != null)
            {
                previous.Next = next;
            }
            if (next != null)
            {
                next.Previous = previous;
            }

            item.Parent = null;
            item.Previous = null;
            item.Next = null;

            children.RemoveAt(index);

            if (_listChanged != null)
            {
                OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
            }
            if (_collectionChanged != null)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            }
        }

        internal virtual bool RemoveItem(JToken item)
        {
            int index = IndexOfItem(item);
            if (index >= 0)
            {
                RemoveItemAt(index);
                return true;
            }

            return false;
        }

        internal virtual JToken GetItem(int index)
        {
            return ChildrenTokens[index];
        }

        internal virtual void SetItem(int index, JToken item)
        {
            IList<JToken> children = ChildrenTokens;

            if (index < 0) { ThrowHelper2.ThrowArgumentOutOfRangeException_Index(); }
            if (index >= children.Count)
            {
                ThrowHelper2.ThrowArgumentOutOfRangeException_Index_is_equal_to_or_greater_than_Count();
            }

            JToken existing = children[index];

            if (IsTokenUnchanged(existing, item))
            {
                return;
            }

            CheckReentrancy();

            item = EnsureParentToken(item, false);

            ValidateToken(item, existing);

            JToken previous = (0u >= (uint)index) ? null : children[index - 1];
            JToken next = (index == children.Count - 1) ? null : children[index + 1];

            item.Parent = this;

            item.Previous = previous;
            if (previous != null)
            {
                previous.Next = item;
            }

            item.Next = next;
            if (next != null)
            {
                next.Previous = item;
            }

            children[index] = item;

            existing.Parent = null;
            existing.Previous = null;
            existing.Next = null;

            if (_listChanged != null)
            {
                OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, index));
            }
            if (_collectionChanged != null)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, existing, index));
            }
        }

        internal virtual void ClearItems()
        {
            CheckReentrancy();

            IList<JToken> children = ChildrenTokens;

            foreach (JToken item in children)
            {
                item.Parent = null;
                item.Previous = null;
                item.Next = null;
            }

            children.Clear();

            if (_listChanged != null)
            {
                OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
            }
            if (_collectionChanged != null)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        internal virtual void ReplaceItem(JToken existing, JToken replacement)
        {
            if (existing == null || existing.Parent != this)
            {
                return;
            }

            int index = IndexOfItem(existing);
            SetItem(index, replacement);
        }

        internal virtual bool ContainsItem(JToken item)
        {
            return (IndexOfItem(item) != -1);
        }

        internal virtual void CopyItemsTo(Array array, int arrayIndex)
        {
            if (array == null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array); }
            if ((uint)arrayIndex > JsonSharedConstant.TooBigOrNegative) { ThrowHelper2.ThrowArgumentOutOfRangeException_ArrayIndex(); }
            if ((uint)arrayIndex >= (uint)array.Length && arrayIndex != 0) { ThrowHelper2.ThrowArgumentException_ArrayIndex(); }
            if ((uint)Count > (uint)(array.Length - arrayIndex)) { ThrowHelper2.ThrowArgumentException_The_number_of_elements(); }

            int index = 0;
            foreach (JToken token in ChildrenTokens)
            {
                array.SetValue(token, arrayIndex + index);
                index++;
            }
        }

        internal static bool IsTokenUnchanged(JToken currentValue, JToken newValue)
        {
            if (currentValue is JValue v1)
            {
                // null will get turned into a JValue of type null
                if (v1.Type == JTokenType.Null && newValue == null)
                {
                    return true;
                }

                return v1.Equals(newValue);
            }

            return false;
        }

        internal virtual void ValidateToken(JToken o, JToken existing)
        {
            if (null == o) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.o); }

            if (o.Type == JTokenType.Property)
            {
                ThrowHelper2.ThrowArgumentException_CanNotAdd(o, this);
            }
        }

        /// <summary>Adds the specified content as children of this <see cref="JToken"/>.</summary>
        /// <param name="content">The content to be added.</param>
        public virtual void Add(object content)
        {
            AddInternal(ChildrenTokens.Count, content, false);
        }

        internal void AddAndSkipParentCheck(JToken token)
        {
            AddInternal(ChildrenTokens.Count, token, true);
        }

        /// <summary>Adds the specified content as the first children of this <see cref="JToken"/>.</summary>
        /// <param name="content">The content to be added.</param>
        public void AddFirst(object content)
        {
            AddInternal(0, content, false);
        }

        internal void AddInternal(int index, object content, bool skipParentCheck)
        {
            if (IsMultiContent(content))
            {
                IEnumerable enumerable = (IEnumerable)content;

                int multiIndex = index;
                foreach (object c in enumerable)
                {
                    AddInternal(multiIndex, c, skipParentCheck);
                    multiIndex++;
                }
            }
            else
            {
                JToken item = CreateFromContent(content);

                InsertItem(index, item, skipParentCheck);
            }
        }

        internal static JToken CreateFromContent(object content)
        {
            if (content is JToken token)
            {
                return token;
            }

            return new JValue(content);
        }

        /// <summary>Replaces the child nodes of this token with the specified content.</summary>
        /// <param name="content">The content.</param>
        public void ReplaceAll(object content)
        {
            ClearItems();
            Add(content);
        }

        /// <summary>Removes the child nodes from this token.</summary>
        public void RemoveAll()
        {
            ClearItems();
        }

        internal abstract void MergeItem(object content, JsonMergeSettings settings);

        /// <summary>Merge the specified content into this <see cref="JToken"/>.</summary>
        /// <param name="content">The content to be merged.</param>
        public void Merge(object content)
        {
            MergeItem(content, new JsonMergeSettings());
        }

        /// <summary>Merge the specified content into this <see cref="JToken"/> using <see cref="JsonMergeSettings"/>.</summary>
        /// <param name="content">The content to be merged.</param>
        /// <param name="settings">The <see cref="JsonMergeSettings"/> used to merge the content.</param>
        public void Merge(object content, JsonMergeSettings settings)
        {
            MergeItem(content, settings);
        }

        internal int ContentsHashCode()
        {
            int hashCode = 0;
            foreach (JToken item in ChildrenTokens)
            {
                hashCode ^= item.GetDeepHashCode();
            }
            return hashCode;
        }

        private JToken EnsureValue(object value)
        {
            if (value == null) { return null; }

            if (value is JToken token) { return token; }

            throw ThrowHelper2.GetArgumentException_Argument_is_not_a_JToken();
        }

        internal static void MergeEnumerableContent(JContainer target, IEnumerable content, JsonMergeSettings settings)
        {
            switch (settings.MergeArrayHandling)
            {
                case MergeArrayHandling.Concat:
                    foreach (JToken item in content)
                    {
                        target.Add(item);
                    }
                    break;
                case MergeArrayHandling.Union:
                    HashSet<JToken> items = new HashSet<JToken>(target, EqualityComparer);

                    foreach (JToken item in content)
                    {
                        if (items.Add(item))
                        {
                            target.Add(item);
                        }
                    }
                    break;
                case MergeArrayHandling.Replace:
                    target.ClearItems();
                    foreach (JToken item in content)
                    {
                        target.Add(item);
                    }
                    break;
                case MergeArrayHandling.Merge:
                    int i = 0;
                    foreach (object targetItem in content)
                    {
                        if (i < target.Count)
                        {
                            JToken sourceItem = target[i];

                            if (sourceItem is JContainer existingContainer)
                            {
                                existingContainer.Merge(targetItem, settings);
                            }
                            else
                            {
                                if (targetItem != null)
                                {
                                    JToken contentValue = CreateFromContent(targetItem);
                                    if (contentValue.Type != JTokenType.Null)
                                    {
                                        target[i] = contentValue;
                                    }
                                }
                            }
                        }
                        else
                        {
                            target.Add(targetItem);
                        }

                        i++;
                    }
                    break;
                default:
                    ThrowHelper2.ThrowArgumentOutOfRangeException_Unexpected_merge_array_handling_when_merging_JSON();
                    break;
            }
        }
    }
}