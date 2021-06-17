using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class Account : BaseExport<Accounts.Account>
    {
        protected Account(Accounts.Account origin) : base(origin)
        { }

        // TBC
    }
}
