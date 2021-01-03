// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Chain;
using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Filters
{
    /// <summary>
    /// Ported from post_splitter
    /// </summary>
    public class PostSplitter : PostHandler
    {
        public PostSplitter(PostHandler postChain, Report report, Expr groupByExpr)
            : base(postChain)
        {
            PostChain = postChain;
            Report = report;
            GroupByExpr = groupByExpr;
            PreFlushFunc = x => PrintTitle(x);

            PostsMap = new SortedDictionary<Value, IList<Post>>(DefaultValueComparer.Instance);
        }

        public IDictionary<Value, IList<Post>> PostsMap { get; private set; }
        public PostHandler PostChain { get; private set; }
        public Report Report { get; set; }
        public Expr GroupByExpr { get; private set; }
        public Action<Value> PreFlushFunc { get; set; }
        public Action<Value> PostFlushFunc { get; set; }

        public virtual void PrintTitle(Value val)
        {
            if (!Report.NoTitlesHandler.Handled)
                PostChain.Title(val.Print());
        }

        /// <summary>
        /// Ported from void post_splitter::flush()
        /// </summary>
        public override void Flush()
        {
            foreach(KeyValuePair<Value, IList<Post>> pair in PostsMap)
            {
                PreFlushFunc(pair.Key);

                foreach (Post post in pair.Value)
                    PostChain.Handle(post);

                PostChain.Flush();
                PostChain.Clear();

                if (PostFlushFunc != null)
                    PostFlushFunc(pair.Key);
            }
        }

        public override void Handle(Post post)
        {
            BindScope boundScope = new BindScope(Report, post);
            Value result = GroupByExpr.Calc(boundScope);

            if (!Value.IsNullOrEmpty(result))
            {
                IList<Post> posts;
                if (PostsMap.TryGetValue(result, out posts))
                    posts.Add(post);
                else
                    PostsMap.Add(result, new List<Post>() { post });
            }
        }

        public override void Clear()
        {
            PostsMap.Clear();
            PostChain.Clear();
            base.Clear();
        }
        
    }
}
