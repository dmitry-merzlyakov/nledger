using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class Session : Scope
    {
        public static implicit operator Session(Scopus.Session session) => new Session(session);

        protected Session(Scopus.Session origin) : base(origin)
        {
            Origin = origin;
        }

        public new Scopus.Session Origin { get; }

        public Journal read_journal(string pathName) => Origin.ReadJournal(pathName);
        public Journal read_journal_from_string(string data) => Origin.ReadJournalFromString(data);
        public Journal read_journal_files() => Origin.ReadJournalFiles();
        public void close_journal_files() => Origin.CloseJournalFiles();
        public Journal journal() => Origin.Journal;
    }

    public static class SessionScopeAttributes
    {
        public static Session session => ExtendedSession.Current;
        public static Journal read_journal(string pathName) => ExtendedSession.Current.ReadJournal(pathName);
        public static Journal read_journal_from_string(string data) => ExtendedSession.Current.ReadJournalFromString(data);
    }
}
