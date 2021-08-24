using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class PostingXData : BaseExport<NLedger.PostXData>
    {
        public static uint POST_EXT_RECEIVED = 0x001;
        public static uint POST_EXT_HANDLED = 0x002;
        public static uint POST_EXT_DISPLAYED = 0x004;
        public static uint POST_EXT_DIRECT_AMT = 0x008;
        public static uint POST_EXT_SORT_CALC = 0x010;
        public static uint POST_EXT_COMPOUND = 0x020;
        public static uint POST_EXT_VISITED = 0x040;
        public static uint POST_EXT_MATCHES = 0x080;
        public static uint POST_EXT_CONSIDERED = 0x100;

        public static implicit operator PostingXData(NLedger.PostXData postXData) => new PostingXData(postXData);

        protected PostingXData(NLedger.PostXData postXData) : base(postXData)
        { }

        public uint flags { get => Flags.Value.GetFlags(Origin); set => Flags.Value.SetFlags(Origin, value); }

        public bool has_flags(uint flag) => Flags.Value.HasFlags(Origin, flag);
        public void clear_flags() => Flags.Value.ClearFlags(Origin);
        public void add_flags(uint flag) => Flags.Value.AddFlags(Origin, flag);
        public void drop_flags(uint flag) => Flags.Value.DropFlags(Origin, flag);

        public Value visited_value { get => Origin.VisitedValue; set => Origin.VisitedValue = value?.Origin; }
        public Value compound_value { get => Origin.CompoundValue; set => Origin.CompoundValue = value?.Origin; }
        public Value total { get => Origin.Total; set => Origin.Total = value?.Origin; }
        public int count { get => Origin.Count; set => Origin.Count = value; }
        public Date date { get => Origin.Date; set => Origin.Date = value; }
        public DateTime datetime { get => Origin.Datetime; set => Origin.Datetime = value; }
        public Account account { get => Origin.Account; set => Origin.Account = value.Origin; }
        public IEnumerable<SortValue> sort_values => Origin?.SortValues?.Select(x => new SortValue(x)).ToList(); // [DM] Implemented as read-only

        private static Lazy<FlagsConverter<NLedger.PostXData>> Flags = new Lazy<FlagsConverter<NLedger.PostXData>>(FlagsAdapter.PostXDataFlagsAdapter, true);

    }
}
