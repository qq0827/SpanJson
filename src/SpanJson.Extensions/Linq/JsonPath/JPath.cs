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
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace SpanJson.Linq.JsonPath
{
    internal ref struct JPath
    {
        private static readonly char[] FloatCharacters = new[] { '.', 'E', 'e' };

        private ReadOnlySpan<char> _expression;
        private readonly uint _expressionLength;

        public List<PathFilter> Filters { get; }

        private int _currentIndex;

        public JPath(string expression)
        {
            if (null == expression) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.expression); }

            _expression = expression.AsSpan();
            _expressionLength = (uint)_expression.Length;
            Filters = new List<PathFilter>();

            _currentIndex = 0;

            ParseMain();
        }

        private void ParseMain()
        {
            int currentPartStartIndex = _currentIndex;

            EatWhitespace();

            if (_expressionLength == (uint)_currentIndex)
            {
                return;
            }

            if (_expression[_currentIndex] == '$')
            {
                if (_expressionLength == 1u)
                {
                    return;
                }

                // only increment position for "$." or "$["
                // otherwise assume property that starts with $
                char c = _expression[_currentIndex + 1];
                if (c == '.' || c == '[')
                {
                    _currentIndex++;
                    currentPartStartIndex = _currentIndex;
                }
            }

            if (!ParsePath(Filters, currentPartStartIndex, false))
            {
                int lastCharacterIndex = _currentIndex;

                EatWhitespace();

                if ((uint)_currentIndex < _expressionLength)
                {
                    ThrowHelper2.ThrowJsonException_Unexpected_character_while_parsing_path(_expression, lastCharacterIndex);
                }
            }
        }

        private bool ParsePath(List<PathFilter> filters, int currentPartStartIndex, bool query)
        {
            bool scan = false;
            bool followingIndexer = false;
            bool followingDot = false;

            bool ended = false;
            while ((uint)_currentIndex < _expressionLength && !ended)
            {
                char currentChar = _expression[_currentIndex];

                switch (currentChar)
                {
                    case '[':
                    case '(':
                        if (_currentIndex > currentPartStartIndex)
                        {
                            var member = _expression.Slice(currentPartStartIndex, _currentIndex - currentPartStartIndex);
                            if (member.Length == 1 && member[0] == '*')
                            {
                                member = default;
                            }

                            filters.Add(CreatePathFilter(member, scan));
                            scan = false;
                        }

                        filters.Add(ParseIndexer(currentChar, scan));
                        _currentIndex++;
                        currentPartStartIndex = _currentIndex;
                        followingIndexer = true;
                        followingDot = false;
                        break;
                    case ']':
                    case ')':
                        ended = true;
                        break;
                    case ' ':
                        if ((uint)_currentIndex < _expressionLength)
                        {
                            ended = true;
                        }
                        break;
                    case '.':
                        if (_currentIndex > currentPartStartIndex)
                        {
                            var member = _expression.Slice(currentPartStartIndex, _currentIndex - currentPartStartIndex);
                            if (member.Length == 1 && member[0] == '*')
                            {
                                member = default;
                            }

                            filters.Add(CreatePathFilter(member, scan));
                            scan = false;
                        }
                        var nextIndex = _currentIndex + 1;
                        if ((uint)nextIndex < _expressionLength && _expression[nextIndex] == '.')
                        {
                            scan = true;
                            _currentIndex++;
                        }
                        _currentIndex++;
                        currentPartStartIndex = _currentIndex;
                        followingIndexer = false;
                        followingDot = true;
                        break;
                    default:
                        if (query && (currentChar == '=' || currentChar == '<' || currentChar == '!' || currentChar == '>' || currentChar == '|' || currentChar == '&'))
                        {
                            ended = true;
                        }
                        else
                        {
                            if (followingIndexer)
                            {
                                ThrowHelper2.ThrowJsonException_Unexpected_character_following_indexer(currentChar);
                            }

                            _currentIndex++;
                        }
                        break;
                }
            }

            bool atPathEnd = ((uint)_currentIndex == _expressionLength) ? true : false;

            if (_currentIndex > currentPartStartIndex)
            {
                var member = _expression.Slice(currentPartStartIndex, _currentIndex - currentPartStartIndex).TrimEnd();
                if (member.Length == 1 && member[0] == '*')
                {
                    member = default;
                }
                filters.Add(CreatePathFilter(member, scan));
            }
            else
            {
                // no field name following dot in path and at end of base path/query
                if (followingDot && (atPathEnd || query))
                {
                    ThrowHelper2.ThrowJsonException_Unexpected_end_while_parsing_path();
                }
            }

            return atPathEnd;
        }

        private static PathFilter CreatePathFilter(string member, bool scan)
        {
            PathFilter filter = (scan) ? (PathFilter)new ScanFilter { Name = member } : new FieldFilter { Name = member };
            return filter;
        }

        private static PathFilter CreatePathFilter(in ReadOnlySpan<char> memberSpan, bool scan)
        {
            var member = memberSpan.IsEmpty ? null : memberSpan.ToString();
            PathFilter filter = (scan) ? (PathFilter)new ScanFilter { Name = member } : new FieldFilter { Name = member };
            return filter;
        }

        private PathFilter ParseIndexer(char indexerOpenChar, bool scan)
        {
            _currentIndex++;

            char indexerCloseChar = (indexerOpenChar == '[') ? ']' : ')';

            if ((uint)_currentIndex >= _expressionLength) { ThrowHelper2.ThrowJsonException_Path_ended_with_open_indexer(); }

            EatWhitespace();

            if (_expression[_currentIndex] == '\'')
            {
                return ParseQuotedField(indexerCloseChar, scan);
            }
            else if (_expression[_currentIndex] == '?')
            {
                return ParseQuery(indexerCloseChar, scan);
            }
            else
            {
                return ParseArrayIndexer(indexerCloseChar);
            }
        }

        private PathFilter ParseArrayIndexer(char indexerCloseChar)
        {
            int start = _currentIndex;
            int? end = null;
            List<int> indexes = null;
            int colonCount = 0;
            int? startIndex = null;
            int? endIndex = null;
            int? step = null;

            while ((uint)_currentIndex < _expressionLength)
            {
                char currentCharacter = _expression[_currentIndex];

                if (currentCharacter == ' ')
                {
                    end = _currentIndex;
                    EatWhitespace();
                    continue;
                }

                if (currentCharacter == indexerCloseChar)
                {
                    int length = (end ?? _currentIndex) - start;

                    if (indexes != null)
                    {
                        if (length == 0)
                        {
                            ThrowHelper2.ThrowJsonException_Array_index_expected();
                        }

                        var indexer = _expression.Slice(start, length);
#if NETSTANDARD2_0 || NET471 || NET451
                        int index = int.Parse(indexer.ToString());
#else
                        int index = int.Parse(indexer);
#endif

                        indexes.Add(index);
                        return new ArrayMultipleIndexFilter { Indexes = indexes };
                    }
                    else if (colonCount > 0)
                    {
                        if (length > 0)
                        {
                            var indexer = _expression.Slice(start, length);
#if NETSTANDARD2_0 || NET471 || NET451
                            int index = int.Parse(indexer.ToString());
#else
                            int index = int.Parse(indexer);
#endif

                            if (colonCount == 1)
                            {
                                endIndex = index;
                            }
                            else
                            {
                                step = index;
                            }
                        }

                        return new ArraySliceFilter { Start = startIndex, End = endIndex, Step = step };
                    }
                    else
                    {
                        if (length == 0)
                        {
                            ThrowHelper2.ThrowJsonException_Array_index_expected();
                        }

                        var indexer = _expression.Slice(start, length);
#if NETSTANDARD2_0 || NET471 || NET451
                        int index = int.Parse(indexer.ToString());
#else
                        int index = int.Parse(indexer);
#endif

                        return new ArrayIndexFilter { Index = index };
                    }
                }
                else if (currentCharacter == ',')
                {
                    int length = (end ?? _currentIndex) - start;

                    if (length == 0)
                    {
                        ThrowHelper2.ThrowJsonException_Array_index_expected();
                    }

                    if (indexes == null)
                    {
                        indexes = new List<int>();
                    }

                    var indexer = _expression.Slice(start, length);
#if NETSTANDARD2_0 || NET471 || NET451
                    indexes.Add(int.Parse(indexer.ToString()));
#else
                    indexes.Add(int.Parse(indexer));
#endif

                    _currentIndex++;

                    EatWhitespace();

                    start = _currentIndex;
                    end = null;
                }
                else if (currentCharacter == '*')
                {
                    _currentIndex++;
                    if ((uint)_currentIndex >= _expressionLength) { ThrowHelper2.ThrowJsonException_Path_ended_with_open_indexer(); }
                    EatWhitespace();

                    if (_expression[_currentIndex] != indexerCloseChar)
                    {
                        ThrowHelper2.ThrowJsonException_Unexpected_character_while_parsing_path_indexer(currentCharacter);
                    }

                    return new ArrayIndexFilter();
                }
                else if (currentCharacter == ':')
                {
                    int length = (end ?? _currentIndex) - start;

                    if (length > 0)
                    {
                        var indexer = _expression.Slice(start, length);
#if NETSTANDARD2_0 || NET471 || NET451
                        int index = int.Parse(indexer.ToString());
#else
                        int index = int.Parse(indexer);
#endif

                        if (colonCount == 0)
                        {
                            startIndex = index;
                        }
                        else if (colonCount == 1)
                        {
                            endIndex = index;
                        }
                        else
                        {
                            step = index;
                        }
                    }

                    colonCount++;

                    _currentIndex++;

                    EatWhitespace();

                    start = _currentIndex;
                    end = null;
                }
                else if (!char.IsDigit(currentCharacter) && currentCharacter != '-')
                {
                    ThrowHelper2.ThrowJsonException_Unexpected_character_while_parsing_path_indexer(currentCharacter);
                }
                else
                {
                    if (end != null)
                    {
                        ThrowHelper2.ThrowJsonException_Unexpected_character_while_parsing_path_indexer(currentCharacter);
                    }

                    _currentIndex++;
                }
            }

            throw ThrowHelper2.GetJsonException_Path_ended_with_open_indexer();
        }

        private void EatWhitespace()
        {
            while ((uint)_currentIndex < _expressionLength)
            {
                if (_expression[_currentIndex] != ' ')
                {
                    break;
                }

                _currentIndex++;
            }
        }

        private PathFilter ParseQuery(char indexerCloseChar, bool scan)
        {
            _currentIndex++;
            if ((uint)_currentIndex >= _expressionLength) { ThrowHelper2.ThrowJsonException_Path_ended_with_open_indexer(); }

            if (_expression[_currentIndex] != '(')
            {
                ThrowHelper2.ThrowJsonException_Unexpected_character_while_parsing_path_indexer(_expression, _currentIndex);
            }

            _currentIndex++;

            QueryExpression expression = ParseExpression();

            _currentIndex++;
            if ((uint)_currentIndex >= _expressionLength) { ThrowHelper2.ThrowJsonException_Path_ended_with_open_indexer(); }
            EatWhitespace();

            if (_expression[_currentIndex] != indexerCloseChar)
            {
                ThrowHelper2.ThrowJsonException_Unexpected_character_while_parsing_path_indexer(_expression, _currentIndex);
            }

            if (!scan)
            {
                return new QueryFilter
                {
                    Expression = expression
                };
            }
            else
            {
                return new QueryScanFilter
                {
                    Expression = expression
                };
            }
        }

        private bool TryParseExpression(out List<PathFilter> expressionPath)
        {
            if (_expression[_currentIndex] == '$')
            {
                expressionPath = new List<PathFilter>();
                expressionPath.Add(RootFilter.Instance);
            }
            else if (_expression[_currentIndex] == '@')
            {
                expressionPath = new List<PathFilter>();
            }
            else
            {
                expressionPath = null;
                return false;
            }

            _currentIndex++;

            if (ParsePath(expressionPath, _currentIndex, true))
            {
                ThrowHelper2.ThrowJsonException_Path_ended_with_open_query();
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private JsonException CreateUnexpectedCharacterException()
        {
            return new JsonException("Unexpected character while parsing path query: " + _expression[_currentIndex]);
        }

        private object ParseSide()
        {
            EatWhitespace();

            if (TryParseExpression(out var expressionPath))
            {
                EatWhitespace();
                if ((uint)_currentIndex >= _expressionLength) { ThrowHelper2.ThrowJsonException_Path_ended_with_open_query(); }

                return expressionPath;
            }

            if (TryParseValue(out var value))
            {
                EatWhitespace();
                if ((uint)_currentIndex >= _expressionLength) { ThrowHelper2.ThrowJsonException_Path_ended_with_open_query(); }

                return new JValue(value);
            }

            throw CreateUnexpectedCharacterException();
        }

        private QueryExpression ParseExpression()
        {
            QueryExpression rootExpression = null;
            CompositeExpression parentExpression = null;

            while ((uint)_currentIndex < _expressionLength)
            {
                object left = ParseSide();
                object right = null;

                QueryOperator op;
                if (_expression[_currentIndex] == ')'
                    || _expression[_currentIndex] == '|'
                    || _expression[_currentIndex] == '&')
                {
                    op = QueryOperator.Exists;
                }
                else
                {
                    op = ParseOperator();

                    right = ParseSide();
                }

                BooleanQueryExpression booleanExpression = new BooleanQueryExpression
                {
                    Left = left,
                    Operator = op,
                    Right = right
                };

                if (_expression[_currentIndex] == ')')
                {
                    if (parentExpression != null)
                    {
                        parentExpression.Expressions.Add(booleanExpression);
                        return rootExpression;
                    }

                    return booleanExpression;
                }
                if (_expression[_currentIndex] == '&')
                {
                    if (!Match("&&"))
                    {
                        throw CreateUnexpectedCharacterException();
                    }

                    if (parentExpression == null || parentExpression.Operator != QueryOperator.And)
                    {
                        CompositeExpression andExpression = new CompositeExpression { Operator = QueryOperator.And };

                        parentExpression?.Expressions.Add(andExpression);

                        parentExpression = andExpression;

                        if (rootExpression == null)
                        {
                            rootExpression = parentExpression;
                        }
                    }

                    parentExpression.Expressions.Add(booleanExpression);
                }
                if (_expression[_currentIndex] == '|')
                {
                    if (!Match("||"))
                    {
                        throw CreateUnexpectedCharacterException();
                    }

                    if (parentExpression == null || parentExpression.Operator != QueryOperator.Or)
                    {
                        CompositeExpression orExpression = new CompositeExpression { Operator = QueryOperator.Or };

                        parentExpression?.Expressions.Add(orExpression);

                        parentExpression = orExpression;

                        if (rootExpression == null)
                        {
                            rootExpression = parentExpression;
                        }
                    }

                    parentExpression.Expressions.Add(booleanExpression);
                }
            }

            throw ThrowHelper2.GetJsonException_Path_ended_with_open_query();
        }

        private bool TryParseValue(out object value)
        {
            char currentChar = _expression[_currentIndex];
            if (currentChar == '\'')
            {
                value = ReadQuotedString();
                return true;
            }
            else if (char.IsDigit(currentChar) || currentChar == '-')
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(currentChar);

                _currentIndex++;
                while ((uint)_currentIndex < _expressionLength)
                {
                    currentChar = _expression[_currentIndex];
                    if (currentChar == ' ' || currentChar == ')')
                    {
                        string numberText = sb.ToString();

                        if (numberText.IndexOfAny(FloatCharacters) != -1)
                        {
                            bool result = double.TryParse(numberText, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var d);
                            value = d;
                            return result;
                        }
                        else
                        {
                            bool result = long.TryParse(numberText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l);
                            value = l;
                            return result;
                        }
                    }
                    else
                    {
                        sb.Append(currentChar);
                        _currentIndex++;
                    }
                }
            }
            else if (currentChar == 't')
            {
                if (Match("true"))
                {
                    value = true;
                    return true;
                }
            }
            else if (currentChar == 'f')
            {
                if (Match("false"))
                {
                    value = false;
                    return true;
                }
            }
            else if (currentChar == 'n')
            {
                if (Match("null"))
                {
                    value = null;
                    return true;
                }
            }
            else if (currentChar == '/')
            {
                value = ReadRegexString();
                return true;
            }

            value = null;
            return false;
        }

        private string ReadQuotedString()
        {
            StringBuilder sb = new StringBuilder();

            _currentIndex++;
            while ((uint)_currentIndex < _expressionLength)
            {
                char currentChar = _expression[_currentIndex];
                if (currentChar == '\\' && (uint)(_currentIndex + 1) < _expressionLength)
                {
                    _currentIndex++;
                    currentChar = _expression[_currentIndex];

                    char resolvedChar;
                    switch (currentChar)
                    {
                        case 'b':
                            resolvedChar = '\b';
                            break;
                        case 't':
                            resolvedChar = '\t';
                            break;
                        case 'n':
                            resolvedChar = '\n';
                            break;
                        case 'f':
                            resolvedChar = '\f';
                            break;
                        case 'r':
                            resolvedChar = '\r';
                            break;
                        case '\\':
                        case '"':
                        case '\'':
                        case '/':
                            resolvedChar = currentChar;
                            break;
                        default:
                            throw ThrowHelper2.GetJsonException_Unknown_escape_character(currentChar);
                    }

                    sb.Append(resolvedChar);

                    _currentIndex++;
                }
                else if (currentChar == '\'')
                {
                    _currentIndex++;
                    return sb.ToString();
                }
                else
                {
                    _currentIndex++;
                    sb.Append(currentChar);
                }
            }

            throw ThrowHelper2.GetJsonException_Path_ended_with_an_open_string();
        }

        private string ReadRegexString()
        {
            int startIndex = _currentIndex;

            _currentIndex++;
            while ((uint)_currentIndex < _expressionLength)
            {
                char currentChar = _expression[_currentIndex];

                // handle escaped / character
                if (currentChar == '\\' && (uint)(_currentIndex + 1) < _expressionLength)
                {
                    _currentIndex += 2;
                }
                else if (currentChar == '/')
                {
                    _currentIndex++;

                    while ((uint)_currentIndex < _expressionLength)
                    {
                        currentChar = _expression[_currentIndex];

                        if (char.IsLetter(currentChar))
                        {
                            _currentIndex++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    return _expression.Slice(startIndex, _currentIndex - startIndex).ToString();
                }
                else
                {
                    _currentIndex++;
                }
            }

            throw ThrowHelper2.GetJsonException_Path_ended_with_an_open_regex();
        }

        private bool Match(string s)
        {
            int currentPosition = _currentIndex;
            foreach (char c in s)
            {
                if ((uint)currentPosition < _expressionLength && _expression[currentPosition] == c)
                {
                    currentPosition++;
                }
                else
                {
                    return false;
                }
            }

            _currentIndex = currentPosition;
            return true;
        }

        private QueryOperator ParseOperator()
        {
            if ((uint)(_currentIndex + 1) >= _expressionLength)
            {
                ThrowHelper2.ThrowJsonException_Path_ended_with_open_query();
            }

            if (Match("==="))
            {
                return QueryOperator.StrictEquals;
            }

            if (Match("=="))
            {
                return QueryOperator.Equals;
            }

            if (Match("=~"))
            {
                return QueryOperator.RegexEquals;
            }

            if (Match("!=="))
            {
                return QueryOperator.StrictNotEquals;
            }

            if (Match("!=") || Match("<>"))
            {
                return QueryOperator.NotEquals;
            }
            if (Match("<="))
            {
                return QueryOperator.LessThanOrEquals;
            }
            if (Match("<"))
            {
                return QueryOperator.LessThan;
            }
            if (Match(">="))
            {
                return QueryOperator.GreaterThanOrEquals;
            }
            if (Match(">"))
            {
                return QueryOperator.GreaterThan;
            }

            throw ThrowHelper2.GetJsonException_Could_not_read_query_operator();
        }

        private PathFilter ParseQuotedField(char indexerCloseChar, bool scan)
        {
            List<string> fields = null;

            while ((uint)_currentIndex < _expressionLength)
            {
                string field = ReadQuotedString();

                EatWhitespace();
                if ((uint)_currentIndex >= _expressionLength) { ThrowHelper2.ThrowJsonException_Path_ended_with_open_indexer(); }

                if (_expression[_currentIndex] == indexerCloseChar)
                {
                    if (fields != null)
                    {
                        fields.Add(field);
                        return (scan)
                            ? (PathFilter)new ScanMultipleFilter { Names = fields }
                            : (PathFilter)new FieldMultipleFilter { Names = fields };
                    }
                    else
                    {
                        return CreatePathFilter(field, scan);
                    }
                }
                else if (_expression[_currentIndex] == ',')
                {
                    _currentIndex++;
                    EatWhitespace();

                    if (fields == null)
                    {
                        fields = new List<string>();
                    }

                    fields.Add(field);
                }
                else
                {
                    ThrowHelper2.ThrowJsonException_Unexpected_character_while_parsing_path_indexer(_expression, _currentIndex);
                }
            }

            throw ThrowHelper2.GetJsonException_Path_ended_with_open_indexer();
        }

        //private void EnsureLength(string message)
        //{
        //    if ((uint)_currentIndex >= _expressionLength)
        //    {
        //        ThrowHelper2.ThrowJsonException(message);
        //    }
        //}

        internal IEnumerable<JToken> Evaluate(JToken root, JToken t, bool errorWhenNoMatch)
        {
            return Evaluate(Filters, root, t, errorWhenNoMatch);
        }

        internal static IEnumerable<JToken> Evaluate(List<PathFilter> filters, JToken root, JToken t, bool errorWhenNoMatch)
        {
            IEnumerable<JToken> current = new[] { t };
            foreach (PathFilter filter in filters)
            {
                current = filter.ExecuteFilter(root, current, errorWhenNoMatch);
            }

            return current;
        }
    }
}