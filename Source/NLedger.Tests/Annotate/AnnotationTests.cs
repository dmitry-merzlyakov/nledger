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
using NLedger.Expressions;
using NLedger.Times;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Annotate
{
    [TestClass]
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon)]
    public class AnnotationTests : TestFixture
    {
        [TestMethod]
        public void Annotation_Parse_HandlesDates()
        {
            Annotation annotation = new Annotation();
            Assert.IsNull(annotation.Date);

            string line = "[2010/02/02]";
            annotation.Parse(ref line);

            Assert.AreEqual(new Date(2010, 2, 2), annotation.Date);
        }

        [TestMethod]
        public void Annotation_Parse_HandlesTags()
        {
            Annotation annotation = new Annotation();
            Assert.IsNull(annotation.Tag);

            string line = "(02/02/2010)";
            annotation.Parse(ref line);

            Assert.AreEqual("02/02/2010", annotation.Tag);
        }

        [TestMethod]
        public void Annotation_Parse_HandlesPrices()
        {
            TestAnnotation annotation = new TestAnnotation();
            Assert.IsNull(annotation.Price);
            Assert.IsFalse(annotation.IsPriceNotPerUnit);
            Assert.IsFalse(annotation.IsPriceFixated);

            string line = "{12345}rest of string";
            annotation.Parse(ref line);

            Assert.AreEqual(1, annotation.PriceParseCallCounter);
            Assert.AreEqual("12345", annotation.PriceParseArgument);
            Assert.IsFalse(annotation.IsPriceNotPerUnit);
            Assert.IsFalse(annotation.IsPriceFixated);
            Assert.AreEqual("rest of string", line);
        }

        [TestMethod]
        public void Annotation_Parse_HandlesPerUnitPrices()
        {
            TestAnnotation annotation = new TestAnnotation();
            Assert.IsNull(annotation.Price);
            Assert.IsFalse(annotation.IsPriceNotPerUnit);
            Assert.IsFalse(annotation.IsPriceFixated);

            string line = "{{12345}}rest of string";
            annotation.Parse(ref line);

            Assert.AreEqual(1, annotation.PriceParseCallCounter);
            Assert.AreEqual("12345", annotation.PriceParseArgument);
            Assert.IsTrue(annotation.IsPriceNotPerUnit);
            Assert.IsFalse(annotation.IsPriceFixated);
            Assert.AreEqual("rest of string", line);
        }

        [TestMethod]
        public void Annotation_Parse_HandlesPerUnitFixatedPrices()
        {
            TestAnnotation annotation = new TestAnnotation();
            Assert.IsNull(annotation.Price);
            Assert.IsFalse(annotation.IsPriceNotPerUnit);
            Assert.IsFalse(annotation.IsPriceFixated);

            string line = "{{=12345}}rest of string";
            annotation.Parse(ref line);

            Assert.AreEqual(1, annotation.PriceParseCallCounter);
            Assert.AreEqual("12345", annotation.PriceParseArgument);
            Assert.IsTrue(annotation.IsPriceNotPerUnit);
            Assert.IsTrue(annotation.IsPriceFixated);
            Assert.AreEqual("rest of string", line);
        }

        [TestMethod]
        public void Annotation_Parse_HandlesExpressions()
        {
            TestAnnotation annotation = new TestAnnotation();
            Assert.IsNull(annotation.ValueExpr);

            string line = "((12345))rest of string";
            annotation.Parse(ref line);

            Assert.IsNotNull(annotation.ValueExpr);
            Assert.AreEqual(1, annotation.CreateExprCallCounter);
            Assert.AreEqual("12345", annotation.CreateExprArgument);
            Assert.AreEqual("rest of string", line);
        }

        [TestMethod]
        public void Annotation_Parse_HandlesCompositeTokens()
        {
            TestAnnotation annotation = new TestAnnotation();
            Assert.IsNull(annotation.Date);
            Assert.IsNull(annotation.Price);
            Assert.IsNull(annotation.ValueExpr);
            Assert.IsNull(annotation.Tag);

            string line = "{123} [2010/02/02] (mytag) ((2345))rest of string";
            annotation.Parse(ref line);

            Assert.AreEqual(new Date(2010, 2, 2), annotation.Date);

            Assert.IsNotNull(annotation.Price);
            Assert.AreEqual(1, annotation.PriceParseCallCounter);
            Assert.AreEqual("123", annotation.PriceParseArgument);
            Assert.IsFalse(annotation.IsPriceNotPerUnit);
            Assert.IsFalse(annotation.IsPriceFixated);

            Assert.AreEqual("mytag", annotation.Tag);

            Assert.IsNotNull(annotation.ValueExpr);
            Assert.AreEqual(1, annotation.CreateExprCallCounter);
            Assert.AreEqual("2345", annotation.CreateExprArgument);

            Assert.AreEqual("rest of string", line);
        }

        [TestMethod]
        public void Annotation_GetHashCode_ReflectsDifferencesInPrice()
        {
            Annotation ann1 = new Annotation() { Price = new Amount(10) };
            Annotation ann2 = new Annotation() { Price = new Amount(20) };
            Assert.AreNotEqual(ann1.GetHashCode(), ann2.GetHashCode());
        }

        [TestMethod]
        public void Annotation_GetHashCode_ShowsNoDifferencesForEqualPrices()
        {
            Annotation ann1 = new Annotation() { Price = new Amount(10) };
            Annotation ann2 = new Annotation() { Price = new Amount(10) };
            Assert.AreEqual(ann1.GetHashCode(), ann2.GetHashCode());
        }

        [TestMethod]
        public void Annotation_GetHashCode_ReflectsDifferencesInDate()
        {
            Annotation ann1 = new Annotation() { Date = new Date(2017, 10, 5) };
            Annotation ann2 = new Annotation() { Date = new Date(2017, 5, 2) };
            Assert.AreNotEqual(ann1.GetHashCode(), ann2.GetHashCode());
        }

        [TestMethod]
        public void Annotation_GetHashCode_ShowsNoDifferencesForEqualDate()
        {
            Annotation ann1 = new Annotation() { Date = new Date(2017, 10, 5) };
            Annotation ann2 = new Annotation() { Date = new Date(2017, 10, 5) };
            Assert.AreEqual(ann1.GetHashCode(), ann2.GetHashCode());
        }

        [TestMethod]
        public void Annotation_GetHashCode_ReflectsDifferencesInTag()
        {
            Annotation ann1 = new Annotation() { Tag = "tag1" };
            Annotation ann2 = new Annotation() { Tag = "tag2" };
            Assert.AreNotEqual(ann1.GetHashCode(), ann2.GetHashCode());
        }

        [TestMethod]
        public void Annotation_GetHashCode_ShowsNoDifferencesForEqualTag()
        {
            Annotation ann1 = new Annotation() { Tag = "tag1" };
            Annotation ann2 = new Annotation() { Tag = "tag1" };
            Assert.AreEqual(ann1.GetHashCode(), ann2.GetHashCode());
        }

        [TestMethod]
        public void Annotation_GetHashCode_ReflectsDifferencesInValueExpr()
        {
            Annotation ann1 = new Annotation() { ValueExpr = new Expr("1==2") };
            Annotation ann2 = new Annotation() { ValueExpr = new Expr("1>2") };
            Assert.AreNotEqual(ann1.GetHashCode(), ann2.GetHashCode());
        }

        [TestMethod]
        public void Annotation_GetHashCode_ShowsNoDifferencesForEqualValueExpr()
        {
            Annotation ann1 = new Annotation() { ValueExpr = new Expr("1==2") };
            Annotation ann2 = new Annotation() { ValueExpr = new Expr("1==2") };
            Assert.AreEqual(ann1.GetHashCode(), ann2.GetHashCode());
        }

        [TestMethod]
        public void Annotation_GetHashCode_ChangedFlagsAreIgnored()
        {
            Annotation ann1 = new Annotation() { Tag = "tag1" };
            Annotation ann2 = new Annotation() { Tag = "tag1" };

            Assert.AreEqual(ann1.GetHashCode(), ann2.GetHashCode());

            ann1.IsPriceNotPerUnit = true;
            Assert.AreEqual(ann1.GetHashCode(), ann2.GetHashCode());

            ann1.IsPriceFixated = true;
            Assert.AreEqual(ann1.GetHashCode(), ann2.GetHashCode());

            ann1.IsPriceCalculated = true;
            Assert.AreEqual(ann1.GetHashCode(), ann2.GetHashCode());

            ann1.IsDateCalculated = true;
            Assert.AreEqual(ann1.GetHashCode(), ann2.GetHashCode());

            ann1.IsTagCalculated = true;
            Assert.AreEqual(ann1.GetHashCode(), ann2.GetHashCode());

            ann1.IsValueExprCalculated = true;
            Assert.AreEqual(ann1.GetHashCode(), ann2.GetHashCode());

            ann2.IsPriceNotPerUnit = true;
            Assert.AreEqual(ann1.GetHashCode(), ann2.GetHashCode());

            ann2.IsPriceFixated = true;
            Assert.AreEqual(ann1.GetHashCode(), ann2.GetHashCode());

            ann2.IsPriceCalculated = true;
            Assert.AreEqual(ann1.GetHashCode(), ann2.GetHashCode());

            ann2.IsDateCalculated = true;
            Assert.AreEqual(ann1.GetHashCode(), ann2.GetHashCode());

            ann2.IsTagCalculated = true;
            Assert.AreEqual(ann1.GetHashCode(), ann2.GetHashCode());

            ann2.IsValueExprCalculated = true;
            Assert.AreEqual(ann1.GetHashCode(), ann2.GetHashCode());
        }

        [TestMethod]
        public void Annotation_IsNullOrEmpty_ReturnsTrueIfNull()
        {
            Assert.IsTrue(Annotation.IsNullOrEmpty(null));
        }

        [TestMethod]
        public void Annotation_IsNullOrEmpty_ReturnsTrueIfEmpty()
        {
            Assert.IsTrue(Annotation.IsNullOrEmpty(new Annotation()));
        }

        [TestMethod]
        public void Annotation_IsNullOrEmpty_ReturnsTrueIfEmptyAndIgnoresFlags()
        {
            Assert.IsTrue(Annotation.IsNullOrEmpty(new Annotation()
            {
                IsPriceNotPerUnit = true,
                IsPriceFixated = true,
                IsPriceCalculated = true,
                IsDateCalculated = true,
                IsTagCalculated = true,
                IsValueExprCalculated = true
            }));
        }

        // TODO - rework using Moq
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
