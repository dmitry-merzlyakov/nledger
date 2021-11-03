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

namespace NLedger.Extensibility.Export
{
    public static class ExportedConsts
    {
        public static readonly int COMMODITY_STYLE_DEFAULTS = (int)Commodities.CommodityFlagsEnum.COMMODITY_STYLE_DEFAULTS;
        public static readonly int COMMODITY_STYLE_SUFFIXED = (int)Commodities.CommodityFlagsEnum.COMMODITY_STYLE_SUFFIXED;
        public static readonly int COMMODITY_STYLE_SEPARATED = (int)Commodities.CommodityFlagsEnum.COMMODITY_STYLE_SEPARATED;
        public static readonly int COMMODITY_STYLE_DECIMAL_COMMA = (int)Commodities.CommodityFlagsEnum.COMMODITY_STYLE_DECIMAL_COMMA;
        public static readonly int COMMODITY_STYLE_TIME_COLON = (int)Commodities.CommodityFlagsEnum.COMMODITY_STYLE_TIME_COLON;
        public static readonly int COMMODITY_STYLE_THOUSANDS = (int)Commodities.CommodityFlagsEnum.COMMODITY_STYLE_THOUSANDS;
        public static readonly int COMMODITY_NOMARKET = (int)Commodities.CommodityFlagsEnum.COMMODITY_NOMARKET;
        public static readonly int COMMODITY_BUILTIN = (int)Commodities.CommodityFlagsEnum.COMMODITY_BUILTIN;
        public static readonly int COMMODITY_WALKED = (int)Commodities.CommodityFlagsEnum.COMMODITY_WALKED;
        public static readonly int COMMODITY_KNOWN = (int)Commodities.CommodityFlagsEnum.COMMODITY_KNOWN;
        public static readonly int COMMODITY_PRIMARY = (int)Commodities.CommodityFlagsEnum.COMMODITY_PRIMARY;

        public static readonly uint ACCOUNT_EXT_SORT_CALC = 0x01;
        public static readonly uint ACCOUNT_EXT_HAS_NON_VIRTUALS = 0x02;
        public static readonly uint ACCOUNT_EXT_HAS_UNB_VIRTUALS = 0x04;
        public static readonly uint ACCOUNT_EXT_AUTO_VIRTUALIZE = 0x08;
        public static readonly uint ACCOUNT_EXT_VISITED = 0x10;
        public static readonly uint ACCOUNT_EXT_MATCHING = 0x20;
        public static readonly uint ACCOUNT_EXT_TO_DISPLAY = 0x40;
        public static readonly uint ACCOUNT_EXT_DISPLAYED = 0x80;

        public static readonly uint ACCOUNT_NORMAL = 0x00;
        public static readonly uint ACCOUNT_KNOWN = 0x01;
        public static readonly uint ACCOUNT_TEMP = 0x02;
        public static readonly uint ACCOUNT_GENERATED = 0x04;

        public static readonly uint ITEM_NORMAL = (uint)SupportsFlagsEnum.ITEM_NORMAL;
        public static readonly uint ITEM_GENERATED = (uint)SupportsFlagsEnum.ITEM_GENERATED;
        public static readonly uint ITEM_TEMP = (uint)SupportsFlagsEnum.ITEM_TEMP;

        public static readonly uint POST_EXT_RECEIVED = 0x001;
        public static readonly uint POST_EXT_HANDLED = 0x002;
        public static readonly uint POST_EXT_DISPLAYED = 0x004;
        public static readonly uint POST_EXT_DIRECT_AMT = 0x008;
        public static readonly uint POST_EXT_SORT_CALC = 0x010;
        public static readonly uint POST_EXT_COMPOUND = 0x020;
        public static readonly uint POST_EXT_VISITED = 0x040;
        public static readonly uint POST_EXT_MATCHES = 0x080;
        public static readonly uint POST_EXT_CONSIDERED = 0x100;

        public static readonly uint POST_VIRTUAL = (uint)SupportsFlagsEnum.POST_VIRTUAL;
        public static readonly uint POST_MUST_BALANCE = (uint)SupportsFlagsEnum.POST_MUST_BALANCE;
        public static readonly uint POST_CALCULATED = (uint)SupportsFlagsEnum.POST_CALCULATED;
        public static readonly uint POST_COST_CALCULATED = (uint)SupportsFlagsEnum.POST_COST_CALCULATED;

        public static readonly uint ANNOTATION_PRICE_CALCULATED = 0x01;
        public static readonly uint ANNOTATION_PRICE_FIXATED = 0x02;
        public static readonly uint ANNOTATION_PRICE_NOT_PER_UNIT = 0x04;
        public static readonly uint ANNOTATION_DATE_CALCULATED = 0x08;
        public static readonly uint ANNOTATION_TAG_CALCULATED = 0x10;
        public static readonly uint ANNOTATION_VALUE_EXPR_CALCULATED = 0x20;
    }
}
