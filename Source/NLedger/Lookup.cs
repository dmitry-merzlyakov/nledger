// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Times;
using NLedger.Utility;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger
{
    public static class Lookup
    {
        public static readonly string DebugLookupAccount = "lookup.account";
        public static readonly string DebugLookup = "lookup";

        public static Tuple<Xact,Account> LookupProbableAccount (string ident, IEnumerable<Xact> iter, Account refAccount = null)
        {
            IList<Tuple<Xact, int>> scores = new List<Tuple<Xact, int>>();

            string loweredIdent = ident.ToLower();

            Logger.Debug(DebugLookupAccount, () => String.Format("Looking up identifier '{0}'", loweredIdent));
            if (refAccount != null)
                Logger.Debug(DebugLookupAccount, () => String.Format("  with reference account: '{0}'", refAccount.FullName));

            foreach(Xact xact in iter)
            {
                #if ZERO  // #if 0
                // Only consider transactions from the last two years (jww (2010-03-07):
                // make this an option)
                if ((TimesCommon.Current.CurrentDate - xact.GetDate()).Days > 700)
                    continue;
                #endif

                // An exact match is worth a score of 100 and terminates the search
                if (ident == xact.Payee)
                {
                    Logger.Debug(DebugLookup, () => "  we have an exact match, score = 100");
                    scores.Add(new Tuple<Xact,int>(xact, 100));
                    break;
                }

                string payee = xact.Payee;
                string valueKey = payee.ToLower();

                Logger.Debug(DebugLookup, () => String.Format("Considering payee: '{0}'", valueKey));

                int index = 0;
                int lastMatchPos = -1;
                int bonus = 0;
                int pos;
                int score = 0;
                IDictionary<char, int> positions = new Dictionary<char, int>();

                // Walk each letter in the source identifier
                foreach (char ch in loweredIdent)
                {
                    int addend = 0;
                    bool addedBonus = false;
                    int valueLen = valueKey.Length;

                    pos = valueKey.IndexOf(ch);

                    // Ensure that a letter which has been matched is not matched twice, so
                    // that the two x's of Exxon don't both match to the single x in Oxford.
                    // This part of the loop is very expensive, but avoids a lot of bogus
                    // matches.

                    int pi;
                    bool piFound = positions.TryGetValue(ch, out pi);
                    while (piFound && pos != -1 && pos <= pi && pi + 1 < valueLen)
                        pos = valueKey.IndexOf(ch, pi + 1);

                    if (pos != -1)
                    {
                        positions[ch] = pos;

                        // If it occurs in the same order as the source identifier -- that is,
                        // without intervening letters to break the pattern -- it's worth 10
                        // points.  Plus, an extra point is added for every letter in chains
                        // of 3 or more.

                        if (lastMatchPos == -1 ? index == 0 && pos == 0 : pos == lastMatchPos + 1)
                        {
                            Logger.Debug(DebugLookup, () => String.Format("  char '{0}' in-sequence match with bonus {1}", index, bonus));

                            addend += 10;
                            if (bonus > 2)
                                addend += bonus - 2;
                            bonus++;
                            addedBonus = true;

                            lastMatchPos = pos;
                        }

                        // If it occurs in the same general sequence as the source identifier,
                        // it's worth 5 points, plus an extra point if it's within the next 3
                        // characters, and an extra point if it's preceded by a non-alphabetic
                        // character.
                        //
                        // If the letter occurs at all in the target identifier, it's worth 1
                        // point, plus an extra point if it's within 3 characters, and an
                        // extra point if it's preceded by a non-alphabetic character.

                        else
                        {
                            bool inOrderMatch = lastMatchPos != -1 && pos > lastMatchPos;
                            Logger.Debug(DebugLookup, () => String.Format("  char '{0}' {1} match{2}", index,
                                inOrderMatch ? "in-order" : "out-of-order",
                                inOrderMatch && pos - index < 3 ? " with proximity bonus of 1" : ""));

                            if (pos < index)
                                addend += 1;
                            else
                                addend += 5;

                            if (inOrderMatch && pos - index < 3)
                                addend++;

                            // #if 0 - #if !HAVE_BOOST_REGEX_UNICODE
                            if (pos == 0 || (pos > 0 && !Char.IsLetterOrDigit(valueKey[pos - 1])))
                                addend++;

                            lastMatchPos = pos;
                        }

                        // If the letter does not appear at all, decrease the score by 1
                    }
                    else
                    {
                        lastMatchPos = -1;

                        Logger.Debug(DebugLookup, () => String.Format("  char '{0}' does not match", index));
                        addend--;
                    }

                    // Finally, decay what is to be added to the score based on its position
                    // in the word.  Since credit card payees in particular often share
                    // information at the end (such as the location where the purchase was
                    // made), we want to give much more credence to what occurs at the
                    // beginning.  Every 5 character positions from the beginning becomes a
                    // divisor for the addend.

                    if (((int)(index / 5) + 1) > 1)
                    {
                        Logger.Debug(DebugLookup, () => String.Format("  discounting the addend by / {0}", (int)(index / 5) + 1));
                        addend = (int)((double)(addend) / ((int)(index / 5) + 1));
                    }

                    Logger.Debug(DebugLookup, () => String.Format("  final addend is {0}", addend));
                    score += addend;
                    Logger.Debug(DebugLookup, () => String.Format("  score is {0}", score));

                    if (!addedBonus)
                        bonus = 0;

                    index++;
                }

                // Only consider payees with a score of 30 or greater
                if (score >= 30)
                    scores.Add(new Tuple<Xact,int>(xact, score));
            }

            // Sort the results by descending score, then look at every account ever
            // used among the top five.  Rank these by number of times used.  Lastly,
            // "decay" any latter accounts, so that we give recently used accounts a
            // slightly higher rating in case of a tie.

            scores = scores.OrderByDescending(x => x.Item2).ToList();

            int decay = 0;
            Xact bestXact = scores.Any() ? scores.First().Item1 : null;
            IDictionary<Account, int> accountUsage = new Dictionary<Account, int>();

            for (int i = 0; i < 5 && i < scores.Count; i++)
            {
                foreach(Post post in scores[i].Item1.Posts)
                {
                    if (!(post.Flags.HasFlag(SupportsFlagsEnum.ITEM_TEMP) || post.Flags.HasFlag(SupportsFlagsEnum.ITEM_GENERATED)) 
                        && post.Account != refAccount
                        && !(post.Account.IsTempAccount || post.Account.IsGeneratedAccount))
                    {
                        accountUsage[post.Account] = scores[i].Item2 - decay;
                    }
                    decay++;
                }
            }

            if (accountUsage.Any())
                return new Tuple<Xact, Account>(bestXact, accountUsage.OrderBy(x => x.Value).Last().Key);
            else
                return new Tuple<Xact, Account>(bestXact, null);
        }
    }
}
