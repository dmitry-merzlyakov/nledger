// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using NLedger.Annotate;
using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Times;
using NLedger.Utility;
using NLedger.Utils;
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
        /* DM: update 2020/3/13 after upgrade to ledger/next/1d8f8833 (Switch amounts_map to std::unordered_map).
         * Though the original commit switches balnace amounts to unordered map, .Net code still need special control
         * for default ordering of commodities. The previous considerations still make sence: changing SortedDictionary to Dictionary
         * leads to failed tests (cmd-pricemap.test, regress\7C44010B.test) because of differences in hashing between .Net Dictionary and the unordered_map.
         * DM: migrated test "cmd-org.test" revealed an issue with class balance_t.
         * In particular, its member "amounts" (typedef std::map<commodity_t *, amount_t> amounts_map)
         * does not specify any comparison rules (there is no third generic type in the template), 
         * so std::map uses a default comparer (std:less<K>). It compares commodity instances by means of operator "<" - __x < __y.
         * Since commodity_to does not override this operator, the system compares _object_allocation_addresses_ in memory.
         * That is, a commodity that is allocated later is always upper any commodities allocated before (because its address in memory 
         * numerically higher).
         * This comparer mimics the same behavior (at least, in order to pass all tests in the same way).
        */
        private static long GlobalCommodityAllocationCounter = 0;
        private long CommodityAllocationNumber = ++GlobalCommodityAllocationCounter;
        public static readonly IComparer<Commodity> DefaultComparer = new DefaultCommodityComparer();
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
            /// <summary>
            /// Ported bool decimal_comma_by_default
            /// </summary>
            public bool DecimalCommaByDefault { get; set; }

            /// <summary>
            /// Ported bool time_colon_by_default
            /// </summary>
            public bool TimeColonByDefault { get; set; }
        }

        public static CommodityDefaults Defaults => MainApplicationContext.Current?.CommodityDefaults;

        public static void Initialize()
        {
            MainApplicationContext.Current.CommodityDefaults = null;
        }

        public static void Shutdown()
        {
            MainApplicationContext.Current.CommodityDefaults = null;
        }
        #endregion

        public static readonly string DebugCommodityCompare = "commodity.compare";

        /// <summary>
        /// Ported from bool commodity_t::compare_by_commodity::operator()
        /// </summary>
        public static int CompareByCommodity(Amount left, Amount right)
        {
            if (left == null)
                throw new ArgumentNullException("left");
            if (right == null)
                throw new ArgumentNullException("right");

            Commodity leftComm = left.Commodity;
            Commodity rightComm = right.Commodity;

            Logger.Current.Debug(DebugCommodityCompare, () => String.Format(" left symbol ({0})", leftComm));
            Logger.Current.Debug(DebugCommodityCompare, () => String.Format("right symbol ({0})", rightComm));

            if (leftComm.BaseSymbol != rightComm.BaseSymbol)
            {
                Logger.Current.Debug(DebugCommodityCompare, () => "symbol is <");
                return String.Compare(leftComm.BaseSymbol, rightComm.BaseSymbol, StringComparison.Ordinal);
            }

            if (!leftComm.IsAnnotated && rightComm.IsAnnotated)
            {
                Logger.Current.Debug(DebugCommodityCompare, () => "left has no annotation, right does");
                return -1;
            }
            else if (leftComm.IsAnnotated && !rightComm.IsAnnotated)
            {
                Logger.Current.Debug(DebugCommodityCompare, () => "right has no annotation, left does");
                return 1;
            }
            else if (!leftComm.IsAnnotated && !rightComm.IsAnnotated)
            {
                Logger.Current.Debug(DebugCommodityCompare, () => "there are no annotations, commodities match");
                return 0;
            }

            AnnotatedCommodity aLeftComm = (AnnotatedCommodity)leftComm;
            AnnotatedCommodity aRightComm = (AnnotatedCommodity)rightComm;

            if (aLeftComm.Details.Price == null && aRightComm.Details.Price != null)
            {
                Logger.Current.Debug(DebugCommodityCompare, () => "left has no price, right does");
                return -1;
            }
            if (aLeftComm.Details.Price != null && aRightComm.Details.Price == null)
            {
                Logger.Current.Debug(DebugCommodityCompare, () => "right has no price, left does");
                return 1;
            }

            if (aLeftComm.Details.Price != null && aRightComm.Details.Price != null)
            {
                Amount leftPrice = aLeftComm.Details.Price;
                Amount rightPrice = aRightComm.Details.Price;

                if (leftPrice.Commodity != rightPrice.Commodity)
                {
                    // Since we have two different amounts, there's really no way
                    // to establish a true sorting order; we'll just do it based
                    // on the numerical values.
                    leftPrice = new Amount(leftPrice.Quantity, null);
                    rightPrice = new Amount(rightPrice.Quantity, null);
                    Logger.Current.Debug(DebugCommodityCompare, () => "both have price, commodities don't match, recursing");
                    int cmp2 = CompareByCommodity(leftPrice, rightPrice);
                    if (cmp2 != 0)
                    {
                        Logger.Current.Debug(DebugCommodityCompare, () => "recursion found a disparity");
                        return cmp2;
                    }
                }
                else
                {
                    if (leftPrice.IsLessThan(rightPrice))
                    {
                        Logger.Current.Debug(DebugCommodityCompare, () => "left price is less");
                        return -1;
                    }
                    else if(leftPrice.IsGreaterThan(rightPrice))
                    {
                        Logger.Current.Debug(DebugCommodityCompare, () => "left price is more");
                        return 1;
                    }
                }
            }

            if (aLeftComm.Details.Date == null && aRightComm.Details.Date != null)
            {
                Logger.Current.Debug(DebugCommodityCompare, () => "left has no date, right does");
                return -1;
            }
            if (aLeftComm.Details.Date != null && aRightComm.Details.Date == null)
            {
                Logger.Current.Debug(DebugCommodityCompare, () => "right has no date, left does");
                return 1;
            }

            if (aLeftComm.Details.Date != null && aRightComm.Details.Date != null)
            {
                Logger.Current.Debug(DebugCommodityCompare, () => "both have dates, comparing on difference");
                TimeSpan diff = aLeftComm.Details.Date.Value - aRightComm.Details.Date.Value;
                if (diff.Ticks < 0)
                {
                    Logger.Current.Debug(DebugCommodityCompare, () => "dates differ");
                    return -1;
                }
                if (diff.Ticks > 0)
                {
                    Logger.Current.Debug(DebugCommodityCompare, () => "dates differ");
                    return 1;
                }
            }

            if (String.IsNullOrEmpty(aLeftComm.Details.Tag) && !String.IsNullOrEmpty(aRightComm.Details.Tag))
            {
                Logger.Current.Debug(DebugCommodityCompare, () => "left has no tag, right does");
                return -1;
            }
            if (!String.IsNullOrEmpty(aLeftComm.Details.Tag) && String.IsNullOrEmpty(aRightComm.Details.Tag))
            {
                Logger.Current.Debug(DebugCommodityCompare, () => "right has no tag, left does");
                return 1;
            }

            if (!String.IsNullOrEmpty(aLeftComm.Details.Tag) && !String.IsNullOrEmpty(aRightComm.Details.Tag))
            {
                Logger.Current.Debug(DebugCommodityCompare, () => "both have tags, comparing lexically");
                return aLeftComm.Details.Tag.CompareTo(aRightComm.Details.Tag);
            }

            if (aLeftComm.Details.ValueExpr == null && aLeftComm.Details.ValueExpr != null)
            {
                Logger.Current.Debug(DebugCommodityCompare, () => "left has no value expr, right does");
                return -1;
            }
            if (aLeftComm.Details.ValueExpr != null && aLeftComm.Details.ValueExpr == null)
            {
                Logger.Current.Debug(DebugCommodityCompare, () => "right has no value expr, left does");
                return 1;
            }

            if (aLeftComm.Details.ValueExpr != null && aLeftComm.Details.ValueExpr != null)
            {
                Logger.Current.Debug(DebugCommodityCompare, () => "both have value exprs, comparing text reprs");
                return aLeftComm.Details.ValueExpr.Text.CompareTo(aLeftComm.Details.ValueExpr.Text);
            }

            Logger.Current.Debug(DebugCommodityCompare, () => "the two are incomparable, which should never happen");

            // assert(false);
            // return -1;

            // [DM] It is an important difference between ledger and nledger code: we need to return "0" here for equal annotated commodities.
            // Otherwise, stable sort in OrderBy will not work.
            return 0;
        }

        public static Comparison<Amount> CompareByCommodityComparison = (x, y) => 
        {
            return CompareByCommodity(x,y);
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

        public string Name
        {
            get { return Base.Name; }
        }

        public string Note
        {
            get { return Base.Note; }
        }

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
                Logger.Current.Debug("history.find", () => String.Format("Marking {0} as a primary commodity", price.Commodity.Symbol));
                price.Commodity.Flags = price.Commodity.Flags | CommodityFlagsEnum.COMMODITY_PRIMARY;
            }
            else
            {
                Logger.Current.Debug("history.find", () => String.Format("Marking {0} as a primary commodity", Symbol));
                Flags = Flags | CommodityFlagsEnum.COMMODITY_PRIMARY;
            }

            Logger.Current.Debug("history.find", () => String.Format("Adding price: {0} for {1} on {2}", Symbol, price, date));

            Pool.CommodityPriceHistory.AddPrice(Referent, date, price);

            Base.PriceMap.Clear(); // a price was added, invalid the map
        }

        /// <summary>
        /// Ported from void commodity_t::remove_price(const datetime_t& date, commodity_t& commodity)
        /// </summary>
        public void RemovePrice(DateTime date, Commodity commodity)
        {
            Pool.CommodityPriceHistory.RemovePrice(Referent, commodity, date);
            Logger.Current.Debug("history.find", () => String.Format("Removing price: {0} on {1}", Symbol, date));
            Base.PriceMap.Clear();
        }

        public PricePoint? FindPriceFromExpr(Expr expr, Commodity commodity, DateTime moment)
        {
            Logger.Current.Debug("commodity.price.find", () =>String.Format("valuation expr: {0}", expr.Dump()));

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
            Logger.Current.Debug("commodity.price.find", () => String.Format("commodity_t::find_price({0})", Symbol));

            Commodity target = null;
            if ((bool)commodity)
                target = commodity;
            else if ((bool)Pool.DefaultCommodity)
                target = Pool.DefaultCommodity;

            if ((bool)target && this == target)
                return null;

            MemoizedPriceEntry entry = new MemoizedPriceEntry() { Start = moment, End = oldest, Commodity = commodity };

            Logger.Current.Debug("commodity.price.find", () => String.Format("looking for memoized args: {0},{1},{2}", 
                !moment.IsNotADateTime() ? TimesCommon.Current.FormatDateTime(moment) : "NONE",
                !oldest.IsNotADateTime() ? TimesCommon.Current.FormatDateTime(oldest) : "NONE",
                commodity != null ? commodity.Symbol : "NONE"));

            PricePoint? memoizedPricePoint;
            if (Base.PriceMap.TryGetValue(entry, out memoizedPricePoint))
            {
                Logger.Current.Debug("commodity.price.find", () => String.Format("found! returning: {0}",
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
                Logger.Current.Debug("history.find", () => "price map has grown too large, clearing it by half");
                for (int i = 0; i < CommodityBase.MaxPriceMapSize / 2; i++)
                    Base.PriceMap.Remove(Base.PriceMap.First());
            }

            Logger.Current.Debug("history.find", () => String.Format("remembered: {0}", point.HasValue ? point.Value.Price : (Amount)0));
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
                        Logger.Current.Debug("commodity.download", () => string.Format("moment = {0}", moment));
                        Logger.Current.Debug("commodity.download", () => string.Format("slip.moment = {0}", secondsDiff));
                    }
                    else
                    {
                        secondsDiff = (long)(TimesCommon.Current.CurrentTime - point.Value.When).TotalSeconds;
                        Logger.Current.Debug("commodity.download", () => string.Format("slip.now = {0}", secondsDiff));
                    }

                    Logger.Current.Debug("commodity.download", () => string.Format("leeway = {0}", Pool.QuoteLeeway));
                    if (secondsDiff < Pool.QuoteLeeway)
                        exceedsLeeway = false;
                }

                if (exceedsLeeway)
                {
                    Logger.Current.Debug("commodity.download", () => "attempting to download a more current quote...");
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

        public void SetName(string arg = null)
        {
            Base.Name = arg;
        }

        public void SetNote(string arg = null)
        {
            Base.Note = arg;
        }

        public int CompareTo(object obj)
        {
            return this.CompareTo(obj as Commodity);
        }

        /// <summary>
        /// Ported from bool commodity_t::valid()
        /// </summary>
        /// <returns></returns>
        public bool Valid()
        {
            if (String.IsNullOrEmpty(Symbol) && this != Pool.NullCommodity)
            {
                Logger.Current.Debug("ledger.validate", () => "commodity_t: symbol().empty() && this != null_commodity");
                return false;
            }

            if (IsAnnotated && Base == null)
            {
                Logger.Current.Debug("ledger.validate", () => "commodity_t: annotated && ! base");
                return false;
            }

            if (Precision > 16)
            {
                Logger.Current.Debug("ledger.validate", () => "commodity_t: precision() > 16");
                return false;
            }

            return true;
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
