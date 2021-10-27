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
    public class MethodFunctor : BaseFunctor
    {
        public MethodFunctor(object subject, MethodInfo[] methods, IValueConverter valueConverter)
            : base (valueConverter)
        {
            Subject = subject;
            Methods = methods ?? throw new ArgumentNullException(nameof(methods));
        }

        public object Subject { get; }
        public MethodInfo[] Methods { get; }

        public override Value ExprFunc(Scope scope)
        {
            var callScope = (scope as CallScope) ?? throw new InvalidOperationException("Expected CallScope");
            var paramList = GetParamList(callScope).ToArray();

            var methodCall = SelectMethod(paramList);
            var result = methodCall.Invoke(Subject);

            return ValueConverter.GetValue(result);
        }

        private MethodCall SelectMethod(object[] paramList)
        {
            var selected = Methods.
                Select(m => BuildMethodCall(m, paramList)).
                Where(m => m != null).
                OrderBy(m => m.SortOrder()).
                LastOrDefault();

            if (selected == null)
            {
                var parms = String.Join(",", paramList.Select(p => p?.GetType().ToString() ?? "null"));
                throw new InvalidOperationException($"Method with an appropriate argument types not found (parameters: [{parms}])");
            }

            return selected;
        }

        private MethodCall BuildMethodCall(MethodInfo methodInfo, object[] paramList)
        {
            var methodCall = new MethodCall() { MethodInfo = methodInfo };
            var methodParams = methodInfo.GetParameters();

            for (int i = 0; i < methodParams.Length; i++)
            {
                var methodParam = methodParams[i];
                if (i >= paramList.Length)
                {
                    if (!methodParam.IsOptional)
                        return null;  // The method requires more parameters rather than paramList contains. Not applicable.
                }
                else
                {
                    var paramValue = paramList[i];
                    if (paramValue != null)
                    {
                        var paramType = paramValue.GetType();
                        if (!methodParam.ParameterType.IsAssignableFrom(paramType))
                        {
                            try
                            {
                                paramValue = System.Convert.ChangeType(paramValue, methodParam.ParameterType);
                            }
                            catch
                            {
                                return null;  // The parameter cannot be converted to type that the method requires. Not applicable.
                            }
                        }
                    }
                    methodCall.ParamList.Add(paramValue);
                }
            }

            return methodCall;
        }

        private class MethodCall
        {
            public MethodInfo MethodInfo { get; set; }
            public bool ContainsImplicitConversion { get; set; }
            public List<object> ParamList { get; } = new List<object>();
            public object Invoke(object subject) => MethodInfo.Invoke(subject, ParamList.ToArray());

            public int SortOrder() => 1024 * (ContainsImplicitConversion ? 0 : 1) + ParamList.Count;
        }
    }
}
