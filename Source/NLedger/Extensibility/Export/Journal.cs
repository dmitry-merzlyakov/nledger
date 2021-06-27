using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class Journal : BaseExport<Journals.Journal>// TBC, IEnumerable<Posting>
    {
        public static implicit operator Journal(Journals.Journal journal) => new Journal(journal);

        protected Journal(Journals.Journal origin) : base(origin)
        { }

        // TBC
    }
}
