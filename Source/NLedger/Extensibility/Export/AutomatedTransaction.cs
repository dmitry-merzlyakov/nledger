using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class AutomatedTransaction : TransactionBase
    {
        public static implicit operator AutomatedTransaction(Xacts.AutoXact xact) => new AutomatedTransaction(xact);

        protected AutomatedTransaction(Xacts.AutoXact origin) : base(origin)
        {
            Origin = origin;
        }

        public new Xacts.AutoXact Origin { get; }

        public Predicate predicate => Origin.Predicate;

        public void extend_xact(TransactionBase xact_base) => Origin.ExtendXact(xact_base.Origin, null);
    }
}
