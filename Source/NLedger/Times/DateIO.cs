// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLedger.Utility;
using System.Globalization;
using NLedger.Utils;

namespace NLedger.Times
{
    public class DateIO : TemporalIO<Date>
    {
        public DateIO(string fmtStr, bool input) : base (fmtStr, input)
        { }

        public override Date Parse(string str)
        {
            Date parsedDate;
            if (Date.TryParseExact(str, ParseDotNetFmtStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                return parsedDate;

            //[DM] Here is a quite dirty but simple and efficient fix for the problem with parsing dates.
            // When .Net date parsing format does not have a placeholder for a year, it tries to put the current year to a result date.
            // If the input string contains 28 of February but current year is not leap one, an exception is generated
            // (System.FormatException: String was not recognized as a valid DateTime.)
            // It prevents parsing day/month dates with pre-defined current year ("Y" option) if specified year is leap.
            // Note: though the fix looks dirty, performance degradation was not detected.
            if (!Traits.HasYear && str.Contains("29"))
            {
                var fmt = "yyyy|" + ParseDotNetFmtStr;  // Add a placeholder for years.
                var s = "0004|" + str;                  // Add a first leap year in calendar. It will be replaced further.
                if (Date.TryParseExact(s, fmt, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                    return parsedDate;
            }

            Logger.Current.Debug("times.parse", () => String.Format("Failed to parse date '{0}' using pattern '{1}'", str, FmtStr));

            return default(Date);
        }

        public override string Format(Date when)
        {
            return when.ToString(PrintDotNetFmtStr, CultureInfo.InvariantCulture);
        }

    }
}
