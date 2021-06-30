using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class PeriodicTransaction : TransactionBase
    {
        public static implicit operator PeriodicTransaction(Xacts.PeriodXact xact) => new PeriodicTransaction(xact);

        protected PeriodicTransaction(Xacts.PeriodXact origin) : base(origin)
        {
            Origin = origin;
        }

        public new Xacts.PeriodXact Origin { get; }

        public NLedger.Times.DateInterval period => Origin.Period;
        public string period_string => Origin.PeriodSting;
    }
}
