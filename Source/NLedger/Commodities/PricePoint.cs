// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Commodities
{
    /// <summary>
    /// Ported from price_point_t
    /// </summary>
    public struct PricePoint
    {
        public PricePoint(DateTime when, Amount price) : this()
        {
            When = when;
            Price = price;
        }

        public DateTime When { get; private set; }
        public Amount Price { get; private set; }

        public override bool Equals(object obj)
        {
            if (!(obj is PricePoint))
                return false;

            PricePoint other = (PricePoint)obj;
            return When == other.When && Price == other.Price;
        }

        public override int GetHashCode()
        {
            return When.GetHashCode() ^ (Price == null ? 0 : Price.GetHashCode());
        }

        public static bool operator ==(PricePoint p1, PricePoint p2)
        {
            return p1.Equals(p2);
        }

        public static bool operator !=(PricePoint p1, PricePoint p2)
        {
            return !p1.Equals(p2);
        }
    }
}
