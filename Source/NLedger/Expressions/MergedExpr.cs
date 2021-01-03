// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Scopus;
using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Expressions
{
    /// <summary>
    /// A merged expression allows one to set an expression term, "foo", and
    /// a base expression, "bar", and then merge in later expressions that
    /// utilize foo.  For example:
    ///
    ///    foo: bar
    ///  merge: foo * 10
    ///  merge: foo + 20
    ///
    /// When this expression is finally compiled, the base and merged
    /// elements are written into this:
    ///
    ///   __tmp=(foo=bar; foo=foo*10; foo=foo+20);__tmp
    ///
    /// This allows users to select flags like -O, -B or -I at any time, and
    /// also combine flags such as -V and -A.
    /// </summary>
    /// <remarks>
    /// Ported from merged_expr_t
    /// </remarks>
    public class MergedExpr : Expr
    {
        public MergedExpr(string term, string expr, string mergeOp = ";")
        {
            Term = term;
            BaseExpr = expr;
            MergeOperator = mergeOp;
            Exprs = new List<string>();
        }

        public string Term { get; private set; }
        public string BaseExpr { get; set; }
        public string MergeOperator { get; private set; }
        public IList<string> Exprs { get; private set; }

        public bool CheckForSingleIdentifier(string expr)
        {
            bool singleIdentifier = expr.All(c => Char.IsLetterOrDigit(c) && c != '_');

            if (singleIdentifier)
            {
                BaseExpr = expr;
                Exprs.Clear();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Prepend(string expr)
        {
            if (!CheckForSingleIdentifier(expr))
                Exprs.Insert(0, expr);
        }

        public void Append(string expr)
        {
            if (!CheckForSingleIdentifier(expr))
                Exprs.Add(expr);
        }

        public void Remove(string expr)
        {
            Exprs.Remove(expr);
        }

        public override void Compile(Scope scope)
        {
            string exprStr;
            if (!Exprs.Any())
            {
                exprStr = BaseExpr;
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("__tmp_{0}=({0}=({1})", Term, BaseExpr);
                foreach(string expr in Exprs)
                {
                    if (MergeOperator == ";")
                        sb.AppendFormat(";{0}={1}", Term, expr);
                    else
                        sb.AppendFormat("{0}({1})", MergeOperator, expr);
                }
                sb.AppendFormat(";{0});__tmp_{0}", Term);

                exprStr = sb.ToString();
                Logger.Current.Debug("expr.merged.compile", () => String.Format("Compiled expr: {0}", exprStr));
            }
            Parse(exprStr);

            base.Compile(scope);
        }

    }
}
