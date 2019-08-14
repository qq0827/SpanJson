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

using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SpanJson.Linq
{
    partial class JContainer : INotifyCollectionChanged
    {
        internal ListChangedEventHandler _listChanged;
        internal AddingNewEventHandler _addingNew;

        private bool _busy;

        /// <summary>Occurs when the list changes or an item in the list changes.</summary>
        public event ListChangedEventHandler ListChanged
        {
            add => _listChanged += value;
            remove => _listChanged -= value;
        }

        /// <summary>Occurs before an item is added to the collection.</summary>
        public event AddingNewEventHandler AddingNew
        {
            add => _addingNew += value;
            remove => _addingNew -= value;
        }

        internal NotifyCollectionChangedEventHandler _collectionChanged;

        /// <summary>Occurs when the items list of the collection has changed, or the collection is reset.</summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { _collectionChanged += value; }
            remove { _collectionChanged -= value; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void CheckReentrancy()
        {
            if (_busy) { ThrowHelper2.ThrowInvalidOperationException_Cannot_change_during_a_collection_change_event(this); }
        }

        /// <summary>Raises the <see cref="AddingNew"/> event.</summary>
        /// <param name="e">The <see cref="AddingNewEventArgs"/> instance containing the event data.</param>
        protected virtual void OnAddingNew(AddingNewEventArgs e)
        {
            _addingNew?.Invoke(this, e);
        }

        /// <summary>Raises the <see cref="ListChanged"/> event.</summary>
        /// <param name="e">The <see cref="ListChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnListChanged(ListChangedEventArgs e)
        {
            ListChangedEventHandler handler = _listChanged;

            if (handler is object)
            {
                _busy = true;
                try
                {
                    handler(this, e);
                }
                finally
                {
                    _busy = false;
                }
            }
        }

        /// <summary>Raises the <see cref="CollectionChanged"/> event.</summary>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler handler = _collectionChanged;

            if (handler is object)
            {
                _busy = true;
                try
                {
                    handler(this, e);
                }
                finally
                {
                    _busy = false;
                }
            }
        }
    }
}