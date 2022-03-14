// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Extensibility.Python.Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace NLedger.Extensibility.Python
{
    public class PythonExtensionProvider : IExtensionProvider
    {
        public ExtendedSession CreateExtendedSession()
        {
            if (!PythonConnector.Current.IsAvailable)
                return null;

            return new PythonSession();
        }
    }
}
