// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
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
    public class DateSpecifierOrRange
    {
        public DateSpecifierOrRange()
        {
            SpecifierOrRange = new BoostVariant(typeof(int), typeof(DateSpecifier), typeof(DateRange));
        }

        public DateSpecifierOrRange(DateSpecifier specifier)
            : this()
        {
            SpecifierOrRange.SetValue(specifier);
        }

        public DateSpecifierOrRange(DateRange range)
            : this()
        {
            SpecifierOrRange.SetValue(range);
        }

        public BoostVariant SpecifierOrRange { get; private set; }

        public Date? Begin
        {
            get
            {
                if (SpecifierOrRange.Type == typeof(DateSpecifier))
                    return SpecifierOrRange.GetValue<DateSpecifier>().Begin;
                else if (SpecifierOrRange.Type == typeof(DateRange))
                    return SpecifierOrRange.GetValue<DateRange>().Begin;
                else
                    return null;
            }
        }

        public Date? End
        {
            get
            {
                if (SpecifierOrRange.Type == typeof(DateSpecifier))
                    return SpecifierOrRange.GetValue<DateSpecifier>().End;
                else if (SpecifierOrRange.Type == typeof(DateRange))
                    return SpecifierOrRange.GetValue<DateRange>().End;
                else
                    return null;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (SpecifierOrRange.Type == typeof(DateSpecifier))
                sb.AppendFormat("in{0}", SpecifierOrRange.GetValue<DateSpecifier>().ToString());
            else if (SpecifierOrRange.Type == typeof(DateRange))
                sb.Append(SpecifierOrRange.GetValue<DateRange>().ToString());

            return sb.ToString();
        }
    }
}
