// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace NLedger.Extensibility.Python
{
    public static class Extensions
    {
        public static string GetPythonTypeName(this PyObject obj)
        {
            return obj?.GetPythonType().GetAttr("__name__").ToString();
        }

        public static bool IsModule(this PyObject obj)
        {
            return obj.GetPythonTypeName() == "module";
        }
    }
}
