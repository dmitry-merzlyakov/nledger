// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
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
    /// Ported from check_expr_kind_t
    /// </summary>
    public enum CheckExprKindEnum
    {
        EXPR_GENERAL,
        EXPR_ASSERTION,
        EXPR_CHECK
    };

    /// <summary>
    /// Ported from expr_t::check_expr_pair
    /// </summary>
    public struct CheckExprPair
    {
        public Expr Expr { get; private set; }
        public CheckExprKindEnum CheckExprKind { get; private set; }

        public CheckExprPair(Expr expr, CheckExprKindEnum checkExprKind) : this()
        {
            Expr = expr;
            CheckExprKind = checkExprKind;
        }
    }
}
