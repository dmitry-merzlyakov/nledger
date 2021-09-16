using NLedger.Utility;
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
        public static explicit operator bool(Annotation annotation) => (bool)annotation.Origin;         // .def("__nonzero__", &annotation_t::operator bool)
        public static bool operator ==(Annotation annLeft, Annotation annRight) => annLeft.Origin == annRight.Origin;
        public static bool operator !=(Annotation annLeft, Annotation annRight) => annLeft.Origin != annRight.Origin;

        protected Annotation(Annotate.Annotation origin) : base(origin)
        { }

        public uint flags { get => Flags.Value.GetFlags(Origin); set => Flags.Value.SetFlags(Origin, value); }

        public bool has_flags(uint flag) => Flags.Value.HasFlags(Origin, flag);
        public void clear_flags() => Flags.Value.ClearFlags(Origin);
        public void add_flags(uint flag) => Flags.Value.AddFlags(Origin, flag);
        public void drop_flags(uint flag) => Flags.Value.DropFlags(Origin, flag);

        public Amount price { get => Origin.Price; set => Origin.Price = value.Origin; }
        public DateTime? date { get => Origin.Date; set => Origin.Date = (Date)value; }
        public string tag { get => Origin.Tag; set => Origin.Tag = value; }
        public bool valid() => true;

        public override bool Equals(object obj) => Origin.Equals((obj as Annotation)?.Origin);
        public override int GetHashCode() => Origin.GetHashCode();


        private static Lazy<FlagsConverter<Annotate.Annotation>> Flags = new Lazy<FlagsConverter<Annotate.Annotation>>(FlagsAdapter.AnnotationFlagsAdapter, true);
    }
}
