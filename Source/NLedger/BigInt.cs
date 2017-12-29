// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Commodities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger
{
    public struct BigInt : IEquatable<BigInt>
    {
        public static BigInt Zero = new BigInt(0, 0);
        public static BigInt One = new BigInt(1, 0);

        public static BigInt operator /(BigInt dividend, BigInt divisor)
        {
            if (!divisor.HasValue || divisor == Zero)
                throw new DivideByZeroException("Division by zero");

            return new BigInt((dividend.Value ?? 0) / divisor.Value, dividend.Precision + divisor.Precision, dividend.KeepPrecision);
        }

        public static BigInt operator *(BigInt left, BigInt right)
        {
            return new BigInt((left.Value ?? 0) * (right.Value ?? 0), left.Precision + right.Precision, left.KeepPrecision);
        }

        public static BigInt operator +(BigInt left, BigInt right)
        {
            return new BigInt((left.Value ?? 0) + (right.Value ?? 0), left.Precision, left.KeepPrecision);
        }

        public static BigInt operator -(BigInt left, BigInt right)
        {
            return new BigInt((left.Value ?? 0) - (right.Value ?? 0), left.Precision, left.KeepPrecision);
        }

        public static bool operator ==(BigInt left, BigInt right)
        {
            return left.Value == right.Value;
        }

        public static bool operator !=(BigInt left, BigInt right)
        {
            return left.Value != right.Value;
        }

        public static BigInt Parse(string s, int precision = 0, bool keepPrecision = false)
        {
            decimal value = decimal.Parse(s, CultureInfo.InvariantCulture);
            return new BigInt(value, precision, keepPrecision);
        }

        public static BigInt FromInt(int value, int precision = 0)
        {
            return new BigInt(value, precision);
        }

        public static BigInt FromLong(long value, int precision = 0)
        {
            return new BigInt(value, precision);
        }

        public static BigInt FromDouble(double value, int precision = 0)
        {
            return new BigInt((decimal)value, precision);
        }

        private BigInt(decimal? value, int precision, bool keepPrecision = false) : this()
        {
            Value = value;
            Precision = precision;
            KeepPrecision = keepPrecision;
        }

        public int Precision { get; private set; }

        public bool KeepPrecision { get; private set; }

        public bool HasValue
        {
            get { return Value.HasValue; }
        }

        public bool FitsInLong
        {
            get { return HasValue && Value.Value < long.MaxValue && Value.Value > long.MinValue; }
        }

        public bool Valid()
        {
            if (Precision > 1024)
                return false;

            return true;
        }

        public BigInt Negative()
        {
            if (HasValue)
                return new BigInt(-Value, Precision, KeepPrecision);
            else
                return this;
        }

        public BigInt Abs()
        {
            if (HasValue)
                return new BigInt(Math.Abs(Value.Value), Precision, KeepPrecision);
            else
                return this;
        }

        public BigInt Floor()
        {
            return new BigInt(Math.Floor(Value.Value), Precision, KeepPrecision);
        }

        public BigInt Ceiling()
        {
            return new BigInt(Math.Ceiling(Value.Value), Precision, KeepPrecision);
        }

        public BigInt RoundTo(int places)
        {
            return new BigInt(Math.Round(Value.Value, places), Precision, KeepPrecision);
        }

        public int Sign
        {
            get { return HasValue && Value != 0 ? (Value.Value > 0 ? 1 : -1) : 0; }
        }

        public bool IsZeroInPrecision()
        {
            return IsZeroInPrecision(Precision);
        }

        public bool IsZeroInPrecision(int precision)
        {
            if (HasValue)
                return Math.Round(Value.Value, precision) == 0;
            else
                return true;
        }

        public BigInt SetPrecision(int precision)
        {
            return new BigInt(Value, precision, KeepPrecision);
        }

        public BigInt SetScale(int scale)
        {
            long pow = (long)Math.Pow(10, scale);
            return new BigInt((Value ?? 0) / pow, Precision, KeepPrecision);
        }

        public BigInt SetKeepPrecision(bool keepPrecision)
        {
            return new BigInt(Value, Precision, keepPrecision);
        }

        // stream_out_mpq
        public string Print(int precision, int zerosSpec = -1, Commodity comm = null)
        {
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

            decimal val = Value ?? default(decimal);
            if (Precision > precision)
                val = Math.Round(Value.Value, precision);

            NumberFormatInfo formatInfo = new NumberFormatInfo();
            formatInfo.NumberDecimalSeparator = decimalMark;
            formatInfo.NumberGroupSeparator = digitSeparator;

            return val.ToString(BuildNumericFormatString(digitSeparator, decimalMark, precision, zerosSpec), formatInfo);
        }

        public bool Equals(BigInt other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is BigInt)
                return this.Equals((BigInt)obj);
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
            // TODO - GMP_RNDN (see mpfr_get_si)
            return Value.HasValue ? (long)(Math.Round(Value.Value)) : 0;
        }

        public decimal ToDecimal()
        {
            return Value ?? 0;
        }

        public int Compare(BigInt bigInt)
        {
            if (!HasValue)
                return bigInt.HasValue ? -1 : 0;

            if (!bigInt.HasValue)
                return 1;

            if (Value.Value == bigInt.Value.Value)
                return 0;
            else
                return Value.Value < bigInt.Value.Value ? -1 : 1;
        }

        private decimal? Value { get; set; }

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
