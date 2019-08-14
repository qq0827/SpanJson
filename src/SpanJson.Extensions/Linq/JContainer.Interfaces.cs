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
using System.ComponentModel;
using System.Threading;

namespace SpanJson.Linq
{
    partial class JContainer : IList<JToken>, ITypedList, IBindingList, IList
    {
        #region -- ITypedList Members --

        string ITypedList.GetListName(PropertyDescriptor[] listAccessors)
        {
            return string.Empty;
        }

        PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            ICustomTypeDescriptor d = First as ICustomTypeDescriptor;
            return d?.GetProperties();
        }

        #endregion

        #region -- IList<JToken> Members --

        int IList<JToken>.IndexOf(JToken item)
        {
            return IndexOfItem(item);
        }

        void IList<JToken>.Insert(int index, JToken item)
        {
            InsertItem(index, item, false);
        }

        void IList<JToken>.RemoveAt(int index)
        {
            RemoveItemAt(index);
        }

        JToken IList<JToken>.this[int index]
        {
            get => GetItem(index);
            set => SetItem(index, value);
        }

        #endregion

        #region -- ICollection<JToken> Members --

        void ICollection<JToken>.Add(JToken item)
        {
            Add(item);
        }

        void ICollection<JToken>.Clear()
        {
            ClearItems();
        }

        bool ICollection<JToken>.Contains(JToken item)
        {
            return ContainsItem(item);
        }

        void ICollection<JToken>.CopyTo(JToken[] array, int arrayIndex)
        {
            CopyItemsTo(array, arrayIndex);
        }

        bool ICollection<JToken>.IsReadOnly => false;

        bool ICollection<JToken>.Remove(JToken item)
        {
            return RemoveItem(item);
        }

        #endregion

        #region -- IList Members --

        int IList.Add(object value)
        {
            Add(EnsureValue(value));
            return Count - 1;
        }

        void IList.Clear()
        {
            ClearItems();
        }

        bool IList.Contains(object value)
        {
            return ContainsItem(EnsureValue(value));
        }

        int IList.IndexOf(object value)
        {
            return IndexOfItem(EnsureValue(value));
        }

        void IList.Insert(int index, object value)
        {
            InsertItem(index, EnsureValue(value), false);
        }

        bool IList.IsFixedSize => false;

        bool IList.IsReadOnly => false;

        void IList.Remove(object value)
        {
            RemoveItem(EnsureValue(value));
        }

        void IList.RemoveAt(int index)
        {
            RemoveItemAt(index);
        }

        object IList.this[int index]
        {
            get => GetItem(index);
            set => SetItem(index, EnsureValue(value));
        }

        #endregion

        #region -- ICollection Members --

        void ICollection.CopyTo(Array array, int index)
        {
            CopyItemsTo(array, index);
        }

        /// <summary>
        /// Gets the count of child JSON tokens.
        /// </summary>
        /// <value>The count of child JSON tokens.</value>
        public int Count => ChildrenTokens.Count;

        bool ICollection.IsSynchronized => false;

        private object _syncRoot;
        object ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot is null)
                {
                    Interlocked.CompareExchange(ref _syncRoot, new object(), null);
                }

                return _syncRoot;
            }
        }

        #endregion

        #region -- IBindingList Members --

        void IBindingList.AddIndex(PropertyDescriptor property)
        {
        }

        object IBindingList.AddNew()
        {
            AddingNewEventArgs args = new AddingNewEventArgs();
            OnAddingNew(args);

            switch (args.NewObject)
            {
                case null:
                    throw ThrowHelper2.GetJsonException_Could_not_determine_new_value_to_add_to(this);

                case JToken newItem:
                    Add(newItem);
                    return newItem;

                default:
                    throw ThrowHelper2.GetJsonException_New_item_to_be_added_to_collection_must_be_compatible_with();
            }
        }

        bool IBindingList.AllowEdit => true;

        bool IBindingList.AllowNew => true;

        bool IBindingList.AllowRemove => true;

        void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        int IBindingList.Find(PropertyDescriptor property, object key)
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        bool IBindingList.IsSorted => false;

        void IBindingList.RemoveIndex(PropertyDescriptor property)
        {
        }

        void IBindingList.RemoveSort()
        {
            throw ThrowHelper.GetNotSupportedException();
        }

        ListSortDirection IBindingList.SortDirection => ListSortDirection.Ascending;

        PropertyDescriptor IBindingList.SortProperty => null;

        bool IBindingList.SupportsChangeNotification => true;

        bool IBindingList.SupportsSearching => false;

        bool IBindingList.SupportsSorting => false;

        #endregion
    }
}