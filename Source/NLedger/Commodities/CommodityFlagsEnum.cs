// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Commodities
{
    [Flags]
    public enum CommodityFlagsEnum
    {
        COMMODITY_STYLE_DEFAULTS         = 0x000,
        COMMODITY_STYLE_SUFFIXED         = 0x001,
        COMMODITY_STYLE_SEPARATED        = 0x002,
        COMMODITY_STYLE_DECIMAL_COMMA    = 0x004,
        COMMODITY_STYLE_THOUSANDS        = 0x008,
        COMMODITY_NOMARKET               = 0x010,
        COMMODITY_BUILTIN                = 0x020,
        COMMODITY_WALKED                 = 0x040,
        COMMODITY_KNOWN                  = 0x080,
        COMMODITY_PRIMARY                = 0x100,
        COMMODITY_SAW_ANNOTATED          = 0x200,
        COMMODITY_SAW_ANN_PRICE_FLOAT    = 0x400,
        COMMODITY_SAW_ANN_PRICE_FIXATED  = 0x800,
        COMMODITY_STYLE_TIME_COLON       = 0x1000,
        COMMODITY_STYLE_NO_MIGRATE       = 0x2000
    }
}
