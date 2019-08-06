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

namespace SpanJson.Linq
{
    partial class JArray : IList<JToken>
    {
        #region -- IList<JToken> Members --

        /// <summary>
        /// Determines the index of a specific item in the <see cref="JArray"/>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="JArray"/>.</param>
        /// <returns>
        /// The index of <paramref name="item"/> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(JToken item)
        {
            return IndexOfItem(item);
        }

        /// <summary>
        /// Inserts an item to the <see cref="JArray"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="JArray"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is not a valid index in the <see cref="JArray"/>.
        /// </exception>
        public void Insert(int index, JToken item)
        {
            InsertItem(index, item, false);
        }

        /// <summary>
        /// Removes the <see cref="JArray"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is not a valid index in the <see cref="JArray"/>.
        /// </exception>
        public void RemoveAt(int index)
        {
            RemoveItemAt(index);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> of <see cref="JToken"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<JToken> GetEnumerator()
        {
            return Children().GetEnumerator();
        }

        #endregion

        #region -- ICollection<JToken> Members --

        /// <summary>
        /// Adds an item to the <see cref="JArray"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="JArray"/>.</param>
        public void Add(JToken item)
        {
            Add((object)item);
        }

        /// <summary>
        /// Removes all items from the <see cref="JArray"/>.
        /// </summary>
        public void Clear()
        {
            ClearItems();
        }

        /// <summary>
        /// Determines whether the <see cref="JArray"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="JArray"/>.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="item"/> is found in the <see cref="JArray"/>; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(JToken item)
        {
            return ContainsItem(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="JArray"/> to an array, starting at a particular array index.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(JToken[] array, int arrayIndex)
        {
            CopyItemsTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="JArray"/> is read-only.
        /// </summary>
        /// <returns><c>true</c> if the <see cref="JArray"/> is read-only; otherwise, <c>false</c>.</returns>
        public bool IsReadOnly => false;

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="JArray"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="JArray"/>.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="item"/> was successfully removed from the <see cref="JArray"/>; otherwise, <c>false</c>. This method also returns <c>false</c> if <paramref name="item"/> is not found in the original <see cref="JArray"/>.
        /// </returns>
        public bool Remove(JToken item)
        {
            return RemoveItem(item);
        }

        #endregion
    }
}
