// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Times
{
    public class LexerToken : IEquatable<LexerToken>
    {
        public const char InverseOne = (char)255;  // -1

        public static bool operator == (LexerToken x, LexerToken y)
        {
            if (Object.ReferenceEquals(x, null))
                return Object.ReferenceEquals(y, null);

            return x.Equals(y);
        }

        public static bool operator !=(LexerToken x, LexerToken y)
        {
            if (Object.ReferenceEquals(x, null))
                return !Object.ReferenceEquals(y, null);

            return !x.Equals(y);
        }

        public static void Expected(char wanted, char c = default(Char))
        {
            if (c == default(char) || c == InverseOne)
            {
                if (wanted == default(char) || wanted == InverseOne)
                    throw new DateError(DateError.ErrorMessageUnexpectedEnd);
                else
                    throw new DateError(String.Format(DateError.ErrorMessageMissing, wanted));
            }
            else
            {
                if (wanted == default(char) || wanted == InverseOne)
                    throw new DateError(String.Format(DateError.ErrorMessageInvalidChar, c, wanted));
                else
                    throw new DateError(String.Format(DateError.ErrorMessageInvalidCharWanted, c, wanted));
            }
        }

        public LexerToken()
        {
            Value = new BoostVariant(typeof(int), typeof(string), typeof(DateSpecifier), typeof(MonthEnum));
            Kind = LexerTokenKindEnum.UNKNOWN;
        }

        public LexerToken(LexerTokenKindEnum kind) : this (kind, BoostVariant.Empty)
        { }

        public LexerToken(LexerTokenKindEnum kind, BoostVariant value) : this()
        {

            Kind = kind;
            if (value != null && !value.IsEmpty)
                Value.SetValue(value);
        }

        public LexerTokenKindEnum Kind { get; private set; }
        public BoostVariant Value { get; private set; }

        public bool IsNotEnd
        {
            get { return Kind != LexerTokenKindEnum.END_REACHED; }
        }

        public override string ToString()
        {
            switch(Kind)
            {
                case LexerTokenKindEnum.UNKNOWN:
                case LexerTokenKindEnum.TOK_DATE:
                case LexerTokenKindEnum.TOK_INT:
                case LexerTokenKindEnum.TOK_A_MONTH:
                case LexerTokenKindEnum.TOK_A_WDAY:
                    return Value.ToString();

                case LexerTokenKindEnum.TOK_SLASH: return "/";
                case LexerTokenKindEnum.TOK_DASH: return "-";
                case LexerTokenKindEnum.TOK_DOT: return ".";

                case LexerTokenKindEnum.TOK_AGO:
                case LexerTokenKindEnum.TOK_HENCE:
                case LexerTokenKindEnum.TOK_SINCE:
                case LexerTokenKindEnum.TOK_UNTIL:
                case LexerTokenKindEnum.TOK_IN:
                case LexerTokenKindEnum.TOK_THIS:
                case LexerTokenKindEnum.TOK_NEXT:
                case LexerTokenKindEnum.TOK_LAST:
                case LexerTokenKindEnum.TOK_EVERY:
                case LexerTokenKindEnum.TOK_TODAY:
                case LexerTokenKindEnum.TOK_TOMORROW:
                case LexerTokenKindEnum.TOK_YESTERDAY:
                case LexerTokenKindEnum.TOK_YEAR:
                case LexerTokenKindEnum.TOK_QUARTER:
                case LexerTokenKindEnum.TOK_MONTH:
                case LexerTokenKindEnum.TOK_WEEK:
                case LexerTokenKindEnum.TOK_DAY:
                case LexerTokenKindEnum.TOK_YEARLY:
                case LexerTokenKindEnum.TOK_QUARTERLY:
                case LexerTokenKindEnum.TOK_BIMONTHLY:
                case LexerTokenKindEnum.TOK_MONTHLY:
                case LexerTokenKindEnum.TOK_BIWEEKLY:
                case LexerTokenKindEnum.TOK_WEEKLY:
                case LexerTokenKindEnum.TOK_DAILY:
                case LexerTokenKindEnum.TOK_YEARS:
                case LexerTokenKindEnum.TOK_QUARTERS:
                case LexerTokenKindEnum.TOK_MONTHS:
                case LexerTokenKindEnum.TOK_WEEKS:
                case LexerTokenKindEnum.TOK_DAYS:
                    return Kind.ToString().Substring(4).ToLower();

                case LexerTokenKindEnum.END_REACHED:
                    return "<EOF>";

                default:
                    throw new NotSupportedException(String.Format("Unsupported option: {0}", Kind));
            }
        }

        public string Dump()
        {
            return Kind.ToString();
        }

        public void Unexpected()
        {
            switch(Kind)
            {
                case LexerTokenKindEnum.END_REACHED:
                    Kind = LexerTokenKindEnum.UNKNOWN;
                    throw new DateError(DateError.ErrorMessageUnexpectedEndOfExpression);

                default:
                    string desc = ToString();
                    Kind = LexerTokenKindEnum.UNKNOWN;
                    throw new DateError(String.Format(DateError.ErrorMessageUnexpectedDatePeriodToken, desc));
            }
        }

        public bool Equals(LexerToken other)
        {
            if (Object.ReferenceEquals(other, null))
                return false;

            return Kind == other.Kind && Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as LexerToken);
        }

        public override int GetHashCode()
        {
            return Kind.GetHashCode() ^ Value.GetHashCode();
        }
    }
}
