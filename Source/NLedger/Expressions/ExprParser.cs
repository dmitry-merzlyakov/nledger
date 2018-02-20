// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using NLedger.Textual;
using NLedger.Utility;
using NLedger.Utils;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Expressions
{
    /// <summary>
    /// Ported from expr_t::parser_t (parser.h)
    /// </summary>
    public class ExprParser
    {
        public ExprParser()
        {
            Lookahead = new ExprToken();
        }

        public ExprToken Lookahead { get; private set; }
        public bool UseLookahead { get; private set; }

        public ExprToken NextToken(InputTextStream inStream, AmountParseFlagsEnum tFlags, ExprTokenKind? expecting = null)
        {
            if (UseLookahead)
                UseLookahead = false;
            else
                Lookahead.Next(inStream, tFlags);

            if (expecting.HasValue && Lookahead.Kind != expecting.Value)
                Lookahead.Expected(expecting.Value);

            return Lookahead;
        }

        public ExprOp Parse(InputTextStream inStream, AmountParseFlagsEnum flags = AmountParseFlagsEnum.PARSE_DEFAULT, string originalString = null)
        {
            try
            {
                ExprOp topNode = ParseValueExpr(inStream, flags);

                if (UseLookahead)
                {
                    UseLookahead = false;
                    Lookahead.Rewind(inStream);
                }
                Lookahead.Clear();

                return topNode;
            }
            catch
            {
                if (!String.IsNullOrEmpty(originalString))
                {
                    ErrorContext.Current.AddErrorContext("While parsing value expression:");
                    int endPos = inStream.Pos;
                    int pos = endPos > 0 ? endPos - Lookahead.Length : 0;

                    Logger.Current.Debug("parser.error", () => String.Format("original_string = '{0}'", originalString));
                    Logger.Current.Debug("parser.error", () => String.Format("            pos = {0}", pos));
                    Logger.Current.Debug("parser.error", () => String.Format("        end_pos = {0}", endPos));
                    Logger.Current.Debug("parser.error", () => String.Format("     token kind = {0}", Lookahead.Kind));
                    Logger.Current.Debug("parser.error", () => String.Format("   token length = {0}", Lookahead.Length));

                    ErrorContext.Current.AddErrorContext(ErrorContext.LineContext(originalString, pos, endPos));
                }
                throw;
            }
        }

        public void PushToken(ExprToken tok)
        {
            UseLookahead = true;
        }

        public void PushToken()
        {
            UseLookahead = true;
        }

        public ExprOp ParseValueTerm(InputTextStream inStream, AmountParseFlagsEnum tFlags)
        {
            ExprOp node = null;
            ExprToken tok = NextToken(inStream, tFlags);

            switch (tok.Kind)
            {
                case ExprTokenKind.VALUE:
                    node = new ExprOp(OpKindEnum.VALUE);
                    node.AsValue = tok.Value;
                    break;

                case ExprTokenKind.IDENT:
                    string ident = tok.Value.AsString;

                    node = new ExprOp(OpKindEnum.IDENT);
                    node.AsIdent = ident;
                    break;

                case ExprTokenKind.LPAREN:
                    node = ParseValueExpr(inStream, (tFlags | AmountParseFlagsEnum.PARSE_PARTIAL) & ~AmountParseFlagsEnum.PARSE_SINGLE);
                    tok = NextToken(inStream, tFlags, ExprTokenKind.RPAREN);
                    break;

                default:
                    PushToken(tok);
                    break;
            }

            return node;
        }

        public ExprOp ParseCallExpr(InputTextStream inStream, AmountParseFlagsEnum tFlags)
        {
            ExprOp node = ParseValueTerm(inStream, tFlags);

            if (node != null && !tFlags.HasFlag(AmountParseFlagsEnum.PARSE_SINGLE))
            {
                while(true)
                {
                    ExprToken tok = NextToken(inStream, tFlags | AmountParseFlagsEnum.PARSE_OP_CONTEXT);
                    if (tok.Kind == ExprTokenKind.LPAREN)
                    {
                        ExprOp prev = node;
                        node = new ExprOp(OpKindEnum.O_CALL);
                        node.Left = prev;
                        PushToken(tok);  // let the parser see the '(' again
                        node.Right = ParseValueExpr(inStream, tFlags | AmountParseFlagsEnum.PARSE_SINGLE);
                    }
                    else
                    {
                        PushToken(tok);
                        break;
                    }
                }
            }

            return node;
        }

        public ExprOp ParseDotExpr(InputTextStream inStream, AmountParseFlagsEnum tFlags)
        {
            ExprOp node = ParseCallExpr(inStream, tFlags);

            if (node != null && !tFlags.HasFlag(AmountParseFlagsEnum.PARSE_SINGLE))
            {
                while (true)
                {
                    ExprToken tok = NextToken(inStream, tFlags | AmountParseFlagsEnum.PARSE_OP_CONTEXT);
                    if (tok.Kind == ExprTokenKind.DOT)
                    {
                        ExprOp prev = node;
                        node = new ExprOp(OpKindEnum.O_LOOKUP);
                        node.Left = prev;
                        node.Right = ParseCallExpr(inStream, tFlags);
                        if (node.Right == null)
                            throw new ParseError(String.Format(ParseError.ParseError_OperatorNotFollowedByArgument, tok.Symbol));
                    }
                    else
                    {
                        PushToken(tok);
                        break;
                    }
                }
            }

            return node;
        }

        public ExprOp ParseUnaryExpr(InputTextStream inStream, AmountParseFlagsEnum tFlags)
        {
            ExprOp node = null;
            ExprToken tok = NextToken(inStream, tFlags);

            switch(tok.Kind)
            {
                case ExprTokenKind.EXCLAM:
                    {
                        ExprOp term = ParseDotExpr(inStream, tFlags);
                        if (term == null)
                            throw new ParseError(String.Format(ParseError.ParseError_OperatorNotFollowedByArgument, tok.Symbol));

                        // A very quick optimization
                        if (term.Kind == OpKindEnum.VALUE)
                        {
                            term.AsValue.InPlaceNot();
                            node = term;
                        }
                        else
                        {
                            node = new ExprOp(OpKindEnum.O_NOT);
                            node.Left = term;
                        }
                        break;
                    }

                case ExprTokenKind.MINUS:
                    {
                        ExprOp term = ParseDotExpr(inStream, tFlags);
                        if (term == null)
                            throw new ParseError(String.Format(ParseError.ParseError_OperatorNotFollowedByArgument, tok.Symbol));

                        // A very quick optimization
                        if (term.Kind == OpKindEnum.VALUE)
                        {
                            term.AsValue.InPlaceNegate();
                            node = term;
                        }
                        else
                        {
                            node = new ExprOp(OpKindEnum.O_NEG);
                            node.Left = term;
                        }
                        break;
                    }

                default:
                    PushToken(tok);
                    node = ParseDotExpr(inStream, tFlags);
                    break;
            }

            return node;
        }

        public ExprOp ParseMulExpr(InputTextStream inStream, AmountParseFlagsEnum tFlags)
        {
            ExprOp node = ParseUnaryExpr(inStream, tFlags);

            if (node != null && !tFlags.HasFlag(AmountParseFlagsEnum.PARSE_SINGLE))
            {
                while (true)
                {
                    ExprToken tok = NextToken(inStream, tFlags | AmountParseFlagsEnum.PARSE_OP_CONTEXT);
                    if (tok.Kind == ExprTokenKind.STAR || tok.Kind == ExprTokenKind.SLASH || tok.Kind == ExprTokenKind.KW_DIV)
                    {
                        ExprOp prev = node;
                        node = new ExprOp(tok.Kind == ExprTokenKind.STAR ? OpKindEnum.O_MUL : OpKindEnum.O_DIV);
                        node.Left = prev;
                        node.Right = ParseUnaryExpr(inStream, tFlags);
                        if (node.Right == null)
                            throw new ParseError(String.Format(ParseError.ParseError_OperatorNotFollowedByArgument, tok.Symbol));
                    }
                    else
                    {
                        PushToken(tok);
                        break;
                    }
                }
            }

            return node;
        }

        public ExprOp ParseAddExpr(InputTextStream inStream, AmountParseFlagsEnum tFlags)
        {
            ExprOp node = ParseMulExpr(inStream, tFlags);

            if (node != null && !tFlags.HasFlag(AmountParseFlagsEnum.PARSE_SINGLE))
            {
                while (true)
                {
                    ExprToken tok = NextToken(inStream, tFlags | AmountParseFlagsEnum.PARSE_OP_CONTEXT);
                    if (tok.Kind == ExprTokenKind.PLUS || tok.Kind == ExprTokenKind.MINUS)
                    {
                        ExprOp prev = node;
                        node = new ExprOp(tok.Kind == ExprTokenKind.PLUS ? OpKindEnum.O_ADD : OpKindEnum.O_SUB);
                        node.Left = prev;
                        node.Right = ParseMulExpr(inStream, tFlags);
                        if (node.Right == null)
                            throw new ParseError(String.Format(ParseError.ParseError_OperatorNotFollowedByArgument, tok.Symbol));
                    }
                    else
                    {
                        PushToken(tok);
                        break;
                    }
                }
            }

            return node;
        }

        public ExprOp ParseLogicExpr(InputTextStream inStream, AmountParseFlagsEnum tFlags)
        {
            ExprOp node = ParseAddExpr(inStream, tFlags);

            if (node != null && !tFlags.HasFlag(AmountParseFlagsEnum.PARSE_SINGLE))
            {
                while (true)
                {
                    OpKindEnum kind = OpKindEnum.LAST;
                    AmountParseFlagsEnum flags = tFlags;
                    ExprToken tok = NextToken(inStream, tFlags | AmountParseFlagsEnum.PARSE_OP_CONTEXT);
                    bool negate = false;

                    switch(tok.Kind)
                    {
                        case ExprTokenKind.EQUAL:
                            if (tFlags.HasFlag(AmountParseFlagsEnum.PARSE_NO_ASSIGN))
                                tok.Rewind(inStream);
                            else
                                kind = OpKindEnum.O_EQ;
                            break;

                        case ExprTokenKind.NEQUAL:
                            kind = OpKindEnum.O_EQ;
                            negate = true;
                            break;

                        case ExprTokenKind.MATCH:
                            kind = OpKindEnum.O_MATCH;
                            break;

                        case ExprTokenKind.NMATCH:
                            kind = OpKindEnum.O_MATCH;
                            negate = true;
                            break;

                        case ExprTokenKind.LESS:
                            kind = OpKindEnum.O_LT;
                            break;

                        case ExprTokenKind.LESSEQ:
                            kind = OpKindEnum.O_LTE;
                            break;

                        case ExprTokenKind.GREATER:
                            kind = OpKindEnum.O_GT;
                            break;

                        case ExprTokenKind.GREATEREQ:
                            kind = OpKindEnum.O_GTE;
                            break;

                        default:
                            PushToken(tok);
                            goto exitLoop;
                    }

                    if (kind != OpKindEnum.LAST)
                    {
                        ExprOp prev = node;
                        node = new ExprOp(kind);
                        node.Left = prev;
                        node.Right = ParseAddExpr(inStream, flags);
                        
                        if (node.Right == null)
                            throw new ParseError(String.Format(ParseError.ParseError_OperatorNotFollowedByArgument, tok.Symbol));

                        if (negate)
                        {
                            prev = node;
                            node = new ExprOp(OpKindEnum.O_NOT);
                            node.Left = prev;
                        }
                    }
                }
            }

            exitLoop:
            return node;
        }

        public ExprOp ParseAndExpr(InputTextStream inStream, AmountParseFlagsEnum tFlags)
        {
            ExprOp node = ParseLogicExpr(inStream, tFlags);

            if (node != null && !tFlags.HasFlag(AmountParseFlagsEnum.PARSE_SINGLE))
            {
                while (true)
                {
                    ExprToken tok = NextToken(inStream, tFlags | AmountParseFlagsEnum.PARSE_OP_CONTEXT);
                    if (tok.Kind == ExprTokenKind.KW_AND)
                    {
                        ExprOp prev = node;
                        node = new ExprOp(OpKindEnum.O_AND);
                        node.Left = prev;
                        node.Right = ParseLogicExpr(inStream, tFlags);
                        if (node.Right == null)
                            throw new ParseError(String.Format(ParseError.ParseError_OperatorNotFollowedByArgument, tok.Symbol));
                    }
                    else
                    {
                        PushToken(tok);
                        break;
                    }
                }
            }

            return node;
        }

        public ExprOp ParseOrExpr(InputTextStream inStream, AmountParseFlagsEnum tFlags)
        {
            ExprOp node = ParseAndExpr(inStream, tFlags);

            if (node != null && !tFlags.HasFlag(AmountParseFlagsEnum.PARSE_SINGLE))
            {
                while (true)
                {
                    ExprToken tok = NextToken(inStream, tFlags | AmountParseFlagsEnum.PARSE_OP_CONTEXT);
                    if (tok.Kind == ExprTokenKind.KW_OR)
                    {
                        ExprOp prev = node;
                        node = new ExprOp(OpKindEnum.O_OR);
                        node.Left = prev;
                        node.Right = ParseAndExpr(inStream, tFlags);
                        if (node.Right == null)
                            throw new ParseError(String.Format(ParseError.ParseError_OperatorNotFollowedByArgument, tok.Symbol));
                    }
                    else
                    {
                        PushToken(tok);
                        break;
                    }
                }
            }

            return node;
        }

        public ExprOp ParseQuerycolonExpr(InputTextStream inStream, AmountParseFlagsEnum tFlags)
        {
            ExprOp node = ParseOrExpr(inStream, tFlags);

            if (node != null && !tFlags.HasFlag(AmountParseFlagsEnum.PARSE_SINGLE))
            {
                ExprToken tok = NextToken(inStream, tFlags |= AmountParseFlagsEnum.PARSE_OP_CONTEXT);

                if (tok.Kind == ExprTokenKind.QUERY)
                {
                    ExprOp prev = node;
                    node = new ExprOp(OpKindEnum.O_QUERY);
                    node.Left = prev;
                    node.Right = ParseOrExpr(inStream, tFlags);
                    if (node.Right == null)
                        throw new ParseError(String.Format(ParseError.ParseError_OperatorNotFollowedByArgument, tok.Symbol));

                    NextToken(inStream, tFlags |= AmountParseFlagsEnum.PARSE_OP_CONTEXT, ExprTokenKind.COLON);
                    prev = node.Right;
                    ExprOp subNode = new ExprOp(OpKindEnum.O_COLON);
                    subNode.Left = prev;
                    subNode.Right = ParseOrExpr(inStream, tFlags);
                    if (subNode.Right == null)
                        throw new ParseError(String.Format(ParseError.ParseError_OperatorNotFollowedByArgument, tok.Symbol));

                    node.Right = subNode;
                }
                else if (tok.Kind == ExprTokenKind.KW_IF)
                {
                    ExprOp ifOp = ParseOrExpr(inStream, tFlags);
                    if (ifOp == null)
                        throw new ParseError("'if' keyword not followed by argument");

                    tok = NextToken(inStream, tFlags |= AmountParseFlagsEnum.PARSE_OP_CONTEXT);
                    if (tok.Kind == ExprTokenKind.KW_ELSE)
                    {
                        ExprOp elseOp = ParseOrExpr(inStream, tFlags);
                        if (elseOp == null)
                            throw new ParseError("'else' keyword not followed by argument");

                        ExprOp subNode = new ExprOp(OpKindEnum.O_COLON);
                        subNode.Left = node;
                        subNode.Right = elseOp;

                        node = new ExprOp(OpKindEnum.O_QUERY);
                        node.Left = ifOp;
                        node.Right = subNode;
                    }
                    else
                    {
                        ExprOp nullNode = new ExprOp(OpKindEnum.VALUE);
                        nullNode.AsValue = Value.Empty;

                        ExprOp subNode = new ExprOp(OpKindEnum.O_COLON);
                        subNode.Left = node;
                        subNode.Right = nullNode;

                        node = new ExprOp(OpKindEnum.O_QUERY);
                        node.Left = ifOp;
                        node.Right = subNode;

                        PushToken(tok);
                    }
                }
                else
                {
                    PushToken(tok);
                }
            }
            return node;
        }

        public ExprOp ParseCommaExpr(InputTextStream inStream, AmountParseFlagsEnum tFlags)
        {
            ExprOp node = ParseQuerycolonExpr(inStream, tFlags);

            if (node != null && !tFlags.HasFlag(AmountParseFlagsEnum.PARSE_SINGLE))
            {
                ExprOp next = null;
                while (true)
                {
                    ExprToken tok = NextToken(inStream, tFlags | AmountParseFlagsEnum.PARSE_OP_CONTEXT);
                    if (tok.Kind == ExprTokenKind.COMMA)
                    {
                        if (next == null)
                        {
                            ExprOp prev = node;
                            node = new ExprOp(OpKindEnum.O_CONS);
                            node.Left = prev;
                            next = node;
                        }

                        ExprToken ntok = NextToken(inStream, tFlags);
                        PushToken(ntok);
                        if (ntok.Kind == ExprTokenKind.RPAREN)
                            break;

                        ExprOp chain = new ExprOp(OpKindEnum.O_CONS);
                        chain.Left = ParseQuerycolonExpr(inStream, tFlags);

                        next.Right = chain;
                        next = chain;
                    }
                    else
                    {
                        PushToken(tok);
                        break;
                    }
                }
            }

            return node;
        }

        public ExprOp ParseLambdaExpr(InputTextStream inStream, AmountParseFlagsEnum tFlags)
        {
            ExprOp node = ParseCommaExpr(inStream, tFlags);

            if (node != null && !tFlags.HasFlag(AmountParseFlagsEnum.PARSE_SINGLE))
            {
                while (true)
                {
                    ExprToken tok = NextToken(inStream, tFlags | AmountParseFlagsEnum.PARSE_OP_CONTEXT);
                    if (tok.Kind == ExprTokenKind.ARROW)
                    {
                        ExprOp prev = node;
                        node = new ExprOp(OpKindEnum.O_LAMBDA);
                        node.Left = prev;
                        ExprOp scope = new ExprOp(OpKindEnum.SCOPE);
                        scope.Left = ParseQuerycolonExpr(inStream, tFlags);
                        node.Right = scope;
                    }
                    else
                    {
                        PushToken(tok);
                        break;
                    }
                }
            }

            return node;
        }

        public ExprOp ParseAssingExpr(InputTextStream inStream, AmountParseFlagsEnum tFlags)
        {
            ExprOp node = ParseLambdaExpr(inStream, tFlags);

            if (node != null && !tFlags.HasFlag(AmountParseFlagsEnum.PARSE_SINGLE))
            {
                while (true)
                {
                    ExprToken tok = NextToken(inStream, tFlags | AmountParseFlagsEnum.PARSE_OP_CONTEXT);
                    if (tok.Kind == ExprTokenKind.ASSIGN)
                    {
                        ExprOp prev = node;
                        node = new ExprOp(OpKindEnum.O_DEFINE);
                        node.Left = prev;
                        ExprOp scope = new ExprOp(OpKindEnum.SCOPE);
                        scope.Left = ParseLambdaExpr(inStream, tFlags);
                        node.Right = scope;
                    }
                    else
                    {
                        PushToken(tok);
                        break;
                    }
                }
            }

            return node;
        }

        public ExprOp ParseValueExpr(InputTextStream inStream, AmountParseFlagsEnum tFlags)
        {
            ExprOp node = ParseAssingExpr(inStream, tFlags);

            if (node != null && !tFlags.HasFlag(AmountParseFlagsEnum.PARSE_SINGLE))
            {
                ExprOp chain = null;
                while (true)
                {
                    ExprToken tok = NextToken(inStream, tFlags | AmountParseFlagsEnum.PARSE_OP_CONTEXT);
                    if (tok.Kind == ExprTokenKind.SEMI)
                    {
                        ExprOp seq = new ExprOp(OpKindEnum.O_SEQ);
                        if (chain == null)
                        {
                            seq.Left = node;
                            node = seq;
                        }
                        else
                        {
                            seq.Left = chain.Right;
                            chain.Right = seq;
                        }
                        seq.Right = ParseAssingExpr(inStream, tFlags);
                        chain = seq;
                    }
                    else
                    {
                        PushToken(tok);
                        break;
                    }
                }
            }

            return node;
        }

    }
}
