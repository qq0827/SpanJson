using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace SpanJson.Internal
{
    public static class StringMutator
    {
        /// <summary>MyProperty -> MyProperty</summary>
        public static string Original(string s)
        {
            return s;
        }

        // borrowed from https://github.com/JamesNK/Newtonsoft.Json/blob/4ab34b0461fb595805d092a46a58f35f66c84d6a/Src/Newtonsoft.Json/Utilities/StringUtils.cs#L149

        private static readonly ConcurrentDictionary<string, string> s_camelCaseCache =
            new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

        /// <summary>MyProperty -> myProperty</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToCamelCaseWithCache(string s)
        {
            if (string.IsNullOrEmpty(s) || !char.IsUpper(s[0])) { return s; }

            return s_camelCaseCache.GetOrAdd(s, _ => ToCamelCase(_));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string ToCamelCase(string s)
        {
            if (string.IsNullOrEmpty(s) || !char.IsUpper(s[0])) { return s; }

            char[] chars = s.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (i == 1 && !char.IsUpper(chars[i]))
                {
                    break;
                }

                bool hasNext = (i + 1 < chars.Length);
                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                {
                    // if the next character is a space, which is not considered uppercase 
                    // (otherwise we wouldn't be here...)
                    // we want to ensure that the following:
                    // 'FOO bar' is rewritten as 'foo bar', and not as 'foO bar'
                    // The code was written in such a way that the first word in uppercase
                    // ends when if finds an uppercase letter followed by a lowercase letter.
                    // now a ' ' (space, (char)32) is considered not upper
                    // but in that case we still want our current character to become lowercase
                    if (char.IsSeparator(chars[i + 1]))
                    {
                        chars[i] = char.ToLower(chars[i], CultureInfo.InvariantCulture);
                    }

                    break;
                }

                chars[i] = char.ToLower(chars[i], CultureInfo.InvariantCulture);
            }

            return new string(chars);
        }

        // borrowed from https://github.com/JamesNK/Newtonsoft.Json/blob/4ab34b0461fb595805d092a46a58f35f66c84d6a/Src/Newtonsoft.Json/Utilities/StringUtils.cs#L208

        private static readonly ConcurrentDictionary<string, string> s_snakeCaseCache =
            new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

        /// <summary>MyProperty -> my_property</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToSnakeCaseWithCache(string s)
        {
            if (string.IsNullOrEmpty(s)) { return s; }

            return s_snakeCaseCache.GetOrAdd(s, _ => ToSnakeCase(_));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string ToSnakeCase(string s)
        {
            if (string.IsNullOrEmpty(s)) { return s; }

            var sb = StringBuilderCache.Acquire();
            var state = SnakeCaseState.Start;

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == ' ')
                {
                    if (state != SnakeCaseState.Start)
                    {
                        state = SnakeCaseState.NewWord;
                    }
                }
                else if (char.IsUpper(s[i]))
                {
                    switch (state)
                    {
                        case SnakeCaseState.Upper:
                            bool hasNext = (i + 1 < s.Length);
                            if (i > 0 && hasNext)
                            {
                                char nextChar = s[i + 1];
                                if (!char.IsUpper(nextChar) && nextChar != '_')
                                {
                                    sb.Append('_');
                                }
                            }
                            break;
                        case SnakeCaseState.Lower:
                        case SnakeCaseState.NewWord:
                            sb.Append('_');
                            break;
                    }

                    var c = char.ToLower(s[i], CultureInfo.InvariantCulture);
                    sb.Append(c);

                    state = SnakeCaseState.Upper;
                }
                else if (s[i] == '_')
                {
                    sb.Append('_');
                    state = SnakeCaseState.Start;
                }
                else
                {
                    if (state == SnakeCaseState.NewWord)
                    {
                        sb.Append('_');
                    }

                    sb.Append(s[i]);
                    state = SnakeCaseState.Lower;
                }
            }

            return StringBuilderCache.GetStringAndRelease(sb);
        }

        private enum SnakeCaseState
        {
            Start,
            Lower,
            Upper,
            NewWord
        }
    }
}
