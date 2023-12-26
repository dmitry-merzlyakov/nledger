// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Commodities;
using NLedger.Expressions;
using NLedger.Items;
using NLedger.Scopus;
using NLedger.Textual;
using NLedger.Utility;
using NLedger.Utils;
using NLedger.Values;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Journals
{
    /// <summary>
    /// Ported from journal_t (journal.h)
    /// </summary>
    public class Journal
    {
        public Journal()
        {
            Master = new Account();
            PayeeUUIDMapping = new Dictionary<string, string>();
            AccountAliases = new Dictionary<string, Account>();
            PayeesForUnknownAccounts = new List<Tuple<Mask, Account>>();
            ChecksumMapping = new Dictionary<string, Xact>();
            PayeeAliasMappings = new List<Tuple<Mask,string>>();

            Xacts = new List<Xact>();
            AutoXacts = new List<AutoXact>();
            PeriodXacts = new List<PeriodXact>();

            CheckingStyle = JournalCheckingStyleEnum.CHECK_NORMAL;

            Sources = new List<JournalFileInfo>();
            KnownTags = new HashSet<string>();
            KnownPayees = new HashSet<string>();
            TagCheckExprsMap = new MultiMap<string, CheckExprPair>();
        }


        public bool NoAliases { get; set; }
        public bool RecursiveAliases { get; set; }
        public bool DayBreak { get; set; }
        public bool CheckPayees { get; set; }
        public JournalCheckingStyleEnum CheckingStyle { get; set; }
        public Expr ValueExpr { get; set; }

        public Account Master { get; set; }
        public Account Bucket { get; set; }
        public IDictionary<string, Account> AccountAliases { get; private set; }
        public IDictionary<string, string> PayeeUUIDMapping { get; private set; }
        public IList<Tuple<Mask,Account>> PayeesForUnknownAccounts { get; private set; }
        public IDictionary<string, Xact> ChecksumMapping { get; private set; }
        public IList<Tuple<Mask, string>> PayeeAliasMappings { get; private set; }

        public IList<Xact> Xacts { get; private set; }
        public IList<AutoXact> AutoXacts { get; private set; }
        public IList<PeriodXact> PeriodXacts { get; private set; }

        public ParseContext CurrentContext { get; private set; }
        public IList<JournalFileInfo> Sources { get; private set; }
        public ISet<string> KnownTags { get; private set; }
        public ISet<string> KnownPayees { get; private set; }
        public IMultiMap<string, CheckExprPair> TagCheckExprsMap { get; private set; }

        /// <summary>
        /// Ported from string register_payee(const string& name, xact_t * xact);
        /// </summary>
        public string RegisterPayee(string name, Xact xact)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            string payee = null;

            if (CheckPayees && (CheckingStyle == JournalCheckingStyleEnum.CHECK_WARNING || CheckingStyle == JournalCheckingStyleEnum.CHECK_ERROR))
            {
                if (!KnownPayees.Contains(name))
                {
                    if (xact == null)
                    {
                        KnownPayees.Add(name);
                    }
                    else if (CheckingStyle == JournalCheckingStyleEnum.CHECK_WARNING)
                    {
                        CurrentContext.Warning(String.Format("Unknown payee '{0}'", name));
                    }
                    else if (CheckingStyle == JournalCheckingStyleEnum.CHECK_ERROR)
                    {
                        throw new ParseError(String.Format("Unknown payee '{0}'", name));
                    }
                }
            }

            foreach (var payeeAliasMapping in PayeeAliasMappings)
            {
                if (payeeAliasMapping.Item1.Match(name))
                {
                    payee = payeeAliasMapping.Item2;
                    break;
                }
            }

            return payee ?? name;
        }

        /// <summary>
        /// Ported from account_t * journal_t::register_account(const string& name,
        /// </summary>
        public Account RegisterAccount(string name, Post post, Account masterAccount = null)
        {
            // If there are any account aliases, substitute before creating an account object.
            Account result = ExpandAliases(name);

            // Create the account object and associate it with the journal; this is registering the account.
            if (result == null)
                result = masterAccount.FindAccount(name);

            // If the account name being registered is "Unknown", check whether
            // the payee indicates an account that should be used.
            if (result.Name == Account.UnknownName && post != null && post.Xact != null)
            {
                Tuple<Mask, Account> tuple = PayeesForUnknownAccounts.FirstOrDefault(t => t.Item1.Match(post.Xact.Payee));
                if (tuple != null)
                    result = tuple.Item2;
            }

            // Now that we have an account, make certain that the account is
            // "known", if the user has requested validation of that fact.
            if (CheckingStyle == JournalCheckingStyleEnum.CHECK_WARNING || CheckingStyle == JournalCheckingStyleEnum.CHECK_ERROR)
            {
                if (!result.IsKnownAccount)
                {
                    if (post == null)
                    {
                        result.IsKnownAccount = true;
                    } 
                    else if (CheckingStyle == JournalCheckingStyleEnum.CHECK_WARNING)
                    {
                        CurrentContext.Warning(String.Format("Unknown account '{0}'", result.FullName));
                    }
                    else if (CheckingStyle == JournalCheckingStyleEnum.CHECK_ERROR)
                    {
                        throw new ParseError(String.Format("Unknown account '{0}'", result.FullName));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Ported from void journal_t::register_commodity(commodity_t& comm,
        /// </summary>
        public void RegisterCommodity(Commodity commodity, Post post = null)
        {
            if (CheckingStyle == JournalCheckingStyleEnum.CHECK_WARNING || CheckingStyle == JournalCheckingStyleEnum.CHECK_ERROR)
            {
                if (!commodity.Flags.HasFlag(CommodityFlagsEnum.COMMODITY_KNOWN))
                {
                    if (post == null)  // Porting note: it is equal "context.which() == 0" assuming that we never deal with xact
                    {
                        commodity.Flags |= CommodityFlagsEnum.COMMODITY_KNOWN;
                    }
                    else if (CheckingStyle == JournalCheckingStyleEnum.CHECK_WARNING)
                    {
                        CurrentContext.Warning(String.Format("Unknown commodity '{0}'", commodity));
                    }
                    else if (CheckingStyle == JournalCheckingStyleEnum.CHECK_ERROR)
                    {
                        throw new ParseError(String.Format("Unknown commodity '{0}'", commodity));
                    }
                }
            }
        }

        // Aliases are expanded recursively, so if both alias Foo=Bar:Foo and
        // alias Bar=Baaz:Bar are in effect, first Foo will be expanded to Bar:Foo,
        // then Bar:Foo will be expanded to Baaz:Bar:Foo.
        // The expansion loop keeps a list of already expanded names in order to
        // prevent infinite excursion. Each alias may only be expanded at most once.
        public Account ExpandAliases(string name)
        {
            Account result = null;

            if (NoAliases)
                return result;

            if (AccountAliases.Any())
            {
                IList<string> alreadySeen = new List<string>();
                bool keepExpanding = true;
                // loop until no expansion can be found
                do
                {
                    Account found;
                    if (AccountAliases.TryGetValue(name, out found))
                    {
                        if (alreadySeen.Contains(name))
                            throw new InvalidOperationException(String.Format("Infinite recursion on alias expansion for {0}", name));
                        alreadySeen.Add(name);
                        result = found;
                        name = result.FullName;
                    }
                    else
                    {
                        // only check the very first account for alias expansion, in case
                        // that can be expanded successfully
                        int colon = name.IndexOf(":");
                        if (colon >= 0)
                        {
                            string firstAccountName = name.Substring(0, colon);
                            if (AccountAliases.TryGetValue(firstAccountName, out found))
                            {
                                if (alreadySeen.Contains(firstAccountName))
                                    throw new InvalidOperationException(String.Format("Infinite recursion on alias expansion for {0}", firstAccountName));
                                alreadySeen.Add(firstAccountName);
                                result = FindAccount(found.FullName + name.Substring(colon));
                                name = result.FullName;
                            }
                            else
                            {
                                keepExpanding = false;
                            }
                        }
                        else
                        {
                            keepExpanding = false;
                        }
                    }

                } while (keepExpanding && RecursiveAliases);
            }

            return result;
        }

        public Account FindAccount(string name, bool autoCreate = true)
        {
            return Master.FindAccount(name, autoCreate);
        }

        public Account FindAccountRe(string regexp)
        {
            return Master.FindAccountRe(regexp);
        }

        /// <summary>
        /// ported from journal_t::add_account
        /// </summary>
        public void AddAccount(Account acct)
        {
            Master.AddAccount(acct);
        }

        /// <summary>
        /// ported from journal_t::remove_account
        /// </summary>
        public bool RemoveAccount(Account acct)
        {
            return Master.RemoveAccount(acct);
        }

        public int Read(ParseContextStack context)
        {
            int count = 0;
            try
            {
                ParseContext current = context.GetCurrent();
                CurrentContext = current;

                current.Count = 0;
                if (current.Scope == null)
                    current.Scope = Scope.DefaultScope;

                if (current.Scope == null)
                    throw new RuntimeError(String.Format(RuntimeError.ErrorMessageNoDefaultScopeInWhichToReadJournalFile, current.PathName));

                if (current.Master == null)
                    current.Master = Master;

                count = ReadTextual(context);
                if (count > 0)
                {
                    if (!String.IsNullOrEmpty(current.PathName))
                        Sources.Add(new JournalFileInfo(current.PathName));
                    else
                        Sources.Add(new JournalFileInfo());
                }
            }
            catch
            {
                ClearXData();
                CurrentContext = null;
                throw;
            }

            // xdata may have been set for some accounts and transaction due to the use
            // of balance assertions or other calculations performed in valexpr-based
            // posting amounts.
            ClearXData();

            return count;
        }

        /// <summary>
        /// Ported from journal_t::read_textual(parse_context_stack_t& context_stack)
        /// </summary>
        public int ReadTextual(ParseContextStack contextStack)
        {
            var trace = Logger.Current.TraceContext(TimerName.ParsingTotal, 1)?.Message("Total time spent parsing text:").Start(); // TRACE_START

            TextualParser instance = new TextualParser(contextStack, contextStack.GetCurrent(), null, CheckingStyle == JournalCheckingStyleEnum.CHECK_PERMISSIVE);
            instance.ApplyStack.PushFront("account", contextStack.GetCurrent().Master);
            instance.Parse();

            trace?.Stop(); // TRACE_STOP

            // Apply any deferred postings at this time
            Master.ApplyDeferredPosts();

            // These tracers were started in textual.cc
            Logger.Current.TraceContext(TimerName.XactText, 1)?.Finish();   // TRACE_FINISH
            Logger.Current.TraceContext(TimerName.XactDetails, 1)?.Finish();
            Logger.Current.TraceContext(TimerName.XactPosts, 1)?.Finish();
            Logger.Current.TraceContext(TimerName.Xacts, 1)?.Finish();
            Logger.Current.TraceContext(TimerName.InstanceParse, 1)?.Finish();  // report per-instance timers
            Logger.Current.TraceContext(TimerName.ParsingTotal, 1)?.Finish();

            if (contextStack.GetCurrent().Errors > 0)
                throw new CountError(contextStack.GetCurrent().Errors, contextStack.GetCurrent().Last);

            return contextStack.GetCurrent().Count;
        }

        public bool AddXact(Xact xact)
        {
            if (xact == null)
                throw new ArgumentNullException("xact");

            xact.Journal = this;

            if (!xact.FinalizeXact())
            {
                xact.Journal = null;
                return false;
            }

            ExtendXact(xact);
            CheckAllMetadata(xact);
            
            foreach(Post post in xact.Posts)
            {
                Post.ExtendPost(post, this);
                CheckAllMetadata(post);
            }

            // If a transaction with this UUID has already been seen, simply do
            // not add this one to the journal.  However, all automated checks
            // will have been performed by extend_xact, so asserts can still be
            // applied to it.
            Value refVal = xact.GetTag("UUID");
            if (!Value.IsNullOrEmpty(refVal))
            {
                string uuid = refVal.AsString;
                if (ChecksumMapping.ContainsKey(uuid))
                {
                    // This UUID has been seen before; apply any postings which the
                    // earlier version may have deferred.
                    foreach(Post post in xact.Posts)
                    {
                        Account acct = post.Account;
                        IEnumerable<Post> deferredPosts = acct.GetDeferredPosts(uuid);
                        if (deferredPosts != null)
                        {
                            foreach (Post rpost in deferredPosts)
                                if (acct == rpost.Account)
                                    acct.AddPost(rpost);
                            acct.DeleteDeferredPosts(uuid);
                        }
                    }

                    Xact other = ChecksumMapping[uuid];

                    // Copy the two lists of postings (which should be relatively
                    // short), and make sure that the intersection is the empty set
                    // (i.e., that they are the same list).
                    IEnumerable<Post> thisPosts = xact.Posts.OrderBy(p => p.Account.FullName);
                    IEnumerable<Post> otherPosts = other.Posts.OrderBy(p => p.Account.FullName);
                    bool match = !thisPosts.Except(otherPosts, EquivalentPostingComparer.Current).Any();
                    if (!match || thisPosts.Count() != otherPosts.Count())
                    {
                        ErrorContext.Current.AddErrorContext("While comparing this previously seen transaction:");
                        ErrorContext.Current.AddErrorContext(ErrorContext.SourceContext(other.Pos.PathName, other.Pos.BegPos, other.Pos.EndPos, "> "));
                        ErrorContext.Current.AddErrorContext("to this later transaction:");
                        ErrorContext.Current.AddErrorContext(ErrorContext.SourceContext(xact.Pos.PathName, xact.Pos.BegPos, xact.Pos.EndPos, "> "));
                        throw new RuntimeError(RuntimeError.ErrorMessageTransactionsWithTheSameUUIDmustHaveEquivalentPostings);
                    }

                    xact.Journal = null;
                    return false;
                }
                else
                {
                    ChecksumMapping.Add(uuid, xact);
                }
            }

            Xacts.Add(xact);
            return true;
        }

        /// <summary>
        /// ported from journal_t::remove_xact
        /// </summary>
        public bool RemoveXact(Xact xact)
        {
            var found = Xacts.Remove(xact);
            if (found)
                xact.Journal = null;

            return found;
        }

        /// <summary>
        /// Ported from journal_t::has_xdata
        /// </summary>
        public bool HasXData()
        {
            return Xacts.Any(x => x.HasXData) || AutoXacts.Any(x => x.HasXData) || PeriodXacts.Any(x => x.HasXData) || Master.HasXData || Master.ChildrenWithXData();
        }

        public void ClearXData()
        {
            foreach (Xact xact in Xacts)
                if (!xact.Flags.HasFlag(SupportsFlagsEnum.ITEM_TEMP))
                    xact.ClearXData();

            foreach (AutoXact xact in AutoXacts)
                if (!xact.Flags.HasFlag(SupportsFlagsEnum.ITEM_TEMP))
                    xact.ClearXData();

            foreach (PeriodXact xact in PeriodXacts)
                if (!xact.Flags.HasFlag(SupportsFlagsEnum.ITEM_TEMP))
                    xact.ClearXData();

            Master.ClearXData();
        }

        public void ExtendXact(XactBase xact)
        {
            foreach (AutoXact autoXact in AutoXacts)
                autoXact.ExtendXact(xact, CurrentContext);
        }

        public void CheckAllMetadata(Item item)
        {
            if (item != null && item.GetMetadata() != null)
            {
                foreach (KeyValuePair<string, ItemTag> pair in item.GetMetadata())
                    RegisterMetadata(pair.Key, pair.Value.Value, item);
            }
        }

        /// <summary>
        /// Porte from void journal_t::register_metadata(const string& key
        /// </summary>
        public void RegisterMetadata(string key, Value value, Item context)
        {
            if (CheckingStyle == JournalCheckingStyleEnum.CHECK_WARNING || CheckingStyle == JournalCheckingStyleEnum.CHECK_ERROR)
            {
                if (!KnownTags.Contains(key))
                {
                    if (context == null)
                    {
                        KnownTags.Add(key);
                    }
                    else if (CheckingStyle == JournalCheckingStyleEnum.CHECK_WARNING)
                    {
                        CurrentContext.Warning(String.Format("Unknown metadata tag '{0}'", key));
                    }
                    else if (CheckingStyle == JournalCheckingStyleEnum.CHECK_ERROR)
                    {
                        throw new ParseError(String.Format("Unknown metadata tag '{0}'", key));
                    }
                }
            }

            if (!Value.IsNullOrEmpty(value))
            {
                foreach(CheckExprPair pair in TagCheckExprsMap.GetValues(key))
                {
                    BindScope boundScope = new BindScope(CurrentContext.Scope, context);
                    ValueScope valScope = new ValueScope(boundScope, value);

                    if (!pair.Expr.Calc(valScope).AsBoolean)
                    {
                        if (pair.CheckExprKind == CheckExprKindEnum.EXPR_ASSERTION)
                            throw new ParseError(String.Format(ParseError.ParseError_MetadataAssertionFailedFor, key, value, pair.Expr));
                        else
                            CurrentContext.Warning(String.Format(ParseError.ParseError_MetadataCheckFailedFor, key, value, pair.Expr));
                    }
                }
            }
        }

        /// <summary>
        /// Ported from bool journal_t::valid()
        /// </summary>
        /// <returns></returns>
        public bool Valid()
        {
            if (!Master.Valid())
            {
                Logger.Current.Debug("ledger.validate", () => "journal_t: master not valid");
                return false;
            }

            foreach(var xact in Xacts)
            {
                if (!xact.Valid())
                {
                    Logger.Current.Debug("ledger.validate", () => "journal_t: xact not valid");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Only for test purposes
        /// </summary>
        public void SetCurrentContext(ParseContext currentContext)
        {
            CurrentContext = currentContext;
        }

        /// <summary>
        /// Ported from is_equivalent_posting(post_t * left, post_t * right)
        /// </summary>
        private class EquivalentPostingComparer : IEqualityComparer<Post>
        {
            public static EquivalentPostingComparer Current = new EquivalentPostingComparer();

            public bool Equals(Post x, Post y)
            {
                if (x.Account != y.Account)
                    return false;

                if (x.Amount != y.Amount)
                    return false;

                return true;
            }

            public int GetHashCode(Post post)
            {
                return post.Amount.GetHashCode() ^ post.Amount.GetHashCode();
            }
        }
    }
}
