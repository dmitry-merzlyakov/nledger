// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Amounts;
using NLedger.Annotate;
using NLedger.Commodities;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Annotate
{
    [TestClass]
    public class AnnotatedCommodityTests : TestFixture
    {
        [TestMethod]
        public void AnnotatedCommodity_StripAnnotations_CanKeepvEverything()
        {
            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            AnnotatedCommodity annCommodity = new AnnotatedCommodity(commodity,
                new Annotation(new Amount(10), (Date)DateTime.Now.Date, "tag") { IsPriceFixated = true, IsPriceCalculated = true, IsDateCalculated = true, IsTagCalculated = true });
            AnnotationKeepDetails keepDetails = new AnnotationKeepDetails() { KeepPrice = true, KeepDate = true, KeepTag = true };

            Commodity newCommodity = annCommodity.StripAnnotations(keepDetails);

            Assert.IsNotNull(newCommodity);
            Assert.AreEqual(annCommodity, newCommodity);
            Assert.IsFalse(Object.ReferenceEquals(annCommodity, newCommodity));
            Assert.IsTrue(newCommodity.IsAnnotated);
            AnnotatedCommodity newAnnCommodity = (AnnotatedCommodity)newCommodity;
            Assert.AreEqual(annCommodity.Details.Price, newAnnCommodity.Details.Price);
            Assert.AreEqual(annCommodity.Details.Date, newAnnCommodity.Details.Date);
            Assert.AreEqual(annCommodity.Details.Tag, newAnnCommodity.Details.Tag);
            // Flags are added even though they were not set in the original commodity
            Assert.IsTrue(newAnnCommodity.Details.IsPriceCalculated);
            Assert.IsTrue(newAnnCommodity.Details.IsPriceFixated);
            Assert.IsTrue(newAnnCommodity.Details.IsDateCalculated);
            Assert.IsTrue(newAnnCommodity.Details.IsTagCalculated);
        }

        [TestMethod]
        public void AnnotatedCommodity_Equals_ChecksIsAnnotated()
        {
            Date date = (Date)DateTime.Now.Date;
            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            AnnotatedCommodity annCommodity1 = new AnnotatedCommodity(commodity, new Annotation(new Amount(10), date, "tag"));
            AnnotatedCommodity annCommodity2 = new AnnotatedCommodity(commodity, new Annotation(new Amount(10), date, "tag"));

            Assert.IsFalse(annCommodity1.Equals(commodity));
            Assert.IsTrue(annCommodity1.Equals(annCommodity2));
        }

        [TestMethod]
        public void AnnotatedCommodity_Equals_ComparesDetails()
        {
            Date date = (Date)DateTime.Now.Date;
            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            AnnotatedCommodity annCommodity1 = new AnnotatedCommodity(commodity, new Annotation(new Amount(10), date, "tag"));
            AnnotatedCommodity annCommodity2 = new AnnotatedCommodity(commodity, new Annotation(new Amount(10), date, "tag"));
            AnnotatedCommodity annCommodity3 = new AnnotatedCommodity(commodity, new Annotation(new Amount(20), date, "tagq"));

            Assert.IsTrue(annCommodity1.Equals(annCommodity2));
            Assert.IsFalse(annCommodity1.Equals(annCommodity3));
            Assert.IsFalse(annCommodity2.Equals(annCommodity3));
        }

        [TestMethod]
        public void AnnotatedCommodity_Constructor_PopulatesQualifiedSymbol()
        {
            Date date = (Date)DateTime.Now.Date;
            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("ann-comm-q-test"));
            commodity.QualifiedSymbol = "qualified-symbol";
            AnnotatedCommodity annCommodity1 = new AnnotatedCommodity(commodity, new Annotation(new Amount(10), date, "tag"));
            Assert.AreEqual("qualified-symbol", annCommodity1.QualifiedSymbol);
        }

        [TestMethod]
        public void AnnotatedCommodity_StripAnnotations_KeepsDateIfNotOnlyActualsAndIsDateCalculated()
        {
            var details = new Annotation(null, new Date(2017, 10, 20), null) { IsDateCalculated = true };
            var commodity = CommodityPool.Current.FindOrCreate("ann-comm-strip-anno-test-1", details);

            var result1 = commodity.StripAnnotations(new AnnotationKeepDetails() { KeepDate = true });
            Assert.AreEqual(new Date(2017, 10, 20), ((AnnotatedCommodity)result1).Details.Date.Value);

            var result2 = commodity.StripAnnotations(new AnnotationKeepDetails() { KeepDate = true, OnlyActuals = true });
            Assert.IsFalse(result2.IsAnnotated);
        }

        [TestMethod]
        public void AnnotatedCommodity_StripAnnotations_KeepsTagIfNotOnlyActualsAndIsTagCalculated()
        {
            var details = new Annotation(null, null, "tag1") { IsTagCalculated = true };
            var commodity = CommodityPool.Current.FindOrCreate("ann-comm-strip-anno-test-1", details);

            var result1 = commodity.StripAnnotations(new AnnotationKeepDetails() { KeepTag = true });
            Assert.AreEqual("tag1", ((AnnotatedCommodity)result1).Details.Tag);

            var result2 = commodity.StripAnnotations(new AnnotationKeepDetails() { KeepTag = true, OnlyActuals = true });
            Assert.IsFalse(result2.IsAnnotated);
        }

    }
}
