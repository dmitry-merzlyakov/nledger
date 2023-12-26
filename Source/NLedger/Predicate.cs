// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using NLedger.Annotate;
using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger
{
    public class Predicate : Expr
    {
        public static readonly Predicate EmptyPredicate = new Predicate(String.Empty, new AnnotationKeepDetails());

        public Predicate(AnnotationKeepDetails whatToKeep = default(AnnotationKeepDetails))
        {
            WhatToKeep = whatToKeep;
        }

        public Predicate (ExprOp op, AnnotationKeepDetails whatToKeep, Scope context = null)
            : base (op, context)
        {
            WhatToKeep = whatToKeep;
        }

        public Predicate (string str, AnnotationKeepDetails whatToKeep, AmountParseFlagsEnum parseFlags = AmountParseFlagsEnum.PARSE_DEFAULT)
            : base(str, parseFlags)
        {
            WhatToKeep = whatToKeep;
        }

        public AnnotationKeepDetails WhatToKeep { get; private set; }

        protected override Value RealCalc(Scope scope)
        {
            return !IsEmpty ? Value.Get(base.RealCalc(scope).StripAnnotations(WhatToKeep).AsBoolean) : Value.True;
        }

    }
}
