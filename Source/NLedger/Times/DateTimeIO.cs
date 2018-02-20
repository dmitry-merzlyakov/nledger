// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Times
{
    public class DateTimeIO : TemporalIO<DateTime>
    {
        public DateTimeIO(string fmtStr, bool input)
            : base(fmtStr, input)
        { }

        public override DateTime Parse(string str)
        {
            DateTime parsedDate;
            if (DateTime.TryParseExact(str, ParseDotNetFmtStr, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out parsedDate))
                return parsedDate;

            Logger.Current.Debug("times.parse", () => String.Format("Failed to parse date/time '{0}' using pattern '{1}'", str, FmtStr));

            return default(DateTime);
        }

        public override string Format(DateTime when)
        {
            return when.ToString(PrintDotNetFmtStr, CultureInfo.InvariantCulture);
        }

    }
}
