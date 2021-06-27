using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class Journal : BaseExport<Journals.Journal>, IEnumerable<Transaction>
    {
        public static implicit operator Journal(Journals.Journal journal) => new Journal(journal);

        protected Journal(Journals.Journal origin) : base(origin)
        { }

        public Account master { get => Origin.Master; set => Origin.Master = value.Origin; }
        public Account bucket { get => Origin.Bucket; set => Origin.Bucket = value.Origin; }
        // [DM] This property is not complete in original code. // public bool was_loaded => Origin.WasLoaded;

        public void add_account(Account acct) => Origin.AddAccount(acct.Origin);
        public bool remove_account(Account acct) => Origin.RemoveAccount(acct.Origin);

        public Account find_account(string name) => Origin.FindAccount(name);
        public Account find_account(string name, bool autoCreate) => Origin.FindAccount(name, autoCreate);
        public Account find_account_re(string regexp) => Origin.FindAccountRe(regexp);

        public Account register_account(string name, Posting post) => Origin.RegisterAccount(name, post?.Origin, Origin.Master);
        public Account expand_aliases(string name) => Origin.ExpandAliases(name);

        public void add_xact(Transaction xact) => Origin.AddXact(xact.Origin);
        public bool remove_xact(Transaction xact) => Origin.RemoveXact(xact.Origin);

        public IEnumerator<Transaction> GetEnumerator() => xacts().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public IEnumerable<Transaction> xacts() => Origin.Xacts.Select(x => (Transaction)x).ToList();
        public IEnumerable<AutomatedTransaction> auto_xacts() => Origin.AutoXacts.Select(x => (AutomatedTransaction)x).ToList();
        public IEnumerable<PeriodicTransaction> period_xacts() => Origin.PeriodXacts.Select(x => (PeriodicTransaction)x).ToList();
        public IEnumerable<FileInfo> sources() => Origin.Sources.Select(x => (FileInfo)x).ToList();

        public bool has_xdata() => Origin.HasXData();
        public void clear_xdata() => Origin.ClearXData();

        // TBC
        // query

        public bool valid() => Origin.Valid();
    }
}
