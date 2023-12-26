// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Amounts;
using NLedger.Expressions;
using NLedger.Journals;
using NLedger.Textual;
using NLedger.Values;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Journals
{
    public class JournalTests : TestFixture
    {
        [Fact]
        public void Journal_Tests_NoAliasesDisablesExpandAliases()
        {
            Journal journal = new Journal();
            Account account = new Account(null, "account");
            journal.AccountAliases.Add("A", account);

            Account account2 = journal.ExpandAliases("A");
            Assert.NotNull(account2); // ExpandAliases should return an account because NoAliases is False
            Assert.Equal(account, account2);
            
            journal.NoAliases = true;
            Account account3 = journal.ExpandAliases("A");
            Assert.Null(account3); // ExpandAliases should return Null because NoAliases is False
        }

        [Fact]
        public void Journal_Tests_ExpandAliasesReturnsNullIfMapIsEmpty()
        {
            Journal journal = new Journal();
            Account account = journal.ExpandAliases("A");
            Assert.Null(account);
        }

        [Fact]
        public void Journal_Tests_ExpandAliasesReturnsNullIfMapIsNotFound()
        {
            Journal journal = new Journal();
            Account account = new Account(null, "account");
            journal.AccountAliases.Add("A", account);

            Account account2 = journal.ExpandAliases("B");
            Assert.Null(account2);
        }

        [Fact]
        public void Journal_Tests_ExpandAliasesPerformsReqursiveSearch()
        {
            Journal journal = new Journal();

            Account account = new Account(null, "account");
            journal.AccountAliases.Add("A", account);

            Account account2 = journal.ExpandAliases("A:B");
            Assert.NotNull(account2);
            Assert.Equal("account:B", account2.FullName);
        }

        [Fact]
        public void Journal_RegisterPayee_ReturnsSentNameOfNoAlias()
        {
            Journal journal = new Journal();
            var name = journal.RegisterPayee("test-name", null);
            Assert.Equal("test-name", name);
        }

        [Fact]
        public void Journal_RegisterPayee_ReturnsAliasForName()
        {
            Journal journal = new Journal();
            journal.PayeeAliasMappings.Add(new Tuple<Mask, string>(new Mask("name1"), "alias1"));
            journal.PayeeAliasMappings.Add(new Tuple<Mask, string>(new Mask("name2"), "alias2"));
            var name = journal.RegisterPayee("name1", null);
            Assert.Equal("alias1", name);
        }

        [Fact]
        public void Journal_RegisterPayee_AddsKnownPayee()
        {
            Journal journal = new Journal();
            journal.CheckPayees = true;
            journal.CheckingStyle = JournalCheckingStyleEnum.CHECK_WARNING;

            var name = journal.RegisterPayee("name1", null);

            Assert.True(journal.KnownPayees.Contains(name));
        }

        [Fact]
        public void Journal_RegisterMetadata_ChecksTagCheckExprsMapIfValueNotEmpty()
        {
            Journal journal = new Journal();
            journal.SetCurrentContext(new ParseContext("current-path"));

            journal.TagCheckExprsMap.Add("name-1", new CheckExprPair(new Expr("1==0"), CheckExprKindEnum.EXPR_ASSERTION));
            Assert.Throws<ParseError>(() => journal.RegisterMetadata("name-1", Value.StringValue("value-1"), null));
        }

        [Fact]
        public void Journal_RegisterMetadata_DoesNotChecksTagCheckExprsMapIfValueIsEmpty()
        {
            Journal journal = new Journal();
            journal.SetCurrentContext(new ParseContext("current-path"));

            journal.TagCheckExprsMap.Add("name-1", new CheckExprPair(new Expr("1==0"), CheckExprKindEnum.EXPR_ASSERTION));
            journal.RegisterMetadata("name-1", Value.Empty, null);
        }

        [Fact]
        public void Journal_RegisterMetadata_RaisesExceptionByTagCheckExprsMapIfFalseExpression()
        {
            Journal journal = new Journal();
            journal.SetCurrentContext(new ParseContext("current-path"));

            journal.TagCheckExprsMap.Add("name-1", new CheckExprPair(new Expr("1==0"), CheckExprKindEnum.EXPR_ASSERTION));
            Assert.Throws<ParseError>(() => journal.RegisterMetadata("name-1", Value.StringValue("some-value"), null));
        }

        [Fact]
        public void Journal_RegisterMetadata_DoesNotRaiseExceptionByTagCheckExprsMapIfTrueExpression()
        {
            Journal journal = new Journal();
            journal.SetCurrentContext(new ParseContext("current-path"));

            journal.TagCheckExprsMap.Add("name-1", new CheckExprPair(new Expr("1==1"), CheckExprKindEnum.EXPR_ASSERTION));
            journal.RegisterMetadata("name-1", Value.StringValue("some-value"), null);
        }

        [Fact]
        public void Journal_AddXact_AllowsToAddDifferentUUID()
        {
            Account account1 = new Account();
            Account account2 = new Account();
            Journal journal = new Journal();

            Xact xact1 = new Xact();
            xact1.SetTag("UUID", Value.StringValue("val1"));
            xact1.AddPost(new Post() { Account = account1, Amount = new Amount(10) });
            xact1.AddPost(new Post() { Account = account2, Amount = new Amount(-10) });

            Xact xact2 = new Xact();
            xact2.SetTag("UUID", Value.StringValue("val2"));
            xact2.AddPost(new Post() { Account = account1, Amount = new Amount(10) });
            xact2.AddPost(new Post() { Account = account2, Amount = new Amount(-10) });

            journal.AddXact(xact1);
            journal.AddXact(xact2);

            Assert.Equal(xact1, journal.ChecksumMapping["val1"]);
            Assert.Equal(xact2, journal.ChecksumMapping["val2"]);
        }

        [Fact]
        public void Journal_AddXact_DoesNotAllowToAddTwoXactsWithTheSameUUIDButDifferentNumberOfPosts()
        {
            Account account = new Account();
            Journal journal = new Journal();

            Xact xact1 = new Xact();
            xact1.SetTag("UUID", Value.StringValue("val1"));
            xact1.AddPost(new Post() { Account = account, Amount = new Amount(10) });
            xact1.AddPost(new Post() { Account = account, Amount = new Amount(-10) });

            Xact xact2 = new Xact();
            xact2.SetTag("UUID", Value.StringValue("val1"));
            xact2.AddPost(new Post() { Account = account, Amount = new Amount(10) });
            xact2.AddPost(new Post() { Account = account, Amount = new Amount(-5) });
            xact2.AddPost(new Post() { Account = account, Amount = new Amount(-5) });

            journal.AddXact(xact1);
            Assert.Throws<RuntimeError>(() => journal.AddXact(xact2));
        }

        [Fact]
        public void Journal_AddXact_DoesNotAllowToAddTwoXactsWithTheSameUUIDButDifferentPostAccounts()
        {
            Account account1 = new Account();
            Account account2 = new Account();
            Account account3 = new Account();

            Journal journal = new Journal();

            Xact xact1 = new Xact();
            xact1.SetTag("UUID", Value.StringValue("val1"));
            xact1.AddPost(new Post() { Account = account1, Amount = new Amount(10) });
            xact1.AddPost(new Post() { Account = account2, Amount = new Amount(-10) });

            Xact xact2 = new Xact();
            xact2.SetTag("UUID", Value.StringValue("val1"));
            xact2.AddPost(new Post() { Account = account2, Amount = new Amount(10) });
            xact2.AddPost(new Post() { Account = account3, Amount = new Amount(-10) });

            journal.AddXact(xact1);
            Assert.Throws<RuntimeError>(() => journal.AddXact(xact2));
        }

        [Fact]
        public void Journal_AddXact_DoesNotAllowToAddTwoXactsWithTheSameUUIDButDifferentPostAmounts()
        {
            Account account1 = new Account();
            Account account2 = new Account();

            Journal journal = new Journal();

            Xact xact1 = new Xact();
            xact1.SetTag("UUID", Value.StringValue("val1"));
            xact1.AddPost(new Post() { Account = account1, Amount = new Amount(10) });
            xact1.AddPost(new Post() { Account = account2, Amount = new Amount(-10) });

            Xact xact2 = new Xact();
            xact2.SetTag("UUID", Value.StringValue("val1"));
            xact2.AddPost(new Post() { Account = account1, Amount = new Amount(5) });
            xact2.AddPost(new Post() { Account = account2, Amount = new Amount(-5) });

            journal.AddXact(xact1);
            Assert.Throws<RuntimeError>(() => journal.AddXact(xact2));
        }

        [Fact]
        public void Journal_Valid_ReturnsFalseIfMasterNotValid()
        {
            Journal journal = new Journal();
            Assert.True(journal.Valid());

            var master = new Account();
            master.Accounts.Add("wrong-self-loop", master);
            journal.Master = master;

            Assert.False(journal.Master.Valid());
            Assert.False(journal.Valid());
        }

        [Fact]
        public void Journal_Valid_ReturnsFalseIfXactNotValid()
        {
            Journal journal = new Journal();
            journal.Master = new Account();
            Assert.True(journal.Valid());

            Xact xact = new Xact();
            xact.AddPost(new Post(journal.Master, new Amount(10)));
            xact.AddPost(new Post(journal.Master, new Amount(-10)));
            journal.AddXact(xact);

            Assert.False(xact.Valid()); // [DM] - Xact is not valid (but finalizable to add to the journal) because of no date.
            Assert.False(journal.Valid());
        }

    }
}
