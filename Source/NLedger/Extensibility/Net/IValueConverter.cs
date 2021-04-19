using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Net
{
    public interface IValueConverter
    {
        object GetObject(Value val);
        Value GetValue(object obj);
    }
}
