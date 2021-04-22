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
            var selected = Methods.
                Select(m => new Tuple<MethodInfo, int>(m, IsApplicableForParameters(m, paramList))).
                Where(t => t.Item2 >= 0).
                OrderBy(t => t.Item2).
                LastOrDefault()?.Item1;

            if (selected == null)
                throw new InvalidOperationException("TODO");

            return selected;
        }

        private int IsApplicableForParameters(MethodInfo methodInfo, object[] paramList)
        {
            var matchedCount = 0;
            var methodParams = methodInfo.GetParameters();

            for (int i = 0; i<methodParams.Length; i++)
            {
                var methodParam = methodParams[i];
                if (i >= paramList.Length)
                {
                    if (!methodParam.IsOptional)
                        return -1;  // Not appliable method; cannot be used for given param list
                }
                else
                {
                    var paramValue = paramList[i];
                    if (paramValue != null)
                    {
                        if (!methodParam.ParameterType.IsAssignableFrom(paramValue.GetType()))  // TODO - check conversion possibility; TODO - caching
                            return -1;  // Not appliable method; cannot be used for given param list
                    }
                    matchedCount++;
                }                
            }

            return matchedCount;
        }
    }
}
