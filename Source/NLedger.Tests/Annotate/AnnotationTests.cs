// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using NLedger.Annotate;
using NLedger.Expressions;
using NLedger.Times;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Annotate
{
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon)]
    public class AnnotationTests : TestFixture
    {
        [Fact]
        public void Annotation_Parse_HandlesDates()
        {
            Annotation annotation = new Annotation();
            Assert.Null(annotation.Date);

            string line = "[2010/02/02]";
            annotation.Parse(ref line);

            Assert.Equal(new Date(2010, 2, 2), annotation.Date);
        }

        [Fact]
        public void Annotation_Parse_HandlesTags()
        {
            Annotation annotation = new Annotation();
            Assert.Null(annotation.Tag);

            string line = "(02/02/2010)";
            annotation.Parse(ref line);

            Assert.Equal("02/02/2010", annotation.Tag);
        }

        [Fact]
        public void Annotation_Parse_HandlesPrices()
        {
            TestAnnotation annotation = new TestAnnotation();
            Assert.Null(annotation.Price);
            Assert.False(annotation.IsPriceNotPerUnit);
            Assert.False(annotation.IsPriceFixated);

            string line = "{12345}rest of string";
            annotation.Parse(ref line);

            Assert.Equal(1, annotation.PriceParseCallCounter);
            Assert.Equal("12345", annotation.PriceParseArgument);
            Assert.False(annotation.IsPriceNotPerUnit);
            Assert.False(annotation.IsPriceFixated);
            Assert.Equal("rest of string", line);
        }

        [Fact]
        public void Annotation_Parse_HandlesPerUnitPrices()
        {
            TestAnnotation annotation = new TestAnnotation();
            Assert.Null(annotation.Price);
            Assert.False(annotation.IsPriceNotPerUnit);
            Assert.False(annotation.IsPriceFixated);

            string line = "{{12345}}rest of string";
            annotation.Parse(ref line);

            Assert.Equal(1, annotation.PriceParseCallCounter);
            Assert.Equal("12345", annotation.PriceParseArgument);
            Assert.True(annotation.IsPriceNotPerUnit);
            Assert.False(annotation.IsPriceFixated);
            Assert.Equal("rest of string", line);
        }

        [Fact]
        public void Annotation_Parse_HandlesPerUnitFixatedPrices()
        {
            TestAnnotation annotation = new TestAnnotation();
            Assert.Null(annotation.Price);
            Assert.False(annotation.IsPriceNotPerUnit);
            Assert.False(annotation.IsPriceFixated);

            string line = "{{=12345}}rest of string";
            annotation.Parse(ref line);

            Assert.Equal(1, annotation.PriceParseCallCounter);
            Assert.Equal("12345", annotation.PriceParseArgument);
            Assert.True(annotation.IsPriceNotPerUnit);
            Assert.True(annotation.IsPriceFixated);
            Assert.Equal("rest of string", line);
        }

        [Fact]
        public void Annotation_Parse_HandlesExpressions()
        {
            TestAnnotation annotation = new TestAnnotation();
            Assert.Null(annotation.ValueExpr);

            string line = "((12345))rest of string";
            annotation.Parse(ref line);

            Assert.NotNull(annotation.ValueExpr);
            Assert.Equal(1, annotation.CreateExprCallCounter);
            Assert.Equal("12345", annotation.CreateExprArgument);
            Assert.Equal("rest of string", line);
        }

        [Fact]
        public void Annotation_Parse_HandlesCompositeTokens()
        {
            TestAnnotation annotation = new TestAnnotation();
            Assert.Null(annotation.Date);
            Assert.Null(annotation.Price);
            Assert.Null(annotation.ValueExpr);
            Assert.Null(annotation.Tag);

            string line = "{123} [2010/02/02] (mytag) ((2345))rest of string";
            annotation.Parse(ref line);

            Assert.Equal(new Date(2010, 2, 2), annotation.Date);

            Assert.NotNull(annotation.Price);
            Assert.Equal(1, annotation.PriceParseCallCounter);
            Assert.Equal("123", annotation.PriceParseArgument);
            Assert.False(annotation.IsPriceNotPerUnit);
            Assert.False(annotation.IsPriceFixated);

            Assert.Equal("mytag", annotation.Tag);

            Assert.NotNull(annotation.ValueExpr);
            Assert.Equal(1, annotation.CreateExprCallCounter);
            Assert.Equal("2345", annotation.CreateExprArgument);

            Assert.Equal("rest of string", line);
        }

        [Fact]
        public void Annotation_GetHashCode_ReflectsDifferencesInPrice()
        {
            Annotation ann1 = new Annotation() { Price = new Amount(10) };
            Annotation ann2 = new Annotation() { Price = new Amount(20) };
            Assert.NotEqual(ann1.GetHashCode(), ann2.GetHashCode());
        }

        [Fact]
        public void Annotation_GetHashCode_ShowsNoDifferencesForEqualPrices()
        {
            Annotation ann1 = new Annotation() { Price = new Amount(10) };
            Annotation ann2 = new Annotation() { Price = new Amount(10) };
            Assert.Equal(ann1.GetHashCode(), ann2.GetHashCode());
        }

        [Fact]
        public void Annotation_GetHashCode_ReflectsDifferencesInDate()
        {
            Annotation ann1 = new Annotation() { Date = new Date(2017, 10, 5) };
            Annotation ann2 = new Annotation() { Date = new Date(2017, 5, 2) };
            Assert.NotEqual(ann1.GetHashCode(), ann2.GetHashCode());
        }

        [Fact]
        public void Annotation_GetHashCode_ShowsNoDifferencesForEqualDate()
        {
            Annotation ann1 = new Annotation() { Date = new Date(2017, 10, 5) };
            Annotation ann2 = new Annotation() { Date = new Date(2017, 10, 5) };
            Assert.Equal(ann1.GetHashCode(), ann2.GetHashCode());
        }

        [Fact]
        public void Annotation_GetHashCode_ReflectsDifferencesInTag()
        {
            Annotation ann1 = new Annotation() { Tag = "tag1" };
            Annotation ann2 = new Annotation() { Tag = "tag2" };
            Assert.NotEqual(ann1.GetHashCode(), ann2.GetHashCode());
        }

        [Fact]
        public void Annotation_GetHashCode_ShowsNoDifferencesForEqualTag()
        {
            Annotation ann1 = new Annotation() { Tag = "tag1" };
            Annotation ann2 = new Annotation() { Tag = "tag1" };
            Assert.Equal(ann1.GetHashCode(), ann2.GetHashCode());
        }

        [Fact]
        public void Annotation_GetHashCode_ReflectsDifferencesInValueExpr()
        {
            Annotation ann1 = new Annotation() { ValueExpr = new Expr("1==2") };
            Annotation ann2 = new Annotation() { ValueExpr = new Expr("1>2") };
            Assert.NotEqual(ann1.GetHashCode(), ann2.GetHashCode());
        }

        [Fact]
        public void Annotation_GetHashCode_ShowsNoDifferencesForEqualValueExpr()
        {
            Annotation ann1 = new Annotation() { ValueExpr = new Expr("1==2") };
            Annotation ann2 = new Annotation() { ValueExpr = new Expr("1==2") };
            Assert.Equal(ann1.GetHashCode(), ann2.GetHashCode());
        }

        [Fact]
        public void Annotation_GetHashCode_ChangedFlagsAreIgnored()
        {
            Annotation ann1 = new Annotation() { Tag = "tag1" };
            Annotation ann2 = new Annotation() { Tag = "tag1" };

            Assert.Equal(ann1.GetHashCode(), ann2.GetHashCode());

            ann1.IsPriceNotPerUnit = true;
            Assert.Equal(ann1.GetHashCode(), ann2.GetHashCode());

            ann1.IsPriceFixated = true;
            Assert.Equal(ann1.GetHashCode(), ann2.GetHashCode());

            ann1.IsPriceCalculated = true;
            Assert.Equal(ann1.GetHashCode(), ann2.GetHashCode());

            ann1.IsDateCalculated = true;
            Assert.Equal(ann1.GetHashCode(), ann2.GetHashCode());

            ann1.IsTagCalculated = true;
            Assert.Equal(ann1.GetHashCode(), ann2.GetHashCode());

            ann1.IsValueExprCalculated = true;
            Assert.Equal(ann1.GetHashCode(), ann2.GetHashCode());

            ann2.IsPriceNotPerUnit = true;
            Assert.Equal(ann1.GetHashCode(), ann2.GetHashCode());

            ann2.IsPriceFixated = true;
            Assert.Equal(ann1.GetHashCode(), ann2.GetHashCode());

            ann2.IsPriceCalculated = true;
            Assert.Equal(ann1.GetHashCode(), ann2.GetHashCode());

            ann2.IsDateCalculated = true;
            Assert.Equal(ann1.GetHashCode(), ann2.GetHashCode());

            ann2.IsTagCalculated = true;
            Assert.Equal(ann1.GetHashCode(), ann2.GetHashCode());

            ann2.IsValueExprCalculated = true;
            Assert.Equal(ann1.GetHashCode(), ann2.GetHashCode());
        }

        [Fact]
        public void Annotation_IsNullOrEmpty_ReturnsTrueIfNull()
        {
            Assert.True(Annotation.IsNullOrEmpty(null));
        }

        [Fact]
        public void Annotation_IsNullOrEmpty_ReturnsTrueIfEmpty()
        {
            Assert.True(Annotation.IsNullOrEmpty(new Annotation()));
        }

        [Fact]
        public void Annotation_IsNullOrEmpty_ReturnsTrueIfEmptyAndIgnoresFlags()
        {
            Assert.True(Annotation.IsNullOrEmpty(new Annotation()
            {
                IsPriceNotPerUnit = true,
                IsPriceFixated = true,
                IsPriceCalculated = true,
                IsDateCalculated = true,
                IsTagCalculated = true,
                IsValueExprCalculated = true
            }));
        }

        public class TestAnnotation : Annotation
        {
            public int PriceParseCallCounter = 0;
            public string PriceParseArgument = null;

            protected override void PriceParse(ref string buf)
            {
                PriceParseArgument = buf;
                PriceParseCallCounter++;
                // Do nothing for test purposes
            }

            public int CreateExprCallCounter = 0;
            public string CreateExprArgument = null;

            protected override Expr CreateExpr(string buf)
            {
                CreateExprCallCounter++;
                CreateExprArgument = buf;
                // Do nothing for test purposes
                return Expr.Empty;
            }
        }
    }
}
