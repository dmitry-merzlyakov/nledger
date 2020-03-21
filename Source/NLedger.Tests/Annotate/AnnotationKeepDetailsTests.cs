// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Annotate;
using NLedger.Commodities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Annotate
{
    [TestClass]
    public class AnnotationKeepDetailsTests : TestFixture
    {
        [TestMethod]
        public void AnnotationKeepDetails_DefaultContructors_ProducesInstanceWithAllFlagsUnchecked()
        {
            AnnotationKeepDetails details = new AnnotationKeepDetails();
            Assert.IsFalse(details.KeepPrice);
            Assert.IsFalse(details.KeepDate);
            Assert.IsFalse(details.KeepTag);
            Assert.IsFalse(details.OnlyActuals);
        }

        [TestMethod]
        public void AnnotationKeepDetails_KeepAll_ValidationMatrix()
        {
            Assert.IsTrue((new AnnotationKeepDetails() { KeepPrice = true, KeepDate = true, KeepTag = true, OnlyActuals = false }).KeepAll() );
            Assert.IsFalse((new AnnotationKeepDetails() { KeepPrice = false, KeepDate = true, KeepTag = true, OnlyActuals = false }).KeepAll());
            Assert.IsFalse((new AnnotationKeepDetails() { KeepPrice = true, KeepDate = false, KeepTag = true, OnlyActuals = false }).KeepAll());
            Assert.IsFalse((new AnnotationKeepDetails() { KeepPrice = true, KeepDate = true, KeepTag = false, OnlyActuals = false }).KeepAll());
            Assert.IsFalse((new AnnotationKeepDetails() { KeepPrice = true, KeepDate = true, KeepTag = true, OnlyActuals = true }).KeepAll());
        }

        [TestMethod]
        public void AnnotationKeepDetails_KeepAll_IfCommodityIsNotAnnotated()
        {
            AnnotationKeepDetails detailsKeepAllTrue = new AnnotationKeepDetails() { KeepPrice = true, KeepDate = true, KeepTag = true, OnlyActuals = false };
            AnnotationKeepDetails detailsKeepAllFalse = new AnnotationKeepDetails() { KeepPrice = true, KeepDate = true, KeepTag = true, OnlyActuals = true };
            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            AnnotatedCommodity annCommodity = new AnnotatedCommodity(commodity, new Annotation());

            Assert.IsTrue(detailsKeepAllTrue.KeepAll(commodity));
            Assert.IsTrue(detailsKeepAllTrue.KeepAll(annCommodity));

            Assert.IsTrue(detailsKeepAllFalse.KeepAll(commodity));
            Assert.IsFalse(detailsKeepAllFalse.KeepAll(annCommodity));
        }

        [TestMethod]
        public void AnnotationKeepDetails_KeepAny_ValidationMatrix()
        {
            Assert.IsFalse((new AnnotationKeepDetails() { KeepPrice = false, KeepDate = false, KeepTag = false }).KeepAny());
            Assert.IsTrue((new AnnotationKeepDetails() { KeepPrice = true, KeepDate = false, KeepTag = false }).KeepAny());
            Assert.IsTrue((new AnnotationKeepDetails() { KeepPrice = false, KeepDate = true, KeepTag = false }).KeepAny());
            Assert.IsTrue((new AnnotationKeepDetails() { KeepPrice = false, KeepDate = false, KeepTag = true }).KeepAny());
        }

        [TestMethod]
        public void AnnotationKeepDetails_KeepAny_IfCommodityIsAnnotated()
        {
            AnnotationKeepDetails detailsKeepAnyTrue = new AnnotationKeepDetails() { KeepPrice = true, KeepDate = true, KeepTag = true };
            AnnotationKeepDetails detailsKeepAnyFalse = new AnnotationKeepDetails() { KeepPrice = false, KeepDate = false, KeepTag = false };
            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            AnnotatedCommodity annCommodity = new AnnotatedCommodity(commodity, new Annotation());

            Assert.IsFalse(detailsKeepAnyTrue.KeepAny(commodity));
            Assert.IsTrue(detailsKeepAnyTrue.KeepAny(annCommodity));

            Assert.IsFalse(detailsKeepAnyFalse.KeepAny(commodity));
            Assert.IsFalse(detailsKeepAnyFalse.KeepAny(annCommodity));
        }

    }
}
