// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Expressions;
using NLedger.Items;
using NLedger.Scopus;
using NLedger.Times;
using NLedger.Values;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NLedger.Tests.Items
{
    [TestClass]
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon | ContextInit.SaveCultureInfo)]
    public class ItemTests : TestFixture
    {
        [TestMethod]
        public void Item_AddFlags_Adds_New_Flag()
        {
            TestItem item = new TestItem();
            Assert.AreEqual(SupportsFlagsEnum.ITEM_NORMAL, item.Flags);

            item.Flags = item.Flags | SupportsFlagsEnum.ITEM_GENERATED;
            Assert.AreEqual(SupportsFlagsEnum.ITEM_GENERATED, item.Flags);

            item.Flags = item.Flags | SupportsFlagsEnum.ITEM_TEMP;
            Assert.AreEqual(SupportsFlagsEnum.ITEM_GENERATED | SupportsFlagsEnum.ITEM_TEMP, item.Flags);

            item.Flags = item.Flags | SupportsFlagsEnum.ITEM_NOTE_ON_NEXT_LINE;
            Assert.AreEqual(SupportsFlagsEnum.ITEM_GENERATED | SupportsFlagsEnum.ITEM_TEMP | SupportsFlagsEnum.ITEM_NOTE_ON_NEXT_LINE, item.Flags);
        }

        [TestMethod]
        public void Item_HasTag_Checks_Whether_TheTag_Exists()
        {
            TestItem item = new TestItem();
            Assert.IsFalse(item.HasTag("mytag"));

            item.SetTag("mytag");
            Assert.IsTrue(item.HasTag("mytag"));
            Assert.IsFalse(item.HasTag("notmytag"));
        }

        [TestMethod]
        public void Item_HasTag_Checks_Whether_TheTag_Exists_By_Template()
        {
            TestItem item = new TestItem();
            item.SetTag("mytag");
            item.SetTag("histag");
            item.SetTag("hertag");
            item.SetTag("dummy");

            Assert.IsTrue(item.HasTag(new Mask("my")));
            Assert.IsFalse(item.HasTag(new Mask("none")));
            Assert.IsTrue(item.HasTag(new Mask("tag")));
            Assert.IsTrue(item.HasTag(new Mask("dummy")));
        }

        [TestMethod]
        public void Item_HasTag_Checks_Whether_TheTag_Exists_By_TagAndValueTemplate()
        {
            TestItem item = new TestItem();
            item.SetTag("mytag", Value.Get("myvalue"));
            item.SetTag("histag", Value.Get("hisvalue"));
            item.SetTag("hertag", Value.Get("hervalue"));
            item.SetTag("dummy", Value.Get("none"));

            Assert.IsTrue(item.HasTag(new Mask("my"), new Mask("value")));
            Assert.IsFalse(item.HasTag(new Mask("my"), new Mask("notexpected")));
            Assert.IsFalse(item.HasTag(new Mask("none"), new Mask("dummy")));
            Assert.IsTrue(item.HasTag(new Mask("dummy"), new Mask("none")));
        }

        [TestMethod]
        public void Item_GetTag_Returns_Value_By_Tag()
        {
            TestItem item = new TestItem();
            item.SetTag("mytag", Value.Get("myvalue"));
            item.SetTag("histag", Value.Get("hisvalue"));
            item.SetTag("hertag", Value.Get("hervalue"));
            item.SetTag("dummy", Value.Get("none"));

            Assert.IsTrue(Value.IsNullOrEmpty(item.GetTag("unknowntag")));
            Assert.AreEqual("myvalue", item.GetTag("mytag").ToString());
        }

        [TestMethod]
        public void Item_GetTag_Returns_Value_By_TagTemplate_InAlphabeticalOrder()
        {
            TestItem item = new TestItem();
            item.SetTag("mytag", Value.Get("myvalue"));
            item.SetTag("histag", Value.Get("hisvalue"));
            item.SetTag("hertag", Value.Get("hervalue"));
            item.SetTag("dummy", Value.Get("none"));

            Assert.IsTrue(Value.IsNullOrEmpty(item.GetTag(new Mask("unknowntag"))));
            Assert.AreEqual("hervalue", item.GetTag(new Mask("tag")).ToString()); // the order is - dummy, hertag, histag, mytag
            Assert.AreEqual("hisvalue", item.GetTag(new Mask("histag")).ToString());
        }

        [TestMethod]
        public void Item_GetTag_Returns_Value_By_TagAndValueTemplate()
        {
            TestItem item = new TestItem();
            item.SetTag("mytag", Value.Get("myvalue"));
            item.SetTag("histag", Value.Get("hisvalue"));
            item.SetTag("hertag", Value.Get("hervalue"));
            item.SetTag("dummy", Value.Get("none"));

            Assert.IsTrue(Value.IsNullOrEmpty(item.GetTag(new Mask("unknowntag"))));
            Assert.AreEqual("hisvalue", item.GetTag(new Mask("tag"), new Mask("his")).ToString());
        }

        [TestMethod]
        public void Item_SetTag_Adds_The_Tag_With_Empty_Value()
        {
            TestItem item = new TestItem();
            item.SetTag("mytag");
            Assert.IsTrue(item.HasTag("mytag"));
            Assert.IsTrue(Value.IsNullOrEmpty(item.GetTag("mytag")));
        }

        [TestMethod]
        public void Item_SetTag_Adds_The_Tag_With_Value()
        {
            TestItem item = new TestItem();
            item.SetTag("mytag", Value.Get("value"));
            Assert.AreEqual("value", item.GetTag("mytag").ToString());
        }

        [TestMethod]
        public void Item_SetTag_Can_Override_Value()
        {
            TestItem item = new TestItem();

            item.SetTag("mytag", Value.Get("value1"));
            Assert.AreEqual("value1", item.GetTag("mytag").ToString());

            item.SetTag("mytag", Value.Get("value2"));
            Assert.AreEqual("value2", item.GetTag("mytag").ToString());

            item.SetTag("mytag", Value.Get("value3"), true);
            Assert.AreEqual("value3", item.GetTag("mytag").ToString());
        }

        [TestMethod]
        public void Item_SetTag_May_Dont_Override_Value()
        {
            TestItem item = new TestItem();

            item.SetTag("mytag", Value.Get("value1"));
            Assert.AreEqual("value1", item.GetTag("mytag").ToString());

            item.SetTag("mytag", Value.Get("value2"), false);
            Assert.AreEqual("value1", item.GetTag("mytag").ToString());
        }

        [TestMethod]
        public void Item_ParseTags_Ignores_Empty_Note()
        {
            TestItem item = new TestItem();
            item.ParseTags(null, null);
            Assert.IsTrue(Value.IsNullOrEmpty(item.GetTag(new Mask(".*"))));
        }

        [TestMethod]
        public void Item_ParseTags_Populates_TheDate()
        {
            Date date = new Date(2010, 10, 5);

            TestItem item = new TestItem();
            item.ParseTags(String.Format("some text [{0:yyyy/MM/dd}] rest of text", date), null);
            Assert.AreEqual(date, item.Date);
            Assert.IsNull(item.DateAux);
        }

        [TestMethod]
        public void Item_ParseTags_Populates_BothDates()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

            Date date = new Date(2010, 10, 5);
            Date dateAux = new Date(2012, 2, 17);

            TestItem item = new TestItem();
            item.ParseTags(String.Format(CultureInfo.InvariantCulture, "some text [{0:yyyy/MM/dd}={1:yyyy/MM/dd}] rest of text", date, dateAux), null);
            Assert.AreEqual(date, item.Date.Value);
            Assert.AreEqual(dateAux, item.DateAux.Value);
        }

        [TestMethod]
        public void Item_ParseTags_Populates_SeriesOfTags()
        {
            TestItem item = new TestItem();
            item.ParseTags("some text :tag1:tag2:tag3: end of the text", null);
            Assert.IsTrue(item.HasTag("tag1"));
            Assert.IsTrue(item.HasTag("tag2"));
            Assert.IsTrue(item.HasTag("tag3"));
        }

        [TestMethod]
        public void Item_ParseTags_Populates_TagsWithValues()
        {
            TestItem item = new TestItem();
            item.ParseTags(" MyTag: 2012/02/02", null);
            Assert.AreEqual("2012/02/02", item.GetTag("MyTag").ToString());
        }

        [TestMethod]
        public void Item_Id_ReturnsUUIDTagsIfExists()
        {
            TestItem item = new TestItem();
            item.SetTag("UUID", Value.StringValue("uuid-val"));
            Assert.AreEqual("uuid-val", item.Id);
        }

        [TestMethod]
        public void Item_Id_ReturnsSeqIfNoUUIDTags()
        {
            TestItem item = new TestItem();
            item.Pos = new ItemPosition() { Sequence = 99 };
            Assert.AreEqual("99", item.Id);
        }

        [TestMethod]
        public void Item_Id_Returns0IfNoSeqAndUUIDTags()
        {
            TestItem item = new TestItem();
            Assert.AreEqual("0", item.Id);
        }

    }

    public class TestItem : Item
    {
        public override string Description
        {
            get { throw new NotImplementedException(); }
        }

        public override ExprOp Lookup(SymbolKindEnum kind, string name)
        {
            throw new NotImplementedException();
        }
    }
}
