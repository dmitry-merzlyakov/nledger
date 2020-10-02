// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLedger.Utility;
using NLedger.Amounts;
using NLedger.Annotate;
using NLedger.Commodities;
using NLedger.Utils;

namespace NLedger
{
    public class Balance : IEquatable<Balance>
    {
        public static Balance AverageLotPrices(Balance bal)
        {
            // First, we split the balance into multiple balances by underlying
            // commodity.
            IDictionary<string, Tuple<Amount, Annotation>> bycomm = new Dictionary<string, Tuple<Amount, Annotation>>();

            foreach (var pair in bal.Amounts)
            {
                string sym = pair.Key.Symbol;
                Amount quant = new Amount(pair.Value).StripAnnotations(new AnnotationKeepDetails());

                Tuple<Amount,Annotation> bycommItem;
                if (!bycomm.TryGetValue(sym, out bycommItem))
                {
                    bycommItem = new Tuple<Amount, Annotation>(quant, new Annotation());
                    bycomm.Add(sym, bycommItem);
                }
                else
                {
                    bycommItem.Item1.InPlaceAdd(quant);
                }

                if (pair.Key.IsAnnotated)
                {
                    AnnotatedCommodity acomm = (AnnotatedCommodity)pair.Key;
                    Annotation ann = bycommItem.Item2;

                    if ((bool)acomm.Details.Price)
                    {
                        if ((bool)ann.Price)
                            ann.Price = ann.Price + (acomm.Details.Price * quant);
                        else
                            ann.Price = acomm.Details.Price * quant;
                    }

                    if (acomm.Details.Date.HasValue)
                        if (!ann.Date.HasValue || acomm.Details.Date < ann.Date)
                            ann.Date = acomm.Details.Date;
                }
            }

            Balance result = new Balance();

            foreach(var pair in bycomm)
            {
                Amount amt = pair.Value.Item1;
                if (!amt.IsRealZero)
                {
                    if ((bool)pair.Value.Item2.Price)
                        pair.Value.Item2.Price = pair.Value.Item2.Price / amt;

                    Commodity acomm = CommodityPool.Current.FindOrCreate(amt.Commodity, pair.Value.Item2);
                    amt.SetCommodity(acomm);

                    result += amt;
                }
            }

            return result;
        }

        public static explicit operator bool(Balance balance)
        {
            return !IsNull(balance) && balance.IsNonZero;
        }

        public static explicit operator Balance(Amount amount)
        {
            return new Balance(amount);
        }

        public static explicit operator Balance(long val)
        {
            return new Balance(val);
        }

        public static explicit operator Balance(string val)
        {
            return new Balance(val);
        }

        public static Balance operator +(Balance val1, Balance val2)
        {
            return val1.Add(val2);
        }

        public static Balance operator +(Balance val1, Amount val2)
        {
            return val1.Add(val2);
        }

        public static Balance operator -(Balance val1, Balance val2)
        {
            return val1.Subtract(val2);
        }

        public static Balance operator -(Balance val1, Amount val2)
        {
            return val1.Subtract(val2);
        }

        public static Balance operator *(Balance val1, Amount val2)
        {
            return val1.Multiply(val2);
        }

        public static Balance operator /(Balance val1, Amount val2)
        {
            return val1.Divide(val2);
        }

        public static Balance operator -(Balance val)
        {
            return val.Negated();
        }

        public static bool operator ==(Balance left, Balance right)
        {
            if (Object.Equals(left, null))
                return Object.Equals(right, null);
            else
                return left.Equals(right);
        }

        public static bool operator !=(Balance left, Balance right)
        {
            if (Object.Equals(left, null))
                return !Object.Equals(right, null);
            else
                return !left.Equals(right);
        }

        public static bool operator ==(Balance left, Amount right)
        {
            if (Object.Equals(left, null))
                return Object.Equals(right, null);
            else
                return left.Equals(right);
        }

        public static bool operator !=(Balance left, Amount right)
        {
            if (Object.Equals(left, null))
                return !Object.Equals(right, null);
            else
                return !left.Equals(right);
        }

        public static bool IsNull(Balance balance)
        {
            return Object.Equals(balance, null);
        }

        public Balance()
        { 
            Amounts = new SortedDictionary<Commodity, Amount>(Commodity.DefaultComparer);
        }

        public Balance(Amount amount) : this()
        {
            Add(amount);
        }

        public Balance(Balance balance)
        {
            Amounts = new SortedDictionary<Commodity, Amount>(balance.Amounts.ToDictionary(kv => kv.Key, kv => new Amount(kv.Value)), Commodity.DefaultComparer);
        }

        public Balance(string val) : this()
        {
            Amount temp = new Amount(val);
            Amounts.Add(temp.Commodity, temp);
        }

        public Balance(double val) : this()
        {
            Amounts.Add(CommodityPool.Current.NullCommodity, (Amount)val);
        }

        public Balance(long val) : this()
        {
            Amounts.Add(CommodityPool.Current.NullCommodity, (Amount)val);
        }

        public bool IsEmpty
        {
            get { return !Amounts.Any(); }
        }

        public bool IsSingleAmount
        {
            get { return Amounts.Count == 1; }
        }

        /// <summary>
        /// Ported from is_nonzero()
        /// </summary>
        public bool IsNonZero
        {
            get
            {
                if (IsEmpty)
                    return false;

                return Amounts.Values.Any(amt => amt.IsNonZero);
            }
        }

        public bool IsZero
        {
            get { return IsEmpty || Amounts.Values.All(amount => amount.IsZero); }
        }

        public bool IsRealZero
        {
            get { return IsEmpty || Amounts.Values.All(amount => amount.IsRealZero); }
        }

        /// <summary>
        /// valid() returns true if the amounts within the balance are valid.
        /// </summary>
        public bool Valid()
        {
            foreach(var amount in Amounts.Values)
            {
                if (!amount.Valid())
                {
                    Logger.Current.Debug("ledger.validate", () => "balance_t: ! pair.second.valid()");
                    return false;
                }
            }
            return true;
        }

        public IDictionary<Commodity, Amount> Amounts { get; private set; }

        public Amount SingleAmount
        {
            get
            {
                if (IsSingleAmount)
                    return Amounts.Values.First();
                else
                    throw new InvalidOperationException("Multiple amounts");
            }
        }

        /// <summary>
        /// Ported from: amount_t to_amount()
        /// </summary>
        public Amount ToAmount()
        {
            if (IsEmpty)
                throw new BalanceError(BalanceError.ErrorMessageCannotConvertAnEmptyBalanceToAnAmount);
            else if (Amounts.Count == 1)
                return Amounts.Values.First();
            else
                throw new BalanceError(BalanceError.ErrorMessageCannotConvertABalanceWithMultipleCommoditiesToAnAmount);
        }

        /// <summary>
        /// Ported from: balance_t& balance_t::operator+=(const amount_t& amt)
        /// </summary>
        public Balance Add(Amount amount)
        {
            if (amount == null || !amount.Quantity.HasValue)
                throw new BalanceError(BalanceError.ErrorMessageCannotAddUninitializedAmountToBalance);

            if (amount.IsRealZero)
                return this;

            if (Amounts.ContainsKey(amount.Commodity))
                Amounts[amount.Commodity].InPlaceAdd(amount);
            else
                Amounts.Add(amount.Commodity, new Amount(amount));

            return this;
        }

        public Balance Subtract(Amount amount)
        {
            if (amount == null || !amount.Quantity.HasValue)
                throw new BalanceError(BalanceError.ErrorMessageCannotSubtractUninitializedAmountFromBalance);

            if (amount.IsRealZero)
                return this;

            if (Amounts.ContainsKey(amount.Commodity))
            {
                Amount existingAmount = Amounts[amount.Commodity];
                existingAmount.InPlaceSubtract(amount);
                if (existingAmount.IsRealZero)
                    Amounts.Remove(amount.Commodity);
            }
            else
                Amounts.Add(amount.Commodity, amount.Negated());

            return this;
        }

        public Balance Multiply(Amount amount)
        {
            if (amount == null || !amount.Quantity.HasValue)
                throw new BalanceError(BalanceError.ErrorMessageCannotMultiplyBalanceByUninitializedAmount);

            if (IsRealZero)
                return this;

            if (amount.IsRealZero)
                return new Balance(amount);

            if (!amount.HasCommodity)
            {
                // Multiplying by an amount with no commodity causes all the
                // component amounts to be increased by the same factor.
                foreach (Amount amt in Amounts.Values)
                    amt.Multiply(amount);
            }
            else if (IsSingleAmount)
            {
                // Multiplying by a commoditized amount is only valid if the sole
                // commodity in the balance is of the same kind as the amount's
                // commodity.
                if (SingleAmount.Commodity == amount.Commodity)
                    SingleAmount.Multiply(amount);
                else
                    throw new BalanceError(BalanceError.ErrorMessageCannotMultiplyBalanceWithAnnotatedCommoditiesByCommoditizedAmount);
            }
            else
            {
                // No sense in this code.
                //if (Amounts.Count > 1)
                //    throw new InvalidOperationException();
                throw new BalanceError(BalanceError.ErrorMessageCannotMultiplyMultiCommodityBalanceByCommoditizedAmount);
            }

            return this;
        }

        public Balance Divide(Amount amount)
        {
            if (amount == null || !amount.Quantity.HasValue)
                throw new BalanceError(BalanceError.ErrorMessageCannotDivideBalanceByUninitializedAmount);

            if (IsRealZero)
                return this;

            if (amount.IsRealZero)
                throw new BalanceError(BalanceError.ErrorMessageDivideByZero);

            if (!amount.HasCommodity)
            {
                // Dividing by an amount with no commodity causes all the
                // component amounts to be divided by the same factor.
                foreach (Amount amt in Amounts.Values)
                    amt.InPlaceDivide(amount);
            }
            else if (IsSingleAmount)
            {
                // Dividing by a commoditized amount is only valid if the sole
                // commodity in the balance is of the same kind as the amount's
                // commodity.
                if (SingleAmount.Commodity == amount.Commodity)
                    SingleAmount.InPlaceDivide(amount);
                else
                    throw new BalanceError(BalanceError.ErrorMessageCannotDivideBalanceWithAnnotatedCommoditiesByCommoditizedAmount);
            }
            else
            {
                // No sense in this code
                //if (Amounts.Count > 1)
                //    throw new InvalidOperationException();
                throw new BalanceError(BalanceError.ErrorMessageCannotDivideMultiCommodityBalanceByCommoditizedAmount);
            }

            return this;
        }

        public Balance Negated()
        {
            Balance temp = new Balance(this);
            temp.InPlaceNegate();
            return temp;
        }

        public void InPlaceNegate()
        {
            foreach (var keyValue in Amounts)
                keyValue.Value.InPlaceNegate();
        }

        public Balance Truncated()
        {
            Balance temp = new Balance(this);
            temp.InPlaceTruncate();
            return temp;
        }

        public void InPlaceTruncate()
        {
            foreach (Amount amount in Amounts.Values)
                amount.InPlaceTruncate();
        }

        public void InPlaceUnreduce()
        {
            // A temporary must be used here because unreduction may cause
            // multiple component amounts to collapse to the same commodity.
            Balance temp = new Balance();
            foreach (Amount amount in Amounts.Values)
                temp = temp.Add(amount.Unreduced());
            Amounts = temp.Amounts;
        }

        public void InPlaceUnround()
        {
            foreach (Amount amount in Amounts.Values)
                amount.InPlaceUnround();
        }

        public Balance Rounded()
        {
            Balance temp = new Balance(this);
            temp.InPlaceRound();
            return temp;
        }

        public void InPlaceRound()
        {
            foreach (Amount amount in Amounts.Values)
                amount.InPlaceRound();
        }

        public Balance Floored()
        {
            Balance temp = new Balance(this);
            temp.InPlaceFloor();
            return temp;
        }

        public void InPlaceFloor()
        {
            foreach (Amount amount in Amounts.Values)
                amount.InPlaceFloor();
        }

        public Balance Ceilinged()
        {
            Balance temp = new Balance(this);
            temp.InPlaceCeiling();
            return temp;
        }

        public void InPlaceCeiling()
        {
            foreach (Amount amount in Amounts.Values)
                amount.InPlaceCeiling();
        }

        public void InPlaceRoundTo(int places)
        {
            foreach (Amount amount in Amounts.Values)
                amount.InPlaceRoundTo(places);
        }

        public Balance Abs()
        {
            Balance temp = new Balance();
            foreach (Amount amount in Amounts.Values)
                temp = temp.Add(amount.Abs());
            return temp;
        }

        public Balance Number()
        {
            Balance temp = new Balance();
            foreach (Amount amount in Amounts.Values)
                temp = temp.Add(amount.Number());
            return temp;
        }

        public Balance Add(Balance balance)
        {
            foreach (var pair in balance.Amounts)
                Add(pair.Value);
            return this;
        }

        public Balance Subtract(Balance balance)
        {
            foreach (var pair in balance.Amounts)
                Subtract(pair.Value);
            return this;
        }

      /**
       * Annotated commodity methods.  The amounts contained by a balance
       * may use annotated commodities.  The `strip_annotations' method
       * will return a balance all of whose component amount have had
       * their commodity annotations likewise stripped.  See
       * amount_t::strip_annotations for more details.
       */
        public Balance StripAnnotations(AnnotationKeepDetails whatToKeep)
        {
            Balance balance = new Balance();
            foreach (Amount amount in Amounts.Values)
                balance = balance.Add(amount.StripAnnotations(whatToKeep));
            return balance;
        }

        public Balance Value (DateTime moment = default(DateTime), Commodity inTermsOf = null)
        {
            Balance temp = new Balance();
            bool resolved = false;
            foreach(KeyValuePair<Commodity,Amount> pair in Amounts)
            {
                Amount val = pair.Value.Value(moment, inTermsOf);
                if (val != null)
                {
                    temp = temp.Add(val);
                    resolved = true;
                }
                else
                {
                    temp = temp.Add(pair.Value);
                }
            }

            return resolved ? temp : null;
        }

        public Amount CommodityAmount(Commodity commodity = null)
        {
            if (commodity == null)
            {
                if (IsSingleAmount)
                    return SingleAmount;
                else if (!IsEmpty)
                {
                    // Try stripping annotations before giving an error.
                    Balance balance = StripAnnotations(new AnnotationKeepDetails());
                    if (balance.IsSingleAmount)
                        return balance.SingleAmount;

                    throw new AmountError(String.Format(AmountError.ErrorMessageRequestedAmountOfBalanceWithMultipleCommodities, balance));
                }

            }
            else if (!IsEmpty)
            {
                Amount amount = null;
                Amounts.TryGetValue(commodity, out amount);
                return amount;
            }
            return null;
        }

        /**
         * Given a balance, insert a commodity-wise sort of the amounts into the
         * given amounts_array.
         */
        /// <summary>
        /// Ported from void balance_t::sorted_amounts(amounts_array& sorted) const
        /// </summary>
        public List<Amount> SortedAmounts()
        {
            return Amounts.Values.
                // [DM] Enumerable.OrderBy is a stable sort that preserve original positions for equal items
                OrderBy(amt => amt, new BalanceCompareByCommodityComparison()).
                ToList();
        }

        /**
         * Iteration primitives.  `map_sorted_amounts' allows one to visit
         * each amount in balance in the proper order for displaying to the
         * user.  Mostly used by `print' and other routinse where the sort
         * order of the amounts' commodities is significant.
         */
        public void MapSortedAmounts(Action<Amount> fn)
        {
            if (Amounts.Any())
            {
                if (Amounts.Count == 1)
                {
                    Amount amount = Amounts.First().Value;
                    if ((bool)amount)
                        fn(amount);
                }
                else
                {
                    List<Amount> sorted = SortedAmounts();
                    sorted.ForEach(amt => fn(amt));
                }
            }
        }

        private class BalanceCompareByCommodityComparison : IComparer<Amount>
        {
            public int Compare(Amount x, Amount y)
            {
                return Commodity.CompareByCommodityComparison(x, y);
            }
        }

        public bool Equals(Amount amount)
        {
            if (amount.IsEmpty)
                throw new BalanceError(BalanceError.ErrorMessageCannotCompareBalanceToUninitializedAmount);

            if (amount.IsRealZero)
                return IsEmpty;

            return IsSingleAmount && SingleAmount.Equals(amount);
        }

        public bool Equals(Balance balance)
        {
            return !IsNull(balance) && Amounts.Compare(balance.Amounts);
        }

        public override bool Equals(object obj)
        {
            if (obj is Balance)
                return this.Equals((Balance)obj);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return Amounts.GetHashCode();
        }

        public bool IsLessThan(Amount amount)
        {
            if (amount == null)
                return false;

            return Amounts.Values.Any(am => am.IsLessThan(amount));
        }

        public bool IsGreaterThan(Amount amount)
        {
            if (amount == null)
                return false;

            return Amounts.Values.Any(am => am.IsGreaterThan(amount));
        }

        /**
         * Printing methods.  A balance may be output to a stream using the
         * `print' method.  There is also a global operator<< defined which
         * simply calls print for a balance on the given stream.  There is
         * one form of the print method, which takes two required arguments
         * and one arguments with a default value:
         *
         * print(ostream, int first_width, int latter_width) prints a
         * balance to the given output stream, using each commodity's
         * default display characteristics.  The first_width parameter
         * specifies the width that should be used for printing amounts
         * (since they are likely to vary in width).  The latter_width, if
         * specified, gives the width to be used for each line after the
         * first.  This is useful when printing in a column which falls at
         * the right-hand side of the screen.
         *
         * In addition to the width constraints, balances will also print
         * with commodities in alphabetized order, regardless of the
         * relative amounts of those commodities.  There is no option to
         * change this behavior.
         */
        public string Print(int firstWidth = -1, int latterWidth = -1, AmountPrintEnum flags = AmountPrintEnum.AMOUNT_PRINT_NO_FLAGS)
        {
            PrintAmountFromBalance amountPrinter = new PrintAmountFromBalance(true, firstWidth, latterWidth == 1 ? firstWidth : latterWidth, flags);
            MapSortedAmounts(amount => amountPrinter.HandleAmount(amount));

            if (amountPrinter.First)
                amountPrinter.Close();

            return amountPrinter.Str.ToString();
        }

        /// <summary>
        /// Ported from std::ostream& operator<<
        /// </summary>
        public override string ToString()
        {
            return Print(12);
        }

        /// <summary>
        /// Ported from operator string() / string to_string()
        /// </summary>
        /// <returns></returns>
        public string AsString()
        {
            return Print();
        }

        private class PrintAmountFromBalance
        {
            public PrintAmountFromBalance(bool first, int firstWidth, int latterWidth, AmountPrintEnum flags)
            {
                Str = new StringBuilder();

                First = first;
                FirstWidth = firstWidth;
                LatterWidth = latterWidth;
                Flags = flags;
            }

            public StringBuilder Str { get; private set; }

            public bool First { get; private set; }
            public int FirstWidth { get; private set; }
            public int LatterWidth { get; private set; }
            public AmountPrintEnum Flags { get; private set; }

            public void HandleAmount(Amount amount)
            {
                int width;
                if (!First)
                {
                    Str.AppendLine();
                    width = LatterWidth;
                }
                else
                {
                    First = false;
                    width = FirstWidth;
                }

                string s = amount.Print(Flags);
                Str.Append(UniString.Justify(s, width, Flags.HasFlag(AmountPrintEnum.AMOUNT_PRINT_RIGHT_JUSTIFY), Flags.HasFlag(AmountPrintEnum.AMOUNT_PRINT_COLORIZE) && amount.Sign < 0));
            }

            public void Close()
            {
                string format = String.Format("{{0,{0}{1}}}", Flags.HasFlag(AmountPrintEnum.AMOUNT_PRINT_RIGHT_JUSTIFY) ? "" : "-", FirstWidth);
                Str.AppendFormat(format, 0);
            }
        }
    }
}
