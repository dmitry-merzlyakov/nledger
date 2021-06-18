using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class Scope : BaseExport<Scopus.Scope>
    {
        public static implicit operator Scope(Scopus.Scope scope) => new Scope(scope);

        protected Scope(Scopus.Scope origin) : base(origin)
        { }

        // TBC
    }
}
