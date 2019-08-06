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
using System.Buffers.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using CuteAnt;
using SpanJson.Document;
using SpanJson.Dynamic;
using SpanJson.Utilities;

namespace SpanJson.Linq
{
    partial class JToken
    {
        private static readonly HashSet<JTokenType> BooleanTypes = new HashSet<JTokenType>(new[] { JTokenType.Integer, JTokenType.Float, JTokenType.String, JTokenType.Comment, JTokenType.Dynamic, JTokenType.Raw, JTokenType.Boolean });
        private static readonly HashSet<JTokenType> NumberTypes = new HashSet<JTokenType>(new[] { JTokenType.Integer, JTokenType.Float, JTokenType.String, JTokenType.Comment, JTokenType.Dynamic, JTokenType.Raw, JTokenType.Boolean });
        private static readonly HashSet<JTokenType> BigIntegerTypes = new HashSet<JTokenType>(new[] { JTokenType.Integer, JTokenType.Float, JTokenType.String, JTokenType.Comment, JTokenType.Dynamic, JTokenType.Raw, JTokenType.Boolean, JTokenType.Bytes });
        private static readonly HashSet<JTokenType> StringTypes = new HashSet<JTokenType>(new[] { JTokenType.Date, JTokenType.Integer, JTokenType.Float, JTokenType.String, JTokenType.Comment, JTokenType.Dynamic, JTokenType.Raw, JTokenType.Boolean, JTokenType.Bytes, JTokenType.Guid, JTokenType.TimeSpan, JTokenType.Uri });
        private static readonly HashSet<JTokenType> GuidTypes = new HashSet<JTokenType>(new[] { JTokenType.String, JTokenType.Comment, JTokenType.Dynamic, JTokenType.Raw, JTokenType.Guid, JTokenType.CombGuid, JTokenType.Bytes });
        private static readonly HashSet<JTokenType> CombGuidTypes = new HashSet<JTokenType>(new[] { JTokenType.String, JTokenType.Comment, JTokenType.Dynamic, JTokenType.Raw, JTokenType.Guid, JTokenType.CombGuid, JTokenType.Bytes });
        private static readonly HashSet<JTokenType> TimeSpanTypes = new HashSet<JTokenType>(new[] { JTokenType.String, JTokenType.Comment, JTokenType.Dynamic, JTokenType.Raw, JTokenType.TimeSpan });
        private static readonly HashSet<JTokenType> UriTypes = new HashSet<JTokenType>(new[] { JTokenType.String, JTokenType.Comment, JTokenType.Dynamic, JTokenType.Raw, JTokenType.Uri });
        private static readonly HashSet<JTokenType> CharTypes = new HashSet<JTokenType>(new[] { JTokenType.Integer, JTokenType.Float, JTokenType.String, JTokenType.Comment, JTokenType.Dynamic, JTokenType.Raw });
        private static readonly HashSet<JTokenType> DateTimeTypes = new HashSet<JTokenType>(new[] { JTokenType.Date, JTokenType.String, JTokenType.Comment, JTokenType.Dynamic, JTokenType.Raw });
        private static readonly HashSet<JTokenType> BytesTypes = new HashSet<JTokenType>(new[] { JTokenType.Bytes, JTokenType.String, JTokenType.Comment, JTokenType.Dynamic, JTokenType.Raw, JTokenType.Integer });

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="System.Boolean"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator bool(JToken value)
        {
            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, BooleanTypes, false))
            {
                ThrowHelper2.ThrowArgumentException_Cast<bool>(value);
            }

            if (v.Value is BigInteger integer)
            {
                return Convert.ToBoolean((int)integer);
            }

            return Convert.ToBoolean(v.Value, CultureInfo.InvariantCulture);
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Nullable{T}"/> of <see cref="Boolean"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator bool?(JToken value)
        {
            if (value == null) { return null; }

            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, BooleanTypes, true))
            {
                ThrowHelper2.ThrowArgumentException_Cast<bool>(value);
            }

            if (v.Value is BigInteger integer)
            {
                return Convert.ToBoolean((int)integer);
            }

            return (v.Value != null) ? (bool?)Convert.ToBoolean(v.Value, CultureInfo.InvariantCulture) : null;
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Char"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator char(JToken value)
        {
            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, CharTypes, false))
            {
                ThrowHelper2.ThrowArgumentException_Cast<char>(value);
            }

            if (v.Value is BigInteger integer)
            {
                return (char)integer;
            }

            return Convert.ToChar(v.Value, CultureInfo.InvariantCulture);
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Nullable{T}"/> of <see cref="Char"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator char?(JToken value)
        {
            if (value == null) { return null; }

            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, CharTypes, true))
            {
                ThrowHelper2.ThrowArgumentException_Cast<char>(value);
            }

            if (v.Value is BigInteger integer)
            {
                return (char?)integer;
            }

            return (v.Value != null) ? (char?)Convert.ToChar(v.Value, CultureInfo.InvariantCulture) : null;
        }




        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="System.SByte"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator sbyte(JToken value)
        {
            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, false))
            {
                ThrowHelper2.ThrowArgumentException_Cast<sbyte>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case BigInteger integer:
                    return (sbyte)integer;

                case SpanJsonDynamicUtf16Number utf16Number:
                    return (sbyte)utf16Number;

                case SpanJsonDynamicUtf8Number utf8Number:
                    return (sbyte)utf8Number;

                case SpanJsonDynamicUtf16String utf16String:
                    return (sbyte)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (sbyte)utf8String;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Number:
                    return jsonNum.GetSByte();

                case JsonElement jsonStr when jsonStr.ValueKind == JsonValueKind.String:
                    {
                        var rawSpan = jsonStr.RawSpan;
                        var valueSpan = rawSpan.Slice(1, rawSpan.Length - 2);
                        if (Utf8Parser.TryParse(valueSpan, out sbyte tmp, out int consumed) && consumed == valueSpan.Length)
                        {
                            return tmp;
                        }
                        throw ThrowHelper2.GetArgumentException_Cast<sbyte>(value);
                    }

                default:
                    return Convert.ToSByte(tokenVal, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Nullable{T}"/> of <see cref="SByte"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator sbyte?(JToken value)
        {
            if (value == null) { return null; }

            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, true))
            {
                ThrowHelper2.ThrowArgumentException_Cast<sbyte>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case null:
                    return null;

                case BigInteger integer:
                    return (sbyte?)integer;

                case SpanJsonDynamicUtf16Number utf16Number:
                    return (sbyte)utf16Number;

                case SpanJsonDynamicUtf8Number utf8Number:
                    return (sbyte)utf8Number;

                case SpanJsonDynamicUtf16String utf16String:
                    return (sbyte)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (sbyte)utf8String;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Null:
                    return null;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Number:
                    return jsonNum.GetSByte();

                case JsonElement jsonStr when jsonStr.ValueKind == JsonValueKind.String:
                    {
                        var rawSpan = jsonStr.RawSpan;
                        var valueSpan = rawSpan.Slice(1, rawSpan.Length - 2);
                        if (Utf8Parser.TryParse(valueSpan, out sbyte tmp, out int consumed) && consumed == valueSpan.Length)
                        {
                            return tmp;
                        }
                        throw ThrowHelper2.GetArgumentException_Cast<sbyte>(value);
                    }

                default:
                    return Convert.ToSByte(tokenVal, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Byte"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator byte(JToken value)
        {
            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, false))
            {
                ThrowHelper2.ThrowArgumentException_Cast<byte>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case BigInteger integer:
                    return (byte)integer;

                case SpanJsonDynamicUtf16Number utf16Number:
                    return (byte)utf16Number;

                case SpanJsonDynamicUtf8Number utf8Number:
                    return (byte)utf8Number;

                case SpanJsonDynamicUtf16String utf16String:
                    return (byte)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (byte)utf8String;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Number:
                    return jsonNum.GetByte();

                case JsonElement jsonStr when jsonStr.ValueKind == JsonValueKind.String:
                    {
                        var rawSpan = jsonStr.RawSpan;
                        var valueSpan = rawSpan.Slice(1, rawSpan.Length - 2);
                        if (Utf8Parser.TryParse(valueSpan, out byte tmp, out int consumed) && consumed == valueSpan.Length)
                        {
                            return tmp;
                        }
                        throw ThrowHelper2.GetArgumentException_Cast<byte>(value);
                    }

                default:
                    return Convert.ToByte(tokenVal, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Nullable{T}"/> of <see cref="Byte"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator byte?(JToken value)
        {
            if (value == null) { return null; }

            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, true))
            {
                ThrowHelper2.ThrowArgumentException_Cast<byte>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case null:
                    return null;

                case BigInteger integer:
                    return (byte?)integer;

                case SpanJsonDynamicUtf16Number utf16Number:
                    return (byte)utf16Number;

                case SpanJsonDynamicUtf8Number utf8Number:
                    return (byte)utf8Number;

                case SpanJsonDynamicUtf16String utf16String:
                    return (byte)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (byte)utf8String;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Null:
                    return null;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Number:
                    return jsonNum.GetByte();

                case JsonElement jsonStr when jsonStr.ValueKind == JsonValueKind.String:
                    {
                        var rawSpan = jsonStr.RawSpan;
                        var valueSpan = rawSpan.Slice(1, rawSpan.Length - 2);
                        if (Utf8Parser.TryParse(valueSpan, out byte tmp, out int consumed) && consumed == valueSpan.Length)
                        {
                            return tmp;
                        }
                        throw ThrowHelper2.GetArgumentException_Cast<byte>(value);
                    }

                default:
                    return Convert.ToByte(tokenVal, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Int16"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator short(JToken value)
        {
            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, false))
            {
                ThrowHelper2.ThrowArgumentException_Cast<short>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case BigInteger integer:
                    return (short)integer;

                case SpanJsonDynamicUtf16Number utf16Number:
                    return (short)utf16Number;

                case SpanJsonDynamicUtf8Number utf8Number:
                    return (short)utf8Number;

                case SpanJsonDynamicUtf16String utf16String:
                    return (short)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (short)utf8String;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Number:
                    return jsonNum.GetInt16();

                case JsonElement jsonStr when jsonStr.ValueKind == JsonValueKind.String:
                    {
                        var rawSpan = jsonStr.RawSpan;
                        var valueSpan = rawSpan.Slice(1, rawSpan.Length - 2);
                        if (Utf8Parser.TryParse(valueSpan, out short tmp, out int consumed) && consumed == valueSpan.Length)
                        {
                            return tmp;
                        }
                        throw ThrowHelper2.GetArgumentException_Cast<short>(value);
                    }

                default:
                    return Convert.ToInt16(tokenVal, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Nullable{T}"/> of <see cref="Int16"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator short?(JToken value)
        {
            if (value == null) { return null; }

            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, true))
            {
                ThrowHelper2.ThrowArgumentException_Cast<short>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case null:
                    return null;

                case BigInteger integer:
                    return (short?)integer;

                case SpanJsonDynamicUtf16Number utf16Number:
                    return (short)utf16Number;

                case SpanJsonDynamicUtf8Number utf8Number:
                    return (short)utf8Number;

                case SpanJsonDynamicUtf16String utf16String:
                    return (short)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (short)utf8String;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Null:
                    return null;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Number:
                    return jsonNum.GetInt16();

                case JsonElement jsonStr when jsonStr.ValueKind == JsonValueKind.String:
                    {
                        var rawSpan = jsonStr.RawSpan;
                        var valueSpan = rawSpan.Slice(1, rawSpan.Length - 2);
                        if (Utf8Parser.TryParse(valueSpan, out short tmp, out int consumed) && consumed == valueSpan.Length)
                        {
                            return tmp;
                        }
                        throw ThrowHelper2.GetArgumentException_Cast<short>(value);
                    }

                default:
                    return Convert.ToInt16(tokenVal, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="UInt16"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator ushort(JToken value)
        {
            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, false))
            {
                ThrowHelper2.ThrowArgumentException_Cast<ushort>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case BigInteger integer:
                    return (ushort)integer;

                case SpanJsonDynamicUtf16Number utf16Number:
                    return (ushort)utf16Number;

                case SpanJsonDynamicUtf8Number utf8Number:
                    return (ushort)utf8Number;

                case SpanJsonDynamicUtf16String utf16String:
                    return (ushort)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (ushort)utf8String;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Number:
                    return jsonNum.GetUInt16();

                case JsonElement jsonStr when jsonStr.ValueKind == JsonValueKind.String:
                    {
                        var rawSpan = jsonStr.RawSpan;
                        var valueSpan = rawSpan.Slice(1, rawSpan.Length - 2);
                        if (Utf8Parser.TryParse(valueSpan, out ushort tmp, out int consumed) && consumed == valueSpan.Length)
                        {
                            return tmp;
                        }
                        throw ThrowHelper2.GetArgumentException_Cast<ushort>(value);
                    }

                default:
                    return Convert.ToUInt16(tokenVal, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Nullable{T}"/> of <see cref="UInt16"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator ushort?(JToken value)
        {
            if (value == null) { return null; }

            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, true))
            {
                ThrowHelper2.ThrowArgumentException_Cast<ushort>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case null:
                    return null;

                case BigInteger integer:
                    return (ushort?)integer;

                case SpanJsonDynamicUtf16Number utf16Number:
                    return (ushort)utf16Number;

                case SpanJsonDynamicUtf8Number utf8Number:
                    return (ushort)utf8Number;

                case SpanJsonDynamicUtf16String utf16String:
                    return (ushort)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (ushort)utf8String;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Null:
                    return null;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Number:
                    return jsonNum.GetUInt16();

                case JsonElement jsonStr when jsonStr.ValueKind == JsonValueKind.String:
                    {
                        var rawSpan = jsonStr.RawSpan;
                        var valueSpan = rawSpan.Slice(1, rawSpan.Length - 2);
                        if (Utf8Parser.TryParse(valueSpan, out ushort tmp, out int consumed) && consumed == valueSpan.Length)
                        {
                            return tmp;
                        }
                        throw ThrowHelper2.GetArgumentException_Cast<ushort>(value);
                    }

                default:
                    return Convert.ToUInt16(tokenVal, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Int32"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator int(JToken value)
        {
            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, false))
            {
                ThrowHelper2.ThrowArgumentException_Cast<int>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case BigInteger integer:
                    return (int)integer;

                case SpanJsonDynamicUtf16Number utf16Number:
                    return (int)utf16Number;

                case SpanJsonDynamicUtf8Number utf8Number:
                    return (int)utf8Number;

                case SpanJsonDynamicUtf16String utf16String:
                    return (int)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (int)utf8String;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Number:
                    return jsonNum.GetInt32();

                case JsonElement jsonStr when jsonStr.ValueKind == JsonValueKind.String:
                    {
                        var rawSpan = jsonStr.RawSpan;
                        var valueSpan = rawSpan.Slice(1, rawSpan.Length - 2);
                        if (Utf8Parser.TryParse(valueSpan, out int tmp, out int consumed) && consumed == valueSpan.Length)
                        {
                            return tmp;
                        }
                        throw ThrowHelper2.GetArgumentException_Cast<int>(value);
                    }

                default:
                    return Convert.ToInt32(tokenVal, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Nullable{T}"/> of <see cref="Int32"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator int?(JToken value)
        {
            if (value == null) { return null; }

            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, true))
            {
                ThrowHelper2.ThrowArgumentException_Cast<int>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case null:
                    return null;

                case BigInteger integer:
                    return (int?)integer;

                case SpanJsonDynamicUtf16Number utf16Number:
                    return (int)utf16Number;

                case SpanJsonDynamicUtf8Number utf8Number:
                    return (int)utf8Number;

                case SpanJsonDynamicUtf16String utf16String:
                    return (int)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (int)utf8String;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Null:
                    return null;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Number:
                    return jsonNum.GetInt32();

                case JsonElement jsonStr when jsonStr.ValueKind == JsonValueKind.String:
                    {
                        var rawSpan = jsonStr.RawSpan;
                        var valueSpan = rawSpan.Slice(1, rawSpan.Length - 2);
                        if (Utf8Parser.TryParse(valueSpan, out int tmp, out int consumed) && consumed == valueSpan.Length)
                        {
                            return tmp;
                        }
                        throw ThrowHelper2.GetArgumentException_Cast<int>(value);
                    }

                default:
                    return Convert.ToInt32(tokenVal, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="UInt32"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator uint(JToken value)
        {
            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, false))
            {
                ThrowHelper2.ThrowArgumentException_Cast<uint>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case BigInteger integer:
                    return (uint)integer;

                case SpanJsonDynamicUtf16Number utf16Number:
                    return (uint)utf16Number;

                case SpanJsonDynamicUtf8Number utf8Number:
                    return (uint)utf8Number;

                case SpanJsonDynamicUtf16String utf16String:
                    return (uint)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (uint)utf8String;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Number:
                    return jsonNum.GetUInt32();

                case JsonElement jsonStr when jsonStr.ValueKind == JsonValueKind.String:
                    {
                        var rawSpan = jsonStr.RawSpan;
                        var valueSpan = rawSpan.Slice(1, rawSpan.Length - 2);
                        if (Utf8Parser.TryParse(valueSpan, out uint tmp, out int consumed) && consumed == valueSpan.Length)
                        {
                            return tmp;
                        }
                        throw ThrowHelper2.GetArgumentException_Cast<uint>(value);
                    }

                default:
                    return Convert.ToUInt32(tokenVal, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Nullable{T}"/> of <see cref="UInt32"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator uint?(JToken value)
        {
            if (value == null) { return null; }

            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, true))
            {
                ThrowHelper2.ThrowArgumentException_Cast<uint>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case null:
                    return null;

                case BigInteger integer:
                    return (uint?)integer;

                case SpanJsonDynamicUtf16Number utf16Number:
                    return (uint)utf16Number;

                case SpanJsonDynamicUtf8Number utf8Number:
                    return (uint)utf8Number;

                case SpanJsonDynamicUtf16String utf16String:
                    return (uint)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (uint)utf8String;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Null:
                    return null;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Number:
                    return jsonNum.GetUInt32();

                case JsonElement jsonStr when jsonStr.ValueKind == JsonValueKind.String:
                    {
                        var rawSpan = jsonStr.RawSpan;
                        var valueSpan = rawSpan.Slice(1, rawSpan.Length - 2);
                        if (Utf8Parser.TryParse(valueSpan, out uint tmp, out int consumed) && consumed == valueSpan.Length)
                        {
                            return tmp;
                        }
                        throw ThrowHelper2.GetArgumentException_Cast<uint>(value);
                    }

                default:
                    return Convert.ToUInt32(tokenVal, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Nullable{T}"/> of <see cref="Int64"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator long(JToken value)
        {
            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, false))
            {
                ThrowHelper2.ThrowArgumentException_Cast<long>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case BigInteger integer:
                    return (long)integer;

                case SpanJsonDynamicUtf16Number utf16Number:
                    return (long)utf16Number;

                case SpanJsonDynamicUtf8Number utf8Number:
                    return (long)utf8Number;

                case SpanJsonDynamicUtf16String utf16String:
                    return (long)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (long)utf8String;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Number:
                    return jsonNum.GetInt64();

                case JsonElement jsonStr when jsonStr.ValueKind == JsonValueKind.String:
                    {
                        var rawSpan = jsonStr.RawSpan;
                        var valueSpan = rawSpan.Slice(1, rawSpan.Length - 2);
                        if (Utf8Parser.TryParse(valueSpan, out long tmp, out int consumed) && consumed == valueSpan.Length)
                        {
                            return tmp;
                        }
                        throw ThrowHelper2.GetArgumentException_Cast<long>(value);
                    }

                default:
                    return Convert.ToInt64(tokenVal, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Nullable{T}"/> of <see cref="Int64"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator long?(JToken value)
        {
            if (value == null) { return null; }

            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, true))
            {
                ThrowHelper2.ThrowArgumentException_Cast<long>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case null:
                    return null;

                case BigInteger integer:
                    return (long?)integer;

                case SpanJsonDynamicUtf16Number utf16Number:
                    return (long)utf16Number;

                case SpanJsonDynamicUtf8Number utf8Number:
                    return (long)utf8Number;

                case SpanJsonDynamicUtf16String utf16String:
                    return (long)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (long)utf8String;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Null:
                    return null;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Number:
                    return jsonNum.GetInt64();

                case JsonElement jsonStr when jsonStr.ValueKind == JsonValueKind.String:
                    {
                        var rawSpan = jsonStr.RawSpan;
                        var valueSpan = rawSpan.Slice(1, rawSpan.Length - 2);
                        if (Utf8Parser.TryParse(valueSpan, out long tmp, out int consumed) && consumed == valueSpan.Length)
                        {
                            return tmp;
                        }
                        throw ThrowHelper2.GetArgumentException_Cast<long>(value);
                    }

                default:
                    return Convert.ToInt64(tokenVal, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="System.UInt64"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator ulong(JToken value)
        {
            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, false))
            {
                ThrowHelper2.ThrowArgumentException_Cast<ulong>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case BigInteger integer:
                    return (ulong)integer;

                case SpanJsonDynamicUtf16Number utf16Number:
                    return (ulong)utf16Number;

                case SpanJsonDynamicUtf8Number utf8Number:
                    return (ulong)utf8Number;

                case SpanJsonDynamicUtf16String utf16String:
                    return (ulong)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (ulong)utf8String;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Number:
                    return jsonNum.GetUInt64();

                case JsonElement jsonStr when jsonStr.ValueKind == JsonValueKind.String:
                    {
                        var rawSpan = jsonStr.RawSpan;
                        var valueSpan = rawSpan.Slice(1, rawSpan.Length - 2);
                        if (Utf8Parser.TryParse(valueSpan, out ulong tmp, out int consumed) && consumed == valueSpan.Length)
                        {
                            return tmp;
                        }
                        throw ThrowHelper2.GetArgumentException_Cast<ulong>(value);
                    }

                default:
                    return Convert.ToUInt64(tokenVal, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Nullable{T}"/> of <see cref="UInt64"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator ulong?(JToken value)
        {
            if (value == null) { return null; }

            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, true))
            {
                ThrowHelper2.ThrowArgumentException_Cast<ulong>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case null:
                    return null;

                case BigInteger integer:
                    return (ulong?)integer;

                case SpanJsonDynamicUtf16Number utf16Number:
                    return (ulong)utf16Number;

                case SpanJsonDynamicUtf8Number utf8Number:
                    return (ulong)utf8Number;

                case SpanJsonDynamicUtf16String utf16String:
                    return (ulong)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (ulong)utf8String;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Null:
                    return null;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Number:
                    return jsonNum.GetUInt64();

                case JsonElement jsonStr when jsonStr.ValueKind == JsonValueKind.String:
                    {
                        var rawSpan = jsonStr.RawSpan;
                        var valueSpan = rawSpan.Slice(1, rawSpan.Length - 2);
                        if (Utf8Parser.TryParse(valueSpan, out ulong tmp, out int consumed) && consumed == valueSpan.Length)
                        {
                            return tmp;
                        }
                        throw ThrowHelper2.GetArgumentException_Cast<ulong>(value);
                    }

                default:
                    return Convert.ToUInt64(tokenVal, CultureInfo.InvariantCulture);
            }
        }




        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Decimal"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator decimal(JToken value)
        {
            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, false))
            {
                ThrowHelper2.ThrowArgumentException_Cast<decimal>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case BigInteger integer:
                    return (decimal)integer;

                case SpanJsonDynamicUtf16Number utf16Number:
                    return (decimal)utf16Number;

                case SpanJsonDynamicUtf8Number utf8Number:
                    return (decimal)utf8Number;

                case SpanJsonDynamicUtf16String utf16String:
                    return (decimal)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (decimal)utf8String;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Number:
                    return jsonNum.GetDecimal();

                case JsonElement jsonStr when jsonStr.ValueKind == JsonValueKind.String:
                    {
                        var rawSpan = jsonStr.RawSpan;
                        var valueSpan = rawSpan.Slice(1, rawSpan.Length - 2);
                        if (Utf8Parser.TryParse(valueSpan, out decimal tmp, out int consumed) && consumed == valueSpan.Length)
                        {
                            return tmp;
                        }
                        throw ThrowHelper2.GetArgumentException_Cast<decimal>(value);
                    }

                default:
                    return Convert.ToDecimal(tokenVal, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Nullable{T}"/> of <see cref="Decimal"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator decimal?(JToken value)
        {
            if (value == null) { return null; }

            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, true))
            {
                ThrowHelper2.ThrowArgumentException_Cast<decimal>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case null:
                    return null;

                case BigInteger integer:
                    return (decimal?)integer;

                case SpanJsonDynamicUtf16Number utf16Number:
                    return (decimal)utf16Number;

                case SpanJsonDynamicUtf8Number utf8Number:
                    return (decimal)utf8Number;

                case SpanJsonDynamicUtf16String utf16String:
                    return (decimal)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (decimal)utf8String;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Null:
                    return null;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Number:
                    return jsonNum.GetDecimal();

                case JsonElement jsonStr when jsonStr.ValueKind == JsonValueKind.String:
                    {
                        var rawSpan = jsonStr.RawSpan;
                        var valueSpan = rawSpan.Slice(1, rawSpan.Length - 2);
                        if (Utf8Parser.TryParse(valueSpan, out decimal tmp, out int consumed) && consumed == valueSpan.Length)
                        {
                            return tmp;
                        }
                        throw ThrowHelper2.GetArgumentException_Cast<decimal>(value);
                    }

                default:
                    return Convert.ToDecimal(tokenVal, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Double"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator double(JToken value)
        {
            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, false))
            {
                ThrowHelper2.ThrowArgumentException_Cast<double>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case BigInteger integer:
                    return (double)integer;

                case SpanJsonDynamicUtf16Number utf16Number:
                    return (double)utf16Number;

                case SpanJsonDynamicUtf8Number utf8Number:
                    return (double)utf8Number;

                case SpanJsonDynamicUtf16String utf16String:
                    return (double)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (double)utf8String;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Number:
                    return jsonNum.GetDouble();

                case JsonElement jsonStr when jsonStr.ValueKind == JsonValueKind.String:
                    {
                        var rawSpan = jsonStr.RawSpan;
                        var valueSpan = rawSpan.Slice(1, rawSpan.Length - 2);
                        if (Utf8Parser.TryParse(valueSpan, out double tmp, out int consumed) && consumed == valueSpan.Length)
                        {
                            return tmp;
                        }
                        throw ThrowHelper2.GetArgumentException_Cast<double>(value);
                    }

                default:
                    return Convert.ToDouble(tokenVal, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Nullable{T}"/> of <see cref="Double"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator double?(JToken value)
        {
            if (value == null) { return null; }

            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, true))
            {
                ThrowHelper2.ThrowArgumentException_Cast<double>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case null:
                    return null;

                case BigInteger integer:
                    return (double?)integer;

                case SpanJsonDynamicUtf16Number utf16Number:
                    return (double)utf16Number;

                case SpanJsonDynamicUtf8Number utf8Number:
                    return (double)utf8Number;

                case SpanJsonDynamicUtf16String utf16String:
                    return (double)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (double)utf8String;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Null:
                    return null;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Number:
                    return jsonNum.GetDouble();

                case JsonElement jsonStr when jsonStr.ValueKind == JsonValueKind.String:
                    {
                        var rawSpan = jsonStr.RawSpan;
                        var valueSpan = rawSpan.Slice(1, rawSpan.Length - 2);
                        if (Utf8Parser.TryParse(valueSpan, out double tmp, out int consumed) && consumed == valueSpan.Length)
                        {
                            return tmp;
                        }
                        throw ThrowHelper2.GetArgumentException_Cast<double>(value);
                    }

                default:
                    return Convert.ToDouble(tokenVal, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Single"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator float(JToken value)
        {
            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, false))
            {
                ThrowHelper2.ThrowArgumentException_Cast<float>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case BigInteger integer:
                    return (float)integer;

                case SpanJsonDynamicUtf16Number utf16Number:
                    return (float)utf16Number;

                case SpanJsonDynamicUtf8Number utf8Number:
                    return (float)utf8Number;

                case SpanJsonDynamicUtf16String utf16String:
                    return (float)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (float)utf8String;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Number:
                    return jsonNum.GetSingle();

                case JsonElement jsonStr when jsonStr.ValueKind == JsonValueKind.String:
                    {
                        var rawSpan = jsonStr.RawSpan;
                        var valueSpan = rawSpan.Slice(1, rawSpan.Length - 2);
                        if (Utf8Parser.TryParse(valueSpan, out float tmp, out int consumed) && consumed == valueSpan.Length)
                        {
                            return tmp;
                        }
                        throw ThrowHelper2.GetArgumentException_Cast<float>(value);
                    }

                default:
                    return Convert.ToSingle(tokenVal, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Nullable{T}"/> of <see cref="Single"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator float?(JToken value)
        {
            if (value == null) { return null; }

            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, true))
            {
                ThrowHelper2.ThrowArgumentException_Cast<float>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case null:
                    return null;

                case BigInteger integer:
                    return (float?)integer;

                case SpanJsonDynamicUtf16Number utf16Number:
                    return (float)utf16Number;

                case SpanJsonDynamicUtf8Number utf8Number:
                    return (float)utf8Number;

                case SpanJsonDynamicUtf16String utf16String:
                    return (float)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (float)utf8String;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Null:
                    return null;

                case JsonElement jsonNum when jsonNum.ValueKind == JsonValueKind.Number:
                    return jsonNum.GetSingle();

                case JsonElement jsonStr when jsonStr.ValueKind == JsonValueKind.String:
                    {
                        var rawSpan = jsonStr.RawSpan;
                        var valueSpan = rawSpan.Slice(1, rawSpan.Length - 2);
                        if (Utf8Parser.TryParse(valueSpan, out float tmp, out int consumed) && consumed == valueSpan.Length)
                        {
                            return tmp;
                        }
                        throw ThrowHelper2.GetArgumentException_Cast<float>(value);
                    }

                default:
                    return Convert.ToSingle(tokenVal, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="String"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator string(JToken value)
        {
            if (value == null) { return null; }

            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, StringTypes, true))
            {
                ThrowHelper2.ThrowArgumentException_Cast<string>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case null:
                    return null;

                case byte[] bytes:
                    return Convert.ToBase64String(bytes);

                case BigInteger integer:
                    return integer.ToString(CultureInfo.InvariantCulture);

                case SpanJsonDynamicUtf16Number utf16Number:
                    return utf16Number.ToString();

                case SpanJsonDynamicUtf8Number utf8Number:
                    return utf8Number.ToString();

                case SpanJsonDynamicUtf16String utf16String:
                    return utf16String.ToString();

                case SpanJsonDynamicUtf8String utf8String:
                    return utf8String.ToString();

                case JsonElement jsonNum:
                    return jsonNum.ToString();

                default:
                    return Convert.ToString(tokenVal, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Byte"/>[].</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator byte[](JToken value)
        {
            if (value == null) { return null; }

            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, BytesTypes, false))
            {
                ThrowHelper2.ThrowArgumentException_Cast<byte[]>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case string str:
                    return Convert.FromBase64String(str);

                case BigInteger integer:
                    return integer.ToByteArray();

                case byte[] bytes:
                    return bytes;

                case Guid guid:
                    return guid.ToByteArray();

                case CombGuid comb:
                    return comb.GetByteArray(CombGuidSequentialSegmentType.Guid);

                case SpanJsonDynamicUtf16String utf16String:
                    return JsonSerializer.Generic.Utf16.Deserialize<byte[]>(utf16String.Symbols);

                case SpanJsonDynamicUtf8String utf8String:
                    return JsonSerializer.Generic.Utf8.Deserialize<byte[]>(utf8String.Symbols);

                case JsonElement jsonElement:
                    return jsonElement.GetBytesFromBase64();

                default:
                    throw ThrowHelper2.GetArgumentException_Cast<byte[]>(value);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Guid"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Guid(JToken value)
        {
            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, GuidTypes, false))
            {
                ThrowHelper2.ThrowArgumentException_Cast<Guid>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case string str:
                    return new Guid(str);

                case byte[] bytes:
                    return new Guid(bytes);

                case Guid guid:
                    return guid;

                case CombGuid comb:
                    return comb.Value;

                case SpanJsonDynamicUtf16String utf16String:
                    return (Guid)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (Guid)utf8String;

                case JsonElement jsonElement:
                    return jsonElement.GetGuid();

                default:
                    throw ThrowHelper2.GetArgumentException_Cast<Guid>(value);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Nullable{T}"/> of <see cref="Guid"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Guid?(JToken value)
        {
            if (value == null) { return null; }

            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, GuidTypes, true))
            {
                ThrowHelper2.ThrowArgumentException_Cast<Guid>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case null:
                    return null;

                case string str:
                    return new Guid(str);

                case byte[] bytes:
                    return new Guid(bytes);

                case Guid guid:
                    return guid;

                case CombGuid comb:
                    return comb.Value;

                case SpanJsonDynamicUtf16String utf16String:
                    return (Guid)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (Guid)utf8String;

                case JsonElement jsonElement:
                    return jsonElement.GetGuid();

                default:
                    throw ThrowHelper2.GetArgumentException_Cast<Guid>(value);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Guid"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator CombGuid(JToken value)
        {
            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, CombGuidTypes, false))
            {
                ThrowHelper2.ThrowArgumentException_Cast<CombGuid>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case string str:
                    return new CombGuid(str, CombGuidSequentialSegmentType.Comb);

                case byte[] bytes:
                    return new CombGuid(bytes, CombGuidSequentialSegmentType.Guid);

                case Guid guid:
                    return guid;

                case CombGuid comb:
                    return comb;

                case SpanJsonDynamicUtf16String utf16String:
                    return (CombGuid)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (CombGuid)utf8String;

                case JsonElement jsonElement:
                    return jsonElement.GetCombGuid();

                default:
                    throw ThrowHelper2.GetArgumentException_Cast<CombGuid>(value);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Nullable{T}"/> of <see cref="Guid"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator CombGuid?(JToken value)
        {
            if (value == null) { return null; }

            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, CombGuidTypes, true))
            {
                ThrowHelper2.ThrowArgumentException_Cast<CombGuid>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case null:
                    return null;

                case string str:
                    return new CombGuid(str, CombGuidSequentialSegmentType.Comb);

                case byte[] bytes:
                    return new CombGuid(bytes, CombGuidSequentialSegmentType.Guid);

                case Guid guid:
                    return guid;

                case CombGuid comb:
                    return comb;

                case SpanJsonDynamicUtf16String utf16String:
                    return (CombGuid)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (CombGuid)utf8String;

                case JsonElement jsonElement:
                    return jsonElement.GetCombGuid();

                default:
                    throw ThrowHelper2.GetArgumentException_Cast<CombGuid>(value);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Nullable{T}"/> of <see cref="DateTime"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator DateTime(JToken value)
        {
            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, DateTimeTypes, false))
            {
                ThrowHelper2.ThrowArgumentException_Cast<DateTime>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case DateTime dt:
                    return dt;

                case DateTimeOffset offset:
                    return offset.DateTime;

                case SpanJsonDynamicUtf16String utf16String:
                    return (DateTime)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (DateTime)utf8String;

                case JsonElement jsonElement:
                    return jsonElement.GetDateTime();

                default:
                    return Convert.ToDateTime(tokenVal, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Nullable{T}"/> of <see cref="DateTime"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator DateTime?(JToken value)
        {
            if (value == null) { return null; }

            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, DateTimeTypes, true))
            {
                ThrowHelper2.ThrowArgumentException_Cast<DateTime>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case null:
                    return null;

                case DateTime dt:
                    return dt;

                case DateTimeOffset offset:
                    return offset.DateTime;

                case SpanJsonDynamicUtf16String utf16String:
                    return (DateTime)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (DateTime)utf8String;

                case JsonElement jsonElement:
                    return jsonElement.GetDateTime();

                default:
                    return Convert.ToDateTime(tokenVal, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="System.DateTimeOffset"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator DateTimeOffset(JToken value)
        {
            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, DateTimeTypes, false))
            {
                ThrowHelper2.ThrowArgumentException_Cast<DateTimeOffset>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case DateTimeOffset offset:
                    return offset;

                case string s:
                    return DateTimeOffset.Parse(s, CultureInfo.InvariantCulture);

                case SpanJsonDynamicUtf16String utf16String:
                    return (DateTimeOffset)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (DateTimeOffset)utf8String;

                case JsonElement jsonElement:
                    return jsonElement.GetDateTimeOffset();

                default:
                    return new DateTimeOffset(Convert.ToDateTime(tokenVal, CultureInfo.InvariantCulture));
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Nullable{T}"/> of <see cref="DateTimeOffset"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator DateTimeOffset?(JToken value)
        {
            if (value == null) { return null; }

            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, DateTimeTypes, true))
            {
                ThrowHelper2.ThrowArgumentException_Cast<DateTimeOffset>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case null:
                    return null;

                case DateTimeOffset offset:
                    return offset;

                case string s:
                    return DateTimeOffset.Parse(s, CultureInfo.InvariantCulture);

                case SpanJsonDynamicUtf16String utf16String:
                    return (DateTimeOffset)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (DateTimeOffset)utf8String;

                case JsonElement jsonElement:
                    return jsonElement.GetDateTimeOffset();

                default:
                    return new DateTimeOffset(Convert.ToDateTime(tokenVal, CultureInfo.InvariantCulture));
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="TimeSpan"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator TimeSpan(JToken value)
        {
            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, TimeSpanTypes, false))
            {
                ThrowHelper2.ThrowArgumentException_Cast<TimeSpan>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case TimeSpan span:
                    return span;

                case SpanJsonDynamicUtf16String utf16String:
                    return (TimeSpan)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (TimeSpan)utf8String;

                case JsonElement element when element.ValueKind == JsonValueKind.String:
                    return ConvertUtils.ParseTimeSpan(element.GetString());

                default:
                    return ConvertUtils.ParseTimeSpan(Convert.ToString(tokenVal, CultureInfo.InvariantCulture));
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Nullable{T}"/> of <see cref="TimeSpan"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator TimeSpan?(JToken value)
        {
            if (value == null) { return null; }

            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, TimeSpanTypes, true))
            {
                ThrowHelper2.ThrowArgumentException_Cast<TimeSpan>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case null:
                    return null;

                case TimeSpan span:
                    return span;

                case SpanJsonDynamicUtf16String utf16String:
                    return (TimeSpan)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (TimeSpan)utf8String;

                case JsonElement element when element.ValueKind == JsonValueKind.String:
                    return ConvertUtils.ParseTimeSpan(element.GetString());

                default:
                    return ConvertUtils.ParseTimeSpan(Convert.ToString(tokenVal, CultureInfo.InvariantCulture));
            }
        }

        /// <summary>Performs an explicit conversion from <see cref="JToken"/> to <see cref="Uri"/>.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Uri(JToken value)
        {
            if (value == null) { return null; }

            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, UriTypes, true))
            {
                ThrowHelper2.ThrowArgumentException_Cast<Uri>(value);
            }

            var tokenVal = v.Value;
            switch (tokenVal)
            {
                case null:
                    return null;

                case Uri uri:
                    return uri;

                case SpanJsonDynamicUtf16String utf16String:
                    return (Uri)utf16String;

                case SpanJsonDynamicUtf8String utf8String:
                    return (Uri)utf8String;

                case JsonElement element when element.ValueKind == JsonValueKind.String:
                    return new Uri(element.GetString(), UriKind.RelativeOrAbsolute);

                default:
                    return new Uri(Convert.ToString(tokenVal, CultureInfo.InvariantCulture), UriKind.RelativeOrAbsolute);
            }
        }

        private static BigInteger ToBigInteger(JToken value)
        {
            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, BigIntegerTypes, false))
            {
                ThrowHelper2.ThrowArgumentException_Cast<BigInteger>(value);
            }

            return ConvertUtils.ToBigInteger(v.Value);
        }

        private static BigInteger? ToBigIntegerNullable(JToken value)
        {
            JValue v = EnsureValue(value);
            if (v == null || !ValidateToken(v, BigIntegerTypes, true))
            {
                ThrowHelper2.ThrowArgumentException_Cast<BigInteger>(value);
            }

            if (v.Value == null)
            {
                return null;
            }

            return ConvertUtils.ToBigInteger(v.Value);
        }
    }
}