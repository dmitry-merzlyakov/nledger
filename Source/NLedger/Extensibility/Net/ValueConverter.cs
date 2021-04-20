using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Net
{
    public class ValueConverter : IValueConverter
    {
        public object GetObject(Value val)
        {
            switch (val.Type)
            {
                case ValueTypeEnum.Amount: return val.AsAmount;
                case ValueTypeEnum.Any: return val.AsAny();
                case ValueTypeEnum.Balance: return val.AsBalance;
                case ValueTypeEnum.Boolean: return val.AsBoolean;
                case ValueTypeEnum.Date: return val.AsDateTime;
                case ValueTypeEnum.DateTime: return val.AsDateTime;
                case ValueTypeEnum.Integer: return val.AsLong;
                case ValueTypeEnum.Mask: return val.AsMask;
                case ValueTypeEnum.String: return val.AsString;
            }
            return null; // TODO add casting scope to post etc
        }

        public Value GetValue(object obj)
        {
            return Value.Get(obj);
        }
    }
}
