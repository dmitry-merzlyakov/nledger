using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class SortValue : BaseExport<Tuple<Values.Value,bool>>
    {
        public SortValue(Tuple<Values.Value, bool> origin) : base(origin)
        { }

        public Value value => Origin.Item1;
        public bool inverted => Origin.Item2;
    }
}
