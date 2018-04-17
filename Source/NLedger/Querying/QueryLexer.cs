// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Textual;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Querying
{
    public class QueryLexer
    {
        public QueryLexer(IEnumerable<Value> begin, bool multipleArgs = true)
        {
            if (begin == null || !begin.Any())
                throw new ArgumentException("begin");

            Begin = begin;
            MultipleArgs = multipleArgs;
            ConsumeWhitespace = false;
            ConsumeNextArg = false;

            BeginEnumerator = begin.GetEnumerator();
            if (!BeginEnumerator.MoveNext())
                throw new InvalidOperationException("assert(begin != end)");
            SetArg();
        }

        public QueryLexerToken TokenCache { get; private set; }
        public IEnumerable<Value> Begin { get; set; }

        public bool ConsumeWhitespace { get; private set; }
        public bool ConsumeNextArg { get; set; }
        public bool MultipleArgs { get; private set; }

        public QueryLexerToken NextToken(QueryLexerTokenKind tokContext = QueryLexerTokenKind.UNKNOWN)
        {
            if (TokenCache.Kind != QueryLexerTokenKind.UNKNOWN)
            {
                QueryLexerToken tok = TokenCache;
                TokenCache = new QueryLexerToken();
                return tok;
            }

            if (ArgI == ArgEnd)
            {
                if (!BeginEnumerator.MoveNext())
                    return new QueryLexerToken(QueryLexerTokenKind.END_REACHED);
                else
                    SetArg();
            }

            resume:

            switch(Arg[ArgI])
            {
                case '\0':
                    throw new InvalidOperationException();

                case '\'':
                case '"':
                case '/':
                    {
                        string pat = String.Empty;
                        char closing = Arg[ArgI];
                        bool foundClosing = false;
                        for(++ArgI; ArgI != ArgEnd; ++ArgI)
                        {
                            if (Arg[ArgI] == '\\')
                            {
                                if (++ArgI == ArgEnd)
                                    throw new ParseError(ParseError.ParseError_UnexpectedBackslashAtEndOfPattern);
                            }
                            else if (Arg[ArgI] == closing)
                            {
                                ++ArgI;
                                foundClosing = true;
                                break;
                            }
                            pat += Arg[ArgI];
                        }
                        if (!foundClosing)
                            throw new ParseError(String.Format(ParseError.ParseError_ExpectedSmthAtEndOfPattern, closing));
                        if (String.IsNullOrEmpty(pat))
                            throw new ParseError(ParseError.ParseError_MatchPatternIsEmpty);

                        return new QueryLexerToken(QueryLexerTokenKind.TERM, pat);
                    }
            }

            if (MultipleArgs && ConsumeNextArg)
            {
                ConsumeNextArg = false;
                QueryLexerToken tok = new QueryLexerToken(QueryLexerTokenKind.TERM, Arg.Substring(ArgI));
                PrevArgI = ArgI;
                ArgI = ArgEnd;
                return tok;
            }

            bool consumeNext = false;
            switch (Arg[ArgI])
            {
                case '\0':
                    throw new InvalidOperationException();

                case ' ':
                case '\t':
                case '\r':
                case '\n':
                    if (++ArgI == ArgEnd)
                        return NextToken(tokContext);
                    goto resume;

                case '(':
                    ++ArgI;
                    if (tokContext == QueryLexerTokenKind.TOK_EXPR)
                        ConsumeWhitespace = true;
                    return new QueryLexerToken(QueryLexerTokenKind.LPAREN);
                case ')':
                    ++ArgI;
                    if (tokContext == QueryLexerTokenKind.TOK_EXPR)
                        ConsumeWhitespace = false;
                    return new QueryLexerToken(QueryLexerTokenKind.RPAREN);

                case '&': ++ArgI; return new QueryLexerToken(QueryLexerTokenKind.TOK_AND);
                case '|': ++ArgI; return new QueryLexerToken(QueryLexerTokenKind.TOK_OR);
                case '!': ++ArgI; return new QueryLexerToken(QueryLexerTokenKind.TOK_NOT);
                case '@': ++ArgI; return new QueryLexerToken(QueryLexerTokenKind.TOK_PAYEE);
                case '#': ++ArgI; return new QueryLexerToken(QueryLexerTokenKind.TOK_CODE);
                case '%': ++ArgI; return new QueryLexerToken(QueryLexerTokenKind.TOK_META);
                case '=':
                    ++ArgI;
                    consumeNext = true;
                    return new QueryLexerToken(QueryLexerTokenKind.TOK_EQ);

                default:
                    {
                        if (Arg[ArgI] == '\\')
                        {
                            ++ArgI;
                            consumeNext = true;
                        }

                        string ident = String.Empty;
                        for(; ArgI != ArgEnd; ++ArgI)
                        {
                            switch (Arg[ArgI])
                            {
                                case '\0':
                                    throw new InvalidOperationException();

                                case ' ':
                                case '\t':
                                case '\n':
                                case '\r':
                                    if (!MultipleArgs && !ConsumeWhitespace && !ConsumeNextArg)
                                        goto test_ident;
                                    else
                                        ident += Arg[ArgI];
                                    break;

                                case ')':
                                    if (!consumeNext)  // DM - the second part of condition "... && tok_context == token_t::TOK_EXPR)" is redundant since it meets an opposite condition after falling through. See query.cc:166
                                        goto test_ident;
                                    ident += Arg[ArgI];
                                    break;

                                case '(':
                                case '&':
                                case '|':
                                case '!':
                                case '@':
                                case '#':
                                case '%':
                                case '=':
                                    if (!consumeNext && tokContext != QueryLexerTokenKind.TOK_EXPR)
                                        goto test_ident;
                                    ident += Arg[ArgI];
                                    break;

                                default:
                                    ident += Arg[ArgI];
                                    break;
                            }
                        }
                        ConsumeWhitespace = false;

                        test_ident:

                        QueryLexerTokenKind kind;
                        if (Idents.TryGetValue(ident, out kind))
                        {
                            if (kind == QueryLexerTokenKind.TOK_EXPR)
                                ConsumeNextArg = true;

                            return new QueryLexerToken(kind);
                        }
                        return new QueryLexerToken(QueryLexerTokenKind.TERM, ident);
                    }
            }

            // Unreachable code... return new QueryLexerToken(QueryLexerTokenKind.UNKNOWN);
        }

        public void PushToken (QueryLexerToken tok)
        {
            if (TokenCache.Kind != QueryLexerTokenKind.UNKNOWN)
                throw new InvalidOperationException("token_cache.kind != token_t::UNKNOWN");

            TokenCache = tok;
        }

        public QueryLexerToken PeekToken(QueryLexerTokenKind tokContext = QueryLexerTokenKind.UNKNOWN)
        {
            if (TokenCache.Kind == QueryLexerTokenKind.UNKNOWN)
                TokenCache = NextToken(tokContext);

            return TokenCache;
        }

        public void SetPrev()
        {
            TokenCache = new QueryLexerToken();
            ArgI = PrevArgI;
        }

        private IEnumerator<Value> BeginEnumerator { get; set; }

        private string Arg { get; set; }
        private int ArgI { get; set; }
        private int ArgEnd { get; set; }
        private int PrevArgI { get; set; }

        private void SetArg()
        {
            Arg = BeginEnumerator.Current.AsString ?? String.Empty;
            ArgI = 0;
            ArgEnd = Arg.Length;
        }

        private static readonly IDictionary<string, QueryLexerTokenKind> Idents = new Dictionary<string, QueryLexerTokenKind>()
        {
            { "and", QueryLexerTokenKind.TOK_AND },
            { "or", QueryLexerTokenKind.TOK_OR },
            { "not", QueryLexerTokenKind.TOK_NOT },
            { "code", QueryLexerTokenKind.TOK_CODE },
            { "desc", QueryLexerTokenKind.TOK_PAYEE },
            { "payee", QueryLexerTokenKind.TOK_PAYEE },
            { "note", QueryLexerTokenKind.TOK_NOTE },
            { "tag", QueryLexerTokenKind.TOK_META },
            { "meta", QueryLexerTokenKind.TOK_META },
            { "data", QueryLexerTokenKind.TOK_META },
            { "show", QueryLexerTokenKind.TOK_SHOW },
            { "only", QueryLexerTokenKind.TOK_ONLY },
            { "bold", QueryLexerTokenKind.TOK_BOLD },
            { "for", QueryLexerTokenKind.TOK_FOR },
            { "since", QueryLexerTokenKind.TOK_SINCE },
            { "until", QueryLexerTokenKind.TOK_UNTIL },
            { "expr", QueryLexerTokenKind.TOK_EXPR },
        };
    }
}
