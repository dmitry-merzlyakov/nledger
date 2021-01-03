// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Amounts;
using NLedger.Items;
using NLedger.Textual;
using NLedger.Times;
using NLedger.Utility;
using NLedger.Utils;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.TimeLogging
{
    public class TimeLog
    {

        /// <remarks>ported from create_timelog_xact</remarks>
        public static void CreateTimelogXact(TimeXact inEvent, TimeXact outEvent, ParseContext context)
        {
            Xact curr = new Xact()
            {
                Date = (Date)inEvent.Checkin.Date,
                Code = outEvent.Desc, // if it wasn't used above
                Payee = inEvent.Desc,
                Pos = inEvent.Position
            };

            if (!String.IsNullOrEmpty(inEvent.Note))
                curr.AppendNote(inEvent.Note, context.Scope);

            string buf = String.Format("{0}s", (outEvent.Checkin - inEvent.Checkin).TotalSeconds);

            Amount amt = new Amount();
            amt.Parse(ref buf);
            Validator.Verify(() => amt.Valid());

            Post post = new Post(inEvent.Account, amt);
            post.Flags |= SupportsFlagsEnum.POST_VIRTUAL;
            post.State = outEvent.Completed ? ItemStateEnum.Cleared : ItemStateEnum.Uncleared;
            post.Pos = inEvent.Position;
            post.Checkin = inEvent.Checkin;
            post.Checkout = outEvent.Checkin;
            curr.AddPost(post);
            inEvent.Account.AddPost(post);

            if (!context.Journal.AddXact(curr))
                throw new ParseError(ParseError.ParseError_FailedToRecordOutTimelogTransaction);
        }

        /// <summary>
        /// Ported from std::size_t clock_out_from_timelog
        /// </summary>
        public static int ClockOutFromTimeLog(IList<TimeXact> timeXacts, TimeXact outEvent, ParseContext context)
        {
            TimeXact evnt;

            if (timeXacts.Count == 1)
            {
                evnt = timeXacts.First();
                timeXacts.Clear();
            }
            else if (!timeXacts.Any())
            {
                throw new ParseError(ParseError.ParseError_TimelogCheckoutEventWithoutACheckIn);
            }
            else if (outEvent.Account == null)
            {
                throw new ParseError(ParseError.ParseError_WhenMultipleCheckinsAreActiveCheckingOutRequiresAnAccount);
            }
            else
            {
                evnt = timeXacts.FirstOrDefault(tx => tx.Account == outEvent.Account);
                if (evnt == null)
                    throw new ParseError(ParseError.ParseError_TimelogCheckoutEventDoesNotMatchAnyCurrentCheckins);
                timeXacts.Remove(evnt);
            }

            if (evnt.Checkin == default(DateTime))
                throw new ParseError(ParseError.ParseError_TimelogCheckinHasNoCorrespondingCheckout);
            if (outEvent.Checkin == default(DateTime))
                throw new ParseError(ParseError.ParseError_TimelogCheckoutHasNoCorrespondingCheckin);

            if (outEvent.Checkin < evnt.Checkin)
                throw new ParseError(ParseError.ParseError_TimelogCheckoutDateLessThanCorrespondingCheckin);

            if (!String.IsNullOrEmpty(outEvent.Desc) && String.IsNullOrEmpty(evnt.Desc))
            {
                evnt.Desc = outEvent.Desc;
                outEvent.Desc = String.Empty;
            }

            if (!String.IsNullOrEmpty(outEvent.Note) && String.IsNullOrEmpty(evnt.Note))
                evnt.Note = outEvent.Note;

            if (!context.Journal.DayBreak)
            {
                CreateTimelogXact(evnt, outEvent, context);
                return 1;
            }
            else
            {
                TimeXact begin = new TimeXact(evnt);
                int xactCount = 0;

                while(begin.Checkin < outEvent.Checkin)
                {
                    Logger.Current.Debug(DebugTimeLog, () => String.Format("begin.checkin: {0}", begin.Checkin));
                    DateTime daysEnd = begin.Checkin.Date.AddDays(1);
                    Logger.Current.Debug(DebugTimeLog, () => String.Format("days_end: {0}", daysEnd));

                    if (outEvent.Checkin <= daysEnd)
                    {
                        CreateTimelogXact(begin, outEvent, context);
                        ++xactCount;
                        break;
                    }
                    else
                    {
                        TimeXact end = new TimeXact(outEvent);
                        end.Checkin = daysEnd;
                        Logger.Current.Debug(DebugTimeLog, () => String.Format("end.checkin: {0}", end.Checkin));
                        CreateTimelogXact(begin, end, context);
                        ++xactCount;

                        begin.Checkin = end.Checkin;
                    }
                }
                return xactCount;
            }
        }

        public TimeLog(ParseContext context)
        {
            Context = context;
            TimeXacts = new List<TimeXact>();
        }

        public IList<TimeXact> TimeXacts { get; private set; }
        public ParseContext Context { get; private set; }

        /// <remarks>ported from clock_in</remarks>
        public void ClockIn (TimeXact evnt)
        {
            if (TimeXacts.Any(xt => xt.Account == evnt.Account))
                throw new ParseError(ParseError.ParseError_TimelogCannotDoubleCheckinToTheSameAccount);

            TimeXacts.Add(evnt);
        }

        /// <remarks>ported from clock_out</remarks>
        public int ClockOut(TimeXact evnt)
        {
            if (!TimeXacts.Any())
                throw new ParseError(ParseError.ParseError_TimelogCheckoutEventWithoutACheckin);

            return ClockOutFromTimeLog(TimeXacts, evnt, Context);
        }

        public void Close()
        {
            if (TimeXacts.Any())
            {
                IEnumerable<Account> accounts = TimeXacts.Select(tx => tx.Account);

                foreach(Account account in accounts)
                {
                    Logger.Current.Debug(DebugTimeLog, () => String.Format("Clocking out from account {0}", account.FullName));
                    Context.Count += ClockOutFromTimeLog(TimeXacts, new TimeXact(null, TimesCommon.Current.CurrentTime, false, account), Context);
                }
                if (TimeXacts.Any())
                    throw new InvalidOperationException("assert(time_xacts.empty());");
            }
        }

        private const string DebugTimeLog = "timelog";
    }
}
