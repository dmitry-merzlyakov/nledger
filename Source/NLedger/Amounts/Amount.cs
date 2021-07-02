// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLedger.Utils;
using NLedger.Utility;
using NLedger.Annotate;
using NLedger.Commodities;
using NLedger.Utility.BigValues;

namespace NLedger.Amounts
{
    // This alias specifies the implementation of NLedger Quantity Arithmetic.
    // By default, it uses arbitrary precision arithmetic provided by BigRational.
    // If you need to use Decimal arithmetic for some reason, uncomment the next alias
    // and recompile the application.
    using BigInt = BigInt<BigRational>;
    //using BigInt = BigInt<BigDecimal>;

    /// <summary>
    /// Helper class that provides generalized factory methods for BigInt
    /// but hides implementation of NLedger Quantity Arithmetic
    /// </summary>
    /// <remarks>
    /// Located here (instead of BigInt.cs) because Quantity Arithmetic implementation is specified in this file.
    /// </remarks>
    public static class Quantity
    {
        public static readonly BigInt Empty = new BigInt();

        public static BigInt Parse(string s, int precision = 0, bool keepPrecsion = false)
        {
            return BigInt.Parse(s, precision, keepPrecsion);
        }

        public static BigInt FromLong(long value, int precision = 0)
        {
            return BigInt.FromLong(value, precision);
        }
    }

    /// <summary>
    /// Basic type for handling commoditized math: amount_t
    /// 
    /// An amount is the most basic numerical type in Ledger, and relies on
    /// commodity.h to represent commoditized amounts, which allows Ledger to
    /// handle mathematical expressions involving disparate commodities.
    /// 
    /// Amounts can be of virtually infinite size and precision.  When
    /// division or multiplication is performed, the precision is
    /// automatically expanded to include as many extra digits as necessary
    /// to avoid losing information.
    /// 
    /// Encapsulate infinite-precision commoditized amounts
    /// 
    /// Used to represent commoditized infinite-precision numbers, and
    /// uncommoditized, plain numbers.  In the commoditized case, commodities
    /// keep track of how they are used, and are always displayed back to the
    /// user after the same fashion.  For uncommoditized numbers, no display
    /// truncation is ever done.  In both cases, internal precision is always
    /// kept to an excessive degree.
    /// </summary>
    /// <remarks>
    /// Mathematical objects
    /// Ported from: amount_t (amount.h)
    /// </remarks>
    public class Amount : IEquatable<Amount>
    {
        /// <summary>
        /// Number of places of precision by which values are extended to
        /// avoid losing precision during division and multiplication.
        /// </summary>
        public static readonly int ExtendByDigits = 6;

        public static explicit operator Amount(long value)
        {
            return new Amount(value);
        }

        public static explicit operator Amount(double value)
        {
            return new Amount(value);
        }

        public static explicit operator Amount(string value)
        {
            return new Amount(value);
        }

        public static bool IsNullOrEmpty(Amount amount)
        {
            return amount == null || amount.IsEmpty;
        }

        public static Amount Exact(string value)
        {
            Amount temp = new Amount();
            temp.Parse(value, AmountParseFlagsEnum.PARSE_NO_MIGRATE);
            return temp;
        }

        public static bool operator ==(Amount left, Amount right)
        {
            if (Object.Equals(left, null))
                return Object.Equals(right, null);
            else
                return left.Equals(right);
        }

        public static bool operator !=(Amount left, Amount right)
        {
            if (Object.Equals(left, null))
                return !Object.Equals(right, null);
            else
                return !left.Equals(right);
        }

        public static explicit operator bool(Amount amount)
        {
            return amount != null && amount.IsNonZero;
        }

        public static Amount operator -(Amount amount)
        {
            return amount?.Negated();
        }

        public static bool operator >(Amount amount1, Amount amount2)
        {
            return amount1.IsGreaterThan(amount2);
        }

        public static bool operator <(Amount amount1, Amount amount2)
        {
            return amount1.IsLessThan(amount2);
        }

        public static bool operator >=(Amount amount1, Amount amount2)
        {
            return !amount1.IsLessThan(amount2);
        }

        public static bool operator <=(Amount amount1, Amount amount2)
        {
            return !amount1.IsGreaterThan(amount2);
        }

        /// <summary>
        /// Ported from parse_conversion
        /// </summary>
        public static void ParseConversion(string largerStr, string smallerStr)
        {
            Amount larger = new Amount();
            Amount smaller = new Amount();

            larger.Parse(ref largerStr, AmountParseFlagsEnum.PARSE_NO_REDUCE);
            smaller.Parse(ref smallerStr, AmountParseFlagsEnum.PARSE_NO_REDUCE);

            larger = larger.Multiply(smaller);

            if (larger.HasCommodity)
            {
                larger.Commodity.Smaller = smaller;
                larger.Commodity.Flags |= (smaller.Commodity.Flags | CommodityFlagsEnum.COMMODITY_NOMARKET);
            }

            if (smaller.HasCommodity)
                smaller.Commodity.Larger = larger;
        }

        /// <summary>
        /// Ready the amount subsystem for use.
        /// </summary>
        /// <remarks>
        /// Normally called by session_t::initialize().
        /// </remarks>
        public static void Initialize()
        {
            CommodityPool.Cleanup();

            // Add time commodity conversions, so that timelog's may be parsed
            // in terms of seconds, but reported as minutes or hours.
            Commodity commS = CommodityPool.Current.Create("s");
            if (commS != null)
                commS.Flags |= (CommodityFlagsEnum.COMMODITY_BUILTIN | CommodityFlagsEnum.COMMODITY_NOMARKET);

            // Add a "percentile" commodity
            Commodity commP = CommodityPool.Current.Create("%");
            if (commP != null)
                commP.Flags |= (CommodityFlagsEnum.COMMODITY_BUILTIN | CommodityFlagsEnum.COMMODITY_NOMARKET);
        }

        /// <summary>
        /// Shutdown the amount subsystem and free all resources.
        /// </summary>
        /// <remarks>
        /// Normally called by session_t::shutdown().
        /// </remarks>
        public static void Shutdown()
        {
            CommodityPool.Cleanup();
        }

        /// <summary>
        /// Creates a value for which is_null() is true, and which has no
        /// value or commodity.  If used in a value expression it evaluates to
        /// zero, and its commodity equals \c commodity_t::null_commodity.
        /// </summary>
        public Amount()
        {
            Commodity = CommodityPool.Current.NullCommodity;
        }

        public Amount(Amount amount) : this(amount.Quantity, amount.Commodity)
        {  }

        public Amount(BigInt quantity, Commodity commodity)
        {
            Quantity = quantity;
            Commodity = commodity ?? CommodityPool.Current.NullCommodity;
        }

        public Amount(long value, Commodity commodity)
        {
            Quantity = BigInt.FromLong(value);
            Commodity = commodity ?? CommodityPool.Current.NullCommodity;
        }

        /// <summary>
        /// Convert a long to an amount.  It's precision is zero, and the sign
        /// is preserved.
        /// </summary>
        public Amount(long value) : this(BigInt.FromLong(value), null)
        { }

        /// <summary>
        /// Convert a long to an amount.  It's precision is zero, and the sign is preserved.
        /// </summary>
        public Amount(ulong value) : this(BigInt.FromLong((long)value), null)
        { }

        /// <summary>
        /// Convert a double to an amount.  As much precision as possible is
        /// decoded from the binary floating point number.
        /// </summary>
        public Amount(double value) : this(BigInt.FromDouble(value), null)
        { }

        /// <summary>
        /// Parse a string as an (optionally commoditized) amount.  If no
        /// commodity is present, the resulting commodity is \c
        /// commodity_t::null_commodity.  The number may be of infinite
        /// precision.
        /// </summary>
        public Amount(string val)
        {
            Parse(ref val);
        }

        /// <summary>
        /// Copy an amount object, applying the given commodity annotation
        /// details afterward.  This is equivalent to doing a normal copy
        /// (@see amount_t(const amount_t&)) and then calling
        /// amount_t::annotate().
        /// </summary>
        public Amount(Amount amt, Annotation details)
            : this(amt)
        {
            Annotate(details);
        }

        public BigInt Quantity { get; private set; }

        /// <summary>
        /// commodity() returns an amount's commodity.  If the amount has no
        /// commodity, the value returned is the `null_commodity'.
        /// </summary>
        public Commodity Commodity { get; private set; }

        /// <summary>
        /// has_commodity() returns true if the amount has a commodity.
        /// </summary>
        public bool HasCommodity
        {
            get { return (bool)Commodity; }
        }

        public bool HasAnnotation
        {
            get
            {
                if (!Quantity.HasValue)
                    throw new AmountError(AmountError.ErrorMessageCannotDetermineIfUninitializedAmountsCommodityIsAnnotated);

                if (HasCommodity && Commodity.IsAnnotated && ((AnnotatedCommodity)Commodity).Details == null)
                    throw new InvalidOperationException("Wrong data structure");

                return HasCommodity && Commodity.IsAnnotated;
            }
        }

        public Annotation Annotation
        {
            get
            {
                if (!Quantity.HasValue)
                    throw new AmountError(AmountError.ErrorMessageCannotReturnCommodityAnnotationDetailsOfUninitializedAmount);

                if (!HasCommodity || !Commodity.IsAnnotated)
                    throw new AmountError(AmountError.ErrorMessageRequestForAnnotationDetailsFromUnannotatedAmount);

                AnnotatedCommodity annComm = (AnnotatedCommodity)Commodity;
                return annComm.Details;
            }
        }


        /// <summary>
        /// Synonym IsNull
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                if (!Quantity.HasValue)
                {
                    if (HasCommodity)
                        throw new InvalidOperationException("Amount should not have a commodity if quantity is empty");

                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Ported from bool is_nonzero() const
        /// </summary>
        public bool IsNonZero
        {
            get { return !IsZero; }
        }

        /// <summary>
        /// returns true if to_long() would not lose precision.
        /// </summary>
        public bool FitsInLong
        {
            get { return Quantity.HasValue && Quantity.FitsInLong; }
        }

        public bool KeepPrecision
        {
            get { return Quantity.KeepPrecision; }
        }

        /// <summary>
        /// Ported from void amount_t::set_keep_precision(const bool keep) const
        /// </summary>
        public void SetKeepPrecision(bool keep = true)
        {
            if (!Quantity.HasValue)
                throw new AmountError(AmountError.ErrorMessageCannotSetWhetherToKeepThePrecisionOfAnUninitializedAmount);

            Quantity = Quantity.SetKeepPrecision(keep);
        }

        /// <summary>
        /// An amount's commodity may be annotated with special details, such as the
        /// price it was purchased for, when it was acquired, or an arbitrary note,
        /// identifying perhaps the lot number of an item.
        /// </summary>
        /// <param name="details"></param>
        public void Annotate(Annotation details)
        {
            Commodity thisBase;
            AnnotatedCommodity thisAnnotated = null;

            if (!Quantity.HasValue)
                throw new AmountError(AmountError.ErrorMessageCannotAnnotateCommodityOfUninitializedAmount);
            else if (!HasCommodity)
                return; // ignore attempt to annotate a "bare" commodity

            if (Commodity.IsAnnotated)
            {
                thisAnnotated = (AnnotatedCommodity)Commodity;
                thisBase = thisAnnotated.Referent;
            }
            else
            {
                thisBase = Commodity;
            }

            if (thisBase == null)
                throw new InvalidOperationException("Cannot find the base commodity");

            Logger.Current.Debug("amount.commodities", () => String.Format("Annotating commodity for amount {0}\r\n{1}", this, details));

            Commodity annotatedCommodity = CommodityPool.Current.FindOrCreate(thisBase, details);
            if (annotatedCommodity != null)
                SetCommodity(annotatedCommodity);

            Logger.Current.Debug("amount.commodities", () => String.Format("Annotated amount is {0}", this));
        }

        /// <summary>
        /// The `flags' argument of both parsing may be one or more of the following:
        /// - PARSE_NO_MIGRATE means to not pay attention to the way an
        /// amount is used.  Ordinarily, if an amount were $100.001, for
        /// example, it would cause the default display precision for $ to be
        /// "widened" to three decimal places.  If PARSE_NO_MIGRATE is
        /// used, the commodity's default display precision is not changed.
        /// - PARSE_NO_REDUCE means not to call in_place_reduce() on the
        /// resulting amount after it is parsed.
        /// 
        /// These parsing methods observe the amounts they parse (unless
        /// PARSE_NO_MIGRATE is true), and set the display details of
        /// the corresponding commodity accordingly.  This way, amounts do
        /// not require commodities to be pre-defined in any way, but merely
        /// displays them back to the user in the same fashion as it saw them
        /// used.
        /// 
        /// There is also a static convenience method called
        /// `parse_conversion' which can be used to define a relationship
        /// between scaling commodity values.  For example, Ledger uses it to
        /// define the relationships among various time values:
        /// <code>
        /// amount_t::parse_conversion("1.0m", "60s"); // a minute is 60 seconds
        /// amount_t::parse_conversion("1.0h", "60m"); // an hour is 60 minutes
        /// </code>
        /// The method parse() is used to parse an amount from an input stream
        /// or a string.  A global operator>>() is also defined which simply
        /// calls parse on the input stream.  The parse() method has two forms:
        /// - parse(istream, flags_t) parses an amount from the given input stream.
        /// - parse(string, flags_t) parses an amount from the given string.
        /// - parse(string, flags_t) also parses an amount from a string.
        /// </summary>
        /// <remarks>
        /// The possible syntax for an amount is:
        /// [-]NUM[ ]SYM [@ AMOUNT]
        /// SYM[ ][-]NUM [@ AMOUNT]
        /// </remarks>
        /// <param name="line"></param>
        /// <param name="parseFlags"></param>
        /// <returns></returns>
        public bool Parse(ref string line, AmountParseFlagsEnum parseFlags = AmountParseFlagsEnum.PARSE_DEFAULT)
        {
            string quant = null;
            string symbol = null;
            bool negative = false;
            CommodityFlagsEnum commodityFlags = CommodityFlagsEnum.COMMODITY_STYLE_DEFAULTS;
            Annotation details = null; 

            line = line.TrimStart();
            if (line.StartsWith("-"))
            {
                negative = true;
                line = line.Remove(0, 1).TrimStart();
            }

            if (line.StartsWithDigit())
            {
                quant = ParseQuantity(ref line);
                if (!String.IsNullOrEmpty(line))
                {
                    if (line.StartsWithWhiteSpace())
                        commodityFlags = CommodityFlagsEnum.COMMODITY_STYLE_SEPARATED;

                    symbol = Commodity.ParseSymbol(ref line);
                    if (!String.IsNullOrEmpty(symbol))
                        commodityFlags = commodityFlags | CommodityFlagsEnum.COMMODITY_STYLE_SUFFIXED;

                    if (!parseFlags.HasFlag(AmountParseFlagsEnum.PARSE_NO_ANNOT) && !string.IsNullOrWhiteSpace(line))
                    {
                        details = new Annotation();
                        details.Parse(ref line);
                    }
                }
            }
            else
            {
                symbol = Commodity.ParseSymbol(ref line);
                if (line.StartsWithWhiteSpace())
                    commodityFlags = CommodityFlagsEnum.COMMODITY_STYLE_SEPARATED;

                quant = ParseQuantity(ref line);

                if ((!parseFlags.HasFlag(AmountParseFlagsEnum.PARSE_NO_ANNOT)) && !String.IsNullOrEmpty(quant) && !string.IsNullOrWhiteSpace(line))
                {
                    details = new Annotation();
                    details.Parse(ref line);
                }                
            }

            if (String.IsNullOrEmpty(quant))
            {
                if (parseFlags.HasFlag(AmountParseFlagsEnum.PARSE_SOFT_FAIL))
                    return false;
                else
                    throw new AmountError(AmountError.ErrorMessageNoQuantitySpecifiedForAmount);
            }

            // Create the commodity if has not already been seen, and update the
            // precision if something greater was used for the quantity.

            if (String.IsNullOrEmpty(symbol))
                ClearCommodity();
            else
            {
                Commodity = CommodityPool.Current.Find(symbol);
                if (Commodity == null)
                    Commodity = CommodityPool.Current.Create(symbol);
            }

            // Quickly scan through and verify the correctness of the amount's use of punctuation.

            int decimalOffset = 0;
            int lastComma = -1;
            int lastPeriod = -1;

            bool noMoreCommas = false;
            bool noMorePeriods = false;
            bool noMigrateStyle = Commodity.Flags.HasFlag(CommodityFlagsEnum.COMMODITY_STYLE_NO_MIGRATE);
            bool decimalCommaStyle = Commodity.Defaults.DecimalCommaByDefault || 
                (Commodity != null && Commodity.Flags.HasFlag(CommodityFlagsEnum.COMMODITY_STYLE_DECIMAL_COMMA));

            int newQuantityPrecision = 0;

            for (int stringIndex = quant.Length - 1; stringIndex >= 0; stringIndex--)
            {
                char ch = quant[stringIndex];

                if (ch == '.')
                {
                    if (noMorePeriods)
                        throw new AmountError(AmountError.ErrorMessageTooManyPeriodsInAmount);

                    if (decimalCommaStyle)
                    {
                        if (decimalOffset % 3 != 0)
                            throw new AmountError(AmountError.ErrorMessageIncorrectUseOfThousandMarkPeriod);
                        commodityFlags |= CommodityFlagsEnum.COMMODITY_STYLE_THOUSANDS;
                        noMoreCommas = true;
                    }
                    else
                    {
                        if (lastComma != -1)
                        {
                            decimalCommaStyle = true;
                            if (decimalOffset % 3 != 0)
                                throw new AmountError(AmountError.ErrorMessageIncorrectUseOfThousandMarkPeriod);
                        }
                        else
                        {
                            noMorePeriods = true;
                            newQuantityPrecision = decimalOffset;
                            decimalOffset = 0;
                        }
                    }

                    if (lastPeriod == -1)
                        lastPeriod = stringIndex;
                }
                else if (ch == ',')
                {
                    if (noMoreCommas)
                        throw new AmountError(AmountError.ErrorMessageTooManyCommasInAmount);

                    if (decimalCommaStyle)
                    {
                        if (lastPeriod != -1)
                            throw new AmountError(AmountError.ErrorMessageIncorrectUseOfDecimalComma);
                        noMoreCommas = true;
                        newQuantityPrecision = decimalOffset;
                        decimalOffset = 0;
                    }
                    else
                    {
                        if (decimalOffset % 3 != 0)
                        {
                            if (lastComma != -1 || lastPeriod != -1)
                                throw new AmountError(AmountError.ErrorMessageIncorrectUseOfThousandMarkComma);
                            decimalCommaStyle = true;
                            noMoreCommas = true;
                            newQuantityPrecision = decimalOffset;
                            decimalOffset = 0;
                        }
                        else
                        {
                            commodityFlags |= CommodityFlagsEnum.COMMODITY_STYLE_THOUSANDS;
                            noMorePeriods = true;
                        }
                    }

                    if (lastComma == -1)
                        lastComma = stringIndex;
                }
                else
                {
                    decimalOffset++;
                }
            }

            if (decimalCommaStyle)
                commodityFlags |= CommodityFlagsEnum.COMMODITY_STYLE_DECIMAL_COMMA;

            bool keepPrecision = false;

            if (parseFlags.HasFlag(AmountParseFlagsEnum.PARSE_NO_MIGRATE))
            {
                keepPrecision = true;
            }
            else
            {
                if (HasCommodity && !noMigrateStyle)
                {
                    Commodity.Flags |= commodityFlags;

                    if (newQuantityPrecision > Commodity.Precision)
                        Commodity.Precision = newQuantityPrecision;
                }
            }

            // Now we have the final number.  Remove commas and periods, if necessary.

            if (lastComma != -1 || lastPeriod != -1)
            {
                Quantity = BigInt.Parse(quant.Replace(".", "").Replace(",", ""), newQuantityPrecision, keepPrecision);
                Quantity = Quantity.SetScale(newQuantityPrecision);

                if (Logger.Current.ShowDebug("amount.parse"))
                    Logger.Current.Debug("amount.parse", () => String.Format("Rational parsed = {0}", Quantity));
            }
            else
            {
                Quantity = BigInt.Parse(quant, newQuantityPrecision, keepPrecision);
            }

            if (negative)
                Quantity = Quantity.Negative();

            if (!parseFlags.HasFlag(AmountParseFlagsEnum.PARSE_NO_REDUCE))
                InPlaceReduce();        // will not throw an exception

            if (HasCommodity && !Annotation.IsNullOrEmpty(details))
            {
                if (details.IsPriceNotPerUnit)
                {
                    if (details.Price == null)
                        throw new InvalidOperationException("Details do not have a price");
                    details.Price /= Abs();
                }
                Commodity = CommodityPool.Current.FindOrCreate(Commodity, details);
            }

            Validator.Verify(() => Valid());

            return true;
        }

        public bool Parse(string line, AmountParseFlagsEnum parseFlags = AmountParseFlagsEnum.PARSE_DEFAULT)
        {
            return Parse(ref line, parseFlags);
        }

        public bool Parse(InputTextStream inStream, AmountParseFlagsEnum parseFlags = AmountParseFlagsEnum.PARSE_DEFAULT)
        {
            using(InputTextStreamWrapper wrapper = new InputTextStreamWrapper(inStream))
                return Parse(ref wrapper.Source, parseFlags);
        }

        public static string ParseQuantity(ref string line)
        {
            if (String.IsNullOrEmpty(line))
                return String.Empty;

            line = line.TrimStart();
            string s = StringExtensions.ReadInto(ref line, QuantityChars);

            int lastDigit = s.LastIndexOfAny(CharExtensions.DigitChars);
            if (lastDigit < s.Length - 1)
            {
                line = s.Substring(lastDigit + 1) + line;
                s = s.Substring(0, lastDigit + 1);
            }

            return s;
        }

        /// <summary>
        /// Ported from inverted()
        /// </summary>
        public Amount Inverted()
        {
            Amount temp = new Amount(this);
            temp.InPlaceInvert();
            return temp;
        }

        /// <summary>
        /// Ported from amount_t::in_place_invert()
        /// </summary>
        public void InPlaceInvert()
        {
            if (!Quantity.HasValue)
                throw new AmountError(AmountError.ErrorMessageCannotInvertUninitializedAmount);

            if (Sign != 0)
                Quantity = BigInt.One / Quantity;
        }

        public BigInt GetInvertedQuantity()
        {
            if (Quantity.HasValue && Sign != 0)
                return BigInt.One / Quantity;
            else
                return BigInt.Zero;
        }

        public Amount Merge (Amount amount)
        {
            if (amount == null)
                throw new ArgumentNullException("amount");

            if (Commodity == amount.Commodity)
                throw new InvalidOperationException("The same commodity");

            return new Amount(Quantity * amount.Quantity, amount.Commodity);
        }

        public static Amount operator/(Amount divisible, Amount divisor)
        {
            return new Amount(divisible).InPlaceDivide(divisor);
        }

        /// <summary>
        /// Divide two amounts while extending the precision to preserve the
        /// accuracy of the result.  For example, if \c 10 is divided by \c 3,
        /// the result ends up having a precision of \link
        /// amount_t::extend_by_digits \endlink place to avoid losing internal
        /// resolution.
        /// Ported from amount_t& operator/=(const amount_t& amt);
        /// </summary>
        public Amount InPlaceDivide(Amount amount)
        {
            if (amount == null)
                throw new ArgumentNullException("amount");

            Validator.Verify(() => Valid());

            if (!Quantity.HasValue || !amount.Quantity.HasValue)
            {
                if (Quantity.HasValue)
                    throw new AmountError(AmountError.ErrorMessageCannotDivideAnAmountByAnUninitializedAmount);
                else if (amount.Quantity.HasValue)
                    throw new AmountError(AmountError.ErrorMessageCannotDivideAnUninitializedAmountByAnAnAmount);
                else
                    throw new AmountError(AmountError.ErrorMessageCannotDivideTwoUninitializedAmounts);
            }

            if (!(bool)amount)
                throw new AmountError(AmountError.ErrorMessageDivideByZero);

            // Increase the value's precision, to capture fractional parts after
            // the divide.  Round up in the last position.
            BigInt quantity = Quantity / amount.Quantity;
            quantity = quantity.SetPrecision(Quantity.Precision + amount.Quantity.Precision + ExtendByDigits);

            if (!HasCommodity && amount.HasCommodity)
                Commodity = amount.Commodity;

            // If this amount has a commodity, and we're not dealing with plain
            // numbers, or internal numbers (which keep full precision at all
            // times), then round the number to within the commodity's precision
            // plus six places.
            if (HasCommodity && !KeepPrecision)
            {
                int commodityPrecision = Commodity.Precision + ExtendByDigits;
                if (quantity.Precision > commodityPrecision)
                    quantity = quantity.SetPrecision(commodityPrecision);
            }

            Quantity = quantity;
            return this;
        }

        public static Amount operator *(Amount multiplierA, Amount multiplierB)
        {
            return new Amount(multiplierA).Multiply(multiplierB);
        }

        public Amount Multiply(Amount amount, bool ignoreCommodity = false)
        {
            if (amount == null)
                throw new ArgumentException("amount");

            Validator.Verify(() => Valid());

            if (!Quantity.HasValue || !amount.Quantity.HasValue)
            {
                if (Quantity.HasValue)
                    throw new AmountError(AmountError.ErrorMessageCannotMultiplyAnAmountByAnUninitializedAmount);
                else if (amount.Quantity.HasValue)
                    throw new AmountError(AmountError.ErrorMessageCannotMultiplyAnUninitializedAmountByAnAnAmount);
                else
                    throw new AmountError(AmountError.ErrorMessageCannotMultiplyTwoUninitializedAmounts);
            }

            BigInt quantity = Quantity * amount.Quantity;
            quantity = quantity.SetPrecision(Quantity.Precision + amount.Quantity.Precision);

            if (!HasCommodity && amount.HasCommodity)
                Commodity = amount.Commodity;

            if (HasCommodity && !KeepPrecision)
            {
                int commodityPrecision = Commodity.Precision + ExtendByDigits;
                if (quantity.Precision > commodityPrecision)
                    quantity = quantity.SetPrecision(commodityPrecision);
            }

            Quantity = quantity;
            return this;
        }

        public static Amount operator +(Amount amountA, Amount amountB)
        {
            return new Amount(amountA).InPlaceAdd(amountB);
        }

        /// <summary>
        /// Ported from amount_t& amount_t::operator+=(const amount_t& amt)
        /// </summary>
        public Amount InPlaceAdd(Amount amount)
        {
            if (amount == null)
                throw new ArgumentException("amount");

            Validator.Verify(() => amount.Valid());

            if (!Quantity.HasValue || !amount.Quantity.HasValue)
            {
                if (Quantity.HasValue)
                    throw new AmountError(AmountError.ErrorMessageCannotAddUninitializedAmountToAmount);
                else if (amount.Quantity.HasValue)
                    throw new AmountError(AmountError.ErrorMessageCannotAddAmountToUninitializedAmount);
                else
                    throw new AmountError(AmountError.ErrorMessageCannotAddTwoUninitializedAmounts);
            }

            if (HasCommodity && amount.HasCommodity && Commodity != amount.Commodity)
                throw new AmountError(String.Format(AmountError.ErrorMessageSubtractingAmountsWithDifferentCommodities, Commodity, amount.Commodity));

            BigInt quantity = Quantity + amount.Quantity;

            if (HasCommodity == amount.HasCommodity && quantity.Precision < amount.Quantity.Precision)
                quantity.SetPrecision(amount.Quantity.Precision);

            Quantity = quantity;
            return this;
        }

        public static Amount operator -(Amount amountA, Amount amountB)
        {
            return new Amount(amountA).InPlaceSubtract(amountB);
        }

        /// <summary>
        /// Ported from amount_t& amount_t::operator-=(const amount_t& amt)
        /// </summary>
        public Amount InPlaceSubtract(Amount amount)
        {
            if (amount == null)
                throw new ArgumentException("amount");

            Validator.Verify(() => amount.Valid());

            if (!Quantity.HasValue || !amount.Quantity.HasValue)
            {
                if (Quantity.HasValue)
                    throw new AmountError(AmountError.ErrorMessageCannotSubtractUninitializedAmountFromAmount);
                else if (amount.Quantity.HasValue)
                    throw new AmountError(AmountError.ErrorMessageCannotSubtractAmountFromUninitializedAmount);
                else
                    throw new AmountError(AmountError.ErrorMessageCannotSubtractTwoUninitializedAmounts);
            }

            if (HasCommodity && amount.HasCommodity && Commodity != amount.Commodity)
                throw new AmountError(String.Format(AmountError.ErrorMessageSubtractingAmountsWithDifferentCommodities, Commodity, amount.Commodity));

            BigInt quantity = Quantity - amount.Quantity;

            if (HasCommodity == amount.HasCommodity && quantity.Precision < amount.Quantity.Precision)
                quantity = quantity.SetPrecision(amount.Quantity.Precision);

            Quantity = quantity;
            return this;
        }

        /// <summary>
        /// Returns the absolute value of an amount.  Equivalent to:
        /// </summary>
        /// <example>
        /// (x < * 0) ? - x : x
        /// </example>
        /// <returns></returns>
        public Amount Abs()
        {
            if (Sign < 0)
                return Negated();

            return this;
        }

        /// <summary>
        /// IsZero returns true if an amount's display value is zero.
        /// Thus, $0.0001 is considered zero if the current display precision
        /// for dollars is two decimal places.
        /// </summary>
        public bool IsZero
        {
            get
            {
                if (!Quantity.HasValue)
                    throw new AmountError(AmountError.ErrorMessageCannotDetermineIfUninitializedAmountIsZero);

                if (HasCommodity)
                {
                    if (KeepPrecision || Quantity.Precision < Commodity.Precision)
                        return IsRealZero;
                    else if (IsRealZero)
                        return true;
                    else
                        return Quantity.IsZeroInPrecision(Commodity.Precision);
                }
                return IsRealZero;
            }
        }

        /// <summary>
        /// IsRealZero returns true if an amount's actual value is zero.
        /// Thus, $0.0001 is never considered realzero.
        /// </summary>
        public bool IsRealZero
        {
            get { return Sign == 0; }
        }

        /// <summary>
        /// sign() returns an integer less than, greater than, or equal to
        /// zero depending on whether the amount is negative, zero, or
        /// greater than zero.  Note that this function tests the actual
        /// value of the amount -- using its internal precision -- and not
        /// the display value.  To test its display value, use:
        /// `round().sign()'.
        /// </summary>
        public int Sign
        {
            get
            {
                if (!Quantity.HasValue)
                    throw new AmountError(AmountError.ErrorMessageCannotDetermineSignOfAnUninitializedAmount);

                return Quantity.Sign;
            }
        }

        public bool Valid()
        {
            if (Quantity.HasValue)
            {
                if (!Quantity.Valid())
                {
                    Logger.Current.Debug("ledger.validate", () => "amount_t: ! quantity->valid()");
                    return false;
                }
            }
            else if (HasCommodity)
            {
                Logger.Current.Debug("ledger.validate", () => "amount_t: commodity_ != NULL");
                return false;
            }
            return true;
        }

        /// <summary>
        /// set_commodity(commodity_t) sets an amount's commodity to the
        /// given value.  Note that this merely sets the current amount to
        /// that commodity, it does not "observe" the amount for possible
        /// changes in the maximum display precision of the commodity, the
        /// way that `parse' does.
        /// </summary>
        public void SetCommodity(Commodity commodity)
        {
            if (!Quantity.HasValue)
                Quantity = BigInt.Zero;
            Commodity = commodity;
        }

        public bool Equals(Amount other)
        {
            return other != null && Quantity == other.Quantity && Commodity == other.Commodity;
        }

        public override bool Equals(object obj)
        {
            if (obj is Amount)
                return this.Equals((Amount)obj);
            else
                return false;
        }

        public bool IsLessThan(Amount amount)
        {
            return Compare(amount) < 0;
        }

        public bool IsGreaterThan(Amount amount)
        {
            return Compare(amount) > 0;
        }

        public int Compare(Amount amount)
        {
            if (amount == null)
                throw new ArgumentException("amount");

            Validator.Verify(() => amount.Valid());

            if (!Quantity.HasValue || !amount.Quantity.HasValue)
            {
                if (Quantity.HasValue)
                    throw new AmountError(AmountError.ErrorMessageCannotCompareAmountToUninitializedAmount);
                else if (amount.Quantity.HasValue)
                    throw new AmountError(AmountError.ErrorMessageCannotCompareUninitializedAmountToAmount);
                else
                    throw new AmountError(AmountError.ErrorMessageCannotCompareTwoUninitializedAmounts);
            }

            if (HasCommodity && amount.HasCommodity && Commodity != amount.Commodity)
                throw new AmountError(String.Format(AmountError.ErrorMessageCannotCompareAmountsWithDifferentCommodities, Commodity, amount.Commodity));

            return Quantity.Compare(amount.Quantity);
        }

        public override int GetHashCode()
        {
            int hash1 = Quantity.GetHashCode();
            int hash2 = Commodity != null ? Commodity.GetHashCode() : 0;
            return (((hash1 << 5) + hash1) ^ hash2);
        }

        /// <summary>
        /// number() returns a commodity-less version of an amount.  This is
        /// useful for accessing just the numeric portion of an amount.
        /// </summary>
        public Amount Number()
        {
            if (!HasCommodity)
                return this;

            Amount temp = new Amount(this);
            temp.ClearCommodity();
            return temp;
        }

        /// <summary>
        /// Yields an amount whose display precision when output is truncated
        /// to the display precision of its commodity.  This is normally the
        /// default state of an amount, but if one has become unrounded, this
        /// sets the "keep precision" state back to false.
        /// @see set_keep_precision
        /// </summary>
        public Amount Rounded()
        {
            Amount temp = new Amount(this);
            temp.InPlaceRound();
            return temp;
        }

        /// <summary>
        /// Yields an amount whose display precision is never truncated, even
        /// though its commodity normally displays only rounded values.
        /// </summary>
        public Amount Unrounded()
        {
            Amount temp = new Amount(this);
            temp.InPlaceUnround();
            return temp;
        }

        public void InPlaceUnround()
        {
            if (!Quantity.HasValue)
                throw new AmountError(AmountError.ErrorMessageCannotUnroundUninitializedAmounts);
            else if (KeepPrecision)
                return;

            Logger.Current.Debug("amount.unround", () => String.Format("Unrounding {0}", this));
            SetKeepPrecision(true);
            Logger.Current.Debug("amount.unround", () => String.Format("Unrounded = {0}", this));
        }

        /// <summary>
        /// unreduce(), if used with a "scaling commodity", yields the most
        /// compact form greater than one.  That is, \c 3599s will unreduce to
        /// \c 59.98m, while \c 3601 unreduces to \c 1h.
        /// </summary>
        public Amount Unreduced()
        {
            Amount temp = new Amount(this);
            temp.InPlaceUnreduce();
            return temp;
        }

        /// <summary>
        /// Ported from void amount_t::in_place_unreduce()
        /// </summary>
        public void InPlaceUnreduce()
        {
            if (!Quantity.HasValue)
                throw new AmountError(AmountError.ErrorMessageCannotUnreduceAnUninitializedAmount);

            Amount tmp = this;
            Commodity comm = Commodity;
            bool shifted = false;

            while ((bool)comm && comm.Larger != null)
            {
                Amount nextTemp = tmp / comm.Larger.Number();
                if (nextTemp.Abs().IsLessThan((Amount)1))
                    break;
                tmp = nextTemp;
                comm = comm.Larger.Commodity;
                shifted = true;
            }

            if (shifted)
            {
                if (("h" == comm.Symbol || "m" == comm.Symbol) && Commodity.Defaults.TimeColonByDefault)
                {
                    Amount floored = tmp.Floored();
                    Amount precision = tmp - floored;
                    if (precision.IsLessThan((Amount)0))
                    {
                        precision.InPlaceAdd((Amount)1);
                        floored.InPlaceSubtract((Amount)1);
                    }
                    tmp = floored + (precision * (comm.Smaller.Number() / new Amount(100)));
                }

                // this = tmp;
                Quantity = tmp.Quantity;
                Commodity = tmp.Commodity;

                Commodity = comm;
            }
        }

        /// <summary>
        /// Yields an amount which has lost all of its extra precision, beyond what
        /// the display precision of the commodity would have printed.
        /// </summary>
        public Amount Truncated()
        {
            Amount temp = new Amount(this);
            temp.InPlaceTruncate();
            return temp;
        }

        public void InPlaceTruncate()
        {
            if (!Quantity.HasValue)
                throw new AmountError(AmountError.ErrorMessageCannotTruncateAnUninitializedAmount);

            Logger.Current.Debug("amount.truncate", () => String.Format("Truncating {0} to precision {1}", this, DisplayPrecision));
            Quantity = Quantity.RoundTo(DisplayPrecision);
            Logger.Current.Debug("amount.truncate", () => String.Format("Truncated = {0}", this));
        }

        /// <summary>
        /// Yields an amount which has lost all of its extra precision, beyond what
        /// the display precision of the commodity would have printed.
        /// </summary>
        public Amount Ceilinged()
        {
            Amount temp = new Amount(this);
            temp.InPlaceCeiling();
            return temp;
        }

        public void InPlaceCeiling()
        {
            if (!Quantity.HasValue)
                throw new AmountError(AmountError.ErrorMessageCannotComputeCeilingOnAnUninitializedAmount);

            Quantity = Quantity.Ceiling();
        }

        public Amount RoundTo(int places)
        {
            Amount temp = new Amount(this);
            temp.InPlaceRoundTo(places);
            return temp;
        }

        public void InPlaceRoundTo(int places)
        {
            if (!Quantity.HasValue)
                throw new AmountError(AmountError.ErrorMessageCannotRoundAnUninitializedAmount);

            Quantity = Quantity.RoundTo(places);
        }

        /// <summary>
        /// Yields an amount which has lost all of its extra precision, beyond what
        /// the display precision of the commodity would have printed.
        /// </summary>
        public Amount Floored()
        {
            Amount temp = new Amount(this);
            temp.InPlaceFloor();
            return temp;
        }

        public void InPlaceFloor()
        {
            if (!Quantity.HasValue)
                throw new AmountError(AmountError.ErrorMessageCannotComputeFloorOnAnUninitializedAmount);

            Quantity = Quantity.Floor();
        }

        /// <summary>
        /// reduces a value to its most basic commodity form, for amounts that
        /// utilize "scaling commodities".  For example, an amount of \c 1h
        /// after reduction will be \c 3600s.
        /// </summary>
        public Amount Reduced()
        {
            Amount temp = new Amount(this);
            temp.InPlaceReduce();
            return temp;
        }

        public void InPlaceReduce()
        {
            if (!Quantity.HasValue)
                throw new AmountError(AmountError.ErrorMessageCannotReduceUninitializedAmounts);

            while (Commodity != null && Commodity.Smaller != null)
            {
                Quantity = Quantity * Commodity.Smaller.Quantity;
                Commodity = Commodity.Smaller.Commodity;
            }
        }

        public void InPlaceNegate()
        {
            if (!Quantity.HasValue)
                throw new AmountError(AmountError.ErrorMessageCannotNegateUninitializedAmounts);

            Quantity = Quantity.Negative();
        }

        public void InPlaceRound()
        {
            if (!Quantity.HasValue)
                throw new AmountError(AmountError.ErrorMessageCannotSetRoundingForAnUninitializedAmounts);
            else if (!KeepPrecision)
                return;

            //_dup;
            SetKeepPrecision(false);
        }

        /// <summary>
        /// strip_annotations() returns an amount whose commodity's annotations have
        /// been stripped.
        /// </summary>
        /// <remarks>
        /// If the lot price is considered whenever working with commoditized values.
        /// Let's say a user adds two values of the following form: 10 AAPL + 10 AAPL {$20}
        /// This expression adds ten shares of Apple stock with another ten
        /// shares that were purchased for \c $20 a share.  If \c keep_price
        /// is false, the result of this expression is an amount equal to
        /// <tt>20 AAPL</tt>.  If \c keep_price is \c true the expression
        /// yields an exception for adding amounts with different commodities.
        /// In that case, a \link balance_t \endlink object must be used to
        /// store the combined sum.
        /// </remarks>
        public Amount StripAnnotations(AnnotationKeepDetails whatToKeep)
        {
            if (!Quantity.HasValue)
                throw new AmountError(AmountError.ErrorMessageCannotStripCommodityAnnotationsFromUninitializedAmount);

            if (!whatToKeep.KeepAll(Commodity))
            {
                Amount amount = new Amount(Quantity, Commodity);
                amount.SetCommodity(Commodity.StripAnnotations(whatToKeep));
                return amount;
            }

            return this;
        }

        /// <summary>
        /// Returns the negated value of an amount.
        /// @see operator-()
        /// </summary>
        public Amount Negated()
        {
            Amount temp = new Amount(this);
            temp.InPlaceNegate();
            return temp;
        }

        public Amount WithCommodity(Commodity comm)
        {
            if (Commodity == comm)
                return this;

            Amount tmp = new Amount(this);
            tmp.SetCommodity(comm);
            return tmp;
        }

        /// <summary>
        /// clear_commodity() sets an amount's commodity to null, such that
        /// has_commodity() afterwards returns false.
        /// </summary>
        public void ClearCommodity()
        {
            Commodity = CommodityPool.Current.NullCommodity;
        }

        public int Precision
        {
            get
            {
                if (!Quantity.HasValue)
                    throw new AmountError(AmountError.ErrorMessageCannotDeterminePrecisionOfAnUninitializedAmount);

                return Quantity.Precision;
            }
        }

        public int DisplayPrecision
        {
            get
            {
                if (!Quantity.HasValue)
                    throw new AmountError(AmountError.ErrorMessageCannotDetermineDisplayPrecisionOfAnUninitializedAmount);

                if (HasCommodity && !KeepPrecision)
                    return Commodity.Precision;
                else
                    return HasCommodity ? Math.Max(Quantity.Precision, Commodity.Precision) : Quantity.Precision;
            }
        }

        /// <summary>
        /// Returns the historical value for an amount -- the default moment
        /// returns the most recently known price -- based on the price history
        /// for the given commodity (or determined automatically, if none is
        /// provided).  For example, if the amount were <tt>10 AAPL</tt>, and
        /// on Apr 10, 2000 each share of \c AAPL was worth \c $10, then
        /// calling value() for that moment in time would yield the amount \c
        /// $100.00.
        /// </summary>
        public Amount Value(DateTime moment = default(DateTime), Commodity inTermsOf = null)
        {
            if (Quantity.HasValue)
            {
                Logger.Current.Debug("commodity.price.find", () => String.Format("amount_t::value of {0}", Commodity.Symbol));
                if (!moment.IsNotADateTime())
                    Logger.Current.Debug("commodity.price.find", () => String.Format("amount_t::value: moment = {0}", moment));
                if (inTermsOf != null)
                    Logger.Current.Debug("commodity.price.find", () => String.Format("amount_t::value: in_terms_of = {0}", inTermsOf.Symbol));

                if (HasCommodity && (inTermsOf != null || !Commodity.Flags.HasFlag(CommodityFlagsEnum.COMMODITY_PRIMARY)))
                {
                    PricePoint? point = null;
                    Commodity comm = inTermsOf;

                    if (HasAnnotation && Annotation.Price != null)
                    {
                        if (Annotation.IsPriceFixated)
                        {
                            point = new PricePoint(default(DateTime), Annotation.Price);
                            Logger.Current.Debug("commodity.prices.find", () => String.Format("amount_t::value: fixated price =  {0}", point.Value.Price));
                        }
                        else if (comm == null)
                            comm = Annotation.Price.Commodity;
                    }

                    if (comm != null && Commodity.Referent == comm.Referent)
                        return WithCommodity (comm.Referent);

                    if (!point.HasValue)
                    {
                        point = Commodity.FindPrice(comm, moment);

                        // Whether a price was found or not, check whether we should attempt
                        // to download a price from the Internet.  This is done if (a) no
                        // price was found, or (b) the price is "stale" according to the
                        // setting of --price-exp.
                        if (point.HasValue)
                            point = Commodity.CheckForUpdatedPrice(point, moment, comm);
                    }

                    if (point.HasValue)
                    {
                        Amount price = new Amount(point.Value.Price);
                        price = price.Multiply(this, true);
                        price.InPlaceRound();
                        return price;
                    }
                }
            }
            else
            {
                throw new AmountError(AmountError.ErrorMessageCannotDetermineValueOfAnUninitializedAmount);
            }
            return null;
        }

        /// <summary>
        /// Ported from optional<amount_t> amount_t::price() const
        /// </summary>
        public Amount Price
        {
            get
            {
                if (HasAnnotation && Annotation.Price != null)
                {
                    Amount tmp = new Amount(Annotation.Price);
                    tmp = tmp.Multiply(this);
                    Logger.Current.Debug("amount.price", () => String.Format("Returning price of {0} = {1}", this, tmp));
                    return tmp;
                }
                return null;
            }
        }

        /// <summary>
        /// An amount may be output to a stream using the `print' method.  There is
        /// also a global operator &lt;&lt; defined which simply calls print for an amount
        /// on the given stream.  There is one form of the print method, which takes
        /// one required argument and two arguments with default values:
        /// 
        /// print(ostream, bool omit_commodity = false, bool full_precision = false)
        /// prints an amounts to the given output stream, using its commodity's
        /// default display characteristics.  If `omit_commodity' is true, the
        /// commodity will not be displayed, only the amount (although the
        /// commodity's display precision is still used).  If `full_precision' is
        /// true, the full internal precision of the amount is displayed, regardless
        /// of its commodity's display precision.
        /// </summary>
        public string Print(AmountPrintEnum flags = AmountPrintEnum.AMOUNT_PRINT_NO_FLAGS)
        {
            Validator.Verify(() => Valid());

            if (!Quantity.HasValue)
                return "<null>";

            StringBuilder sb = new StringBuilder();

            if (!Commodity.Flags.HasFlag(CommodityFlagsEnum.COMMODITY_STYLE_SUFFIXED))
            {
                sb.Append(Commodity.Print(flags.HasFlag(AmountPrintEnum.AMOUNT_PRINT_ELIDE_COMMODITY_QUOTES)));
                if (Commodity.Flags.HasFlag(CommodityFlagsEnum.COMMODITY_STYLE_SEPARATED))
                    sb.Append(" ");
            }

            sb.Append(Quantity.Print(DisplayPrecision, Commodity != null ? Commodity.Precision : 0, Commodity));

            if (Commodity.Flags.HasFlag(CommodityFlagsEnum.COMMODITY_STYLE_SUFFIXED))
            {
                if (Commodity.Flags.HasFlag(CommodityFlagsEnum.COMMODITY_STYLE_SEPARATED))
                    sb.Append(" ");
                sb.Append(Commodity.Print(flags.HasFlag(AmountPrintEnum.AMOUNT_PRINT_ELIDE_COMMODITY_QUOTES)));
            }
              
            // If there are any annotations associated with this commodity, output them
            // now.
            sb.Append(Commodity.WriteAnnotations(flags.HasFlag(AmountPrintEnum.AMOUNT_PRINT_NO_COMPUTED_ANNOTATIONS)));
              
            // Things are output to a string first, so that if anyone has specified a
            // width or fill for _out, it will be applied to the entire amount string,
            // and not just the first part.
            return sb.ToString();
        }

        /// <summary>
        /// to_string() returns an amount'ss "display value" as a string --
        /// after rounding the value according to the commodity's default
        /// precision.  It is equivalent to: `round().to_fullstring()'.
        /// </summary>
        public override string ToString()
        {
            return Print();
        }

        /// <summary>
        ///  Ported from inline string amount_t::to_fullstring() const {
        /// </summary>
        /// <returns></returns>
        public string ToFullString()
        {
            return Unrounded().Print();
        }

        /// <summary>
        /// to_long([bool]) returns an amount as a long integer.  If the
        /// optional boolean argument is true (the default), an exception is
        /// thrown if the conversion would lose information.
        /// </summary>
        public long ToLong()
        {
            if (!Quantity.HasValue)
                throw new AmountError("Cannot convert an uninitialized amount to a long");

            return Quantity.ToLong();
        }

        /// <summary>
        /// to_double([bool]) returns an amount as a double.  If the optional
        /// boolean argument is true (the default), an exception is thrown if
        /// the conversion would lose information.
        /// </summary>
        public double ToDouble()
        {
            if (!Quantity.HasValue)
                throw new AmountError("Cannot convert an uninitialized amount to a double");

            return (double)Quantity.ToDecimal();
        }

        /// <summary>
        /// quantity_string() returns an amount's "display value", but
        /// without any commodity.  Note that this is different from
        /// `number().to_string()', because in that case the commodity has
        /// been stripped and the full, internal precision of the amount
        /// would be displayed.
        /// </summary>
        public string QuantityString()
        {
            return Number().Print();
        }

        private static Func<char, bool> QuantityChars = (c) => Char.IsDigit(c) || c == '-' || c == ',' || c == '.';
    }
}
