// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility
{
    public static class StringExtensions
    {
        public static IEnumerable<string> SplitArguments(string line)
        {
            line = line ?? String.Empty;
            IList<string> args = new List<string>();

            char[] buf = new char[4096];
            int q = 0; // position in buf
            char inQuotedString = default(char);

            for (int p = 0; p < line.Length; p++)
            {
                if (inQuotedString == default(char) && char.IsWhiteSpace(line[p]))
                {
                    if (q != 0)
                    {
                        args.Add(new string(buf, 0, q));
                        q = 0;
                    }
                }
                else if (inQuotedString != '\'' && line[p] == '\\')
                {
                    p++;
                    if (p == line.Length)
                        throw new LogicError(LogicError.ErrorMessageInvalidUseOfBackslash);
                    buf[q++] = line[p];
                }
                else if (inQuotedString != '"' && line[p] == '\'')
                {
                    if (inQuotedString == '\'')
                        inQuotedString = default(char);
                    else
                        inQuotedString = '\'';
                }
                else if (inQuotedString != '\'' && line[p] == '"')
                {
                    if (inQuotedString == '"')
                        inQuotedString = default(char);
                    else
                        inQuotedString = '"';
                }
                else
                {
                    buf[q++] = line[p];
                }
            }

            if (inQuotedString != default(char))
                throw new LogicError(String.Format(LogicError.ErrorMessageUnterminatedStringExpectedSmth, inQuotedString));

            if (q != 0)
                args.Add(new string(buf, 0, q));

            return args;
        }

        public static string SafeSubstring(this string s, int startIndex, int length)
        {
            if (String.IsNullOrEmpty(s))
                return String.Empty;

            return s.Length > startIndex ? s.Substring(startIndex, length) : String.Empty;
        }

        public static string SafeSubstring(this string s, int startIndex)
        {
            if (String.IsNullOrEmpty(s))
                return String.Empty;

            return s.Length > startIndex ? s.Substring(startIndex) : String.Empty;
        }

        /// <summary>
        /// Ported from next_element
        /// </summary>
        public static string NextElement(ref string s, bool variable = false)
        {
            if (String.IsNullOrEmpty(s))
                return String.Empty;

            int startIndex = 0;
            while (true)
            {
                int pos = s.IndexOfAny(CharExtensions.WhitespaceChars, startIndex);
                if (pos == -1)
                    return String.Empty;

                if (!variable || s[pos] == '\t' || pos == s.Length - 1 || s[pos + 1] == ' ')
                {
                    string element = s.Substring(pos).TrimStart();
                    s = s.Substring(0, pos);
                    return element;
                }
                else
                {
                    startIndex = pos + 2;
                }
            }
        }

        public static string SplitBySeparator(ref string s, char separator, bool variable = false)
        {
            if (String.IsNullOrEmpty(s))
                return String.Empty;

            int startIndex = 0;
            while (true)
            {
                int pos = s.IndexOf(separator, startIndex);

                if (pos == -1)
                    return String.Empty;

                if (!variable || (pos > 2 && (s[pos-1] == '\t' || String.IsNullOrWhiteSpace(s.Substring(pos-2, 2)))))
                {
                    string element = s.Substring(pos + 1);
                    s = s.Substring(0, pos).TrimEnd();
                    return element;
                }
                else
                {
                    startIndex = pos + 1;
                }
            }
        }

        public static string ReadInto(ref string s, Func<char, bool> condition)
        {
            if (condition == null)
                throw new ArgumentNullException("condition");

            if (String.IsNullOrEmpty(s))
                return String.Empty;

            for (int i = 0; i < s.Length; i++)
            {
                if (!condition(s[i]))
                {
                    string result = s.Substring(0, i).EncodeEscapeSequenecs();
                    s = s.Substring(i);
                    return result;
                }
            }

            string finalResult = s;
            s = string.Empty;
            return finalResult;
        }

        public static string EncodeEscapeSequenecs(this string s)
        {
            if (!String.IsNullOrEmpty(s) && s.IndexOf('\\') >= 0)
                return s.
                        Replace(@"\b", "\b").
                        Replace(@"\f", "\f").
                        Replace(@"\n", "\n").
                        Replace(@"\r", "\r").
                        Replace(@"\t", "\t").
                        Replace(@"\v", "\v");
            else
                return s ?? String.Empty;
        }

        public static bool StartsWithDigit(this string s)
        {
            return !String.IsNullOrEmpty(s) && Char.IsDigit(s[0]);
        }

        public static bool StartsWithWhiteSpace(this string s)
        {
            return !String.IsNullOrEmpty(s) && Char.IsWhiteSpace(s[0]);
        }

        public static string GetWidthAlignFormatString(int width = 0, bool alignRight = false)
        {
            int key = alignRight ? width : -width;
            string formatString;
            if (!WidthAlignFormatStrings.TryGetValue(key, out formatString))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("{0");
                if (width > 0)
                {
                    sb.Append(",");
                    if (alignRight)
                        sb.Append("-");
                    sb.Append(width);
                }
                sb.Append("}");

                formatString = sb.ToString();
                WidthAlignFormatStrings[key] = formatString;
            }
            return formatString;
        }

        public static string GetFirstLine(this string s)
        {
            return String.IsNullOrEmpty(s) ? s : s.Split(new[] { '\r', '\n' }).FirstOrDefault();
        }

        public static string GetWord(ref string s)
        {
            s = s?.TrimStart();
            if (String.IsNullOrEmpty(s))
                return String.Empty;

            var pos = s.IndexOfAny(CharExtensions.WhitespaceChars);
            if (pos >= 0)
            {
                var word = s.Substring(0, pos);
                s = s.Substring(pos).TrimStart();
                return word;
            }
            else
            {
                var word = s;
                s = String.Empty;
                return word;
            }
        }

        private static readonly IDictionary<int, string> WidthAlignFormatStrings = new Dictionary<int, string>();
    }
}
