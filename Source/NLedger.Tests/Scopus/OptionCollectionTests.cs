// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Scopus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Scopus
{
    public class OptionCollectionTests : TestFixture
    {

        [Fact]
        public void OptionCollection_IsEq_ProperlyComparesStrings()
        {
            Assert.False(OptionCollection.IsEq("payee_width", "payee"));
            Assert.True(OptionCollection.IsEq("date_width", "date_width_"));
        }

        [Fact]
        public void OptionCollection_Add_AddsOptionToItems()
        {
            OptionCollection optionCollection = new OptionCollection();
            Assert.Empty(optionCollection.Options);

            Option option = new Option("name");
            optionCollection.Add(option);
            Assert.Single(optionCollection.Options);
            Assert.Equal(option, optionCollection.Options.First());
        }

        [Fact]
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
            Assert.Equal(option1, result11);
            Assert.Equal(option1, result12);

            Option result21 = optionCollection.LookupOption("name_2");
            Option result22 = optionCollection.LookupOption("name-2");
            Assert.Equal(option2, result21);
            Assert.Equal(option2, result22);
        }

        [Fact]
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
            Assert.Equal(option1, result11);
            Assert.Equal(option1, result12);
            Assert.Equal(option1, result13);

            Option result21 = optionCollection.LookupOption("name_2");
            Option result22 = optionCollection.LookupOption("name-2");
            Option result23 = optionCollection.LookupOption("w");
            Assert.Equal(option2, result21);
            Assert.Equal(option2, result22);
            Assert.Equal(option2, result23);
        }

        [Fact]
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
            Assert.Equal(option1, result11);
            Assert.Equal(option1, result12);
            Assert.Equal(option1, result13);

            Option result21 = optionCollection.LookupOption("name_2");
            Option result22 = optionCollection.LookupOption("name-2");
            Option result23 = optionCollection.LookupOption("w");
            Option result24 = optionCollection.LookupOption("w_");
            Assert.Equal(option2, result21);
            Assert.Equal(option2, result22);
            Assert.Equal(option2, result23);
            Assert.Equal(option2, result24);
        }

        [Fact]
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
            Assert.Equal(option1, result11);

            Option result23 = optionCollection.LookupOption("w");
            Option result24 = optionCollection.LookupOption("w_");
            Assert.Equal(option2, result23);
            Assert.Equal(option2, result24);
        }

        [Fact]
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
            Assert.Equal(report.Replace("\r\n", "\n"), ReportResult.Replace("\r\n", "\n"));
        }

        [Fact]
        public void OptionCollection_LookupOption_ReturnsNullForEmptytString()
        {
            OptionCollection optionCollection = new OptionCollection();
            Assert.Null(optionCollection.LookupOption(null));
            Assert.Null(optionCollection.LookupOption(String.Empty));
        }

        [Fact]
        public void OptionCollection_LookupOption_ReplacesMinusToUnderscoreBeforeSearching()
        {
            Option option1 = new Option("name_1");
            option1.On("whence");
            OptionCollection optionCollection = new OptionCollection();
            optionCollection.Add(option1);
            optionCollection.AddLookupOpt("name_1");
            Option result = optionCollection.LookupOption("name-1"); // search with minus
            Assert.Equal(option1, result);
        }

        [Fact]
        public void OptionCollection_LookupOption_SetsParentForFoundItem()
        {
            object parent = new object();
            Option option1 = new Option("name_1");
            option1.On("whence");
            OptionCollection optionCollection = new OptionCollection();
            optionCollection.Add(option1);
            optionCollection.AddLookupOpt("name_1");
            Option result = optionCollection.LookupOption("name_1", parent);  // send parent
            Assert.Equal(option1, result);
            Assert.Equal(option1.Parent, parent);
        }

        [Fact]
        public void OptionCollection_LookupOption_ReturnsNullIfOptionisNotFound()
        {
            object parent = new object();
            Option option1 = new Option("name_1");
            option1.On("whence");
            OptionCollection optionCollection = new OptionCollection();
            optionCollection.Add(option1);
            optionCollection.AddLookupOpt("name_1");
            Option result = optionCollection.LookupOption("dummy");  // unknown name
            Assert.Null(result);
        }

        private const string ReportResult =
@"                  name-1                                             src1
                  name-2 = val                                       src2
";

    }
}
