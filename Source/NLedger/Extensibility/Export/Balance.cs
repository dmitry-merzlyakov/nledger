using NLedger.Times;
using NLedger.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class Balance : BaseExport<NLedger.Balance>, IEnumerable<Amount>
    {
        public static implicit operator Balance(NLedger.Balance balance) => new Balance(balance);
        public static implicit operator Balance(Amount amount) => new Balance(amount);
        public static implicit operator Balance(long val) => new Balance(val);
        public static implicit operator Balance(string val) => new Balance(val);

        public static bool operator ==(Balance left, Balance right) => left.Origin == right.Origin;
        public static bool operator !=(Balance left, Balance right) => left.Origin != right.Origin;

        public static bool operator ==(Balance left, Amount right) => left.Origin == right.Origin;
        public static bool operator !=(Balance left, Amount right) => left.Origin != right.Origin;

        public static bool operator ==(Balance left, long right) => left.Origin == (NLedger.Balance)right;
        public static bool operator !=(Balance left, long right) => left.Origin != (NLedger.Balance)right;

        public static explicit operator bool(Balance value) => (bool)value.Origin;
        public static bool operator !(Balance value) => !(bool)value.Origin;
        public static Balance operator -(Balance value) => -value.Origin;

        public static Balance operator +(Balance left, Balance right) => left.Origin + right.Origin;
        public static Balance operator +(Balance left, Amount right) => left.Origin + right.Origin;
        public static Balance operator +(Balance left, long right) => left.Origin + (NLedger.Balance)right;

        public static Balance operator -(Balance left, Balance right) => left.Origin - right.Origin;
        public static Balance operator -(Balance left, Amount right) => left.Origin - right.Origin;
        public static Balance operator -(Balance left, long right) => left.Origin - (NLedger.Balance)right;

        public static Balance operator *(Balance left, Amount right) => left.Origin * right.Origin;
        public static Balance operator *(Balance left, long right) => left.Origin * (Amounts.Amount)right;

        public static Balance operator /(Balance left, Amount right) => left.Origin / right.Origin;
        public static Balance operator /(Balance left, long right) => left.Origin / (Amounts.Amount)right;

        protected Balance(NLedger.Balance origin) : base(origin)
        { }

        public Balance(Balance balance) : this(new NLedger.Balance(balance?.Origin))
        { }

        public Balance(Amount amount) : this(new NLedger.Balance(amount?.Origin))
        { }

        public Balance(long val) : this(new NLedger.Balance(val))
        { }

        public Balance(string val) : this(new NLedger.Balance(val))
        { }

        public string to_string => ToString();

        public Balance negated() => Origin.Negated();
        public void in_place_negate() => Origin.InPlaceNegate();
        public Balance abs() => Origin.Abs();

        public Balance rounded() => Origin.Rounded();
        public void in_place_round() => Origin.InPlaceRound();

        public Balance truncated() => Origin.Truncated();
        public void in_place_truncate() => Origin.InPlaceTruncate();

        public Balance floored() => Origin.Floored();
        public void in_place_floor() => Origin.InPlaceFloor();

        public Balance unrounded() => Origin.Unrounded();
        public void in_place_unround() => Origin.InPlaceUnround();

        public Balance reduced() => Origin.Reduced();
        public void in_place_reduce() => Origin.InPlaceReduce();

        public Balance unreduced() => Origin.Unreduced();
        public void in_place_unreduce() => Origin.InPlaceUnreduce();

        public Balance value() => Origin.Value(TimesCommon.Current.CurrentTime);
        public Balance value(Commodity in_terms_of) => Origin.Value(TimesCommon.Current.CurrentTime, in_terms_of.Origin);
        public Balance value(Commodity in_terms_of, DateTime moment) => Origin.Value(moment, in_terms_of.Origin);
        public Balance value(Commodity in_terms_of, Date moment) => Origin.Value(moment, in_terms_of.Origin);

        public bool is_nonzero() => Origin.IsNonZero;
        public bool is_zero() => Origin.IsZero;
        public bool is_realzero() => Origin.IsRealZero;

        public bool is_empty() => Origin.IsEmpty;
        public Amount single_amount() => Origin.SingleAmount;

        public Amount to_amount() => Origin.ToAmount();

        public int commodity_count() => Origin.CommodityCount;
        public Amount commodity_amount() => Origin.CommodityAmount();
        public Amount commodity_amount(Commodity commodity) => Origin.CommodityAmount(commodity.Origin);

        public Balance number() => Origin.Number();

        public Balance strip_annotations() => Origin.StripAnnotations(new Annotate.AnnotationKeepDetails());
        public Balance strip_annotations(KeepDetails keep) => Origin.StripAnnotations(keep.Origin);

        public bool valid() => Origin.Valid();

        public IEnumerator<Amount> GetEnumerator() => Origin.Amounts.Values.Select(a => (Amount)a).ToList().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        public Amount this[int index] => Origin.Amounts.Values.ElementAt(index);


        public override bool Equals(object obj) => Origin.Equals(obj);
        public override int GetHashCode() => Origin.GetHashCode();
        public override string ToString() => Origin.ToString();
    }
}
