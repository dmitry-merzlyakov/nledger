using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class Annotation : BaseExport<Annotate.Annotation>
    {
        public static implicit operator Annotation(Annotate.Annotation annotation) => new Annotation(annotation);

        protected Annotation(Annotate.Annotation origin) : base(origin)
        { }

    }
}
