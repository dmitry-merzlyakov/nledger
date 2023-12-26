// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Output;
using NLedger.Scopus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Output
{
    public class FormatAccountsTests : TestFixture
    {
        [Fact]
        public void FormatAccounts_MarkAccounts_IgnoresRootAccount()
        {
            Account account = new Account(null, "root-account-does-not-have-a-parent");
            FormatAccounts formatAccounts = CreateFormatAccounts();

            var result = formatAccounts.MarkAccounts(account, false); // "flat"=false
            Assert.Equal(0, result.Item1);
            Assert.Equal(0, result.Item2);

            result = formatAccounts.MarkAccounts(account, true);  // "flat"=true
            Assert.Equal(0, result.Item1);
            Assert.Equal(0, result.Item2);
        }

        [Fact]
        public void FormatAccounts_MarkAccounts_ChildAccountShouldBeVisitedToBeIncluded()
        {
            Account account = new Account();
            Account childAccount = new Account(account, "child");
            account.AddAccount(childAccount);
            FormatAccounts formatAccounts = CreateFormatAccounts();

            var result = formatAccounts.MarkAccounts(account, false);
            Assert.Equal(0, result.Item1);

            childAccount.XData.Visited = true;
            result = formatAccounts.MarkAccounts(account, false);
            Assert.Equal(1, result.Item1);
        }

        [Fact]
        public void FormatAccounts_MarkAccounts_ChildAccountShouldHaveVisitedChildrenToBeIncluded()
        {
            Account account = new Account();
            Account childAccount = new Account(account, "child");
            account.AddAccount(childAccount);
            Account grandChildAccount = new Account(childAccount, "grand-child");
            childAccount.AddAccount(grandChildAccount);
            FormatAccounts formatAccounts = CreateFormatAccounts();

            var result = formatAccounts.MarkAccounts(account, false);
            Assert.Equal(0, result.Item1);
            result = formatAccounts.MarkAccounts(account, true);
            Assert.Equal(0, result.Item1);

            // once grandchild is marked as visited, it is counted by child account and the child is included - but only for NOT FLAT mode
            childAccount.XData.Visited = false;
            grandChildAccount.XData.Visited = true; 
            result = formatAccounts.MarkAccounts(account, false);  // flat = false
            Assert.Equal(1, result.Item1);  // Last visited is grandchild

            childAccount.XData.Visited = false;
            grandChildAccount.XData.Visited = true;             
            result = formatAccounts.MarkAccounts(account, true);  // flat = true
            Assert.Equal(1, result.Item1);   // last visited is child
        }

        [Fact]
        public void FormatAccounts_PostAccount_PrintsToReportOutputSteam()
        {
            Account account = new Account(null, "root-account");
            account.XData.ToDisplay = true;

            Report report = new Report(new Session());
            StringWriter output = new StringWriter();
            report.OutputStream = output;

            FormatAccounts formatAccounts = new FormatAccounts(report, "some-format-string");
            int result = formatAccounts.PostAccount(account, false);
            string outString = output.ToString();

            Assert.Equal(1, result);
            Assert.Equal("some-format-string", outString);
        }

        private FormatAccounts CreateFormatAccounts()
        {
            return new FormatAccounts(new Report(new Session()), String.Empty);
        }
    }
}
