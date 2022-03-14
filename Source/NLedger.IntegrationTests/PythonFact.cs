// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Extensibility.Python;
using NLedger.Extensibility.Python.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.IntegrationTests
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
