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

namespace NLedger.Querying
{
    public enum QueryLexerTokenKind
    {
        UNKNOWN,

        LPAREN,
        RPAREN,

        TOK_NOT,
        TOK_AND,
        TOK_OR,
        TOK_EQ,

        TOK_CODE,
        TOK_PAYEE,
        TOK_NOTE,
        TOK_ACCOUNT,
        TOK_META,
        TOK_EXPR,

        TOK_SHOW,
        TOK_ONLY,
        TOK_BOLD,
        TOK_FOR,
        TOK_SINCE,
        TOK_UNTIL,

        TERM,

        END_REACHED
    }
}
