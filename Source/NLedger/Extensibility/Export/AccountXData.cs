using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class AccountXData : BaseExport<Accounts.AccountXData>
    {
        public static uint ACCOUNT_EXT_SORT_CALC = 0x01;
        public static uint ACCOUNT_EXT_HAS_NON_VIRTUALS = 0x02;
        public static uint ACCOUNT_EXT_HAS_UNB_VIRTUALS = 0x04;
        public static uint ACCOUNT_EXT_AUTO_VIRTUALIZE = 0x08;
        public static uint ACCOUNT_EXT_VISITED = 0x10;
        public static uint ACCOUNT_EXT_MATCHING = 0x20;
        public static uint ACCOUNT_EXT_TO_DISPLAY = 0x40;
        public static uint ACCOUNT_EXT_DISPLAYED = 0x80;

        public static implicit operator AccountXData(Accounts.AccountXData xdata) => new AccountXData(xdata);

        protected AccountXData(Accounts.AccountXData origin) : base(origin)
        { }

        public uint flags { get => Flags.Value.GetFlags(Origin); set => Flags.Value.SetFlags(Origin, value); }

        public bool has_flags(uint flag) => Flags.Value.HasFlags(Origin, flag);
        public void clear_flags() => Flags.Value.ClearFlags(Origin);
        public void add_flags(uint flag) => Flags.Value.AddFlags(Origin, flag);
        public void drop_flags(uint flag) => Flags.Value.DropFlags(Origin, flag);

        public AccountXDataDetails self_details => Origin.SelfDetails;
        public AccountXDataDetails family_details => Origin.FamilyDetails;
        public IList<Post> reported_posts => Origin.ReportedPosts.Select(p => (Post)p).ToList();
        public IList<SortValue> sort_values => Origin.SortValues.Select(t => new SortValue(t)).ToList();

        public static Lazy<FlagsConverter<Accounts.AccountXData>> Flags = new Lazy<FlagsConverter<Accounts.AccountXData>>(() =>
        {
            return new FlagsConverter<Accounts.AccountXData>().
                AddMapping(ACCOUNT_EXT_SORT_CALC, a => a.SortCalc, (a, v) => a.SortCalc = v).
                AddMapping(ACCOUNT_EXT_HAS_NON_VIRTUALS, a => a.HasNonVirtuals, (a, v) => a.HasNonVirtuals = v).
                AddMapping(ACCOUNT_EXT_HAS_UNB_VIRTUALS, a => a.HasUnbVirtuals, (a, v) => a.HasUnbVirtuals = v).
                AddMapping(ACCOUNT_EXT_AUTO_VIRTUALIZE, a => a.AutoVirtualize, (a, v) => a.AutoVirtualize = v).
                AddMapping(ACCOUNT_EXT_VISITED, a => a.Visited, (a, v) => a.Visited = v).
                AddMapping(ACCOUNT_EXT_MATCHING, a => a.Matching, (a, v) => a.Matching = v).
                AddMapping(ACCOUNT_EXT_TO_DISPLAY, a => a.ToDisplay, (a, v) => a.ToDisplay = v).
                AddMapping(ACCOUNT_EXT_DISPLAYED, a => a.Displayed, (a, v) => a.Displayed = v);
        }, true);

    }
}
