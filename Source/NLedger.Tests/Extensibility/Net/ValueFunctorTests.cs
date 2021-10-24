using NLedger.Extensibility.Net;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Extensibility.Net
{
    public class ValueFunctorTests
    {
        [Fact]
        public void ValueFunctor_Constructor_PopulatesProperties()
        {
            var val = new object();
            var converter = new ValueConverter();

            var valueFunctor = new ValueFunctor(val, converter);

            Assert.Equal(val, valueFunctor.ObjectValue);
            Assert.Equal(converter, valueFunctor.ValueConverter);
        }

        [Fact]
        public void ValueFunctor_ExprFunc_ReturnsConvertedValue()
        {
            var val = "text";
            var converter = new ValueConverter();
            var valueFunctor = new ValueFunctor(val, converter);

            var result = valueFunctor.ExprFunc(null);

            Assert.Equal(ValueTypeEnum.String, result.Type);
            Assert.Equal("text", result.AsString);
        }
    }
}
