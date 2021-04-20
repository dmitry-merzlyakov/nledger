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

            var methodInfo = SelectMethod(paramList) ?? throw new InvalidOperationException("No appropriate method");
            var result = methodInfo.Invoke(Subject, paramList);

            return ValueConverter.GetValue(result);
        }

        private MethodInfo SelectMethod(object[] paramList)
        {
            var selected = Methods.Where(m => IsApplicableForParameters(m, paramList));

            if (!selected.Any())
                throw new InvalidOperationException("TODO");
            if (selected.Count() > 1)
                throw new InvalidOperationException("TODO");

            return selected.First();
        }

        private bool IsApplicableForParameters(MethodInfo methodInfo, object[] paramList)
        {
            var methodParams = methodInfo.GetParameters();
            for (int i = 0; i<methodParams.Length; i++)
            {
                var methodParam = methodParams[i];
                if (i >= paramList.Length)
                {
                    if (!methodParam.IsOptional)
                        return false;
                }
                else
                {
                    var paramValue = paramList[i];
                    if (paramValue != null)
                    {
                        if (!methodParam.ParameterType.IsAssignableFrom(paramValue.GetType()))  // TODO - check conversion possibility; TODO - caching
                            return false;
                    }
                }                
            }
            return true;
        }
    }
}
