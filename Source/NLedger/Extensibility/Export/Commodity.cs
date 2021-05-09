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
        public Commodity referent => Origin.Referent;

        public bool has_annotation() => Origin.IsAnnotated;
        public Commodity strip_annotations() => Origin.StripAnnotations(new Annotate.AnnotationKeepDetails());
        public Commodity strip_annotations(KeepDetails keep) => Origin.StripAnnotations(keep.Origin);
        public string write_annotations() => Origin.WriteAnnotations();

        public CommodityPool pool => Origin.Pool;
        public string base_symbol => Origin.BaseSymbol;
        public string symbol => Origin.Symbol;

        public override bool Equals(object obj) => Origin.Equals(obj);
        public override int GetHashCode() => Origin.GetHashCode();
    }
}
