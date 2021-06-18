using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class Mask : BaseExport<NLedger.Mask>
    {
        public static implicit operator Mask(NLedger.Mask mask) => new Mask(mask);

        protected Mask(NLedger.Mask origin) : base(origin)
        { }

        public Mask(string pattern) : base(new NLedger.Mask(pattern))
        { }

        public bool match(string text) => Origin.Match(text);
        public bool is_empty => Origin.IsEmpty;
        public string str() => Origin.Str();

        public override string ToString()
        {
            return Origin.ToString();
        }
    }
}
