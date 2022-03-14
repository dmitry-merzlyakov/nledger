// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
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
    /// <summary>
    /// Ported from date_parser_t
    /// </summary>
    public class DateParser
    {
        public DateParser(string arg)
        {
            Arg = arg;
            Lexer = new DateParserLexer(arg);
        }

        public string Arg { get; private set; }
        public DateParserLexer Lexer { get; private set; }

        /// <summary>
        /// Ported from date_parser_t::parse()
        /// </summary>
        public DateInterval Parse()
        {
            DateSpecifier sinceSpecifier = null;
            DateSpecifier untilSpecifier = null;
            DateSpecifier inclusionSpecifier = null;

            DateInterval period = new DateInterval();
            Date today = TimesCommon.Current.CurrentDate;
            bool endInclusive = false;

            for(LexerToken tok = Lexer.NextToken(); tok.Kind != LexerTokenKindEnum.END_REACHED; tok = Lexer.NextToken())
            {
                switch(tok.Kind)
                {
                    case LexerTokenKindEnum.TOK_DATE:
                    case LexerTokenKindEnum.TOK_INT:
                    case LexerTokenKindEnum.TOK_A_MONTH:
                    case LexerTokenKindEnum.TOK_A_WDAY:
                        DetermineWhen(ref tok, ref inclusionSpecifier);
                        break;

                    case LexerTokenKindEnum.TOK_DASH:
                        if (inclusionSpecifier != null)
                        {
                            sinceSpecifier = inclusionSpecifier;
                            inclusionSpecifier = null;
                            tok = Lexer.NextToken();
                            DetermineWhen(ref tok, ref untilSpecifier);

                            // The dash operator is special: it has an _inclusive_ end.
                            endInclusive = true;                            
                        }
                        else
                        {
                            tok.Unexpected();
                        }
                        break;

                    case LexerTokenKindEnum.TOK_SINCE:
                        if (sinceSpecifier != null)
                        {
                            tok.Unexpected();
                        }
                        else
                        {
                            tok = Lexer.NextToken();
                            DetermineWhen(ref tok, ref sinceSpecifier);
                        }
                        break;

                    case LexerTokenKindEnum.TOK_UNTIL:
                        if (untilSpecifier != null)
                        {
                            tok.Unexpected();
                        }
                        else
                        {
                            tok = Lexer.NextToken();
                            DetermineWhen(ref tok, ref untilSpecifier);
                        }
                        break;

                    case LexerTokenKindEnum.TOK_IN:
                        if (inclusionSpecifier != null)
                        {
                            tok.Unexpected();
                        }
                        else
                        {
                            tok = Lexer.NextToken();
                            DetermineWhen(ref tok, ref inclusionSpecifier);
                        }
                        break;

                    case LexerTokenKindEnum.TOK_THIS:
                    case LexerTokenKindEnum.TOK_NEXT:
                    case LexerTokenKindEnum.TOK_LAST:
                        {
                            int adjust = 0;
                            if (tok.Kind == LexerTokenKindEnum.TOK_NEXT)
                                adjust = 1;
                            else if (tok.Kind == LexerTokenKindEnum.TOK_LAST)
                                adjust = -1;

                            tok = Lexer.NextToken();
                            switch (tok.Kind)
                            {
                                case LexerTokenKindEnum.TOK_INT:
                                    {
                                        int amount = tok.Value.GetValue<int>();

                                        Date baseDate = today;
                                        Date end = today;

                                        if (adjust == 0)
                                            adjust = 1;

                                        tok = Lexer.NextToken();
                                        switch(tok.Kind)
                                        {
                                            case LexerTokenKindEnum.TOK_YEARS:
                                                baseDate = baseDate.AddYears(amount * adjust);
                                                break;
                                            case LexerTokenKindEnum.TOK_QUARTERS:
                                                baseDate = baseDate.AddMonths(amount * adjust * 3);
                                                break;
                                            case LexerTokenKindEnum.TOK_MONTHS:
                                                baseDate = baseDate.AddMonths(amount * adjust);
                                                break;
                                            case LexerTokenKindEnum.TOK_WEEKS:
                                                baseDate = baseDate.AddDays(amount * adjust * 7);
                                                break;
                                            case LexerTokenKindEnum.TOK_DAYS:
                                                baseDate = baseDate.AddDays(amount * adjust);
                                                break;
                                            default:
                                                tok.Unexpected();
                                                break;
                                        }

                                        if (adjust > 0)
                                        {
                                            Date temp = baseDate;
                                            baseDate = end;
                                            end = temp;
                                        }

                                        sinceSpecifier = new DateSpecifier(baseDate);
                                        untilSpecifier = new DateSpecifier(end);
                                        break;
                                    }

                                case LexerTokenKindEnum.TOK_A_MONTH:
                                    {
                                        DetermineWhen(ref tok, ref inclusionSpecifier);

                                        Date temp = new Date(today.Year, (int)inclusionSpecifier.Month, 1);
                                        temp = temp.AddYears(adjust);
                                        inclusionSpecifier = new DateSpecifier(temp.Year, (MonthEnum)temp.Month);
                                        break;
                                    }

                                case LexerTokenKindEnum.TOK_A_WDAY:
                                    {
                                        DetermineWhen(ref tok, ref inclusionSpecifier);

                                        Date temp = DateDuration.FindNearest(today, SkipQuantumEnum.WEEKS);
                                        while (temp.DayOfWeek != inclusionSpecifier.WDay) temp = temp.AddDays(1);
                                        temp = temp.AddDays(7 * adjust);
                                        inclusionSpecifier = new DateSpecifier(temp);
                                        break;
                                    }

                                case LexerTokenKindEnum.TOK_YEAR:
                                    {
                                        Date temp = today;
                                        temp = temp.AddYears(adjust);
                                        inclusionSpecifier = new DateSpecifier(temp.Year);
                                        break;
                                    }

                                case LexerTokenKindEnum.TOK_QUARTER:
                                    {
                                        Date baseDate = DateDuration.FindNearest(today, SkipQuantumEnum.QUARTERS);
                                        Date temp;
                                        if (adjust < 0)
                                            temp = baseDate.AddMonths(3 * adjust);
                                        else if (adjust == 0)
                                            temp = baseDate.AddMonths(3);
                                        else
                                        {
                                            baseDate = baseDate.AddMonths(3 * adjust);
                                            temp = baseDate.AddMonths(3 * adjust);
                                        }
                                        sinceSpecifier = new DateSpecifier(adjust < 0 ? temp : baseDate);
                                        untilSpecifier = new DateSpecifier(adjust < 0 ? baseDate : temp);
                                        break;
                                    }

                                case LexerTokenKindEnum.TOK_WEEK:
                                    {
                                        Date baseDate = DateDuration.FindNearest(today, SkipQuantumEnum.WEEKS);
                                        Date temp;
                                        if (adjust < 0)
                                            temp = baseDate.AddDays(7 * adjust);
                                        else if (adjust == 0)
                                            temp = baseDate.AddDays(7);
                                        else
                                        {
                                            baseDate = baseDate.AddDays(7 * adjust);
                                            temp = baseDate.AddDays(7 * adjust);
                                        }
                                        sinceSpecifier = new DateSpecifier(adjust < 0 ? temp : baseDate);
                                        untilSpecifier = new DateSpecifier(adjust < 0 ? baseDate : temp);
                                        break;
                                    }

                                case LexerTokenKindEnum.TOK_DAY:
                                    {
                                        Date temp = today;
                                        temp = temp.AddDays(adjust);
                                        inclusionSpecifier = new DateSpecifier(temp);
                                        break;
                                    }

                                case LexerTokenKindEnum.TOK_MONTH:
                                default:
                                    {
                                        Date temp = today;
                                        temp = temp.AddMonths(adjust);
                                        inclusionSpecifier = new DateSpecifier(temp.Year, (MonthEnum)temp.Month);
                                        break;
                                    }

                            }
                            break;
                        }

                    case LexerTokenKindEnum.TOK_TODAY:
                        inclusionSpecifier = new DateSpecifier(today);
                        break;
                    case LexerTokenKindEnum.TOK_TOMORROW:
                        inclusionSpecifier = new DateSpecifier(today.AddDays(1));
                        break;
                    case LexerTokenKindEnum.TOK_YESTERDAY:
                        inclusionSpecifier = new DateSpecifier(today.AddDays(-1));
                        break;

                    case LexerTokenKindEnum.TOK_EVERY:
                        tok = Lexer.NextToken();
                        if (tok.Kind == LexerTokenKindEnum.TOK_INT)
                        {
                            int quantity = tok.Value.GetValue<int>();
                            tok = Lexer.NextToken();
                            switch (tok.Kind)
                            {
                                case LexerTokenKindEnum.TOK_YEARS:
                                    period.Duration = new DateDuration(SkipQuantumEnum.YEARS, quantity);
                                    break;
                                case LexerTokenKindEnum.TOK_QUARTERS:
                                    period.Duration = new DateDuration(SkipQuantumEnum.QUARTERS, quantity);
                                    break;
                                case LexerTokenKindEnum.TOK_MONTHS:
                                    period.Duration = new DateDuration(SkipQuantumEnum.MONTHS, quantity);
                                    break;
                                case LexerTokenKindEnum.TOK_WEEKS:
                                    period.Duration = new DateDuration(SkipQuantumEnum.WEEKS, quantity);
                                    break;
                                case LexerTokenKindEnum.TOK_DAYS:
                                    period.Duration = new DateDuration(SkipQuantumEnum.DAYS, quantity);
                                    break;
                                default:
                                    tok.Unexpected();
                                    break;
                            }
                        }
                        else
                        {
                            switch (tok.Kind)
                            {
                                case LexerTokenKindEnum.TOK_YEAR:
                                    period.Duration = new DateDuration(SkipQuantumEnum.YEARS, 1);
                                    break;
                                case LexerTokenKindEnum.TOK_QUARTER:
                                    period.Duration = new DateDuration(SkipQuantumEnum.QUARTERS, 1);
                                    break;
                                case LexerTokenKindEnum.TOK_MONTH:
                                    period.Duration = new DateDuration(SkipQuantumEnum.MONTHS, 1);
                                    break;
                                case LexerTokenKindEnum.TOK_WEEK:
                                    period.Duration = new DateDuration(SkipQuantumEnum.WEEKS, 1);
                                    break;
                                case LexerTokenKindEnum.TOK_DAY:
                                    period.Duration = new DateDuration(SkipQuantumEnum.DAYS, 1);
                                    break;
                                default:
                                    tok.Unexpected();
                                    break;
                            }
                        }
                        break;

                    case LexerTokenKindEnum.TOK_YEARLY:
                        period.Duration = new DateDuration(SkipQuantumEnum.YEARS, 1);
                        break;
                    case LexerTokenKindEnum.TOK_QUARTERLY:
                        period.Duration = new DateDuration(SkipQuantumEnum.QUARTERS, 1);
                        break;
                    case LexerTokenKindEnum.TOK_BIMONTHLY:
                        period.Duration = new DateDuration(SkipQuantumEnum.MONTHS, 2);
                        break;
                    case LexerTokenKindEnum.TOK_MONTHLY:
                        period.Duration = new DateDuration(SkipQuantumEnum.MONTHS, 1);
                        break;
                    case LexerTokenKindEnum.TOK_BIWEEKLY:
                        period.Duration = new DateDuration(SkipQuantumEnum.WEEKS, 2);
                        break;
                    case LexerTokenKindEnum.TOK_WEEKLY:
                        period.Duration = new DateDuration(SkipQuantumEnum.WEEKS, 1);
                        break;
                    case LexerTokenKindEnum.TOK_DAILY:
                        period.Duration = new DateDuration(SkipQuantumEnum.DAYS, 1);
                        break;

                    default:
                        tok.Unexpected();
                        break;
                }
            }

            if (sinceSpecifier != null || untilSpecifier != null)
            {
                DateRange range = new DateRange(sinceSpecifier, untilSpecifier) { EndExclusive = endInclusive };
                period.Range = new DateSpecifierOrRange(range);
            }
            else if (inclusionSpecifier != null)
            {
                period.Range = new DateSpecifierOrRange(inclusionSpecifier);
            }
            else
            {
                /* otherwise, it's something like "monthly", with no date reference */
            }

            return period;
        }

        private void DetermineWhen(ref LexerToken tok, ref DateSpecifier specifier)
        {
            Date today = TimesCommon.Current.CurrentDate;
            specifier = specifier ?? new DateSpecifier();

            switch (tok.Kind)
            {
                case LexerTokenKindEnum.TOK_DATE:
                    specifier = tok.Value.GetValue<DateSpecifier>();
                    break;

                case LexerTokenKindEnum.TOK_INT:
                    {
                        int amount = tok.Value.GetValue<int>();
                        int adjust = 0;

                        tok = Lexer.PeekToken();
                        LexerTokenKindEnum kind = tok.Kind;
                        switch (kind)
                        {
                            case LexerTokenKindEnum.TOK_YEAR:
                            case LexerTokenKindEnum.TOK_YEARS:
                            case LexerTokenKindEnum.TOK_QUARTER:
                            case LexerTokenKindEnum.TOK_QUARTERS:
                            case LexerTokenKindEnum.TOK_MONTH:
                            case LexerTokenKindEnum.TOK_MONTHS:
                            case LexerTokenKindEnum.TOK_WEEK:
                            case LexerTokenKindEnum.TOK_WEEKS:
                            case LexerTokenKindEnum.TOK_DAY:
                            case LexerTokenKindEnum.TOK_DAYS:
                                Lexer.NextToken();
                                tok = Lexer.NextToken();
                                switch (tok.Kind)
                                {
                                    case LexerTokenKindEnum.TOK_AGO:
                                        adjust = -1;
                                        break;
                                    case LexerTokenKindEnum.TOK_HENCE:
                                        adjust = 1;
                                        break;
                                    default:
                                        tok.Unexpected();
                                        break;
                                }
                                break;
                            default:
                                break;
                        }

                        Date when = today;

                        switch(kind)
                        {
                            case LexerTokenKindEnum.TOK_YEAR:
                            case LexerTokenKindEnum.TOK_YEARS:
                                when = when.AddYears(amount * adjust);
                                break;

                            case LexerTokenKindEnum.TOK_QUARTER:
                            case LexerTokenKindEnum.TOK_QUARTERS:
                                when = when.AddMonths(amount * 3 * adjust);
                                break;

                            case LexerTokenKindEnum.TOK_MONTH:
                            case LexerTokenKindEnum.TOK_MONTHS:
                                when = when.AddMonths(amount * adjust);
                                break;

                            case LexerTokenKindEnum.TOK_WEEK:
                            case LexerTokenKindEnum.TOK_WEEKS:
                                when = when.AddDays(amount * 7 *adjust);
                                break;

                            case LexerTokenKindEnum.TOK_DAY:
                            case LexerTokenKindEnum.TOK_DAYS:
                                when = when.AddDays(amount * adjust);
                                break;

                            default:
                                if (amount > 31)
                                    specifier.Year = amount;
                                else
                                    specifier.Day = amount;
                                break;
                        }

                        if (adjust != 0)
                            specifier = new DateSpecifier(when);
                        break;
                    }

                case LexerTokenKindEnum.TOK_THIS:
                case LexerTokenKindEnum.TOK_NEXT:
                case LexerTokenKindEnum.TOK_LAST:
                    {
                        int adjust = 0;
                        if (tok.Kind == LexerTokenKindEnum.TOK_NEXT)
                            adjust = 1;
                        else if (tok.Kind == LexerTokenKindEnum.TOK_LAST)
                            adjust = -1;

                        tok = Lexer.NextToken();
                        switch (tok.Kind)
                        {
                            case LexerTokenKindEnum.TOK_A_MONTH:
                                {
                                    Date temp = new Date(today.Year, tok.Value.GetValue<int>(), 1);
                                    temp = temp.AddYears(adjust);
                                    specifier = new DateSpecifier(temp.Year, (MonthEnum)temp.Month);
                                    break;
                                }

                            case LexerTokenKindEnum.TOK_A_WDAY:
                                {
                                    Date temp = DateDuration.FindNearest(today, SkipQuantumEnum.WEEKS);
                                    while (temp.DayOfWeek != tok.Value.GetValue<DayOfWeek>())
                                        temp = temp.AddDays(1);
                                    temp = temp.AddDays(7 * adjust);
                                    specifier = new DateSpecifier(temp);
                                    break;
                                }

                            case LexerTokenKindEnum.TOK_YEAR:
                                {
                                    Date temp = today;
                                    temp = temp.AddYears(adjust);
                                    specifier = new DateSpecifier(temp.Year);
                                    break;
                                }

                            case LexerTokenKindEnum.TOK_QUARTER:
                                {
                                    Date baseDate = DateDuration.FindNearest(today, SkipQuantumEnum.QUARTERS);
                                    Date temp = default(Date);
                                    if (adjust < 0)
                                    {
                                        temp = baseDate.AddMonths(3 * adjust);
                                    }
                                    else if (adjust == 0)
                                    {
                                        temp = baseDate.AddMonths(3);
                                    }
                                    else if (adjust > 0)
                                    {
                                        baseDate = baseDate.AddMonths(3 * adjust);
                                        temp = baseDate.AddMonths(3 * adjust);
                                    }
                                    specifier = new DateSpecifier(adjust < 0 ? temp : baseDate);
                                    break;
                                }

                            case LexerTokenKindEnum.TOK_WEEK:
                                {
                                    Date baseDate = DateDuration.FindNearest(today, SkipQuantumEnum.WEEKS);
                                    Date temp = default(Date);
                                    if (adjust < 0)
                                    {
                                        temp = baseDate.AddDays(7 * adjust);
                                    }
                                    else if (adjust == 0)
                                    {
                                        temp = baseDate.AddDays(7);
                                    }
                                    else if (adjust > 0)
                                    {
                                        baseDate = baseDate.AddDays(7 * adjust);
                                        temp = baseDate.AddDays(7 * adjust);
                                    }
                                    specifier = new DateSpecifier(adjust < 0 ? temp : baseDate);
                                    break;
                                }

                            case LexerTokenKindEnum.TOK_DAY:
                                {
                                    Date temp = today;
                                    temp = temp.AddDays(adjust);
                                    specifier = new DateSpecifier(temp);
                                    break;
                                }

                            case LexerTokenKindEnum.TOK_MONTH:
                            default:
                                {
                                    Date temp = today;
                                    temp = temp.AddMonths(adjust);
                                    specifier = new DateSpecifier(temp.Year, (MonthEnum)temp.Month);
                                    break;
                                }
                        }
                        break;
                    }

                case LexerTokenKindEnum.TOK_A_MONTH:
                    specifier.Month = tok.Value.GetValue<MonthEnum>();
                    tok = Lexer.PeekToken();
                    switch (tok.Kind)
                    {
                        case LexerTokenKindEnum.TOK_INT:
                            specifier.Year = tok.Value.GetValue<int>();
                            break;
                        case LexerTokenKindEnum.END_REACHED:
                            break;
                        default:
                            break;
                    }
                    break;

                case LexerTokenKindEnum.TOK_A_WDAY:
                    specifier.WDay = tok.Value.GetValue<DayOfWeek>();
                    break;

                case LexerTokenKindEnum.TOK_TODAY:
                    specifier = new DateSpecifier(today);
                    break;

                case LexerTokenKindEnum.TOK_TOMORROW:
                    specifier = new DateSpecifier(today.AddDays(1));
                    break;

                case LexerTokenKindEnum.TOK_YESTERDAY:
                    specifier = new DateSpecifier(today.AddDays(-1));
                    break;

                default:
                    tok.Unexpected();
                    break;
            }
        }
    }
}
