// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Extensibility.Python.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

// Unit test "PythonModule_ExecuteUnitTests" (Python unit tests) is not compatible with .Net Threading model
// Parallelization is disabled for tests in this assembly (though all other tests pass well with Parallelization)
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace NLedger.Extensibility.Python.Tests
{

    /// <summary>
    /// The fact requires NLedger Python connection configured. Otherwise, the test is skipped.
    /// </summary>
    public sealed class PythonFact : FactAttribute
    {
        public PythonFact()
        {
            if (!PythonConnector.Current.IsAvailable)
            {
                Skip = $"NLedger Python Extension is not configured (no file {XmlFilePythonConfigurationReader.DefaultFileName}). Test is skipped.";
            }
        }
    }
}
