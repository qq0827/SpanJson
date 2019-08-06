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
using System.Collections.Concurrent;
using System.Numerics;
using CuteAnt;
using SpanJson.Document;
using SpanJson.Dynamic;

namespace SpanJson.Linq
{
    partial class JValue
    {
        public static readonly ConcurrentDictionary<Type, JTokenType> CustomPrimitiveTypes = new ConcurrentDictionary<Type, JTokenType>();

        private JTokenType _valueType;
        private object _value;

        #region @@ Constructors @@

        internal JValue(object value, JTokenType type)
        {
            _value = value;
            _valueType = type;
        }

        /// <summary>Initializes a new instance of the <see cref="JValue"/> class from another <see cref="JValue"/> object.</summary>
        /// <param name="other">A <see cref="JValue"/> object to copy from.</param>
        public JValue(JValue other) : this(other.Value, other.Type) { }

        /// <summary>Initializes a new instance of the <see cref="JValue"/> class with the given value.</summary>
        /// <param name="value">The value.</param>
        public JValue(long value) : this(value, JTokenType.Integer) { }

        /// <summary>Initializes a new instance of the <see cref="JValue"/> class with the given value.</summary>
        /// <param name="value">The value.</param>
        public JValue(decimal value) : this(value, JTokenType.Float) { }

        /// <summary>Initializes a new instance of the <see cref="JValue"/> class with the given value.</summary>
        /// <param name="value">The value.</param>
        public JValue(char value) : this(value, JTokenType.String) { }

        /// <summary>Initializes a new instance of the <see cref="JValue"/> class with the given value.</summary>
        /// <param name="value">The value.</param>
        public JValue(ulong value) : this(value, JTokenType.Integer) { }

        /// <summary>Initializes a new instance of the <see cref="JValue"/> class with the given value.</summary>
        /// <param name="value">The value.</param>
        public JValue(double value) : this(value, JTokenType.Float) { }

        /// <summary>Initializes a new instance of the <see cref="JValue"/> class with the given value.</summary>
        /// <param name="value">The value.</param>
        public JValue(float value) : this(value, JTokenType.Float) { }

        /// <summary>Initializes a new instance of the <see cref="JValue"/> class with the given value.</summary>
        /// <param name="value">The value.</param>
        public JValue(DateTime value) : this(value, JTokenType.Date) { }

        /// <summary>Initializes a new instance of the <see cref="JValue"/> class with the given value.</summary>
        /// <param name="value">The value.</param>
        public JValue(DateTimeOffset value) : this(value, JTokenType.Date) { }

        /// <summary>Initializes a new instance of the <see cref="JValue"/> class with the given value.</summary>
        /// <param name="value">The value.</param>
        public JValue(bool value) : this(value, JTokenType.Boolean) { }

        /// <summary>Initializes a new instance of the <see cref="JValue"/> class with the given value.</summary>
        /// <param name="value">The value.</param>
        public JValue(string value) : this(value, JTokenType.String) { }

        /// <summary>Initializes a new instance of the <see cref="JValue"/> class with the given value.</summary>
        /// <param name="value">The value.</param>
        public JValue(Guid value) : this(value, JTokenType.Guid) { }

        /// <summary>Initializes a new instance of the <see cref="JValue"/> class with the given value.</summary>
        /// <param name="value">The value.</param>
        public JValue(CombGuid value) : this(value, JTokenType.CombGuid) { }

        /// <summary>Initializes a new instance of the <see cref="JValue"/> class with the given value.</summary>
        /// <param name="value">The value.</param>
        public JValue(Uri value) : this(value, (value != null) ? JTokenType.Uri : JTokenType.Null) { }

        /// <summary>Initializes a new instance of the <see cref="JValue"/> class with the given value.</summary>
        /// <param name="value">The value.</param>
        public JValue(TimeSpan value) : this(value, JTokenType.TimeSpan) { }

        /// <summary>Initializes a new instance of the <see cref="JValue"/> class with the given value.</summary>
        /// <param name="value">The value.</param>
        public JValue(SpanJsonDynamicUtf8Number value) : this(value, value.IsFloat ? JTokenType.Float : JTokenType.Integer) { }

        /// <summary>Initializes a new instance of the <see cref="JValue"/> class with the given value.</summary>
        /// <param name="value">The value.</param>
        public JValue(SpanJsonDynamicUtf8String value) : this(value, JTokenType.Dynamic) { }

        /// <summary>Initializes a new instance of the <see cref="JValue"/> class with the given value.</summary>
        /// <param name="value">The value.</param>
        public JValue(SpanJsonDynamicUtf16Number value) : this(value, value.IsFloat ? JTokenType.Float : JTokenType.Integer) { }

        /// <summary>Initializes a new instance of the <see cref="JValue"/> class with the given value.</summary>
        /// <param name="value">The value.</param>
        public JValue(SpanJsonDynamicUtf16String value) : this(value, JTokenType.Dynamic) { }

        /// <summary>Initializes a new instance of the <see cref="JValue"/> class with the given value.</summary>
        /// <param name="value">The value.</param>
        public JValue(in JsonElement value)
        {
            switch (value.ValueKind)
            {
                case JsonValueKind.String:
                    _value = value;
                    _valueType = JTokenType.Dynamic;
                    break;
                case JsonValueKind.Number:
                    _value = value;
                    var isFloat = value.RawSpan.IndexOf(JsonUtf8Constant.Period) != -1;
                    _valueType = isFloat ? JTokenType.Float : JTokenType.Integer;
                    break;
                case JsonValueKind.True:
                    _value = true;
                    _valueType = JTokenType.Boolean;
                    break;
                case JsonValueKind.False:
                    _value = false;
                    _valueType = JTokenType.Boolean;
                    break;
                case JsonValueKind.Null:
                    _value = null;
                    _valueType = JTokenType.Null;
                    break;
                case JsonValueKind.Object:
                case JsonValueKind.Array:
                case JsonValueKind.Undefined:
                default:
                    throw ThrowHelper2.GetArgumentException_Could_not_determine_JSON_object_type_for_type_JsonElement();
            }
        }

        /// <summary>Initializes a new instance of the <see cref="JValue"/> class with the given value.</summary>
        /// <param name="value">The value.</param>
        public JValue(object value)
        {
            _valueType = GetValueType(_valueType, value, out _value);
        }

        #endregion

        /// <summary>Gets a value indicating whether this token has child tokens.</summary>
        /// <value><c>true</c> if this token has child values; otherwise, <c>false</c>.</value>
        public override bool HasValues => false;

        /// <summary>Gets the node type for this <see cref="JToken"/>.</summary>
        /// <value>The type.</value>
        public override JTokenType Type => _valueType;

        /// <summary>Gets or sets the underlying token value.</summary>
        /// <value>The underlying token value.</value>
        public object Value
        {
            get => _value;
            set
            {
                Type currentType = _value?.GetType();
                Type newType = value?.GetType();

                if (currentType != newType)
                {
                    _valueType = GetValueType(_valueType, value, out _value);
                }
                else
                {
                    _value = value;
                }
            }
        }

        private static JTokenType GetValueType(JTokenType? current, object value, out object v)
        {
            v = value;
            switch (value)
            {
                case null:
                case DBNull _:
                    return JTokenType.Null;

                case string _:
                    return GetStringValueType(current);

                case sbyte _:
                case byte _:
                case short _:
                case ushort _:
                case int _:
                case uint _:
                case long _:
                case ulong _:
                case Enum _:
                case BigInteger _:
                    return JTokenType.Integer;

                case float _:
                case double _:
                case decimal _:
                    return JTokenType.Float;

                case DateTime _:
                case DateTimeOffset _:
                    return JTokenType.Date;

                case byte[] _:
                    return JTokenType.Bytes;

                case bool _:
                    return JTokenType.Boolean;

                case Guid _:
                    return JTokenType.Guid;

                case CombGuid _:
                    return JTokenType.CombGuid;

                case Uri _:
                    return JTokenType.Uri;

                case TimeSpan _:
                    return JTokenType.TimeSpan;

                case SpanJsonDynamicUtf16Number utf16Number:
                    return utf16Number.IsFloat ? JTokenType.Float : JTokenType.Integer;
                case SpanJsonDynamicUtf8Number utf8Number:
                    return utf8Number.IsFloat ? JTokenType.Float : JTokenType.Integer;

                case SpanJsonDynamicUtf16String _:
                case SpanJsonDynamicUtf8String _:
                    return JTokenType.Dynamic;

                case JsonElement jsonElement:
                    return LocalReadJsonElement(jsonElement, out v);

                default:
                    if (CustomPrimitiveTypes.TryGetValue(value.GetType(), out JTokenType tokenType))
                    {
                        return tokenType;
                    }
                    else
                    {
                        throw ThrowHelper2.GetArgumentException_Could_not_determine_JSON_object_type_for_type(value);
                    }
            }

            static JTokenType LocalReadJsonElement(JsonElement element, out object v)
            {
                switch (element.ValueKind)
                {
                    case JsonValueKind.String:
                        v = element;
                        return JTokenType.Dynamic;

                    case JsonValueKind.Number:
                        v = element;
                        return JTokenType.Integer;

                    case JsonValueKind.True:
                        v = true;
                        return JTokenType.Boolean;

                    case JsonValueKind.False:
                        v = false;
                        return JTokenType.Boolean;

                    case JsonValueKind.Null:
                        v = null;
                        return JTokenType.Null;

                    case JsonValueKind.Object:
                    case JsonValueKind.Array:
                    case JsonValueKind.Undefined:
                    default:
                        throw ThrowHelper.GetNotSupportedException();
                }
            }
        }

        private static JTokenType GetStringValueType(JTokenType? current)
        {
            if (current == null)
            {
                return JTokenType.String;
            }

            switch (current.GetValueOrDefault())
            {
                case JTokenType.Comment:
                case JTokenType.String:
                case JTokenType.Raw:
                    return current.GetValueOrDefault();

                default:
                    return JTokenType.String;
            }
        }
    }
}