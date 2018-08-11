// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Amounts;
using NLedger.Expressions;
using NLedger.Formatting;
using NLedger.Items;
using NLedger.Scopus;
using NLedger.Textual;
using NLedger.Utility;
using NLedger.Utils;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NLedger.Xacts
{
    /// <summary>
    /// Ported from auto_xact_t (xact.h)
    /// </summary>
    public class AutoXact : XactBase
    {
        public const string GeneratedAutomatedTransactionKey = "generated automated transaction";

        public AutoXact()
        {
            TryQuickMatch = true;
            MemoizedResults = new Dictionary<string, bool>();
        }

        public AutoXact(Predicate predicate)
            : this()
        {
            Predicate = predicate;
        }

        public bool TryQuickMatch { get; set; }
        public IDictionary<string, bool> MemoizedResults { get; private set; }
        public IList<DeferredTagData> DeferredNotes { get; private set; }
        public IList<CheckExprPair> CheckExprs { get; set; }
        public Predicate Predicate { get; private set; }
        public Post ActivePost { get; set; }

        public override string Description
        {
            get { return HasPos ? String.Format("automated transaction at line {0}", Pos.BegLine) : GeneratedAutomatedTransactionKey; }
        }

        public static bool PostPred(ExprOp op, Post post)
        {
            switch (op.Kind)
            {
                case OpKindEnum.VALUE:
                    return op.AsValue.AsBoolean;

                case OpKindEnum.O_MATCH:
                    if (op.Left.Kind == OpKindEnum.IDENT && op.Left.AsIdent == "account" &&
                        op.Right.Kind == OpKindEnum.VALUE && op.Right.AsValue.Type == ValueTypeEnum.Mask)
                        return op.Right.AsValue.AsMask.Match(post.ReportedAccount.FullName);
                    else
                        break;

                case OpKindEnum.O_EQ:
                    return PostPred(op.Left, post) == PostPred(op.Right, post);

                case OpKindEnum.O_NOT:
                    return !PostPred(op.Left, post);

                case OpKindEnum.O_AND:
                    return PostPred(op.Left, post) && PostPred(op.Right, post);

                case OpKindEnum.O_OR:
                    return PostPred(op.Left, post) || PostPred(op.Right, post);

                case OpKindEnum.O_QUERY:
                    if (PostPred(op.Left, post))
                        return PostPred(op.Right.Left, post);
                    else
                        return PostPred(op.Right.Right, post);

                default:
                    break;
            }

            throw new CalcError(CalcError.ErrorMessageUnhandledOperator);
        }

        public static string ApplyFormat(string str, Scope scope)
        {
            if (str.Contains("%("))
            {
                Format format = new Format(str);
                return format.Calc(scope);
            }
            else
            {
                return str;
            }
        }

        public void ExtendXact(XactBase xact, ParseContext context)
        {
            IList<Post> initialPosts = xact.Posts.ToList();

            try
            {
                bool needsFurtherVerification = false;

                foreach(Post initialPost in initialPosts)
                {
                    if (initialPost.Flags.HasFlag(SupportsFlagsEnum.ITEM_GENERATED))
                        continue;

                    BindScope boundScope = new BindScope(Scope.DefaultScope, initialPost);

                    bool matchesPredicate = false;
                    if (TryQuickMatch)
                    {
                        try
                        {
                            bool foundMemoizedResult = MemoizedResults.TryGetValue(initialPost.Account.FullName, out matchesPredicate);

                            // Since the majority of people who use automated transactions simply
                            // match against account names, try using a *much* faster version of
                            // the predicate evaluator.
                            if (!foundMemoizedResult)
                            {
                                matchesPredicate = PostPred(Predicate.Op, initialPost);
                                MemoizedResults.Add(initialPost.Account.FullName, matchesPredicate);
                            }
                        }
                        catch
                        {
                            Logger.Current.Debug("xact.extend.fail", () => "The quick matcher failed, going back to regular eval");
                            TryQuickMatch = false;
                            matchesPredicate = Predicate.Calc(boundScope).AsBoolean;
                        }
                    }
                    else
                    {
                        matchesPredicate = Predicate.Calc(boundScope).AsBoolean;
                    }

                    if (matchesPredicate)
                    {
                        if (DeferredNotes != null)
                        {
                            foreach(DeferredTagData data in DeferredNotes)
                            {
                                if (data.ApplyToPost == null)
                                    initialPost.AppendNote(ApplyFormat(data.TagData, boundScope), boundScope, data.OverwriteExisting);
                            }
                        }

                        if (CheckExprs != null)
                        {
                            foreach(CheckExprPair pair in CheckExprs)
                            {
                                if (pair.CheckExprKind == CheckExprKindEnum.EXPR_GENERAL)
                                {
                                    pair.Expr.Calc(boundScope);
                                }
                                else if (!pair.Expr.Calc(boundScope).AsBoolean)
                                {
                                    if (pair.CheckExprKind == CheckExprKindEnum.EXPR_ASSERTION)
                                        throw new ParseError(String.Format(ParseError.ParseError_TransactionAssertionFailed, pair.Expr));
                                    else
                                        context.Warning(String.Format(ParseError.ParseError_TransactionCheckFailed, pair.Expr));
                                }
                            }
                        }

                        foreach(Post post in Posts)
                        {
                            Amount postAmount;
                            if (post.Amount.IsEmpty)
                            {
                                if (post.AmountExpr == null)
                                    throw new AmountError(AmountError.ErrorMessageAutomatedTransactionsPostingHasNoAmount);

                                Value result = post.AmountExpr.Calc(boundScope);
                                if (result.Type == ValueTypeEnum.Integer)
                                {
                                    postAmount = result.AsAmount;
                                }
                                else
                                {
                                    if (result.Type != ValueTypeEnum.Amount)
                                        throw new AmountError(AmountError.ErrorMessageAmountExpressionsMustResultInASimpleAmount);
                                    postAmount = result.AsAmount;
                                }

                            }
                            else
                            {
                                postAmount = post.Amount;
                            }

                            Amount amt;
                            if (!postAmount.HasCommodity)
                                amt = initialPost.Amount * postAmount;
                            else
                                amt = postAmount;

                            Account account = post.Account;
                            string fullName = account.FullName;
                            if (String.IsNullOrEmpty(fullName))
                                throw new InvalidOperationException("fullName");

                            if (Logger.Current.ShowDebug("xact.extend"))
                            {
                                Logger.Current.Debug("xact.extend", () => String.Format("Initial post on line {0}: amount {1} (precision {2})", 
                                    initialPost.Pos.BegLine, initialPost.Amount, initialPost.Amount.DisplayPrecision));

                                if (initialPost.Amount.KeepPrecision)
                                    Logger.Current.Debug("xact.extend", () => "  precision is kept");

                                Logger.Current.Debug("xact.extend", () => String.Format("Posting on line {0}: amount {1}, amt {2} (precision {3} != {4})", 
                                    post.Pos.BegLine, postAmount, amt, postAmount.DisplayPrecision, amt.DisplayPrecision));

                                if (postAmount.KeepPrecision)
                                    Logger.Current.Debug("xact.extend", () => "  precision is kept");
                                if (amt.KeepPrecision)
                                    Logger.Current.Debug("xact.extend", () => "  amt precision is kept");
                            }

                            if (String.IsNullOrEmpty(post.Account.FullName))
                                throw new AssertionFailedError("assert(! fullname.empty());");

                            if (fullName.Contains("$account"))
                            {
                                fullName = AccountRegex.Replace(fullName, initialPost.Account.FullName);
                                while (account.Parent != null)
                                    account = account.Parent;
                                account = account.FindAccount(fullName);
                            }
                            else if (fullName.Contains("%("))
                            {
                                Format accountName = new Format(fullName);
                                while (account.Parent != null)
                                    account = account.Parent;
                                account = account.FindAccount(accountName.Calc(boundScope));
                            }

                            // Copy over details so that the resulting post is a mirror of
                            // the automated xact's one.
                            Post newPost = new Post(account, amt);
                            newPost.CopyDetails(post);

                            // A Cleared transaction implies all of its automatic posting are cleared
                            // CPR 2012/10/23
                            if (xact.State == ItemStateEnum.Cleared)
                            {
                                Logger.Current.Debug("xact.extend.cleared", () => "CLEARED");
                                newPost.State = ItemStateEnum.Cleared;
                            }

                            newPost.Flags = newPost.Flags | SupportsFlagsEnum.ITEM_GENERATED;
                            newPost.Account = Journal.RegisterAccount(account.FullName, newPost, Journal.Master);

                            if (DeferredNotes != null)
                            {
                                foreach(DeferredTagData data in DeferredNotes)
                                {
                                    if (data.ApplyToPost == null || data.ApplyToPost == post)
                                        newPost.AppendNote(ApplyFormat(data.TagData, boundScope), boundScope, data.OverwriteExisting);
                                }
                            }

                            Post.ExtendPost(newPost, Journal);

                            xact.AddPost(newPost);
                            newPost.Account.AddPost(newPost);

                            // Add flag so this post updates the account balance
                            newPost.XData.Visited = true; // POST_EXT_VISITED

                            if (newPost.MustBalance)
                                needsFurtherVerification = true;

                        }
                    }
                }

                if (needsFurtherVerification)
                    xact.Verify();

            }
            catch
            {
                ErrorContext.Current.AddErrorContext(Item.ItemContext(this, "While applying automated transaction"));
                ErrorContext.Current.AddErrorContext(Item.ItemContext(xact, "While extending transaction"));
                throw;
            }
        }

        public override void ParseTags(string note, Scope scope, bool overwriteExisting = true)
        {
            if (DeferredNotes == null)
                DeferredNotes = new List<DeferredTagData>();
            DeferredNotes.Add(new DeferredTagData(note, overwriteExisting) { ApplyToPost = ActivePost });
        }

        // [DM] See xact.cc:762 - the original regex "\\$account\\>" (remember about escaped back slashes)
        // has a special anchor at the end "\>" that indicates End of Word 
        // (see Boost Regex Syntax http://www.boost.org/doc/libs/1_31_0/libs/regex/doc/syntax.html
        // or https://www.cheatography.com/davechild/cheat-sheets/regular-expressions )
        // This anchor is not supported by .Net Regex; it should be replaced with Word Boundary \b
        // that has pretty similar meaning (not a word char - (^\w|\w$|\W\w|\w\W).
        private readonly Regex AccountRegex = new Regex("\\$account\\b", RegexOptions.Compiled);
    }

    public class DeferredTagData
    {
        public string TagData { get; private set; }
        public bool OverwriteExisting { get; private set; }
        public Post ApplyToPost { get; set; }

        public DeferredTagData(string tagData, bool overwriteExisting)
        {
            TagData = tagData;
            OverwriteExisting = overwriteExisting;
        }
    }
}
