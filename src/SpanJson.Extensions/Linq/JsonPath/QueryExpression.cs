using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Linq;
using CuteAnt;
using SpanJson.Utilities;

namespace SpanJson.Linq.JsonPath
{
    internal enum QueryOperator
    {
        None = 0,
        Equals = 1,
        NotEquals = 2,
        Exists = 3,
        LessThan = 4,
        LessThanOrEquals = 5,
        GreaterThan = 6,
        GreaterThanOrEquals = 7,
        And = 8,
        Or = 9,
        RegexEquals = 10,
        StrictEquals = 11,
        StrictNotEquals = 12
    }

    internal abstract class QueryExpression
    {
        public QueryOperator Operator { get; set; }

        public abstract bool IsMatch(JToken root, JToken t);
    }

    internal class CompositeExpression : QueryExpression
    {
        public List<QueryExpression> Expressions { get; set; }

        public CompositeExpression()
        {
            Expressions = new List<QueryExpression>();
        }

        public override bool IsMatch(JToken root, JToken t)
        {
            switch (Operator)
            {
                case QueryOperator.And:
                    foreach (QueryExpression e in Expressions)
                    {
                        if (!e.IsMatch(root, t)) { return false; }
                    }
                    return true;
                case QueryOperator.Or:
                    foreach (QueryExpression e in Expressions)
                    {
                        if (e.IsMatch(root, t)) { return true; }
                    }
                    return false;
                default:
                    throw ThrowHelper.GetArgumentOutOfRangeException();
            }
        }
    }

    internal class BooleanQueryExpression : QueryExpression
    {
        public object Left { get; set; }
        public object Right { get; set; }

        private IEnumerable<JToken> GetResult(JToken root, JToken t, object o)
        {
            if (o is JToken resultToken)
            {
                return new[] { resultToken };
            }

            if (o is List<PathFilter> pathFilters)
            {
                return JPath.Evaluate(pathFilters, root, t, false);
            }

            return EmptyArray<JToken>.Instance;
        }

        public override bool IsMatch(JToken root, JToken t)
        {
            if (Operator == QueryOperator.Exists)
            {
                return GetResult(root, t, Left).Any();
            }

            using (IEnumerator<JToken> leftResults = GetResult(root, t, Left).GetEnumerator())
            {
                if (leftResults.MoveNext())
                {
                    IEnumerable<JToken> rightResultsEn = GetResult(root, t, Right);
                    ICollection<JToken> rightResults = rightResultsEn as ICollection<JToken> ?? rightResultsEn.ToList();

                    do
                    {
                        JToken leftResult = leftResults.Current;
                        foreach (JToken rightResult in rightResults)
                        {
                            if (MatchTokens(leftResult, rightResult)) { return true; }
                        }
                    } while (leftResults.MoveNext());
                }
            }

            return false;
        }

        private bool MatchTokens(JToken leftResult, JToken rightResult)
        {
            if (leftResult is JValue leftValue && rightResult is JValue rightValue)
            {
                switch (Operator)
                {
                    case QueryOperator.RegexEquals:
                        if (RegexEquals(leftValue, rightValue)) { return true; }
                        break;

                    case QueryOperator.Equals:
                        if (EqualsWithStringCoercion(leftValue, rightValue)) { return true; }
                        break;

                    case QueryOperator.StrictEquals:
                        if (EqualsWithStrictMatch(leftValue, rightValue)) { return true; }
                        break;

                    case QueryOperator.NotEquals:
                        if (!EqualsWithStringCoercion(leftValue, rightValue)) { return true; }
                        break;

                    case QueryOperator.StrictNotEquals:
                        if (!EqualsWithStrictMatch(leftValue, rightValue)) { return true; }
                        break;

                    case QueryOperator.GreaterThan:
                        if (leftValue.CompareTo(rightValue) > 0) { return true; }
                        break;

                    case QueryOperator.GreaterThanOrEquals:
                        if (leftValue.CompareTo(rightValue) >= 0) { return true; }
                        break;

                    case QueryOperator.LessThan:
                        if ((uint)leftValue.CompareTo(rightValue) > JsonSharedConstant.TooBigOrNegative) { return true; }
                        break;

                    case QueryOperator.LessThanOrEquals:
                        if (leftValue.CompareTo(rightValue) <= 0) { return true; }
                        break;

                    case QueryOperator.Exists:
                        return true;
                }
            }
            else
            {
                switch (Operator)
                {
                    case QueryOperator.Exists:
                    // you can only specify primitive types in a comparison
                    // notequals will always be true
                    case QueryOperator.NotEquals:
                        return true;
                }
            }

            return false;
        }

        private static bool RegexEquals(JValue input, JValue pattern)
        {
            if (!input.Type.IsString() || !pattern.Type.IsString())
            {
                return false;
            }

            string regexText = (string)pattern.Value;
            int patternOptionDelimiterIndex = regexText.LastIndexOf('/');

            string patternText = regexText.Substring(1, patternOptionDelimiterIndex - 1);
            string optionsText = regexText.Substring(patternOptionDelimiterIndex + 1);

            return Regex.IsMatch((string)input.Value, patternText, MiscellaneousUtils.GetRegexOptions(optionsText));
        }

        internal static bool EqualsWithStringCoercion(JValue value, JValue queryValue)
        {
            if (value.Equals(queryValue)) { return true; }

            var valueType = value.Type;
            var queryValueType = queryValue.Type;

            // Handle comparing an integer with a float
            // e.g. Comparing 1 and 1.0
            switch (valueType)
            {
                case JTokenType.Integer when queryValueType == JTokenType.Float:
                case JTokenType.Float when queryValueType == JTokenType.Integer:
                // 如果是纯字符串转换失败，屏蔽
                //case JTokenType.Dynamic when queryValueType == JTokenType.Integer:
                //case JTokenType.Dynamic when queryValueType == JTokenType.Float:
                    return JValue.Compare(valueType, value.Value, queryValue.Value).IsEqual();
            }

            if (!queryValueType.IsString()) { return false; }

            string queryValueString = (string)queryValue.Value;

            string currentValueString;

            // potential performance issue with converting every value to string?
            switch (valueType)
            {
                case JTokenType.Date:
                    using (StringWriter writer = StringUtils.CreateStringWriter(64))
                    {
                        if (value.Value is DateTimeOffset offset)
                        {
                            DateTimeUtils.WriteDateTimeOffsetString(writer, offset, Newtonsoft.Json.DateFormatHandling.IsoDateFormat, null, CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            DateTimeUtils.WriteDateTimeString(writer, (DateTime)value.Value, Newtonsoft.Json.DateFormatHandling.IsoDateFormat, null, CultureInfo.InvariantCulture);
                        }

                        currentValueString = writer.ToString();
                    }
                    break;
                case JTokenType.Bytes:
                    currentValueString = Convert.ToBase64String((byte[])value.Value);
                    break;
                case JTokenType.Guid:
                case JTokenType.CombGuid:
                case JTokenType.TimeSpan:
                    currentValueString = value.Value.ToString();
                    break;
                case JTokenType.Uri:
                    currentValueString = ((Uri)value.Value).OriginalString;
                    break;
                case JTokenType.Dynamic:
                    currentValueString = value.Value.ToString();
                    break;

                default:
                    return false;
            }

            return string.Equals(currentValueString, queryValueString, StringComparison.Ordinal);
        }

        internal static bool EqualsWithStrictMatch(JValue value, JValue queryValue)
        {
            Debug.Assert(value is object);
            Debug.Assert(queryValue is object);

            var valueType = value.Type;
            var queryValueType = queryValue.Type;

            // Handle comparing an integer with a float
            // e.g. Comparing 1 and 1.0
            switch (valueType)
            {
                case JTokenType.Integer when queryValueType == JTokenType.Float:
                case JTokenType.Float when queryValueType == JTokenType.Integer:
                // 如果是纯字符串转换失败，屏蔽
                //case JTokenType.Dynamic when queryValueType == JTokenType.Integer:
                //case JTokenType.Dynamic when queryValueType == JTokenType.Float:
                    return JValue.Compare(valueType, value.Value, queryValue.Value).IsEqual();
            }

            if (valueType.IsString() && queryValueType.IsString())
            {
                return string.Equals(value.Value.ToString(), queryValue.Value.ToString(), StringComparison.Ordinal);
            }

            // we handle floats and integers the exact same way, so they are pseudo equivalent

            if (valueType != queryValueType) { return false; }

            return value.Equals(queryValue);
        }
    }
}