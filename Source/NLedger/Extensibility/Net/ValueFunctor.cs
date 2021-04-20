using NLedger.Scopus;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Net
{
    public class ValueFunctor : BaseFunctor
    {
        public ValueFunctor(object objectValue, IValueConverter valueConverter)
            : base (valueConverter)
        {
            ObjectValue = objectValue;
        }

        public object ObjectValue { get; }

        public override Value ExprFunc(Scope scope)
        {
            return ValueConverter.GetValue(ObjectValue);
        }
    }
}
