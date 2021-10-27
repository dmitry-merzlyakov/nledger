// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Extensibility
{
    public class SessionExtensionsTests
    {
        [Fact]
        public void SessionExtensions_CommandExecutionResult_CanCreateSuccess()
        {
            var result = SessionExtensions.CommandExecutionResult.Success("success-text");
            Assert.Equal("success-text", result.Output);
            Assert.Equal(String.Empty, result.Error);
        }

        [Fact]
        public void SessionExtensions_CommandExecutionResult_Failure()
        {
            var result = SessionExtensions.CommandExecutionResult.Failure("error-text");
            Assert.Equal(String.Empty, result.Output);
            Assert.Equal("error-text", result.Error);
        }

        [Fact]
        public void SessionExtensions_CommandExecutionResult_EmptyContainsNothing()
        {
            Assert.Equal(String.Empty, SessionExtensions.CommandExecutionResult.Empty.Output);
            Assert.Equal(String.Empty, SessionExtensions.CommandExecutionResult.Empty.Error);
        }

        private static readonly string Input = @"
2009/11/01 Panera Bread  ; Got something to eat
    Expenses:Food               $4.50
    Assets:Checking

";

        [Fact]
        public void SessionExtensions_ExecuteCommand_TakesArgumentLine()
        {
            using (var session = NLedger.Extensibility.Net.NetSession.CreateStandaloneSession())
            {
                MainApplicationContext.Current.IsAtty = false;
                session.ReadJournalFromString(Input);
                var result = session.ExecuteCommand("bal ^Expenses");
                Assert.Equal("$4.50  Expenses:Food", result.Output.Trim());
                Assert.Equal(String.Empty, result.Error);
            }
        }

        [Fact]
        public void SessionExtensions_ExecuteCommand_TakesArgumentList()
        {
            using (var session = NLedger.Extensibility.Net.NetSession.CreateStandaloneSession())
            {
                MainApplicationContext.Current.IsAtty = false;
                session.ReadJournalFromString(Input);
                var result = session.ExecuteCommand(new string[] { "bal", "^Expenses" });
                Assert.Equal("$4.50  Expenses:Food", result.Output.Trim());
                Assert.Equal(String.Empty, result.Error);
            }
        }

    }
}
