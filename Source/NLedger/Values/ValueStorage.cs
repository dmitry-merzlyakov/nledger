// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using NLedger.Annotate;
using NLedger.Commodities;
using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Times;
using NLedger.Utility;
using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Values
{
    public interface IValueStorage
    {
        ValueTypeEnum Type { get; }

        bool Bool { get; }
        bool AsBoolean { get; }
        Date AsDate { get; }
        DateTime AsDateTime { get; }
        long AsLong { get; }
        Amount AsAmount { get; }
        Balance AsBalance { get; }
        string AsString { get; }
        Mask AsMask { get; }
        IList<Value> AsSequence { get; }
        Scope AsScope { get; }
        object AsAny();
        T AsAny<T>();
        bool IsValid { get; }
        object StoredValue { get; }

        string Dump(bool relaxed);
        string Print(int firstWidth, int latterWidth, AmountPrintEnum flags);

        bool IsEqualTo(IValueStorage storage);
        bool IsLessThan(IValueStorage storage);
        bool IsGreaterThan(IValueStorage storage);

        bool IsZero();
        bool IsRealZero();
        IValueStorage Add(IValueStorage valueStorage);
        IValueStorage Subtract(IValueStorage valueStorage);
        IValueStorage Multiply(IValueStorage valueStorage);
        IValueStorage Divide(IValueStorage valueStorage);
        IValueStorage Negate();
        IValueStorage StripAnnotations(AnnotationKeepDetails whatToKeep);
        IValueStorage Simplify();
        IValueStorage Truncate();
        IValueStorage Reduce();
        IValueStorage Unreduce();
    }

    public static class ValueStorageExtensions
    {
        public static ValueTypeEnum GetValueType<T>()
        {
            if (typeof(T) == typeof(Boolean))
                return ValueTypeEnum.Boolean;
            else if (typeof(T) == typeof(Date))
                return ValueTypeEnum.Date;
            else if (typeof(T) == typeof(DateTime))
                return ValueTypeEnum.DateTime;
            else if (typeof(T) == typeof(int))
                return ValueTypeEnum.Integer;
            else if (typeof(T) == typeof(long))
                return ValueTypeEnum.Integer;
            else if (typeof(T) == typeof(Amount))
                return ValueTypeEnum.Amount;
            else if (typeof(T) == typeof(Balance))
                return ValueTypeEnum.Balance;
            else if (typeof(T) == typeof(String))
                return ValueTypeEnum.String;
            else if (typeof(T) == typeof(Mask))
                return ValueTypeEnum.Mask;
            else if (typeof(Scope).IsAssignableFrom(typeof(T)))
                return ValueTypeEnum.Scope;
            else if (typeof(IList<Value>).IsAssignableFrom(typeof(T)))
                return ValueTypeEnum.Sequence;
            else
                return ValueTypeEnum.Void;
        }

        public static IValueStorage Create<T>(T value)
        {
            if (typeof(T) == typeof(Boolean))
                return new BooleanValueStorage((bool)(object)value);
            else if (typeof(T) == typeof(Date))
                return new DateValueStorage((Date)(object)value);
            else if (typeof(T) == typeof(DateTime))
                return new DateTimeValueStorage((DateTime)(object)value);
            else if (typeof(T) == typeof(DateTime?))
                return Create((DateTime)(object)value);
            else if (typeof(T) == typeof(int))
                return new IntegerValueStorage((int)(object)value);
            else if (typeof(T) == typeof(long))
                return new IntegerValueStorage((long)(object)value);
            else if (typeof(T) == typeof(Amount))
                return new AmountValueStorage((Amount)(object)value);
            else if (typeof(T) == typeof(Balance))
                return new BalanceValueStorage((Balance)(object)value);
            else if (typeof(T) == typeof(String))
                return new StringValueStorage((String)(object)value);
            else if (typeof(T) == typeof(Mask))
                return new MaskValueStorage((Mask)(object)value);
            else if (typeof(Scope).IsAssignableFrom(typeof(T)))
                return new ScopeValueStorage((Scope)(object)value);
            else if (typeof(IList<Value>).IsAssignableFrom(typeof(T)))
                return new SequenceValueStorage((IList<Value>)(object)value);
            else
                return new AnyValueStorage<T>(value);
                //throw new InvalidOperationException(String.Format("Could not create a Value instance for '{0}'", typeof(T)));
        }

        public static IValueStorage CreateFromObject(object value)
        {
            if (value == null)
                return null;

            Type type = value.GetType();
            if (type == typeof(Boolean))
                return new BooleanValueStorage((bool)(object)value);
            else if (type == typeof(Date))
                return new DateValueStorage((Date)(object)value);
            else if (type == typeof(DateTime))
                return new DateTimeValueStorage((DateTime)(object)value);
            else if (type == typeof(int))
                return new IntegerValueStorage((int)(object)value);
            else if (type == typeof(long))
                return new IntegerValueStorage((long)(object)value);
            else if (type == typeof(Amount))
                return new AmountValueStorage(new Amount((Amount)(object)value));
            else if (type == typeof(Balance))
                return new BalanceValueStorage(new Balance((Balance)(object)value));
            else if (type == typeof(String))
                return new StringValueStorage((String)(object)value);
            else if (type == typeof(Mask))
                return new MaskValueStorage((Mask)(object)value);
            else if (typeof(Scope).IsAssignableFrom(type))
                return new ScopeValueStorage((Scope)(object)value);
            else if (typeof(IList<Value>).IsAssignableFrom(type))
                return new SequenceValueStorage(((IList<Value>)(object)value).Select(v => Value.Clone(v)).ToList());
            else
            {
                Type anyType = typeof(AnyValueStorage<>).MakeGenericType(type);
                return (IValueStorage)Activator.CreateInstance(anyType, new object[] { value });
            }
        }

        public static ValueTypeEnum SafeType(this IValueStorage storage)
        {
            return storage != null ? storage.Type : ValueTypeEnum.Void;
        }
    }

    public abstract class ValueStorage<T> : IValueStorage
    {
        public ValueStorage(ValueTypeEnum type, T val)
        {
            Type = type;
            Val = val;
        }

        public ValueTypeEnum Type { get; private set; }
        public T Val { get; private set; }

        public object StoredValue
        {
            get { return Val; }
        }

        public virtual bool Bool
        {
            get { return !IsRealZero(); }
        }

        public abstract bool AsBoolean { get; }
        public abstract Date AsDate { get; }
        public abstract DateTime AsDateTime { get; }
        public abstract long AsLong { get; }
        public abstract Amount AsAmount { get; }
        public abstract Balance AsBalance { get; }
        public virtual string AsString
        {
            get { return ToString(); }
        }
        public abstract Mask AsMask { get; }
        public virtual IList<Value> AsSequence
        {
            get { return new List<Value>() { Value.Get(Val) }; }
        }

        public virtual Scope AsScope
        {
            get { throw new ValueError(String.Format("Cannot convert {0} to Scope", typeof(T))); }
        }

        public object AsAny()
        {
            return Val;
        }

        public T1 AsAny<T1>()
        {
            // #remove-boxing - Consider removing excess boxing in this method.
            object val = (object)Val;
            if (val == null)
                return (T1)val;

            Type valType = val.GetType();
            if (typeof(T).IsAssignableFrom(valType))
                return (T1)val;

            return (T1)Convert.ChangeType(val, typeof(T1));
        }

        public virtual bool IsValid
        {
            get { return true; }
        }

        public virtual bool IsZero()
        {
            return IsRealZero();
        }

        public virtual bool IsRealZero()
        {
            throw new ValueError(String.Format(ValueError.CannotDetermineIfItIsReallyZero, this));
        }

        public IValueStorage Add(IValueStorage valueStorage)
        {
            if (valueStorage == null || valueStorage.Type == ValueTypeEnum.Void)
                return this;

            return AddValueStorage(valueStorage);
        }
        protected virtual IValueStorage AddValueStorage(IValueStorage valueStorage)
        {
            throw new ValueError(String.Format("Cannot add {0} to {1}", Type, valueStorage.Type));
        }

        public IValueStorage Subtract(IValueStorage valueStorage)
        {
            if (valueStorage == null || valueStorage.Type == ValueTypeEnum.Void)
                return this;

            return SubtractValueStorage(valueStorage);
        }
        protected virtual IValueStorage SubtractValueStorage(IValueStorage valueStorage)
        {
            throw new ValueError(String.Format("Cannot subtract {0} from {1}", Type, valueStorage.Type));
        }

        public IValueStorage Multiply(IValueStorage valueStorage)
        {
            return MultiplyValueStorage(valueStorage);
        }
        protected virtual IValueStorage MultiplyValueStorage(IValueStorage valueStorage)
        {
            throw new ValueError(String.Format("Cannot multiply {0} with {1}", Type, valueStorage.Type));
        }

        public IValueStorage Divide(IValueStorage valueStorage)
        {
            return DivideValueStorage(valueStorage);
        }
        protected virtual IValueStorage DivideValueStorage(IValueStorage valueStorage)
        {
            throw new ValueError(String.Format("Cannot divide {0} by {1}", Type, valueStorage.Type));
        }

        public virtual IValueStorage Negate()
        {
            throw new ValueError(String.Format("Cannot Negate {0}", Type));
        }

        public virtual IValueStorage Truncate()
        {
            throw new ValueError(String.Format("Cannot truncate {0}", Type));
        }

        public virtual IValueStorage Unreduce()
        {
            return this;
        }

        public virtual IValueStorage Reduce()
        {
            return this;
        }

        public virtual IValueStorage StripAnnotations(AnnotationKeepDetails whatToKeep)
        {
            return this;
        }

        /// <summary>
        /// Not exactly copied but very close to void value_t::in_place_simplify()
        /// </summary>
        public IValueStorage Simplify()
        {
            if (IsRealZero())
            {
                Logger.Current.Debug("value.simplify", () => String.Format("Zeroing type {0}", Type));
                return new IntegerValueStorage(0);
            }

            IValueStorage value = this;
            if (Type == ValueTypeEnum.Balance && AsBalance.IsSingleAmount)
            {
                Logger.Current.Debug("value.simplify", () => "Reducing balance to amount");
                Logger.Current.Debug("value.simplify", () => String.Format("as a balance it looks like: {0}", Dump(false)));
                value = new AmountValueStorage(AsAmount);
                Logger.Current.Debug("value.simplify", () => String.Format("as an amount it looks like: {0}", value.ToString()));
            }

            // [DM] - commented out according to the source code - #if REDUCE_TO_INTEGER        // this is off by default
            //if (value.Type == ValueTypeEnum.Amount && !value.AsAmount.HasCommodity && value.AsAmount.FitsInLong)
            //    return new IntegerValueStorage(value.AsAmount.Quantity.ToLong());

            return value;
        }

        public override string ToString()
        {
            return Val != null ? Val.ToString() : String.Empty;
        }

        public virtual bool IsEqualTo(IValueStorage storage)
        {
            throw new ValueError(String.Format(ValueError.CannotCompareSmthToSmth, Type, storage.SafeType()));
        }

        public virtual bool IsLessThan(IValueStorage storage)
        {
            throw new ValueError(String.Format(ValueError.CannotCompareSmthToSmth, Type, storage.SafeType()));
        }

        public virtual bool IsGreaterThan(IValueStorage storage)
        {
            throw new ValueError(String.Format(ValueError.CannotCompareSmthToSmth, Type, storage.SafeType()));
        }

        public abstract string Dump(bool relaxed);
        public abstract string Print(int firstWidth, int latterWidth, AmountPrintEnum flags);
    }

    public sealed class BooleanValueStorage : ValueStorage<Boolean>
    {
        public BooleanValueStorage(bool val)
            : base(ValueTypeEnum.Boolean, val)
        {  }

        public override bool AsBoolean 
        {
            get { return Val; }
        }

        public override Date AsDate
        {
            get { throw new ValueError(ValueError.CannotConvertBooleanToDate); }
        }

        public override DateTime AsDateTime 
        {
            get { throw new ValueError(ValueError.CannotConvertBooleanToDateTime); }
        }

        public override long AsLong 
        {
            get { return AsBoolean ? 1 : 0; }
        }

        public override Amount AsAmount 
        {
            get { return new Amount(AsLong); }
        }

        public override Balance AsBalance
        {
            get { throw new ValueError(ValueError.CannotConvertBooleanToBalance); }
        }

        public override string AsString 
        { 
            get { return Val ? "true" : "false"; }
        }

        public override Mask AsMask
        {
            get { throw new ValueError(ValueError.CannotConvertBooleanToMask); }
        }

        public override bool IsRealZero()
        {
            return !Val;
        }

        public override IValueStorage Negate()
        {
            return new BooleanValueStorage(!AsBoolean);
        }

        public override bool IsEqualTo(IValueStorage storage)
        {
            if (storage.SafeType() == ValueTypeEnum.Boolean)
                return AsBoolean == storage.AsBoolean;
            else
                return base.IsEqualTo(storage);
        }

        public override bool IsLessThan(IValueStorage storage)
        {
            if (storage.SafeType() == ValueTypeEnum.Boolean)
                return AsLong < storage.AsLong;
            else
                return base.IsLessThan(storage);
        }

        public override bool IsGreaterThan(IValueStorage storage)
        {
            if (storage.SafeType() == ValueTypeEnum.Boolean)
                return AsLong > storage.AsLong;
            else
                return base.IsLessThan(storage);
        }

        public override string Dump(bool relaxed)
        {
            return AsBoolean ? "true" : "false";
        }

        public override string Print(int firstWidth, int latterWidth, AmountPrintEnum flags)
        {
            return AsBoolean ? "1" : "0";
        }
    }

    public sealed class DateTimeValueStorage : ValueStorage<DateTime>
    {
        public DateTimeValueStorage(DateTime val)
            : base(ValueTypeEnum.DateTime, val)
        { }

        public override bool Bool
        {
            get { return !IsRealZero(); }
        }

        public override bool AsBoolean
        {
            get { return Val != default(DateTime); }
        }

        public override Date AsDate
        {
            get { return (Date)Val; }
        }

        public override DateTime AsDateTime
        {
            get { return Val; }
        }

        public override long AsLong
        {
            get { return Val.Ticks; }
        }

        public override Amount AsAmount
        {
            get { return new Amount(AsLong); }
        }

        public override Balance AsBalance
        {
            get { throw new ValueError(ValueError.CannotConvertDateTimeToBalance); }
        }

        public override Mask AsMask
        {
            get { throw new ValueError(ValueError.CannotConvertDateTimeToMask); }
        }

        public override bool IsRealZero()
        {
            return Val == default(DateTime);
        }

        public override bool IsEqualTo(IValueStorage storage)
        {
            if (storage.SafeType() == ValueTypeEnum.DateTime)
                return AsDateTime == storage.AsDateTime;
            else
                return base.IsEqualTo(storage);
        }

        public override bool IsLessThan(IValueStorage storage)
        {
            if (storage.SafeType() == ValueTypeEnum.DateTime)
                return AsDateTime < storage.AsDateTime;
            else
                return base.IsLessThan(storage);
        }

        public override bool IsGreaterThan(IValueStorage storage)
        {
            if (storage.SafeType() == ValueTypeEnum.DateTime)
                return AsDateTime > storage.AsDateTime;
            else
                return base.IsGreaterThan(storage);
        }

        protected override IValueStorage AddValueStorage(IValueStorage valueStorage)
        {
            if (valueStorage.Type == ValueTypeEnum.Integer || valueStorage.Type == ValueTypeEnum.Amount)
                return new DateTimeValueStorage(AsDateTime + TimeSpan.FromSeconds(valueStorage.AsLong));
            else
                return base.AddValueStorage(valueStorage);
        }

        protected override IValueStorage SubtractValueStorage(IValueStorage valueStorage)
        {
            if (valueStorage.Type == ValueTypeEnum.Integer || valueStorage.Type == ValueTypeEnum.Amount)
                return new DateTimeValueStorage(AsDateTime - TimeSpan.FromSeconds(valueStorage.AsLong));
            else
                return base.SubtractValueStorage(valueStorage);
        }

        public override string Dump(bool relaxed)
        {
            return TimesCommon.Current.FormatDateTime(AsDateTime);
        }

        public override string Print(int firstWidth, int latterWidth, AmountPrintEnum flags)
        {
            return TimesCommon.Current.FormatDateTime(AsDateTime, FormatTypeEnum.FMT_WRITTEN);
        }

        public override string AsString
        {
            get { return TimesCommon.Current.FormatDateTime(AsDateTime, FormatTypeEnum.FMT_WRITTEN); }
        }
    }

    public sealed class DateValueStorage : ValueStorage<Date>
    {
        public DateValueStorage(Date val)
            : base(ValueTypeEnum.Date, val)
        { }

        public override bool Bool
        {
            get { return !IsRealZero(); }
        }

        public override bool AsBoolean
        {
            get { return Val.IsValid(); }
        }

        public override Date AsDate
        {
            get { return Val; }
        }

        public override DateTime AsDateTime
        {
            get { return Val; }
        }

        public override long AsLong
        {
            get { return Val.Ticks; }
        }

        public override Amount AsAmount
        {
            get { return new Amount(AsLong); }
        }

        public override Balance AsBalance
        {
            get { throw new ValueError(ValueError.CannotConvertDateTimeToBalance); }
        }

        public override Mask AsMask
        {
            get { throw new ValueError(ValueError.CannotConvertDateTimeToMask); }
        }

        public override bool IsRealZero()
        {
            return Val == default(Date);
        }

        public override bool IsEqualTo(IValueStorage storage)
        {
            if (storage.SafeType() == ValueTypeEnum.Date)
                return AsDate == storage.AsDate;
            else
                return base.IsEqualTo(storage);
        }

        public override bool IsLessThan(IValueStorage storage)
        {
            if (storage.SafeType() == ValueTypeEnum.Date)
                return AsDate < storage.AsDate;
            else
                return base.IsLessThan(storage);
        }

        public override bool IsGreaterThan(IValueStorage storage)
        {
            if (storage.SafeType() == ValueTypeEnum.Date)
                return AsDate > storage.AsDate;
            else
                return base.IsGreaterThan(storage);
        }

        protected override IValueStorage AddValueStorage(IValueStorage valueStorage)
        {
            if (valueStorage.Type == ValueTypeEnum.Integer || valueStorage.Type == ValueTypeEnum.Amount)
                return new DateValueStorage(AsDate + TimeSpan.FromDays(valueStorage.AsLong));
            else
                return base.AddValueStorage(valueStorage);
        }

        protected override IValueStorage SubtractValueStorage(IValueStorage valueStorage)
        {
            if (valueStorage.Type == ValueTypeEnum.Integer || valueStorage.Type == ValueTypeEnum.Amount)
                return new DateValueStorage(AsDate - TimeSpan.FromDays(valueStorage.AsLong));
            else
                return base.SubtractValueStorage(valueStorage);
        }

        public override string Dump(bool relaxed)
        {
            return TimesCommon.Current.FormatDate(AsDate);
        }

        public override string Print(int firstWidth, int latterWidth, AmountPrintEnum flags)
        {
            return TimesCommon.Current.FormatDate(AsDate, FormatTypeEnum.FMT_WRITTEN);
        }

        public override string AsString
        {
            get { return TimesCommon.Current.FormatDate(AsDate, FormatTypeEnum.FMT_WRITTEN); }
        }
    }

    public sealed class IntegerValueStorage : ValueStorage<long>
    {
        public IntegerValueStorage(long val)
            : base(ValueTypeEnum.Integer, val)
        { }

        public override bool AsBoolean
        {
            get { return Val != default(long); }
        }

        public override Date AsDate
        {
            get { return (Date)AsDateTime; }
        }

        public override DateTime AsDateTime
        {
            get { return new DateTime(Val); }
        }

        public override long AsLong
        {
            get { return Val; }
        }

        public override Amount AsAmount
        {
            get { return new Amount(Val); }
        }

        public override Balance AsBalance
        {
            get { return new Balance(AsAmount); }
        }

        public override Mask AsMask
        {
            get { throw new ValueError(ValueError.CannotConvertLongToMask); }
        }

        public override bool IsRealZero()
        {
            return Val == 0;
        }

        public override IValueStorage Negate()
        {
            return new IntegerValueStorage(-AsLong);
        }

        public override bool IsEqualTo(IValueStorage storage)
        {
            switch (storage.SafeType())
            {
                case ValueTypeEnum.Integer:
                    return AsLong == storage.AsLong;

                case ValueTypeEnum.Amount:
                    return storage.AsAmount.Equals(AsAmount);

                case ValueTypeEnum.Balance:
                    return storage.AsBalance.Equals(AsAmount);

                default:
                    return base.IsEqualTo(storage);
            }
        }

        public override bool IsLessThan(IValueStorage storage)
        {
            switch (storage.SafeType())
            {
                case ValueTypeEnum.Integer:
                    return AsLong < storage.AsLong;

                case ValueTypeEnum.Amount:
                    return storage.AsAmount.IsGreaterThan(AsAmount);

                case ValueTypeEnum.Balance:
                    return storage.AsAmount.IsGreaterThan(AsAmount);

                default:
                    return base.IsEqualTo(storage);
            }
        }

        public override bool IsGreaterThan(IValueStorage storage)
        {
            switch (storage.SafeType())
            {
                case ValueTypeEnum.Integer:
                    return AsLong > storage.AsLong;

                case ValueTypeEnum.Amount:
                    return AsAmount.IsGreaterThan(storage.AsAmount);

                case ValueTypeEnum.Balance:
                    return AsAmount.IsGreaterThan(storage.AsAmount);

                default:
                    return base.IsEqualTo(storage);
            }
        }

        protected override IValueStorage AddValueStorage(IValueStorage valueStorage)
        {
            if (valueStorage.Type == ValueTypeEnum.Integer)
            {
                return new IntegerValueStorage(AsLong + valueStorage.AsLong);
            }
            else if (valueStorage.Type == ValueTypeEnum.Amount)
            {
                if (valueStorage.AsAmount.HasCommodity)
                    return new BalanceValueStorage(new Balance(AsAmount).Add(valueStorage.AsAmount));
                return new AmountValueStorage(AsAmount.InPlaceAdd(valueStorage.AsAmount));
            }
            else if (valueStorage.Type == ValueTypeEnum.Balance)
            {
                return new BalanceValueStorage(new Balance(AsAmount).Add(valueStorage.AsBalance));
            }
            else
                return base.AddValueStorage(valueStorage);
        }

        protected override IValueStorage SubtractValueStorage(IValueStorage valueStorage)
        {
            if (valueStorage.Type == ValueTypeEnum.Integer)
            {
                return new IntegerValueStorage(AsLong - valueStorage.AsLong);
            }
            else if (valueStorage.Type == ValueTypeEnum.Amount)
            {
                return new AmountValueStorage(AsAmount.InPlaceSubtract(valueStorage.AsAmount)).Simplify();
            }
            else if (valueStorage.Type == ValueTypeEnum.Balance)
            {
                return new BalanceValueStorage(new Balance(AsAmount).Subtract(valueStorage.AsBalance)).Simplify();
            }
            else
                return base.SubtractValueStorage(valueStorage);
        }

        protected override IValueStorage MultiplyValueStorage(IValueStorage valueStorage)
        {
            if (valueStorage.SafeType() == ValueTypeEnum.Integer)
                return new IntegerValueStorage(AsLong * valueStorage.AsLong);

            if (valueStorage.SafeType() == ValueTypeEnum.Amount)
                return new AmountValueStorage(valueStorage.AsAmount * AsAmount);

            return base.MultiplyValueStorage(valueStorage);
        }

        protected override IValueStorage DivideValueStorage(IValueStorage valueStorage)
        {
            if (valueStorage.SafeType() == ValueTypeEnum.Integer)
                return new IntegerValueStorage(AsLong / valueStorage.AsLong);

            // [DM] Initial c# code for [Integer]/[Amount] division: return new AmountValueStorage(valueStorage.AsAmount / AsAmount);
            // This initial code reflected the original Ledger code that is likely contains a mistake:
            //    value.cc - value_t& value_t::operator/=(const value_t& val); case INTEGER/case AMOUNT:
            //       set_amount(val.as_amount() / as_long());
            // In the source code the dividend and divisor are reversed that causes wrong division results. Notice that previous division INTEGER/INTEGER is correct.
            // It was decided to fix this problem in c# code since wrong division results cause negative effect on integrated capabilities.

            if (valueStorage.SafeType() == ValueTypeEnum.Amount)
                return new AmountValueStorage(AsAmount / valueStorage.AsAmount);

            return base.DivideValueStorage(valueStorage);
        }

        public override string Dump(bool relaxed)
        {
            return AsLong.ToString();
        }

        public override string Print(int firstWidth, int latterWidth, AmountPrintEnum flags)
        {
            if (flags.HasFlag(AmountPrintEnum.AMOUNT_PRINT_COLORIZE) && AsLong < 0)
                return UniString.Justify(ToString(), firstWidth, flags.HasFlag(AmountPrintEnum.AMOUNT_PRINT_RIGHT_JUSTIFY), true);
            else
                return AsLong.ToString();
        }

        public override  IValueStorage Truncate()
        {
            return this;
        }
    }

    public sealed class AmountValueStorage : ValueStorage<Amount>
    {
        public AmountValueStorage(Amount val)
            : base(ValueTypeEnum.Amount, val)
        {
            Validator.Verify(() => val.Valid());
        }

        public override bool Bool
        {
            get { return (bool)AsAmount; }
        }

        public override bool AsBoolean
        {
            get { return !Val.IsEmpty; }
        }

        public override Date AsDate
        {
            get { return (Date)AsDateTime; }
        }

        public override DateTime AsDateTime
        {
            get { return new DateTime(Val.Quantity.ToLong()); }
        }

        public override long AsLong
        {
            get { return Val.Quantity.ToLong(); }
        }

        public override Amount AsAmount
        {
            get { return Val; }
        }

        public override Balance AsBalance
        {
            get { return new Balance(AsAmount); }
        }

        public override Mask AsMask
        {
            get { throw new ValueError(ValueError.CannotConvertAmountToMask); }
        }

        public override bool IsValid
        {
            get { return AsAmount.Valid(); }
        }

        public override bool IsZero()
        {
            return Val.IsZero;
        }

        public override bool IsRealZero()
        {
            return Val.IsRealZero;
        }

        public override IValueStorage Negate()
        {
            AsAmount.InPlaceNegate();
            return this;
        }

        public override bool IsEqualTo(IValueStorage storage)
        {
            switch (storage.SafeType())
            {
                case ValueTypeEnum.Integer:
                    return AsAmount.Equals(storage.AsAmount);

                case ValueTypeEnum.Amount:
                    return AsAmount.Equals(storage.AsAmount);

                case ValueTypeEnum.Balance:
                    return storage.AsBalance.Equals(AsAmount);

                default:
                    return base.IsEqualTo(storage);
            }
        }

        public override bool IsLessThan(IValueStorage storage)
        {
            switch (storage.SafeType())
            {
                case ValueTypeEnum.Integer:
                    return AsAmount.IsLessThan(storage.AsAmount);

                case ValueTypeEnum.Amount:
                    if (AsAmount.Commodity == storage.AsAmount.Commodity || !AsAmount.HasCommodity || !storage.AsAmount.HasCommodity)
                        return AsAmount.IsLessThan(storage.AsAmount);
                    else
                        return Commodity.CompareByCommodity(AsAmount, storage.AsAmount) < 0;

                case ValueTypeEnum.Balance:
                    return AsAmount.IsLessThan(storage.AsAmount);

                default:
                    return base.IsEqualTo(storage);
            }
        }

        public override bool IsGreaterThan(IValueStorage storage)
        {
            switch (storage.SafeType())
            {
                case ValueTypeEnum.Integer:
                    return AsAmount.IsGreaterThan(storage.AsAmount);

                case ValueTypeEnum.Amount:
                    return AsAmount.IsGreaterThan(storage.AsAmount);

                case ValueTypeEnum.Balance:
                    return AsAmount.IsGreaterThan(storage.AsAmount);

                default:
                    return base.IsEqualTo(storage);
            }
        }

        protected override IValueStorage AddValueStorage(IValueStorage valueStorage)
        {
            if (valueStorage.Type == ValueTypeEnum.Integer)
            {
                if (AsAmount.HasCommodity)
                    return new BalanceValueStorage(new Balance(AsAmount).Add(valueStorage.AsAmount));
                else
                    return new AmountValueStorage(AsAmount.InPlaceAdd(new Amount(valueStorage.AsLong)));
            }
            else if (valueStorage.Type == ValueTypeEnum.Amount)
            {
                if (AsAmount.Commodity != valueStorage.AsAmount.Commodity)
                    return new BalanceValueStorage(new Balance(AsAmount).Add(valueStorage.AsAmount));
                else
                    return new AmountValueStorage(AsAmount.InPlaceAdd(valueStorage.AsAmount));
            }
            else if (valueStorage.Type == ValueTypeEnum.Balance)
            {
                return new BalanceValueStorage(new Balance(AsAmount).Add(valueStorage.AsBalance));
            }
            else
                return base.AddValueStorage(valueStorage);
        }

        protected override IValueStorage SubtractValueStorage(IValueStorage valueStorage)
        {
            if (valueStorage.Type == ValueTypeEnum.Integer)
            {
                if (AsAmount.HasCommodity)
                    return new BalanceValueStorage(new Balance(AsAmount).Subtract(valueStorage.AsAmount)).Simplify();
                else
                    return new AmountValueStorage(AsAmount.InPlaceSubtract(new Amount(valueStorage.AsLong))).Simplify();
            }
            else if (valueStorage.Type == ValueTypeEnum.Amount)
            {
                if (AsAmount.Commodity != valueStorage.AsAmount.Commodity)
                    return new BalanceValueStorage(new Balance(AsAmount).Subtract(valueStorage.AsAmount)).Simplify();
                else
                    return new AmountValueStorage(AsAmount.InPlaceSubtract(valueStorage.AsAmount)).Simplify();
            }
            else if (valueStorage.Type == ValueTypeEnum.Balance)
            {
                return new BalanceValueStorage(new Balance(AsAmount).Subtract(valueStorage.AsBalance)).Simplify();
            }
            else
                return base.SubtractValueStorage(valueStorage);
        }

        protected override IValueStorage MultiplyValueStorage(IValueStorage valueStorage)
        {
            if (valueStorage.SafeType() == ValueTypeEnum.Integer)
                return new AmountValueStorage(AsAmount.Multiply(valueStorage.AsAmount));

            if (valueStorage.SafeType() == ValueTypeEnum.Amount)
                return new AmountValueStorage(AsAmount.Multiply(valueStorage.AsAmount));

            if (valueStorage.SafeType() == ValueTypeEnum.Balance)
                if (valueStorage.AsBalance.IsSingleAmount)
                    return new AmountValueStorage(AsAmount.Multiply(valueStorage.Simplify().AsAmount));

            return base.MultiplyValueStorage(valueStorage);
        }

        protected override IValueStorage DivideValueStorage(IValueStorage valueStorage)
        {
            if (valueStorage.SafeType() == ValueTypeEnum.Integer)
                return new AmountValueStorage(AsAmount / valueStorage.AsAmount);

            if (valueStorage.SafeType() == ValueTypeEnum.Amount)
                return new AmountValueStorage(AsAmount / valueStorage.AsAmount);

            if (valueStorage.SafeType() == ValueTypeEnum.Balance)
            {
                if (valueStorage.AsBalance.IsSingleAmount)
                {
                    IValueStorage simpler = valueStorage.Simplify();
                    if (simpler.Type == ValueTypeEnum.Integer)
                        return new AmountValueStorage(AsAmount / simpler.AsAmount);
                    else if (simpler.Type == ValueTypeEnum.Amount)
                        return new AmountValueStorage(AsAmount / simpler.AsAmount);
                    else
                        throw new InvalidOperationException();
                }
            }

            return base.DivideValueStorage(valueStorage);
        }

        public override IValueStorage StripAnnotations(AnnotationKeepDetails whatToKeep)
        {
            return new AmountValueStorage(AsAmount.StripAnnotations(whatToKeep));
        }

        public override string Dump(bool relaxed)
        {
            return !relaxed ? String.Format("{{{0}}}", AsAmount) : AsAmount.ToString();
        }

        public override string Print(int firstWidth, int latterWidth, AmountPrintEnum flags)
        {
            if (AsAmount.IsZero)
                return "0";
            else
            {
                string str = AsAmount.Print(flags);
                return UniString.Justify(str, firstWidth, flags.HasFlag(AmountPrintEnum.AMOUNT_PRINT_RIGHT_JUSTIFY),
                    flags.HasFlag(AmountPrintEnum.AMOUNT_PRINT_COLORIZE) && AsAmount.Sign < 0);
            }
        }

        public override IValueStorage Truncate()
        {
            AsAmount.InPlaceTruncate();
            return this;
        }

        public override IValueStorage Reduce()
        {
            AsAmount.InPlaceReduce();
            return this;
        }

        public override IValueStorage Unreduce()
        {
            AsAmount.InPlaceUnreduce();
            return this;
        }
    }

    public sealed class BalanceValueStorage : ValueStorage<Balance>
    {
        public BalanceValueStorage(Balance val)
            : base(ValueTypeEnum.Balance, val)
        {
            Validator.Verify(() => val.Valid());
        }

        public override bool Bool
        {
            get { return (bool)AsBalance; }
        }

        public override bool AsBoolean
        {
            get { throw new ValueError(ValueError.CannotConvertBalanceToBoolean); }
        }

        public override Date AsDate
        {
            get { throw new ValueError(ValueError.CannotConvertBalanceToDate); }
        }

        public override DateTime AsDateTime
        {
            get { throw new ValueError(ValueError.CannotConvertBalanceToDateTime); }
        }

        public override long AsLong
        {
            get { throw new ValueError(ValueError.CannotConvertBalanceToInteger); }
        }

        public override Amount AsAmount
        {
            get 
            {
                if (Val.IsSingleAmount)
                    return Val.SingleAmount;
                else if (Val.IsEmpty)
                    return new Amount(0);
                else
                    throw new ValueError(ValueError.CannotConvertBalanceWithMultipleCommoditiesToAmount);
            }
        }

        public override Balance AsBalance
        {
            get { return Val; }
        }

        public override Mask AsMask
        {
            get { throw new ValueError(ValueError.CannotConvertBalanceToMask); }
        }

        public override bool IsValid
        {
            get { return AsBalance.Valid(); }
        }

        public override bool IsZero()
        {
            return Val.IsZero;
        }

        public override bool IsRealZero()
        {
            return Val.IsRealZero;
        }

        public override bool IsEqualTo(IValueStorage storage)
        {
            switch (storage.SafeType())
            {
                case ValueTypeEnum.Integer:
                    return AsBalance.Equals(storage.AsAmount);

                case ValueTypeEnum.Amount:
                    return AsBalance.Equals(storage.AsAmount);

                case ValueTypeEnum.Balance:
                    return AsBalance.Equals(storage.AsBalance);

                default:
                    return base.IsEqualTo(storage);
            }
        }

        public override bool IsLessThan(IValueStorage storage)
        {
            if (storage.SafeType() == ValueTypeEnum.Integer || storage.SafeType() == ValueTypeEnum.Amount)
                return AsBalance.IsLessThan(storage.AsAmount);

            if (storage.SafeType() == ValueTypeEnum.Balance)
                return AsAmount.IsLessThan(storage.AsAmount);

            return base.IsEqualTo(storage);
        }

        public override bool IsGreaterThan(IValueStorage storage)
        {
            if (storage.SafeType() == ValueTypeEnum.Integer || storage.SafeType() == ValueTypeEnum.Amount)
                return AsBalance.IsGreaterThan(storage.AsAmount);

            if (storage.SafeType() == ValueTypeEnum.Balance)
                return AsAmount.IsGreaterThan(storage.AsAmount);

            return base.IsEqualTo(storage);
        }

        public override IValueStorage Negate()
        {
            AsBalance.InPlaceNegate();
            return this;
        }

        protected override IValueStorage AddValueStorage(IValueStorage valueStorage)
        {
            if (valueStorage.Type == ValueTypeEnum.Integer || valueStorage.Type == ValueTypeEnum.Amount)
            {
                return new BalanceValueStorage(AsBalance.Add(valueStorage.AsAmount));
            }
            else if (valueStorage.Type == ValueTypeEnum.Balance)
            {
                return new BalanceValueStorage(AsBalance.Add(valueStorage.AsBalance));
            }
            else
                return base.AddValueStorage(valueStorage);
        }

        protected override IValueStorage SubtractValueStorage(IValueStorage valueStorage)
        {
            if (valueStorage.Type == ValueTypeEnum.Integer || valueStorage.Type == ValueTypeEnum.Amount)
            {
                return new BalanceValueStorage(AsBalance.Subtract(valueStorage.AsAmount)).Simplify();
            }
            else if (valueStorage.Type == ValueTypeEnum.Balance)
            {
                return new BalanceValueStorage(AsBalance.Subtract(valueStorage.AsBalance)).Simplify();
            }
            else
                return base.SubtractValueStorage(valueStorage);
        }

        protected override IValueStorage MultiplyValueStorage(IValueStorage valueStorage)
        {
            if (valueStorage.SafeType() == ValueTypeEnum.Integer)
                return new BalanceValueStorage(AsBalance.Multiply(valueStorage.AsAmount));

            if (valueStorage.SafeType() == ValueTypeEnum.Amount)
            {
                if (AsBalance.IsSingleAmount)
                    return new AmountValueStorage(Simplify().AsAmount.Multiply(valueStorage.AsAmount));
                else
                    return new BalanceValueStorage(AsBalance.Multiply(valueStorage.AsAmount));
            }

            return base.MultiplyValueStorage(valueStorage);
        }

        protected override IValueStorage DivideValueStorage(IValueStorage valueStorage)
        {
            if (valueStorage.SafeType() == ValueTypeEnum.Integer)
                return new BalanceValueStorage(AsBalance.Divide(valueStorage.AsAmount));

            if (valueStorage.SafeType() == ValueTypeEnum.Amount)
            {
                if (AsBalance.IsSingleAmount)
                {
                    return new AmountValueStorage(AsAmount / valueStorage.AsAmount);
                }
                else
                {
                    if (!valueStorage.AsAmount.HasCommodity)
                        return new BalanceValueStorage(AsBalance.Divide(valueStorage.AsAmount));
                }
            }

            return base.DivideValueStorage(valueStorage);
        }

        public override IValueStorage StripAnnotations(AnnotationKeepDetails whatToKeep)
        {
            return new BalanceValueStorage(AsBalance.StripAnnotations(whatToKeep));
        }

        public override string Dump(bool relaxed)
        {
            return AsBalance.ToString();
        }

        public override string Print(int firstWidth, int latterWidth, AmountPrintEnum flags)
        {
            return AsBalance.Print(firstWidth, latterWidth, flags);
        }

        public override IValueStorage Truncate()
        {
            AsBalance.InPlaceTruncate();
            return this;
        }

        public override IValueStorage Reduce()
        {
            AsBalance.InPlaceReduce();
            return this;
        }

        public override IValueStorage Unreduce()
        {
            AsBalance.InPlaceUnreduce();
            return this;
        }
    }

    public sealed class StringValueStorage : ValueStorage<String>
    {
        public StringValueStorage(string val)
            : base(ValueTypeEnum.String, val)
        { }

        public override bool AsBoolean
        {
            get { return String.Equals(Val, "true", StringComparison.InvariantCultureIgnoreCase); }
        }

        public override Date AsDate
        {
            get { return TimesCommon.Current.ParseDate(Val); }
        }

        public override DateTime AsDateTime
        {
            get { return TimesCommon.Current.ParseDateTime(Val); }
        }

        public override long AsLong
        {
            get { return String.IsNullOrEmpty(Val) ? default(long) : long.Parse(Val); }
        }

        public override Amount AsAmount
        {
            get { return new Amount(AsString); }
        }

        public override Balance AsBalance
        {
            get { throw new ValueError(ValueError.CannotConvertStringToBalance); }
        }

        public override Mask AsMask
        {
            get { return new Mask(Val); }
        }

        public override bool IsRealZero()
        {
            return String.IsNullOrEmpty(Val);
        }

        public override bool IsEqualTo(IValueStorage storage)
        {
            if (storage.Type == ValueTypeEnum.String)
                return AsString == storage.AsString;
            else
                return base.IsEqualTo(storage);
        }

        public override bool IsLessThan(IValueStorage storage)
        {
            if (storage.SafeType() == ValueTypeEnum.String)
                return AsString.CompareTo(storage.AsString) < 0;

            return base.IsEqualTo(storage);
        }

        public override bool IsGreaterThan(IValueStorage storage)
        {
            if (storage.SafeType() == ValueTypeEnum.String)
                return AsString.CompareTo(storage.AsString) > 0;

            return base.IsEqualTo(storage);
        }

        protected override IValueStorage AddValueStorage(IValueStorage valueStorage)
        {
            return new StringValueStorage(Val + valueStorage.ToString());
        }

        protected override IValueStorage MultiplyValueStorage(IValueStorage valueStorage)
        {
            StringBuilder sb = new StringBuilder();
            string temp = string.Empty;
            for (int i = 0; i < valueStorage.AsLong; i++)
                sb.Append(AsString);
            return new StringValueStorage(sb.ToString());
        }

        public override string Dump(bool relaxed)
        {
            return "\"" + AsString.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }

        public override string Print(int firstWidth, int latterWidth, AmountPrintEnum flags)
        {
            if (firstWidth > 0)
                return UniString.Justify(AsString, firstWidth, flags.HasFlag(AmountPrintEnum.AMOUNT_PRINT_RIGHT_JUSTIFY));
            else
                return AsString;
        }
    }

    public sealed class MaskValueStorage : ValueStorage<Mask>
    {
        public MaskValueStorage(Mask val)
            : base(ValueTypeEnum.Mask, val)
        { }

        public override bool Bool
        {
            get
            {
                throw new ValueError(String.Format(ValueError.CannotDetermineTruthOfSmth, this, Val));
            }
        }

        public override bool AsBoolean
        {
            get { return Val != null; }
        }

        public override Date AsDate
        {
            get { throw new ValueError(ValueError.CannotConvertMaskToDate); }
        }

        public override DateTime AsDateTime
        {
            get { throw new ValueError(ValueError.CannotConvertMaskToDateTime); }
        }

        public override long AsLong
        {
            get { throw new ValueError(ValueError.CannotConvertMaskToLong); }
        }

        public override Amount AsAmount
        {
            get { throw new ValueError(ValueError.CannotConvertMaskToAmount); }
        }

        public override Balance AsBalance
        {
            get { throw new ValueError(ValueError.CannotConvertMaskToBalance); }
        }

        public override Mask AsMask
        {
            get { return Val; }
        }

        public override bool IsEqualTo(IValueStorage storage)
        {
            if (storage.Type == ValueTypeEnum.Mask)
                return AsMask.ToString() == storage.AsMask.ToString();
            else
                return base.IsEqualTo(storage);
        }

        public override string Dump(bool relaxed)
        {
            return String.Format("/{0}/", AsMask);
        }

        public override string Print(int firstWidth, int latterWidth, AmountPrintEnum flags)
        {
            return Dump(false);
        }
    }

    public sealed class SequenceValueStorage : ValueStorage<IList<Value>>
    {
        public SequenceValueStorage(IList<Value> val)
            : base(ValueTypeEnum.Sequence, new List<Value>(val))
        { }

        public override bool Bool
        {
            get { return AsSequence.Any(v => v.Bool); }
        }
            
        public override bool AsBoolean
        {
            get { return Val != null; }
        }

        public override Date AsDate
        {
            get { throw new ValueError(ValueError.CannotConvertSequenceToDate); }
        }

        public override DateTime AsDateTime
        {
            get { throw new ValueError(ValueError.CannotConvertSequenceToDateTime); }
        }

        public override long AsLong
        {
            get { throw new ValueError(ValueError.CannotConvertSequenceToLong); }
        }

        public override Amount AsAmount
        {
            get { throw new ValueError(ValueError.CannotConvertSequenceToAmount); }
        }

        public override Balance AsBalance
        {
            get { throw new ValueError(ValueError.CannotConvertSequenceToBalance); }
        }

        public override Mask AsMask
        {
            get { throw new ValueError(ValueError.CannotConvertSequenceToMask); }
        }

        public override IList<Value> AsSequence
        {
            get { return Val; }
        }

        public override bool IsRealZero()
        {
            return !AsSequence.Any();
        }

        public override IValueStorage Negate()
        {
            var sequence = AsSequence;
            for (int i = 0; i < sequence.Count; i++)
                sequence[i] = sequence[i].Negated();
            return this;
        }

        public override bool IsEqualTo(IValueStorage storage)
        {
            if (storage.Type == ValueTypeEnum.Sequence)
                return Enumerable.SequenceEqual(AsSequence, storage.AsSequence);
            else
                return base.IsEqualTo(storage);
        }

        public override bool IsLessThan(IValueStorage storage)
        {
            if (storage.SafeType() == ValueTypeEnum.Integer || storage.SafeType() == ValueTypeEnum.Amount)
                return AsSequence.All(v => v.IsGreaterThan(storage));

            return base.IsEqualTo(storage);
        }

        public override bool IsGreaterThan(IValueStorage storage)
        {
            if (storage.SafeType() == ValueTypeEnum.Integer || storage.SafeType() == ValueTypeEnum.Amount)
                return AsSequence.All(v => v.IsLessThan(storage));

            return base.IsEqualTo(storage);
        }

        protected override IValueStorage AddValueStorage(IValueStorage valueStorage)
        {
            if (valueStorage.Type == ValueTypeEnum.Sequence)
            {
                IList<Value> thisList = AsSequence;
                IList<Value> valueList = valueStorage.AsSequence;
                if (thisList.Count == valueList.Count)
                {
                    for (int i = 0; i < thisList.Count; i++)
                        thisList[i].InPlaceAdd(valueList[i]);
                }
                else
                {
                    ErrorContext.Current.AddErrorContext(String.Format("While adding {0} to {1}:", valueStorage.AsString, AsString));
                    throw new ValueError("Cannot add sequences of different lengths");
                }
            }
            else
            {
                AsSequence.Add(new Value(valueStorage));
            }

            return this;
        }

        protected override IValueStorage SubtractValueStorage(IValueStorage valueStorage)
        {
            if (valueStorage.Type == ValueTypeEnum.Sequence)
            {
                IList<Value> thisList = AsSequence;
                IList<Value> valueList = valueStorage.AsSequence;
                if (thisList.Count == valueList.Count)
                {
                    for (int i = 0; i < thisList.Count; i++)
                        thisList[i].InPlaceSubtract(valueList[i]);
                }
                else
                {
                    ErrorContext.Current.AddErrorContext(String.Format("While subtracting {0} to {1}:", valueStorage.AsString, AsString));
                    throw new ValueError("Cannot subtract sequences of different lengths");
                }
            }
            else
            {
                Value value = AsSequence.FirstOrDefault(val => val.IsEqualTo(valueStorage));
                if (value != null)
                    AsSequence.Remove(value);
            }

            return this;
        }

        protected override IValueStorage MultiplyValueStorage(IValueStorage valueStorage)
        {
            IValueStorage storage = this;
            for (int i = 0; i < (valueStorage.AsLong - 1); i++)
                storage = storage.Add(storage);
            return storage;
        }

        public override IValueStorage StripAnnotations(AnnotationKeepDetails whatToKeep)
        {
            IList<Value> list = new List<Value>();
            foreach (Value value in AsSequence)
                list.Add(value.StripAnnotations(whatToKeep));
            return new SequenceValueStorage(list);
        }

        public override string Dump(bool relaxed)
        {
            StringBuilder sb = new StringBuilder("(");
            bool first = true;
            foreach (Value value in AsSequence)
            {
                if (first)
                    first = false;
                else
                    sb.Append(", ");

                sb.Append(value.Dump(relaxed));
            }
            sb.Append(")");
            return sb.ToString();
        }

        public override string Print(int firstWidth, int latterWidth, AmountPrintEnum flags)
        {
            StringBuilder sb = new StringBuilder("(");
            bool first = true;
            foreach (Value value in AsSequence)
            {
                if (first)
                    first = false;
                else
                    sb.Append(", ");

                sb.Append(value.Print(firstWidth, latterWidth, flags));
            }
            sb.Append(")");
            return sb.ToString();
        }

        public override IValueStorage Reduce()
        {
            var sequence = AsSequence;
            for (int i = 0; i < sequence.Count; i++)
                sequence[i].InPlaceReduce();
            return this;
        }

        public override IValueStorage Unreduce()
        {
            var sequence = AsSequence;
            for (int i = 0; i < sequence.Count; i++)
                sequence[i].InPlaceUnreduce();
            return this;
        }

    }

    public sealed class AnyValueStorage<T> : ValueStorage<T>
    {
        public AnyValueStorage(T val)
            : base(ValueTypeEnum.Any, val)
        { }

        public override bool AsBoolean
        {
            get { return Convert.ToBoolean(Val); }
        }

        public override Date AsDate
        {
            get { return (Date)AsDateTime; }
        }

        public override DateTime AsDateTime
        {
            get { return Convert.ToDateTime(Val); }
        }

        public override long AsLong
        {
            get { return Convert.ToInt64(Val); }
        }

        public override Amount AsAmount
        {
            get { return Val as Amount; }
        }

        public override Balance AsBalance
        {
            get { return Val as Balance; }
        }

        public override Mask AsMask
        {
            get { return Val as Mask; }
        }

        public override IList<Value> AsSequence
        {
            get { return (Val as IList<Value>) ?? base.AsSequence; }
        }

        public override bool IsRealZero()
        {
            return Val != null;
        }

        public override string Dump(bool relaxed)
        {
            if (typeof(T) == typeof(ExprOp))
                return ((ExprOp)(object)Val).Dump();
            else
                return Val.ToString();
        }

        public override string Print(int firstWidth, int latterWidth, AmountPrintEnum flags)
        {
            if (typeof(T) == typeof(ExprOp))
            {
                string str = String.Empty;
                ((ExprOp)(object)Val).Print(ref str);
                return "<#EXPR " + str;
            }
            else
                return "<#OBJECT>";
        }
    }

    public sealed class ScopeValueStorage : ValueStorage<Scope>
    {
        public ScopeValueStorage(Scope val)
            : base(ValueTypeEnum.Scope, val)
        { }

        public override bool Bool
        {
            get { return AsScope != null; }
        }

        public override bool AsBoolean
        {
            get { return Val != null; }
        }

        public override Date AsDate
        {
            get { throw new ValueError(ValueError.CannotConvertScopeToDate); }
        }

        public override DateTime AsDateTime
        {
            get { throw new ValueError(ValueError.CannotConvertScopeToDateTime); }
        }

        public override long AsLong
        {
            get { throw new ValueError(ValueError.CannotConvertScopeToLong); }
        }

        public override Amount AsAmount
        {
            get { throw new ValueError(ValueError.CannotConvertScopeToAmount); }
        }

        public override Balance AsBalance
        {
            get { throw new ValueError(ValueError.CannotConvertScopeToBalance); }
        }

        public override Mask AsMask
        {
            get { throw new ValueError(ValueError.CannotConvertScopeToMask); }
        }

        public override Scope AsScope
        {
            get { return Val; }
        }

        public override string Dump(bool relaxed)
        {
            return AsScope.ToString();
        }

        public override string Print(int firstWidth, int latterWidth, AmountPrintEnum flags)
        {
            return "<#SCOPE>";
        }

    }

}
