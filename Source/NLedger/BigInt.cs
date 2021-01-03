// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Commodities;
using NLedger.Utility.BigValues;
using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger
{
    public struct BigInt<T> : IEquatable<BigInt<T>> where T : IBigValue<T>, new()
    {
        public static BigInt<T> Zero = FromInt(0);
        public static BigInt<T> One = FromInt(1);

        public static BigInt<T> operator /(BigInt<T> dividend, BigInt<T> divisor)
        {
            if (!divisor.HasValue || divisor == Zero)
                throw new DivideByZeroException("Division by zero");

            T value;
            dividend.Value.Divide(out value, ref divisor.Value);
            return new BigInt<T>(value, dividend.Precision + divisor.Precision, dividend.KeepPrecision);
        }

        public static BigInt<T> operator *(BigInt<T> left, BigInt<T> right)
        {
            T value;
            left.Value.Multiply(out value, ref right.Value);
            return new BigInt<T>(value, left.Precision + right.Precision, left.KeepPrecision);
        }

        public static BigInt<T> operator +(BigInt<T> left, BigInt<T> right)
        {
            T value;
            left.Value.Add(out value, ref right.Value);
            return new BigInt<T>(value, left.Precision, left.KeepPrecision);
        }

        public static BigInt<T> operator -(BigInt<T> left, BigInt<T> right)
        {
            T value;
            left.Value.Subtract(out value, ref right.Value);
            return new BigInt<T>(value, left.Precision, left.KeepPrecision);
        }

        public static bool operator ==(BigInt<T> left, BigInt<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BigInt<T> left, BigInt<T> right)
        {
            return !left.Equals(right);
        }

        public static BigInt<T> Parse(string s, int precision = 0, bool keepPrecision = false)
        {
            T value;
            Empty.Parse(out value, s, CultureInfo.InvariantCulture);
            return new BigInt<T>(value, precision, keepPrecision);
        }

        public static BigInt<T> FromInt(int value, int precision = 0)
        {
            T val;
            Empty.FromLong(out val, value);
            return new BigInt<T>(val, precision);
        }

        public static BigInt<T> FromLong(long value, int precision = 0)
        {
            T val;
            Empty.FromLong(out val, value);
            return new BigInt<T>(val, precision);
        }

        public static BigInt<T> FromDouble(double value, int precision = 0)
        {
            T val;
            Empty.FromDouble(out val, value);
            return new BigInt<T>(val, precision);
        }

        private BigInt(T value, int precision, bool keepPrecision = false) : this()
        {
            Value = value;
            Precision = precision;
            KeepPrecision = keepPrecision;
            HasValue = true; // DM - whatever is assigned here, it indicates that the value is not empty. The only opposite case is a default constructor.
        }

        //private BigInt(decimal value, int precision, bool keepPrecision = false) 
        //    : this(new BigValue(value), precision, keepPrecision)
        //{ }

        public int Precision { get; private set; }

        public bool KeepPrecision { get; private set; }

        public bool HasValue { get; private set; }

        public bool FitsInLong
        {
            get { return HasValue && Value.ConvertibleToLong(); }
        }

        public bool Valid()
        {
            if (Precision > 1024)
            {
                Logger.Current.Debug("ledger.validate", () => "amount_t::bigint_t: prec > 1024");
                return false;
            }

            return true;
        }

        public BigInt<T> Negative()
        {
            if (HasValue)
            {
                T value;
                Value.Negate(out value);
                return new BigInt<T>(value, Precision, KeepPrecision);
            }
            else
                return this;
        }

        public BigInt<T> Abs()
        {
            if (HasValue)
            {
                T value;
                Value.Abs(out value);
                return new BigInt<T>(value, Precision, KeepPrecision);
            }
            else
                return this;
        }

        public BigInt<T> Floor()
        {
            T value;
            Value.Floor(out value);
            return new BigInt<T>(value, Precision, KeepPrecision);
        }

        public BigInt<T> Ceiling()
        {
            T value;
            Value.Ceiling(out value);
            return new BigInt<T>(value, Precision, KeepPrecision);
        }

        public BigInt<T> RoundTo(int places)
        {
            T value;
            Value.Round(out value, places);
            return new BigInt<T>(value, Precision, KeepPrecision);
        }

        public int Sign
        {
            get { return Value.Sign(); }
        }

        public bool IsZeroInPrecision()
        {
            return IsZeroInPrecision(Precision);
        }

        public bool IsZeroInPrecision(int precision)
        {
            if (HasValue)
            {
                T value;
                Value.Round(out value, precision);
                return value.IsZero();
            }
            else
                return true;
        }

        public BigInt<T> SetPrecision(int precision)
        {
            return new BigInt<T>(Value, precision, KeepPrecision);
        }

        public BigInt<T> SetScale(int scale)
        {
            T value;
            Value.Scale(out value, scale);
            return new BigInt<T>(value, Precision, KeepPrecision);
        }

        public BigInt<T> SetKeepPrecision(bool keepPrecision)
        {
            return new BigInt<T>(Value, Precision, keepPrecision);
        }

        // stream_out_mpq
        public string Print(int precision, int zerosSpec = -1, Commodity comm = null)
        {
            if (Logger.Current.ShowDebug("amount.convert"))
            {
                var value = Value;
                var prec = Precision;
                Logger.Current.Debug("amount.convert", () => String.Format("Rational to convert = {0}", value.ToString("B", CultureInfo.CurrentCulture)));
                Logger.Current.Debug("amount.convert", () => String.Format("mpfr_print = {0} (precision {1}, zeros_prec {2})", value.ToString(), prec, zerosSpec));
            }

            string digitSeparator = "";
            if (comm != null && comm.Flags.HasFlag(CommodityFlagsEnum.COMMODITY_STYLE_THOUSANDS))
            {
                if ((comm.Symbol == "h" || comm.Symbol == "m") && (Commodity.Defaults.TimeColonByDefault || comm.Flags.HasFlag(CommodityFlagsEnum.COMMODITY_STYLE_TIME_COLON)))
                    digitSeparator = ":";
                else if (Commodity.Defaults.DecimalCommaByDefault || comm.Flags.HasFlag(CommodityFlagsEnum.COMMODITY_STYLE_DECIMAL_COMMA))
                    digitSeparator = ".";
                else
                    digitSeparator = ",";
            }

            string decimalMark = ".";
            if (comm != null)
            {
                if ((comm.Symbol == "h" || comm.Symbol == "m") && (Commodity.Defaults.TimeColonByDefault || comm.Flags.HasFlag(CommodityFlagsEnum.COMMODITY_STYLE_TIME_COLON)))
                    decimalMark = ":";
                else if (Commodity.Defaults.DecimalCommaByDefault || comm.Flags.HasFlag(CommodityFlagsEnum.COMMODITY_STYLE_DECIMAL_COMMA))
                    decimalMark = ",";
            }

            T val = Value;
            if (Precision > precision)
            {
                Value.Round(out val, precision);
            }

            NumberFormatInfo formatInfo = new NumberFormatInfo();
            formatInfo.NumberDecimalSeparator = decimalMark;
            formatInfo.NumberGroupSeparator = digitSeparator;

            return val.ToString(BuildNumericFormatString(digitSeparator, decimalMark, precision, zerosSpec), formatInfo);
        }

        public bool Equals(BigInt<T> other)
        {
            if (!HasValue && !other.HasValue)
                return true;
            if ((HasValue && !other.HasValue) || (!HasValue && other.HasValue))
                return false;
            return Value.Equals(ref other.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj is BigInt<T>)
            {
                return this.Equals((BigInt<T>)obj);
            }
            else
                return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public long ToLong()
        {
            // GMP_RNDN (see mpfr_get_si)
            if (HasValue)
            {
                T value;
                Value.Round(out value, 0);
                return value.ToLong();
            }
            else
            {
                return 0;
            }
        }

        public decimal ToDecimal()
        {
            return Value.ToDecimal();
        }

        public int Compare(BigInt<T> bigInt)
        {
            if (!HasValue)
                return bigInt.HasValue ? -1 : 0;

            if (!bigInt.HasValue)
                return 1;

            if (Value.Equals(ref bigInt.Value))
                return 0;
            else
                return Value.CompareTo(ref bigInt.Value) < 0 ? -1 : 1;
        }

        private T Value;
        private readonly static T Empty = new T();

        private string BuildNumericFormatString(string digitSeparator, string decimalMark, int precision, int zerosSpec)
        {
            StringBuilder sb = new StringBuilder(String.IsNullOrEmpty(digitSeparator) ? "0." : "#,##0.");
            while (precision > 0)
            {
                if (zerosSpec > 0)
                {
                    sb.Append("0");
                    zerosSpec--;
                }
                else
                {
                    sb.Append("#");
                }
                precision--;
            }
            return sb.ToString();
        }
    }
}
