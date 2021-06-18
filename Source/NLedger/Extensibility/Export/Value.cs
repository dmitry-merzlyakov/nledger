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
        public static implicit operator Value(bool val) => new Value(new Values.Value(val));
        public static implicit operator Value(long val) => new Value(new Values.Value(val));
        public static implicit operator Value(string val) => new Value(new Values.Value(val));
        public static implicit operator Value(Amount val) => new Value(new Values.Value(val.Origin));
        public static implicit operator Value(Balance val) => new Value(new Values.Value(val.Origin));
        public static implicit operator Value(Mask val) => new Value(new Values.Value(val.Origin));
        public static implicit operator Value(Date val) => new Value(new Values.Value(val));
        public static implicit operator Value(DateTime val) => new Value(new Values.Value(val));

        public static readonly Value NULL_VALUE = new Value(Values.Value.Empty);
        public static Value string_value(string str) => Values.Value.StringValue(str);
        public static Value mask_value(string str) => Values.Value.MaskValue(str);
        public static string value_context(Value val) => Values.Value.ValueContext(val.Origin);

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
        public void set_boolean(bool value) => Origin.SetBoolean(value);
        public bool is_datetime() => Origin.Type == Values.ValueTypeEnum.DateTime;
        public void set_datetime(DateTime value) => Origin.SetDateTime(value);
        public bool is_date() => Origin.Type == Values.ValueTypeEnum.Date;
        public void set_date(Date value) => Origin.SetDate(value);
        public bool is_long() => Origin.Type == Values.ValueTypeEnum.Integer;
        public void set_long(long value) => Origin.SetLong(value);
        public bool is_amount() => Origin.Type == Values.ValueTypeEnum.Amount;
        public void set_amount(Amount value) => Origin.SetAmount(value.Origin);
        public bool is_balance() => Origin.Type == Values.ValueTypeEnum.Balance;
        public void set_balance(Balance value) => Origin.SetBalance(value.Origin);
        public bool is_string() => Origin.Type == Values.ValueTypeEnum.String;
        public void set_string(string value) => Origin.SetString(value);
        public bool is_mask() => Origin.Type == Values.ValueTypeEnum.Mask;
        public void set_mask(Mask value) => Origin.SetMask(value.Origin);
        public bool is_sequence() => Origin.Type == Values.ValueTypeEnum.Sequence;
        public void set_sequence(IEnumerable<Value> value) => Origin.SetSequence(value?.Select(v => v.Origin).ToList());

        public bool to_boolean() => Origin.AsBoolean;
        public long to_long() => Origin.AsLong;
        public DateTime to_datetime() => Origin.AsDateTime;
        public Date to_date() => Origin.AsDate;
        public Amount to_amount() => Origin.AsAmount;
        public Balance to_balance() => Origin.AsBalance;
        public string to_string() => Origin.AsString;
        public Mask to_mask() => Origin.AsMask;
        public IList<Value> to_sequence() => Origin.AsSequence.Select(v => (Value)v).ToList();

        public Value casted(ValueType type) => Origin.Casted((Values.ValueTypeEnum)type);
        public void in_place_cast(ValueType type) => Origin.InPlaceCast((Values.ValueTypeEnum)type);
        public Value simplified() => Origin.Simplified();
        public void in_place_simplify() => Origin.InPlaceSimplify();
        public Value number() => Origin.Number();

        public void annotate(Annotation details) => Origin.Annotate(details.Origin);
        public bool has_annotation() => Origin.HasAnnotation;
        public Annotation annotation => Origin.Annotation;
        public Value strip_annotations() => Origin.StripAnnotations(new Annotate.AnnotationKeepDetails());
        public Value strip_annotations(KeepDetails keepDetails) => Origin.StripAnnotations(keepDetails.Origin);

        public void push_back(Value value) => Origin.PushBack(value.Origin);
        public void pop_back() => Origin.PopBack();
        public long size() => Origin.Size;
        public string label() => Origin.Label();
        public string label(ValueType type) => Origin.Label((Values.ValueTypeEnum)type);
        public bool valid() => Origin.IsValid;

        public Type basetype() => Origin.BaseType();

        public override bool Equals(object obj) => Origin.Equals(obj);
        public override int GetHashCode() => Origin.GetHashCode();
        public override string ToString() => Origin.ToString();
    }
}
