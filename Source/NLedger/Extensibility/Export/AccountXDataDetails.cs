using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class AccountXDataDetails : BaseExport<Accounts.AccountXDataDetails>
    {
        protected AccountXDataDetails(Accounts.AccountXDataDetails origin) : base(origin)
        { }

        //public bool total => Origin.Total;
        //TBC
    }
}
