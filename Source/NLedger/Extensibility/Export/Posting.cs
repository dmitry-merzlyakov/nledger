using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class Posting : JournalItem
    {
        public static readonly uint POST_VIRTUAL = (uint)SupportsFlagsEnum.POST_VIRTUAL;
        public static readonly uint POST_MUST_BALANCE = (uint)SupportsFlagsEnum.POST_MUST_BALANCE;
        public static readonly uint POST_CALCULATED = (uint)SupportsFlagsEnum.POST_CALCULATED;
        public static readonly uint POST_COST_CALCULATED = (uint)SupportsFlagsEnum.POST_COST_CALCULATED;

        public static implicit operator Posting(NLedger.Post post) => new Posting(post);

        protected Posting(NLedger.Post origin) : base(origin)
        {
            Origin = origin;
        }

        public new NLedger.Post Origin { get; }

        public string id() => Origin.Id;
        public long seq() => Origin.Seq;

        public Transaction xact { get => Origin.Xact; set => Origin.Xact = value.Origin; }
        public Account account { get => Origin.Account; set => Origin.Account = value.Origin; }
        public Amount amount { get => Origin.Amount; set => Origin.Amount = value.Origin; }
        public Amount cost { get => Origin.Cost; set => Origin.Cost = value.Origin; }
        public Amount given_cost { get => Origin.GivenCost; set => Origin.GivenCost = value.Origin; }
        public Amount assigned_amount { get => Origin.AssignedAmount; set => Origin.AssignedAmount = value.Origin; }

        public bool must_balance() => Origin.MustBalance;

        public bool has_xdata() => Origin.HasXData;
        public void clear_xdata() => Origin.ClearXData();
        public PostingXData xdata() => Origin.XData;

        public void set_reported_account(Account account) => Origin.ReportedAccount = account?.Origin;
        public Account reported_account() => Origin.ReportedAccount;

    }
}
