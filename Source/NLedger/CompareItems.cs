// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger
{
    /// <summary>
    /// Ported from compare_items
    /// </summary>
    public abstract class CompareItems<T> : IComparer<T>
    {
        public static void PushSortValue(IList<Tuple<Value,bool>> sortValues, ExprOp node, Scope scope)
        {
            if (node.Kind  == OpKindEnum.O_CONS)
            {
                while(node != null && node.Kind == OpKindEnum.O_CONS)
                {
                    PushSortValue(sortValues, node.Left, scope);
                    node = node.HasRight ? node.Right : null;
                }
            }
            else
            {
                bool inverted = false;

                if (node.Kind == OpKindEnum.O_NEG)
                {
                    inverted = true;
                    node = node.Left;
                }

                Value value = new Expr(node).Calc(scope).Simplified();

                if (Value.IsNullOrEmpty(value))
                    throw new CalcError(CalcError.ErrorMessageCouldNotDetermineSortingValueBasedAnExpression);

                sortValues.Add(new Tuple<Value, bool>(value, inverted));
            }
        }

        public static int SortValueIsLessThan(IList<Tuple<Value,bool>> leftValues, IList<Tuple<Value,bool>> rightValues)
        {
            using(IEnumerator<Tuple<Value,bool>> left = leftValues.GetEnumerator())
            {
                using (IEnumerator<Tuple<Value, bool>> right = rightValues.GetEnumerator())
                {
                    while(left.MoveNext() && right.MoveNext())
                    {
                        Value leftVal = left.Current.Item1;
                        Value rightVal = right.Current.Item1;

                        // Don't even try to sort balance values
                        if (leftVal.Type != ValueTypeEnum.Balance && rightVal.Type != ValueTypeEnum.Balance)
                        {
                            if (leftVal.IsLessThan(rightVal))
                                return left.Current.Item2 ? -1 : 1;
                            if (leftVal.IsGreaterThan(rightVal))
                                return left.Current.Item2 ? 1 : -1;
                        }
                    }
                }
            }

            return 0;
        }

        public CompareItems(Expr sortOrder, Report report)
        {
            SortOrder = sortOrder;
            Report = report;
        }

        public Expr SortOrder { get; private set; }
        public Report Report { get; private set; }

        public void FindSortValues(IList<Tuple<Value,bool>> sortValues, Scope scope)
        {
            var boundScope = new BindScope(Report, scope);
            PushSortValue(sortValues, SortOrder.Op, boundScope);
        }

        public abstract int Compare(T x, T y);
    }

    public class ComparePosts : CompareItems<Post>
    {
        public ComparePosts(Expr sortOrder, Report report)
            : base(sortOrder, report)
        { }

        public override int Compare(Post left, Post right)
        {
            if (left == null)
                throw new ArgumentNullException("left");

            if (right == null)
                throw new ArgumentNullException("right");

            PostXData lXData = left.XData;
            if (!lXData.SortCalc)
            {
                BindScope boundScope = new BindScope(SortOrder.Context, left);
                FindSortValues(lXData.SortValues, boundScope);
                lXData.SortCalc = true;
            }

            PostXData rXData = right.XData;
            if (!rXData.SortCalc)
            {
                BindScope boundScope = new BindScope(SortOrder.Context, right);
                FindSortValues(rXData.SortValues, boundScope);
                rXData.SortCalc = true;
            }

            return - SortValueIsLessThan(lXData.SortValues, rXData.SortValues); // [DM] Invert result because it is used as IComparer output that requires opposite meaning
        }
    }

    public class CompareAccounts : CompareItems<Account>
    {
        public CompareAccounts(Expr sortOrder, Report report)
            : base(sortOrder, report)
        { }

        public override int Compare(Account left, Account right)
        {
            if (left == null)
                throw new ArgumentNullException("left");

            if (right == null)
                throw new ArgumentNullException("right");

            AccountXData lXData = left.XData;
            if (!lXData.SortCalc)
            {
                BindScope boundScope = new BindScope(SortOrder.Context, left);
                FindSortValues(lXData.SortValues, boundScope);
                lXData.SortCalc = true;
            }

            AccountXData rXData = right.XData;
            if (!rXData.SortCalc)
            {
                BindScope boundScope = new BindScope(SortOrder.Context, right);
                FindSortValues(rXData.SortValues, boundScope);
                rXData.SortCalc = true;
            }

            return - SortValueIsLessThan(lXData.SortValues, rXData.SortValues); // [DM] Invert result because it is used as IComparer output that requires opposite meaning
        }
    }

}
