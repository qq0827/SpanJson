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

namespace SpanJson.Linq
{
    partial class JValue : IEquatable<JValue>
    {
        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <returns><c>true</c> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.</returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(JValue other)
        {
            if (other is null) { return false; }

            return ValuesEquals(this, other);
        }

        private static bool ValuesEquals(JValue v1, JValue v2)
        {
            return (v1 == v2 || (v1._valueType == v2._valueType && Compare(v1._valueType, v1._value, v2._value).IsEqual()));
        }

        /// <summary>Determines whether the specified <see cref="Object"/> is equal to the current <see cref="Object"/>.</summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Object"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="Object"/> is equal to the current <see cref="Object"/>;
        /// otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj is JValue v)
            {
                return Equals(v);
            }

            return false;
        }

        /// <summary>Serves as a hash function for a particular type.</summary>
        /// <returns>A hash code for the current <see cref="Object"/>.</returns>
        public override int GetHashCode()
        {
            if (_value is null) { return 0; }

            return _value.GetHashCode();
        }
    }
}