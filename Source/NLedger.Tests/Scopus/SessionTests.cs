// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Abstracts.Impl;
using NLedger.Accounts;
using NLedger.Amounts;
using NLedger.Annotate;
using NLedger.Commodities;
using NLedger.Expressions;
using NLedger.Journals;
using NLedger.Scopus;
using NLedger.Textual;
using NLedger.Times;
using NLedger.Utility;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Scopus
{
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon)]
    public class SessionTests : TestFixture
    {
        [Fact]
        public void Session_Constructor_SetsDefaultProperties()
        {
            Session session = new Session();

            Assert.NotNull(session.ParsingContext);

            Assert.NotNull(session.CheckPayeesHandler);
            Assert.NotNull(session.DayBreakHandler);
            Assert.NotNull(session.DownloadHandler);
            Assert.NotNull(session.DecimalCommaHandler);
            Assert.NotNull(session.TimeColonHandler);
            Assert.NotNull(session.PriceExpHandler);
            Assert.NotNull(session.FileHandler);
            Assert.NotNull(session.InputDateFormatHandler);
            Assert.NotNull(session.ExplicitHandler);
            Assert.NotNull(session.MasterAccountHandler);
            Assert.NotNull(session.PedanticHandler);
            Assert.NotNull(session.PermissiveHandler);
            Assert.NotNull(session.PriceDbHandler);
            Assert.NotNull(session.StrictHandler);
            Assert.NotNull(session.ValueExprHandler);
            Assert.NotNull(session.RecursiveAliasesHandler);
            Assert.NotNull(session.NoAliasesHandler);

            Assert.NotNull(session.Journal);

            Assert.Equal(FileSystem.CurrentPath(), session.ParsingContext.GetCurrent().CurrentPath);
        }

        [Fact]
        public void Session_Description_ReturnsCurrentSessionKey()
        {
            Session session = new Session();
            Assert.Equal(Session.CurrentSessionKey, session.Description);
        }

        [Fact]
        public void Session_ReadData_UsesDefaultLedgerFileNameIfNoFilesSpecifiedAndFilesBecauseOfNoFile()
        {
            Session session = new Session();
            Assert.Throws<ParseError>(() => session.ReadData(null));
        }

        [Fact]
        public void Session_ReadData_UsesInputStreamIfFileNameIsMinus()
        {
            var input = new System.IO.StringReader(Session_ReadJournalFromString_Example);
            MainApplicationContext.Current.SetApplicationServiceProvider(new ApplicationServiceProvider
                (virtualConsoleProviderFactory: () => new VirtualConsoleProvider(input)));

            Scope.DefaultScope = new EmptyScope();
            Session session = new Session();
            session.FileHandler.DataFiles.Add("-");
            int xacts = session.ReadData(null);
            Assert.Equal(1, xacts);
        }

        [Fact]
        public void Session_ReadJournalFromString_ParsesJournalComingFromString()
        {
            Scope.DefaultScope = new EmptyScope();
            Session session = new Session();
            session.CloseJournalFiles();

            Assert.Equal(1, session.ParsingContext.Count);
            Journal journal = session.ReadJournalFromString(Session_ReadJournalFromString_Example);
            Assert.NotNull(journal);
            Assert.Equal(1, session.ParsingContext.Count);

            Assert.NotNull(journal.Master.FindAccount("Income", false));
            Assert.NotNull(journal.Master.FindAccount("Assets:Checking", false));
        }

        [Fact]
        public void Session_CloseJournalFiles_ReInitializesCommodityPool()
        {
            Session session = new Session();

            Journal journal1 = session.Journal;
            CommodityPool pool1 = CommodityPool.Current;

            session.CloseJournalFiles();

            Assert.NotEqual(journal1, session.Journal);
            Assert.NotEqual(pool1, CommodityPool.Current);
        }

        [Fact]
        public void Session_FnAccount_FindsAccountIfFirstArgIsString()
        {
            Session session = new Session();
            Account account = session.Journal.Master.FindAccount("my-account");
            Value val = Value.Get("my-account");

            CallScope scope1 = new CallScope(new EmptyScope());
            scope1.PushBack(val);

            var result = session.FnAccount(scope1).AsScope;
            Assert.Equal(result, account);
        }

        [Fact]
        public void Session_FnAccount_FindsAccountByMaskIfFirstArgIsMask()
        {
            Session session = new Session();
            Account account = session.Journal.Master.FindAccount("my-account");
            Value val = Value.Get(new Mask("my-account"));

            CallScope scope1 = new CallScope(new EmptyScope());
            scope1.PushBack(val);

            var result = session.FnAccount(scope1).AsScope;
            Assert.Equal(result, account);
        }

        [Fact]
        public void Session_FnMin_ReturnsMinArgument()
        {
            Session session = new Session();

            Value minVal = Value.Get(-1);
            Value maxVal = Value.Get(1);

            CallScope scope1 = new CallScope(new EmptyScope());
            scope1.PushBack(minVal);
            scope1.PushBack(maxVal);
            Assert.Equal(minVal, session.FnMin(scope1));

            CallScope scope2 = new CallScope(new EmptyScope());
            scope2.PushBack(maxVal);
            scope2.PushBack(minVal);
            Assert.Equal(minVal, session.FnMin(scope2));
        }

        [Fact]
        public void Session_FnMax_ReturnsMaxArgument()
        {
            Session session = new Session();

            Value minVal = Value.Get(-1);
            Value maxVal = Value.Get(1);

            CallScope scope1 = new CallScope(new EmptyScope());
            scope1.PushBack(minVal);
            scope1.PushBack(maxVal);
            Assert.Equal(maxVal, session.FnMax(scope1));

            CallScope scope2 = new CallScope(new EmptyScope());
            scope2.PushBack(maxVal);
            scope2.PushBack(minVal);
            Assert.Equal(maxVal, session.FnMax(scope2));
        }

        [Fact]
        public void Session_FnInt_ReturnsFirstArgumentAsLong()
        {
            Session session = new Session();
            Value val = Value.Get(234);

            CallScope scope1 = new CallScope(new EmptyScope());
            scope1.PushBack(val);
            Assert.Equal(234, session.FnInt(scope1).AsLong);
        }

        [Fact]
        public void Session_FnStr_ReturnsFirstArgumentAsString()
        {
            Session session = new Session();
            Value val = Value.Get("USD23");

            CallScope scope1 = new CallScope(new EmptyScope());
            scope1.PushBack(val);
            Assert.Equal(23, session.FnStr(scope1).AsAmount.Quantity.ToLong());
        }

        [Fact]
        public void Session_FnLotPrice_ReturnsPriceFromFirstArgAmouunt()
        {
            Session session = new Session();

            Amount amount1 = new Amount(0);  // No price
            CallScope scope1 = new CallScope(new EmptyScope());
            //scope1.PushBack(Value.Get(false));  // first argument
            scope1.PushBack(Value.Get(amount1));
            Assert.Equal(Value.Empty, session.FnLotPrice(scope1));

            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("base"));
            Amount price = new Amount(5);
            Annotation annotation = new Annotation(price);
            AnnotatedCommodity annotatedCommodity = new AnnotatedCommodity(commodity, annotation);
            Amount amount2 = new Amount(10, annotatedCommodity);  // With price
            CallScope scope2 = new CallScope(new EmptyScope());
            //scope2.PushBack(Value.Get(false));  // first argument
            scope2.PushBack(Value.Get(amount2));
            Assert.Equal(price, session.FnLotPrice(scope2).AsAmount);
        }

        [Fact]
        public void Session_FnLotDate_ReturnsDateFromFirstArgAmount()
        {
            Session session = new Session();

            Amount amount1 = new Amount(0);  // No date
            CallScope scope1 = new CallScope(new EmptyScope());
            //scope1.PushBack(Value.Get(false));  // first argument
            scope1.PushBack(Value.Get(amount1));
            Assert.Equal(Value.Empty, session.FnLotDate(scope1));

            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("base"));
            Date date = (Date)DateTime.Now.Date;
            Annotation annotation = new Annotation() { Date = date };
            AnnotatedCommodity annotatedCommodity = new AnnotatedCommodity(commodity, annotation);
            Amount amount2 = new Amount(10, annotatedCommodity);  // With date
            CallScope scope2 = new CallScope(new EmptyScope());
            //scope2.PushBack(Value.Get(false));  // first argument
            scope2.PushBack(Value.Get(amount2));
            Assert.Equal(date, session.FnLotDate(scope2).AsDate);
        }

        [Fact]
        public void Session_FnLotTag_ReturnsDateFromFirstArgAmouunt()
        {
            Session session = new Session();

            Amount amount1 = new Amount(0);  // No tag
            CallScope scope1 = new CallScope(new EmptyScope());
            //scope1.PushBack(Value.Get(false));  // first argument
            scope1.PushBack(Value.Get(amount1));
            Assert.Equal(Value.Empty, session.FnLotTag(scope1));

            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("base"));
            string tag = "my-tag";
            Annotation annotation = new Annotation() { Tag = tag };
            AnnotatedCommodity annotatedCommodity = new AnnotatedCommodity(commodity, annotation);
            Amount amount2 = new Amount(10, annotatedCommodity);  // With date
            CallScope scope2 = new CallScope(new EmptyScope());
            //scope2.PushBack(Value.Get(false));  // first argument
            scope2.PushBack(Value.Get(amount2));
            Assert.Equal(tag, session.FnLotTag(scope2).AsString);
        }

        [Fact]
        public void Session_LookupOption_ReturnsOptionsByNames()
        {
            Session session = new Session();

            Assert.Equal(session.CheckPayeesHandler, session.LookupOption("check-payees"));
            Assert.Equal(session.DayBreakHandler, session.LookupOption("day-break"));
            Assert.Equal(session.DownloadHandler, session.LookupOption("download"));
            Assert.Equal(session.DecimalCommaHandler, session.LookupOption("decimal-comma"));
            Assert.Equal(session.TimeColonHandler, session.LookupOption("time-colon"));
            Assert.Equal(session.PriceExpHandler, session.LookupOption("price-exp"));
            Assert.Equal(session.FileHandler, session.LookupOption("file"));
            Assert.Equal(session.InputDateFormatHandler, session.LookupOption("input-date-format"));
            Assert.Equal(session.ExplicitHandler, session.LookupOption("explicit"));
            Assert.Equal(session.MasterAccountHandler, session.LookupOption("master-account"));
            Assert.Equal(session.PedanticHandler, session.LookupOption("pedantic"));
            Assert.Equal(session.PermissiveHandler, session.LookupOption("permissive"));
            Assert.Equal(session.PriceDbHandler, session.LookupOption("price-db"));
            Assert.Equal(session.StrictHandler, session.LookupOption("strict"));
            Assert.Equal(session.ValueExprHandler, session.LookupOption("value-expr"));
            Assert.Equal(session.RecursiveAliasesHandler, session.LookupOption("recursive-aliases"));
            Assert.Equal(session.NoAliasesHandler, session.LookupOption("no-aliases"));
        }

        [Fact]
        public void Session_Lookup_LooksForHandlers()
        {
            Session session = new Session();

            ExprOp checkPayeesOp = session.Lookup(SymbolKindEnum.OPTION, "check-payees");
            Assert.NotNull(checkPayeesOp);
            Assert.True(checkPayeesOp.IsFunction);
            Assert.Equal(OpKindEnum.FUNCTION, checkPayeesOp.Kind);
            Assert.NotNull(checkPayeesOp.AsFunction);
            // Check that the function is callable
            CallScope callScope = new CallScope(new EmptyScope());
            callScope.PushBack(Value.Get("str"));
            checkPayeesOp.AsFunction(callScope);
            Assert.True(session.CheckPayeesHandler.Handled);
        }

        [Fact]
        public void Session_Lookup_LooksForFunctors()
        {
            Session session = new Session();
            ExprOp minFunc = session.Lookup(SymbolKindEnum.FUNCTION, "min");
            Assert.NotNull(minFunc);
            Assert.True(minFunc.IsFunction);
            Assert.Equal(OpKindEnum.FUNCTION, minFunc.Kind);
            Assert.NotNull(minFunc.AsFunction);
            // Check that the function is callable
            CallScope callScope = new CallScope(new EmptyScope());
            callScope.PushBack(Value.Get("1"));
            callScope.PushBack(Value.Get("2"));
            string result = minFunc.AsFunction(callScope).AsString;
            Assert.Equal("1", result);
        }

        [Fact]
        public void Session_ReportOptions_ReturnsCompositeReportForAllOptions()
        {
            Session session = new Session();
            string reportOptions = session.ReportOptions();
            Assert.Equal("", reportOptions);  // None of options were handled yet

            session.CheckPayeesHandler.On("whence");
            reportOptions = session.ReportOptions();
            Assert.Equal(Session_ReportOptions_Example, reportOptions.TrimEnd());
        }

        private const string Session_ReportOptions_Example = "            check-payees                                             whence";
        private const string Session_ReadJournalFromString_Example = @"2009/10/30 (DEP) Pay day!
    Assets:Checking            $20.00
    Income
";
    }
}
