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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NLedger
{
    /// <summary>
    /// Regular expression masking.
    /// </summary>
    /// <remarks>
    /// Ported from mask_t
    /// </remarks>
    public sealed class Mask
    {
        public static Mask AssignGlob(string pat)
        {
            string re_pat = "";
            int len = pat.Length;
            for (var i = 0; i < len; i++)
            {
                switch (pat[i])
                {
                    case '?':
                        re_pat += '.';
                        break;
                    case '*':
                        re_pat += ".*";
                        break;
                    case '[':
                        while (i < len && pat[i] != ']')
                            re_pat += pat[i++];
                        if (i < len)
                            re_pat += pat[i];
                        break;

                    case '\\':
                        if (i + 1 < len)
                        {
                            re_pat += pat[++i];
                            break;
                        }
                        else
                        {
                            // fallthrough...
                            re_pat += pat[i];
                            break;
                        }
                    default:
                        re_pat += pat[i];
                        break;
                }
            }
            return new Mask(re_pat);
        }

        public Mask()
        { }

        public Mask(string pattern)
        {
            Expr = new Regex(pattern, RegexOptions.IgnoreCase);
        }

        public bool Match(string text)
        {
            return IsEmpty ? false : Expr.IsMatch(text);
        }

        public IEnumerable<Match> Matches(string text)
        {
            return IsEmpty ? Enumerable.Empty<Match>() : Expr.Matches(text).Cast<Match>();
        }

        public bool IsEmpty
        {
            get { return Expr == null; }
        }

        public string Str()
        {
            return IsEmpty ? String.Empty : Expr.ToString();
        }

        public override string ToString()
        {
            return Str();
        }

        private Regex Expr { get; set; }
    }
}
