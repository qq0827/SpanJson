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
using System.Globalization;

namespace SpanJson.Linq
{
    /// <summary>Represents a value in JSON (string, integer, date, etc).</summary>
    public partial class JValue : JToken, IFormattable
    {
        internal override bool DeepEquals(JToken node)
        {
            if (!(node is JValue other)) { return false; }
            if (other == this) { return true; }

            return ValuesEquals(this, other);
        }

        internal override JToken CloneToken()
        {
            return new JValue(this);
        }

        /// <summary>Creates a <see cref="JValue"/> comment with the given value.</summary>
        /// <param name="value">The value.</param>
        /// <returns>A <see cref="JValue"/> comment with the given value.</returns>
        public static JValue CreateComment(string value)
        {
            return new JValue(value, JTokenType.Comment);
        }

        /// <summary>Creates a <see cref="JValue"/> string with the given value.</summary>
        /// <param name="value">The value.</param>
        /// <returns>A <see cref="JValue"/> string with the given value.</returns>
        public static JValue CreateString(string value)
        {
            return new JValue(value, JTokenType.String);
        }

        /// <summary>Creates a <see cref="JValue"/> null value.</summary>
        /// <returns>A <see cref="JValue"/> null value.</returns>
        public static JValue CreateNull()
        {
            return new JValue(null, JTokenType.Null);
        }

        ///// <summary>Creates a <see cref="JValue"/> undefined value.</summary>
        ///// <returns>A <see cref="JValue"/> undefined value.</returns>
        //public static JValue CreateUndefined()
        //{
        //    return new JValue(null, JTokenType.Undefined);
        //}

        internal override int GetDeepHashCode()
        {
            int valueHashCode = (_value is object) ? _value.GetHashCode() : 0;

            // GetHashCode on an enum boxes so cast to int
            return ((int)_valueType).GetHashCode() ^ valueHashCode;
        }

        /// <summary>Returns a <see cref="String"/> that represents this instance.</summary>
        /// <returns>A <see cref="String"/> that represents this instance.</returns>
        public override string ToString()
        {
            if (_value is null) { return string.Empty; }

            return _value.ToString();
        }

        /// <summary>Returns a <see cref="String"/> that represents this instance.</summary>
        /// <param name="format">The format.</param>
        /// <returns>A <see cref="String"/> that represents this instance.</returns>
        public string ToString(string format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }

        /// <summary>Returns a <see cref="String"/> that represents this instance.</summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>A <see cref="String"/> that represents this instance.</returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return ToString(null, formatProvider);
        }

        /// <summary>Returns a <see cref="String"/> that represents this instance.</summary>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>A <see cref="String"/> that represents this instance.</returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (_value is null)
            {
                return string.Empty;
            }

            if (_value is IFormattable formattable)
            {
                return formattable.ToString(format, formatProvider);
            }
            else
            {
                return _value.ToString();
            }
        }
    }
}