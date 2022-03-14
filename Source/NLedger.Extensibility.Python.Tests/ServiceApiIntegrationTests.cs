// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Abstracts.Impl;
using NLedger.Utility.ServiceAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Extensibility.Python.Tests
{
    public class ServiceApiIntegrationTests
    {
        [PythonFact]
        // Python extension example: using Python function in 'check' directive. The function takes PATH tag value as a named parameter ('value').
        public void ServiceAPI_Uses_PythonSession()
        {
            var inputText = @"
python
    import os
    def check_path(path):
        return path=='expected_path'

tag PATH
    check check_path(value)

2012-02-29 KFC
    ; PATH: expected_path
    Expenses:Food                $20
    Assets:Cash

2012-02-29 KFC
    ; PATH: wrong_path
    Expenses:Food                $20
    Assets:Cash
";

            var engine = new ServiceEngine(
                createCustomProvider: mem =>
                {
                    return new ApplicationServiceProvider(
                        virtualConsoleProviderFactory: () => new VirtualConsoleProvider(mem.ConsoleInput, mem.ConsoleOutput, mem.ConsoleError),
                        extensionProviderFactory: () => new PythonExtensionProvider());
                });

            var session = engine.CreateSession("-f /dev/stdin", inputText);
            Assert.True(session.IsActive, session.ErrorText);
            Assert.Equal("Warning: \"\", line 18: Metadata check failed for (PATH: wrong_path): check_path(value)", session.ErrorText.Trim());

            var response = session.ExecuteCommand("reg");
            Assert.False(response.HasErrors);
        }

    }
}
