// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Expressions;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Scopus
{
    public class ValueScope : ChildScope
    {
        public ValueScope(Scope parent, Value value)
            : base(parent)
        {
            Value = value;
        }

        public Value Value { get; private set; }

        public override string Description
        {
            get { return Parent.Description; }
        }

        public override ExprOp Lookup(SymbolKindEnum kind, string name)
        {
            if (kind != SymbolKindEnum.FUNCTION)
                return null;

            if (name == "value")
                return ExprOp.WrapFunctor(GetValue);

            return base.Lookup(kind, name);
        }

        private Value GetValue(Scope scope)
        {
            // [DM] This is a workaround for .Net delegate limitation: delegates to instance (not static) methods
            // cannot change context object. Original code changes the context object in function pointer
            // See MAKE_FUNCTOR(x) expr_t::op_t::wrap_functor(bind(&x, this, _1)) and value_scope
            // See more:
            // https://stackoverflow.com/questions/2304203/how-to-use-boost-bind-with-a-member-function
            // http://www.boost.org/doc/libs/1_42_0/libs/bind/bind.html#with_functions
            // http://www.boost.org/doc/libs/1_42_0/libs/bind/bind.html#with_member_pointers
            // It might indicate that using ExprOp.WrapFunctor for MAKE_FUNCTOR should be reconsidered
            if (scope != null)
            {
                var sc = scope.FindScope<ValueScope>();
                if (sc != null)
                    return sc.Value;
            }
            // [DM] end workaround

            return Value;
        }
    }
}
