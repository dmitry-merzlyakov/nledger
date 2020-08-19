// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
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
using Xunit;

namespace NLedger.Tests.Items
{
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon | ContextInit.SaveCultureInfo)]
    public class ItemTests : TestFixture
    {
        [Fact]
        public void Item_AddFlags_Adds_New_Flag()
        {
            TestItem item = new TestItem();
            Assert.Equal(SupportsFlagsEnum.ITEM_NORMAL, item.Flags);

            item.Flags = item.Flags | SupportsFlagsEnum.ITEM_GENERATED;
            Assert.Equal(SupportsFlagsEnum.ITEM_GENERATED, item.Flags);

            item.Flags = item.Flags | SupportsFlagsEnum.ITEM_TEMP;
            Assert.Equal(SupportsFlagsEnum.ITEM_GENERATED | SupportsFlagsEnum.ITEM_TEMP, item.Flags);

            item.Flags = item.Flags | SupportsFlagsEnum.ITEM_NOTE_ON_NEXT_LINE;
            Assert.Equal(SupportsFlagsEnum.ITEM_GENERATED | SupportsFlagsEnum.ITEM_TEMP | SupportsFlagsEnum.ITEM_NOTE_ON_NEXT_LINE, item.Flags);
        }

        [Fact]
        public void Item_HasTag_Checks_Whether_TheTag_Exists()
        {
            TestItem item = new TestItem();
            Assert.False(item.HasTag("mytag"));

            item.SetTag("mytag");
            Assert.True(item.HasTag("mytag"));
            Assert.False(item.HasTag("notmytag"));
        }

        [Fact]
        public void Item_HasTag_Checks_Whether_TheTag_Exists_By_Template()
        {
            TestItem item = new TestItem();
            item.SetTag("mytag");
            item.SetTag("histag");
            item.SetTag("hertag");
            item.SetTag("dummy");

            Assert.True(item.HasTag(new Mask("my")));
            Assert.False(item.HasTag(new Mask("none")));
            Assert.True(item.HasTag(new Mask("tag")));
            Assert.True(item.HasTag(new Mask("dummy")));
        }

        [Fact]
        public void Item_HasTag_Checks_Whether_TheTag_Exists_By_TagAndValueTemplate()
        {
            TestItem item = new TestItem();
            item.SetTag("mytag", Value.Get("myvalue"));
            item.SetTag("histag", Value.Get("hisvalue"));
            item.SetTag("hertag", Value.Get("hervalue"));
            item.SetTag("dummy", Value.Get("none"));

            Assert.True(item.HasTag(new Mask("my"), new Mask("value")));
            Assert.False(item.HasTag(new Mask("my"), new Mask("notexpected")));
            Assert.False(item.HasTag(new Mask("none"), new Mask("dummy")));
            Assert.True(item.HasTag(new Mask("dummy"), new Mask("none")));
        }

        [Fact]
        public void Item_GetTag_Returns_Value_By_Tag()
        {
            TestItem item = new TestItem();
            item.SetTag("mytag", Value.Get("myvalue"));
            item.SetTag("histag", Value.Get("hisvalue"));
            item.SetTag("hertag", Value.Get("hervalue"));
            item.SetTag("dummy", Value.Get("none"));

            Assert.True(Value.IsNullOrEmpty(item.GetTag("unknowntag")));
            Assert.Equal("myvalue", item.GetTag("mytag").ToString());
        }

        [Fact]
        public void Item_GetTag_Returns_Value_By_TagTemplate_InAlphabeticalOrder()
        {
            TestItem item = new TestItem();
            item.SetTag("mytag", Value.Get("myvalue"));
            item.SetTag("histag", Value.Get("hisvalue"));
            item.SetTag("hertag", Value.Get("hervalue"));
            item.SetTag("dummy", Value.Get("none"));

            Assert.True(Value.IsNullOrEmpty(item.GetTag(new Mask("unknowntag"))));
            Assert.Equal("hervalue", item.GetTag(new Mask("tag")).ToString()); // the order is - dummy, hertag, histag, mytag
            Assert.Equal("hisvalue", item.GetTag(new Mask("histag")).ToString());
        }

        [Fact]
        public void Item_GetTag_Returns_Value_By_TagAndValueTemplate()
        {
            TestItem item = new TestItem();
            item.SetTag("mytag", Value.Get("myvalue"));
            item.SetTag("histag", Value.Get("hisvalue"));
            item.SetTag("hertag", Value.Get("hervalue"));
            item.SetTag("dummy", Value.Get("none"));

            Assert.True(Value.IsNullOrEmpty(item.GetTag(new Mask("unknowntag"))));
            Assert.Equal("hisvalue", item.GetTag(new Mask("tag"), new Mask("his")).ToString());
        }

        [Fact]
        public void Item_SetTag_Adds_The_Tag_With_Empty_Value()
        {
            TestItem item = new TestItem();
            item.SetTag("mytag");
            Assert.True(item.HasTag("mytag"));
            Assert.True(Value.IsNullOrEmpty(item.GetTag("mytag")));
        }

        [Fact]
        public void Item_SetTag_Adds_The_Tag_With_Value()
        {
            TestItem item = new TestItem();
            item.SetTag("mytag", Value.Get("value"));
            Assert.Equal("value", item.GetTag("mytag").ToString());
        }

        [Fact]
        public void Item_SetTag_Can_Override_Value()
        {
            TestItem item = new TestItem();

            item.SetTag("mytag", Value.Get("value1"));
            Assert.Equal("value1", item.GetTag("mytag").ToString());

            item.SetTag("mytag", Value.Get("value2"));
            Assert.Equal("value2", item.GetTag("mytag").ToString());

            item.SetTag("mytag", Value.Get("value3"), true);
            Assert.Equal("value3", item.GetTag("mytag").ToString());
        }

        [Fact]
        public void Item_SetTag_May_Dont_Override_Value()
        {
            TestItem item = new TestItem();

            item.SetTag("mytag", Value.Get("value1"));
            Assert.Equal("value1", item.GetTag("mytag").ToString());

            item.SetTag("mytag", Value.Get("value2"), false);
            Assert.Equal("value1", item.GetTag("mytag").ToString());
        }

        [Fact]
        public void Item_ParseTags_Ignores_Empty_Note()
        {
            TestItem item = new TestItem();
            item.ParseTags(null, null);
            Assert.True(Value.IsNullOrEmpty(item.GetTag(new Mask(".*"))));
        }

        [Fact]
        public void Item_ParseTags_Populates_TheDate()
        {
            Date date = new Date(2010, 10, 5);

            TestItem item = new TestItem();
            item.ParseTags(String.Format("some text [{0:yyyy/MM/dd}] rest of text", date), null);
            Assert.Equal(date, item.Date);
            Assert.Null(item.DateAux);
        }

        [Fact]
        public void Item_ParseTags_Populates_BothDates()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

            Date date = new Date(2010, 10, 5);
            Date dateAux = new Date(2012, 2, 17);

            TestItem item = new TestItem();
            item.ParseTags(String.Format(CultureInfo.InvariantCulture, "some text [{0:yyyy/MM/dd}={1:yyyy/MM/dd}] rest of text", date, dateAux), null);
            Assert.Equal(date, item.Date.Value);
            Assert.Equal(dateAux, item.DateAux.Value);
        }

        [Fact]
        public void Item_ParseTags_Populates_SeriesOfTags()
        {
            TestItem item = new TestItem();
            item.ParseTags("some text :tag1:tag2:tag3: end of the text", null);
            Assert.True(item.HasTag("tag1"));
            Assert.True(item.HasTag("tag2"));
            Assert.True(item.HasTag("tag3"));
        }

        [Fact]
        public void Item_ParseTags_Populates_TagsWithValues()
        {
            TestItem item = new TestItem();
            item.ParseTags(" MyTag: 2012/02/02", null);
            Assert.Equal("2012/02/02", item.GetTag("MyTag").ToString());
        }

        [Fact]
        public void Item_Id_ReturnsUUIDTagsIfExists()
        {
            TestItem item = new TestItem();
            item.SetTag("UUID", Value.StringValue("uuid-val"));
            Assert.Equal("uuid-val", item.Id);
        }

        [Fact]
        public void Item_Id_ReturnsSeqIfNoUUIDTags()
        {
            TestItem item = new TestItem();
            item.Pos = new ItemPosition() { Sequence = 99 };
            Assert.Equal("99", item.Id);
        }

        [Fact]
        public void Item_Id_Returns0IfNoSeqAndUUIDTags()
        {
            TestItem item = new TestItem();
            Assert.Equal("0", item.Id);
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
