// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
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
    public class DateSpecifier
    {
        public DateSpecifier(int? year = null, MonthEnum? month = null, int? day = null, DayOfWeek? dayOfWeek = null)
        {
            Year = year;
            Month = month;
            Day = day;
            WDay = dayOfWeek;
        }

        public DateSpecifier (Date date, DateTraits? traits = null)
        {
            if (traits != null && traits.Value.HasYear)
                Year = date.Year;
            if (traits != null && traits.Value.HasMonth)
                Month = (MonthEnum)date.Month;
            if (traits != null && traits.Value.HasDay)
                Day = date.Day;
        }

        public int? Year { get;  set; }
        public MonthEnum? Month { get; set; }
        public int? Day { get; set; }
        public DayOfWeek? WDay { get; set; }

        public Date Begin
        {
            get 
            {
                int theYear = Year ?? TimesCommon.Current.CurrentDate.Year;
                int theMonth = Month.HasValue ? (int)Month : 1;
                int theDay = Day ?? 1;

                // jww (2009-11-16): Handle wday.  If a month is set, find the most recent
                // wday in that month; if the year is set, then in that year.

                return new Date(theYear, theMonth, theDay);
            }
        }

        public Date End
        {
            get 
            {
                if (Day.HasValue || WDay.HasValue)
                    return Begin.AddDays(1);
                if (Month.HasValue)
                    return Begin.AddMonths(1);
                if (Year.HasValue)
                    return Begin.AddYears(1);
                return default(Date);
            }
        }

        public bool IsWithin(Date date)
        {
            return date >= Begin && date < End;
        }

        public DateDuration ImpliedDuration
        {
            get 
            {
                if (Day.HasValue || WDay.HasValue)
                    return new DateDuration(SkipQuantumEnum.DAYS, 1);
                else if (Month.HasValue)
                    return new DateDuration(SkipQuantumEnum.MONTHS, 1);
                else if (Year.HasValue)
                    return new DateDuration(SkipQuantumEnum.YEARS, 1);
                else
                    return null;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (Year.HasValue)
                sb.AppendFormat(" year {0}", Year.Value);
            if (Month.HasValue)
                sb.AppendFormat(" month {0}", Month.Value);
            if (Day.HasValue)
                sb.AppendFormat(" day {0}", Day.Value);
            if (WDay.HasValue)
                sb.AppendFormat(" wday {0}", WDay.Value);
            return sb.ToString();
        }
    }
}
