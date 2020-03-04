// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Amounts;
using NLedger.Annotate;
using NLedger.Commodities;
using NLedger.Items;
using NLedger.Journals;
using NLedger.Utility;
using NLedger.Utils;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Xacts
{
    /// <summary>
    /// Ported from xact_base_t (xact.h)
    /// </summary>
    public abstract class XactBase : Item
    {
        public static bool AccountEndsWithSpecialChar(string name)
        {
            char lastChar = String.IsNullOrEmpty(name) ? default(char) : name[name.Length-1];
            return Char.IsDigit(lastChar) || lastChar == ')' || lastChar == '}' || lastChar == ']';
        }

        public XactBase()
        {
            Posts = new List<Post>();
        }

        public XactBase(XactBase xactBase)
            : base(xactBase)
        {
            Posts = new List<Post>();
            Journal = xactBase.Journal;
        }

        /// <summary>
        /// Ported from xact_base_t::~xact_base_t()
        /// </summary>
        public void Detach()
        {
            if (!Flags.HasFlag(SupportsFlagsEnum.ITEM_TEMP))
            {
                foreach(Post post in Posts)
                {
                    // If the posting is a temporary, it will be destructed when the
                    // temporary is.
                    if (post.Flags.HasFlag(SupportsFlagsEnum.ITEM_TEMP))
                        throw new InvalidOperationException("assert");

                    if (post.Account != null)
                        post.Account.RemovePost(post);

                }
            }
        }

        public Journal Journal { get; set; }
        public IList<Post> Posts { get; private set; }

        public bool HasXData
        {
            get { return Posts.Any(p => p.HasXData); }
        }

        public virtual void AddPost(Post post)
        {
            if (post == null)
                throw new ArgumentNullException("post");

            // You can add temporary postings to transactions, but not real postings to temporary transactions.
            if (!post.Flags.HasFlag(SupportsFlagsEnum.ITEM_TEMP) && Flags.HasFlag(SupportsFlagsEnum.ITEM_TEMP))
                throw new InvalidOperationException("Cannot add a real posting to a temporary transaction");

            Posts.Add(post);
        }

        public virtual bool RemovePost(Post post)
        {
            Posts.Remove(post);
            post.Xact = null;
            return true;
        }

        public void ClearXData()
        {
            foreach (Post post in Posts.Where(p => !p.Flags.HasFlag(SupportsFlagsEnum.ITEM_TEMP)))
                post.ClearXData();
        }

        public bool Verify()
        {
            // Scan through and compute the total balance for the xact.

            Value balance = null;

            foreach(Post post in Posts)
            {
                if (!post.MustBalance)
                    continue;

                Amount p = post.Cost ?? post.Amount;

                if (p == null)
                    throw new InvalidOperationException("p");

                // If the amount was a cost, it very likely has the "keep_precision" flag
                // set, meaning commodity display precision is ignored when displaying the
                // amount.  We never want this set for the balance, so we must clear the
                // flag in a temporary to avoid it propagating into the balance.
                balance = Value.AddOrSetValue(balance, Value.Get(p.KeepPrecision ? p.Rounded().Reduced() : p.Reduced()));
            }
            Validator.Verify(() => balance.IsValid);

            // Now that the post list has its final form, calculate the balance once
            // more in terms of total cost, accounting for any possible gain/loss
            // amounts.

            foreach(Post post in Posts)
            {
                if (post.Cost == null)
                    continue;

                if (post.Amount.Commodity == post.Cost.Commodity)
                    throw new AmountError(AmountError.ErrorMessageAPostingsCostMustBeOfADifferentCommodityThanItsAmount);
            }

            if (!Value.IsNullOrEmpty(balance) && !balance.IsZero)
            {
                ErrorContext.Current.AddErrorContext(ItemContext(this, "While balancing transaction"));
                ErrorContext.Current.AddErrorContext("Unbalanced remainder is:");
                ErrorContext.Current.AddErrorContext(Value.ValueContext(balance));
                ErrorContext.Current.AddErrorContext("Amount to balance against:");
                ErrorContext.Current.AddErrorContext(Value.ValueContext(Magnitude()));
                throw new BalanceError(BalanceError.ErrorMessageTransactionDoesNotBalance);
            }

            Validator.Verify(() => Valid());

            return true;
        }

        public override bool Valid()
        {
            return true;
        }

        public bool FinalizeXact()
        {
            // Scan through and compute the total balance for the xact.  This is used
            // for auto-calculating the value of xacts with no cost, and the per-unit
            // price of unpriced commodities.

            Value balance = null;
            Post nullPost = null;

            foreach(Post post in Posts)
            {
                if (!post.MustBalance)
                    continue;

                Amount p = post.Cost ?? post.Amount;
                if (p != null && !p.IsEmpty)
                {
                    Logger.Current.Debug("xact.finalize", () => String.Format("post must balance = {0}", p.Reduced()));
                    // If the amount was a cost, it very likely has the
                    // "keep_precision" flag set, meaning commodity display precision
                    // is ignored when displaying the amount.  We never want this set
                    // for the balance, so we must clear the flag in a temporary to
                    // avoid it propagating into the balance.
                    balance = Value.AddOrSetValue(balance, Value.Get(p.KeepPrecision ? p.Rounded().Reduced() : p.Reduced()));
                }
                else if (nullPost != null)
                {
                    bool postAccountBad = AccountEndsWithSpecialChar(post.Account.FullName);
                    bool nullPostAccountBad = AccountEndsWithSpecialChar(nullPost.Account.FullName);

                    if (postAccountBad || nullPostAccountBad)
                        throw new LogicError(String.Format(LogicError.ErrorMessagePostingWithNullAmountsAccountMayBeMisspelled, postAccountBad ? post.Account.FullName : nullPost.Account.FullName));
                    else
                        throw new LogicError(LogicError.ErrorMessageOnlyOnePostingWithNullAmountAllowedPerTransaction);
                }
                else
                {
                    nullPost = post;
                }
            }
            Validator.Verify(() => balance == null || balance.IsValid);

            Logger.Current.Debug("xact.finalize", () => String.Format("initial balance = {0}", balance));
            Logger.Current.Debug("xact.finalize", () => String.Format("balance is {0}", balance.Label()));
            if (balance != null && balance.Type == ValueTypeEnum.Balance)
                Logger.Current.Debug("xact.finalize", () => String.Format("balance commodity count = {0}", balance.AsBalance.Amounts.Count()));

            // If there is only one post, balance against the default account if one has
            // been set.

            if (Journal != null && Journal.Bucket != null && Posts.Count == 1 && !Value.IsNullOrEmpty(balance))
            {
                nullPost = new Post() { Account = Journal.Bucket, Flags = SupportsFlagsEnum.ITEM_GENERATED };
                nullPost.State = Posts.First().State;
                AddPost(nullPost);
            }

            if (nullPost == null && balance != null && balance.Type == ValueTypeEnum.Balance && balance.AsBalance.Amounts.Count == 2)
            {
                // When an xact involves two different commodities (regardless of how
                // many posts there are) determine the conversion ratio by dividing the
                // total value of one commodity by the total value of the other.  This
                // establishes the per-unit cost for this post for both commodities.

                Logger.Current.Debug("xact.finalize", () => "there were exactly two commodities, and no null post");

                bool sawCost = false;
                Post topPost = null;

                foreach(Post post in Posts)
                {
                    if (!Amount.IsNullOrEmpty(post.Amount) && post.MustBalance)
                    {
                        if (post.Amount.HasAnnotation)
                            topPost = post;
                        else if (topPost == null)
                            topPost = post;
                    }

                    if (post.Cost != null && !post.Flags.HasFlag(SupportsFlagsEnum.POST_COST_CALCULATED))
                    {
                        sawCost = true;
                        break;
                    }
                }

                if (!sawCost && topPost != null)
                {
                    Balance bal = balance.AsBalance;

                    Logger.Current.Debug("xact.finalize", () => "there were no costs, and a valid top_post");

                    Amount x = bal.Amounts.ElementAt(0).Value;
                    Amount y = bal.Amounts.ElementAt(1).Value;

                    if ((bool)x && (bool)y)
                    {
                        if (x.Commodity != topPost.Amount.Commodity)
                        {
                            Amount z = x; x = y; y = z;
                        }

                        Logger.Current.Debug("xact.finalize", () => String.Format("primary   amount = {0}", x));
                        Logger.Current.Debug("xact.finalize", () => String.Format("secondary amount = {0}", y));

                        Commodity comm = x.Commodity;
                        Amount perUnitCost = (y / x).Abs().Unrounded();

                        Logger.Current.Debug("xact.finalize", () => String.Format("per_unit_cost = {0}", perUnitCost));

                        foreach (Post post in Posts)
                        {
                            Amount amt = post.Amount;

                            if (post.MustBalance && amt != null && amt.Commodity == comm)
                            {
                                balance.InPlaceSubtract(Value.Get(amt));
                                post.Cost = perUnitCost * amt;
                                post.Flags = post.Flags | SupportsFlagsEnum.POST_COST_CALCULATED;
                                balance.InPlaceAdd(Value.Get(post.Cost));

                                Logger.Current.Debug("xact.finalize", () => String.Format("set post->cost to = {0}", post.Cost));
                            }
                        }
                    }
                }
            }

            var postsCopy = new List<Post>(Posts);

            if (HasDate)
            {
                foreach(Post post in postsCopy)
                {
                    if (post.Cost == null)
                        continue;

                    if (post.Amount.Commodity == post.Cost.Commodity)
                        throw new BalanceError(BalanceError.ErrorMessageAPostingsCostMustBeOfADifferentCommodityThanItsAmount);

                    CostBreakdown breakdown = CommodityPool.Current.Exchange(post.Amount, post.Cost, false, !post.Flags.HasFlag(SupportsFlagsEnum.POST_COST_VIRTUAL), GetDate());

                    if (post.Amount.HasAnnotation && post.Amount.Annotation.Price != null)
                    {
                        if (breakdown.BasisCost.Commodity == breakdown.FinalCost.Commodity)
                        {
                            Logger.Current.Debug("xact.finalize", () => String.Format("breakdown.basis_cost = {0}", breakdown.BasisCost));
                            Logger.Current.Debug("xact.finalize", () => String.Format("breakdown.final_cost = {0}", breakdown.FinalCost));
                            Amount gainLoss = breakdown.BasisCost - breakdown.FinalCost;
                            if ((bool)gainLoss)
                            {
                                Logger.Current.Debug("xact.finalize", () => String.Format("gain_loss = {0}", gainLoss));
                                gainLoss.InPlaceRound();
                                Logger.Current.Debug("xact.finalize", () => String.Format("gain_loss rounds to = {0}", gainLoss));
                                if (post.MustBalance)
                                    balance = Value.AddOrSetValue(balance, Value.Get(gainLoss.Reduced()));

                                post.Cost.InPlaceAdd(gainLoss);
                                Logger.Current.Debug("xact.finalize", () => String.Format("added gain_loss, balance = {0}", balance));
                            }
                            else
                            {
                                Logger.Current.Debug("xact.finalize", () => "gain_loss would have displayed as zero");
                            }
                        }
                    }
                    else
                    {
                        post.Amount = breakdown.Amount.HasAnnotation ? 
                            new Amount(breakdown.Amount, new Annotation(breakdown.Amount.Annotation.Price, breakdown.Amount.Annotation.Date, 
                            post.Amount.HasAnnotation ? post.Amount.Annotation.Tag : breakdown.Amount.Annotation.Tag) 
                            { ValueExpr = breakdown.Amount.Annotation.ValueExpr })
                            : breakdown.Amount;
                        Logger.Current.Debug("xact.finalize", () => String.Format("added breakdown, balance = {0}", balance));
                    }

                    if (post.Flags.HasFlag(SupportsFlagsEnum.POST_COST_FIXATED) && post.Amount.HasAnnotation && post.Amount.Annotation.Price != null)
                    {
                        Logger.Current.Debug("xact.finalize", () => "fixating annotation price");
                        post.Amount.Annotation.IsPriceFixated = true;
                    }
                }
            }

            if (nullPost != null)
            {
                // If one post has no value at all, its value will become the inverse of
                // the rest.  If multiple commodities are involved, multiple posts are
                // generated to balance them all.

                Logger.Current.Debug("xact.finalize", () => "there was a null posting");
                AddBalancingPost postAdder = new AddBalancingPost(this, nullPost);

                if (balance != null) // DM - all further steps makes sense only if the balance is not null
                {
                    if (balance.Type == ValueTypeEnum.Balance)
                        balance.AsBalance.MapSortedAmounts(amt => postAdder.Amount(amt));
                    else if (balance.Type == ValueTypeEnum.Amount)
                        postAdder.Amount(balance.AsAmount);
                    else if (balance.Type == ValueTypeEnum.Integer)
                        postAdder.Amount(balance.AsAmount);
                    else if (!Value.IsNullOrEmpty(balance) && !balance.IsRealZero)
                        throw new BalanceError(BalanceError.ErrorMessageTransactionDoesNotBalance);
                }

                balance = Value.Empty;
            }
            Logger.Current.Debug("xact.finalize", () => String.Format("resolved balance = ", balance));

            if (!Value.IsNullOrEmpty(balance) && !balance.IsZero)
            {
                ErrorContext.Current.AddErrorContext(ItemContext(this, "While balancing transaction"));
                ErrorContext.Current.AddErrorContext("Unbalanced remainder is:");
                ErrorContext.Current.AddErrorContext(Value.ValueContext(balance));
                ErrorContext.Current.AddErrorContext("Amount to balance against:");
                ErrorContext.Current.AddErrorContext(Value.ValueContext(Magnitude()));
                throw new BalanceError(BalanceError.ErrorMessageTransactionDoesNotBalance);
            }

            // Add a pointer to each posting to their related accounts

            if (this is Xact)
            {
                bool allNull = true;
                bool someNull = false;

                foreach(Post post in Posts)
                {
                    if (!Amount.IsNullOrEmpty(post.Amount))
                    {
                        allNull = false;
                        post.Amount.InPlaceReduce();
                    }
                    else
                    {
                        someNull = true;
                    }

                    if (post.Flags.HasFlag(SupportsFlagsEnum.POST_DEFERRED))
                    {
                        if(!Amount.IsNullOrEmpty(post.Amount))
                            post.Account.AddDeferredPosts(Id, post);
                    }
                    else
                        post.Account.AddPost(post);

                    post.XData.Visited = true; // POST_EXT_VISITED
                    post.Account.XData.Visited = true; // ACCOUNT_EXT_VISITED;
                }

                if (allNull)
                    return false; // ignore this xact completely
                else if (someNull)
                    throw new BalanceError(BalanceError.ErrorMessageThereCannotBeNullAmountsAfterBalancingATransaction);
            }

            Validator.Verify(() => Valid());

            return true;
        }

        public Value Magnitude()
        {
            Value halfBal = Value.Get(0);
            foreach(Post post in Posts)
            {
                if (post.Amount.Sign > 0)
                    halfBal.InPlaceAdd(Value.Get(post.Cost ?? post.Amount));
            }
            return halfBal;
        }
    }
}
