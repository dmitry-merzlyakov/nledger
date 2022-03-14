// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility.BigValues
{
    /// <summary>
    /// Presents arbitrary precision arithmetic based on rational fractions of BigInteger values. 
    /// Implements IBigValue; NLedger uses it as a primary way to deal with quantities.
    /// </summary>
    public struct BigRational : IBigValue<BigRational>
    {
        public static readonly BigRational Zero = new BigRational(BigInteger.Zero);

        public static BigRational Create(decimal value)
        {
            BigRational bigRational;
            Zero.FromDecimal(out bigRational, value);
            return bigRational;
        }

        public static BigRational Create(string value)
        {
            BigRational bigRational;
            Zero.Parse(out bigRational, value, CultureInfo.CurrentCulture);
            return bigRational;
        }

        public BigRational(BigInteger numerator, BigInteger denominator)
        {
            if (denominator.Sign == 0)
                throw new DivideByZeroException();

            if (numerator.Sign == 0)
            {
                numerator = BigInteger.Zero;
                denominator = BigInteger.One;
            }
            else if (denominator.Sign < 0)
            {
                numerator = BigInteger.Negate(numerator);
                denominator = BigInteger.Negate(denominator);
            }

            Simplify(ref numerator, ref denominator);

            Numerator = numerator;
            Denominator = denominator;
        }

        public BigRational(BigInteger numerator) : this (numerator, BigInteger.One)
        { }

        public void Abs(out BigRational result)
        {
            result = (Numerator.Sign < 0 ? new BigRational(BigInteger.Abs(Numerator), Denominator) : this);
        }

        public void Add(out BigRational result, ref BigRational addend)
        {
            result = new BigRational((Numerator * addend.Denominator) + (Denominator * addend.Numerator), (Denominator * addend.Denominator));
        }

        public void Ceiling(out BigRational result)
        {
            var fractSign = GetFractionPart().Sign();

            if (fractSign == 0)
            {
                result = this;
                return;
            }

            if (fractSign > 0)
                result = new BigRational(GetWholePart() + 1);
            else
                result = new BigRational(GetWholePart());
        }

        public int CompareTo(ref BigRational value)
        {
            return BigInteger.Compare(Numerator * value.Denominator, value.Numerator * Denominator);
        }

        public bool ConvertibleToLong()
        {
            var fractional = GetFractionPart();
            if (!Zero.Equals(ref fractional))
                return false;

            if (LongMinValue.CompareTo(ref this) > 0)
                return false;

            if (LongMaxValue.CompareTo(ref this) < 0)
                return false;

            return true;
        }

        public void Divide(out BigRational result, ref BigRational divisor)
        {
            result = new BigRational((Numerator * divisor.Denominator), (Denominator * divisor.Numerator));
        }

        public bool Equals(ref BigRational value)
        {
            if (this.Denominator == value.Denominator)
            {
                return Numerator == value.Numerator;
            }
            else
            {
                return (Numerator * value.Denominator) == (Denominator * value.Numerator);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is BigRational))
                return false;

            BigRational value = (BigRational)obj;
            return this.Equals(ref value);
        }

        public override int GetHashCode()
        {
            return (Numerator / Denominator).GetHashCode();
        }

        public void Floor(out BigRational result)
        {
            var fractSign = GetFractionPart().Sign();

            if (fractSign == 0)
            {
                result = this;
                return;
            }

            if (fractSign > 0)
                result = new BigRational(GetWholePart());
            else
                result = new BigRational(GetWholePart() - 1);
        }

        public void FromDouble(out BigRational result, double value)
        {
            FromDecimal(out result, (decimal)value);
        }

        public void FromDecimal(out BigRational result, decimal value)
        {
            if (value == 0)
                result = Zero;

            int decimalPlaces = BitConverter.GetBytes(decimal.GetBits(value)[3])[2];
            if (decimalPlaces == 0)
            {
                result = new BigRational((BigInteger)value);
                return;
            }

            var pow = (decimal)Math.Pow(10, decimalPlaces);
            result = new BigRational(((BigInteger)(value * pow)));

            var divisor = new BigRational((BigInteger)pow);
            result.Divide(out result, ref divisor);
        }

        public void FromLong(out BigRational result, long value)
        {
            result = new BigRational(value);
        }

        public bool IsZero()
        {
            return Zero.Equals(ref this);
        }

        public void Multiply(out BigRational result, ref BigRational multiplier)
        {
            result = new BigRational((Numerator * multiplier.Numerator), (Denominator * multiplier.Denominator));
        }

        public void Negate(out BigRational result)
        {
            result = new BigRational(BigInteger.Negate(Numerator), Denominator);
        }

        public void Parse(out BigRational result, string s, IFormatProvider provider)
        {
            NumberFormatInfo numberFormatProvider = (NumberFormatInfo)provider?.GetFormat(typeof(NumberFormatInfo));
            if (numberFormatProvider == null)
                numberFormatProvider = CultureInfo.CurrentCulture.NumberFormat;

            int pos = s.IndexOf(numberFormatProvider.NumberDecimalSeparator);
            if (pos < 0)
            {
                var value = BigInteger.Parse(s, provider);
                result = new BigRational(value);
            }
            else
            {
                var strWholePart = s.Substring(0, pos).TrimStart();
                var strFractPart = pos < s.Length ? s.Substring(pos + 1) : string.Empty;

                BigInteger bigWholePart = BigInteger.Zero;
                if (!string.IsNullOrEmpty(strWholePart) && strWholePart != "-")
                    bigWholePart = BigInteger.Parse(strWholePart, provider);

                BigInteger bigFractPart = BigInteger.Zero;
                if (!string.IsNullOrEmpty(strFractPart))
                {
                    var pow = new BigRational((long)Math.Pow(10, strFractPart.Length));
                    bigFractPart = BigInteger.Parse(strFractPart, provider);

                    if (bigWholePart.Sign < 0 || (bigWholePart.IsZero && strWholePart.StartsWith(numberFormatProvider.NegativeSign)))
                        bigFractPart = BigInteger.Negate(bigFractPart);

                    var rationalWhole = new BigRational(bigWholePart);
                    var rationalFract = new BigRational(bigFractPart);

                    rationalFract.Divide(out rationalFract, ref pow);
                    rationalWhole.Add(out result, ref rationalFract);
                }
                else
                {
                    result = new BigRational(bigWholePart);
                }
            }
        }

        public void Round(out BigRational result, int decimals)
        {
            if (decimals < 0)
                throw new ArgumentOutOfRangeException("BigRational can only round to 0 or any positive number of digits of precision.");

            var fract = GetFractionPart();
            if (Zero.Equals(ref fract))
            {
                result = this;
                return;
            }

            var factor = BigInteger.Pow(10, decimals);
            var multiplier = fract.Numerator * factor;

            var reminder = BigInteger.Remainder(multiplier, fract.Denominator);
            if (reminder.IsZero)
            {
                result = this;
                return;
            }

            var newFract = BigInteger.Divide(multiplier, fract.Denominator);
            var cmp = BigInteger.Abs(fract.Denominator).CompareTo(BigInteger.Abs(reminder) * 2);
            if (cmp < 0)
            {
                newFract = newFract + reminder.Sign;
            }
            else if (cmp == 0)
            {
                // It is a halfway between two integers, one of which is even and the other odd. The even number is returned.
                if (!newFract.IsEven)
                {
                    newFract = newFract + reminder.Sign;
                }
            }

            var wholePart = new BigRational(GetWholePart());
            var fractPart = new BigRational(newFract, factor);
            wholePart.Add(out result, ref fractPart);
        }

        public void Scale(out BigRational result, int scale)
        {
            var pow = BigInteger.Pow(BigIntegerTen, scale);
            var divisor = new BigRational(pow);
            this.Divide(out result, ref divisor);
        }

        public int Sign()
        {
            return Numerator.Sign;
        }

        public void Subtract(out BigRational result, ref BigRational addend)
        {
            result = new BigRational((Numerator * addend.Denominator) - (Denominator * addend.Numerator), (Denominator * addend.Denominator));
        }

        public decimal ToDecimal()
        {
            if (SafeCastToDecimal(Numerator) && SafeCastToDecimal(Denominator))
            {
                return (decimal)Numerator / (decimal)Denominator;
            }

            BigInteger denormalized = (Numerator * DecimalPrecision) / Denominator;
            if (denormalized.IsZero)
            {
                return Decimal.Zero;
            }

            for (int scale = DecimalMaxScale; scale >= 0; scale--)
            {
                if (!SafeCastToDecimal(denormalized))
                {
                    denormalized = denormalized / 10;
                }
                else
                {
                    return ((decimal)denormalized) / scale;
                }
            }

            throw new OverflowException();
        }

        public long ToLong()
        {
            return (long)ToDecimal();
        }

        public override string ToString()
        {
            return ToString("R", CultureInfo.CurrentCulture.NumberFormat);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (String.IsNullOrEmpty(format))
                format = "R";

            NumberFormatInfo numberFormatProvider = (NumberFormatInfo)formatProvider?.GetFormat(typeof(NumberFormatInfo));
            if (numberFormatProvider == null)
                numberFormatProvider = CultureInfo.CurrentCulture.NumberFormat;

            if (format == "B")
            {
                StringBuilder ret = new StringBuilder();
                ret.Append(Numerator.ToString("R", CultureInfo.InvariantCulture));
                ret.Append('/');
                ret.Append(Denominator.ToString("R", CultureInfo.InvariantCulture));
                return ret.ToString();
            }

            // Get integral and fractional parts of format; detect precision
            int pos = format.IndexOf('.');
            string integralFormat = pos >= 0 ? format.Substring(0, pos) : format;
            string fractionalFormat = pos >= 0 && format.Length > pos + 1 ? format.Substring(pos + 1) : String.Empty;
            int prec = pos >= 0 ? format.Length - pos - 1 : 0;

            // Round value and get integral and fractional parts of this valud
            BigRational value;
            Round(out value, prec);
            var integral = value.GetWholePart();
            //var fractional = value.GetFractionPart();

            // Print integral and fractional parts
            var sIntegral = integral.IsZero ? (Sign() < 0 ? numberFormatProvider.NegativeSign : String.Empty) + "0"
                : integral.ToString(integralFormat, formatProvider);
            var sFractional = PrintFractionalPart(ref value, prec);

            // Format output
            StringBuilder sb = new StringBuilder(sIntegral);
            if (String.IsNullOrEmpty(fractionalFormat))
            {
                if (integralFormat == "R" && !String.IsNullOrEmpty(sFractional))
                {
                    sb.Append(numberFormatProvider.NumberDecimalSeparator);
                    sb.Append(sFractional);
                }
            }
            else
            {
                bool isSeparatorAdded = false;
                for (int i = 0; i < fractionalFormat.Length; i++)
                {
                    char ch = fractionalFormat[i];
                    if (ch == '0')
                    {
                        if (!isSeparatorAdded)
                        {
                            sb.Append(numberFormatProvider.NumberDecimalSeparator);
                            isSeparatorAdded = true;
                        }
                        sb.Append(i < sFractional.Length ? sFractional[i] : '0');
                    }
                    else if (ch == '#')
                    {
                        if (i < sFractional.Length)
                        {
                            if (!isSeparatorAdded)
                            {
                                sb.Append(numberFormatProvider.NumberDecimalSeparator);
                                isSeparatorAdded = true;
                            }
                            sb.Append(sFractional[i]);
                        }
                    }
                }
            }

            return sb.ToString();
        }

        private BigInteger GetWholePart()
        {
            return BigInteger.Divide(Numerator, Denominator);
        }

        private BigRational GetFractionPart()
        {
            return new BigRational(BigInteger.Remainder(Numerator, Denominator), Denominator);
        }

        private static void Simplify(ref BigInteger numerator, ref BigInteger denominator)
        {
            if (numerator == BigInteger.Zero)
            {
                denominator = BigInteger.One;
            }

            BigInteger gcd = BigInteger.GreatestCommonDivisor(numerator, denominator);
            if (gcd > BigInteger.One)
            {
                numerator = numerator / gcd;
                denominator = denominator / gcd;
            }
        }

        private static string PrintFractionalPart(ref BigRational value, int maxExp = 1024)
        {
            var fract = value.GetFractionPart();

            BigInteger reminder = BigInteger.Remainder(fract.Numerator, fract.Denominator);
            if (reminder.IsZero) return String.Empty;

            var exp = 1;
            while (true)
            {
                var pow = BigInteger.Pow(10, exp);
                var factor = fract.Numerator * pow;
                reminder = BigInteger.Remainder(factor, fract.Denominator);

                if (reminder.IsZero || exp >= maxExp)
                {
                    var stage1 = factor / fract.Denominator;
                    var stage2 = BigInteger.Abs(stage1).ToString().PadLeft(exp, '0');
                    return stage2.TrimEnd('0');
                }

                exp++;
            }
        }

        private static bool SafeCastToDecimal(BigInteger value)
        {
            return DecimalMinValue <= value && value <= DecimalMaxValue;
        }

        private static readonly BigInteger BigIntegerTen = new BigInteger(10);
        private const int DecimalMaxScale = 28;
        private static readonly BigInteger DecimalPrecision = BigInteger.Pow(10, DecimalMaxScale);
        private static readonly BigInteger DecimalMaxValue = (BigInteger)Decimal.MaxValue;
        private static readonly BigInteger DecimalMinValue = (BigInteger)Decimal.MinValue;
        private static readonly BigRational LongMaxValue = new BigRational(long.MaxValue);
        private static readonly BigRational LongMinValue = new BigRational(long.MinValue);

        private BigInteger Numerator;
        private BigInteger Denominator;
    }
}
