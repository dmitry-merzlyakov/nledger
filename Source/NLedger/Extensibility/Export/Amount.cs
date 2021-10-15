using NLedger.Times;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class Amount : BaseExport<Amounts.Amount>
    {
        public static implicit operator Amount(Amounts.Amount amount) => amount != null ? new Amount(amount) : null;
        public static explicit operator Amount(long amount) => new Amount(new Amounts.Amount(amount));

        public static bool operator ==(Amount left, Amount right) => left.Origin == right.Origin;
        public static bool operator !=(Amount left, Amount right) => left.Origin != right.Origin;

        public static bool operator ==(Amount left, long right) => left == (Amount)right;
        public static bool operator !=(Amount left, long right) => left != (Amount)right;

        public static explicit operator bool(Amount value) => (bool)value.Origin;
        public static bool operator !(Amount value) => !(bool)value.Origin;
        public static Amount operator -(Amount value) => -value.Origin;

        public static bool operator <(Amount left, Amount right) => left.Origin < right.Origin;
        public static bool operator >(Amount left, Amount right) => left.Origin > right.Origin;

        public static bool operator <=(Amount left, Amount right) => left.Origin <= right.Origin;
        public static bool operator >=(Amount left, Amount right) => left.Origin >= right.Origin;

        public static bool operator <(Amount left, long right) => left.Origin < (Amount)right;
        public static bool operator >(Amount left, long right) => left.Origin > (Amount)right;

        public static bool operator <=(Amount left, long right) => left.Origin <= (Amount)right;
        public static bool operator >=(Amount left, long right) => left.Origin >= (Amount)right;

        public static Amount operator +(Amount left, Amount right) => left.Origin + right.Origin;
        public static Amount operator +(Amount left, long right) => left.Origin + (Amount)right;
        public static Amount operator +(long left, Amount right) => (Amount)left + right.Origin;

        public static Amount operator -(Amount left, Amount right) => left.Origin - right.Origin;
        public static Amount operator -(Amount left, long right) => left.Origin - (Amount)right;
        public static Amount operator -(long left, Amount right) => (Amount)left - right.Origin;

        public static Amount operator *(Amount left, Amount right) => left.Origin * right.Origin;
        public static Amount operator *(Amount left, long right) => left.Origin * (Amount)right;
        public static Amount operator *(long left, Amount right) => (Amount)left * right.Origin;

        public static Amount operator /(Amount left, Amount right) => left.Origin / right.Origin;
        public static Amount operator /(Amount left, long right) => left.Origin / (Amount)right;
        public static Amount operator /(long left, Amount right) => (Amount)left / right.Origin;

        protected Amount(Amounts.Amount origin) : base(origin)
        { }

        public static void initialize() => Amounts.Amount.Initialize();
        public static void shutdown() => Amounts.Amount.Shutdown();

        //public static bool is_initialized => TODO - not implemented
        //public static bool stream_fullstrings => TODO - not implemented

        public Amount(long value) : base(new Amounts.Amount(value))
        { }

        public Amount(string value) : base(new Amounts.Amount(value))
        { }

        /// <summary>
        /// Construct an amount object whose display precision is always equal to its internal precision.
        /// </summary>
        public static Amount exact(string value) => Amounts.Amount.Exact(value);

        public Amount(Amount value) : base(new Amounts.Amount(value.Origin))
        { }

        /// <summary>
        /// Compare two amounts for equality, returning <0, 0 or >0.
        /// </summary>
        public int compare(Amount value) => Origin.Compare(value.Origin);

        public int precision => Origin.Precision;
        public int display_precision => Origin.DisplayPrecision;
        public bool keep_precision { get => Origin.KeepPrecision; set => Origin.SetKeepPrecision(value); }

        public Amount negated() => Origin.Negated();
        public void in_place_negate() => Origin.InPlaceNegate();
        public Amount abs() => Origin.Abs();
        public Amount inverted() => Origin.Inverted();
        public Amount rounded() => Origin.Rounded();
        public void in_place_round() => Origin.InPlaceRound();
        public Amount truncated() => Origin.Truncated();
        public void in_place_truncate() => Origin.InPlaceTruncate();
        public Amount floored() => Origin.Floored();
        public void in_place_floor() => Origin.InPlaceFloor();
        public Amount unrounded() => Origin.Unrounded();
        public void in_place_unround() => Origin.InPlaceUnround();
        public Amount reduced() => Origin.Reduced();
        public void in_place_reduce() => Origin.InPlaceReduce();
        public Amount unreduced() => Origin.Unreduced();
        public void in_place_unreduce() => Origin.InPlaceUnreduce();

        public Amount value() => Origin.Value(TimesCommon.Current.CurrentTime);
        public Amount value(Commodity in_terms_of) => Origin.Value(TimesCommon.Current.CurrentTime, in_terms_of.Origin);
        public Amount value(Commodity in_terms_of, Date moment) => Origin.Value(moment, in_terms_of.Origin);
        public Amount value(Commodity in_terms_of, DateTime moment) => Origin.Value(moment, in_terms_of.Origin);

        public Amount price() => Origin.Price;

        public int sign() => Origin.Sign;
        public bool is_nonzero() => Origin.IsNonZero;
        public bool is_zero() => Origin.IsZero;
        public bool is_realzero() => Origin.IsRealZero;
        public bool is_null() => Amounts.Amount.IsNullOrEmpty(Origin);

        public double to_double() => Origin.ToDouble();
        public long to_long() => Origin.ToLong();
        public bool fits_in_long() => Origin.FitsInLong;

        public string to_string() => ToString();
        public string to_fullstring() => Origin.ToFullString();
        public string quantity_string() => Origin.QuantityString();

        public Commodity commodity { get => Origin.Commodity; set => Origin.SetCommodity(value.Origin); }
        public bool has_commodity() => Origin.HasCommodity;
        public Amount with_commodity(Commodity comm) => Origin.WithCommodity(comm.Origin);
        public void clear_commodity() => Origin.ClearCommodity();

        public Amount number() => Origin.Number();

        public void annotate(Annotation details) => Origin.Annotate(details.Origin);
        public bool has_annotation() => Origin.HasAnnotation;
        public Annotation annotation => Origin.Annotation;
        public Amount strip_annotations() => Origin.StripAnnotations(new Annotate.AnnotationKeepDetails());
        public Amount strip_annotations(KeepDetails whatToKeep) => Origin.StripAnnotations(whatToKeep.Origin);

        public static void parse_conversion(string largerStr, string smallerStr) => Amounts.Amount.ParseConversion(largerStr, smallerStr);
        public bool valid() => Origin.Valid();

        public override bool Equals(object obj) => Origin.Equals(obj);
        public override int GetHashCode() => Origin.GetHashCode();
        public override string ToString() => Origin.ToString();
    }
}
