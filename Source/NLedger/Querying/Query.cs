// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Annotate;
using NLedger.Expressions;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Querying
{
    public class Query
    {
        public Query()
        {
            /* Obsolete member
            Predicates = new Dictionary<QueryKindEnum, string>(); */
        }

        public Query(string arg, AnnotationKeepDetails whatToKeep = default(AnnotationKeepDetails), bool multipleArgs = true)
            : this()
        {
            if (!String.IsNullOrEmpty(arg))
            {
                Value temp = Value.Get(arg);
                ParseArgs(Value.Get(temp.AsSequence), whatToKeep, multipleArgs);
            }
        }

        public Query(Value args, AnnotationKeepDetails whatToKeep = default(AnnotationKeepDetails), bool multipleArgs = true)
            : this()
        {
            if (!Value.IsNullOrEmpty(args))
                ParseArgs(args, whatToKeep, multipleArgs);
        }

        public QueryParser Parser { get; private set; }
        /* Obsolete member; is not used in code
        public IDictionary<QueryKindEnum, string> Predicates { get; private set; } */

        public bool TokensRemaining
        {
            get { return Parser != null && Parser.TokensRemaining; }
        }

        public ExprOp ParseArgs(Value args, AnnotationKeepDetails whatToKeep = default(AnnotationKeepDetails), bool multipleArgs = true, bool subExpression = false)
        {
            if (Parser == null)
                Parser = new QueryParser(args, whatToKeep, multipleArgs);

            return Parser.Parse(subExpression);
        }

        public bool HasQuery(QueryKindEnum id)
        {
            return Parser != null && Parser.QueryMap.ContainsKey(id);
        }

        public string GetQuery(QueryKindEnum id)
        {
            if (Parser != null)
            {
                string str;
                Parser.QueryMap.TryGetValue(id, out str);
                return str ?? String.Empty;
            }
            return String.Empty;
        }

    }
}
