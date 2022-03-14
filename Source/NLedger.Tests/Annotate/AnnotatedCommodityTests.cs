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
using NLedger.Commodities;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Annotate
{
    public class AnnotatedCommodityTests : TestFixture
    {
        [Fact]
        public void AnnotatedCommodity_StripAnnotations_CanKeepvEverything()
        {
            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            AnnotatedCommodity annCommodity = new AnnotatedCommodity(commodity,
                new Annotation(new Amount(10), (Date)DateTime.Now.Date, "tag") { IsPriceFixated = true, IsPriceCalculated = true, IsDateCalculated = true, IsTagCalculated = true });
            AnnotationKeepDetails keepDetails = new AnnotationKeepDetails() { KeepPrice = true, KeepDate = true, KeepTag = true };

            Commodity newCommodity = annCommodity.StripAnnotations(keepDetails);

            Assert.NotNull(newCommodity);
            Assert.Equal(annCommodity, newCommodity);
            Assert.False(Object.ReferenceEquals(annCommodity, newCommodity));
            Assert.True(newCommodity.IsAnnotated);
            AnnotatedCommodity newAnnCommodity = (AnnotatedCommodity)newCommodity;
            Assert.Equal(annCommodity.Details.Price, newAnnCommodity.Details.Price);
            Assert.Equal(annCommodity.Details.Date, newAnnCommodity.Details.Date);
            Assert.Equal(annCommodity.Details.Tag, newAnnCommodity.Details.Tag);
            // Flags are added even though they were not set in the original commodity
            Assert.True(newAnnCommodity.Details.IsPriceCalculated);
            Assert.True(newAnnCommodity.Details.IsPriceFixated);
            Assert.True(newAnnCommodity.Details.IsDateCalculated);
            Assert.True(newAnnCommodity.Details.IsTagCalculated);
        }

        [Fact]
        public void AnnotatedCommodity_Equals_ChecksIsAnnotated()
        {
            Date date = (Date)DateTime.Now.Date;
            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            AnnotatedCommodity annCommodity1 = new AnnotatedCommodity(commodity, new Annotation(new Amount(10), date, "tag"));
            AnnotatedCommodity annCommodity2 = new AnnotatedCommodity(commodity, new Annotation(new Amount(10), date, "tag"));

            Assert.False(annCommodity1.Equals(commodity));
            Assert.True(annCommodity1.Equals(annCommodity2));
        }

        [Fact]
        public void AnnotatedCommodity_Equals_ComparesDetails()
        {
            Date date = (Date)DateTime.Now.Date;
            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            AnnotatedCommodity annCommodity1 = new AnnotatedCommodity(commodity, new Annotation(new Amount(10), date, "tag"));
            AnnotatedCommodity annCommodity2 = new AnnotatedCommodity(commodity, new Annotation(new Amount(10), date, "tag"));
            AnnotatedCommodity annCommodity3 = new AnnotatedCommodity(commodity, new Annotation(new Amount(20), date, "tagq"));

            Assert.True(annCommodity1.Equals(annCommodity2));
            Assert.False(annCommodity1.Equals(annCommodity3));
            Assert.False(annCommodity2.Equals(annCommodity3));
        }

        [Fact]
        public void AnnotatedCommodity_Constructor_PopulatesQualifiedSymbol()
        {
            Date date = (Date)DateTime.Now.Date;
            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("ann-comm-q-test"));
            commodity.QualifiedSymbol = "qualified-symbol";
            AnnotatedCommodity annCommodity1 = new AnnotatedCommodity(commodity, new Annotation(new Amount(10), date, "tag"));
            Assert.Equal("qualified-symbol", annCommodity1.QualifiedSymbol);
        }

        [Fact]
        public void AnnotatedCommodity_StripAnnotations_KeepsDateIfNotOnlyActualsAndIsDateCalculated()
        {
            var details = new Annotation(null, new Date(2017, 10, 20), null) { IsDateCalculated = true };
            var commodity = CommodityPool.Current.FindOrCreate("ann-comm-strip-anno-test-1", details);

            var result1 = commodity.StripAnnotations(new AnnotationKeepDetails() { KeepDate = true });
            Assert.Equal(new Date(2017, 10, 20), ((AnnotatedCommodity)result1).Details.Date.Value);

            var result2 = commodity.StripAnnotations(new AnnotationKeepDetails() { KeepDate = true, OnlyActuals = true });
            Assert.False(result2.IsAnnotated);
        }

        [Fact]
        public void AnnotatedCommodity_StripAnnotations_KeepsTagIfNotOnlyActualsAndIsTagCalculated()
        {
            var details = new Annotation(null, null, "tag1") { IsTagCalculated = true };
            var commodity = CommodityPool.Current.FindOrCreate("ann-comm-strip-anno-test-1", details);

            var result1 = commodity.StripAnnotations(new AnnotationKeepDetails() { KeepTag = true });
            Assert.Equal("tag1", ((AnnotatedCommodity)result1).Details.Tag);

            var result2 = commodity.StripAnnotations(new AnnotationKeepDetails() { KeepTag = true, OnlyActuals = true });
            Assert.False(result2.IsAnnotated);
        }

    }
}
