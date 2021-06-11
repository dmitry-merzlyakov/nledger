using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class Commodity : BaseExport<Commodities.Commodity>
    {
        public static implicit operator Commodity(Commodities.Commodity commodity) => new Commodity(commodity);

        public static explicit operator bool(Commodity commodity) => (bool)commodity.Origin;
        public static bool operator ==(Commodity commLeft, Commodity commRight) => commLeft.Origin == commRight.Origin;
        public static bool operator !=(Commodity commLeft, Commodity commRight) => commLeft.Origin != commRight.Origin;

        protected Commodity(Commodities.Commodity origin) : base(origin)
        { }

        public int flags { get => (int)Origin.Flags; set => Origin.Flags = (Commodities.CommodityFlagsEnum)value; }

        public bool has_flags(int flag) => Origin.Flags.HasFlag((Commodities.CommodityFlagsEnum)flag);
        public void clear_flags(int flag) => Origin.Flags = Commodities.CommodityFlagsEnum.COMMODITY_STYLE_DEFAULTS;
        public void add_flags(int flag) => Origin.Flags |= (Commodities.CommodityFlagsEnum)flag;
        public void drop_flags(int flag) => Origin.Flags &= ~(Commodities.CommodityFlagsEnum)flag;

        public static bool decimal_comma_by_default { get => Commodities.Commodity.Defaults.DecimalCommaByDefault; set => Commodities.Commodity.Defaults.DecimalCommaByDefault = value; }
        public override string ToString() => Origin.Symbol;
        public static bool symbol_needs_quotes(string symbol) => Commodities.Commodity.SymbolNeedsQuotes(symbol);
        public virtual Commodity referent => Origin.Referent;

        public bool has_annotation() => Origin.IsAnnotated;
        public virtual Commodity strip_annotations() => Origin.StripAnnotations(new Annotate.AnnotationKeepDetails());
        public virtual Commodity strip_annotations(KeepDetails keep) => Origin.StripAnnotations(keep.Origin);
        public string write_annotations() => Origin.WriteAnnotations();

        public CommodityPool pool => Origin.Pool;
        public string base_symbol => Origin.BaseSymbol;
        public string symbol => Origin.Symbol;
        public string name { get => Origin.Name; set => Origin.SetName(value); }
        public string note { get => Origin.Note; set => Origin.SetNote(value); }
        public int precision { get => Origin.Precision; set => Origin.Precision = value; }
        public Amount smaller { get => Origin.Smaller; set => Origin.Smaller = value.Origin; }
        public Amount larger { get => Origin.Larger; set => Origin.Larger = value.Origin; }

        public void add_price(DateTime date, Amount price) => Origin.AddPrice(date, price.Origin);
        public void add_price(DateTime date, Amount price, bool reflexive) => Origin.AddPrice(date, price.Origin, reflexive);
        public void remove_price(DateTime date, Commodity commodity) => Origin.RemovePrice(date, commodity.Origin);
        public PricePoint find_price(Commodity commodity = null, DateTime moment = default(DateTime), DateTime oldest = default(DateTime)) => Origin.FindPrice(commodity?.Origin, moment, oldest);
        public PricePoint check_for_updated_price(PricePoint point = null, DateTime moment = default(DateTime), Commodity inTermsOf = null) => Origin.CheckForUpdatedPrice(point?.Origin, moment, inTermsOf?.Origin);
        public void Valid() => Origin.Valid();

        public override bool Equals(object obj) => Origin.Equals((obj as Commodity)?.Origin);
        public override int GetHashCode() => Origin.GetHashCode();
    }
}
