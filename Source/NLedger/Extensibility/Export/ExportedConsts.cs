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
    }
}
