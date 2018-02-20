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
using NLedger.Utility;
using System.IO;
using NLedger.Times;
using NLedger.Amounts;
using NLedger.Textual;
using NLedger.Values;

namespace NLedger.Expressions
{
    /// <summary>
    /// Ported from expr_t::token_t (token.h)
    /// </summary>
    public class ExprToken
    {
        public const char InverseOne = (char)255;  // -1

        public static string KindToString(ExprTokenKind kind)
        {
            string result;
            KindToStringValues.TryGetValue(kind, out result);
            return result ?? String.Empty;
        }

        public static string TokenToString(ExprToken token)
        {
            switch(token.Kind)
            {
                case ExprTokenKind.VALUE:
                    return String.Format("<value '{0}'>", token.Value);
                case ExprTokenKind.IDENT:
                    return String.Format("<ident '{0}'>", token.Value);
                case ExprTokenKind.MASK:
                    return String.Format("<mask '{0}'>", token.Value);
                default:
                    return KindToString(token.Kind);
            }
        }

        public ExprToken()
        { }

        public ExprToken(ExprTokenKind kind, Value value)
            : this()
        {
            Kind = kind;
            Value = value;
        }

        public ExprTokenKind Kind { get; private set; }
        public string Symbol { get; private set; }
        public int Length { get; private set; }
        public Value Value { get; private set; }

        public void Clear()
        {
            Kind = ExprTokenKind.UNKNOWN;
            Length = 0;
            Value = Value.Empty;
            Symbol = string.Empty;
        }

        public int ParseReservedWord(InputTextStream inStream)
        {
            char c = inStream.Peek;
            if (Array.IndexOf(ReservedWordLetters, c) >= 0)
            {
                string buf;
                Length = inStream.ReadInto(out buf, out c, ch => char.IsLetter(ch));

                if (buf == "and")
                {
                    Symbol = "&";
                    Kind = ExprTokenKind.KW_AND;
                    return 1;
                }

                if (buf == "div")
                {
                    Symbol = "/";
                    Kind = ExprTokenKind.KW_DIV;
                    return 1;
                }

                if (buf == "else")
                {
                    Symbol = "else";
                    Kind = ExprTokenKind.KW_ELSE;
                    return 1;
                }

                if (buf == "false")
                {
                    Symbol = "false";
                    Kind = ExprTokenKind.VALUE;
                    Value = Value.False;
                    return 1;
                }

                if (buf == "if")
                {
                    Symbol = "if";
                    Kind = ExprTokenKind.KW_IF;
                    return 1;
                }

                if (buf == "or")
                {
                    Symbol = "|";
                    Kind = ExprTokenKind.KW_OR;
                    return 1;
                }

                if (buf == "not")
                {
                    Symbol = "!";
                    Kind = ExprTokenKind.EXCLAM;
                    return 1;
                }

                if (buf == "true")
                {
                    Symbol = "true";
                    Kind = ExprTokenKind.VALUE;
                    Value = Value.True;
                    return 1;
                }

                return 0;
            }
            return -1;
        }

        public void ParseIdent(InputTextStream inStream)
        {
            Kind = ExprTokenKind.IDENT;

            string buf;
            char c;
            Length = inStream.ReadInto(out buf, out c, ch => char.IsLetterOrDigit(ch) || ch == '_');

            Value = Value.Get(buf);
        }

        /// <summary>
        /// Ported from void expr_t::token_t::next(std::istream& in, const parse_flags_t& pflags)
        /// </summary>
        public void Next(InputTextStream inStream, AmountParseFlagsEnum pflags)
        {
            if (inStream.Eof)
            {
                Kind = ExprTokenKind.TOK_EOF;
                return;
            }

            char c = inStream.PeekNextNonWS();

            if (inStream.Eof)
            {
                Kind = ExprTokenKind.TOK_EOF;
                return;
            }

            Symbol = new string(c, 1);
            Length = 1;

            switch(c)
            {
                case '&':
                    inStream.Get();
                    c = inStream.Peek;
                    if (c == '&')
                    {
                        c = inStream.Get();
                        Length = 2;
                    }
                    Kind = ExprTokenKind.KW_AND;
                    break;

                case '|':
                    inStream.Get();
                    c = inStream.Peek;
                    if (c == '|')
                    {
                        c = inStream.Get();
                        Length = 2;
                    }
                    Kind = ExprTokenKind.KW_OR;
                    break;

                case '(':
                    c = inStream.Get();
                    Kind = ExprTokenKind.LPAREN;
                    break;

                case ')':
                    c = inStream.Get();
                    Kind = ExprTokenKind.RPAREN;
                    break;

                case '[':
                    {
                        c = inStream.Get();
                        string buf;
                        Length += inStream.ReadInto(out buf, out c, ch => ch != ']');
                        if (c != ']')
                            Expected(']', c);

                        c = inStream.Get();
                        Length++;

                        DateInterval timespan = new DateInterval(buf);
                        Date? begin = timespan.Begin;
                        if (!begin.HasValue)
                            throw new ParseError(ParseError.ParseError_DateSpecifierDoesNotReferToAStartingDate);
                        Kind = ExprTokenKind.VALUE;
                        Value = Value.Get(begin.Value);
                        break;
                    }

                case '\'':
                case '"':
                    {
                        char delim = inStream.Get();
                        string buf;
                        Length += inStream.ReadInto(out buf, out c, ch => ch != delim);
                        if (c != delim)
                            Expected(delim, c);
                        c = inStream.Get();
                        Length++;
                        Kind = ExprTokenKind.VALUE;
                        Value = Value.Get(buf);
                        break;
                    }

                case '{':
                    {
                        c = inStream.Get();
                        Amount temp = new Amount();
                        temp.Parse(inStream, AmountParseFlagsEnum.PARSE_NO_MIGRATE);
                        c = inStream.Get();
                        if (c != '}')
                            Expected('}', c);
                        Length++;
                        Kind = ExprTokenKind.VALUE;
                        Value = Value.Get(temp);
                        break;
                    }

                case '!':
                    inStream.Get();
                    c = inStream.Peek;
                    if (c == '=')
                    {
                        c = inStream.Get();
                        Symbol = "!=";
                        Kind = ExprTokenKind.NEQUAL;
                        Length = 2;
                        break;
                    }
                    else if (c == '~')
                    {
                        c = inStream.Get();
                        Symbol = "!~";
                        Kind = ExprTokenKind.NMATCH;
                        Length = 2;
                        break;
                    }
                    Kind = ExprTokenKind.EXCLAM;
                    break;

                case '-':
                    inStream.Get();
                    c = inStream.Peek;
                    if (c == '>')
                    {
                        c = inStream.Get();
                        Symbol = "->";
                        Kind = ExprTokenKind.ARROW;
                        Length = 2;
                        break;
                    }
                    Kind = ExprTokenKind.MINUS;
                    break;

                case '+':
                    c = inStream.Get();
                    Kind = ExprTokenKind.PLUS;
                    break;

                case '*':
                    c = inStream.Get();
                    Kind = ExprTokenKind.STAR;
                    break;

                case '?':
                    c = inStream.Get();
                    Kind = ExprTokenKind.QUERY;
                    break;

                case ':':
                    inStream.Get();
                    c = inStream.Peek;
                    Kind = ExprTokenKind.COLON;
                    break;

                case '/':
                    {
                        c = inStream.Get();
                        if (pflags.HasFlag(AmountParseFlagsEnum.PARSE_OP_CONTEXT))
                        {
                            // operator context
                            Kind = ExprTokenKind.SLASH;
                        }
                        else
                        {
                            // terminal context
                            // Read in the regexp
                            string buf;
                            Length += inStream.ReadInto(out buf, out c, ch => ch != '/');
                            if (c != '/')
                                Expected('/', c);
                            c = inStream.Get();
                            Length++;

                            Kind = ExprTokenKind.VALUE;
                            Value = Value.Get(new Mask(buf));
                        }
                        break;
                    }

                case '=':
                    {
                        inStream.Get();
                        c = inStream.Peek;
                        if (c == '~')
                        {
                            c = inStream.Get();
                            Symbol = "=~";
                            Kind = ExprTokenKind.MATCH;
                            Length = 2;
                            break;
                        }
                        if (c == '=')
                        {
                            c = inStream.Get();
                            Symbol = "==";
                            Kind = ExprTokenKind.EQUAL;
                            Length = 2;
                            break;
                        }
                        Kind = ExprTokenKind.ASSIGN;
                        break;
                    }

                case '<':
                    c = inStream.Get();
                    if (inStream.Peek == '=')
                    {
                        c = inStream.Get();
                        Symbol = "<=";
                        Kind = ExprTokenKind.LESSEQ;
                        Length = 2;
                        break;
                    }
                    Kind = ExprTokenKind.LESS;
                    break;

                case '>':
                    c = inStream.Get();
                    if (inStream.Peek == '=')
                    {
                        c = inStream.Get();
                        Symbol = ">=";
                        Kind = ExprTokenKind.GREATEREQ;
                        Length = 2;
                        break;
                    }
                    Kind = ExprTokenKind.GREATER;
                    break;

                case '.':
                    c = inStream.Get();
                    Kind = ExprTokenKind.DOT;
                    break;

                case ',':
                    c = inStream.Get();
                    Kind = ExprTokenKind.COMMA;
                    break;

                case ';':
                    c = inStream.Get();
                    Kind = ExprTokenKind.SEMI;
                    break;

                default:
                    {
                        int pos = inStream.Pos;

                        // First, check to see if it's a reserved word, such as: and or not
                        int result = ParseReservedWord(inStream);
                        if (char.IsLetter(c) && result == 1)
                            break;

                        // TODO - insteam - stack of positions (push\pop); ParseReservedWord - return enum

                        // If not, rewind back to the beginning of the word to scan it
                        // again.  If the result was -1, it means no identifier was scanned
                        // so we don't have to rewind.
                        if (result == 0 || inStream.Eof)
                            inStream.Pos = pos;

                        if (inStream.Eof)
                            throw new InvalidOperationException("eof");

                        // When in relaxed parsing mode, we want to migrate commodity flags
                        // so that any precision specified by the user updates the current
                        // maximum displayed precision.

                        AmountParseFlagsEnum parseFlags = AmountParseFlagsEnum.PARSE_NO_ANNOT;
                        if (pflags.HasFlag(AmountParseFlagsEnum.PARSE_NO_MIGRATE))
                            parseFlags |= AmountParseFlagsEnum.PARSE_NO_MIGRATE;
                        if (pflags.HasFlag(AmountParseFlagsEnum.PARSE_NO_REDUCE))
                            parseFlags |= AmountParseFlagsEnum.PARSE_NO_REDUCE;

                        try
                        {
                            Amount temp = new Amount();
                            if (!temp.Parse(inStream, parseFlags | AmountParseFlagsEnum.PARSE_SOFT_FAIL))
                            {
                                inStream.Pos = pos;

                                c = inStream.Peek;
                                if (c != InputTextStream.EndOfFileChar)
                                {
                                    if (!char.IsLetter(c) && c != '_')
                                        Expected(default(char), c);
                                    ParseIdent(inStream);
                                }
                                else
                                {
                                    throw new ParseError(ParseError.ParseError_UnexpectedEOF);
                                }

                                if (Value.Type != ValueTypeEnum.String || Value.IsZero)
                                {
                                    Kind = ExprTokenKind.ERROR;
                                    Symbol = new string(c, 1);
                                    throw new ParseError(ParseError.ParseError_FailedToParseIdentifier);
                                }
                            }
                            else
                            {
                                Kind = ExprTokenKind.VALUE;
                                Value = Value.Get(temp);
                                Length = inStream.Pos - pos;
                            }
                        }
                        catch
                        {
                            Kind = ExprTokenKind.ERROR;
                            Length = inStream.Pos - pos;
                            throw;
                        }
                        break;
                    }
            }
        }

        public void Expected(char wanted, char c = default(Char))
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
                    throw new ParseError(String.Format(ParseError.ParseError_InvalidChar, c, wanted));
                else
                    throw new ParseError(String.Format(ParseError.ParseError_InvalidCharWanted, c, wanted));
            }
        }

        public void Expected(ExprTokenKind wanted)
        {
            Kind = ExprTokenKind.ERROR;
            if (wanted == ExprTokenKind.ERROR || wanted == ExprTokenKind.UNKNOWN)
                throw new ParseError(String.Format(ParseError.ParseError_InvalidToken, this));
            else
                throw new ParseError(String.Format(ParseError.ParseError_InvalidTokenWanted, this, wanted));
        }

        public void Unexpected(char wanted)
        {
            ExprTokenKind prevKind = Kind;
            Kind = ExprTokenKind.ERROR;

            if (wanted == default(char))
            {
                switch(prevKind)
                {
                    case ExprTokenKind.TOK_EOF:
                        throw new ParseError(ParseError.ParseError_UnexpectedEndOfExpression);
                    case ExprTokenKind.IDENT:
                        throw new ParseError(String.Format(ParseError.ParseError_UnexpectedSymbol, Value));
                    case ExprTokenKind.VALUE:
                        throw new ParseError(String.Format(ParseError.ParseError_UnexpectedValue, Value));
                    default:
                        throw new ParseError(String.Format(ParseError.ParseError_UnexpectedExpressionToken, Symbol));
                }
            }
            else
            {
                switch(prevKind)
                {
                    case ExprTokenKind.TOK_EOF:
                        throw new ParseError(String.Format(ParseError.ParseError_UnexpectedEndOfExpressionWanted, wanted));
                    case ExprTokenKind.IDENT:
                        throw new ParseError(String.Format(ParseError.ParseError_UnexpectedSymbolWanted, Value, wanted));
                    case ExprTokenKind.VALUE:
                        throw new ParseError(String.Format(ParseError.ParseError_UnexpectedValueWanted, Value, wanted));
                    default:
                        throw new ParseError(String.Format(ParseError.ParseError_UnexpectedExpressionTokenWanted, Symbol, wanted));
                }
            }
        }

        public void Rewind(InputTextStream inStream)
        {
            inStream.Pos = inStream.Pos - Length;
        }

        public override string ToString()
        {
            return TokenToString(this);
        }
    
        private static char[] ReservedWordLetters = new char[] { 'a', 'd', 'e', 'f', 'i', 'o', 'n', 't' };
        private static IDictionary<ExprTokenKind, string> KindToStringValues = new Dictionary<ExprTokenKind, string>()
        {
            { ExprTokenKind.ERROR, "<error token>" },


            { ExprTokenKind.VALUE, "<value>" },
            { ExprTokenKind.IDENT, "<identifier>" },
            { ExprTokenKind.MASK, "<regex mask>" },

            { ExprTokenKind.LPAREN, "(" },
            { ExprTokenKind.RPAREN, ")" },
            { ExprTokenKind.LBRACE, "{" },
            { ExprTokenKind.RBRACE, "}" },

            { ExprTokenKind.EQUAL, "==" },
            { ExprTokenKind.NEQUAL, "!=" },
            { ExprTokenKind.LESS, "<" },
            { ExprTokenKind.LESSEQ, "<=" },
            { ExprTokenKind.GREATER, ">" },
            { ExprTokenKind.GREATEREQ, ">=" },

            { ExprTokenKind.ASSIGN, "=" },
            { ExprTokenKind.MATCH, "=~" },
            { ExprTokenKind.NMATCH, "!~" },
            { ExprTokenKind.MINUS, "-" },
            { ExprTokenKind.PLUS, "+" },
            { ExprTokenKind.STAR, "*" },
            { ExprTokenKind.SLASH, "/" },
            { ExprTokenKind.ARROW, "->" },
            { ExprTokenKind.KW_DIV, "div" },

            { ExprTokenKind.EXCLAM, "!" },
            { ExprTokenKind.KW_AND, "and" },
            { ExprTokenKind.KW_OR, "or" },
            { ExprTokenKind.KW_MOD, "mod" },

            { ExprTokenKind.KW_IF, "if" },
            { ExprTokenKind.KW_ELSE, "else" },

            { ExprTokenKind.QUERY, "?" },
            { ExprTokenKind.COLON, ":" },

            { ExprTokenKind.DOT, "." },
            { ExprTokenKind.COMMA, "," },
            { ExprTokenKind.SEMI, ";" },

            { ExprTokenKind.TOK_EOF, "<end of input>" },
            { ExprTokenKind.UNKNOWN, "<unknown>" }
        };

    }
}
