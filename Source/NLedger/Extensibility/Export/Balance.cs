using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class Balance : BaseExport<NLedger.Balance>
    {
        public static implicit operator Balance(NLedger.Balance balance) => new Balance(balance);

        protected Balance(NLedger.Balance origin) : base(origin)
        {  }

        // TBC
    }
}
