using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class Transaction : TransactionBase
    {
        public static implicit operator Transaction(Xacts.Xact xact) => new Transaction(xact);

        protected Transaction(Xacts.Xact origin) : base(origin)
        {
            Origin = origin;
        }

        public new Xacts.Xact Origin { get; }

        public string id() => Origin.Id;
        public long seq() => Origin.Seq;

        public string code { get => Origin.Code; set => Origin.Code = value; }
        public string payee { get => Origin.Payee; set => Origin.Payee = value; }

        public Value magnitude() => Origin.Magnitude();
        public bool has_xdata() => Origin.HasXData;
        public void clear_xdata() => Origin.ClearXData();

        // See py_xact.cc, py_xact_to_string, jww (2012-03-01)
        public override string ToString() => String.Empty;
    }
}
