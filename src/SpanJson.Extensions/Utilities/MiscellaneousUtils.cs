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
using System.Text.RegularExpressions;

namespace SpanJson.Utilities
{
    internal delegate T Creator<T>();

    internal static class MiscellaneousUtils
    {
        public static ArgumentOutOfRangeException CreateArgumentOutOfRangeException(string paramName, object actualValue, string message)
        {
            string newMessage = message + Environment.NewLine + string.Format(CultureInfo.InvariantCulture, "Actual value was {0}.", actualValue);

            return new ArgumentOutOfRangeException(paramName, newMessage);
        }

        public static string ToString(object value)
        {
            if (value is null)
            {
                return "{null}";
            }

            return (value is string s) ? @"""" + s + @"""" : value.ToString();
        }

        internal static RegexOptions GetRegexOptions(string optionsText)
        {
            RegexOptions options = RegexOptions.None;

            for (int i = 0; i < optionsText.Length; i++)
            {
                switch (optionsText[i])
                {
                    case 'i':
                        options |= RegexOptions.IgnoreCase;
                        break;
                    case 'm':
                        options |= RegexOptions.Multiline;
                        break;
                    case 's':
                        options |= RegexOptions.Singleline;
                        break;
                    case 'x':
                        options |= RegexOptions.ExplicitCapture;
                        break;
                }
            }

            return options;
        }
    }
}