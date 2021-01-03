// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Textual;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Querying
{
    public struct QueryLexerToken
    {
        public const char InverseOne = (char)255;  // -1

        public QueryLexerToken(QueryLexerTokenKind kind = QueryLexerTokenKind.UNKNOWN, string value = null) : this()
        {
            Kind = kind;
            Value = value;
        }

        public QueryLexerTokenKind Kind { get; private set; }
        public string Value { get; private set; }

        public bool IsNotEnd
        {
            get { return Kind != QueryLexerTokenKind.END_REACHED; }
        }

        public override string ToString()
        {
            if (Kind != QueryLexerTokenKind.TERM)
                return Kind.ToString();
            else
                return String.Format("TERM({0})", Value);
        }

        public string Symbol()
        {
            string symbol;
            if (Symbols.TryGetValue(Kind, out symbol))
                return symbol;
            else
                return "<ERROR>";
        }

        public void Unexpected()
        {
            QueryLexerTokenKind prevKind = Kind;
            Kind = QueryLexerTokenKind.UNKNOWN;

            switch (prevKind)
            {
                case QueryLexerTokenKind.END_REACHED:
                    throw new ParseError(ParseError.ParseError_UnexpectedEndOfExpression);
                case QueryLexerTokenKind.TERM:
                    throw new ParseError(String.Format(ParseError.ParseError_UnexpectedString, Value));
                default:
                    throw new ParseError(String.Format(ParseError.ParseError_UnexpectedToken, Symbol()));
            }
        }

        public void Expected(char wanted, char c = default(char))
        {
            if (c == default(char) || c == InverseOne)
            {
                if (wanted == default(char) || wanted == InverseOne)
                    throw new ParseError(ParseError.ParseError_UnexpectedEnd);
                else
                    throw new ParseError(String.Format(ParseError.ParseError_MissingSmth, wanted));
            }
            else
            {
                if (wanted == default(char) || wanted == InverseOne)
                    throw new ParseError(String.Format(ParseError.ParseError_InvalidChar, c));
                else
                    throw new ParseError(String.Format(ParseError.ParseError_InvalidCharWanted, c, wanted));
            }
        }

        private static IDictionary<QueryLexerTokenKind, string> Symbols = new Dictionary<QueryLexerTokenKind, string>()
        {
            { QueryLexerTokenKind.LPAREN, "("},
            { QueryLexerTokenKind.RPAREN, ")"},
            { QueryLexerTokenKind.TOK_NOT, "not"},
            { QueryLexerTokenKind.TOK_AND, "and"},
            { QueryLexerTokenKind.TOK_OR, "or"},
            { QueryLexerTokenKind.TOK_EQ, "="},
            { QueryLexerTokenKind.TOK_CODE, "code"},
            { QueryLexerTokenKind.TOK_PAYEE, "payee"},
            { QueryLexerTokenKind.TOK_NOTE, "note"},
            { QueryLexerTokenKind.TOK_ACCOUNT, "account"},
            { QueryLexerTokenKind.TOK_META, "meta"},
            { QueryLexerTokenKind.TOK_EXPR, "expr"},
            { QueryLexerTokenKind.TOK_SHOW, "show"},
            { QueryLexerTokenKind.TOK_ONLY, "only"},
            { QueryLexerTokenKind.TOK_BOLD, "bold"},
            { QueryLexerTokenKind.TOK_FOR, "for"},
            { QueryLexerTokenKind.TOK_SINCE, "since"},
            { QueryLexerTokenKind.TOK_UNTIL, "until"},
            { QueryLexerTokenKind.END_REACHED, "<EOF>"},
            { QueryLexerTokenKind.TERM, "<TERM>"},  // [DM] assert(false)
            { QueryLexerTokenKind.UNKNOWN, "<UNKNOWN>"}  // [DM] assert(false)
        };
    }
}
