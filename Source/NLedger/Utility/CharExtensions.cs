// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility
{
    public static class CharExtensions
    {
        public static bool IsCommentLine(this string s)
        {
            return !String.IsNullOrWhiteSpace(s) && s[0].IsCommentChar();
        }

        public static bool IsCommentChar(this char ch)
        {
            return ch == ';' || ch == '#' || ch == '|' || ch == '*';
        }

        public static bool IsDigitChar(this char ch)
        {
            return Char.IsDigit(ch) || ch == '-' || ch == ',' || ch == '.';
        }

        public static bool IsTransactionFlagChar(this char ch)
        {
            return ch == '*' || ch == '!';
        }

        public static char[] WhitespaceChars = new char[] { ' ', '\t' };
        public static char[] DigitChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
    }
}
