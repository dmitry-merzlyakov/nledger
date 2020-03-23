// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Scopus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Scopus
{
    [TestClass]
    public class OptionCollectionTests : TestFixture
    {

        [TestMethod]
        public void OptionCollection_IsEq_ProperlyComparesStrings()
        {
            Assert.IsFalse(OptionCollection.IsEq("payee_width", "payee"));
            Assert.IsTrue(OptionCollection.IsEq("date_width", "date_width_"));
        }

        [TestMethod]
        public void OptionCollection_Add_AddsOptionToItems()
        {
            OptionCollection optionCollection = new OptionCollection();
            Assert.AreEqual(0, optionCollection.Options.Count());

            Option option = new Option("name");
            optionCollection.Add(option);
            Assert.AreEqual(1, optionCollection.Options.Count());
            Assert.AreEqual(option, optionCollection.Options.First());
        }

        [TestMethod]
        public void OptionCollection_AddLookupOpt_AddsOptionHandlerWithOptComparer()  // OPT
        {
            Option option1 = new Option("name_1");
            Option option2 = new Option("name_2_");

            OptionCollection optionCollection = new OptionCollection();
            optionCollection.Add(option1);
            optionCollection.Add(option2);

            optionCollection.AddLookupOpt("name_1");
            optionCollection.AddLookupOpt("name_2_");

            Option result11 = optionCollection.LookupOption("name_1");
            Option result12 = optionCollection.LookupOption("name-1");
            Assert.AreEqual(option1, result11);
            Assert.AreEqual(option1, result12);

            Option result21 = optionCollection.LookupOption("name_2");
            Option result22 = optionCollection.LookupOption("name-2");
            Assert.AreEqual(option2, result21);
            Assert.AreEqual(option2, result22);
        }

        [TestMethod]
        public void OptionCollection_AddLookupOptAlt_AddsOptionHandlerWithOptAltComparer()  // OPT_ALT
        {
            Option option1 = new Option("name_1");
            Option option2 = new Option("name_2_");

            OptionCollection optionCollection = new OptionCollection();
            optionCollection.Add(option1);
            optionCollection.Add(option2);

            optionCollection.AddLookupOptAlt("name_1", "qwe_");
            optionCollection.AddLookupOptAlt("name_2_", "w");

            Option result11 = optionCollection.LookupOption("name_1");
            Option result12 = optionCollection.LookupOption("name-1");
            Option result13 = optionCollection.LookupOption("qwe");
            Assert.AreEqual(option1, result11);
            Assert.AreEqual(option1, result12);
            Assert.AreEqual(option1, result13);

            Option result21 = optionCollection.LookupOption("name_2");
            Option result22 = optionCollection.LookupOption("name-2");
            Option result23 = optionCollection.LookupOption("w");
            Assert.AreEqual(option2, result21);
            Assert.AreEqual(option2, result22);
            Assert.AreEqual(option2, result23);
        }

        [TestMethod]
        public void OptionCollection_AddLookupOptArgs_AddsOptionHandlerWithOptArgsComparer()  // OPT_
        {
            Option option1 = new Option("name_1");    // WantsArgs = no
            Option option2 = new Option("name_2_");   // WantsArgs = yes

            OptionCollection optionCollection = new OptionCollection();
            optionCollection.Add(option1);
            optionCollection.Add(option2);

            optionCollection.AddLookupOptArgs("name_1",  "q");
            optionCollection.AddLookupOptArgs("name_2_", "w");

            Option result11 = optionCollection.LookupOption("name_1");
            Option result12 = optionCollection.LookupOption("name-1");
            Option result13 = optionCollection.LookupOption("q");
            Assert.AreEqual(option1, result11);
            Assert.AreEqual(option1, result12);
            Assert.AreEqual(option1, result13);

            Option result21 = optionCollection.LookupOption("name_2");
            Option result22 = optionCollection.LookupOption("name-2");
            Option result23 = optionCollection.LookupOption("w");
            Option result24 = optionCollection.LookupOption("w_");
            Assert.AreEqual(option2, result21);
            Assert.AreEqual(option2, result22);
            Assert.AreEqual(option2, result23);
            Assert.AreEqual(option2, result24);
        }

        [TestMethod]
        public void OptionCollection_AddLookupArgs_AddsOptionHandlerWithArgsComparer()  // OPT_CH
        {
            Option option1 = new Option("name_1");    // WantsArgs = no
            Option option2 = new Option("name_2_");   // WantsArgs = yes

            OptionCollection optionCollection = new OptionCollection();
            optionCollection.Add(option1);
            optionCollection.Add(option2);

            optionCollection.AddLookupArgs("name_1", "q");
            optionCollection.AddLookupArgs("name_2_", "w");

            Option result11 = optionCollection.LookupOption("q");
            Assert.AreEqual(option1, result11);

            Option result23 = optionCollection.LookupOption("w");
            Option result24 = optionCollection.LookupOption("w_");
            Assert.AreEqual(option2, result23);
            Assert.AreEqual(option2, result24);
        }

        [TestMethod]
        public void OptionCollection_Report_ReturnsCompositeReportText()
        {
            Option option1 = new Option("name_1");    // WantsArgs = no
            Option option2 = new Option("name_2_") { Value = "val" };   // WantsArgs = yes

            option1.On("src1");
            option2.On("src2");

            OptionCollection optionCollection = new OptionCollection();
            optionCollection.Add(option1);
            optionCollection.Add(option2);

            string report = optionCollection.Report();
            Assert.AreEqual(report.Replace("\r\n", "\n"), ReportResult.Replace("\r\n", "\n"));
        }

        [TestMethod]
        public void OptionCollection_LookupOption_ReturnsNullForEmptytString()
        {
            OptionCollection optionCollection = new OptionCollection();
            Assert.IsNull(optionCollection.LookupOption(null));
            Assert.IsNull(optionCollection.LookupOption(String.Empty));
        }

        [TestMethod]
        public void OptionCollection_LookupOption_ReplacesMinusToUnderscoreBeforeSearching()
        {
            Option option1 = new Option("name_1");
            option1.On("whence");
            OptionCollection optionCollection = new OptionCollection();
            optionCollection.Add(option1);
            optionCollection.AddLookupOpt("name_1");
            Option result = optionCollection.LookupOption("name-1"); // search with minus
            Assert.AreEqual(option1, result);
        }

        [TestMethod]
        public void OptionCollection_LookupOption_SetsParentForFoundItem()
        {
            object parent = new object();
            Option option1 = new Option("name_1");
            option1.On("whence");
            OptionCollection optionCollection = new OptionCollection();
            optionCollection.Add(option1);
            optionCollection.AddLookupOpt("name_1");
            Option result = optionCollection.LookupOption("name_1", parent);  // send parent
            Assert.AreEqual(option1, result);
            Assert.AreEqual(option1.Parent, parent);
        }

        [TestMethod]
        public void OptionCollection_LookupOption_ReturnsNullIfOptionisNotFound()
        {
            object parent = new object();
            Option option1 = new Option("name_1");
            option1.On("whence");
            OptionCollection optionCollection = new OptionCollection();
            optionCollection.Add(option1);
            optionCollection.AddLookupOpt("name_1");
            Option result = optionCollection.LookupOption("dummy");  // unknown name
            Assert.IsNull(result);
        }

        private const string ReportResult =
@"                  name-1                                             src1
                  name-2 = val                                       src2
";

    }
}
