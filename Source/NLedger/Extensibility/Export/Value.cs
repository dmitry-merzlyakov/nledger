using NLedger.Times;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    [Flags]
    public enum ValueType
    {
        Void = Values.ValueTypeEnum.Void,
        Boolean = Values.ValueTypeEnum.Boolean,
        DateTime = Values.ValueTypeEnum.DateTime,
        Date = Values.ValueTypeEnum.Date,
        Integer = Values.ValueTypeEnum.Integer,
        Amount = Values.ValueTypeEnum.Amount,
        Balance = Values.ValueTypeEnum.Balance,
        String = Values.ValueTypeEnum.String,
        Sequence = Values.ValueTypeEnum.Sequence,
        Scope = Values.ValueTypeEnum.Scope
    }

    public class Value : BaseExport<Values.Value>
    {
        public static implicit operator Value(Values.Value val) => new Value(val);
        public static implicit operator Value(long val) => new Value(new Values.Value(val));
        public static implicit operator Value(Amount val) => new Value(new Values.Value(val.Origin));
        public static implicit operator Value(Balance val) => new Value(new Values.Value(val.Origin));

        public static bool operator ==(Value left, Value right) => left.Origin == right.Origin;
        public static bool operator !=(Value left, Value right) => left.Origin != right.Origin;

        public static bool operator ==(Value left, long right) => left.Origin == (Value)right;
        public static bool operator !=(Value left, long right) => left.Origin != (Value)right;

        public static bool operator ==(Value left, Amount right) => left.Origin == (Value)right;
        public static bool operator !=(Value left, Amount right) => left.Origin != (Value)right;

        public static bool operator ==(Value left, Balance right) => left.Origin == (Value)right;
        public static bool operator !=(Value left, Balance right) => left.Origin != (Value)right;

        public static explicit operator bool(Value value) => (bool)value.Origin;
        public static Value operator -(Value value) => -value.Origin;

        public static bool operator <(Value left, Value right) => left.Origin < right.Origin;
        public static bool operator >(Value left, Value right) => left.Origin > right.Origin;

        public static bool operator <(Value left, long right) => left.Origin < (Value)right;
        public static bool operator >(Value left, long right) => left.Origin > (Value)right;

        public static bool operator <(Value left, Amount right) => left.Origin < (Value)right;
        public static bool operator >(Value left, Amount right) => left.Origin > (Value)right;

        public static Value operator +(Value left, Value right) => left.Origin + right.Origin;
        public static Value operator +(Value left, long right) => left.Origin + (Value)right;
        public static Value operator +(Value left, Amount right) => left.Origin + (Value)right;
        public static Value operator +(Value left, Balance right) => left.Origin + (Value)right;

        public static Value operator -(Value left, Value right) => left.Origin - right.Origin;
        public static Value operator -(Value left, long right) => left.Origin - (Value)right;
        public static Value operator -(Value left, Amount right) => left.Origin - (Value)right;
        public static Value operator -(Value left, Balance right) => left.Origin - (Value)right;

        public static Value operator *(Value left, Value right) => left.Origin * right.Origin;
        public static Value operator *(Value left, long right) => left.Origin * (Value)right;
        public static Value operator *(Value left, Amount right) => left.Origin * (Value)right;

        public static Value operator /(Value left, Value right) => left.Origin / right.Origin;
        public static Value operator /(Value left, long right) => left.Origin / (Value)right;
        public static Value operator /(Value left, Amount right) => left.Origin / (Value)right;

        protected Value(Values.Value origin) : base(origin)
        { }

        // public static void initialize() => TODO - not implemented
        // public static void shutdown() => TODO - not implemented

        public Value(bool val) : this(new Values.Value(val))
        { }

        public Value(DateTime val) : this(new Values.Value(val))
        { }

        public Value(Date val) : this(new Values.Value(val))
        { }

        public Value(long val) : this(new Values.Value(val))
        { }

        public Value(double val) : this(new Values.Value(val))
        { }

        public Value(Amount val) : this(new Values.Value(val.Origin))
        { }

        public Value(Balance val) : this(new Values.Value(val.Origin))
        { }

        public Value(Mask val) : this(new Values.Value(val.Origin))
        { }

        public Value(string val) : this(new Values.Value(val))
        { }

        public Value(Value val) : this(new Values.Value(val.Origin))
        { }

        public bool is_equal_to(Value val) => Origin.IsEqualTo(val.Origin);
        public bool is_less_than(Value val) => Origin.IsLessThan(val.Origin);
        public bool is_greater_than(Value val) => Origin.IsGreaterThan(val.Origin);

        public Value negated() => Origin.Negated();
        public void in_place_negate() => Origin.InPlaceNegate();
        public void in_place_not() => Origin.InPlaceNot();

        public Value abs() => Origin.Abs();

        public Value rounded() => Origin.Rounded();
        public void in_place_round() => Origin.InPlaceRound();
        public Value truncated() => Origin.Truncated();
        public void in_place_truncate() => Origin.InPlaceTruncate();
        public Value floored() => Origin.Floored();
        public void in_place_floor() => Origin.InPlaceFloor();
        public Value unrounded() => Origin.Unrounded();
        public void in_place_unround() => Origin.InPlaceUnround();
        public Value reduced() => Origin.Reduced();
        public void in_place_reduce() => Origin.InPlaceReduce();
        public Value unreduced() => Origin.Unreduced();
        public void in_place_unreduce() => Origin.InPlaceUnreduce();

        public Value value() => Origin.ValueOf(TimesCommon.Current.CurrentTime);
        public Value value(Commodity in_terms_of) => Origin.ValueOf(TimesCommon.Current.CurrentTime, in_terms_of?.Origin);
        public Value value(Commodity in_terms_of, DateTime moment) => Origin.ValueOf(moment, in_terms_of?.Origin);
        public Value value(Commodity in_terms_of, Date moment) => Origin.ValueOf(moment, in_terms_of?.Origin);

        public Value exchange_commodities(string commodities) => Origin.ExchangeCommodities(commodities);
        public Value exchange_commodities(string commodities, bool addPrices) => Origin.ExchangeCommodities(commodities, addPrices);
        public Value exchange_commodities(string commodities, bool addPrices, DateTime moment) => Origin.ExchangeCommodities(commodities, addPrices, moment);

        public bool is_nonzero() => Origin.IsNonZero;
        public bool is_realzero() => Origin.IsRealZero;
        public bool is_zero() => Origin.IsZero;
        public bool is_null() => Values.Value.IsNullOrEmpty(Origin);

        public Values.ValueTypeEnum type() => Origin.Type;
        public bool type(Values.ValueTypeEnum typeEnum) => Origin.Type == typeEnum;

        public bool is_boolean() => Origin.Type == Values.ValueTypeEnum.Boolean;
        //public void set_boolean(bool value) => Origin.
        //TBC


        public override bool Equals(object obj) => Origin.Equals(obj);
        public override int GetHashCode() => Origin.GetHashCode();
        public override string ToString() => Origin.ToString();
    }
}
