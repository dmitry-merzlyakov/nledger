// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLedger.Utility;

namespace NLedger.Times
{
    public class DateRange
    {
        public DateRange(DateSpecifier rangeBegin = null, DateSpecifier rangeEnd = null)
        {
            RangeBegin = rangeBegin;
            RangeEnd = rangeEnd;
        }

        public DateSpecifier RangeBegin { get; private set; }
        public DateSpecifier RangeEnd { get; private set; }
        public bool EndExclusive { get; set; }

        public Date? Begin
        {
            get { return RangeBegin != null ? RangeBegin.Begin : (Date?)null; }
        }

        public Date? End
        {
            get { return RangeEnd != null ? (EndExclusive ? RangeEnd.End : RangeEnd.Begin) : (Date?)null; }
        }

        public bool IsWithin(Date date)
        {
            Date? b = Begin;
            Date? e = End;
            bool afterBegin = b.HasValue ? date >= b.Value : true;
            bool beforeEnd = e.HasValue ? date < e.Value : true;
            return afterBegin && beforeEnd;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (RangeBegin != null)
                sb.AppendFormat("from{0}", RangeBegin);
            if (RangeEnd != null)
                sb.AppendFormat(" to{0}", RangeEnd);
            return sb.ToString();
        }
    }
}
