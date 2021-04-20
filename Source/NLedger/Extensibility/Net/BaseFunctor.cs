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
