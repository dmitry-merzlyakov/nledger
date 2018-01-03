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
using NLedger.Times;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Commodities
{
    /// <summary>
    /// Porrted from cost_breakdown_t
    /// </summary>
    public struct CostBreakdown
    {
        public Amount Amount { get; set; }
        public Amount FinalCost { get; set; }
        public Amount BasisCost { get; set; }
    }

    /// <summary>
    /// Ported from commodity_pool_t
    /// </summary>
    public class CommodityPool
    {
        public static CommodityPool Current
        {
            get { return MainApplicationContext.Current.CommodityPool; }
        }

        public static void Cleanup()
        {
            MainApplicationContext.Current.CommodityPool = null;
        }

        private class AnnotatedCommodityComparer : IEqualityComparer<Tuple<string, Annotation>>
        {
            public static AnnotatedCommodityComparer Current = new AnnotatedCommodityComparer();

            public bool Equals(Tuple<string, Annotation> x, Tuple<string, Annotation> y)
            {
                return x.Item1 == y.Item1 && x.Item2.Equals(y.Item2);
            }

            public int GetHashCode(Tuple<string, Annotation> obj)
            {
                return obj.Item1.GetHashCode() ^ obj.Item2.GetHashCode();
            }
        }

        public CommodityPool()
        {
            Commodities = new Dictionary<string, Commodity>();
            AnnotatedCommodities = new Dictionary<Tuple<string, Annotation>, Commodity>(AnnotatedCommodityComparer.Current);
            CommodityPriceHistory = new CommodityHistory();

            NullCommodity = Create(String.Empty);
            NullCommodity.Flags |= CommodityFlagsEnum.COMMODITY_BUILTIN;
            NullCommodity.Flags |= CommodityFlagsEnum.COMMODITY_NOMARKET;

            GetCommodityQuote = CommodityQuoteFromScript;
        }

        public static PricePoint? CommodityQuoteFromScript(Commodity commodity, Commodity exchange_commodity)
        {
            // All original code is not intended to work on WIN32 platform...
            // TODO - future enhancements...
            return null;
        }

        public bool KeepBase { get; set; }  // --base
        public string PriceDb { get; set; }  // --price-db= 
        public long QuoteLeeway { get; set; }  // --leeway= 
        public bool GetQuotes { get; set; }   // --download

        public Commodity NullCommodity { get; private set; }
        public IDictionary<string, Commodity> Commodities { get; private set; }
        public IDictionary<Tuple<string,Annotation>, Commodity> AnnotatedCommodities { get; private set; }
        public CommodityHistory CommodityPriceHistory { get; private set; }
        public Commodity DefaultCommodity { get; set; }
        public Func<Commodity, Commodity, PricePoint?> GetCommodityQuote { get; set; }

        public Commodity Create(string symbol)
        {
            CommodityBase commodityBase = new CommodityBase(symbol);
            Commodity commodity = new Commodity(this, commodityBase);

            Logger.Debug("pool.commodities", () => String.Format("Creating base commodity {0}", symbol));

            // Create the "qualified symbol" version of this commodity's symbol
            if (Commodity.SymbolNeedsQuotes(symbol))
                commodity.QualifiedSymbol = String.Format("\"{0}\"", symbol);

            Logger.Debug("pool.commodities", () => String.Format("Creating commodity '{0}'", symbol));

            Commodities.Add(symbol, commodity);
            CommodityPriceHistory.AddCommodity(commodity);

            return commodity;
        }

        public Commodity Create(Commodity commodity, Annotation details)
        {
            if (commodity == null)
                throw new ArgumentNullException("commodity");
            if (commodity.IsAnnotated)
                throw new ArgumentException("Commodity is already annotated");
            if (details == null)
                throw new ArgumentNullException("details");

            Logger.Debug("pool.commodities", () => String.Format("commodity_pool_t::create[ann:comm] symbol {0}\r\n{1}", commodity.BaseSymbol, details));

            AnnotatedCommodity annotatedCommodity = new AnnotatedCommodity(commodity, details);

            commodity.Flags |= CommodityFlagsEnum.COMMODITY_SAW_ANNOTATED;
            if (details.Price != null)
            {
                if (details.IsPriceFixated)
                    commodity.Flags |= CommodityFlagsEnum.COMMODITY_SAW_ANN_PRICE_FIXATED;
                else
                    commodity.Flags |= CommodityFlagsEnum.COMMODITY_SAW_ANN_PRICE_FLOAT;
            }

            Logger.Debug("pool.commodities", () => String.Format("Creating annotated commodity symbol {0}\r\n{1}", commodity.BaseSymbol, details));

            AnnotatedCommodities.Add(new Tuple<string, Annotation>(commodity.BaseSymbol, details), annotatedCommodity);

            return annotatedCommodity;
        }

        public Commodity Create(string symbol, Annotation details)
        {
            Logger.Debug("pool.commodities", () => String.Format("commodity_pool_t::create[ann] symbol {0}\r\n{1}", symbol, details));

            if (details != null)
                return Create(FindOrCreate(symbol), details);
            else
                return Create(symbol);
        }

        public Commodity Find(string symbol)
        {
            Logger.Debug("pool.commodities", () => String.Format("Find commodity {0}", symbol));

            Commodity commodity;
            Commodities.TryGetValue(symbol, out commodity);
            return commodity;
        }

        public Commodity Find(string symbol, Annotation details)
        {
            Logger.Debug("pool.commodities", () => String.Format("commodity_pool_t::find[ann] symbol {0}\r\n{1}", symbol, details));

            Commodity commodity;
            var key = new Tuple<string, Annotation>(symbol, details);
            AnnotatedCommodities.TryGetValue(key, out commodity);

            if (commodity != null)
                Logger.Debug("pool.commodities", () => String.Format("commodity_pool_t::find[ann] found symbol {0}\r\n{1}", commodity.BaseSymbol, ((AnnotatedCommodity)commodity).Details));

            return commodity;
        }

        public Commodity FindOrCreate(string symbol)
        {
            Logger.Debug("pool.commodities", () => String.Format("Find-or-create commodity {0}", symbol));
            return Find(symbol) ?? Create(symbol);
        }

        public Commodity FindOrCreate(Commodity commodity, Annotation details)
        {
            Logger.Debug("pool.commodities", () => String.Format("commodity_pool_t::find_or_create[ann:comm] symbol {0}\r\n{1}", commodity.BaseSymbol, details));

            if (details != null)
            {
                Commodity annotatedCommodity = Find(commodity.BaseSymbol, details);
                if (annotatedCommodity != null)
                {
                    if (!annotatedCommodity.IsAnnotated || ((AnnotatedCommodity)annotatedCommodity).Details == null)
                        throw new InvalidOperationException("Commodity is not annotated");

                    return annotatedCommodity;
                }
                else
                {
                    return Create(commodity, details);
                }
            }
            else
            {
                return commodity;
            }
        }
        
        public Commodity FindOrCreate(string symbol, Annotation details)
        {
            Logger.Debug("pool.commodities", () => String.Format("commodity_pool_t::find_or_create[ann] symbol {0}\r\n{1}", symbol, details));

            if (details != null)
            {
                Commodity annComm = Find(symbol, details);
                if (annComm != null)
                {
                    if (!annComm.IsAnnotated || ((AnnotatedCommodity)annComm).Details == null)
                        throw new InvalidOperationException("Commodity is not annotated");
                    return annComm;
                }
                else
                {
                    return Create(symbol, details);
                }
                
            }
            else
                return FindOrCreate(symbol);
        }

        /// <summary>
        /// Exchange one commodity for another, while recording the factored price.
        /// </summary>
        public void Exchange(Commodity commodity, Amount perUnitCost, DateTime moment)
        {
            Logger.Debug("commodity.prices.add", () => String.Format("exchanging commodity {0} at per unit cost {1} on {2}", commodity, perUnitCost, moment));
            Commodity baseCommodity = commodity.IsAnnotated ? ((AnnotatedCommodity)commodity).Referent : commodity;
            baseCommodity.AddPrice(moment, perUnitCost);
        }

        public CostBreakdown Exchange(Amount amount, Amount cost, bool isPerUnit = false, bool addPrice = true, DateTime? moment = null, string tag = null)
        {
            Logger.Debug("commodity.prices.add", () => String.Format("exchange: {0} for {1}", amount, cost));
            Logger.Debug("commodity.prices.add", () => String.Format("exchange: is-per-unit   = {0}", isPerUnit));
            if (moment.HasValue)
                Logger.Debug("commodity.prices.add", () => String.Format("exchange: moment        = {0}", moment.Value));
            if (tag != null)
                Logger.Debug("commodity.prices.add", () => String.Format("exchange: tag           = {0}", tag));

            Commodity commodity = amount.Commodity;
            Annotation currentAnnotation = null;

            if (commodity.IsAnnotated)
                currentAnnotation = ((AnnotatedCommodity)commodity).Details;

            Amount perUnitCost = isPerUnit || amount.IsRealZero ? cost.Abs() : (cost / amount).Abs();

            if (!cost.HasCommodity)
                perUnitCost.ClearCommodity();

            Logger.Debug("commodity.prices.add", () => String.Format("exchange: per-unit-cost = {0}", perUnitCost));

            // Do not record commodity exchanges where amount's commodity has a
            // fixated price, since this does not establish a market value for the
            // base commodity.
            if (addPrice && !perUnitCost.IsRealZero && (currentAnnotation == null || !(currentAnnotation.Price != null && currentAnnotation.IsPriceFixated)) &&
                commodity.Referent != perUnitCost.Commodity.Referent)
            {
                Exchange(commodity, perUnitCost, moment ?? TimesCommon.Current.CurrentTime);
            }

            CostBreakdown breakdown = new CostBreakdown();
            breakdown.FinalCost = !isPerUnit ? cost : cost * amount.Abs();

            Logger.Debug("commodity.prices.add", () => String.Format("exchange: final-cost    = {0}", breakdown.FinalCost));

            if (currentAnnotation != null && currentAnnotation.Price != null)
                breakdown.BasisCost = (currentAnnotation.Price * amount).Unrounded();
            else
                breakdown.BasisCost = breakdown.FinalCost;

            Logger.Debug("commodity.prices.add", () => String.Format("exchange: basis-cost    = {0}", breakdown.BasisCost));

            Annotation annotation = new Annotation(perUnitCost, moment.HasValue ? (Date)moment.Value.Date : default(Date), tag);
            annotation.IsPriceCalculated = true;
            if (currentAnnotation != null && currentAnnotation.IsPriceFixated)
                annotation.IsPriceFixated = true;
            if (moment.HasValue)
                annotation.IsDateCalculated = true;
            if (!string.IsNullOrEmpty(tag))
                annotation.IsTagCalculated = true;

            breakdown.Amount = new Amount(amount, annotation);

            Logger.Debug("commodity.prices.add", () => String.Format("exchange: amount        = {0}", breakdown.Amount));

            return breakdown;
        }

        /// <summary>
        /// Parse commodity prices from a textual representation
        /// </summary>
        /// <remarks>
        /// ported from parse_price_directive
        /// </remarks>
        public Tuple<Commodity,PricePoint> ParsePriceDirective(string line, bool doNotAddPrice = false, bool noDate = false)
        {
            string timeField = StringExtensions.NextElement(ref line);
            if (String.IsNullOrWhiteSpace(timeField))
                return default(Tuple<Commodity,PricePoint>);
            string dateField = line;

            string symbolAndPrice = null;
            DateTime dateTime = default(DateTime);
            string symbol = null;

            if (!noDate && Char.IsDigit(timeField[0]))
            {
                symbolAndPrice = StringExtensions.NextElement(ref timeField);
                if (String.IsNullOrWhiteSpace(symbolAndPrice))
                    return default(Tuple<Commodity,PricePoint>);

                dateTime = TimesCommon.Current.ParseDateTime(dateField + " " + timeField);
            }
            else if (!noDate && Char.IsDigit(dateField[0]))
            {
                symbolAndPrice = timeField;

                dateTime = TimesCommon.Current.ParseDate(dateField);
            }
            else
            {
                symbol = dateField;
                symbolAndPrice = timeField;
                dateTime = TimesCommon.Current.CurrentTime;
            }

            if (String.IsNullOrEmpty(symbol))
                symbol = Commodity.ParseSymbol(ref symbolAndPrice);

            PricePoint point = new PricePoint(dateTime, new Amount());
            point.Price.Parse(ref symbolAndPrice, AmountParseFlagsEnum.PARSE_NO_MIGRATE);
            Validator.Verify(point.Price.Valid());

            Logger.Debug(DebugCommodityDownload, () => "Looking up symbol: " + symbol);
            Commodity commodity = FindOrCreate(symbol);
            if (commodity != null)
            {
                Logger.Debug(DebugCommodityDownload, () => String.Format("Adding price for {0}: {1} {2}", symbol, point.When, point.Price));
                if (!doNotAddPrice)
                    commodity.AddPrice(point.When, point.Price, true);
                commodity.Flags |= CommodityFlagsEnum.COMMODITY_KNOWN;
                return new Tuple<Commodity,PricePoint>(commodity, point);
            }

            return default(Tuple<Commodity, PricePoint>);
        }

        public Commodity ParsePriceExpression (string str, bool addPrice = true, DateTime? moment = null)
        {
            string buf = str;
            string price = null;
            int pos = str.IndexOf('=');
            if (pos >= 0)
            {
                buf = str.Remove(pos).Trim();
                price = str.Substring(pos + 1).Trim();
            }

            Commodity commodity = FindOrCreate(buf);
            if (commodity != null)
            {
                if (!String.IsNullOrEmpty(price) && addPrice)
                {
                    foreach (string p in price.Split(';'))
                    {
                        commodity.AddPrice(moment.HasValue ? moment.Value : TimesCommon.Current.CurrentDate, new Amount(p));
                    }
                }
                return commodity;
            }
            return null;
        }

        public Commodity Alias(string name, Commodity referent)
        {
            Commodity commodity;
            if (!Commodities.TryGetValue(referent.BaseSymbol, out commodity))
                throw new InvalidOperationException("assert(i != commodities.end());");

            Commodities.Add(name, commodity);
            return commodity;
        }

        private const string DebugCommodityDownload = "commodity.download";
    }
}
