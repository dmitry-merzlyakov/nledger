using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class AccountXData : BaseExport<Accounts.AccountXData>
    {
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
        public IList<Posting> reported_posts => Origin.ReportedPosts.Select(p => (Posting)p).ToList();
        public IList<SortValue> sort_values => Origin.SortValues.Select(t => new SortValue(t)).ToList();

        public static Lazy<FlagsConverter<Accounts.AccountXData>> Flags = new Lazy<FlagsConverter<Accounts.AccountXData>>(FlagsAdapter.AccountXDataFlagsAdapter, true);

    }
}
