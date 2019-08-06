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
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using StringEscapeHandling = Newtonsoft.Json.StringEscapeHandling;

namespace SpanJson.Utilities
{
    internal static class BufferUtils
    {
        public static char[] EnsureBufferSize(int size, char[] buffer)
        {
            if (buffer != null)
            {
                ArrayPool<char>.Shared.Return(buffer);
            }

            return ArrayPool<char>.Shared.Rent(size);
        }
    }

    internal static class JavaScriptUtils
    {
        internal static readonly bool[] SingleQuoteCharEscapeFlags = new bool[128];

        private const int UnicodeTextLength = 6;

        static JavaScriptUtils()
        {
            IList<char> escapeChars = new List<char>
            {
                '\n', '\r', '\t', '\\', '\f', '\b',
            };
            for (int i = 0; i < ' '; i++)
            {
                escapeChars.Add((char)i);
            }

            foreach (char escapeChar in escapeChars.Union(new[] { '\'' }))
            {
                SingleQuoteCharEscapeFlags[escapeChar] = true;
            }
        }

        private const string EscapedUnicodeText = "!";

        public static bool ShouldEscapeJavaScriptString(string s, bool[] charEscapeFlags)
        {
            if (s == null)
            {
                return false;
            }

            foreach (char c in s)
            {
                if (c >= charEscapeFlags.Length || charEscapeFlags[c])
                {
                    return true;
                }
            }

            return false;
        }

        public static void WriteEscapedJavaScriptString(TextWriter writer, string s, char delimiter, bool appendDelimiters,
            bool[] charEscapeFlags, StringEscapeHandling stringEscapeHandling, ref char[] writeBuffer)
        {
            // leading delimiter
            if (appendDelimiters)
            {
                writer.Write(delimiter);
            }

            if (!string.IsNullOrEmpty(s))
            {
                int lastWritePosition = FirstCharToEscape(s, charEscapeFlags, stringEscapeHandling);
                if (lastWritePosition == -1)
                {
                    writer.Write(s);
                }
                else
                {
                    if (lastWritePosition != 0)
                    {
                        if (writeBuffer == null || writeBuffer.Length < lastWritePosition)
                        {
                            writeBuffer = BufferUtils.EnsureBufferSize(lastWritePosition, writeBuffer);
                        }

                        // write unchanged chars at start of text.
                        s.CopyTo(0, writeBuffer, 0, lastWritePosition);
                        writer.Write(writeBuffer, 0, lastWritePosition);
                    }

                    int length;
                    for (int i = lastWritePosition; i < s.Length; i++)
                    {
                        char c = s[i];

                        if (c < charEscapeFlags.Length && !charEscapeFlags[c])
                        {
                            continue;
                        }

                        string escapedValue;

                        switch (c)
                        {
                            case '\t':
                                escapedValue = @"\t";
                                break;
                            case '\n':
                                escapedValue = @"\n";
                                break;
                            case '\r':
                                escapedValue = @"\r";
                                break;
                            case '\f':
                                escapedValue = @"\f";
                                break;
                            case '\b':
                                escapedValue = @"\b";
                                break;
                            case '\\':
                                escapedValue = @"\\";
                                break;
                            case '\u0085': // Next Line
                                escapedValue = @"\u0085";
                                break;
                            case '\u2028': // Line Separator
                                escapedValue = @"\u2028";
                                break;
                            case '\u2029': // Paragraph Separator
                                escapedValue = @"\u2029";
                                break;
                            default:
                                if (c < charEscapeFlags.Length || stringEscapeHandling == StringEscapeHandling.EscapeNonAscii)
                                {
                                    if (c == '\'' && stringEscapeHandling != StringEscapeHandling.EscapeHtml)
                                    {
                                        escapedValue = @"\'";
                                    }
                                    else if (c == '"' && stringEscapeHandling != StringEscapeHandling.EscapeHtml)
                                    {
                                        escapedValue = @"\""";
                                    }
                                    else
                                    {
                                        if (writeBuffer == null || writeBuffer.Length < UnicodeTextLength)
                                        {
                                            writeBuffer = BufferUtils.EnsureBufferSize(UnicodeTextLength, writeBuffer);
                                        }

                                        ToCharAsUnicode(c, writeBuffer);

                                        // slightly hacky but it saves multiple conditions in if test
                                        escapedValue = EscapedUnicodeText;
                                    }
                                }
                                else
                                {
                                    escapedValue = null;
                                }
                                break;
                        }

                        if (escapedValue == null)
                        {
                            continue;
                        }

                        bool isEscapedUnicodeText = string.Equals(escapedValue, EscapedUnicodeText, StringComparison.Ordinal);

                        if (i > lastWritePosition)
                        {
                            length = i - lastWritePosition + ((isEscapedUnicodeText) ? UnicodeTextLength : 0);
                            int start = (isEscapedUnicodeText) ? UnicodeTextLength : 0;

                            if (writeBuffer == null || writeBuffer.Length < length)
                            {
                                char[] newBuffer = ArrayPool<char>.Shared.Rent(length);

                                // the unicode text is already in the buffer
                                // copy it over when creating new buffer
                                if (isEscapedUnicodeText)
                                {
                                    Debug.Assert(writeBuffer != null, "Write buffer should never be null because it is set when the escaped unicode text is encountered.");

                                    Array.Copy(writeBuffer, newBuffer, UnicodeTextLength);
                                }

                                ArrayPool<char>.Shared.Return(writeBuffer);

                                writeBuffer = newBuffer;
                            }

                            s.CopyTo(lastWritePosition, writeBuffer, start, length - start);

                            // write unchanged chars before writing escaped text
                            writer.Write(writeBuffer, start, length - start);
                        }

                        lastWritePosition = i + 1;
                        if (!isEscapedUnicodeText)
                        {
                            writer.Write(escapedValue);
                        }
                        else
                        {
                            writer.Write(writeBuffer, 0, UnicodeTextLength);
                        }
                    }

                    Debug.Assert(lastWritePosition != 0);
                    length = s.Length - lastWritePosition;
                    if (length > 0)
                    {
                        if (writeBuffer == null || writeBuffer.Length < length)
                        {
                            writeBuffer = BufferUtils.EnsureBufferSize(length, writeBuffer);
                        }

                        s.CopyTo(lastWritePosition, writeBuffer, 0, length);

                        // write remaining text
                        writer.Write(writeBuffer, 0, length);
                    }
                }
            }

            // trailing delimiter
            if (appendDelimiters)
            {
                writer.Write(delimiter);
            }
        }

        private static void ToCharAsUnicode(char c, char[] buffer)
        {
            buffer[0] = '\\';
            buffer[1] = 'u';
            buffer[2] = MathUtils.IntToHex((c >> 12) & '\x000f');
            buffer[3] = MathUtils.IntToHex((c >> 8) & '\x000f');
            buffer[4] = MathUtils.IntToHex((c >> 4) & '\x000f');
            buffer[5] = MathUtils.IntToHex(c & '\x000f');
        }

        private static int FirstCharToEscape(string s, bool[] charEscapeFlags, StringEscapeHandling stringEscapeHandling)
        {
            for (int i = 0; i != s.Length; i++)
            {
                char c = s[i];

                if (c < charEscapeFlags.Length)
                {
                    if (charEscapeFlags[c])
                    {
                        return i;
                    }
                }
                else if (stringEscapeHandling == StringEscapeHandling.EscapeNonAscii)
                {
                    return i;
                }
                else
                {
                    switch (c)
                    {
                        case '\u0085':
                        case '\u2028':
                        case '\u2029':
                            return i;
                    }
                }
            }

            return -1;
        }
    }
}