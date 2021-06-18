using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class AccountXDataDetails : BaseExport<Accounts.AccountXDataDetails>
    {
        public static implicit operator AccountXDataDetails(Accounts.AccountXDataDetails details) => new AccountXDataDetails(details);

        protected AccountXDataDetails(Accounts.AccountXDataDetails origin) : base(origin)
        { }

        public Value total => Origin.Total;
        public bool calculated => Origin.Calculated;
        public bool gathered => Origin.Gathered;
        public int posts_count => Origin.PostsCount;
        public int posts_virtuals_count => Origin.PostsVirtualsCount;
        public int posts_cleared_count => Origin.PostsClearedCount;
        public int posts_last_7_count => Origin.PostsLast7Count;
        public int posts_last_30_count => Origin.PostsLast30Count;
        public int posts_this_month_count => Origin.PostsThisMountCount;

        public Date earliest_post => Origin.EarliestPost;
        public Date earliest_cleared_post => Origin.EarliestClearedPost;
        public Date latest_post => Origin.LatestPost;
        public Date latest_cleared_post => Origin.LatestClearedPost;

        public IEnumerable<string> filenames => Origin.Filenames.ToList();
        public IEnumerable<string> accounts_referenced => Origin.AccountsReferenced.ToList();
        public IEnumerable<string> payees_referenced => Origin.PayeesReferenced.ToList();

        public void update(Post post) => Origin.Update(post.Origin);
        public void update(Post post, bool gatherAll) => Origin.Update(post.Origin, gatherAll);
    }
}
