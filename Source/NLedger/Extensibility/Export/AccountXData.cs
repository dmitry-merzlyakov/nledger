using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public class AccountXData
    {
        public static int ACCOUNT_EXT_SORT_CALC = 0x01;
        public static int ACCOUNT_EXT_HAS_NON_VIRTUALS = 0x02;
        public static int ACCOUNT_EXT_HAS_UNB_VIRTUALS = 0x04;
        public static int ACCOUNT_EXT_AUTO_VIRTUALIZE = 0x08;
        public static int ACCOUNT_EXT_VISITED = 0x10;
        public static int ACCOUNT_EXT_MATCHING = 0x20;
        public static int ACCOUNT_EXT_TO_DISPLAY = 0x40;
        public static int ACCOUNT_EXT_DISPLAYED = 0x80;

        // TBC
    }
}
