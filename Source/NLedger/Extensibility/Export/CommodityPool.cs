using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class CommodityPool : BaseExport<Commodities.CommodityPool> // TODO - IDictionary<string,Commodity>
    {
        public static implicit operator CommodityPool(Commodities.CommodityPool commodityPool) => new CommodityPool(commodityPool);

        public static CommodityPool commodities => Commodities.CommodityPool.Current;

        public static int COMMODITY_STYLE_DEFAULTS = (int)Commodities.CommodityFlagsEnum.COMMODITY_STYLE_DEFAULTS;
        public static int COMMODITY_STYLE_SUFFIXED = (int)Commodities.CommodityFlagsEnum.COMMODITY_STYLE_SUFFIXED;
        public static int COMMODITY_STYLE_SEPARATED = (int)Commodities.CommodityFlagsEnum.COMMODITY_STYLE_SEPARATED;
        public static int COMMODITY_STYLE_DECIMAL_COMMA = (int)Commodities.CommodityFlagsEnum.COMMODITY_STYLE_DECIMAL_COMMA;
        public static int COMMODITY_STYLE_TIME_COLON = (int)Commodities.CommodityFlagsEnum.COMMODITY_STYLE_TIME_COLON;
        public static int COMMODITY_STYLE_THOUSANDS = (int)Commodities.CommodityFlagsEnum.COMMODITY_STYLE_THOUSANDS;
        public static int COMMODITY_NOMARKET = (int)Commodities.CommodityFlagsEnum.COMMODITY_NOMARKET;
        public static int COMMODITY_BUILTIN = (int)Commodities.CommodityFlagsEnum.COMMODITY_BUILTIN;
        public static int COMMODITY_WALKED = (int)Commodities.CommodityFlagsEnum.COMMODITY_WALKED;
        public static int COMMODITY_KNOWN = (int)Commodities.CommodityFlagsEnum.COMMODITY_KNOWN;
        public static int COMMODITY_PRIMARY = (int)Commodities.CommodityFlagsEnum.COMMODITY_PRIMARY;

        protected CommodityPool(Commodities.CommodityPool origin): base(origin)
        { }

        public Commodity null_commodity => Origin.NullCommodity;
        public Commodity default_commodity { get => Origin.DefaultCommodity; set => Origin.DefaultCommodity = value.Origin; }
        public bool keep_base { get => Origin.KeepBase; set => Origin.KeepBase = value; }
        public string price_db { get => Origin.PriceDb; set => Origin.PriceDb = value; }
        public long quote_leeway { get => Origin.QuoteLeeway; set => Origin.QuoteLeeway = value; }
        public bool get_quotes { get => Origin.GetQuotes; set => Origin.GetQuotes = value; }
        //Func<Commodity, Commodity, PricePoint?> get_commodity_quote { get; set; } TODO - make a decision whether exporting a function pointer makes sense

        public Commodity create(string symbol) => Origin.Create(symbol);
        public Commodity create(string symbol, Annotation details) => Origin.Create(symbol, details.Origin);

        public Commodity find_or_create(string symbol) => Origin.FindOrCreate(symbol);
        public Commodity find_or_create(string symbol, Annotation details) => Origin.FindOrCreate(symbol, details.Origin);

        public Commodity find(string name) => Origin.Find(name);
        public Commodity find(string name, Annotation details) => Origin.Find(name, details.Origin);

        public void exchange(Commodity commodity, Amount per_unit_cost) => Origin.Exchange(commodity.Origin, per_unit_cost.Origin, NLedger.Times.TimesCommon.Current.CurrentTime);
        public void exchange(Commodity commodity, Amount per_unit_cost, DateTime moment) => Origin.Exchange(commodity.Origin, per_unit_cost.Origin, moment);
        public void exchange(Amount amount, Amount cost, bool is_per_unit, bool add_prices, DateTime? moment, string tag) => Origin.Exchange(amount.Origin, cost.Origin, is_per_unit, add_prices, moment, tag);

        Tuple<Commodity, PricePoint> parse_price_directive(string line, bool doNotAddPrice = false, bool noDate = false)
        {
            var tuple = Origin.ParsePriceDirective(line, doNotAddPrice, noDate);
            return new Tuple<Commodity, PricePoint>(tuple.Item1, tuple.Item2);
        }

        public Commodity parse_price_expression(string str, bool addPrice = true, DateTime? moment = null) => Origin.ParsePriceExpression(str, addPrice, moment);

    }
}
