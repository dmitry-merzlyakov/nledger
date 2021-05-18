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

        public const int ANNOTATION_PRICE_CALCULATED = 0x01;
        public const int ANNOTATION_PRICE_FIXATED = 0x02;
        public const int ANNOTATION_PRICE_NOT_PER_UNIT = 0x04;
        public const int ANNOTATION_DATE_CALCULATED = 0x08;
        public const int ANNOTATION_TAG_CALCULATED = 0x10;
        public const int ANNOTATION_VALUE_EXPR_CALCULATED = 0x20;

        protected Annotation(Annotate.Annotation origin) : base(origin)
        { }

        public uint flags { get => Flags.Value.GetFlags(Origin); set => Flags.Value.SetFlags(Origin, value); }

        public bool has_flags(uint flag) => Flags.Value.HasFlags(Origin, flag);
        public void clear_flags(uint flag) => Flags.Value.ClearFlags(Origin, flag);
        public void add_flags(uint flag) => Flags.Value.AddFlags(Origin, flag);
        public void drop_flags(uint flag) => Flags.Value.DropFlags(Origin, flag);

        private static Lazy<FlagsConverter<Annotate.Annotation>> Flags = new Lazy<FlagsConverter<Annotate.Annotation>>(() =>
        {
            return new FlagsConverter<Annotate.Annotation>().
                AddMapping(ANNOTATION_PRICE_CALCULATED, a => a.IsPriceCalculated, (a, v) => a.IsPriceCalculated = v).
                AddMapping(ANNOTATION_PRICE_FIXATED, a => a.IsPriceFixated, (a, v) => a.IsPriceFixated = v).
                AddMapping(ANNOTATION_PRICE_NOT_PER_UNIT, a => a.IsPriceNotPerUnit, (a, v) => a.IsPriceNotPerUnit = v).
                AddMapping(ANNOTATION_DATE_CALCULATED, a => a.IsDateCalculated, (a, v) => a.IsDateCalculated = v).  
                AddMapping(ANNOTATION_TAG_CALCULATED, a => a.IsTagCalculated, (a, v) => a.IsTagCalculated = v).
                AddMapping(ANNOTATION_VALUE_EXPR_CALCULATED, a => a.IsValueExprCalculated, (a, v) => a.IsValueExprCalculated = v);
        }, true);
    }
}
