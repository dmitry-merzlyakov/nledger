using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    [Flags]
    public enum ParseFlags
    {
        Default = Amounts.AmountParseFlagsEnum.PARSE_DEFAULT,
        Partial = Amounts.AmountParseFlagsEnum.PARSE_PARTIAL,
        Single = Amounts.AmountParseFlagsEnum.PARSE_SINGLE,
        NoMigrate = Amounts.AmountParseFlagsEnum.PARSE_NO_MIGRATE,
        NoReduce = Amounts.AmountParseFlagsEnum.PARSE_NO_REDUCE,
        NoAssign = Amounts.AmountParseFlagsEnum.PARSE_NO_ASSIGN,
        OpContext = Amounts.AmountParseFlagsEnum.PARSE_OP_CONTEXT,
        SoftFail = Amounts.AmountParseFlagsEnum.PARSE_SOFT_FAIL
    }
}
