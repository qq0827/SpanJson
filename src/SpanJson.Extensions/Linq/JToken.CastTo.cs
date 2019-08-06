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
using CuteAnt;
using SpanJson.Document;
using SpanJson.Dynamic;

namespace SpanJson.Linq
{
    partial class JToken
    {
        /// <summary>Performs an implicit conversion from <see cref="Boolean"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(bool value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Nullable{T}"/> of <see cref="Boolean"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(bool? value)
        {
            return new JValue(value);
        }




        /// <summary>Performs an implicit conversion from <see cref="SByte"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(sbyte value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Nullable{T}"/> of <see cref="SByte"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(sbyte? value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Byte"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(byte value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Nullable{T}"/> of <see cref="Byte"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(byte? value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Nullable{T}"/> of <see cref="Int16"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(short? value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Int16"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(short value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="UInt16"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(ushort value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Nullable{T}"/> of <see cref="UInt16"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(ushort? value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Int32"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(int value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Nullable{T}"/> of <see cref="Int32"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(int? value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="UInt32"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(uint value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Nullable{T}"/> of <see cref="UInt32"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(uint? value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Nullable{T}"/> of <see cref="Int64"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(long value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Nullable{T}"/> of <see cref="Int64"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(long? value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="UInt64"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(ulong value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Nullable{T}"/> of <see cref="UInt64"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(ulong? value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Decimal"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(decimal value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Nullable{T}"/> of <see cref="Decimal"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(decimal? value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Double"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(double value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Nullable{T}"/> of <see cref="Double"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(double? value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Single"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(float value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Nullable{T}"/> of <see cref="Single"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(float? value)
        {
            return new JValue(value);
        }




        /// <summary>Performs an implicit conversion from <see cref="String"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(string value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Byte"/>[] to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(byte[] value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="DateTime"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(DateTime value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Nullable{T}"/> of <see cref="DateTime"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(DateTime? value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="DateTimeOffset"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(DateTimeOffset value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Nullable{T}"/> of <see cref="DateTimeOffset"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(DateTimeOffset? value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="TimeSpan"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(TimeSpan value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Nullable{T}"/> of <see cref="TimeSpan"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(TimeSpan? value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Guid"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(Guid value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Nullable{T}"/> of <see cref="Guid"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(Guid? value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="CombGuid"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(CombGuid value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Nullable{T}"/> of <see cref="CombGuid"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(CombGuid? value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="JsonElement"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(JsonElement value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="SpanJsonDynamicUtf16Number"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(SpanJsonDynamicUtf16Number value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="SpanJsonDynamicUtf16String"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(SpanJsonDynamicUtf16String value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="SpanJsonDynamicUtf8Number"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(SpanJsonDynamicUtf8Number value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="SpanJsonDynamicUtf8String"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(SpanJsonDynamicUtf8String value)
        {
            return new JValue(value);
        }

        /// <summary>Performs an implicit conversion from <see cref="Uri"/> to <see cref="JToken"/>.</summary>
        /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
        /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
        public static implicit operator JToken(Uri value)
        {
            return new JValue(value);
        }
    }
}