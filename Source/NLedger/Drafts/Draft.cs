// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Amounts;
using NLedger.Commodities;
using NLedger.Expressions;
using NLedger.Items;
using NLedger.Journals;
using NLedger.Scopus;
using NLedger.Textual;
using NLedger.Times;
using NLedger.Values;
using NLedger.Xacts;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NLedger.Utils;

namespace NLedger.Drafts
{
    // draft_t
    public class Draft : ExprBase<Value>
    {
        public Draft(Value args)
            : base()
        {
            if (!Value.IsNullOrEmpty(args))
                ParseArgs(args);
        }

        public override string Dump()
        {
            return Tmpl != null ? Tmpl.Dump() : String.Empty;
        }

        protected DraftXactTemplate Tmpl { get; set; }

        /// <summary>
        /// Ported from draft_t::parse_args
        /// </summary>
        protected void ParseArgs(Value args)
        {
            Match what = Match.Empty;
            bool checkForDate = true;

            Tmpl = new DraftXactTemplate();

            DayOfWeek? weekday;
            DraftXactPostTemplate post = null;

            IList<Value> argsSequence = args.AsSequence;
            Value endVal = argsSequence.LastOrDefault();
            for(int index=0; index<argsSequence.Count; index++)
            {
                Value val = argsSequence[index];

                if (checkForDate && (what = DateMask.Match(val.AsString)).Length > 0)
                {
                    Tmpl.Date = TimesCommon.Current.ParseDate(what.Value);
                    checkForDate = false;
                }
                else if (checkForDate && (weekday = DateParserLexer.StringToDayOfWeek(what.Value)).HasValue)
                {
                    Date date = TimesCommon.Current.CurrentDate.AddDays(-1);
                    while (date.DayOfWeek != weekday)
                        date = date.AddDays(-1);
                    Tmpl.Date = date;
                    checkForDate = false;
                }
                else
                {
                    string arg = val.AsString;

                    if (arg == "at")
                    {
                        if (val == endVal)
                            throw new RuntimeError(RuntimeError.ErrorMessageInvalidXactCommandArguments);
                        Tmpl.PayeeMask = new Mask(argsSequence[++index].AsString);
                    }
                    else if (arg == "to" || arg == "from")
                    {
                        if (post == null || post.AccountMask != null)
                            Tmpl.Posts.Add(post = new DraftXactPostTemplate());
                        if (val == endVal)
                            throw new RuntimeError(RuntimeError.ErrorMessageInvalidXactCommandArguments);
                        post.AccountMask = new Mask(argsSequence[++index].AsString);
                        post.From = arg == "from";
                    }
                    else if (arg == "on")
                    {
                        if (val == endVal)
                            throw new RuntimeError(RuntimeError.ErrorMessageInvalidXactCommandArguments);
                        Tmpl.Date = TimesCommon.Current.ParseDate(argsSequence[++index].AsString);
                        checkForDate = false;
                    }
                    else if (arg == "code")
                    {
                        if (val == endVal)
                            throw new RuntimeError(RuntimeError.ErrorMessageInvalidXactCommandArguments);
                        Tmpl.Code = argsSequence[++index].AsString;
                    }
                    else if (arg == "note")
                    {
                        if (val == endVal)
                            throw new RuntimeError(RuntimeError.ErrorMessageInvalidXactCommandArguments);
                        Tmpl.Note = argsSequence[++index].AsString;
                    }
                    else if (arg == "rest")
                    {
                        // just ignore this argument
                    }
                    else if (arg == "@" || arg == "@@")
                    {
                        Amount cost = new Amount();
                        post.CostOperator = arg;
                        if (val == endVal)
                            throw new RuntimeError(RuntimeError.ErrorMessageInvalidXactCommandArguments);
                        arg = argsSequence[++index].AsString;
                        if (!cost.Parse(ref arg, AmountParseFlagsEnum.PARSE_SOFT_FAIL | AmountParseFlagsEnum.PARSE_NO_MIGRATE))
                            throw new RuntimeError(RuntimeError.ErrorMessageInvalidXactCommandArguments);
                        post.Cost = cost;
                    }
                    else
                    {
                        // Without a preposition, it is either:
                        //
                        //  A payee, if we have not seen one
                        //  An account or an amount, if we have
                        //  An account if an amount has just been seen
                        //  An amount if an account has just been seen

                        if (Tmpl.PayeeMask == null)
                        {
                            Tmpl.PayeeMask = new Mask(arg);
                        }
                        else
                        {
                            Amount amt = new Amount();
                            Mask account = null;

                            string argToParse = arg;
                            if (!amt.Parse(ref argToParse, AmountParseFlagsEnum.PARSE_SOFT_FAIL | AmountParseFlagsEnum.PARSE_NO_MIGRATE))
                                account = new Mask(arg);

                            if (post == null || (account != null && post.AccountMask != null) || (account == null && post.Amount != null))
                                Tmpl.Posts.Add(post = new DraftXactPostTemplate());

                            if (account != null)
                                post.AccountMask = account;
                            else
                            {
                                post.Amount = amt;
                                post = null;        // an amount concludes this posting
                            }
                        }
                    }
                }
            }

            if (Tmpl.Posts.Any())
            {
                // A single account at the end of the line is the "from" account
                if (Tmpl.Posts.Count > 1 && Tmpl.Posts.Last().AccountMask != null && !(bool)Tmpl.Posts.Last().Amount)
                    Tmpl.Posts.Last().From = true;

                bool hasOnlyFrom = !Tmpl.Posts.Any(p => !p.From);
                bool hasOnlyTo = !Tmpl.Posts.Any(p => p.From);

                if (hasOnlyFrom)
                    Tmpl.Posts.Insert(0, new DraftXactPostTemplate());
                else if (hasOnlyTo)
                    Tmpl.Posts.Add(new DraftXactPostTemplate() { From = true });
            }
        }

        /// <summary>
        /// Ported from draft_t::insert
        /// </summary>
        public Xact Insert(Journal journal)
        {
            if (Tmpl == null)
                return null;

            if (Tmpl.PayeeMask == null)
                throw new RuntimeError(RuntimeError.ErrorMessageXactCommandRequiresAtLeastAPayee);

            Xact matching = null;
            Xact added = new Xact();

            Xact xact = Lookup.LookupProbableAccount(Tmpl.PayeeMask.ToString(), journal.Xacts.Reverse()).Item1;
            if (xact != null)
            {
                Logger.Current.Debug("draft.xact", () => String.Format("Found payee by lookup: transaction on line {0}", xact.Pos.BegLine));
                matching = xact;
            }
            else
            {
                matching = journal.Xacts.LastOrDefault(x => Tmpl.PayeeMask.Match(x.Payee));
                if (matching != null)
                    Logger.Current.Debug("draft.xact", () => String.Format("Found payee match: transaction on line {0}", matching.Pos.BegLine));
            }

            if (!Tmpl.Date.HasValue)
            {
                added.Date = TimesCommon.Current.CurrentDate;
                Logger.Current.Debug("draft.xact", () => "Setting date to current date");
            }
            else
            {
                added.Date = Tmpl.Date;
                Logger.Current.Debug("draft.xact", () => String.Format("Setting date to template date: {0}", Tmpl.Date));
            }

            added.State = ItemStateEnum.Uncleared;

            if (matching != null)
            {
                added.Payee = matching.Payee;
                //added->code  = matching->code;
                //added->note  = matching->note;
                Logger.Current.Debug("draft.xact", () => String.Format("Setting payee from match: {0}", added.Payee));
            }
            else
            {
                added.Payee = Tmpl.PayeeMask.ToString();
                Logger.Current.Debug("draft.xact", () => String.Format("Setting payee from template: {0}", added.Payee));
            }

            if (!String.IsNullOrEmpty(Tmpl.Code))
            {
                added.Code = Tmpl.Code;
                Logger.Current.Debug("draft.xact", () => String.Format("Now setting code from template:  {0}", added.Code));
            }

            if (!String.IsNullOrEmpty(Tmpl.Note))
            {
                added.Note = Tmpl.Note;
                Logger.Current.Debug("draft.xact", () => String.Format("Now setting note from template:  {0}", added.Note));
            }

            if (!Tmpl.Posts.Any())
            {
                if (matching != null)
                {
                    Logger.Current.Debug("draft.xact", () => "Template had no postings, copying from match");

                    foreach (Post post in matching.Posts)
                        added.AddPost(new Post(post) { State = ItemStateEnum.Uncleared });
                }
                else
                {
                    throw new RuntimeError(String.Format(RuntimeError.ErrorMessageNoAccountsAndNoPastTransactionMatchingSmth, Tmpl.PayeeMask.ToString()));
                }
            }
            else
            {
                Logger.Current.Debug("draft.xact", () => "Template had postings");
                bool anyPostHasAmount = Tmpl.Posts.Any(p => (bool)p.Amount);
                if (anyPostHasAmount)
                    Logger.Current.Debug("draft.xact", () => "  and at least one has an amount specified");

                foreach (DraftXactPostTemplate post in Tmpl.Posts)
                {
                    Post newPost = null;

                    Commodity foundCommodity = null;

                    if (matching != null)
                    {
                        if (post.AccountMask != null)
                        {
                            Logger.Current.Debug("draft.xact", () => "Looking for matching posting based on account mask");
                            Post x = matching.Posts.FirstOrDefault(p => post.AccountMask.Match(p.Account.FullName));
                            if (x != null)
                            {
                                newPost = new Post(x);
                                Logger.Current.Debug("draft.xact", () => String.Format("Founding posting from line {0}", x.Pos.BegLine));
                            }
                        }
                        else
                        {
                            if (post.From)
                            {
                                Post x = matching.Posts.LastOrDefault(p => p.MustBalance);
                                if (x != null)
                                {
                                    newPost = new Post(x);
                                    Logger.Current.Debug("draft.xact", () => "Copied last real posting from matching");
                                }
                            }
                            else
                            {
                                Post x = matching.Posts.FirstOrDefault(p => p.MustBalance);
                                if (x != null)
                                {
                                    newPost = new Post(x);
                                    Logger.Current.Debug("draft.xact", () => "Copied first real posting from matching");
                                }
                            }
                        }
                    }

                    if (newPost == null)
                    {
                        newPost = new Post();
                        Logger.Current.Debug("draft.xact", () => "New posting was NULL, creating a blank one");
                    }

                    if (newPost.Account == null)
                    {
                        Logger.Current.Debug("draft.xact", () => "New posting still needs an account");

                        if (post.AccountMask != null)
                        {
                            Logger.Current.Debug("draft.xact", () => "The template has an account mask");

                            Account acct = journal.FindAccountRe(post.AccountMask.ToString());
                            if (acct != null)
                            {
                                Logger.Current.Debug("draft.xact", () => "Found account as a regular expression");
                            }
                            else
                            {
                                acct = journal.FindAccount(post.AccountMask.ToString());
                                if (acct != null)
                                    Logger.Current.Debug("draft.xact", () => "Found (or created) account by name");
                            }

                            // Find out the default commodity to use by looking at the last
                            // commodity used in that account
                            foreach (Xact j in journal.Xacts.Reverse())
                            {
                                Post x = j.Posts.FirstOrDefault(p => p.Account == acct && !(p.Amount == null || p.Amount.IsEmpty));
                                if (x != null)
                                {
                                    newPost = new Post(x);
                                    Logger.Current.Debug("draft.xact", () => "Found account in journal postings, setting new posting");
                                    break;
                                }
                            }

                            newPost.Account = acct;
                            Logger.Current.Debug("draft.xact", () => String.Format("Set new posting's account to: {0}", acct.FullName));
                        }
                        else
                        {
                            if (post.From)
                            {
                                newPost.Account = journal.FindAccount("Liabilities:Unknown");
                                Logger.Current.Debug("draft.xact", () => "Set new posting's account to: Liabilities:Unknown");
                            }
                            else
                            {
                                newPost.Account = journal.FindAccount("Expenses:Unknown");
                                Logger.Current.Debug("draft.xact", () => "Set new posting's account to: Expenses:Unknown");
                            }
                        }
                    }
                    if (newPost.Account == null)
                        throw new InvalidOperationException("assert(new_post->account)");

                    if (newPost != null && !(newPost.Amount == null || newPost.Amount.IsEmpty))
                    {
                        foundCommodity = newPost.Amount.Commodity;

                        if (anyPostHasAmount)
                        {
                            newPost.Amount = new Amount();
                            Logger.Current.Debug("draft.xact", () => "New posting has an amount, but we cleared it");
                        }
                        else
                        {
                            anyPostHasAmount = true;
                            Logger.Current.Debug("draft.xact", () => "New posting has an amount, and we're using it");
                        }
                    }

                    if ((bool)post.Amount)
                    {
                        newPost.Amount = post.Amount;
                        Logger.Current.Debug("draft.xact", () => "Copied over posting amount");

                        if (post.From)
                        {
                            newPost.Amount.InPlaceNegate();
                            Logger.Current.Debug("draft.xact", () => "Negated new posting amount");
                        }
                    }

                    if ((bool)post.Cost)
                    {
                        if (post.Cost.Sign < 0)
                            throw new ParseError(ParseError.ParseError_PostingCostMayNotBeNegative);

                        post.Cost.InPlaceUnround();

                        if (post.CostOperator == "@")
                        {
                            // For the sole case where the cost might be uncommoditized,
                            // guarantee that the commodity of the cost after multiplication
                            // is the same as it was before.
                            Commodity costCommodity = post.Cost.Commodity;
                            post.Cost = post.Cost.Multiply(newPost.Amount);
                            post.Cost.SetCommodity(costCommodity);
                        }
                        else if (newPost.Amount.Sign < 0)
                        {
                            newPost.Cost.InPlaceNegate();
                        }

                        newPost.Cost = post.Cost;
                        Logger.Current.Debug("draft.xact", () => "Copied over posting cost");
                    }

                    if (foundCommodity!=null && !(newPost.Amount==null) && !(newPost.Amount.IsEmpty) && !(newPost.Amount.HasCommodity))
                    {
                        newPost.Amount.SetCommodity(foundCommodity);
                        Logger.Current.Debug("draft.xact", () => String.Format("Set posting amount commodity to: {0}", newPost.Amount.Commodity));

                        newPost.Amount = newPost.Amount.Rounded();
                        Logger.Current.Debug("draft.xact", () => String.Format("Rounded posting amount to: {0}", newPost.Amount));
                    }

                    added.AddPost(newPost);
                    newPost.Account.AddPost(newPost);
                    newPost.State = ItemStateEnum.Uncleared;

                    Logger.Current.Debug("draft.xact", () => "Added new posting to derived entry");
                }
            }

            if (!journal.AddXact(added))
                throw new RuntimeError(RuntimeError.ErrorMessageFailedToFinalizeDerivedTransactionCheckCommodities);

            return added;
        }

        protected override Value RealCalc(Scope scope)
        {
            throw new InvalidOperationException("assert(false)");
        }

        private static Regex DateMask = new Regex("([0-9]+(?:[-/.][0-9]+)?(?:[-/.][0-9]+))?");
    }
}
