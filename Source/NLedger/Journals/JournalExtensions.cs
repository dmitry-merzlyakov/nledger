using NLedger.Filters;
using NLedger.Scopus;
using NLedger.Utility;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Journals
{
    public static class JournalExtensions
    {
        /// <summary>
        /// Ported from py_journal.cc - py_query(journal_t& journal...
        /// </summary>
        /// <param name="journal"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IEnumerable<Post> Query(this Journal journal, string query)
        {
            if (journal == null)
                throw new ArgumentNullException(nameof(journal));

            if (journal.HasXData())
                throw new RuntimeError("Cannot have more than one active journal query");

            var currentReport = (Report)Scope.DefaultScope;
            using (var coll = new CollectorWraper(journal, currentReport))
            {
                var remaining = Option.ProcessArguments(StringExtensions.SplitArguments(query), coll.Report);
                coll.Report.NormalizeOptions("register");

                var args = new Value();
                foreach (var arg in remaining)
                    args.PushBack(Value.StringValue(arg));
                coll.Report.ParseQueryArgs(args, "@Journal.query");

                currentReport.PostsReport(coll.CollectPosts);
                return coll.CollectPosts.Posts;
            }
        }

        /// <summary>
        /// Ported from py_journal.cc - collector_wrapper
        /// </summary>
        private class CollectorWraper : IDisposable
        {
            public CollectorWraper(Journal journal, Report report)
            {
                Journal = journal ?? throw new ArgumentNullException(nameof(journal));
                Report = new Report(report ?? throw new ArgumentNullException(nameof(report)));
                CollectPosts = new CollectPosts();
            }

            public Journal Journal { get; }
            public Report Report { get; }
            public CollectPosts CollectPosts { get; }

            public void Dispose()
            {
                Journal.ClearXData();
            }
        }
    }
}
