// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

namespace NLedger.Tests.Scopus
{
    [TestClass]
    [TestFixtureInit(ContextInit.InitMainApplicationContext | ContextInit.InitTimesCommon)]
    public class SessionTests : TestFixture
    {
        [TestMethod]
        public void Session_Constructor_SetsDefaultProperties()
        {
            Session session = new Session();

            Assert.IsNotNull(session.ParsingContext);

            Assert.IsNotNull(session.CheckPayeesHandler);
            Assert.IsNotNull(session.DayBreakHandler);
            Assert.IsNotNull(session.DownloadHandler);
            Assert.IsNotNull(session.DecimalCommaHandler);
            Assert.IsNotNull(session.TimeColonHandler);
            Assert.IsNotNull(session.PriceExpHandler);
            Assert.IsNotNull(session.FileHandler);
            Assert.IsNotNull(session.InputDateFormatHandler);
            Assert.IsNotNull(session.ExplicitHandler);
            Assert.IsNotNull(session.MasterAccountHandler);
            Assert.IsNotNull(session.PedanticHandler);
            Assert.IsNotNull(session.PermissiveHandler);
            Assert.IsNotNull(session.PriceDbHandler);
            Assert.IsNotNull(session.StrictHandler);
            Assert.IsNotNull(session.ValueExprHandler);
            Assert.IsNotNull(session.RecursiveAliasesHandler);
            Assert.IsNotNull(session.NoAliasesHandler);

            Assert.IsNotNull(session.Journal);

            Assert.AreEqual(FileSystem.CurrentPath(), session.ParsingContext.GetCurrent().CurrentPath);
        }

        [TestMethod]
        public void Session_Description_ReturnsCurrentSessionKey()
        {
            Session session = new Session();
            Assert.AreEqual(Session.CurrentSessionKey, session.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(ParseError))]
        public void Session_ReadData_UsesDefaultLedgerFileNameIfNoFilesSpecifiedAndFilesBecauseOfNoFile()
        {
            Session session = new Session();
            session.ReadData(null);
        }

        [TestMethod]
        public void Session_ReadData_UsesInputStreamIfFileNameIsMinus()
        {
            var input = new System.IO.StringReader(Session_ReadJournalFromString_Example);
            MainApplicationContext.Current.SetVirtualConsoleProvider(() => new VirtualConsoleProvider(input));

            Scope.DefaultScope = new EmptyScope();
            Session session = new Session();
            session.FileHandler.DataFiles.Add("-");
            int xacts = session.ReadData(null);
            Assert.AreEqual(1, xacts);
        }

        [TestMethod]
        public void Session_ReadJournalFromString_ParsesJournalComingFromString()
        {
            Scope.DefaultScope = new EmptyScope();
            Session session = new Session();
            session.CloseJournalFiles();

            Assert.AreEqual(1, session.ParsingContext.Count);
            Journal journal = session.ReadJournalFromString(Session_ReadJournalFromString_Example);
            Assert.IsNotNull(journal);
            Assert.AreEqual(1, session.ParsingContext.Count);

            Assert.IsNotNull(journal.Master.FindAccount("Income", false));
            Assert.IsNotNull(journal.Master.FindAccount("Assets:Checking", false));
        }

        [TestMethod]
        public void Session_CloseJournalFiles_ReInitializesCommodityPool()
        {
            Session session = new Session();

            Journal journal1 = session.Journal;
            CommodityPool pool1 = CommodityPool.Current;

            session.CloseJournalFiles();

            Assert.AreNotEqual(journal1, session.Journal);
            Assert.AreNotEqual(pool1, CommodityPool.Current);
        }

        [TestMethod]
        public void Session_FnAccount_FindsAccountIfFirstArgIsString()
        {
            Session session = new Session();
            Account account = session.Journal.Master.FindAccount("my-account");
            Value val = Value.Get("my-account");

            CallScope scope1 = new CallScope(new EmptyScope());
            scope1.PushBack(val);

            var result = session.FnAccount(scope1).AsScope;
            Assert.AreEqual(result, account);
        }

        [TestMethod]
        public void Session_FnAccount_FindsAccountByMaskIfFirstArgIsMask()
        {
            Session session = new Session();
            Account account = session.Journal.Master.FindAccount("my-account");
            Value val = Value.Get(new Mask("my-account"));

            CallScope scope1 = new CallScope(new EmptyScope());
            scope1.PushBack(val);

            var result = session.FnAccount(scope1).AsScope;
            Assert.AreEqual(result, account);
        }

        [TestMethod]
        public void Session_FnMin_ReturnsMinArgument()
        {
            Session session = new Session();

            Value minVal = Value.Get(-1);
            Value maxVal = Value.Get(1);

            CallScope scope1 = new CallScope(new EmptyScope());
            scope1.PushBack(minVal);
            scope1.PushBack(maxVal);
            Assert.AreEqual(minVal, session.FnMin(scope1));

            CallScope scope2 = new CallScope(new EmptyScope());
            scope2.PushBack(maxVal);
            scope2.PushBack(minVal);
            Assert.AreEqual(minVal, session.FnMin(scope2));
        }

        [TestMethod]
        public void Session_FnMax_ReturnsMaxArgument()
        {
            Session session = new Session();

            Value minVal = Value.Get(-1);
            Value maxVal = Value.Get(1);

            CallScope scope1 = new CallScope(new EmptyScope());
            scope1.PushBack(minVal);
            scope1.PushBack(maxVal);
            Assert.AreEqual(maxVal, session.FnMax(scope1));

            CallScope scope2 = new CallScope(new EmptyScope());
            scope2.PushBack(maxVal);
            scope2.PushBack(minVal);
            Assert.AreEqual(maxVal, session.FnMax(scope2));
        }

        [TestMethod]
        public void Session_FnInt_ReturnsFirstArgumentAsLong()
        {
            Session session = new Session();
            Value val = Value.Get(234);

            CallScope scope1 = new CallScope(new EmptyScope());
            scope1.PushBack(val);
            Assert.AreEqual(234, session.FnInt(scope1).AsLong);
        }

        [TestMethod]
        public void Session_FnStr_ReturnsFirstArgumentAsString()
        {
            Session session = new Session();
            Value val = Value.Get("USD23");

            CallScope scope1 = new CallScope(new EmptyScope());
            scope1.PushBack(val);
            Assert.AreEqual(23, session.FnStr(scope1).AsAmount.Quantity.ToLong());
        }

        [TestMethod]
        public void Session_FnLotPrice_ReturnsPriceFromFirstArgAmouunt()
        {
            Session session = new Session();

            Amount amount1 = new Amount(0);  // No price
            CallScope scope1 = new CallScope(new EmptyScope());
            //scope1.PushBack(Value.Get(false));  // first argument
            scope1.PushBack(Value.Get(amount1));
            Assert.AreEqual(Value.Empty, session.FnLotPrice(scope1));

            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("base"));
            Amount price = new Amount(5);
            Annotation annotation = new Annotation(price);
            AnnotatedCommodity annotatedCommodity = new AnnotatedCommodity(commodity, annotation);
            Amount amount2 = new Amount(10, annotatedCommodity);  // With price
            CallScope scope2 = new CallScope(new EmptyScope());
            //scope2.PushBack(Value.Get(false));  // first argument
            scope2.PushBack(Value.Get(amount2));
            Assert.AreEqual(price, session.FnLotPrice(scope2).AsAmount);
        }

        [TestMethod]
        public void Session_FnLotDate_ReturnsDateFromFirstArgAmount()
        {
            Session session = new Session();

            Amount amount1 = new Amount(0);  // No date
            CallScope scope1 = new CallScope(new EmptyScope());
            //scope1.PushBack(Value.Get(false));  // first argument
            scope1.PushBack(Value.Get(amount1));
            Assert.AreEqual(Value.Empty, session.FnLotDate(scope1));

            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("base"));
            Date date = (Date)DateTime.Now.Date;
            Annotation annotation = new Annotation() { Date = date };
            AnnotatedCommodity annotatedCommodity = new AnnotatedCommodity(commodity, annotation);
            Amount amount2 = new Amount(10, annotatedCommodity);  // With date
            CallScope scope2 = new CallScope(new EmptyScope());
            //scope2.PushBack(Value.Get(false));  // first argument
            scope2.PushBack(Value.Get(amount2));
            Assert.AreEqual(date, session.FnLotDate(scope2).AsDate);
        }

        [TestMethod]
        public void Session_FnLotTag_ReturnsDateFromFirstArgAmouunt()
        {
            Session session = new Session();

            Amount amount1 = new Amount(0);  // No tag
            CallScope scope1 = new CallScope(new EmptyScope());
            //scope1.PushBack(Value.Get(false));  // first argument
            scope1.PushBack(Value.Get(amount1));
            Assert.AreEqual(Value.Empty, session.FnLotTag(scope1));

            Commodity commodity = new Commodity(CommodityPool.Current, new CommodityBase("base"));
            string tag = "my-tag";
            Annotation annotation = new Annotation() { Tag = tag };
            AnnotatedCommodity annotatedCommodity = new AnnotatedCommodity(commodity, annotation);
            Amount amount2 = new Amount(10, annotatedCommodity);  // With date
            CallScope scope2 = new CallScope(new EmptyScope());
            //scope2.PushBack(Value.Get(false));  // first argument
            scope2.PushBack(Value.Get(amount2));
            Assert.AreEqual(tag, session.FnLotTag(scope2).AsString);
        }

        [TestMethod]
        public void Session_LookupOption_ReturnsOptionsByNames()
        {
            Session session = new Session();

            Assert.AreEqual(session.CheckPayeesHandler, session.LookupOption("check-payees"));
            Assert.AreEqual(session.DayBreakHandler, session.LookupOption("day-break"));
            Assert.AreEqual(session.DownloadHandler, session.LookupOption("download"));
            Assert.AreEqual(session.DecimalCommaHandler, session.LookupOption("decimal-comma"));
            Assert.AreEqual(session.TimeColonHandler, session.LookupOption("time-colon"));
            Assert.AreEqual(session.PriceExpHandler, session.LookupOption("price-exp"));
            Assert.AreEqual(session.FileHandler, session.LookupOption("file"));
            Assert.AreEqual(session.InputDateFormatHandler, session.LookupOption("input-date-format"));
            Assert.AreEqual(session.ExplicitHandler, session.LookupOption("explicit"));
            Assert.AreEqual(session.MasterAccountHandler, session.LookupOption("master-account"));
            Assert.AreEqual(session.PedanticHandler, session.LookupOption("pedantic"));
            Assert.AreEqual(session.PermissiveHandler, session.LookupOption("permissive"));
            Assert.AreEqual(session.PriceDbHandler, session.LookupOption("price-db"));
            Assert.AreEqual(session.StrictHandler, session.LookupOption("strict"));
            Assert.AreEqual(session.ValueExprHandler, session.LookupOption("value-expr"));
            Assert.AreEqual(session.RecursiveAliasesHandler, session.LookupOption("recursive-aliases"));
            Assert.AreEqual(session.NoAliasesHandler, session.LookupOption("no-aliases"));
        }

        [TestMethod]
        public void Session_Lookup_LooksForHandlers()
        {
            Session session = new Session();

            ExprOp checkPayeesOp = session.Lookup(SymbolKindEnum.OPTION, "check-payees");
            Assert.IsNotNull(checkPayeesOp);
            Assert.IsTrue(checkPayeesOp.IsFunction);
            Assert.AreEqual(OpKindEnum.FUNCTION, checkPayeesOp.Kind);
            Assert.IsNotNull(checkPayeesOp.AsFunction);
            // Check that the function is callable
            CallScope callScope = new CallScope(new EmptyScope());
            callScope.PushBack(Value.Get("str"));
            checkPayeesOp.AsFunction(callScope);
            Assert.IsTrue(session.CheckPayeesHandler.Handled);
        }

        [TestMethod]
        public void Session_Lookup_LooksForFunctors()
        {
            Session session = new Session();
            ExprOp minFunc = session.Lookup(SymbolKindEnum.FUNCTION, "min");
            Assert.IsNotNull(minFunc);
            Assert.IsTrue(minFunc.IsFunction);
            Assert.AreEqual(OpKindEnum.FUNCTION, minFunc.Kind);
            Assert.IsNotNull(minFunc.AsFunction);
            // Check that the function is callable
            CallScope callScope = new CallScope(new EmptyScope());
            callScope.PushBack(Value.Get("1"));
            callScope.PushBack(Value.Get("2"));
            string result = minFunc.AsFunction(callScope).AsString;
            Assert.AreEqual("1", result);
        }

        [TestMethod]
        public void Session_ReportOptions_ReturnsCompositeReportForAllOptions()
        {
            Session session = new Session();
            string reportOptions = session.ReportOptions();
            Assert.AreEqual("", reportOptions);  // None of options were handled yet

            session.CheckPayeesHandler.On("whence");
            reportOptions = session.ReportOptions();
            Assert.AreEqual(Session_ReportOptions_Example, reportOptions.TrimEnd());
        }

        private const string Session_ReportOptions_Example = "            check-payees                                             whence";
        private const string Session_ReadJournalFromString_Example = @"2009/10/30 (DEP) Pay day!
    Assets:Checking            $20.00
    Income
";
    }
}
