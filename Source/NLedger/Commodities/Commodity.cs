// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using NLedger.Annotate;
using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Times;
using NLedger.Utility;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Commodities
{
    /// <summary>
    /// Ported from commodity_t
    /// </summary>
    public class Commodity : IComparable<Commodity>, IEquatable<Commodity>, IComparable
    {
        #region DefaultCommodityComparer
        /* DM: migrated test "cmd-org.test" revealed an issue with class balance_t.
         * In partucular, its member "amounts" (typedef std::map<commodity_t *, amount_t> amounts_map)
         * does not specify any comparison rules (there is no third generic type in the template), 
         * so std::map uses a default comparer (std:less<K>). It compares commodity instances by means of operator "<" - __x < __y.
         * Since commodity_to does not override this operator, the system compares _object_allocation_addresses_ in memory.
         * That is, a commodity that is allocated later is always upper any commodities allocated before (because its address in memory 
         * numerically higher).
         * This comparer mimics the same behavior (at least, in order to pass all tests in the same way).
        */
        private static long GlobalCommodityAllocationCounter = 0;
        private long CommodityAllocationNumber = ++GlobalCommodityAllocationCounter;
        public static readonly DefaultCommodityComparer DefaultComparer = new DefaultCommodityComparer();
        public class DefaultCommodityComparer : IComparer<Commodity>
        {
            public int Compare(Commodity x, Commodity y)
            {
                return Math.Sign(x.CommodityAllocationNumber - y.CommodityAllocationNumber);
            }
        }
        #endregion

        #region Commodity Defaults
        public class CommodityDefaults
        {
            public bool DecimalCommaByDefault { get; set; }
            public bool TimeColonByDefault { get; set; }
        }

        public static CommodityDefaults Defaults
        {
            get { return _Defaults ?? (_Defaults = new CommodityDefaults()); }
        }

        public static void Initialize()
        {
            _Defaults = null;
        }

        public static void Shutdown()
        {
            _Defaults = null;
        }

        private static CommodityDefaults _Defaults;
        #endregion

        public static bool CompareByCommodity(Amount left, Amount right)
        {
            if (left == null)
                throw new ArgumentNullException("left");
            if (right == null)
                throw new ArgumentNullException("right");

            Commodity leftComm = left.Commodity;
            Commodity rightComm = right.Commodity;

            if (leftComm.BaseSymbol != rightComm.BaseSymbol)
                return String.Compare(leftComm.BaseSymbol, rightComm.BaseSymbol, StringComparison.Ordinal) < 0;

            if (!leftComm.IsAnnotated)
                return rightComm.IsAnnotated;
            if (!rightComm.IsAnnotated)
                return !leftComm.IsAnnotated;

            AnnotatedCommodity aLeftComm = (AnnotatedCommodity)leftComm;
            AnnotatedCommodity aRightComm = (AnnotatedCommodity)rightComm;

            if (aLeftComm.Details.Price == null && aRightComm.Details.Price != null)
                return true;
            if (aLeftComm.Details.Price != null && aRightComm.Details.Price == null)
                return false;

            if (aLeftComm.Details.Price != null && aRightComm.Details.Price != null)
            {
                Amount leftPrice = aLeftComm.Details.Price;
                Amount rightPrice = aRightComm.Details.Price;

                if (leftPrice.Commodity == rightPrice.Commodity)
                {
                    return leftPrice.IsLessThan(rightPrice);
                }
                else
                {
                    // Since we have two different amounts, there's really no way
                    // to establish a true sorting order; we'll just do it based
                    // on the numerical values.
                    leftPrice = new Amount(leftPrice.Quantity, null);
                    rightPrice = new Amount(rightPrice.Quantity, null);
                    return leftPrice.IsLessThan(rightPrice);
                }
            }

            if (aLeftComm.Details.Date == null && aRightComm.Details.Date != null)
                return true;
            if (aLeftComm.Details.Date != null && aRightComm.Details.Date == null)
                return false;

            if (aLeftComm.Details.Date != null && aRightComm.Details.Date != null)
                return aLeftComm.Details.Date.Value < aRightComm.Details.Date.Value;

            if (String.IsNullOrEmpty(aLeftComm.Details.Tag) && !String.IsNullOrEmpty(aRightComm.Details.Tag))
                return true;
            if (!String.IsNullOrEmpty(aLeftComm.Details.Tag) && String.IsNullOrEmpty(aRightComm.Details.Tag))
                return false;

            if (!String.IsNullOrEmpty(aLeftComm.Details.Tag) && !String.IsNullOrEmpty(aRightComm.Details.Tag))
                return aLeftComm.Details.Tag.CompareTo(aRightComm.Details.Tag) < 0;

            if (aLeftComm.Details.ValueExpr == null && aLeftComm.Details.ValueExpr != null)
                return true;
            if (aLeftComm.Details.ValueExpr != null && aLeftComm.Details.ValueExpr == null)
                return true;

            if (aLeftComm.Details.ValueExpr != null && aLeftComm.Details.ValueExpr != null)
                return aLeftComm.Details.ValueExpr.Text.CompareTo(aLeftComm.Details.ValueExpr.Text) < 0;

            throw new InvalidOperationException("Unexpected end of method");
        }

        public static Comparison<Amount> CompareByCommodityComparison = (x, y) => 
        {
            return CompareByCommodity(x,y) ? -1 : (CompareByCommodity(y, x) ? 1 : 0);
        };

        public static bool operator == (Commodity commLeft, Commodity commRight)
        {
            if (object.ReferenceEquals(commLeft, null))
                return object.ReferenceEquals(commRight, null);

            return commLeft.Equals(commRight);
        }

        public static bool operator !=(Commodity commLeft, Commodity commRight)
        {
            return !(commLeft == commRight);
        }

        public static explicit operator bool(Commodity commodity)
        {
            return commodity != null && commodity != CommodityPool.Current.NullCommodity;
        }

        //private CommodityBase commodityBase;

        public Commodity(CommodityPool parent, CommodityBase commodityBase)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");
            if (commodityBase == null)
                throw new ArgumentNullException("commodityBase");

            Parent = parent;
            Base = commodityBase;
            // IsAnnotated = false;
        }

        public CommodityPool Parent { get; private set; }
        public CommodityBase Base { get; private set; }
        public CommodityFlagsEnum Flags
        {
            // [DM] This works in the same way as commodity's delegates_flags operate with commodity base
            get { return Base.Flags; }
            set { Base.Flags = value; }
        }

        public CommodityPool Pool
        {
            get { return Parent; }
        }

        public virtual Commodity Referent
        {
            get { return this; }
        }

        public virtual bool IsAnnotated 
        {
            get { return false; } 
        }

        public string BaseSymbol
        {
            get { return Base.Symbol; }
        }

        public string QualifiedSymbol { get; set; }

        public string Symbol
        {
            get { return String.IsNullOrEmpty(QualifiedSymbol) ? BaseSymbol : QualifiedSymbol; }
        }

        public int? GraphIndex
        {
            get { return Base.GraphIndex; }
            set { Base.GraphIndex = value; }
        }

        public Amount Smaller
        {
            get { return Base.Smaller; }
            set { Base.Smaller = value; }
        }

        public Amount Larger
        {
            get { return Base.Larger; }
            set { Base.Larger = value; }
        }

        public int Precision
        {
            get { return Base.Precision; }
            set { Base.Precision = value; }
        }

        public Expr ValueExpr 
        {
            get { return Base.ValueExpr; }
            set { Base.ValueExpr = value; }
        }

        public static string ParseSymbol(ref string line)
        {
            if (String.IsNullOrEmpty(line))
                return String.Empty;

            string originalLine = line;

            string symbol = null;
            line = line.TrimStart();
            if (line.StartsWith(QuoteCharString))
            {
                line = line.Remove(0, 1);
                symbol = StringExtensions.ReadInto(ref line, NonQuoteChars);
                if (line.StartsWith(QuoteCharString))
                    line = line.Remove(0, 1);
                else
                    throw new AmountError(AmountError.ErrorMessageNonClosedQuote);
            }
            else
            {
                int pos = 0;
                while (pos < line.Length && line[pos] != '\n' && Array.IndexOf(InvalidCommodityChars, line[pos]) == -1)
                {
                    // 1) No needs to handle UTF8
                    // 2) I see no reasons to handle backslashes
                    pos++;
                }
                symbol = line.Substring(0, pos);
                line = line.Substring(pos);

                if (ReservedTokens.Contains(symbol))
                    symbol = String.Empty;
            }

            if (String.IsNullOrEmpty(symbol))
                line = originalLine;

            return symbol;
        }

        public static bool SymbolNeedsQuotes(string symbol)
        {
            if (String.IsNullOrEmpty(symbol))
                return false;

            return symbol.Any(c => Array.IndexOf(InvalidCommodityChars, c) >= 0);
        }

        public int CompareTo(Commodity other)
        {
            return String.Compare(Symbol, other.Symbol);
        }

        public virtual bool Equals(Commodity comm)
        {
            if (Object.ReferenceEquals(comm, null))
                return false;

            if (comm.Base != Base)
                return false;

            if (IsAnnotated ^ comm.IsAnnotated)
                return false;

            return true;            
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Commodity);
        }

        public override int GetHashCode()
        {
            return Base.GetHashCode();
        }

        public virtual Commodity StripAnnotations(AnnotationKeepDetails whatToKeep)
        {
            return this;
        }

        /// <summary>
        /// Ported from void commodity_t::add_price(const datetime_t& date, const amount_t& price,
        /// </summary>
        public void AddPrice(DateTime date, Amount price, bool reflexive = true)
        {
            if (reflexive)
            {
                Logger.Debug("history.find", () => String.Format("Marking {0} as a primary commodity", price.Commodity.Symbol));
                price.Commodity.Flags = price.Commodity.Flags | CommodityFlagsEnum.COMMODITY_PRIMARY;
            }
            else
            {
                Logger.Debug("history.find", () => String.Format("Marking {0} as a primary commodity", Symbol));
                Flags = Flags | CommodityFlagsEnum.COMMODITY_PRIMARY;
            }

            Logger.Debug("history.find", () => String.Format("Adding price: {0} for {1} on {2}", Symbol, price, date));

            Pool.CommodityPriceHistory.AddPrice(Referent, date, price);

            Base.PriceMap.Clear(); // a price was added, invalid the map
        }

        public PricePoint? FindPriceFromExpr(Expr expr, Commodity commodity, DateTime moment)
        {
            Logger.Debug("commodity.price.find", () =>String.Format("valuation expr: {0}", expr.Dump()));

            Value result = expr.Calc(Scope.DefaultScope);

            if (Expr.IsExpr(result))
            {
                Value callArgs = new Value();

                callArgs.PushBack(Value.Get(BaseSymbol));
                callArgs.PushBack(Value.Get(moment));
                if (commodity != null)
                    callArgs.PushBack(Value.Get(commodity.Symbol));

                result = Expr.AsExpr(result).Call(callArgs, Scope.DefaultScope);
            }

            return new PricePoint(moment, result.AsAmount);
        }

        /// <summary>
        /// Ported from commodity_t::find_price
        /// </summary>
        public virtual PricePoint? FindPrice(Commodity commodity = null, DateTime moment = default(DateTime), DateTime oldest = default(DateTime))
        {
            Logger.Debug("commodity.price.find", () => String.Format("commodity_t::find_price({0})", Symbol));

            Commodity target = null;
            if ((bool)commodity)
                target = commodity;
            else if ((bool)Pool.DefaultCommodity)
                target = Pool.DefaultCommodity;

            if ((bool)target && this == target)
                return null;

            MemoizedPriceEntry entry = new MemoizedPriceEntry() { Start = moment, End = oldest, Commodity = commodity };

            Logger.Debug("commodity.price.find", () => String.Format("looking for memoized args: {0},{1},{2}", 
                !moment.IsNotADateTime() ? TimesCommon.Current.FormatDateTime(moment) : "NONE",
                !oldest.IsNotADateTime() ? TimesCommon.Current.FormatDateTime(oldest) : "NONE",
                commodity != null ? commodity.Symbol : "NONE"));

            PricePoint? memoizedPricePoint;
            if (Base.PriceMap.TryGetValue(entry, out memoizedPricePoint))
            {
                Logger.Debug("commodity.price.find", () => String.Format("found! returning: {0}",
                    memoizedPricePoint.HasValue ? memoizedPricePoint.Value.Price : (Amount)0));
                return memoizedPricePoint;
            }

            DateTime when;
            if (!moment.IsNotADateTime())
                when = moment;
            else if (TimesCommon.Current.Epoch.HasValue)
                when = TimesCommon.Current.Epoch.Value;
            else
                when = TimesCommon.Current.CurrentDate;

            if (Base.ValueExpr != null)
                return FindPriceFromExpr(Base.ValueExpr, commodity, when);

            PricePoint? point = target != null
                ? Pool.CommodityPriceHistory.FindPrice(Referent, target, when, oldest)
                : Pool.CommodityPriceHistory.FindPrice(Referent, when, oldest);

            // Record this price point in the memoization map
            if (Base.PriceMap.Count > CommodityBase.MaxPriceMapSize)
            {
                Logger.Debug("history.find", () => "price map has grown too large, clearing it by half");
                for (int i = 0; i < CommodityBase.MaxPriceMapSize / 2; i++)
                    Base.PriceMap.Remove(Base.PriceMap.First());
            }

            Logger.Debug("history.find", () => String.Format("remembered: {0}", point.HasValue ? point.Value.Price : (Amount)0));
            Base.PriceMap.Add(entry, point);

            return point;
        }

        public PricePoint? CheckForUpdatedPrice(PricePoint? point, DateTime moment, Commodity inTermsOf)
        {
            if (Pool.GetQuotes && !Flags.HasFlag(CommodityFlagsEnum.COMMODITY_NOMARKET))
            {
                bool exceedsLeeway = true;

                if (point.HasValue)
                {
                    long secondsDiff;
                    if (moment != default(DateTime))
                    {
                        secondsDiff = (long)(moment - point.Value.When).TotalSeconds;
                    }
                    else
                    {
                        secondsDiff = (long)(TimesCommon.Current.CurrentTime - point.Value.When).TotalSeconds;
                    }

                    if (secondsDiff < Pool.QuoteLeeway)
                        exceedsLeeway = false;
                }

                if (exceedsLeeway)
                {
                    PricePoint? quote = Pool.GetCommodityQuote(Referent, inTermsOf);
                    if (quote.HasValue)
                    {
                        if (inTermsOf == null || (quote.Value.Price.HasCommodity && quote.Value.Price.Commodity == inTermsOf))
                            return quote;
                    }
                }
            }
            return point;
        }

        public virtual string Print(bool elideQuotes = false, bool printAnnotations = false)
        {
            string sym = Symbol;
            if (elideQuotes && Flags.HasFlag(CommodityFlagsEnum.COMMODITY_STYLE_SEPARATED) && !String.IsNullOrEmpty(sym)
                && sym.StartsWith("\"") && sym.IndexOf(' ') < 0)
            {
                string subsym = sym.Substring(1, sym.Length - 2);
                if (!subsym.All(c => c.IsDigitChar()))
                    return subsym;
                else
                    return sym;
            }
            else
                return sym;
        }

        public override string ToString()
        {
            return Print(false, true);
        }

        public virtual string WriteAnnotations(bool noComputedAnnotations = false)
        {
            return string.Empty;
        }

        /// <summary>
        /// Ported from commodity_t::map_prices
        /// </summary>
        public void MapPrices(Action<DateTime,Amount> fn, DateTime moment = default(DateTime), DateTime oldest = default(DateTime), bool bidirectionally = false)
        {
            DateTime when;
            if (!moment.IsNotADateTime())
                when = moment;
            else if (TimesCommon.Current.Epoch.HasValue)
                when = TimesCommon.Current.Epoch.Value;
            else
                when = TimesCommon.Current.CurrentTime;

            CommodityPool.Current.CommodityPriceHistory.MapPrices(fn, Referent, when, oldest, bidirectionally);
        }

        public Commodity NailDown(Expr expr)
        {
            Annotation newDetails = new Annotation();

            newDetails.ValueExpr = expr;
            newDetails.IsValueExprCalculated = true;

            return Pool.FindOrCreate(Symbol, newDetails);
        }

        public void SetNote(string arg = null)
        {
            Base.Note = arg;
        }

        public int CompareTo(object obj)
        {
            return this.CompareTo(obj as Commodity);
        }

        private static string QuoteCharString = "\"";
        private static Func<char, bool> NonQuoteChars = (c) => c != '\"';

        // Invalid commodity characters:
        //   SPACE, TAB, NEWLINE, RETURN
        //   0-9 . , ; : ? ! - + * / ^ & | =
        //   < > { } [ ] ( ) @
        private static char[] InvalidCommodityChars = new char[] { ' ', '\t', '\n', '\r', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            '.', ',', ';', ':', '?', '!', '-', '+', '*', '/', '^', '&', '|', '=', '<', '>', '{', '}', '[', ']', '(', ')', '@' };

        private static ICollection<string> ReservedTokens = new SortedSet<string>() { "and", "div", "else", "false", "if", "or", "not", "true" };
    }
}
