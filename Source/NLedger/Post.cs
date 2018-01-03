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
using NLedger.Annotate;
using NLedger.Commodities;
using NLedger.Expressions;
using NLedger.Formatting;
using NLedger.Items;
using NLedger.Journals;
using NLedger.Scopus;
using NLedger.Values;
using NLedger.Xacts;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger
{
    public class Post : Item
    {
        public static void ExtendPost(Post post, Journal journal)
        {
            Commodity comm = post.Amount.Commodity;
            Annotation details = comm.IsAnnotated ? ((AnnotatedCommodity)comm).Details : null;

            if (details == null || details.ValueExpr == null)
            {
                Expr valueExpr = null;

                Value data = post.GetTag("Value");
                if (!Value.IsNullOrEmptyOrFalse(data))
                    valueExpr = new Expr(data.AsString);

                if (valueExpr == null)
                    valueExpr = post.Account.ValueExpr;

                if (valueExpr == null)
                    valueExpr = post.Amount.Commodity.ValueExpr;

                if (valueExpr == null)
                    valueExpr = journal.ValueExpr;

                if (valueExpr != null)
                {
                    if (details == null)
                    {
                        Annotation newDetails = new Annotation() { ValueExpr = valueExpr };
                        Commodity newComm = CommodityPool.Current.FindOrCreate(comm, newDetails);
                        post.Amount.SetCommodity(newComm);
                    }
                    else
                    {
                        details.ValueExpr = valueExpr;
                    }
                }

            }
        }

        public Post()
        {
            CreateLookupItems(); 
        }

        public Post(Post post)
            : base(post)
        {
            CreateLookupItems();

            Xact = post.Xact;
            Account = post.Account;
            if (post.Amount != null)
                Amount = new Amount(post.Amount);
            Cost = post.Cost;
            AssignedAmount = post.AssignedAmount;
            Checkin = post.Checkin;
            Checkout = post.Checkout;
            if (post.HasXData)
                _XData = new PostXData(post._XData);
        }

        public Post(Account account, Amount amount = null) : this()
        {
            Amount = amount;
            Account = account;
        }

        // only set for posts of regular xacts
        public Xact Xact { get; set; }

        public DateTime? Checkin { get; set; }
        public DateTime? Checkout { get; set; }

        public Account Account { get; set; }
        public Amount Amount { get; set; }
        public Expr AmountExpr { get; set; }
        public Amount AssignedAmount { get; set; }
        public Amount Cost { get; set; }
        public Amount GivenCost { get; set; }

        public PostXData XData { get { return _XData ?? (_XData = new PostXData()); } }
        public bool HasXData { get { return _XData != null; } }

        public bool MustBalance
        {
            get { return !Flags.HasFlag(SupportsFlagsEnum.POST_VIRTUAL) || Flags.HasFlag(SupportsFlagsEnum.POST_MUST_BALANCE); }
        }

        public string Payee 
        { 
            get
            {
                Value postPayee = GetTag("Payee");
                if (!Value.IsNullOrEmpty(postPayee))
                    return postPayee.ToString();
                else
                    return Xact.Payee;
            }
        }

        public override string Description
        {
            get { return "TODO!"; }
        }

        public Account ReportedAccount
        {
            get
            {
                if (XData != null && XData.Account != null)
                    return XData.Account;

                if (Account == null)
                    throw new InvalidOperationException("Account");

                return Account;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                XData.Account = value;
                value.XData.ReportedPosts.Add(this);
            }
        }

        public int AccountId
        {
            get 
            {
                int id = Account.Posts.IndexOf(this);
                if (id == -1)
                    throw new InvalidOperationException("Failed to find posting within its transaction");
                return id + 1;            
            }
        }

        public int XactId
        {
            get
            {
                int id = Xact.Posts.IndexOf(this);
                if (id == -1)
                    throw new InvalidOperationException("Failed to find posting within its transaction");
                return id + 1;
            }
        }

        public override bool HasTag(string tag, bool inherit = true)
        {
            if (base.HasTag(tag))
                return true;

            if (inherit && Xact != null)
                return Xact.HasTag(tag);

            return false;
        }

        public override bool HasTag(Mask tagMask, Mask valueMask = null, bool inherit = true)
        {
            if (base.HasTag(tagMask, valueMask))
                return true;

            if (inherit && Xact != null)
                return Xact.HasTag(tagMask, valueMask);

            return false;
        }

        /// <summary>
        /// Ported from post_t::date()
        /// </summary>
        public override Date GetDate()
        {
            if (HasXData && XData.Date.IsValid())
                return XData.Date;

            Date? aux;
            if (UseAuxDate && (aux = GetAuxDate()).HasValue)
                return aux.Value;

            return PrimaryDate();
        }

        /// <summary>
        /// Ported from post_t::aux_date()
        /// </summary>
        public override Date? GetAuxDate()
        {
            Date? date = base.GetAuxDate();
            if (!date.HasValue && Xact != null)
                return Xact.GetAuxDate();
            return date;
        }

        /// <summary>
        /// Ported from post_t::primary_date()
        /// </summary>
        public override Date PrimaryDate()
        {
            if (HasXData && XData.Date.IsValid())
                return XData.Date;

            if (!Date.HasValue)
            {
                if (Xact == null)
                    throw new InvalidOperationException("Xact is empty");

                return Xact.GetDate();
            }

            return Date.Value;
        }

        /// <summary>
        /// Porrted from post_t::value_date()
        /// </summary>
        public Date ValueDate
        {
            get { return HasXData && XData.ValueDate.IsValid() ? XData.ValueDate : GetDate(); }
        }

        public Value AddToValue(Value value, Expr expr = null)
        {
            if (HasXData && XData.Compound)
            {
                if (!Value.IsNullOrEmpty(XData.CompoundValue))
                    value = Value.AddOrSetValue(value, XData.CompoundValue);
            }
            else if (expr != null)
            {
                BindScope boundScope = new BindScope(expr.Context, this);
                Value temp = expr.Calc(boundScope);
                value = Value.AddOrSetValue(value, temp);
            }
            else if (HasXData && XData.Visited && !Value.IsNullOrEmpty(XData.VisitedValue))
            {
                value = Value.AddOrSetValue(value, XData.VisitedValue);
            }
            else
            {
                value = Value.AddOrSetValue(value, Value.Get(Amount));
            }

            return value;
        }

        public void ClearXData()
        {
            _XData = null;
        }

        public override ExprOp Lookup(SymbolKindEnum kind, string name)
        {
            return LookupItems.Lookup(kind, name, this) ?? base.Lookup(kind, name);
        }

        public override void CopyDetails(Item item)
        {
            base.CopyDetails(item);
            Post post = (Post)item;
            if (post.HasXData)
                _XData = new PostXData(post.XData);            
        }

        public bool Valid()
        {
            if (Xact == null)
                return false;

            if (!Xact.Posts.Contains(this))
                return false;

            if (Account == null)
                return false;

            if (!Amount.Valid())
                return false;

            if (Cost != null)
            {
                if (!Cost.Valid())
                    return false;

                if (!Cost.KeepPrecision)
                    return false;
            }

            return true;
        }

        public override Value GetTag(string tag, bool inherit = true)
        {
            Value value = base.GetTag(tag, inherit);
            if (!Value.IsNullOrEmpty(value))
                return value;

            if (inherit && Xact != null)
                return Xact.GetTag(tag);

            return Value.Empty;
        }

        public Amount ResolveExpr(Scope scope, Expr expr)
        {
            BindScope boundScope = new BindScope(scope, this);
            Value result = expr.Calc(boundScope);
            if (result.Type == ValueTypeEnum.Integer)
            {
                return result.AsAmount;
            }
            else
            {
                if (result.Type != ValueTypeEnum.Amount)
                    throw new AmountError(AmountError.ErrorMessageAmountExpressionsMustResultInASimpleAmount);
                return result.AsAmount;
            }
        }

        #region Lookup Functions

        private static Value GetWrapper(CallScope scope, Func<Post,Value> func)
        {
            return func(ScopeExtensions.FindScope<Post>(scope));
        }

        private static Value GetAmount(Post post)
        {
            if (post.HasXData && post.XData.Compound)
                return post.XData.CompoundValue;
            else if (post.Amount.IsEmpty)
                return Value.Zero;
            else
                return Value.Get(post.Amount);
        }

        private static Value GetAccount(CallScope args)
        {
            Post post = args.Context<Post>();
            Account account = post.ReportedAccount;
            string name;

            if (args.Has(0))
            {
                if (args.Has<long>(0))
                {
                    if (args.Get<long>(0) > 2)
                        name = Format.Truncate(account.FullName, (int)args.Get<long>(0) - 2, /* account_abbrev_length= */ 2);
                    else
                        name = account.FullName;
                }
                else
                {
                    Account acct = null;
                    Account master = account;
                    while (master.Parent != null)
                        master = master.Parent;

                    if (args.Has<string>(0))
                    {
                        name = args.Get<string>(0);
                        acct = master.FindAccount(name, false);
                    }
                    else if (args.Has<Mask>(0))
                    {
                        name = args.Get<Mask>(0).ToString();
                        acct = master.FindAccount(name);
                    }
                    else
                    {
                        throw new RuntimeError(String.Format(RuntimeError.ErrorMessageExpectedStringOrMaskForArgument1ButReceivedSmth, args[0].ToString()));
                    }

                    if (acct == null)
                        throw new RuntimeError(String.Format(RuntimeError.ErrorMessageCouldNotFindAnAccountMatchingSmth, args[0].ToString()));

                    return Value.ScopeValue(acct);
                }
            }
            else if (args.TypeContext == ValueTypeEnum.Scope)
                return Value.ScopeValue(account);
            else
                name = account.FullName;
            return Value.StringValue(name);
        }

        private static Value GetAccountBase(Post post)
        {
            return Value.StringValue(post.ReportedAccount.Name);
        }

        private static Value GetAccountId(Post post)
        {
            return Value.Get(post.AccountId);
        }

        private static Value FnAny(CallScope args)
        {
            Post post = args.Context<Post>();
            ExprOp expr = args.Get<ExprOp>(0);

            foreach(Post p in post.Xact.Posts)
            {
                BindScope boundScope = new BindScope(args, p);
                if (p == post && args.Has<ExprOp>(1) && !args.Get<ExprOp>(1).Calc(boundScope, args.Locus, args.Depth).AsBoolean)
                {
                    // If the user specifies any(EXPR, false), and the context is a
                    // posting, then that posting isn't considered by the test.
                    ;                       // skip it
                }
                else if (expr.Calc(boundScope, args.Locus, args.Depth).AsBoolean)
                {
                    return Value.True;
                }
            }
            return Value.False;
        }

        private static Value FnAll(CallScope args)
        {
            Post post = args.Context<Post>();
            ExprOp expr = args.Get<ExprOp>(0);

            foreach (Post p in post.Xact.Posts)
            {
                BindScope boundScope = new BindScope(args, p);
                if (p == post && args.Has<ExprOp>(1) && !args.Get<ExprOp>(1).Calc(boundScope, args.Locus, args.Depth).AsBoolean)
                {
                    // If the user specifies any(EXPR, false), and the context is a
                    // posting, then that posting isn't considered by the test.
                    ;                       // skip it
                }
                else if (expr.Calc(boundScope, args.Locus, args.Depth).AsBoolean)
                {
                    return Value.False;
                }
            }
            return Value.True;
        }

        private static Value GetCost(Post post)
        {
            if (post.Cost != null)
                return Value.Get(post.Cost);
            else if (post.HasXData && post.XData.Compound)
                return post.XData.CompoundValue;
            else if (post.Amount.IsEmpty)
                return Value.Zero;
            else
                return Value.Get(post.Amount);
        }

        private static Value GetCode(Post post)
        {
            if (!String.IsNullOrEmpty(post.Xact.Code))
                return Value.StringValue(post.Xact.Code);
            else
                return Value.Empty;
        }

        private static Value GetIsCostCalculated(Post post)
        {
            return Value.Get(post.Flags.HasFlag(SupportsFlagsEnum.POST_COST_CALCULATED));
        }

        private static Value GetCount(Post post)
        {
            return post.HasXData ? Value.Get(post.XData.Count) : Value.One;
        }

        private static Value GetIsCalculated(Post post)
        {
            return Value.Get(post.Flags.HasFlag(SupportsFlagsEnum.POST_CALCULATED));
        }

        private static Value GetCommodity(CallScope args)
        {
            if (args.Has<Amount>(0))
            {
                return Value.StringValue(args.Get<Amount>(0).Commodity.Symbol);
            }
            else
            {
                Post post = args.Context<Post>();
                if (post.HasXData && post.XData.Compound)
                    return Value.StringValue(post.XData.CompoundValue.AsAmount.Commodity.Symbol);
                else
                    return Value.StringValue(post.Amount.Commodity.Symbol);
            }
        }

        private static Value GetCheckin(Post post)
        {
            return Value.Get(post.Checkin != null ? post.Checkin.Value : default(DateTime) );
        }

        private static Value GetCheckout(Post post)
        {
            return Value.Get(post.Checkout != null ? post.Checkout.Value : default(DateTime));
        }

        private static Value GetDisplayAccount(CallScope args)
        {
            Value acct = GetAccount(args);
            if (acct.Type == ValueTypeEnum.String)
            {
                Post post = args.Context<Post>();
                if (post.Flags.HasFlag(SupportsFlagsEnum.POST_VIRTUAL))
                {
                    if (post.MustBalance)
                        acct = Value.StringValue(String.Format("[{0}]", acct.AsString));
                    else
                        acct = Value.StringValue(String.Format("({0})", acct.AsString));
                }
            }
            return acct;
        }

        private static Value GetAccountDepth(Post post)
        {
            return Value.Get(post.ReportedAccount.Depth);
        }

        /// <summary>
        /// Ported from value_t get_datetime(post_t& post)
        /// </summary>
        private static Value GetDateTime(Post post)
        {
            return Value.Get(!post.XData.Datetime.IsNotADateTime() ? post.XData.Datetime : post.GetDate());
        }

        private static Value GetHasCost(Post post)
        {
            return Value.Get(post.Cost != null);
        }

        private static Value GetMagnitude(Post post)
        {
            return post.Xact.Magnitude();
        }

        private static Value GetNote(Post post)
        {
            if (!String.IsNullOrEmpty(post.Note) || !String.IsNullOrEmpty(post.Xact.Note))
            {
                return Value.StringValue(String.Format("{0}{1}", post.Note, post.Xact.Note));
            }
            else
            {
                return Value.Empty;
            }
        }

        private static Value GetCommodityIsPrimary(Post post)
        {
            if (post.HasXData && post.XData.Compound)
                return Value.Get(post.XData.CompoundValue.AsAmount.Commodity.Flags.HasFlag(CommodityFlagsEnum.COMMODITY_PRIMARY));
            else
                return Value.Get(post.Amount.Commodity.Flags.HasFlag(CommodityFlagsEnum.COMMODITY_PRIMARY));
        }

        private static Value GetPrice(Post post)
        {
            if (post.Amount.IsEmpty)
                return Value.Zero;
            if (post.Amount.HasAnnotation && post.Amount.Annotation.Price != null)
                return Value.Get(post.Amount.Price);
            else
                return GetCost(post);
        }

        private static Value GetXact(Post post)
        {
            return Value.ScopeValue(post.Xact);
        }

        private static Value GetXactId(Post post)
        {
            return Value.Get(post.XactId);
        }

        private static Value GetReal(Post post)
        {
            return Value.Get(!post.Flags.HasFlag(SupportsFlagsEnum.POST_VIRTUAL));
        }

        private static Value GetVirtual(Post post)
        {
            return Value.Get(post.Flags.HasFlag(SupportsFlagsEnum.POST_VIRTUAL));
        }

        private static Value GetTotal(Post post)
        {
            if (post.HasXData && !Value.IsNullOrEmpty(post.XData.Total))
                return post.XData.Total;
            else if (post.Amount.IsEmpty)
                return Value.Zero;
            else
                return Value.Get(post.Amount);
        }

        private static Value GetUseDirectAmount(Post post)
        {
            return Value.Get(post.HasXData && post.XData.DirectAmt);
        }

        /// <summary>
        /// Ported from get_value_date
        /// </summary>
        private static Value GetValueDate(Post post)
        {
            if (post.HasXData)
            {
                PostXData xdata = post.XData;
                if (!xdata.ValueDate.IsNotADate())
                    return Value.Get(xdata.ValueDate);
            }
            return Value.Get(post.GetDate());
        }

        private void CreateLookupItems()
        {            
            // a
            LookupItems.MakeFunctor("a", scope => GetWrapper((CallScope)scope, p => GetAmount(p)), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("amount", scope => GetWrapper((CallScope)scope, p => GetAmount(p)), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("account", scope => GetAccount((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("account_base", scope => GetWrapper((CallScope)scope, p => GetAccountBase(p)), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("account_id", scope => GetWrapper((CallScope)scope, p => GetAccountId(p)), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("any", scope => FnAny((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("all", scope => FnAll((CallScope)scope), SymbolKindEnum.FUNCTION);

            // b
            LookupItems.MakeFunctor("b", scope => GetWrapper((CallScope)scope, p => GetCost(p)), SymbolKindEnum.FUNCTION);

            // c
            LookupItems.MakeFunctor("code", scope => GetWrapper((CallScope)scope, p => GetCode(p)), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("cost", scope => GetWrapper((CallScope)scope, p => GetCost(p)), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("cost_calculated", scope => GetWrapper((CallScope)scope, p => GetIsCostCalculated(p)), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("count", scope => GetWrapper((CallScope)scope, p => GetCount(p)), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("calculated", scope => GetWrapper((CallScope)scope, p => GetIsCalculated(p)), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("commodity", scope => GetCommodity((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("checkin", scope => GetWrapper((CallScope)scope, p => GetCheckin(p)), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("checkout", scope => GetWrapper((CallScope)scope, p => GetCheckout(p)), SymbolKindEnum.FUNCTION);

            // d
            LookupItems.MakeFunctor("display_account", scope => GetDisplayAccount((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("depth", scope => GetWrapper((CallScope)scope, p => GetAccountDepth(p)), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("datetime", scope => GetWrapper((CallScope)scope, p => GetDateTime(p)), SymbolKindEnum.FUNCTION);

            // h
            LookupItems.MakeFunctor("has_cost", scope => GetWrapper((CallScope)scope, p => GetHasCost(p)), SymbolKindEnum.FUNCTION);

            // i
            LookupItems.MakeFunctor("index", scope => GetWrapper((CallScope)scope, p => GetCount(p)), SymbolKindEnum.FUNCTION);

            // m
            LookupItems.MakeFunctor("magnitude", scope => GetWrapper((CallScope)scope, p => GetMagnitude(p)), SymbolKindEnum.FUNCTION);

            // n
            LookupItems.MakeFunctor("n", scope => GetWrapper((CallScope)scope, p => GetCount(p)), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("note", scope => GetWrapper((CallScope)scope, p => GetNote(p)), SymbolKindEnum.FUNCTION);

            // p
            LookupItems.MakeFunctor("post", scope => GetWrapper((CallScope)scope, p => Value.ScopeValue(p)), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("payee", scope => GetWrapper((CallScope)scope, p => Value.StringValue(p.Payee)), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("primary", scope => GetWrapper((CallScope)scope, p => GetCommodityIsPrimary(p)), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("price", scope => GetWrapper((CallScope)scope, p => GetPrice(p)), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("parent", scope => GetWrapper((CallScope)scope, p => GetXact(p)), SymbolKindEnum.FUNCTION);

            // r
            LookupItems.MakeFunctor("real", scope => GetWrapper((CallScope)scope, p => GetReal(p)), SymbolKindEnum.FUNCTION);

            // t
            LookupItems.MakeFunctor("total", scope => GetWrapper((CallScope)scope, p => GetTotal(p)), SymbolKindEnum.FUNCTION);

            // u
            LookupItems.MakeFunctor("use_direct_amount", scope => GetWrapper((CallScope)scope, p => GetUseDirectAmount(p)), SymbolKindEnum.FUNCTION);

            // v
            LookupItems.MakeFunctor("virtual", scope => GetWrapper((CallScope)scope, p => GetVirtual(p)), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("value_date", scope => GetWrapper((CallScope)scope, p => GetValueDate(p)), SymbolKindEnum.FUNCTION);

            // x
            LookupItems.MakeFunctor("xact", scope => GetWrapper((CallScope)scope, p => GetXact(p)), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("xact_id", scope => GetWrapper((CallScope)scope, p => GetXactId(p)), SymbolKindEnum.FUNCTION);

            // N
            LookupItems.MakeFunctor("N", scope => GetWrapper((CallScope)scope, p => GetCount(p)), SymbolKindEnum.FUNCTION);

            // O
            LookupItems.MakeFunctor("O", scope => GetWrapper((CallScope)scope, p => GetTotal(p)), SymbolKindEnum.FUNCTION);

            // R
            LookupItems.MakeFunctor("R", scope => GetWrapper((CallScope)scope, p => GetReal(p)), SymbolKindEnum.FUNCTION);
        }

        #endregion

        private PostXData _XData = null;
        private readonly ExprOpCollection LookupItems = new ExprOpCollection();
    }
}
