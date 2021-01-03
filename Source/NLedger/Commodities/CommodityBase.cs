// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using NLedger.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Commodities
{
    /// <summary>
    /// Ported from memoized_price_entry
    /// </summary>
    public class MemoizedPriceEntry
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public Commodity Commodity { get; set; }

        public override int GetHashCode()
        {
            int hash = 269;
            hash = (hash * 47) + Start.GetHashCode();
            hash = (hash * 47) + End.GetHashCode();
            hash = (hash * 47) + (Commodity != null ? Commodity.GetHashCode() : 0).GetHashCode();
            return hash;
        }

        public override bool Equals(object obj)
        {
            MemoizedPriceEntry entry = obj as MemoizedPriceEntry;
            if (obj == null)
                return false;

            return Start == entry.Start && End == entry.End && Commodity == entry.Commodity;
        }
    }

    /// <summary>
    /// Ported from commodity_t/base_t
    /// </summary>
    public class CommodityBase
    {
        public const int MaxPriceMapSize = 8;

        public CommodityBase(string symbol)
        {
            Symbol = symbol;            
            //Precision = 0;
            if (Commodity.Defaults.DecimalCommaByDefault)
                Flags = CommodityFlagsEnum.COMMODITY_STYLE_DECIMAL_COMMA;

            PriceMap = new Dictionary<MemoizedPriceEntry, PricePoint?>();
        }

        public string Symbol { get; private set; }
        public int Precision { get; set; }
        public CommodityFlagsEnum Flags { get; set; }
        public int? GraphIndex { get; set; }
        public string Note { get; set; }
        public Amount Smaller { get; set; }
        public Amount Larger { get; set; }
        public Expr ValueExpr { get; set; }
        public IDictionary<MemoizedPriceEntry, PricePoint?> PriceMap { get; private set; }
    }
}
