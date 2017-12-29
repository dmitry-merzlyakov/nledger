// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Expressions
{
    /// <summary>
    /// Ported from expr_t::op_t/context_t
    /// </summary>
    public class ExprOpContext
    {
        public ExprOpContext()
        { }

        public ExprOpContext(ExprOp exprOp, ExprOp opToFind, long? startPos = null, long? endPos = null, bool relaxed = true)
        {
            ExprOp = exprOp;
            OpToFind = opToFind;
            StartPos = startPos;
            EndPos = endPos;
            Relaxed = relaxed;
        }

        public ExprOp ExprOp { get; private set; }
        public ExprOp OpToFind { get; private set; }
        public long? StartPos { get; set; }
        public long? EndPos { get; set; }
        public bool Relaxed { get; private set; }
    }
}
