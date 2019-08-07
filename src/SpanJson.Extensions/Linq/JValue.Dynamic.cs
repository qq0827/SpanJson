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
using System.Dynamic;
using System.Globalization;
using System.Linq.Expressions;
using System.Numerics;
using SpanJson.Utilities;

namespace SpanJson.Linq
{
    /// <summary>Represents a value in JSON (string, integer, date, etc).</summary>
    public partial class JValue : JToken, IEquatable<JValue>, IFormattable, IComparable, IComparable<JValue>, IConvertible
    {
        /// <summary>Returns the <see cref="DynamicMetaObject"/> responsible for binding operations performed on this object.</summary>
        /// <param name="parameter">The expression tree representation of the runtime value.</param>
        /// <returns>The <see cref="DynamicMetaObject"/> to bind this object.</returns>
        protected override DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new DynamicProxyMetaObject<JValue>(parameter, this, new JValueDynamicProxy());
        }

        private static bool Operation(ExpressionType operation, object objA, object objB, out object result)
        {
            if (objA is string || objB is string)
            {
                if (operation == ExpressionType.Add || operation == ExpressionType.AddAssign)
                {
                    result = objA?.ToString() + objB?.ToString();
                    return true;
                }
            }

            if (objA is BigInteger || objB is BigInteger)
            {
                if (objA == null || objB == null)
                {
                    result = null;
                    return true;
                }

                // not that this will lose the fraction
                // BigInteger doesn't have operators with non-integer types
                BigInteger i1 = ConvertUtils.ToBigInteger(objA);
                BigInteger i2 = ConvertUtils.ToBigInteger(objB);

                switch (operation)
                {
                    case ExpressionType.Add:
                    case ExpressionType.AddAssign:
                        result = i1 + i2;
                        return true;

                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractAssign:
                        result = i1 - i2;
                        return true;

                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyAssign:
                        result = i1 * i2;
                        return true;

                    case ExpressionType.Divide:
                    case ExpressionType.DivideAssign:
                        result = i1 / i2;
                        return true;
                }
            }
            else if (objA is ulong || objB is ulong || objA is decimal || objB is decimal)
            {
                if (objA == null || objB == null)
                {
                    result = null;
                    return true;
                }

                decimal d1 = ConvertUtils.ConvertOrCast<decimal>(objA, CultureInfo.InvariantCulture);
                decimal d2 = ConvertUtils.ConvertOrCast<decimal>(objB, CultureInfo.InvariantCulture);

                switch (operation)
                {
                    case ExpressionType.Add:
                    case ExpressionType.AddAssign:
                        result = d1 + d2;
                        return true;

                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractAssign:
                        result = d1 - d2;
                        return true;

                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyAssign:
                        result = d1 * d2;
                        return true;

                    case ExpressionType.Divide:
                    case ExpressionType.DivideAssign:
                        result = d1 / d2;
                        return true;
                }
            }
            else if (objA is float || objB is float || objA is double || objB is double)
            {
                if (objA == null || objB == null)
                {
                    result = null;
                    return true;
                }

                double d1 = ConvertUtils.ConvertOrCast<double>(objA, CultureInfo.InvariantCulture);
                double d2 = ConvertUtils.ConvertOrCast<double>(objB, CultureInfo.InvariantCulture);

                switch (operation)
                {
                    case ExpressionType.Add:
                    case ExpressionType.AddAssign:
                        result = d1 + d2;
                        return true;

                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractAssign:
                        result = d1 - d2;
                        return true;

                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyAssign:
                        result = d1 * d2;
                        return true;

                    case ExpressionType.Divide:
                    case ExpressionType.DivideAssign:
                        result = d1 / d2;
                        return true;
                }
            }
            else if (objA is int || objA is uint || objA is long || objA is short || objA is ushort || objA is sbyte || objA is byte ||
                     objB is int || objB is uint || objB is long || objB is short || objB is ushort || objB is sbyte || objB is byte)
            {
                if (objA == null || objB == null)
                {
                    result = null;
                    return true;
                }

                long l1 = ConvertUtils.ConvertOrCast<long>(objA, CultureInfo.InvariantCulture);
                long l2 = ConvertUtils.ConvertOrCast<long>(objB, CultureInfo.InvariantCulture);

                switch (operation)
                {
                    case ExpressionType.Add:
                    case ExpressionType.AddAssign:
                        result = l1 + l2;
                        return true;

                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractAssign:
                        result = l1 - l2;
                        return true;

                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyAssign:
                        result = l1 * l2;
                        return true;

                    case ExpressionType.Divide:
                    case ExpressionType.DivideAssign:
                        result = l1 / l2;
                        return true;
                }
            }

            result = null;
            return false;
        }

        private class JValueDynamicProxy : DynamicProxy<JValue>
        {
            public override bool TryConvert(JValue instance, ConvertBinder binder, out object result)
            {
                if (binder.Type == typeof(JValue) || binder.Type == typeof(JToken))
                {
                    result = instance;
                    return true;
                }

                object value = instance.Value;

                if (value == null)
                {
                    result = null;
                    return ReflectionUtils.IsNullable(binder.Type);
                }

                result = ConvertUtils.Convert(value, CultureInfo.InvariantCulture, binder.Type);
                return true;
            }

            public override bool TryBinaryOperation(JValue instance, BinaryOperationBinder binder, object arg, out object result)
            {
                object compareValue = arg is JValue value ? value.Value : arg;

                switch (binder.Operation)
                {
                    case ExpressionType.Equal:
                        result = Compare(instance.Type, instance.Value, compareValue).IsEqual();
                        return true;

                    case ExpressionType.NotEqual:
                        result = Compare(instance.Type, instance.Value, compareValue).IsNotEqual();
                        return true;

                    case ExpressionType.GreaterThan:
                        result = Compare(instance.Type, instance.Value, compareValue).IsGreaterThan();
                        return true;

                    case ExpressionType.GreaterThanOrEqual:
                        result = Compare(instance.Type, instance.Value, compareValue).IsGreaterThanOrEqual();
                        return true;

                    case ExpressionType.LessThan:
                        result = Compare(instance.Type, instance.Value, compareValue).IsLessThan();
                        return true;

                    case ExpressionType.LessThanOrEqual:
                        result = Compare(instance.Type, instance.Value, compareValue).IsLessThanOrEqual();
                        return true;

                    case ExpressionType.Add:
                    case ExpressionType.AddAssign:
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractAssign:
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyAssign:
                    case ExpressionType.Divide:
                    case ExpressionType.DivideAssign:
                        if (Operation(binder.Operation, instance.Value, compareValue, out result))
                        {
                            result = new JValue(result);
                            return true;
                        }
                        break;
                }

                result = null;
                return false;
            }
        }
    }
}