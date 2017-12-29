// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Expressions
{
    /// <summary>
    /// Ported from expr_t::token_t/kind_t (token.h)
    /// </summary>
    public enum ExprTokenKind
    {
        ERROR,                      // an error occurred while tokenizing
        VALUE,                      // any kind of literal value
        IDENT,                      // [A-Za-z_][-A-Za-z0-9_:]*
        MASK,                       // /regexp/

        LPAREN,                     // (
        RPAREN,                     // )
        LBRACE,                     // {
        RBRACE,                     // }

        EQUAL,                      // ==
        NEQUAL,                     // !=
        LESS,                       // <
        LESSEQ,                     // <=
        GREATER,                    // >
        GREATEREQ,                  // >=

        ASSIGN,                     // =
        MATCH,                      // =~
        NMATCH,                     // !~
        MINUS,                      // -
        PLUS,                       // +
        STAR,                       // *
        SLASH,                      // /
        ARROW,                      // ->
        KW_DIV,                     // div

        EXCLAM,                     // !, not
        KW_AND,                     // &, &&, and
        KW_OR,                      // |, ||, or
        KW_MOD,                     // %

        KW_IF,                      // if
        KW_ELSE,                    // else

        QUERY,                      // ?
        COLON,                      // :

        DOT,                        // .
        COMMA,                      // ,
        SEMI,                       // ;

        TOK_EOF,
        UNKNOWN
    }
}
