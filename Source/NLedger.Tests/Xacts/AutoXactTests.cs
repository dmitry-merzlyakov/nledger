// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Accounts;
using NLedger.Expressions;
using NLedger.Items;
using NLedger.Values;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Xacts
{
    [TestClass]
    public class AutoXactTests : TestFixture
    {
        [TestMethod]
        public void AutoXact_Description_ReturnsPosOrStaticText()
        {
            AutoXact autoXact = new AutoXact();
            Assert.AreEqual(AutoXact.GeneratedAutomatedTransactionKey, autoXact.Description);

            autoXact.Pos = new ItemPosition() { BegLine = 22 };
            Assert.AreEqual("automated transaction at line 22", autoXact.Description);
        }

        [TestMethod]
        public void AutoXact_PostPred_Checks_VALUE()
        {
            ExprOp op = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(true) };
            Assert.IsTrue(AutoXact.PostPred(op, null));

            ExprOp op1 = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(false) };
            Assert.IsFalse(AutoXact.PostPred(op1, null));
        }

        [TestMethod]
        public void AutoXact_PostPred_Checks_O_MATCH()
        {
            ExprOp op = new ExprOp(OpKindEnum.O_MATCH)
            {
                Left = new ExprOp(OpKindEnum.IDENT) { AsIdent = "account" },
                Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(new Mask("acc-name")) }
            };

            Post post = new Post()
            {
                ReportedAccount = new Account(null, "acc-name")
            };
            Assert.IsTrue(AutoXact.PostPred(op, post));

            Post post1 = new Post()
            {
                ReportedAccount = new Account(null, "dummy")
            };
            Assert.IsFalse(AutoXact.PostPred(op, post1));
        }

        [TestMethod]
        public void AutoXact_PostPred_Checks_O_EQ()
        {
            ExprOp op = new ExprOp(OpKindEnum.O_EQ)
            {
                Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(true) },
                Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(true) },
            };
            Assert.IsTrue(AutoXact.PostPred(op, null));

            ExprOp op1 = new ExprOp(OpKindEnum.O_EQ)
            {
                Left = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(false) },
                Right = new ExprOp(OpKindEnum.VALUE) { AsValue = Value.Get(true) },
            };
            Assert.IsFalse(AutoXact.PostPred(op1, null));
        }

    }
}
