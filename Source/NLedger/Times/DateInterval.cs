// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Times
{
    public class DateInterval : IComparable<DateInterval>
    {
        public static DateInterval operator ++ (DateInterval dateInterval)
        {
            return dateInterval.Increment();
        }

        public static bool operator == (DateInterval x, DateInterval y)
        {
            if (object.ReferenceEquals(x, null))
                return object.ReferenceEquals(y, null);
            else
                return x.Equals(y);
        }

        public static bool operator !=(DateInterval x, DateInterval y)
        {
            if (object.ReferenceEquals(x, null))
                return !object.ReferenceEquals(y, null);
            else
                return !x.Equals(y);
        }

        public static bool operator <(DateInterval x, DateInterval y)
        {
            return x != null && x.CompareTo(y) == -1;
        }

        public static bool operator >(DateInterval x, DateInterval y)
        {
            return x != null && x.CompareTo(y) == 1;
        }

        public DateInterval()
        { }

        public DateInterval(string str)
        {
            Parse(str);
        }

        public DateInterval(DateInterval dateInterval)
        {
            Assign(dateInterval);
        }

        public DateSpecifierOrRange Range { get; set; }
        public Date? Start { get; private set; }  // the real start, after adjustment
        public Date? Finish { get; private set; }  // the real end, likewise
        public bool Aligned { get; private set; }
        public Date? Next { get; private set; }
        public DateDuration Duration { get; set; }
        public Date? EndOfDuration { get; private set; }

        public Date? Begin
        {
            get { return Start ?? (Range != null ? Range.Begin : null); }
        }

        public Date? End
        {
            get { return Finish ?? (Range != null ? Range.End : null); }
        }

        public bool IsValid
        {
            get { return Start.HasValue; }
        }

        public void Parse(string str)
        {
            DateParser parser = new DateParser(str);
            Assign(parser.Parse());
        }

        public void ResolveEnd()
        {
            if (Start.HasValue && !EndOfDuration.HasValue)
                EndOfDuration = Duration.Add(Start.Value);

            if (Finish.HasValue && EndOfDuration > Finish)
                EndOfDuration = Finish;

            if (Start.HasValue && !Next.HasValue)
                Next = EndOfDuration;
        }

        public void Stabilize(Date? date = null)
        {
            if (date.HasValue)
                Logger.Debug("times.interval", () => String.Format("stabilize: with date = {0}", date));

            if (date.HasValue & !Aligned)
            {
                Logger.Debug("times.interval", () => "stabilize: date passed, but not aligned");
                if (Duration != null)
                {
                    Logger.Debug("times.interval", () => String.Format("stabilize: aligning with a duration: {0}", Duration));

                    // The interval object has not been seeded with a start date yet, so
                    // find the nearest period before on on date which fits, if possible.
                    //
                    // Find an efficient starting point for the upcoming while loop.  We
                    // want a date early enough that the range will be correct, but late
                    // enough that we don't spend hundreds of thousands of loops skipping
                    // through time.
                    Date? initialStart = Start ?? Begin;
                    Date? initialFinish = Finish ?? End;

                    if (initialStart.HasValue)
                        Logger.Debug("times.interval", () => String.Format("stabilize: initial_start  = {0}", initialStart));
                    if (initialFinish.HasValue)
                        Logger.Debug("times.interval", () => String.Format("stabilize: initial_finish = {0}", initialFinish));

                    Date when = Start ?? date.Value;

                    switch(Duration.Quantum)
                    {
                        case SkipQuantumEnum.MONTHS:
                        case SkipQuantumEnum.QUARTERS:
                        case SkipQuantumEnum.YEARS:
                            Logger.Debug("times.interval", () => "stabilize: monthly, quarterly or yearly duration");
                            // These start on most recent period start quantum before when
                            // DEBUG("times.interval", "stabilize: monthly, quarterly or yearly duration");
                            Start = DateDuration.FindNearest(when, Duration.Quantum);
                            break;

                        case SkipQuantumEnum.WEEKS:
                            // Weeks start on the beginning of week prior to 400 remainder period length
                            // Either the first quanta of the period or the last quanta of the period seems more sensible
                            // implies period is never less than 400 days not too unreasonable
                            // DEBUG("times.interval", "stabilize: weekly duration");
                            Logger.Debug("times.interval", () => "stabilize: weekly duration");
                            int period = Duration.Length * 7;
                            Start = DateDuration.FindNearest(when.AddDays(-(period + 400 % period)), Duration.Quantum);
                            break;

                        default:
                            // multiples of days have a quanta of 1 day so should not have the start date adjusted to a quanta
                            Logger.Debug("times.interval", () => "stabilize: daily duration - stable by definition");
                            Start = when;
                            break;
                    }
                    Logger.Debug("times.interval", () => String.Format("stabilize: beginning start date = {0}", Start));

                    while (Start < date)
                    {
                        DateInterval nextInterval = new DateInterval(this);
                        nextInterval++;

                        if (nextInterval.Start.HasValue && nextInterval.Start <= date)
                        {
                            Assign(nextInterval);
                        }
                        else
                        {
                            EndOfDuration = null;
                            Next = null;
                            break;
                        }
                    }

                    Logger.Debug("times.interval", () => String.Format("stabilize: proposed start date = {0}", Start));

                    if (initialStart.HasValue && (!Start.HasValue || Start < initialStart))
                    {
                        // Using the discovered start, find the end of the period
                        ResolveEnd();
                        Start = initialStart;
                        Logger.Debug("times.interval", () => "stabilize: start reset to initial start");
                    }
                    if (initialFinish.HasValue && (!Finish.HasValue || Finish > initialFinish))
                    {
                        Finish = initialFinish;
                        Logger.Debug("times.interval", () => "stabilize: finish reset to initial finish");
                    }

                    if (Start.HasValue)
                        Logger.Debug("times.interval", () => String.Format("stabilize: final start  = {0}", Start));
                    if (Finish.HasValue)
                        Logger.Debug("times.interval", () => String.Format("stabilize: final finish = {0}", Finish));

                }
                else if (Range != null)
                {
                    Start = Range.Begin;
                    Finish = Range.End;
                }
                Aligned = true;
            }
                
            // If there is no duration, then if we've reached here the date falls
            // between start and finish.
            if (Duration == null)
            {
                Logger.Debug("times.interval", () => "stabilize: there was no duration given");
                if (!Start.HasValue && !Finish.HasValue)
                    throw new DateError(DateError.ErrorMessageInvalidDateIntervalNeitherStartNorFinishNorDuration);
            }
            else
            {
                ResolveEnd();
            }
        }

        /** Find the current or next period containing date.  Returns false if
            no such period can be found.  If allow_shift is true, the default,
            then the interval may be shifted in time to find the period. */
        public bool FindPeriod(Date date, bool allowShift = true)
        {
            Stabilize(date);

            if (Finish.HasValue && date > Finish)
                return false;

            if (!Start.HasValue)
                throw new RuntimeError(RuntimeError.ErrorMessageDateIntervalIsImproperlyInitialized);

            if (date < Start)
                return false;

            if (EndOfDuration.HasValue)
            {
                if (date < EndOfDuration)
                    return true;
            }
            else
            {
                return false;
            }

            // If we've reached here, it means the date does not fall into the current
            // interval, so we must seek another interval that does match -- unless we
            // pass by date in so doing, which means we shouldn't alter the current
            // period of the interval at all.

            Date scan = Start.Value;
            Date endOfScan = EndOfDuration.Value;

            while (date >= scan && (!Finish.HasValue || scan < Finish))
            { 
                if (date < endOfScan)
                {
                    Start = scan;
                    EndOfDuration = endOfScan;
                    Next = null;

                    ResolveEnd();
                    return true;
                }
                else if (!allowShift)
                {
                    break;
                }

                scan = Duration.Add(scan);
                endOfScan = Duration.Add(scan);
            }

            return false;
        }

        public bool FindPeriod()
        {
            return FindPeriod(TimesCommon.Current.CurrentDate);
        }

        public bool WithinPeriod(Date date)
        {
            return FindPeriod(date, false);
        }

        public bool WithinPeriod()
        {
            return WithinPeriod(TimesCommon.Current.CurrentDate);
        }

        public Date? InclusiveEnd
        {
            get { return EndOfDuration.HasValue ? EndOfDuration.Value.AddDays(-1) : (Date?)null; }
        }

        public string Dump()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--- Before stabilization ---");

            if (Range != null)
                sb.AppendLine(String.Format("   range: {0}", Range));
            if (Start.HasValue)
                sb.AppendLine(String.Format("   start: {0}", TimesCommon.Current.FormatDate(Start.Value)));
            if (Finish.HasValue)
                sb.AppendLine(String.Format("  finish: {0}", TimesCommon.Current.FormatDate(Finish.Value)));
            if (Duration != null)
                sb.AppendLine(String.Format("duration: {0}", Duration));

            Date? when = Begin ?? TimesCommon.Current.CurrentDate;

            Stabilize(when);

            sb.AppendLine();
            sb.AppendLine("--- After stabilization ---");

            if (Range != null)
                sb.AppendLine(String.Format("   range: {0}", Range));
            if (Start.HasValue)
                sb.AppendLine(String.Format("   start: {0}", TimesCommon.Current.FormatDate(Start.Value)));
            if (Finish.HasValue)
                sb.AppendLine(String.Format("  finish: {0}", TimesCommon.Current.FormatDate(Finish.Value)));
            if (Duration != null)
                sb.AppendLine(String.Format("duration: {0}", Duration));

            sb.AppendLine();
            sb.AppendLine("--- Sample dates in range (max. 20) ---");

            Date lastDate = default(Date);

            for (int i = 0; i < 20 && IsValid; ++i, this.Increment())
            {
                if (!lastDate.IsNotADate() && lastDate == Start.Value)
                    break;

                sb.AppendFormat("{0,2}: {1}", i + 1, TimesCommon.Current.FormatDate(Start.Value));
                if (Duration != null)
                    sb.AppendFormat(" -- {0}", TimesCommon.Current.FormatDate(InclusiveEnd.Value));
                sb.AppendLine();

                if (Duration == null)
                    break;

                lastDate = Start.Value;
            }

            return sb.ToString();
        }

        public DateInterval Increment()
        {
            if (!Start.HasValue)
                throw new DateError(DateError.ErrorMessageCannotIncrementAnUnstartedDateInterval);

            Stabilize();

            if (Duration == null)
                throw new DateError(DateError.ErrorMessageCannotIncrementADateIntervalWithoutADuration);

            if (!Next.HasValue)
                throw new InvalidOperationException("Next");

            if (Finish.HasValue && Next >= Finish)
            {
                Start = null;
            }
            else
            {
                Start = Next;
                EndOfDuration = Duration.Add(Start.Value);
            }
            Next = null;

            ResolveEnd();

            return this;
        }

        private void Assign(DateInterval dateInterval)
        {
            Range = dateInterval.Range;
            Start = dateInterval.Start;
            Finish = dateInterval.Finish;
            Aligned = dateInterval.Aligned;
            Next = dateInterval.Next;
            Duration = dateInterval.Duration;
            EndOfDuration = dateInterval.EndOfDuration;
        }

        public override bool Equals(object obj)
        {
            return CompareTo((DateInterval)obj) == 0;
        }

        public override int GetHashCode()
        {
            return Start.GetHashCode();
        }

        public int CompareTo(DateInterval other)
        {
            if (object.ReferenceEquals(other, null))
                return -1;

            if (Start == other.Start)
                return 0;
            else if (Start < other.Start)
                return -1;
            else
                return 1;
        }
    }
}
