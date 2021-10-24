using NLedger.Amounts;
using NLedger.Expressions;
using NLedger.Extensibility.Net;
using NLedger.Utility;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Extensibility.Net
{
    public class ValueConverterTests : TestFixture
    {
        [Fact]        
        public void ValueConverter_GetObject_ConvertsValuesToObject()
        {
            var valueConverter = new ValueConverter();

            Assert.IsType<Amount>(valueConverter.GetObject(Value.Get(new Amount(10))));
            Assert.IsType<Balance>(valueConverter.GetObject(Value.Get(new Balance(10))));
            Assert.IsType<Boolean>(valueConverter.GetObject(Value.True));
            Assert.IsType<Date>(valueConverter.GetObject(Value.Get(new Date(2021, 10, 15))));
            Assert.IsType<DateTime>(valueConverter.GetObject(Value.Get(new DateTime(2021, 10, 15))));
            Assert.IsType<int>(valueConverter.GetObject(Value.Get(int.MaxValue - 10)));
            Assert.IsType<long>(valueConverter.GetObject(Value.Get(long.MaxValue - 10)));
            Assert.IsType<Mask>(valueConverter.GetObject(Value.MaskValue("mask")));
            Assert.IsType<String>(valueConverter.GetObject(Value.StringValue("text")));
            Assert.IsType<Post>(valueConverter.GetObject(Value.Get(new Post())));
            Assert.IsType<List<Value>>(valueConverter.GetObject(Value.Get(new List<Value>())));
            Assert.IsType<Expr>(valueConverter.GetObject(Value.Get(new Expr())));
            Assert.Null(valueConverter.GetObject(Value.Empty));
            Assert.Null(valueConverter.GetObject(null));
        }

        [Fact]
        public void ValueConverter_GetValue_ConvertsObjectToValue()
        {
            var valueConverter = new ValueConverter();

            Assert.Equal(ValueTypeEnum.Amount, valueConverter.GetValue(new Amount(10)).Type);
            Assert.Equal(ValueTypeEnum.Balance, valueConverter.GetValue(new Balance(10)).Type);
            Assert.Equal(ValueTypeEnum.Boolean, valueConverter.GetValue(true).Type);
            Assert.Equal(ValueTypeEnum.Date, valueConverter.GetValue(new Date(2021, 10, 15)).Type);
            Assert.Equal(ValueTypeEnum.DateTime, valueConverter.GetValue(new DateTime(2021, 10, 15)).Type);
            Assert.Equal(ValueTypeEnum.Integer, valueConverter.GetValue(10).Type);
            Assert.Equal(ValueTypeEnum.Integer, valueConverter.GetValue(10L).Type);
            Assert.Equal(ValueTypeEnum.Mask, valueConverter.GetValue(new Mask("mask")).Type);
            Assert.Equal(ValueTypeEnum.String, valueConverter.GetValue("text").Type);
            Assert.Equal(ValueTypeEnum.Scope, valueConverter.GetValue(new Post()).Type);
            Assert.Equal(ValueTypeEnum.Sequence, valueConverter.GetValue(new List<Value>()).Type);
            Assert.Equal(ValueTypeEnum.Any, valueConverter.GetValue(new Expr()).Type);
            Assert.Equal(ValueTypeEnum.Void, valueConverter.GetValue(null).Type);
        }

        [Fact]
        public void ValueConverter_ReduceLong_ReducesLongToIntIfPOssible()
        {
            Assert.IsType<int>(ValueConverter.ReduceLong(0L));

            Assert.IsType<int>(ValueConverter.ReduceLong(10L));
            Assert.IsType<int>(ValueConverter.ReduceLong(int.MaxValue - 10));
            Assert.IsType<long>(ValueConverter.ReduceLong(long.MaxValue));

            Assert.IsType<int>(ValueConverter.ReduceLong(-10L));
            Assert.IsType<int>(ValueConverter.ReduceLong(int.MinValue + 10));
            Assert.IsType<long>(ValueConverter.ReduceLong(long.MinValue));

        }

    }
}
