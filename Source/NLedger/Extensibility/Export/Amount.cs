using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class Amount : BaseExport<Amounts.Amount>
    {
        public static implicit operator Amount(Amounts.Amount amount) => new Amount(amount);

        protected Amount(Amounts.Amount origin) : base(origin)
        { }

    }
}
