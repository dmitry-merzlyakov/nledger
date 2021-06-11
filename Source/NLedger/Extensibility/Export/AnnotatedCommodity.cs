using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class AnnotatedCommodity : Commodity
    {
        public static bool operator ==(AnnotatedCommodity annLeft, AnnotatedCommodity annRight) => annLeft.Origin == annRight.Origin;
        public static bool operator !=(AnnotatedCommodity annLeft, AnnotatedCommodity annRight) => annLeft.Origin != annRight.Origin;

        public static bool operator ==(AnnotatedCommodity annLeft, Commodity commRight) => annLeft.Origin == commRight.Origin;
        public static bool operator !=(AnnotatedCommodity annLeft, Commodity commRight) => annLeft.Origin != commRight.Origin;

        protected AnnotatedCommodity(Annotate.AnnotatedCommodity origin) : base(origin)
        {
            Origin = origin;
        }

        public new Annotate.AnnotatedCommodity Origin { get; }

        public Annotation details { get => Origin.Details; set => Origin.SetDetails(value.Origin); }
        public override Commodity referent => Origin.Referent;

        public override Commodity strip_annotations() => Origin.StripAnnotations(new Annotate.AnnotationKeepDetails());
        public override Commodity strip_annotations(KeepDetails keep) => Origin.StripAnnotations(keep.Origin);

        public override bool Equals(object obj) => Origin.Equals((obj as AnnotatedCommodity)?.Origin ?? (obj as Commodity)?.Origin);
        public override int GetHashCode() => Origin.GetHashCode();
        
    }
}
