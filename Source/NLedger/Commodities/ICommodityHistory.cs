// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Commodities
{
    public interface ICommodityHistory
    {
        void AddCommodity(Commodity commodity);
        void AddPrice(Commodity source, DateTime when, Amount price);
        void RemovePrice(Commodity source, Commodity target, DateTime date);
        void MapPrices(Action<DateTime,Amount> fn, Commodity source, DateTime moment, DateTime oldest = default(DateTime), bool bidirectionally = false);
        PricePoint? FindPrice(Commodity source, DateTime moment, DateTime oldest = default(DateTime));
        PricePoint? FindPrice(Commodity source, Commodity target, DateTime moment, DateTime oldest = default(DateTime));
        string PrintMap(DateTime moment = default(DateTime));
    }
}
