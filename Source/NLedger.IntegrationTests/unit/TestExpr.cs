// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Querying;
using NLedger.Times;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.IntegrationTests.unit
{
    /// <summary>
    /// Ported from t_expr.cc
    /// </summary>
    [TestClass]
    public class TestExpr
    {
        [TestInitialize]
        public void Initialize()
        {
            MainApplicationContext.Initialize();
            TimesCommon.Current.TimesInitialize();
        }

        [TestCleanup]
        public void Cleanup()
        {
            MainApplicationContext.Cleanup();
        }

        // 1.  foo and bar
        // 2.  'foo and bar'
        // 3.  (foo and bar)
        // 4.  ( foo and bar )
        // 5.  '( foo and' bar)
        // 6.  =foo and bar
        // 7.  ='foo and bar'
        // 8.  'expr foo and bar'
        // 9.  expr 'foo and bar'
        // 10. expr foo and bar
        // 11. foo and bar or baz
        // 12. foo and bar | baz
        // 13. foo and bar |baz
        // 14. foo and bar| baz
        // 15. foo and bar|baz
        // 16. foo 'and bar|baz'

        [TestMethod]
        [TestCategory("BoostAutoTest")]
        public void AutoTestCase_Expr_TestPredicateTokenizer1()
        {
            Value args = new Value();
            args.PushBack(Value.StringValue("foo"));
            args.PushBack(Value.StringValue("and"));
            args.PushBack(Value.StringValue("bar"));

            QueryLexer tokens = new QueryLexer(args.AsSequence);

            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TOK_AND, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.END_REACHED, tokens.NextToken().Kind);
        }

        [TestMethod]
        [TestCategory("BoostAutoTest")]
        public void AutoTestCase_Expr_TestPredicateTokenizer2()
        {
            Value args = new Value();
            args.PushBack(Value.StringValue("foo and bar"));

            QueryLexer tokens = new QueryLexer(args.AsSequence, false);

            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TOK_AND, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.END_REACHED, tokens.NextToken().Kind);
        }

        [TestMethod]
        [TestCategory("BoostAutoTest")]
        public void AutoTestCase_Expr_TestPredicateTokenizer3()
        {
            Value args = new Value();
            args.PushBack(Value.StringValue("(foo"));
            args.PushBack(Value.StringValue("and"));
            args.PushBack(Value.StringValue("bar)"));

            QueryLexer tokens = new QueryLexer(args.AsSequence);

            Assert.AreEqual(QueryLexerTokenKind.LPAREN, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TOK_AND, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.RPAREN, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.END_REACHED, tokens.NextToken().Kind);
        }

        [TestMethod]
        [TestCategory("BoostAutoTest")]
        public void AutoTestCase_Expr_TestPredicateTokenizer4()
        {
            Value args = new Value();
            args.PushBack(Value.StringValue("("));
            args.PushBack(Value.StringValue("foo"));
            args.PushBack(Value.StringValue("and"));
            args.PushBack(Value.StringValue("bar"));
            args.PushBack(Value.StringValue(")"));

            QueryLexer tokens = new QueryLexer(args.AsSequence);

            Assert.AreEqual(QueryLexerTokenKind.LPAREN, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TOK_AND, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.RPAREN, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.END_REACHED, tokens.NextToken().Kind);
        }

        [TestMethod]
        [TestCategory("BoostAutoTest")]
        public void AutoTestCase_Expr_TestPredicateTokenizer5()
        {
            Value args = new Value();
            args.PushBack(Value.StringValue("( foo and"));
            args.PushBack(Value.StringValue("bar)"));

            QueryLexer tokens = new QueryLexer(args.AsSequence, false);

            Assert.AreEqual(QueryLexerTokenKind.LPAREN, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TOK_AND, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.RPAREN, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.END_REACHED, tokens.NextToken().Kind);
        }

        [TestMethod]
        [TestCategory("BoostAutoTest")]
        public void AutoTestCase_Expr_TestPredicateTokenizer6()
        {
            Value args = new Value();
            args.PushBack(Value.StringValue("=foo"));
            args.PushBack(Value.StringValue("and"));
            args.PushBack(Value.StringValue("bar"));

            QueryLexer tokens = new QueryLexer(args.AsSequence);

            Assert.AreEqual(QueryLexerTokenKind.TOK_EQ, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TOK_AND, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.END_REACHED, tokens.NextToken().Kind);
        }

        [TestMethod]
        [TestCategory("BoostAutoTest")]
        public void AutoTestCase_Expr_TestPredicateTokenizer7()
        {
            Value args = new Value();
            args.PushBack(Value.StringValue("=foo and bar"));

            QueryLexer tokens = new QueryLexer(args.AsSequence);

            Assert.AreEqual(QueryLexerTokenKind.TOK_EQ, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.END_REACHED, tokens.NextToken().Kind);
        }

        [TestMethod]
        [TestCategory("BoostAutoTest")]
        public void AutoTestCase_Expr_TestPredicateTokenizer8()
        {
            Value args = new Value();
            args.PushBack(Value.StringValue("expr 'foo and bar'"));

            QueryLexer tokens = new QueryLexer(args.AsSequence, false);

            Assert.AreEqual(QueryLexerTokenKind.TOK_EXPR, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.END_REACHED, tokens.NextToken().Kind);
        }

        [TestMethod]
        [TestCategory("BoostAutoTest")]
        public void AutoTestCase_Expr_TestPredicateTokenizer9()
        {
            Value args = new Value();
            args.PushBack(Value.StringValue("expr"));
            args.PushBack(Value.StringValue("'foo and bar'"));

            QueryLexer tokens = new QueryLexer(args.AsSequence);

            Assert.AreEqual(QueryLexerTokenKind.TOK_EXPR, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.END_REACHED, tokens.NextToken().Kind);
        }

        [TestMethod]
        [TestCategory("BoostAutoTest")]
        public void AutoTestCase_Expr_TestPredicateTokenizer10()
        {
            Value args = new Value();
            args.PushBack(Value.StringValue("expr"));
            args.PushBack(Value.StringValue("foo"));
            args.PushBack(Value.StringValue("and"));
            args.PushBack(Value.StringValue("bar"));

            QueryLexer tokens = new QueryLexer(args.AsSequence);

            Assert.AreEqual(QueryLexerTokenKind.TOK_EXPR, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TOK_AND, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.END_REACHED, tokens.NextToken().Kind);
        }

        [TestMethod]
        [TestCategory("BoostAutoTest")]
        public void AutoTestCase_Expr_TestPredicateTokenizer11()
        {
            Value args = new Value();
            args.PushBack(Value.StringValue("foo"));
            args.PushBack(Value.StringValue("and"));
            args.PushBack(Value.StringValue("bar"));
            args.PushBack(Value.StringValue("or"));
            args.PushBack(Value.StringValue("baz"));

            QueryLexer tokens = new QueryLexer(args.AsSequence);

            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TOK_AND, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TOK_OR, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.END_REACHED, tokens.NextToken().Kind);
        }

        [TestMethod]
        [TestCategory("BoostAutoTest")]
        public void AutoTestCase_Expr_TestPredicateTokenizer12()
        {
            Value args = new Value();
            args.PushBack(Value.StringValue("foo"));
            args.PushBack(Value.StringValue("and"));
            args.PushBack(Value.StringValue("bar"));
            args.PushBack(Value.StringValue("|"));
            args.PushBack(Value.StringValue("baz"));

            QueryLexer tokens = new QueryLexer(args.AsSequence);

            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TOK_AND, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TOK_OR, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.END_REACHED, tokens.NextToken().Kind);
        }

        [TestMethod]
        [TestCategory("BoostAutoTest")]
        public void AutoTestCase_Expr_TestPredicateTokenizer13()
        {
            Value args = new Value();
            args.PushBack(Value.StringValue("foo"));
            args.PushBack(Value.StringValue("and"));
            args.PushBack(Value.StringValue("bar"));
            args.PushBack(Value.StringValue("|baz"));

            QueryLexer tokens = new QueryLexer(args.AsSequence);

            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TOK_AND, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TOK_OR, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.END_REACHED, tokens.NextToken().Kind);
        }

        [TestMethod]
        [TestCategory("BoostAutoTest")]
        public void AutoTestCase_Expr_TestPredicateTokenizer14()
        {
            Value args = new Value();
            args.PushBack(Value.StringValue("foo"));
            args.PushBack(Value.StringValue("and"));
            args.PushBack(Value.StringValue("bar|"));
            args.PushBack(Value.StringValue("baz"));

            QueryLexer tokens = new QueryLexer(args.AsSequence);

            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TOK_AND, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TOK_OR, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.END_REACHED, tokens.NextToken().Kind);
        }

        [TestMethod]
        [TestCategory("BoostAutoTest")]
        public void AutoTestCase_Expr_TestPredicateTokenizer15()
        {
            Value args = new Value();
            args.PushBack(Value.StringValue("foo"));
            args.PushBack(Value.StringValue("and"));
            args.PushBack(Value.StringValue("bar|baz"));

            QueryLexer tokens = new QueryLexer(args.AsSequence);

            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TOK_AND, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TOK_OR, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.END_REACHED, tokens.NextToken().Kind);
        }

        [TestMethod]
        [TestCategory("BoostAutoTest")]
        public void AutoTestCase_Expr_TestPredicateTokenizer16()
        {
            Value args = new Value();
            args.PushBack(Value.StringValue("foo"));
            args.PushBack(Value.StringValue("and bar|baz"));

            QueryLexer tokens = new QueryLexer(args.AsSequence, false);

            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TOK_AND, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TOK_OR, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.TERM, tokens.NextToken().Kind);
            Assert.AreEqual(QueryLexerTokenKind.END_REACHED, tokens.NextToken().Kind);
        }

    }
}
