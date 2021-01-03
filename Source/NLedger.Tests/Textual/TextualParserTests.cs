// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Amounts;
using NLedger.Annotate;
using NLedger.Items;
using NLedger.Textual;
using NLedger.Times;
using NLedger.Xacts;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Textual
{
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon)]
    public class TextualParserTests : TestFixture
    {
        private TextualParser CreateTextualParser()
        {
            ParseContextStack parseContextStack = new ParseContextStack();
            ParseContext parseContext = new ParseContext("path") { Journal = new NLedger.Journals.Journal() };
            parseContextStack.Push(parseContext);
            return new TextualParser(parseContextStack, parseContext);
        }

        [Fact]
        public void TextualParser_ParsePost_ExpensesFoodGroceriesIntegrationTest()
        {
            string line = "  Expenses:Food:Groceries             $ 37.50  ; [=2011/03/01]";
            TextualParser parser = CreateTextualParser();

            Account account = new Account();
            Xact xact = new Xact();
            Post post = parser.ParsePost(line, xact, account);

            Assert.NotNull(post);
            Assert.Equal("Expenses:Food:Groceries", post.Account.FullName);
            Assert.Equal("$", post.Amount.Commodity.Symbol);
            Assert.Equal(Quantity.Parse("37.50", 2), post.Amount.Quantity);
            Assert.Equal(new Date(2011, 03, 01), post.DateAux);
        }

        [Fact]
        public void TextualParser_ParsePost_BrokerageIntegrationTest()
        {
            string line = "  Assets:Brokerage                                 50 AAPL @ $30.00";
            TextualParser parser = CreateTextualParser();

            Account account = new Account();
            Xact xact = new Xact();
            Post post = parser.ParsePost(line, xact, account);

            Assert.NotNull(post);
            Assert.Equal("Assets:Brokerage", post.Account.FullName);
            Assert.Equal("AAPL", post.Amount.Commodity.Symbol);
            Assert.Equal(Quantity.Parse("50"), post.Amount.Quantity);
            Assert.Equal("$", post.Cost.Commodity.Symbol);
            Assert.Equal(Quantity.Parse("1500", 2), post.Cost.Quantity);
            Assert.Equal(post.Cost, post.GivenCost);
        }

        [Fact]
        public void TextualParser_ParsePost_AssetsWyshonaItemsIntegrationTest()
        {
            string line = "  Assets:Wyshona:Items                \"Plans: Wildthorn Mail\" 1 {1.25G}";
            TextualParser parser = CreateTextualParser();

            Account account = new Account();
            Xact xact = new Xact();
            Post post = parser.ParsePost(line, xact, account);

            Assert.NotNull(post);
            Assert.Equal("Assets:Wyshona:Items", post.Account.FullName);
            Assert.Equal("\"Plans: Wildthorn Mail\"", post.Amount.Commodity.Symbol);
            Assert.Equal(Quantity.Parse("1"), post.Amount.Quantity);
            Assert.Equal("G", ((AnnotatedCommodity)post.Amount.Commodity).Details.Price.Commodity.Symbol);
            Assert.Equal(Quantity.Parse("1.25", 2), ((AnnotatedCommodity)post.Amount.Commodity).Details.Price.Quantity);
        }

        [Fact]
        public void TextualParser_ParseXact_CheckingBalanceIntegrationTest()
        {
            string line = 
@"2003/12/01 * Checking balance
  Assets:Checking                   $1,000.00
  Equity:Opening Balances";
            ITextualReader reader = CreateReaderForString(line);
            TextualParser parser = CreateTextualParser();

            Account account = new Account();
            string current = reader.ReadLine();
            Xact xact = parser.ParseXact(current, reader, account);

            Assert.NotNull(xact);
            Assert.Equal(new Date(2003,12,01), xact.Date);
            Assert.Null(xact.DateAux);
            Assert.Equal(ItemStateEnum.Cleared, xact.State);
            Assert.Equal("Checking balance", xact.Payee);
            Assert.Equal(2, xact.Posts.Count);

            Post post1 = xact.Posts.First();
            Assert.Equal("Assets:Checking", post1.Account.FullName);
            Assert.Equal("$", post1.Amount.Commodity.Symbol);
            Assert.Equal(Quantity.Parse("1000", 2), post1.Amount.Quantity);

            Post post2 = xact.Posts.Last();
            Assert.Equal("Equity:Opening Balances", post2.Account.FullName);
            Assert.Null(post2.Amount);
        }

        [Fact]
        public void TextualParser_ParseXact_BankIntegrationTest()
        {
            string line =
@"2011/01/25=2011/01/28 Bank
  ; Transfer to cover car purchase
  Assets:Checking                  $ 5,500.00
  Assets:Savings
  ; :nobudget:";
            ITextualReader reader = CreateReaderForString(line);
            TextualParser parser = CreateTextualParser();

            Account account = new Account();
            string current = reader.ReadLine();
            Xact xact = parser.ParseXact(current, reader, account);

            Assert.NotNull(xact);
            Assert.Equal(new Date(2011, 01, 25), xact.Date);
            Assert.Equal(new Date(2011, 01, 28), xact.DateAux);
            Assert.Equal(ItemStateEnum.Uncleared, xact.State);
            Assert.Equal("Bank", xact.Payee);
            Assert.Equal(" Transfer to cover car purchase", xact.Note); // Notice the trailing space.
            Assert.True(xact.Flags.HasFlag(SupportsFlagsEnum.ITEM_NOTE_ON_NEXT_LINE));
            Assert.Equal(2, xact.Posts.Count);

            Post post1 = xact.Posts.First();
            Assert.Equal("Assets:Checking", post1.Account.FullName);
            Assert.Equal("$", post1.Amount.Commodity.Symbol);
            Assert.Equal(Quantity.Parse("5500", 2), post1.Amount.Quantity);

            Post post2 = xact.Posts.Last();
            Assert.Equal("Assets:Savings", post2.Account.FullName);
            Assert.Equal(" :nobudget:", post2.Note);
            Assert.True(post2.Flags.HasFlag(SupportsFlagsEnum.ITEM_NOTE_ON_NEXT_LINE));
            Assert.True(post2.HasTag("nobudget"));
            Assert.Null(post2.Amount);
        }

        private ITextualReader CreateReaderForString(string s)
        {
            return new TextualReader(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(s))));
        }

    }
}
