// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Scopus
{
    public class ContextScope : ChildScope
    {
        public ContextScope(Scope parent, ValueTypeEnum typeContext = ValueTypeEnum.Void, bool isRequired = true)
            : base(parent)
        {
            _TypeContext = typeContext;
            IsRequired = isRequired;
        }

        public override ValueTypeEnum TypeContext
        {
            get { return _TypeContext; }
        }

        public bool IsRequired { get; private set; }

        public override string Description
        {
            get { return Parent.Description; }
        }

        private ValueTypeEnum _TypeContext;
    }
}
