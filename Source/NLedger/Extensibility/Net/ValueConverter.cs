// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using NLedger.Scopus;
using NLedger.Utility;
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
            if (Value.IsNullOrEmpty(val))
                return null;

            switch (val.Type)
            {
                case ValueTypeEnum.Amount: return val.AsAmount;
                case ValueTypeEnum.Any: return val.AsAny();
                case ValueTypeEnum.Balance: return val.AsBalance;
                case ValueTypeEnum.Boolean: return val.AsBoolean;
                case ValueTypeEnum.Date: return val.AsDate;
                case ValueTypeEnum.DateTime: return val.AsDateTime;
                case ValueTypeEnum.Integer: return ReduceLong(val.AsLong);
                case ValueTypeEnum.Mask: return val.AsMask;
                case ValueTypeEnum.String: return val.AsString;
                case ValueTypeEnum.Scope: return val.AsScope;
                case ValueTypeEnum.Sequence: return val.AsSequence;
            }
            return null;
        }

        public Value GetValue(object obj)
        {
            if (obj == null)
                return new Value();

            var type = obj.GetType();
            if (type == typeof(Boolean))
                return Value.Get((bool)obj);
            else if (type == typeof(Date))
                return Value.Get((Date)obj);
            else if (type == typeof(DateTime))
                return Value.Get((DateTime)obj);
            else if (type == typeof(DateTime?))
                return Value.Get((DateTime?)obj);
            else if (type == typeof(int))
                return Value.Get((int)obj);
            else if (type == typeof(long))
                return Value.Get((long)obj);
            else if (type == typeof(Amount))
                return Value.Get((Amount)obj);
            else if (type == typeof(Balance))
                return Value.Get((Balance)obj);
            else if (type == typeof(String))
                return Value.Get((String)obj);
            else if (type == typeof(Mask))
                return Value.Get((Mask)obj);
            else if (typeof(Scope).IsAssignableFrom(type))
                return Value.Get((Scope)obj);
            else if (typeof(IList<Value>).IsAssignableFrom(type))
                return Value.Get((IList<Value>)obj);
            else
                return Value.Get(obj);
        }

        public static object ReduceLong(long value)
        {
            if (value > Int32.MaxValue || value < Int32.MinValue)
                return value;
            else
                return (int)value;
        }
    }
}
