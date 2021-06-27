using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class Account : BaseExport<Accounts.Account>, IEnumerable<Account>
    {
        public static readonly uint ACCOUNT_NORMAL = 0x00;
        public static readonly uint ACCOUNT_KNOWN = 0x01;
        public static readonly uint ACCOUNT_TEMP = 0x02;
        public static readonly uint ACCOUNT_GENERATED = 0x02;

        public static implicit operator Account(Accounts.Account xdata) => new Account(xdata);

        protected Account(Accounts.Account origin) : base(origin)
        { }

        public uint flags { get => Flags.Value.GetFlags(Origin); set => Flags.Value.SetFlags(Origin, value); }

        public bool has_flags(uint flag) => Flags.Value.HasFlags(Origin, flag);
        public void clear_flags() => Flags.Value.ClearFlags(Origin);
        public void add_flags(uint flag) => Flags.Value.AddFlags(Origin, flag);
        public void drop_flags(uint flag) => Flags.Value.DropFlags(Origin, flag);

        public Account parent => Origin.Parent;
        public string name => Origin.Name;
        public int depth => Origin.Depth;

        public string fullname() => Origin.FullName;
        public string partial_name() => Origin.PartialName();
        public string partial_name(bool flat) => Origin.PartialName(flat);

        public void add_account(Account account) => Origin.AddAccount(account.Origin);
        public bool remove_account(Account account) => Origin.RemoveAccount(account.Origin);

        public Account find_account(string acctName) => Origin.FindAccount(acctName);
        public Account find_account(string acctName, bool autoCreate) => Origin.FindAccount(acctName, autoCreate);
        public Account find_account_re(string regexp) => Origin.FindAccountRe(regexp);

        public bool valid() => Origin.Valid();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        public IEnumerator<Account> GetEnumerator() => accounts().GetEnumerator();
        public IList<Account> accounts() => Origin.Accounts.Values.Select(a => (Account)a).ToList();
        public IList<Posting> posts() => Origin.Posts.Select(p => (Posting)p).ToList();

        public bool has_xdata() => Origin.HasXData;
        public void clear_xdata() => Origin.ClearXData();
        public AccountXData xdata() => Origin.XData;

        public Value amount() => Origin.Amount();
        public Value amount(Expr expr) => Origin.Amount(false, expr.Origin);
        public Value total() => Origin.Total();
        public Value total(Expr expr) => Origin.Total(expr.Origin);

        public AccountXDataDetails self_details() => Origin.SelfDetails();
        public AccountXDataDetails self_details(bool gatherAll) => Origin.SelfDetails(gatherAll);
        public AccountXDataDetails family_details() => Origin.FamilyDetails();
        public AccountXDataDetails family_details(bool gatherAll) => Origin.FamilyDetails(gatherAll);

        public bool has_xflags(uint flags) => Origin.HasXFlags(axd => AccountXData.Flags.Value.HasFlags(axd, flags));
        public int children_with_flags(bool toDisplay, bool visited) => Origin.ChildrenWithFlags(toDisplay, visited);

        public override string ToString() => Origin.ToString();

        private static Lazy<FlagsConverter<Accounts.Account>> Flags = new Lazy<FlagsConverter<Accounts.Account>>(() =>
        {
            return new FlagsConverter<Accounts.Account>().
                AddMapping(ACCOUNT_KNOWN, a => a.IsKnownAccount, (a, v) => a.IsKnownAccount = v).
                AddMapping(ACCOUNT_TEMP, a => a.IsTempAccount, (a, v) => a.IsTempAccount = v).
                AddMapping(ACCOUNT_GENERATED, a => a.IsGeneratedAccount, (a, v) => a.IsGeneratedAccount = v);
        }, true);

    }
}
