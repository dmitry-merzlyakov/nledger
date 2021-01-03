// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Expressions;
using NLedger.Scopus;
using NLedger.Utils;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Accounts
{
    /// <summary>
    /// Ported from account_t (account.h)
    /// </summary>
    public class Account : Scope
    {
        public const string UnknownName = "Unknown";

        public Account()
        {
            Accounts = new SortedDictionary<string, Account>();
            Posts = new List<Post>();
        }

        public Account(Account parent, string name, string note = null) : this()
        {
            Parent = parent;
            Depth = Parent != null ? Parent.Depth + 1 : 0;
            Name = name;
            Note = note ?? String.Empty;
        }

        public override string Description
        {
            get { return string.Format("account {0}", FullName); }
        }

        public Account Parent { get; private set; }
        public string Name { get; private set; }
        public string Note { get; set; }
        public IDictionary<string, Account> Accounts { get; private set; }
        public IList<Post> Posts { get; private set; }
        public IDictionary<string, IList<Post>> DeferredPosts { get; private set; }
        public Expr ValueExpr { get; set; }
        public int Depth { get; private set; }

        public bool IsKnownAccount { get; set; }      // ACCOUNT_KNOWN
        public bool IsTempAccount { get; set; }       // ACCOUNT_TEMP - account is a temporary object
        public bool IsGeneratedAccount { get; set; }  // ACCOUNT_GENERATED - account never actually existed

        public string FullName
        {
            get { return _FullName ?? (_FullName = GetFullName()); }
        }

        /// <summary>
        // This variable holds optional "extended data" which is usually produced
        // only during reporting, and only for the posting set being reported.
        // It's a memory-saving measure to delay allocation until the last possible
        // moment.
        /// </summary>
        public AccountXData XData { get { return _XData ?? (_XData = new AccountXData()); } }
        public bool HasXData { get { return _XData != null; } }

        public bool HasXFlags(Func<AccountXData,bool> xDataFunc)
        {
            return HasXData && xDataFunc(XData);
        }

        public Account FindAccount(string acctName, bool autoCreate = true)
        {
            Account account;
            if (Accounts.TryGetValue(acctName, out account))
                return account;

            string first, rest;
            int pos = acctName.IndexOf(":");
            if (pos < 0)
            {
                first = acctName;
                rest = null;
            }
            else
            {
                first = acctName.Substring(0, pos);
                rest = acctName.Substring(pos + 1);
            }

            if (!Accounts.TryGetValue(first, out account))
            {
                if (!autoCreate)
                    return null;

                account = new Account(this, first);

                // An account created within a temporary or generated account is itself
                // temporary or generated, so that the whole tree has the same status.
                if (IsTempAccount)
                    account.IsTempAccount = IsTempAccount;
                if (IsGeneratedAccount)
                    account.IsGeneratedAccount = IsGeneratedAccount;

                Accounts.Add(first, account);
            }

            if (!String.IsNullOrEmpty(rest))
                account = account.FindAccount(rest, autoCreate);

            return account;
        }

        public Account FindAccountRe(string regexp)
        {
            return DoFindAccountRe(this, new Mask(regexp));
        }

        public Value Amount(bool realOnly = false, Expr expr = null)
        {
            Logger.Current.Debug("account.amount", () => $"real only: {realOnly}");

            if (HasXData && XData.Visited)
            {
                for (int i = XData.SelfDetails.LastPost; i < Posts.Count; i++)
                {
                    Post post = Posts[i];
                    if (post.XData.Visited && !post.XData.Considered)
                    {
                        if (!post.Flags.HasFlag(SupportsFlagsEnum.POST_VIRTUAL))
                            XData.SelfDetails.RealTotal = post.AddToValue(XData.SelfDetails.RealTotal, expr);

                        XData.SelfDetails.Total = post.AddToValue(XData.SelfDetails.Total, expr);
                        post.XData.Considered = true;
                    }
                }
                XData.SelfDetails.LastPost = Posts.Count;

                for (int i = XData.SelfDetails.LastReportedPost; i < XData.ReportedPosts.Count; i++)
                {
                    Post post = XData.ReportedPosts[i];
                    if (post.XData.Visited && !post.XData.Considered)
                    {
                        if (!post.Flags.HasFlag(SupportsFlagsEnum.POST_VIRTUAL))
                            XData.SelfDetails.RealTotal = post.AddToValue(XData.SelfDetails.RealTotal, expr);

                        XData.SelfDetails.Total = post.AddToValue(XData.SelfDetails.Total, expr);
                        post.XData.Considered = true;
                    }
                }
                XData.SelfDetails.LastReportedPost = XData.ReportedPosts.Count;

                if (realOnly)
                    return XData.SelfDetails.RealTotal;
                else
                    return XData.SelfDetails.Total;
            }
            else
            {
                return new Value();
            }
        }

        private string GetFullName()
        {
            Account first = this;
            StringBuilder sb = new StringBuilder(Name);

            while(first.Parent != null)
            {
                first = first.Parent;
                if (!String.IsNullOrEmpty(first.Name))
                    sb.Insert(0, first.Name + ":");
            }

            return sb.ToString();
        }

        public void AddAccount(Account acct)
        {
            Accounts[acct.Name] = acct;
        }

        public bool RemoveAccount(Account acct)
        {
            if (Accounts.ContainsKey(acct.Name))
            {
                Accounts.Remove(acct.Name);
                return true;
            }
            else
                return false;
        }

        public void AddPost(Post post)
        {
            Posts.Add(post);

            // Adding a new post changes the possible totals that may have been
            // computed before.
            if (XData != null)
            {
                XData.SelfDetails.Gathered = false;
                XData.SelfDetails.Calculated = false;
                XData.FamilyDetails.Gathered = false;
                XData.FamilyDetails.Calculated = false;

                if (!Value.IsNullOrEmpty(XData.FamilyDetails.Total))
                    XData.FamilyDetails.Total = null; // [DM] - decided to replace 'new Value();' with 'null' for performance reasons

                Account ancestor = this;

                while(ancestor.Parent != null)
                {
                    ancestor = ancestor.Parent;
                    if (ancestor.HasXData)
                    {
                        AccountXData xdata = ancestor.XData;
                        xdata.FamilyDetails.Gathered = false;
                        xdata.FamilyDetails.Calculated = false;
                        xdata.FamilyDetails.Total = null; // [DM] - decided to replace 'new Value();' with 'null' for performance reasons
                    }
                }
            }
        }

        public bool RemovePost(Post post)
        {
            // It's possible that 'post' wasn't yet in this account, but try to
            // remove it anyway.  This can happen if there is an error during
            // parsing, when the posting knows what it's account is, but
            // xact_t::finalize has not yet added that posting to the account.
            Posts.Remove(post);
            post.Account = null;
            return true;
        }

        public void AddDeferredPosts(string uuid, Post post)
        {
            if (DeferredPosts == null)
                DeferredPosts = new Dictionary<string, IList<Post>>();

            IList<Post> posts;
            if (!DeferredPosts.TryGetValue(uuid, out posts))
                DeferredPosts[uuid] = posts = new List<Post>();

            posts.Add(post);
        }

        public IEnumerable<Post> GetDeferredPosts(string uuid)
        {
            if (DeferredPosts != null)
            {
                IList<Post> posts;
                if (DeferredPosts.TryGetValue(uuid, out posts))
                    return posts;
            }
            return null;
        }

        public void DeleteDeferredPosts(string uuid)
        {
            if (DeferredPosts != null)
                DeferredPosts.Remove(uuid);
        }

        public void ApplyDeferredPosts()
        {
            if (DeferredPosts != null)
            {
                foreach (Post post in DeferredPosts.Values.SelectMany(list => list))
                    post.Account.AddPost(post);

                DeferredPosts = null;
            }

            // Also apply in child accounts
            foreach (Account account in Accounts.Values)
                account.ApplyDeferredPosts();
        }

        public void ClearXData()
        {
            _XData = null;

            foreach (Account account in Accounts.Values.Where(acc => !acc.IsTempAccount))
                account.ClearXData();
        }

        public void SetFlags(Account account)
        {
            IsKnownAccount = account.IsKnownAccount;
            IsTempAccount = account.IsTempAccount;
            IsGeneratedAccount = account.IsGeneratedAccount;
        }

        public AccountXDataDetails SelfDetails(bool gatherAll = true)
        {
            if (!(HasXData && XData.SelfDetails.Gathered))
            {
                this.XData.SelfDetails.Gathered = true;

                foreach (Post post in Posts)
                    XData.SelfDetails.Update(post, gatherAll);
            }
            return XData.SelfDetails;
        }

        public AccountXDataDetails FamilyDetails(bool gatherAll = true)
        {
            if (!(HasXData && XData.FamilyDetails.Gathered))
            {
                this.XData.FamilyDetails.Gathered = true;

                foreach (var pair in Accounts)
                    XData.FamilyDetails.Add(pair.Value.FamilyDetails(gatherAll));

                XData.FamilyDetails.Add(SelfDetails(gatherAll));
            }
            return XData.FamilyDetails;
        }

        public override ExprOp Lookup(SymbolKindEnum kind, string name)
        {
            return LookupItems.Value.Lookup(kind, name, this);
        }

        public string PartialName(bool flat)
        {
            StringBuilder sb = new StringBuilder(Name);

            for(Account acct = Parent; acct != null && acct.Parent != null; acct = acct.Parent)
            {
                if (!flat)
                {
                    int count = acct.ChildrenWithFlags(true, false);
                    if (count == 0)
                        throw new InvalidOperationException("assert(count > 0);");
                    if (count > 1 || (acct.HasXData && acct.XData.ToDisplay))
                        break;
                }
                sb.Insert(0, acct.Name + ":");
            }
            return sb.ToString();
        }

        public Value Total(Expr expr = null)
        {
            if (!(HasXData && XData.FamilyDetails.Calculated))
            {
                XData.FamilyDetails.Calculated = true;

                Value temp;
                foreach(Account acct in Accounts.Values)
                {
                    temp = acct.Total(expr);
                    if (!Value.IsNullOrEmpty(temp))
                        XData.FamilyDetails.Total = Value.AddOrSetValue(XData.FamilyDetails.Total, temp);
                }

                temp = Amount(false, expr);
                if (!Value.IsNullOrEmpty(temp))
                    XData.FamilyDetails.Total = Value.AddOrSetValue(XData.FamilyDetails.Total, temp);
            }
            return XData.FamilyDetails.Total;
        }

        public int ChildrenWithFlags(bool toDisplay, bool visited)
        {
            int count = 0;
            bool grandchildrenVisited = false;

            foreach(Account account in Accounts.Values)
            {
                if ((account.HasXData && HasXFlags(account.XData, toDisplay, visited)) || account.ChildrenWithFlags(toDisplay, visited) > 0)
                    count++;
            }

            // Although no immediately children were visited, if any progeny at all were
            // visited, it counts as one.
            if (count == 0 && grandchildrenVisited)
                count = 1;

            return count;
        }

        public override string ToString()
        {
            return FullName;
        }

        /// <summary>
        /// Ported from bool account_t::valid()
        /// </summary>
        /// <returns></returns>
        public bool Valid()
        {
            if (Depth > 256)
            {
                Logger.Current.Debug("ledger.validate", () => "account_t: depth > 256");
                return false;
            }

            foreach(var account in Accounts.Values)
            {
                if (this == account)
                {
                    Logger.Current.Debug("ledger.validate", () => "account_t: parent refers to itself!");
                    return false;
                }

                if (!account.Valid())
                {
                    Logger.Current.Debug("ledger.validate", () => "account_t: child not valid");
                    return false;
                }
            }

            return true;
        }

        private static bool HasXFlags(AccountXData xdata, bool toDisplay, bool visited)
        {
            if (toDisplay && !xdata.ToDisplay)
                return false;

            if (visited && !xdata.Visited)
                return false;

            return toDisplay || visited;
        }

        #region Lookup Functions

        private static Value GetWrapper(CallScope scope, Func<Account, Value> func)
        {
            return func(ScopeExtensions.FindScope<Account>(scope));
        }

        private static Value GetAmount(Account account)
        {
            return Value.SimplifiedValueOrZero(account.Amount());
        }

        private static Value GetAccount(CallScope args)  // this gets the name
        {
            Account account = args.Context<Account>();
            if (args.Has<string>(0))
            {
                Account acct = account.Parent;
                while (acct != null)
                    acct = acct.Parent;
                if (args[0].Type == ValueTypeEnum.String)
                    return Value.ScopeValue(acct.FindAccount(args.Get<string>(0), false));
                else if (args[0].Type == ValueTypeEnum.Mask)
                    return Value.ScopeValue(acct.FindAccountRe(args.Get<Mask>(0).ToString()));
                else
                    return Value.Empty;
            }
            else if (args.TypeContext == ValueTypeEnum.Scope)
            {
                return Value.ScopeValue(account);
            }
            else
            {
                return Value.StringValue(account.FullName);
            }
        }

        private static Value FnAny(CallScope args)
        {
            Account account = args.Context<Account>();
            ExprOp expr = args.Get<ExprOp>(0);

            foreach(Post p in account.Posts)
            {
                BindScope boundScope = new BindScope(args, p);
                if (expr.Calc(boundScope, args.Locus, args.Depth).AsBoolean)
                    return Value.True;
            }
            return Value.False;
        }

        private static Value FnAll(CallScope args)
        {
            Account account = args.Context<Account>();
            ExprOp expr = args.Get<ExprOp>(0);

            foreach (Post p in account.Posts)
            {
                BindScope boundScope = new BindScope(args, p);
                if (!expr.Calc(boundScope, args.Locus, args.Depth).AsBoolean)
                    return Value.False;
            }
            return Value.True;
        }

        private static Value GetCount(Account account)
        {
            return Value.Get(account.FamilyDetails().PostsCount);
        }

        private static Value GetCost(Account account)
        {
            throw new CalcError(CalcError.ErrorMessageAnAccountDoesNotHaveACostValue);
        }

        private static Value GetDepth(Account account)
        {
            return Value.Get(account.Depth);
        }

        private static Value GetDepthParent(Account account)
        {
            int depth = 0;
            for (Account acct = account.Parent; acct != null && acct.Parent != null; acct = acct.Parent)
            {
                int count = acct.ChildrenWithFlags(true, false);
                if (count == 0)
                    throw new InvalidOperationException("assert(count > 0)");
                if (count > 1 || (acct.HasXData && acct.XData.ToDisplay))
                    depth++;
            }

            return Value.Get((long)depth);
        }

        private static Value GetDepthSpacer(Account account)
        {
            int depth = 0;
            for (Account acct = account.Parent; acct != null && acct.Parent != null; acct = acct.Parent)
            {
                int count = acct.ChildrenWithFlags(true, false);
                if (count == 0)
                    throw new InvalidOperationException("assert(count > 0)");
                if (count > 1 || (acct.HasXData && acct.XData.ToDisplay))
                    depth++;
            }

            return Value.StringValue(new String(' ', depth * 2));
        }

        private static Value GetEarliest(Account account)
        {
            return Value.Get(account.SelfDetails().EarliestPost);
        }

        private static Value GetEarliestCheckin(Account account)
        {
            return account.SelfDetails().EarliestCheckin != null ? Value.Get(account.SelfDetails().EarliestCheckin) : Value.Empty;
        }

        private static Value GetSubcount(Account account)
        {
            return Value.Get(account.SelfDetails().PostsCount);
        }

        private static Value GetLatestCleared(Account account)
        {
            return Value.Get(account.SelfDetails().LatestClearedPost);
        }

        private static Value GetLatest(Account account)
        {
            return Value.Get(account.SelfDetails().LatestPost);
        }

        private static Value GetLatestCheckout(Account account)
        {
            return account.SelfDetails().LatestCheckout != null ? Value.Get(account.SelfDetails().LatestCheckout) : Value.Empty;
        }

        private static Value GetLatestCheckoutCleared(Account account)
        {
            return Value.Get(account.SelfDetails().LatestCheckoutCleared);
        }

        private static Value GetNote(Account account)
        {
            return !String.IsNullOrEmpty(account.Note) ? Value.StringValue(account.Note) : Value.Empty;
        }

        private static Value GetPartialName(CallScope args)
        {
            return Value.StringValue(args.Context<Account>().PartialName(args.Has<bool>(0) && args.Get<bool>(0)));
        }

        private static Value GetTotal(Account account)
        {
            return Value.SimplifiedValueOrZero(account.Total());
        }

        private static ExprOpCollection CreateLookupItems()
        {
            var lookupItems = new ExprOpCollection();

            // a
            lookupItems.MakeFunctor("a", scope => GetWrapper((CallScope)scope, a => GetAmount(a)), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("amount", scope => GetWrapper((CallScope)scope, a => GetAmount(a)), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("account", scope => GetAccount((CallScope)scope), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("account_base", scope => GetWrapper((CallScope)scope, a => Value.StringValue(a.Name)), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("addr", scope => GetWrapper((CallScope)scope, a => Value.Get(a) /* [DM] Address is not allowed in .Net; return a whole entity */ ), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("any", scope => FnAny((CallScope)scope), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("all", scope => FnAll((CallScope)scope), SymbolKindEnum.FUNCTION);

            // c
            lookupItems.MakeFunctor("count", scope => GetWrapper((CallScope)scope, a => GetCount(a)), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("cost", scope => GetWrapper((CallScope)scope, a => GetCost(a)), SymbolKindEnum.FUNCTION);

            // d
            lookupItems.MakeFunctor("depth", scope => GetWrapper((CallScope)scope, a => GetDepth(a)), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("depth_parent", scope => GetWrapper((CallScope)scope, a => GetDepthParent(a)), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("depth_spacer", scope => GetWrapper((CallScope)scope, a => GetDepthSpacer(a)), SymbolKindEnum.FUNCTION);

            // e
            lookupItems.MakeFunctor("earliest", scope => GetWrapper((CallScope)scope, a => GetEarliest(a)), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("earliest_checkin", scope => GetWrapper((CallScope)scope, a => GetEarliestCheckin(a)), SymbolKindEnum.FUNCTION);

            // i
            lookupItems.MakeFunctor("is_account", scope => Value.True, SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("is_index", scope => GetWrapper((CallScope)scope, a => GetSubcount(a)), SymbolKindEnum.FUNCTION);

            // l
            lookupItems.MakeFunctor("l", scope => GetWrapper((CallScope)scope, a => GetDepth(a)), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("latest_cleared", scope => GetWrapper((CallScope)scope, a => GetLatestCleared(a)), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("latest", scope => GetWrapper((CallScope)scope, a => GetLatest(a)), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("latest_checkout", scope => GetWrapper((CallScope)scope, a => GetLatestCheckout(a)), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("latest_checkout_cleared", scope => GetWrapper((CallScope)scope, a => GetLatestCheckoutCleared(a)), SymbolKindEnum.FUNCTION);

            // n
            lookupItems.MakeFunctor("n", scope => GetWrapper((CallScope)scope, a => GetSubcount(a)), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("note", scope => GetWrapper((CallScope)scope, a => GetNote(a)), SymbolKindEnum.FUNCTION);

            // p
            lookupItems.MakeFunctor("partial_account", scope => GetPartialName((CallScope)scope), SymbolKindEnum.FUNCTION);
            lookupItems.MakeFunctor("parent", scope => GetWrapper((CallScope)scope, a => Value.ScopeValue(a.Parent)), SymbolKindEnum.FUNCTION);

            // s
            lookupItems.MakeFunctor("subcount", scope => GetWrapper((CallScope)scope, a => GetSubcount(a)), SymbolKindEnum.FUNCTION);

            // t
            lookupItems.MakeFunctor("total", scope => GetWrapper((CallScope)scope, a => GetTotal(a)), SymbolKindEnum.FUNCTION);

            // u
            lookupItems.MakeFunctor("use_direct_amount", scope => Value.False /* ignore */, SymbolKindEnum.FUNCTION);

            // N
            lookupItems.MakeFunctor("N", scope => GetWrapper((CallScope)scope, a => GetCount(a)), SymbolKindEnum.FUNCTION);

            // O
            lookupItems.MakeFunctor("O", scope => GetWrapper((CallScope)scope, a => GetTotal(a)), SymbolKindEnum.FUNCTION);

            return lookupItems;
        }

        #endregion

        private Account DoFindAccountRe(Account account, Mask regex)
        {
            if (regex.Match(account.FullName))
                return account;

            foreach (Account acc in account.Accounts.Values)
            {
                Account result = DoFindAccountRe(acc, regex);
                if (result != null)
                    return result;
            }

            return null;
        }

        private string _FullName = null;
        private AccountXData _XData = null;
        private static readonly Lazy<ExprOpCollection> LookupItems = new Lazy<ExprOpCollection>(() => CreateLookupItems(), true);
    }
}
