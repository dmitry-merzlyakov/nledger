// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using NLedger.Annotate;
using NLedger.Commodities;
using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Utility;
using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Values
{
    public sealed class Value
    {
        public static explicit operator Value(bool value)
        {
            return new Value(value);
        }

        public static explicit operator Value(DateTime value)
        {
            return new Value(value);
        }

        public static explicit operator Value(Date value)
        {
            return new Value(value);
        }

        public static explicit operator Value(long value)
        {
            return new Value(value);
        }

        public static explicit operator Value(double value)
        {
            return new Value(value);
        }

        public static explicit operator Value(Amount value)
        {
            return new Value(value);
        }

        public static explicit operator Value(Balance value)
        {
            return new Value(value);
        }

        public static explicit operator Value(Mask value)
        {
            return new Value(value);
        }

        public static explicit operator Value(List<Value> value)
        {
            return new Value(value);
        }

        public static bool operator ==(Value left, Value right)
        {
            if (Object.Equals(left, null))
                return Object.Equals(right, null);
            else
            {
                if (Object.Equals(right, null))
                    return false;
                else
                    return left.Equals(right);
            }
        }

        public static bool operator !=(Value left, Value right)
        {
            if (Object.Equals(left, null))
                return !Object.Equals(right, null);
            else
            {
                if (Object.Equals(right, null))
                    return true;
                else
                    return !left.Equals(right);
            }
        }

        public static Value operator -(Value val)
        {
            return val.Negated();
        }

        public static ValueTypeEnum GetValueType<T>()
        {
            return ValueStorageExtensions.GetValueType<T>();
        }

        public static Value Get<T>(T value)
        {
            if (value == null)
                return new Value();

            if (Object.Equals(value, default(T)) && !typeof(T).IsValueType)  // DM - TODO - concerns!!
                return new Value();

            if (typeof(T) == typeof(String) && String.IsNullOrEmpty((string)(object)value))
                return new Value();

            return new Value(ValueStorageExtensions.Create(value));
        }

        /// <summary>
        /// Ported from explicit value_t(const string& val, bool literal = false)
        /// </summary>
        public static Value Get(string value, bool literal = false)
        {
            if (literal)
                return new Value(ValueStorageExtensions.Create(value ?? String.Empty));
            else
                return Get(new Amount(value));
        }

        public static Value Clone(Value value)
        {
            return Value.IsNullOrEmpty(value) ? new Value() : new Value(ValueStorageExtensions.CreateFromObject(value.Storage.StoredValue));
        }

        public static bool IsNullOrEmpty(Value value)
        {
            return Object.Equals(value, null) || value.Type == ValueTypeEnum.Void;
        }

        public static bool IsNullOrEmptyOrFalse(Value value)
        {
            if (IsNullOrEmpty(value))
                return true;

            return !value.Bool;
        }

        public static Value SimplifiedValueOrZero(Value value)
        {
            return Value.IsNullOrEmpty(value) ? Value.Zero : value.Simplified();
        }

        public static Value ScopeValue(Scope scope)
        {
            return Get(scope);
        }

        /// <summary>
        /// Ported from inline value_t string_value(const string& str = "")
        /// </summary>
        public static Value StringValue(string str = "")
        {
            return Get(str, true);
        }

        public static Value AddOrSetValue(Value lhs, Value rhs)
        {
            if (Value.IsNullOrEmpty(lhs))
                return Clone(rhs);
            else
                return lhs.InPlaceAdd(rhs);
        }

        public static string ValueContext(Value value)
        {
            // [DM] val.print(buf, 20, 20, true); // true is 0x01 // it is AMOUNT_PRINT_RIGHT_JUSTIFY
            return value.Print(20, 20, AmountPrintEnum.AMOUNT_PRINT_RIGHT_JUSTIFY);
        }

        public static readonly Value Empty = new Value();

        public static readonly Value Zero = Value.Get(0);

        public static readonly Value One = Value.Get(1);

        public static readonly Value True = Value.Get(true);

        public static readonly Value False = Value.Get(false);

        public Value()
        { }

        public Value(bool val) : this(new BooleanValueStorage(val))
        { }

        public Value(DateTime val) : this(new DateTimeValueStorage(val))
        { }

        public Value(Date val) : this(new DateValueStorage(val))
        { }

        public Value(long val) : this(new IntegerValueStorage(val))
        { }

        public Value(double val) : this(new AmountValueStorage((Amount)val))
        { }

        public Value(Amount val) : this(new AmountValueStorage(val))
        { }

        public Value(Balance val) : this(new BalanceValueStorage(val))
        { }

        public Value(Mask val) : this(new MaskValueStorage(val))
        { }

        public Value(string val, bool literal = false) 
            : this(literal ? (IValueStorage)new StringValueStorage(val) : new AmountValueStorage(new Amount(val)))
        { }

        public Value(IList<Value> val) : this(new SequenceValueStorage(val))
        { }

        public Value(Value value) : this(value?.Storage)
        { }

        internal Value(IValueStorage storage)
        {
            Storage = storage;
        }

        public ValueTypeEnum Type
        {
            get { return Storage == null ? ValueTypeEnum.Void : Storage.Type; }
        }

        public long Size
        {
            get
            {
                if (IsNullOrEmpty(this))
                    return 0;
                else if (Storage.Type == ValueTypeEnum.Sequence)
                    return Storage.AsSequence.Count;
                else
                    return 1;
            }
        }

        // operator bool()
        public bool Bool
        {
            get { return Storage != null ? Storage.Bool : false; } // VOID is always False
        }

        public bool AsBoolean
        {
            get { return Storage != null ? Storage.AsBoolean : default(bool); }
        }

        public long AsLong
        {
            get { return Storage != null ? Storage.AsLong : 0; }
        }

        public Date AsDate
        {
            get { return Storage != null ? Storage.AsDate : default(Date); }
        }

        public DateTime AsDateTime
        {
            get { return Storage != null ? Storage.AsDateTime : default(DateTime); }
        }

        public Amount AsAmount
        {
            get { return Storage != null ? Storage.AsAmount : null; }
        }

        public Balance AsBalance
        {
            get { return Storage != null ? Storage.AsBalance : null; }
        }

        public Mask AsMask
        {
            get { return Storage != null ? Storage.AsMask : null; }
        }

        public Scope AsScope
        {
            get { return Storage != null ? Storage.AsScope : null; }
        }

        public IList<Value> AsSequence
        {
            get { return Storage != null ? Storage.AsSequence : null; }
        }

        public string AsString
        {
            get { return Storage != null ? Storage.AsString : null; }
        }

        public T AsAny<T>()
        {
            if (Type != ValueTypeEnum.Any)
                throw new ValueError("Wrong type");

            return Storage != null ? Storage.AsAny<T>() : default(T);
        }

        public object AsAny()
        {
            return Storage != null ? Storage.AsAny() : null;
        }

        public bool IsValid
        {
            get { return Storage != null ? Storage.IsValid : true; }
        }

        public bool IsZero
        {
            get { return Storage != null ? Storage.IsZero() : true; }
        }

        public bool IsNonZero
        {
            get { return !IsZero; }
        }

        public bool IsRealZero
        {
            get { return Storage != null ? Storage.IsRealZero() : true; }
        }

        public static Value operator +(Value valueA, Value valueB)
        {
            return Value.Clone(valueA).InPlaceAdd(valueB);
        }

        /// <summary>
        /// Ported from value_t& operator+=(const value_t& val);
        /// </summary>
        public Value InPlaceAdd(Value value)
        {
            if (IsNullOrEmpty(value))
                return this;

            if (IsNullOrEmpty(this))
            {
                Storage = value.Storage;
                return this;
            }

            Storage = Storage.Add(value.Storage);
            return this;
        }

        public static Value operator -(Value valueA, Value valueB)
        {
            return Value.Clone(valueA).InPlaceSubtract(valueB);
        }

        /// <summary>
        /// Ported from value_t& value_t::operator-=(const value_t& val)
        /// </summary>
        public Value InPlaceSubtract(Value value)
        {
            if (IsNullOrEmpty(value))
                return this;

            if (IsNullOrEmpty(this))
            {
                Storage = value.Storage;
                return this;
            }

            Storage = Storage.Subtract(value.Storage);
            return this;
        }

        public static Value operator *(Value valueA, Value valueB)
        {
            return Value.Clone(valueA).InPlaceMultiply(valueB);
        }

        /// <summary>
        /// Ported from value_t& value_t::operator*=(const value_t& val)
        /// </summary>
        public Value InPlaceMultiply(Value value)
        {
            if (IsNullOrEmpty(value) || IsNullOrEmpty(this))
                return Value.Empty;

            Storage = Storage.Multiply(value.Storage);
            return this;
        }

        public static Value operator /(Value valueA, Value valueB)
        {
            return Value.Clone(valueA).InPlaceDivide(valueB);
        }

        /// <summary>
        /// Ported from value_t& value_t::operator/=(const value_t& val)
        /// </summary>
        public Value InPlaceDivide(Value value)
        {
            if (IsNullOrEmpty(this))
                return Value.Empty;

            Storage = Storage.Divide(value.Storage);
            return this;
        }

        public Value Negated()
        {
            Value temp = Value.Clone(this);
            temp.InPlaceNegate();
            return temp;
        }

        public Value StripAnnotations(AnnotationKeepDetails whatToKeep)
        {
            IValueStorage storage = Storage != null ? Storage.StripAnnotations(whatToKeep) : null;
            return storage == Storage ? this : new Value(storage);
        }

        public bool HasAnnotation
        {
            get
            {
                if (Type == ValueTypeEnum.Amount)
                    return AsAmount.HasAnnotation;
                else
                    // DM - no sense...
                    //add_error_context(_f("While checking if %1% has annotations:") % *this);
                    //throw_(value_error,
                    //   _f("Cannot determine whether %1% is annotated") % label());
                    return false;
            }
        }

        public Annotation Annotation
        {
            get
            {
                if (Type == ValueTypeEnum.Amount)
                    return AsAmount.Annotation;
                else
                {
                    ErrorContext.Current.AddErrorContext(String.Format("While requesting the annotations of {0}:", this));
                    throw new ValueError(String.Format(ValueError.CannotRequestAnnotationOfSmth, this));
                }
            }
        }

        public Value ExchangeCommodities(string commodities, bool addPrices = false, DateTime moment = default(DateTime))
        {
            if (Type == ValueTypeEnum.Sequence)
            {
                Value temp = new Value();
                foreach (Value value in AsSequence)
                    temp.PushBack(value.ExchangeCommodities(commodities, addPrices, moment));
                return temp;
            }

            // If we are repricing to just a single commodity, with no price
            // expression, skip the expensive logic below.

            if (commodities.IndexOfAny(CommaOrEqual) < 0)
                return ValueOf(moment, CommodityPool.Current.FindOrCreate(commodities));

            IList<Commodity> comms = new List<Commodity>();
            IList<bool> force = new List<bool>();

            IEnumerable<string> tokens = commodities.Split(',').Select(s => s.Trim());
            foreach(string name in tokens)
            {
                int nameLen = name.Length;
                bool isForce = name.Last() == '!';

                Commodity commodity = CommodityPool.Current.ParsePriceExpression(isForce ? name.Remove(nameLen - 1) : name, addPrices, moment);
                if (commodity != null)
                {
                    Logger.Current.Debug("commodity.exchange", () => String.Format("Pricing for commodity: {0}", commodity.Symbol));
                    comms.Add(commodity.Referent);
                    force.Add(isForce);
                }
            }

            int index = 0;
            foreach(Commodity comm in comms)
            {
                switch(Type)
                {
                    case ValueTypeEnum.Amount:
                        Logger.Current.Debug("commodity.exchange", () => String.Format("We have an amount: {0}", AsAmount));
                        if (!force[index] && comms.Contains(AsAmount.Commodity.Referent))
                            break;

                        Logger.Current.Debug("commodity.exchange", () => "Referent doesn't match, pricing...");
                        Amount val = AsAmount.Value(moment, comm);
                        if (val != null)
                            return Value.Get(val);

                        Logger.Current.Debug("commodity.exchange", () => "Was unable to find a price");
                        break;

                    case ValueTypeEnum.Balance:
                        Balance temp = new Balance();
                        bool rePriced = false;

                        Logger.Current.Debug("commodity.exchange", () => String.Format("We have a balance: {0}", AsBalance));
                        foreach (KeyValuePair<Commodity,Amount> pair in AsBalance.Amounts)
                        {
                            Logger.Current.Debug("commodity.exchange", () => String.Format("We have a balance amount of commodity: {0} == {1}", pair.Key.Symbol, pair.Value.Commodity.Symbol));
                            if (!force[index] && comms.Contains(pair.Key.Referent))
                            {
                                temp = temp.Add(pair.Value);
                            }
                            else
                            {
                                Logger.Current.Debug("commodity.exchange", () => "Referent doesn't match, pricing...");
                                Amount val1 = pair.Value.Value(moment, comm);
                                if (val1 != null)
                                {
                                    Logger.Current.Debug("commodity.exchange", () => String.Format("Re-priced member amount is: {0}", val1));
                                    temp = temp.Add(val1);
                                    rePriced = true;
                                }
                                else
                                {
                                    Logger.Current.Debug("commodity.exchange", () => "Was unable to find price");
                                    temp = temp.Add(pair.Value);
                                }
                            }
                        }

                        if (rePriced)
                        {
                            Logger.Current.Debug("commodity.exchange", () => String.Format("Re-priced balance is: {0}", temp));
                            return Value.Get(temp);
                        }

                        break;

                    default:
                        break;
                }

                ++index;
            }

            return this;
        }

        // Return the "market value" of a given value at a specific time.
        // (equal to value(...
        public Value ValueOf(DateTime moment = default(DateTime), Commodity inTermOf = null)
        {
            switch (Type)
            {
                case ValueTypeEnum.Integer:
                    return Value.Empty;

                case ValueTypeEnum.Amount:
                    Amount val = AsAmount.Value(moment, inTermOf);
                    if (val != null)
                        return Value.Get(val);
                    return Value.Empty;

                case ValueTypeEnum.Balance:
                    Balance bal = AsBalance.Value(moment, inTermOf);
                    if (!Balance.IsNull(bal))
                        return Value.Get(bal);
                    return Value.Empty;

                case ValueTypeEnum.Sequence:
                    Value temp = new Value();
                    foreach (Value value in AsSequence)
                        temp.PushBack(value.ValueOf(moment, inTermOf));
                    return temp;

                default:
                    break;
            }

            ErrorContext.Current.AddErrorContext(String.Format(ValueError.WhileFindingValuationOfSmth, this));
            throw new ValueError(String.Format(ValueError.CannotFindTheValueOfSmth, this));
        }

        public Value Simplified()
        {
            if (Storage == null)
                return this;

            return new Value(Storage.Simplify());
        }

        public void PushBack(Value val)
        {
            if (Storage == null)
                Storage = ValueStorageExtensions.Create(new List<Value>());
            if (Storage.Type != ValueTypeEnum.Sequence)
                Storage = ValueStorageExtensions.Create(Storage.AsSequence);
            AsSequence.Add(val);
        }

        public void PushFront(Value val)
        {
            if (Storage == null)
                Storage = ValueStorageExtensions.Create(new List<Value>());
            if (Storage.Type != ValueTypeEnum.Sequence)
                Storage = ValueStorageExtensions.Create(Storage.AsSequence);
            AsSequence.Insert(0, val);
        }

        public void PopBack()
        {
            Validator.Verify(() => !IsNullOrEmpty(this));
            
            if (Type != ValueTypeEnum.Sequence)
            {
                Storage = null;
            }
            else
            {
                var sequence = AsSequence;
                if (sequence.Any())
                    sequence.RemoveAt(sequence.Count - 1);
                if (!sequence.Any())
                    Storage = null;
                else if (sequence.Count == 1)
                    Storage = sequence.Single().Storage;
            }
        }

        public Value this[int index]
        {
            get
            {
                Validator.Verify(() => !IsNullOrEmpty(this));
                if (Type == ValueTypeEnum.Sequence)
                    return AsSequence[index];
                else if (index == 0)
                    return this;

                throw new IndexOutOfRangeException();
            }
        }

        public override string ToString()
        {
            if (Type == ValueTypeEnum.String)
            {
                return AsString;
            }
            else
            {
                Value temp = Value.Clone(this);
                temp.InPlaceCast(ValueTypeEnum.String);
                return temp.AsString;
            }
        }

        public string Print(int firstWidth = -1, int latterWidth = -1, AmountPrintEnum flags = AmountPrintEnum.AMOUNT_PRINT_NO_FLAGS)
        {
            int outWidth = 0;
            bool outRight = false;

            if (firstWidth > 0 && (Type != ValueTypeEnum.Amount || AsAmount.IsZero) && Type != ValueTypeEnum.Balance && Type != ValueTypeEnum.String)
            {
                outWidth = firstWidth;

                if (flags.HasFlag(AmountPrintEnum.AMOUNT_PRINT_RIGHT_JUSTIFY))
                    outRight = true;
            }

            string result = Storage != null ? Storage.Print(firstWidth, latterWidth, flags) : String.Empty;  // "" means VOID

            if (outWidth > 0)
            {
                string format = String.Format("{{0,{0}{1}}}", outRight ? "" : "-", outWidth);
                return String.Format(format, result);
            }
            else
                return result;
        }

        public string Dump(bool relaxed = true)
        {
            return Storage != null ? Storage.Dump(relaxed) : "null";  // "null" means VOID
        }

        public Value InPlaceCast(ValueTypeEnum type)
        {
            if (Storage == null)  // VOID
            {
                switch (type)
                {
                    case ValueTypeEnum.Integer:
                        Storage = ValueStorageExtensions.Create(0);
                        return this;
                    case ValueTypeEnum.Amount:
                        Storage = ValueStorageExtensions.Create(new Amount(0));
                        return this;
                    case ValueTypeEnum.String:
                        Storage = ValueStorageExtensions.Create(String.Empty);
                        return this;
                    default:
                        throw new InvalidOperationException(String.Format("Cannot cast VOID to {0}", type));
                }
            }

            // TODO - verify this approach
            switch(type)
            {
                case ValueTypeEnum.Integer:
                    Storage = ValueStorageExtensions.Create(Storage.AsLong);
                    return this;
                case ValueTypeEnum.Boolean:
                    Storage = ValueStorageExtensions.Create(Storage.AsBoolean);
                    return this;
                case ValueTypeEnum.Date:
                    Storage = ValueStorageExtensions.Create(Storage.AsDate);
                    return this;
                case ValueTypeEnum.DateTime:
                    Storage = ValueStorageExtensions.Create(Storage.AsDateTime);
                    return this;
                case ValueTypeEnum.Amount:
                    Storage = ValueStorageExtensions.Create(Storage.AsAmount);
                    return this;
                case ValueTypeEnum.Balance:
                    Storage = ValueStorageExtensions.Create(Storage.AsBalance);
                    return this;
                case ValueTypeEnum.String:
                    Storage = ValueStorageExtensions.Create(Storage.AsString);
                    return this;
                case ValueTypeEnum.Mask:
                    Storage = ValueStorageExtensions.Create(Storage.AsMask);
                    return this;
                case ValueTypeEnum.Any:
                    Storage = ValueStorageExtensions.Create(Storage.StoredValue);
                    return this;
                case ValueTypeEnum.Scope:
                    Storage = ValueStorageExtensions.Create(Storage.AsScope);
                    return this;
                case ValueTypeEnum.Sequence:
                    Storage = ValueStorageExtensions.Create(Storage.AsSequence);
                    return this;
                default:
                    throw new InvalidOperationException();
            }
        }

        public Value Rounded()
        {
            Value temp = Value.Clone(this);
            temp.InPlaceRound();
            return temp;
        }

        public void InPlaceRound()
        {
            switch (Type)
            {
                case ValueTypeEnum.Integer:
                    return;

                case ValueTypeEnum.Amount:
                    AsAmount.InPlaceRound();
                    return;

                case ValueTypeEnum.Balance:
                    AsBalance.InPlaceRound();
                    return;

                case ValueTypeEnum.Sequence:
                    foreach (Value value in AsSequence)
                        value.InPlaceRound();
                    return;

                default:
                    ErrorContext.Current.AddErrorContext(String.Format("While rounding {0}:", this));
                    throw new ValueError(String.Format(ValueError.CannotSetRoundingForSmth, this));
            }
        }

        public Value Unrounded()
        {
            Value temp = Value.Clone(this);
            temp.InPlaceUnround();
            return temp;
        }

        public void InPlaceUnround()
        {
            switch (Type)
            {
                case ValueTypeEnum.Integer:
                    return;

                case ValueTypeEnum.Amount:
                    AsAmount.InPlaceUnround();
                    return;

                case ValueTypeEnum.Balance:
                    AsBalance.InPlaceUnround();
                    return;

                case ValueTypeEnum.Sequence:
                    foreach (Value value in AsSequence)
                        value.InPlaceUnround();
                    return;

                default:
                    ErrorContext.Current.AddErrorContext(String.Format("While unrounding {0}:", this));
                    throw new ValueError(String.Format(ValueError.CannotUnroundSmth, this));
            }
        }

        public Value Floored()
        {
            Value temp = Value.Clone(this);
            temp.InPlaceFloor();
            return temp;
        }

        public void InPlaceFloor()
        {
            switch (Type)
            {
                case ValueTypeEnum.Integer:
                    return;

                case ValueTypeEnum.Amount:
                    AsAmount.InPlaceFloor();
                    return;

                case ValueTypeEnum.Balance:
                    AsBalance.InPlaceFloor();
                    return;

                case ValueTypeEnum.Sequence:
                    foreach (Value value in AsSequence)
                        value.InPlaceFloor();
                    return;

                default:
                    ErrorContext.Current.AddErrorContext(String.Format("While flooring {0}:", this));
                    throw new ValueError(String.Format(ValueError.CannotFloorSmth, this));
            }
        }

        public Value Ceilinged()
        {
            Value temp = Value.Clone(this);
            temp.InPlaceCeiling();
            return temp;
        }

        public void InPlaceCeiling()
        {
            switch (Type)
            {
                case ValueTypeEnum.Integer:
                    return;

                case ValueTypeEnum.Amount:
                    AsAmount.InPlaceCeiling();
                    return;

                case ValueTypeEnum.Balance:
                    AsBalance.InPlaceCeiling();
                    return;

                case ValueTypeEnum.Sequence:
                    foreach (Value value in AsSequence)
                        value.InPlaceCeiling();
                    return;

                default:
                    ErrorContext.Current.AddErrorContext(String.Format("While ceiling {0}:", this));
                    throw new ValueError(String.Format(ValueError.CannotCeilingSmth, this));
            }
        }

        public Value RoundTo(int places)
        {
            Value temp = Value.Clone(this);
            temp.InPlaceRoundTo(places);
            return temp;
        }

        public void InPlaceRoundTo(int places)
        {
            Logger.Current.Debug("amount.roundto", () => String.Format("=====> roundto places {0}", places));
            switch (Type)
            {
                case ValueTypeEnum.Integer:
                    return;

                case ValueTypeEnum.Amount:
                    AsAmount.InPlaceRoundTo(places);
                    return;

                case ValueTypeEnum.Balance:
                    AsBalance.InPlaceRoundTo(places);
                    return;

                case ValueTypeEnum.Sequence:
                    foreach (Value value in AsSequence)
                        value.InPlaceRoundTo(places);
                    return;

                default:
                    break;
            }
        }

        public Value Abs()
        {
            switch (Type)
            {
                case ValueTypeEnum.Integer:
                    long val = AsLong;
                    return Value.Get(val < 0 ? -val : val);

                case ValueTypeEnum.Amount:
                    return Value.Get(AsAmount.Abs());

                case ValueTypeEnum.Balance:
                    return Value.Get(AsBalance.Abs());

                default:
                    ErrorContext.Current.AddErrorContext(String.Format("While taking abs of {0}:", this));
                    throw new ValueError(String.Format(ValueError.CannotAbsSmth, this));                    
            }
        }

        public Value Number()
        {
            switch (Type)
            {
                case ValueTypeEnum.Void:
                    return Value.Zero;

                case ValueTypeEnum.Boolean:
                    return AsBoolean ? Value.One : Value.Zero;

                case ValueTypeEnum.Integer:
                    return Value.Get(AsLong);

                case ValueTypeEnum.Amount:
                    return Value.Get(AsAmount.Number());

                case ValueTypeEnum.Balance:
                    return Value.Get(AsBalance.Number());

                case ValueTypeEnum.Sequence:
                    if (AsSequence.Any())
                    {
                        Value temp = new Value();
                        foreach(Value value in AsSequence)
                            temp.InPlaceAdd(value.Number());
                        return temp;
                    }
                    break;

                default:
                    break;
            }
            ErrorContext.Current.AddErrorContext(String.Format("While calling number() on {0}:", this));
            throw new ValueError(String.Format(ValueError.CannotDetermineNumericValueOfSmth, this));
        }

        public Value Unreduced()
        {
            Value temp = Value.Clone(this);
            temp.InPlaceUnreduce();
            return temp;
        }

        public void InPlaceUnreduce()
        {
            if (Storage != null)
                Storage = Storage.Unreduce();
        }

        public Value Truncated()
        {
            Value temp = Value.Clone(this);
            temp.InPlaceTruncate();
            return temp;
        }

        public void InPlaceTruncate()
        {
            if (Storage != null)
                Storage = Storage.Truncate();
            else
                throw new ValueError("Cannot truncate VOID");
        }

        public void InPlaceNot()
        {
            if (Storage != null)
                Storage = new BooleanValueStorage(Storage.IsZero());
            else
                throw new ValueError("Cannot 'not' VOID");
        }

        public void InPlaceNegate()
        {
            if (Storage != null)
                Storage = Storage.Negate();
            else
                throw new ValueError("Cannot negate VOID");
        }

        /**
         * Informational methods.
         */
        public string Label(ValueTypeEnum? theType = null)
        {
            switch(theType ?? Type)
            {
                case ValueTypeEnum.Void: return "an uninitialized value";
                case ValueTypeEnum.Boolean: return "a boolean";
                case ValueTypeEnum.DateTime: return "a date/time";
                case ValueTypeEnum.Date: return "a date";
                case ValueTypeEnum.Integer: return "an integer";
                case ValueTypeEnum.Amount: return "an amount";
                case ValueTypeEnum.Balance: return "a balance";
                case ValueTypeEnum.String: return "a string";
                case ValueTypeEnum.Mask: return "a regexp";
                case ValueTypeEnum.Sequence: return "a sequence";
                case ValueTypeEnum.Scope: return "a scope";
                case ValueTypeEnum.Any: return AsAny().SafeGetType() == typeof(ExprOp) ? "an expr" : "an object";
                default:
                    throw new InvalidOperationException(String.Format("Unknown type: {0}", theType ?? Type));
            }
        }

        public bool IsEqualTo(Value value)
        {
            return IsEqualTo(value?.Storage);
        }

        public bool IsLessThan(Value value)
        {
            return IsLessThan(value?.Storage);
        }

        public bool IsGreaterThan(Value value)
        {
            return IsGreaterThan(value?.Storage);
        }

        public bool Equals(Value value)
        {
            return IsEqualTo(value);
        }

        public override bool Equals(object obj)
        {
            if (obj is Value)
                return this.Equals((Value)obj);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return Storage != null ? Storage.GetHashCode() : 0;
        }

        internal bool IsEqualTo(IValueStorage valueStorage)
        {
            if (Storage == null)
                return valueStorage == null;
            else
                return Storage.IsEqualTo(valueStorage);
        }

        internal bool IsLessThan(IValueStorage valueStorage)
        {
            if (Storage == null)
                return false;
            else
                return Storage.IsLessThan(valueStorage);
        }

        internal bool IsGreaterThan(IValueStorage valueStorage)
        {
            if (Storage == null)
                return false;
            else
                return Storage.IsGreaterThan(valueStorage);
        }

        private IValueStorage Storage { get; set; }
        private static readonly char[] CommaOrEqual = new char[] { ',', '=' };
    }

}
