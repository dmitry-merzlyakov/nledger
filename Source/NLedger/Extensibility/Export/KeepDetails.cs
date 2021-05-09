using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class KeepDetails : BaseStructExport<Annotate.AnnotationKeepDetails>
    {
        public static implicit operator KeepDetails(Annotate.AnnotationKeepDetails keepDetails) => new KeepDetails(keepDetails);

        protected KeepDetails(Annotate.AnnotationKeepDetails origin) : base(origin)
        { }

        public KeepDetails(bool keepPrice = false, bool keepDate = false, bool keepTag = false, bool onlyActuals = false)
            : base(new Annotate.AnnotationKeepDetails(keepPrice, keepDate, keepTag, onlyActuals))
        { }

        public bool keep_price { get => Origin.KeepPrice; set => Origin.KeepPrice = value; }
        public bool keep_date { get => Origin.KeepDate; set => Origin.KeepDate = value; }
        public bool keep_tag { get => Origin.KeepTag; set => Origin.KeepTag = value; }
        public bool only_actuals { get => Origin.OnlyActuals; set => Origin.OnlyActuals = value; }

        public void keep_all() => Origin.KeepAll();
        public void keep_all(Commodity comm) => Origin.KeepAll(comm.Origin);
        public void keep_any() => Origin.KeepAny();
        public void keep_any(Commodity comm) => Origin.KeepAny(comm.Origin);
    }
}
