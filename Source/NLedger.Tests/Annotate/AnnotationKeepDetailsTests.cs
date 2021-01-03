// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Annotate;
using NLedger.Commodities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Annotate
{
    public class AnnotationKeepDetailsTests : TestFixture
    {
        [Fact]
        public void AnnotationKeepDetails_DefaultContructors_ProducesInstanceWithAllFlagsUnchecked()
        {
            AnnotationKeepDetails details = new AnnotationKeepDetails();
            Assert.False(details.KeepPrice);
            Assert.False(details.KeepDate);
            Assert.False(details.KeepTag);
            Assert.False(details.OnlyActuals);
        }

        [Fact]
        public void AnnotationKeepDetails_KeepAll_ValidationMatrix()
        {
            Assert.True((new AnnotationKeepDetails() { KeepPrice = true, KeepDate = true, KeepTag = true, OnlyActuals = false }).KeepAll() );
            Assert.False((new AnnotationKeepDetails() { KeepPrice = false, KeepDate = true, KeepTag = true, OnlyActuals = false }).KeepAll());
            Assert.False((new AnnotationKeepDetails() { KeepPrice = true, KeepDate = false, KeepTag = true, OnlyActuals = false }).KeepAll());
            Assert.False((new AnnotationKeepDetails() { KeepPrice = true, KeepDate = true, KeepTag = false, OnlyActuals = false }).KeepAll());
            Assert.False((new AnnotationKeepDetails() { KeepPrice = true, KeepDate = true, KeepTag = true, OnlyActuals = true }).KeepAll());
        }

        [Fact]
        public void AnnotationKeepDetails_KeepAll_IfCommodityIsNotAnnotated()
        {
            AnnotationKeepDetails detailsKeepAllTrue = new AnnotationKeepDetails() { KeepPrice = true, KeepDate = true, KeepTag = true, OnlyActuals = false };
            AnnotationKeepDetails detailsKeepAllFalse = new AnnotationKeepDetails() { KeepPrice = true, KeepDate = true, KeepTag = true, OnlyActuals = true };
            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            AnnotatedCommodity annCommodity = new AnnotatedCommodity(commodity, new Annotation());

            Assert.True(detailsKeepAllTrue.KeepAll(commodity));
            Assert.True(detailsKeepAllTrue.KeepAll(annCommodity));

            Assert.True(detailsKeepAllFalse.KeepAll(commodity));
            Assert.False(detailsKeepAllFalse.KeepAll(annCommodity));
        }

        [Fact]
        public void AnnotationKeepDetails_KeepAny_ValidationMatrix()
        {
            Assert.False((new AnnotationKeepDetails() { KeepPrice = false, KeepDate = false, KeepTag = false }).KeepAny());
            Assert.True((new AnnotationKeepDetails() { KeepPrice = true, KeepDate = false, KeepTag = false }).KeepAny());
            Assert.True((new AnnotationKeepDetails() { KeepPrice = false, KeepDate = true, KeepTag = false }).KeepAny());
            Assert.True((new AnnotationKeepDetails() { KeepPrice = false, KeepDate = false, KeepTag = true }).KeepAny());
        }

        [Fact]
        public void AnnotationKeepDetails_KeepAny_IfCommodityIsAnnotated()
        {
            AnnotationKeepDetails detailsKeepAnyTrue = new AnnotationKeepDetails() { KeepPrice = true, KeepDate = true, KeepTag = true };
            AnnotationKeepDetails detailsKeepAnyFalse = new AnnotationKeepDetails() { KeepPrice = false, KeepDate = false, KeepTag = false };
            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("comm"));
            AnnotatedCommodity annCommodity = new AnnotatedCommodity(commodity, new Annotation());

            Assert.False(detailsKeepAnyTrue.KeepAny(commodity));
            Assert.True(detailsKeepAnyTrue.KeepAny(annCommodity));

            Assert.False(detailsKeepAnyFalse.KeepAny(commodity));
            Assert.False(detailsKeepAnyFalse.KeepAny(annCommodity));
        }

    }
}
