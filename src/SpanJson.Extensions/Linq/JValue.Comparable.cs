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
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Threading;
using CuteAnt;
using SpanJson.Utilities;

namespace SpanJson.Linq
{
    partial class JValue : IComparable, IComparable<JValue>
    {
        private static List<JValueCompareDelegate> s_comparers = new List<JValueCompareDelegate>();

        internal static List<JValueCompareDelegate> CustomComparers => Volatile.Read(ref s_comparers);

        public static bool RegisterCustomrComparer(params JValueCompareDelegate[] comparers)
        {
            if (null == comparers || 0u >= (uint)comparers.Length) { return false; }

            List<JValueCompareDelegate> snapshot, newCache;
            do
            {
                snapshot = Volatile.Read(ref s_comparers);
                newCache = new List<JValueCompareDelegate>();
                newCache.AddRange(s_comparers);
                if ((uint)snapshot.Count > 0u) { newCache.AddRange(snapshot); }
            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref s_comparers, newCache, snapshot), snapshot));
            return true;
        }

        private static int CompareBigInteger(BigInteger i1, object i2)
        {
            int result = i1.CompareTo(ConvertUtils.ToBigInteger(i2));

            if (result != 0)
            {
                return result;
            }

            // converting a fractional number to a BigInteger will lose the fraction check for
            // fraction if result is two numbers are equal
            if (i2 is decimal d1)
            {
                return (0m).CompareTo(Math.Abs(d1 - Math.Truncate(d1)));
            }
            else if (i2 is double || i2 is float)
            {
                double d = Convert.ToDouble(i2, CultureInfo.InvariantCulture);
                return (0d).CompareTo(Math.Abs(d - Math.Truncate(d)));
            }

            return result;
        }

        internal static int Compare(JTokenType valueType, object objA, object objB)
        {
            if (objA == objB) { return 0; }
            if (objB == null) { return 1; }
            if (objA == null) { return -1; }

            switch (valueType)
            {
                case JTokenType.Integer:
                    {
                        if (objA is BigInteger integerA)
                        {
                            return CompareBigInteger(integerA, objB);
                        }
                        if (objB is BigInteger integerB)
                        {
                            return -CompareBigInteger(integerB, objA);
                        }
                        if (objA is ulong || objB is ulong || objA is decimal || objB is decimal)
                        {
                            return ConvertUtils.ConvertOrCast<decimal>(objA, CultureInfo.InvariantCulture).CompareTo(ConvertUtils.ConvertOrCast<decimal>(objB, CultureInfo.InvariantCulture));
                        }
                        else if (objA is float || objB is float || objA is double || objB is double)
                        {
                            return CompareFloat(objA, objB);
                        }
                        else
                        {
                            return ConvertUtils.ConvertOrCast<long>(objA, CultureInfo.InvariantCulture).CompareTo(ConvertUtils.ConvertOrCast<long>(objB, CultureInfo.InvariantCulture));
                        }
                    }
                case JTokenType.Float:
                    {
                        if (objA is BigInteger integerA)
                        {
                            return CompareBigInteger(integerA, objB);
                        }
                        if (objB is BigInteger integerB)
                        {
                            return -CompareBigInteger(integerB, objA);
                        }
                        if (objA is ulong || objB is ulong || objA is decimal || objB is decimal)
                        {
                            return ConvertUtils.ConvertOrCast<decimal>(objA, CultureInfo.InvariantCulture).CompareTo(ConvertUtils.ConvertOrCast<decimal>(objB, CultureInfo.InvariantCulture));
                        }
                        return CompareFloat(objA, objB);
                    }
                case JTokenType.Dynamic:
                //{
                //    switch (objB)
                //    {
                //        case ulong _:
                //        case decimal _:
                //            return ConvertUtils.ConvertOrCast<decimal>(objA, CultureInfo.InvariantCulture).CompareTo(ConvertUtils.ConvertOrCast<decimal>(objB, CultureInfo.InvariantCulture));
                //        case float _:
                //        case double _:
                //            return CompareFloat(objA, objB);

                //        case sbyte _:
                //        case byte _:
                //        case short _:
                //        case ushort _:
                //        case int _:
                //        case uint _:
                //        case long _:
                //            return ConvertUtils.ConvertOrCast<long>(objA, CultureInfo.InvariantCulture).CompareTo(ConvertUtils.ConvertOrCast<long>(objB, CultureInfo.InvariantCulture));

                //        default:
                //            string sa = ConvertUtils.ToString(objA);
                //            string sb = ConvertUtils.ToString(objB);

                //            return string.CompareOrdinal(sa, sb);
                //    }
                //}
                case JTokenType.Comment:
                case JTokenType.String:
                case JTokenType.Raw:
                    string s1 = ConvertUtils.ToString(objA);
                    string s2 = ConvertUtils.ToString(objB);

                    return string.CompareOrdinal(s1, s2);

                case JTokenType.Boolean:
                    bool b1 = Convert.ToBoolean(objA, CultureInfo.InvariantCulture);
                    bool b2 = Convert.ToBoolean(objB, CultureInfo.InvariantCulture);

                    return b1.CompareTo(b2);

                case JTokenType.Date:
                    if (objA is DateTime dateA)
                    {
                        DateTime dateB;

                        if (objB is DateTimeOffset offsetB)
                        {
                            dateB = offsetB.DateTime;
                        }
                        else
                        {
                            dateB = Convert.ToDateTime(objB, CultureInfo.InvariantCulture);
                        }

                        return dateA.CompareTo(dateB);
                    }
                    else
                    {
                        DateTimeOffset offsetA = (DateTimeOffset)objA;
                        if (!(objB is DateTimeOffset offsetB))
                        {
                            offsetB = new DateTimeOffset(Convert.ToDateTime(objB, CultureInfo.InvariantCulture));
                        }

                        return offsetA.CompareTo(offsetB);
                    }
                case JTokenType.Bytes:
                    if (!(objB is byte[] bytesB))
                    {
                        throw ThrowHelper2.GetArgumentException_MustBe<byte[]>();
                    }

                    byte[] bytesA = objA as byte[];
                    Debug.Assert(bytesA != null);

                    return bytesA.AsSpan().SequenceCompareTo(bytesB);

                case JTokenType.Guid:
                    if (!(objB is Guid guid2))
                    {
                        throw ThrowHelper2.GetArgumentException_MustBe<Guid>();
                    }

                    Guid guid1 = (Guid)objA;

                    return guid1.CompareTo(guid2);

                case JTokenType.CombGuid:
                    if (!(objB is CombGuid comb2))
                    {
                        throw ThrowHelper2.GetArgumentException_MustBe<CombGuid>();
                    }

                    CombGuid comb1 = (CombGuid)objA;

                    return comb1.CompareTo(comb2);

                case JTokenType.Uri:
                    Uri uri2 = objB as Uri;
                    if (uri2 == null)
                    {
                        throw ThrowHelper2.GetArgumentException_MustBe<Uri>();
                    }

                    Uri uri1 = (Uri)objA;

                    return Comparer<string>.Default.Compare(uri1.ToString(), uri2.ToString());

                case JTokenType.TimeSpan:
                    if (!(objB is TimeSpan ts2))
                    {
                        throw ThrowHelper2.GetArgumentException_MustBe<TimeSpan>();
                    }

                    TimeSpan ts1 = (TimeSpan)objA;

                    return ts1.CompareTo(ts2);

                default:
                    var comparers = CustomComparers;
                    foreach (var comparer in comparers)
                    {
                        if (comparer(valueType, objA, objB, out int result))
                        {
                            return result;
                        }
                    }
                    throw ThrowHelper2.GetArgumentOutOfRangeException_UnexpectedValueType(valueType);
            }
        }

        internal static int CompareFloat(object objA, object objB)
        {
            double d1 = ConvertUtils.ConvertOrCast<double>(objA, CultureInfo.InvariantCulture);
            double d2 = ConvertUtils.ConvertOrCast<double>(objB, CultureInfo.InvariantCulture);

            // take into account possible floating point errors
            if (MathUtils.ApproxEquals(d1, d2))
            {
                return 0;
            }

            return d1.CompareTo(d2);
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj == null) { return 1; }

            JTokenType comparisonType;
            object otherValue;
            if (obj is JValue value)
            {
                otherValue = value.Value;
                comparisonType = (_valueType.IsString() && _valueType != value._valueType)
                    ? value._valueType
                    : _valueType;
            }
            else
            {
                otherValue = obj;
                comparisonType = _valueType;
            }

            return Compare(comparisonType, _value, otherValue);
        }

        /// <summary>Compares the current instance with another object of the same type and returns an integer that indicates 
        /// whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.</summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings:
        /// Value
        /// Meaning
        /// Less than zero
        /// This instance is less than <paramref name="obj"/>.
        /// Zero
        /// This instance is equal to <paramref name="obj"/>.
        /// Greater than zero
        /// This instance is greater than <paramref name="obj"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// 	<paramref name="obj"/> is not of the same type as this instance.
        /// </exception>
        public int CompareTo(JValue obj)
        {
            if (obj == null)
            {
                return 1;
            }

            JTokenType comparisonType = (_valueType.IsString() && _valueType != obj._valueType)
                ? obj._valueType
                : _valueType;

            return Compare(comparisonType, _value, obj._value);
        }
    }
}