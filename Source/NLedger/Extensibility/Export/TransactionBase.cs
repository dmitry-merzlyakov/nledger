using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class TransactionBase : JournalItem, IEnumerable<Posting>
    {
        public static implicit operator TransactionBase(Xacts.XactBase xbase) => new TransactionBase(xbase);

        protected TransactionBase(Xacts.XactBase origin) : base(origin)
        {
            Origin = origin;
        }

        public new Xacts.XactBase Origin { get; }

        public Journal journal { get => Origin.Journal; set => Origin.Journal = value?.Origin; }

        public void add_post(Posting post) => Origin.AddPost(post?.Origin);
        public bool remove_post(Posting post) => Origin.RemovePost(post?.Origin);
        public bool finalize() => Origin.FinalizeXact();
        public IEnumerable<Posting> posts() => Origin.Posts.Select(p => new Posting(p)).ToList();

        public IEnumerator<Posting> GetEnumerator() => posts().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
