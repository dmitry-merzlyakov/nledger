using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class Post : BaseExport<NLedger.Post>
    {
        public static implicit operator Post(NLedger.Post post) => new Post(post);

        protected Post(NLedger.Post origin) : base(origin)
        { }

        // TBC
    }
}
