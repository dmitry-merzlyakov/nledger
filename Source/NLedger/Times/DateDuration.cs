// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
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
    /// Ported from date_duration_t
    /// </summary>
    public class DateDuration
    {
        public static Date FindNearest(Date date, SkipQuantumEnum skip)
        {
            Date result;

            switch (skip)
            {
                case SkipQuantumEnum.DAYS:
                    return date;

                case SkipQuantumEnum.WEEKS:
                    result = date;
                    while (result.DayOfWeek != TimesCommon.Current.StartToWeek) result = result.AddDays(-1);
                    return result;

                case SkipQuantumEnum.MONTHS:
                    return new Date(date.Year, date.Month, 1);

                case SkipQuantumEnum.QUARTERS:
                    result = new Date(date.Year, date.Month, 1);
                    while (result.Month != 1 && result.Month != 4 && result.Month != 7 && result.Month != 10) result = result.AddMonths(-1);
                    return result;

                case SkipQuantumEnum.YEARS:
                    return new Date(date.Year, 1, 1);

                default:
                    throw new NotSupportedException(String.Format("Unsupported option: {0}", skip));
            }
        }

        public DateDuration()
        { }

        public DateDuration(SkipQuantumEnum quantum, int length)
        {
            Quantum = quantum;
            Length = length;
        }

        public SkipQuantumEnum Quantum { get; private set; }
        public int Length { get; private set; }

        public Date Add (Date date)
        {
            return Add(date, Length, Quantum);
        }

        public Date Subtract(Date date)
        {
            return Add(date, - Length, Quantum);
        }

        public override string ToString()
        {
            string quantumStr = Quantum.ToString();
            return String.Format("{0} {1}{2}", Length, quantumStr.Remove(quantumStr.Length - 1).ToLower(), Length > 1 ? "s" : String.Empty);
        }

        private static Date Add(Date date, int length, SkipQuantumEnum quantum)
        {
            switch (quantum)
            {
                case SkipQuantumEnum.DAYS:
                    return date.AddDays(length);

                case SkipQuantumEnum.WEEKS:
                    return date.AddDays(length * 7);

                case SkipQuantumEnum.MONTHS:
                    return date.AddMonths(length);

                case SkipQuantumEnum.QUARTERS:
                    return date.AddMonths(length * 3);

                case SkipQuantumEnum.YEARS:
                    return date.AddYears(length);

                default:
                    throw new NotSupportedException(String.Format("Unsupported option: {0}", quantum));
            }
        }
    }
}
