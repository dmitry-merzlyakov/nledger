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
    public class DateParserLexer
    {
        /// <summary>
        /// Ported from string_to_month_of_year
        /// </summary>
        public static MonthEnum? StringToMonthsOfYear(string str)
        {
            if (str == ("jan") || str == ("january") || str == "0")
                return MonthEnum.Jan;
            else if (str == ("feb") || str == ("february") || str == "1")
                return MonthEnum.Feb;
            else if (str == ("mar") || str == ("march") || str == "2")
                return MonthEnum.Mar;
            else if (str == ("apr") || str == ("april") || str == "3")
                return MonthEnum.Apr;
            else if (str == ("may") || str == ("may") || str == "4")
                return MonthEnum.May;
            else if (str == ("jun") || str == ("june") || str == "5")
                return MonthEnum.Jun;
            else if (str == ("jul") || str == ("july") || str == "6")
                return MonthEnum.Jul;
            else if (str == ("aug") || str == ("august") || str == "7")
                return MonthEnum.Aug;
            else if (str == ("sep") || str == ("september") || str == "8")
                return MonthEnum.Sep;
            else if (str == ("oct") || str == ("october") || str == "9")
                return MonthEnum.Oct;
            else if (str == ("nov") || str == ("november") || str == "10")
                return MonthEnum.Nov;
            else if (str == ("dec") || str == ("december") || str == "11")
                return MonthEnum.Dec;
            else
                return null;
        }

        public static DayOfWeek? StringToDayOfWeek(string str)
        {
            if (str == ("sun") || str == ("sunday") || str == "0")
                return DayOfWeek.Sunday;
            else if (str == ("mon") || str == ("monday") || str == "1")
                return DayOfWeek.Monday;
            else if (str == ("tue") || str == ("tuesday") || str == "2")
                return DayOfWeek.Tuesday;
            else if (str == ("wed") || str == ("wednesday") || str == "3")
                return DayOfWeek.Wednesday;
            else if (str == ("thu") || str == ("thursday") || str == "4")
                return DayOfWeek.Thursday;
            else if (str == ("fri") || str == ("friday") || str == "5")
                return DayOfWeek.Friday;
            else if (str == ("sat") || str == ("saturday") || str == "6")
                return DayOfWeek.Saturday;
            else
                return null;
        }

        public DateParserLexer(string begin)
        {
            Begin = begin;
        }

        public LexerToken TokenCache { get; set; }
        public string Begin { get; private set; }

        public LexerToken NextToken()
        {
            if (TokenCache != null && TokenCache.Kind != LexerTokenKindEnum.UNKNOWN)
            {
                LexerToken tok = TokenCache;
                TokenCache = new LexerToken();
                return tok;
            }

            Begin = Begin.TrimStart();

            if (String.IsNullOrEmpty(Begin))
                return new LexerToken(LexerTokenKindEnum.END_REACHED);

            switch (Begin[0])
            {
                case '/': return new LexerToken(LexerTokenKindEnum.TOK_SLASH);
                case '-': return new LexerToken(LexerTokenKindEnum.TOK_DASH);
                case '.': return new LexerToken(LexerTokenKindEnum.TOK_DOT);
                default: break;
            }

            // If the first character is a digit, try parsing the whole argument as a
            // date using the typical date formats.  This allows not only dates like
            // "2009/08/01", but also dates that fit the user's --input-date-format,
            // assuming their format fits in one argument and begins with a digit.
            if (Char.IsDigit(Begin[0]))
            {
                int pos = Begin.IndexOf(" ");
                string possibleDate = Begin.Substring(0, pos > 0 ? pos : Begin.Length);

                try
                {
                    DateTraits dateTraits;
                    Date when = TimesCommon.Current.ParseDateMask(possibleDate, out dateTraits);
                    if (!when.IsNotADate())
                    {
                        Begin = Begin.Substring(possibleDate.Length);
                        return new LexerToken(LexerTokenKindEnum.TOK_DATE, new BoostVariant(new DateSpecifier(when, dateTraits)));
                    }

                }
                catch(DateError)
                {
                    if (possibleDate.IndexOfAny(PossibleDateContains) >= 0)
                        throw;
                }
            }

            string start = Begin;

            bool alNul = Char.IsLetterOrDigit(Begin[0]);
            int pos1 = 0;
            while (pos1 < Begin.Length && !Char.IsWhiteSpace(Begin[pos1]) &&
                ((alNul && Char.IsLetterOrDigit(Begin[pos1])) || (!alNul && !Char.IsLetterOrDigit(Begin[pos1])))) pos1++;
            string term = Begin.Substring(0, pos1);
            Begin = Begin.Substring(pos1);

            if (!String.IsNullOrEmpty(term))
            {
                if (Char.IsDigit(term[0]))
                {
                    return new LexerToken(LexerTokenKindEnum.TOK_INT, new BoostVariant(Int32.Parse(term)));
                }
                else if (Char.IsLetter(term[0]))
                {
                    term = term.ToLower();

                    MonthEnum? month = StringToMonthsOfYear(term);
                    if (month != null)
                        return new LexerToken(LexerTokenKindEnum.TOK_A_MONTH, new BoostVariant(month.Value));

                    DayOfWeek? wday = StringToDayOfWeek(term);
                    if (wday != null)
                        return new LexerToken(LexerTokenKindEnum.TOK_A_WDAY, new BoostVariant(wday.Value));

                    LexerTokenKindEnum stringTokenEnum;
                    if (StringTokens.TryGetValue(term, out stringTokenEnum))
                        return new LexerToken(stringTokenEnum);
                }
                else
                {
                    LexerToken.Expected(default(char), term[0]);
                    Begin = start.Substring(1);
                }
            }
            else
            {             
                LexerToken.Expected(default(char), term[0]);
            }

            return new LexerToken(LexerTokenKindEnum.UNKNOWN, new BoostVariant(term));
        }

        public void PushToken(LexerToken tok)
        {
            if (TokenCache.Kind != LexerTokenKindEnum.UNKNOWN)
                throw new InvalidOperationException("Unexpected state");

            TokenCache = tok;
        }

        public LexerToken PeekToken()
        {
            if (TokenCache == null || TokenCache.Kind == LexerTokenKindEnum.UNKNOWN)
                TokenCache = NextToken();

            return TokenCache;
        }

        private static readonly char[] PossibleDateContains = new char[] { '/', '-', '.' };
        private static readonly IDictionary<string, LexerTokenKindEnum> StringTokens = new Dictionary<string, LexerTokenKindEnum>()
        {
            { "ago", LexerTokenKindEnum.TOK_AGO },
            { "hence", LexerTokenKindEnum.TOK_HENCE },
            { "from", LexerTokenKindEnum.TOK_SINCE },
            { "since", LexerTokenKindEnum.TOK_SINCE },
            { "to", LexerTokenKindEnum.TOK_UNTIL },
            { "until", LexerTokenKindEnum.TOK_UNTIL },
            { "in", LexerTokenKindEnum.TOK_IN },
            { "this", LexerTokenKindEnum.TOK_THIS },
            { "next", LexerTokenKindEnum.TOK_NEXT },
            { "last", LexerTokenKindEnum.TOK_LAST },
            { "every", LexerTokenKindEnum.TOK_EVERY },
            { "today", LexerTokenKindEnum.TOK_TODAY },
            { "tomorrow", LexerTokenKindEnum.TOK_TOMORROW },
            { "yesterday", LexerTokenKindEnum.TOK_YESTERDAY },
            { "year", LexerTokenKindEnum.TOK_YEAR },
            { "quarter", LexerTokenKindEnum.TOK_QUARTER },
            { "month", LexerTokenKindEnum.TOK_MONTH },
            { "week", LexerTokenKindEnum.TOK_WEEK },
            { "day", LexerTokenKindEnum.TOK_DAY },
            { "yearly", LexerTokenKindEnum.TOK_YEARLY },
            { "quarterly", LexerTokenKindEnum.TOK_QUARTERLY },
            { "bimonthly", LexerTokenKindEnum.TOK_BIMONTHLY },
            { "monthly", LexerTokenKindEnum.TOK_MONTHLY },
            { "biweekly", LexerTokenKindEnum.TOK_BIWEEKLY },
            { "weekly", LexerTokenKindEnum.TOK_WEEKLY },
            { "daily", LexerTokenKindEnum.TOK_DAILY },
            { "years", LexerTokenKindEnum.TOK_YEARS },
            { "quarters", LexerTokenKindEnum.TOK_QUARTERS },
            { "months", LexerTokenKindEnum.TOK_MONTHS },
            { "weeks", LexerTokenKindEnum.TOK_WEEKS },
            { "days", LexerTokenKindEnum.TOK_DAYS },
        };
    }
}
