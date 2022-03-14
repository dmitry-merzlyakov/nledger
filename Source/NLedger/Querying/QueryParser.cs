// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Annotate;
using NLedger.Expressions;
using NLedger.Textual;
using NLedger.Utils;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Querying
{
    public class QueryParser
    {
        protected QueryParser()
        {
            QueryMap = new Dictionary<QueryKindEnum, string>();
        }

        public QueryParser(Value args, AnnotationKeepDetails whatToKeep = default(AnnotationKeepDetails), bool multipleArgs = true)
        {
            Args = args;
            Lexer = new QueryLexer(args.AsSequence, multipleArgs);
            WhatToKeep = whatToKeep;
            QueryMap = new Dictionary<QueryKindEnum, string>();
        }

        public Value Args { get; private set; }
        public QueryLexer Lexer { get; private set; }
        public AnnotationKeepDetails WhatToKeep { get; private set; }
        public IDictionary<QueryKindEnum, string> QueryMap { get; private set; }

        public bool TokensRemaining
        {
            get
            {
                QueryLexerToken tok = Lexer.PeekToken();
                if (tok.Kind == QueryLexerTokenKind.UNKNOWN)
                    throw new InvalidOperationException("TokensRemaining");
                return tok.Kind != QueryLexerTokenKind.END_REACHED;
            }
        }

        public ExprOp Parse(bool subexpression = false)
        {
            return ParseQueryExpr(QueryLexerTokenKind.TOK_ACCOUNT, subexpression);
        }

        public ExprOp ParseQueryTerm(QueryLexerTokenKind tokContext)
        {
            ExprOp node = null;

            QueryLexerToken tok = Lexer.NextToken(tokContext);
            switch (tok.Kind)
            {
                case QueryLexerTokenKind.TOK_SHOW:
                case QueryLexerTokenKind.TOK_ONLY:
                case QueryLexerTokenKind.TOK_BOLD:
                case QueryLexerTokenKind.TOK_FOR:
                case QueryLexerTokenKind.TOK_SINCE:
                case QueryLexerTokenKind.TOK_UNTIL:
                case QueryLexerTokenKind.END_REACHED:
                    Lexer.PushToken(tok);
                    break;

                case QueryLexerTokenKind.TOK_CODE:
                case QueryLexerTokenKind.TOK_PAYEE:
                case QueryLexerTokenKind.TOK_NOTE:
                case QueryLexerTokenKind.TOK_ACCOUNT:
                case QueryLexerTokenKind.TOK_META:
                case QueryLexerTokenKind.TOK_EXPR:
                    node = ParseQueryTerm(tok.Kind);
                    if (node == null)
                        throw new ParseError(String.Format(ParseError.ParseError_OperatorNotFollowedByArgument, tok.Symbol()));
                    break;

                case QueryLexerTokenKind.TERM:
                    if (String.IsNullOrEmpty(tok.Value))
                        throw new InvalidOperationException("term");

                    switch (tokContext)
                    {
                        case QueryLexerTokenKind.TOK_EXPR:
                            node = new Expr(tok.Value).Op;
                            break;

                        case QueryLexerTokenKind.TOK_META:
                            {
                                node = new ExprOp(OpKindEnum.O_CALL);

                                ExprOp ident = new ExprOp(OpKindEnum.IDENT);
                                ident.AsIdent = "has_tag";
                                node.Left = ident;

                                ExprOp arg1 = new ExprOp(OpKindEnum.VALUE);
                                arg1.AsValue = Value.Get(new Mask(tok.Value));

                                tok = Lexer.PeekToken(tokContext);
                                if (tok.Kind == QueryLexerTokenKind.TOK_EQ)
                                {
                                    tok = Lexer.NextToken(tokContext);
                                    tok = Lexer.NextToken(tokContext);
                                    if (tok.Kind != QueryLexerTokenKind.TERM)
                                        throw new ParseError(ParseError.ParseError_MetadataEqualityOperatorNotFollowedByTerm);

                                    ExprOp arg2 = new ExprOp(OpKindEnum.VALUE);
                                    if (String.IsNullOrEmpty(tok.Value))
                                        throw new InvalidOperationException();
                                    arg2.AsValue = Value.Get(new Mask(tok.Value));

                                    node.Right = ExprOp.NewNode(OpKindEnum.O_SEQ, ExprOp.NewNode(OpKindEnum.O_CONS, arg1, arg2));
                                }
                                else
                                {
                                    node.Right = arg1;
                                }
                                break;
                            }

                        default:
                            {
                                node = new ExprOp(OpKindEnum.O_MATCH);

                                ExprOp ident = new ExprOp(OpKindEnum.IDENT);
                                switch (tokContext)
                                {
                                    case QueryLexerTokenKind.TOK_ACCOUNT:
                                        ident.AsIdent = "account"; break;
                                    case QueryLexerTokenKind.TOK_PAYEE:
                                        ident.AsIdent = "payee"; break;
                                    case QueryLexerTokenKind.TOK_CODE:
                                        ident.AsIdent = "code"; break;
                                    case QueryLexerTokenKind.TOK_NOTE:
                                        ident.AsIdent = "note"; break;
                                    default:
                                        throw new InvalidOperationException();
                                }

                                ExprOp mask = new ExprOp(OpKindEnum.VALUE);
                                Logger.Current.Debug("query.mask", () => String.Format("Mask from string: {0}", tok.Value));
                                mask.AsValue = Value.Get(new Mask(tok.Value));
                                Logger.Current.Debug("query.mask", () => String.Format("Mask is: {0}", mask.AsValue.AsMask.Str()));

                                node.Left = ident;
                                node.Right = mask;

                                break;
                            }
                    }
                    break;

                case QueryLexerTokenKind.LPAREN:
                    node = ParseQueryExpr(tokContext, true);
                    tok = Lexer.NextToken(tokContext);
                    if (tok.Kind != QueryLexerTokenKind.RPAREN)
                        tok.Expected(')');
                    break;

                default:
                    Lexer.PushToken(tok);
                    break;
            }

            return node;
        }

        public ExprOp ParseUnaryExpr(QueryLexerTokenKind tokContext)
        {
            ExprOp node = null;

            QueryLexerToken tok = Lexer.NextToken(tokContext);
            switch(tok.Kind)
            {
                case QueryLexerTokenKind.TOK_NOT:
                    {
                        ExprOp term = ParseQueryTerm(tokContext);
                        if (term == null)
                            throw new ParseError(String.Format(ParseError.ParseError_OperatorNotFollowedByArgument, tok.Symbol()));

                        node = new ExprOp(OpKindEnum.O_NOT);
                        node.Left = term;
                        break;
                    }

                default:
                    Lexer.PushToken(tok);
                    node = ParseQueryTerm(tokContext);
                    break;
            }

            return node;
        }

        public ExprOp ParseAndExpr(QueryLexerTokenKind tokContext)
        {
            ExprOp node = ParseUnaryExpr(tokContext);
            if (node != null)
            {
                while(true)
                {
                    QueryLexerToken tok = Lexer.NextToken(tokContext);
                    if (tok.Kind == QueryLexerTokenKind.TOK_AND)
                    {
                        ExprOp prev = node;
                        node = new ExprOp(OpKindEnum.O_AND);
                        node.Left = prev;
                        node.Right = ParseUnaryExpr(tokContext);
                        if (node.Right == null)
                            throw new ParseError(String.Format(ParseError.ParseError_OperatorNotFollowedByArgument, tok.Symbol()));
                    }
                    else
                    {
                        Lexer.PushToken(tok);
                        break;
                    }

                }
                return node;
            }
            return null; // new ExprOp();
        }

        public ExprOp ParseOrExpr(QueryLexerTokenKind tokContext)
        {
            ExprOp node = ParseAndExpr(tokContext);
            if (node != null)
            {
                while (true)
                {
                    QueryLexerToken tok = Lexer.NextToken(tokContext);
                    if (tok.Kind == QueryLexerTokenKind.TOK_OR)
                    {
                        ExprOp prev = node;
                        node = new ExprOp(OpKindEnum.O_OR);
                        node.Left = prev;
                        node.Right = ParseAndExpr(tokContext);
                        if (node.Right == null)
                            throw new ParseError(String.Format(ParseError.ParseError_OperatorNotFollowedByArgument, tok.Symbol()));
                    }
                    else
                    {
                        Lexer.PushToken(tok);
                        break;
                    }

                }
                return node;
            }
            return null; // new ExprOp();
        }

        public ExprOp ParseQueryExpr(QueryLexerTokenKind tokContext, bool subExpression = false)
        {
            ExprOp limiter = null;

            ExprOp next;
            while ((next = ParseOrExpr(tokContext)) != null)
            {
                if (limiter == null)
                {
                    limiter = next;
                }
                else
                {
                    ExprOp prev = limiter;
                    limiter  = new ExprOp(OpKindEnum.O_OR);
                    limiter.Left = prev;
                    limiter.Right = next;
                }
            }

            if (!subExpression)
            {
                if (limiter != null)
                    QueryMap.Add(QueryKindEnum.QUERY_LIMIT, new Predicate(limiter, WhatToKeep).PrintToStr());

                QueryLexerToken tok = Lexer.PeekToken(tokContext);
                while(tok.Kind != QueryLexerTokenKind.END_REACHED)
                {
                    switch(tok.Kind)
                    {
                        case QueryLexerTokenKind.TOK_SHOW:
                        case QueryLexerTokenKind.TOK_ONLY:
                        case QueryLexerTokenKind.TOK_BOLD:
                            {
                                Lexer.NextToken(tokContext);

                                QueryKindEnum kind;
                                switch(tok.Kind)
                                {
                                    case QueryLexerTokenKind.TOK_SHOW:
                                        kind = QueryKindEnum.QUERY_SHOW;
                                        break;
                                    case QueryLexerTokenKind.TOK_ONLY:
                                        kind = QueryKindEnum.QUERY_ONLY;
                                        break;
                                    case QueryLexerTokenKind.TOK_BOLD:
                                        kind = QueryKindEnum.QUERY_BOLD;
                                        break;
                                    default:
                                        throw new InvalidOperationException("To avoid using unassigned QueryKindEnum kind...");
                                }

                                ExprOp node = next = null;
                                while ((next = ParseOrExpr(tokContext)) != null)
                                {
                                    if (node == null)
                                    {
                                        node = next;
                                    }
                                    else
                                    {
                                        ExprOp prev = node;
                                        node = new ExprOp(OpKindEnum.O_OR);
                                        node.Left = prev;
                                        node.Right = next;
                                    }
                                }

                                if (node != null)
                                    QueryMap.Add(kind, new Predicate(node, WhatToKeep).PrintToStr());

                                break;
                            }

                        case QueryLexerTokenKind.TOK_FOR:
                        case QueryLexerTokenKind.TOK_SINCE:
                        case QueryLexerTokenKind.TOK_UNTIL:
                            {
                                tok = Lexer.NextToken(tokContext);

                                string forString = string.Empty;

                                if (tok.Kind == QueryLexerTokenKind.TOK_SINCE)
                                    forString = "since";
                                else if (tok.Kind == QueryLexerTokenKind.TOK_UNTIL)
                                    forString = "until";

                                Lexer.ConsumeNextArg = true;
                                tok = Lexer.PeekToken(tokContext);

                                while(tok.Kind != QueryLexerTokenKind.END_REACHED)
                                {
                                    tok = Lexer.NextToken(tokContext);
                                    if (tok.Kind != QueryLexerTokenKind.TERM)
                                        throw new InvalidOperationException();

                                    if (tok.Value == "show" || tok.Value == "bold" || tok.Value == "for" || tok.Value == "since" || tok.Value == "until")
                                    {
                                        Lexer.SetPrev();
                                        Lexer.ConsumeNextArg = false;
                                        break;
                                    }

                                    if (!String.IsNullOrEmpty(forString))
                                        forString += " ";
                                    forString += tok.Value;

                                    Lexer.ConsumeNextArg = true;
                                    tok = Lexer.PeekToken(tokContext);
                                }

                                if (!String.IsNullOrEmpty(forString))
                                    QueryMap.Add(QueryKindEnum.QUERY_FOR, forString);
                                break;
                            }

                        default:
                            goto done;

                    }

                    tok = Lexer.PeekToken(tokContext);
                }

            done: ;

            }

            return limiter;
        }

    }
}
