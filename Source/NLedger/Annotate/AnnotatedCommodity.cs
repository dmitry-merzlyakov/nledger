// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Commodities;
using NLedger.Times;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Annotate
{
    public class AnnotatedCommodity : Commodity
    {
        public AnnotatedCommodity(Commodity commodity, Annotation details)
            : base(commodity.Parent, commodity.Base)
        {
            Commodity = commodity;
            Details = details;
            QualifiedSymbol = commodity.QualifiedSymbol;
        }

        public Annotation Details { get; private set; }

        public override bool IsAnnotated
        {
            get { return true; }
        }

        public override Commodity Referent
        {
            get { return Commodity; }
        }

        public override Commodity StripAnnotations(AnnotationKeepDetails whatToKeep)
        {
            bool keepPrice = (whatToKeep.KeepPrice || (Details.IsPriceFixated && Flags.HasFlag(CommodityFlagsEnum.COMMODITY_SAW_ANN_PRICE_FLOAT) && Flags.HasFlag(CommodityFlagsEnum.COMMODITY_SAW_ANN_PRICE_FIXATED)))
                && (!whatToKeep.OnlyActuals || !Details.IsPriceCalculated);
            bool keepDate = whatToKeep.KeepDate && (!whatToKeep.OnlyActuals || !Details.IsDateCalculated);
            bool keepTag = whatToKeep.KeepTag && (!whatToKeep.OnlyActuals || !Details.IsTagCalculated);

            if ((keepPrice && Details.Price != null) || (keepDate && Details.Date.HasValue) || (keepTag && !String.IsNullOrEmpty(Details.Tag)))
            {
                Annotation newAnnotation = new Annotation(keepPrice ? Details.Price : null, keepDate ? Details.Date : null, keepTag ? Details.Tag : null);
                Commodity newCommodity = Pool.FindOrCreate(Commodity, newAnnotation);
                
                // Transfer over any relevant annotation flags, as they still apply.
                if (newCommodity.IsAnnotated)
                {
                    AnnotatedCommodity annotatedCommodity = (AnnotatedCommodity)newCommodity;
                    if (keepPrice)
                    {
                        annotatedCommodity.Details.IsPriceCalculated = Details.IsPriceCalculated;
                        annotatedCommodity.Details.IsPriceFixated = Details.IsPriceFixated;
                    }
                    if (keepDate)
                        annotatedCommodity.Details.IsDateCalculated = Details.IsDateCalculated;
                    if (keepTag)
                        annotatedCommodity.Details.IsTagCalculated = Details.IsTagCalculated;
                }

                return newCommodity;
            }
            
            return Commodity;
        }

        /// <summary>
        /// Ported from annotated_commodity_t::find_price
        /// </summary>
        public override PricePoint? FindPrice(Commodity commodity = null, DateTime moment = default(DateTime), DateTime oldest = default(DateTime))
        {
            Logger.Debug("commodity.price.find", () => String.Format("annotated_commodity_t::find_price({0})", Symbol));

            DateTime when;
            if (moment != default(DateTime))
                when = moment;
            else if (TimesCommon.Current.Epoch.HasValue)
                when = TimesCommon.Current.Epoch.Value;
            else
                when = TimesCommon.Current.CurrentTime;

            Logger.Debug("commodity.price.find", () => String.Format("reference time: {0}", when));

            Commodity target = null;
            if (commodity != null)
                target = commodity;

            if (Details.Price != null)
            {
                Logger.Debug("commodity.price.find", () => String.Format("price annotation: {0}", Details.Price));

                if (Details.IsPriceFixated)
                {
                    Logger.Debug("commodity.price.find", () => String.Format("amount_t::value: fixated price = {0}", Details.Price));
                    return new PricePoint(when, Details.Price);
                }
                else if (target == null)
                {
                    Logger.Debug("commodity.price.find", () => "setting target commodity from price");
                    target = Details.Price.Commodity;
                }
            }

            if (target != null)
                Logger.Debug("commodity.price.find", () => String.Format("target commodity: {0}", target.Symbol));

            if (Details.ValueExpr != null)
                return FindPriceFromExpr(Details.ValueExpr, commodity, when);

            return Commodity.FindPrice(target, when, oldest);
        }

        public override string Print(bool elideQuotes = false, bool printAnnotations = false)
        {
            if (printAnnotations)
            {
                return base.Print(elideQuotes) + WriteAnnotations();
            }
            else
            {
                return base.Print(elideQuotes);
            }
        }

        public override string WriteAnnotations(bool noComputedAnnotations = false)
        {
            return Details.Print(Pool.KeepBase, noComputedAnnotations);
        }

        public override bool Equals(Commodity comm)
        {
            if (!base.Equals(comm))
                return false;

            if (!comm.IsAnnotated)
                return false;

            Annotation commDetails = ((AnnotatedCommodity)comm).Details;
            if (Details == null)
                return commDetails == null;

            return Details.Equals(commDetails);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ (Details != null ? Details.GetHashCode() : 0);
        }

        protected Commodity Commodity { get; private set; }   // referent() is the synonym
    }
}
