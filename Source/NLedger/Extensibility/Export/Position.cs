using NLedger.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class Position : BaseExport<Items.ItemPosition>
    {
        public static implicit operator Position(Items.ItemPosition position) => new Position(position);

        public Position() : this(new ItemPosition())
        { }

        protected Position(ItemPosition origin) : base(origin)
        { }

        public string pathname { get => Origin.PathName; set => Origin.PathName = value; }
        public long beg_pos { get => Origin.BegPos; set => Origin.BegPos = value; }
        public int beg_line { get => Origin.BegLine; set => Origin.BegLine = value; }
        public int end_pos { get => Origin.EndPos; set => Origin.EndPos = value; }
        public int end_line { get => Origin.EndLine; set => Origin.EndLine = value; }
    }
}
