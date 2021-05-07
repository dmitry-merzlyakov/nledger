using NLedger.Amounts;
using NLedger.Annotate;
using NLedger.Commodities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    /// <summary>
    /// [DM] Exported commodity members and globals as it is defined in py_commodity.cc
    /// </summary>
    public static class CommodityExport
    {
        public static CommodityPool commodities => CommodityPool.Current;

        public static int COMMODITY_STYLE_DEFAULTS = (int)CommodityFlagsEnum.COMMODITY_STYLE_DEFAULTS;
        public static int COMMODITY_STYLE_SUFFIXED = (int)CommodityFlagsEnum.COMMODITY_STYLE_SUFFIXED;
        public static int COMMODITY_STYLE_SEPARATED = (int)CommodityFlagsEnum.COMMODITY_STYLE_SEPARATED;
        public static int COMMODITY_STYLE_DECIMAL_COMMA = (int)CommodityFlagsEnum.COMMODITY_STYLE_DECIMAL_COMMA;
        public static int COMMODITY_STYLE_TIME_COLON = (int)CommodityFlagsEnum.COMMODITY_STYLE_TIME_COLON;
        public static int COMMODITY_STYLE_THOUSANDS = (int)CommodityFlagsEnum.COMMODITY_STYLE_THOUSANDS;
        public static int COMMODITY_NOMARKET = (int)CommodityFlagsEnum.COMMODITY_NOMARKET;
        public static int COMMODITY_BUILTIN = (int)CommodityFlagsEnum.COMMODITY_BUILTIN;
        public static int COMMODITY_WALKED = (int)CommodityFlagsEnum.COMMODITY_WALKED;
        public static int COMMODITY_KNOWN = (int)CommodityFlagsEnum.COMMODITY_KNOWN;
        public static int COMMODITY_PRIMARY = (int)CommodityFlagsEnum.COMMODITY_PRIMARY;
    }

    public interface ICommodityPoolExport // TODO : IDictionary<string,Commodity>
    {
        Commodity null_commodity { get; }
        Commodity default_commodity { get; set; }
        bool keep_base { get; set; }
        string price_db { get; set; }
        long quote_leeway { get; set; }
        bool get_quotes { get; set; }
        Func<Commodity, Commodity, PricePoint?> get_commodity_quote { get; set; }

        Commodity create(string symbol);
        Commodity create(string symbol, Annotation details);

        Commodity find_or_create(string symbol);
        Commodity find_or_create(string symbol, Annotation details);

        Commodity find(string name);
        Commodity find(string name, Annotation details);

        void exchange(Commodity commodity, Amount per_unit_cost);
        void exchange(Commodity commodity, Amount per_unit_cost, DateTime moment);
        void exchange(Amount amount, Amount cost, bool is_per_unit, bool add_prices, DateTime? moment, string tag);

        Tuple<Commodity, PricePoint> parse_price_directive(string line, bool doNotAddPrice = false, bool noDate = false);
        Commodity parse_price_expression(string str, bool addPrice = true, DateTime? moment = null);
    }
}
