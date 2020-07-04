// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLedger.Utility;
using NLedger.Utils;

namespace NLedger.Times
{
    public class TimesCommon
    {
        public static TimesCommon Current
        {
            get { return MainApplicationContext.Current.TimesCommon; }
        }

        public static void SetCurrent(TimesCommon current = null)
        {
            MainApplicationContext.Current.TimesCommon = current ?? new TimesCommon();
        }

        public static string ToIsoExtendedString(DateTime dateTime)
        {
            // Posix Time & to_iso_extended_string - http://www.boost.org/doc/libs/1_49_0/doc/html/date_time/posix_time.html
            // c# options - http://stackoverflow.com/questions/114983/given-a-datetime-object-how-do-i-get-a-iso-8601-date-in-string-format
            // Note: time part is made optional
            return dateTime.TimeOfDay == TimeSpan.Zero ? dateTime.ToString("yyyy-MM-dd") : dateTime.ToString("yyyy-MM-ddTHH:mm:ss,fffffff");
        }

        public TimesCommon()
        {
            ConvertSeparatorsToSlashes = true;
            Readers = new List<DateIO>();
            TempDateIO = new Dictionary<string, DateIO>();
            TempDateTimeIO = new Dictionary<string, DateTimeIO>();
        }

        public bool ConvertSeparatorsToSlashes { get; set; }
        public DateTime? Epoch { get; set; }
        public DayOfWeek StartToWeek { get; set; }

        /// <summary>
        /// Ported from CURRENT_DATE()
        /// </summary>
        public Date CurrentDate
        {
            get { return Epoch.HasValue ? (Date)Epoch.Value.Date : (Date)DateTime.UtcNow.Date; }
        }
        public DateTime CurrentTime
        {
            get { return Epoch.HasValue ? Epoch.Value : TrueCurrentTime; }
        }
        public DateTime TrueCurrentTime
        {
            get { return DateTime.UtcNow; }
        }

        public DateTimeIO InputDateTimeIO { get; private set; }
        public DateTimeIO TimelogDateTimeIO { get; private set; }
        public IList<DateIO> Readers { get; private set; }
        public DateIO WrittenDateIO { get; private set; }
        public DateTimeIO WrittenDateTimeIO { get; private set; }
        public DateIO PrintedDateIO { get; private set; }
        public DateTimeIO PrintedDateTimeIO { get; private set; }
        public IDictionary<string, DateIO> TempDateIO { get; private set; }
        public IDictionary<string, DateTimeIO> TempDateTimeIO { get; private set; }

        public bool IsInitialized { get; private set; }

        public void TimesInitialize()
        {
            if (!IsInitialized)
            {
                InputDateTimeIO = new DateTimeIO("%Y/%m/%d %H:%M:%S", true);
                TimelogDateTimeIO = new DateTimeIO("%m/%d/%Y %H:%M:%S", true);

                WrittenDateTimeIO = new DateTimeIO("%Y/%m/%d %H:%M:%S", false);
                WrittenDateIO = new DateIO("%Y/%m/%d", false);

                PrintedDateTimeIO = new DateTimeIO("%y-%b-%d %H:%M:%S", false);
                PrintedDateIO = new DateIO("%y-%b-%d", false);

                Readers.Add(new DateIO("%m/%d", true));
                Readers.Add(new DateIO("%Y/%m/%d", true));
                Readers.Add(new DateIO("%Y/%m", true));
                Readers.Add(new DateIO("%y/%m/%d", true));
                Readers.Add(new DateIO("%Y-%m-%d", true));

                IsInitialized = true;
            }
        }

        public void TimesShutdown()
        {
            if (IsInitialized)
            {
                InputDateTimeIO = null;
                TimelogDateTimeIO = null;
                WrittenDateTimeIO = null;
                WrittenDateIO = null;
                PrintedDateTimeIO = null;
                PrintedDateIO = null;

                Readers.Clear();

                TempDateIO.Clear();
                TempDateTimeIO.Clear();

                // [DM] Reset custom epoch settings
                Epoch = null;

                IsInitialized = false;
            }
        }

        /// <summary>
        /// Ported from parse_date_mask_routine
        /// </summary>
        public Date ParseDateMaskRoutine(string dateStr, DateIO io, out DateTraits traits)
        {
            if (dateStr != null && dateStr.Length > 127)
                throw new DateError(String.Format(DateError.ErrorMessageInvalidDate, dateStr));

            string buf = dateStr;
            if (ConvertSeparatorsToSlashes)
                buf = buf.Replace('.', '/').Replace('-', '/');

            Date when = io.Parse(buf);

            traits = default(DateTraits);
            if (!when.IsNotADate())
            {
                Logger.Current.Debug(DebugTimesParse, () => String.Format("Passed date string:  {0}", dateStr));
                Logger.Current.Debug(DebugTimesParse, () => String.Format("Parsed date string:  {0}", buf));
                Logger.Current.Debug(DebugTimesParse, () => String.Format("Parsed result is:    {0}", when));
                Logger.Current.Debug(DebugTimesParse, () => String.Format("Formatted result is: {0}", io.Format(when)));

                string whenStr = io.Format(when);

                int indexP = 0;
                int indexQ = 0;
                for (; indexP < whenStr.Length && indexQ < buf.Length; indexP++, indexQ++)
                {
                    if (whenStr[indexP] != buf[indexQ] && whenStr[indexP] == '0') indexP++;
                    if (!(indexP < whenStr.Length) || whenStr[indexP] != buf[indexQ]) break;
                }

                if (indexP < whenStr.Length || indexQ < buf.Length)
                    throw new DateError(String.Format("Invalid date: {0}", dateStr));

                traits = io.Traits;

                if (!io.Traits.HasYear)
                {
                    when = new Date(CurrentDate.Year, when.Month, when.Day);

                    if (when.Month > CurrentDate.Month)
                        when = when.AddYears(-1);
                }
            }

            return when;
        }

        /// <summary>
        /// Ported from parse_date_mask
        /// </summary>
        public Date ParseDateMask(string dateStr, out DateTraits traits)
        {
            foreach(DateIO reader in Readers)
            {
                Date when = ParseDateMaskRoutine(dateStr, reader, out traits);
                if (!when.IsNotADate())
                    return when;
            }

            throw new DateError(String.Format(DateError.ErrorMessageInvalidDate, dateStr));
        }

        /// <summary>
        /// Ported from parse_date
        /// </summary>
        public Date ParseDate(string str)
        {
            DateTraits traits;
            return ParseDateMask(str, out traits);
        }

        /// <summary>
        /// Ported from parse_datetime
        /// </summary>
        public DateTime ParseDateTime(string str)
        {
            if (str == null || str.Length > 127)
                throw new DateError(String.Format("Invalid date: {0}", str));

            string buf = str;
            buf = buf.Replace('.', '/').Replace('-', '/');

            DateTime when = InputDateTimeIO.Parse(buf);
            if (when.IsNotADateTime())
            {
                when = TimelogDateTimeIO.Parse(buf);
                if (when.IsNotADateTime())
                    throw new DateError(String.Format("Invalid date/time: {0}", str));
            }
            return when;
        }

        /// <summary>
        /// Ported from format_date
        /// </summary>
        public string FormatDate(Date when, FormatTypeEnum formatType = FormatTypeEnum.FMT_PRINTED, string format = null)
        {
            if (formatType == FormatTypeEnum.FMT_WRITTEN)
                return WrittenDateIO.Format(when);
            else if (formatType == FormatTypeEnum.FMT_CUSTOM && format != null)
            {
                DateIO dateIO;
                if (TempDateIO.TryGetValue(format, out dateIO))
                {
                    return dateIO.Format(when);
                } 
                else
                {
                    dateIO = new DateIO(format, false);
                    TempDateIO.Add(format, dateIO);
                    return dateIO.Format(when);
                }
            }
            else if (formatType == FormatTypeEnum.FMT_PRINTED)
                return PrintedDateIO.Format(when);

            throw new InvalidOperationException("formatter");
        }

        /// <summary>
        /// Ported from format_datetime
        /// </summary>
        public string FormatDateTime(DateTime when, FormatTypeEnum formatType = FormatTypeEnum.FMT_PRINTED, string format = null)
        {
            if (formatType == FormatTypeEnum.FMT_WRITTEN)
                return WrittenDateTimeIO.Format(when);
            else if (formatType == FormatTypeEnum.FMT_CUSTOM && format != null)
            {
                DateTimeIO dateIO;
                if (TempDateTimeIO.TryGetValue(format, out dateIO))
                {
                    return dateIO.Format(when);
                }
                else
                {
                    dateIO = new DateTimeIO(format, false);
                    TempDateTimeIO.Add(format, dateIO);
                    return dateIO.Format(when);
                }
            }
            else if (formatType == FormatTypeEnum.FMT_PRINTED)
                return PrintedDateTimeIO.Format(when);

            throw new InvalidOperationException("formatter");
        }

        public void SetDateFormat(string format)
        {
            WrittenDateIO.SetFormat(format);
            PrintedDateIO.SetFormat(format);
        }

        public void SetDateTimeFormat(string format)
        {
            WrittenDateTimeIO.SetFormat(format);
            PrintedDateTimeIO.SetFormat(format);
        }

        public void SetInputDateFormat(string format)
        {
            Readers.Insert(0, new DateIO(format, true));
            ConvertSeparatorsToSlashes = false;
        }

        public string ShowPeriodTokens(string arg)
        {
            StringBuilder sb = new StringBuilder();

            DateParserLexer lexer = new DateParserLexer(arg);

            sb.AppendLine("--- Period expression tokens ---");

            LexerToken token;
            do {
                token = lexer.NextToken();
                sb.Append(token.Dump());
                sb.AppendLine(": " + token.ToString());
            } while (token.Kind != LexerTokenKindEnum.END_REACHED);

            return sb.ToString();
        }

        private const string DebugTimesParse = "times.parse";
    }
}
