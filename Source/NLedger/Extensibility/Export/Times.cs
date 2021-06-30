using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public static class Times
    {
        public static DateTime parse_datetime(string str) => NLedger.Times.TimesCommon.Current.ParseDateTime(str);
        public static Date parse_date(string str) => NLedger.Times.TimesCommon.Current.ParseDate(str);
        public static void times_initialize() => NLedger.Times.TimesCommon.Current.TimesInitialize();
        public static void times_shutdown() => NLedger.Times.TimesCommon.Current.TimesShutdown();
    }
}
