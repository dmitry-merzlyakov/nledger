// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Net
{
    public abstract class BaseFunctor
    {
        public static BaseFunctor Selector(object obj, IValueConverter valueConverter)
        {
            if (obj == null)
                return new ValueFunctor(null, valueConverter);

            var type = obj.GetType();
            if (typeof(System.Delegate).IsAssignableFrom(type))
                return new MethodFunctor(obj, new MethodInfo[] { type.GetMethod("Invoke") }, valueConverter);
            
            return new ValueFunctor(obj, valueConverter);
        }

        public BaseFunctor(IValueConverter valueConverter)
        {
            ValueConverter = valueConverter ?? throw new ArgumentNullException(nameof(valueConverter));
        }

        public IValueConverter ValueConverter { get; }

        public ExprFunc ExprFunctor => ExprFunc;

        public abstract Value ExprFunc(Scope scope);

        protected IEnumerable<object> GetParamList(CallScope callScope)
        {
            if (callScope.Value().Type == ValueTypeEnum.Sequence)
            {
                foreach (var val in callScope.Value().AsSequence)
                    yield return ValueConverter.GetObject(val);
            }
            else
            {
                yield return ValueConverter.GetObject(callScope.Value());
            }
        }
    }
}
