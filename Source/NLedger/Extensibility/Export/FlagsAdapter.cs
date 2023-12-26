// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Commodities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Extensibility.Export
{
    public static class FlagsAdapter
    {
        public static FlagsConverter<Annotate.Annotation> AnnotationFlagsAdapter()
        {
            return new FlagsConverter<Annotate.Annotation>().
                AddMapping(ExportedConsts.ANNOTATION_PRICE_CALCULATED, a => a.IsPriceCalculated, (a, v) => a.IsPriceCalculated = v).
                AddMapping(ExportedConsts.ANNOTATION_PRICE_FIXATED, a => a.IsPriceFixated, (a, v) => a.IsPriceFixated = v).
                AddMapping(ExportedConsts.ANNOTATION_PRICE_NOT_PER_UNIT, a => a.IsPriceNotPerUnit, (a, v) => a.IsPriceNotPerUnit = v).
                AddMapping(ExportedConsts.ANNOTATION_DATE_CALCULATED, a => a.IsDateCalculated, (a, v) => a.IsDateCalculated = v).
                AddMapping(ExportedConsts.ANNOTATION_TAG_CALCULATED, a => a.IsTagCalculated, (a, v) => a.IsTagCalculated = v).
                AddMapping(ExportedConsts.ANNOTATION_VALUE_EXPR_CALCULATED, a => a.IsValueExprCalculated, (a, v) => a.IsValueExprCalculated = v);
        }

        public static FlagsConverter<Accounts.Account> AccountFlagsAdapter()
        {
            return new FlagsConverter<Accounts.Account>().
                AddMapping(ExportedConsts.ACCOUNT_KNOWN, a => a.IsKnownAccount, (a, v) => a.IsKnownAccount = v).
                AddMapping(ExportedConsts.ACCOUNT_TEMP, a => a.IsTempAccount, (a, v) => a.IsTempAccount = v).
                AddMapping(ExportedConsts.ACCOUNT_GENERATED, a => a.IsGeneratedAccount, (a, v) => a.IsGeneratedAccount = v);
        }

        public static FlagsConverter<NLedger.PostXData> PostXDataFlagsAdapter()
        {
            return new FlagsConverter<NLedger.PostXData>().
                AddMapping(ExportedConsts.POST_EXT_RECEIVED, x => x.Received, (x, v) => x.Received = v).
                AddMapping(ExportedConsts.POST_EXT_HANDLED, x => x.Handled, (x, v) => x.Handled = v).
                AddMapping(ExportedConsts.POST_EXT_DISPLAYED, x => x.Displayed, (x, v) => x.Displayed = v).
                AddMapping(ExportedConsts.POST_EXT_DIRECT_AMT, x => x.DirectAmt, (x, v) => x.DirectAmt = v).
                AddMapping(ExportedConsts.POST_EXT_SORT_CALC, x => x.SortCalc, (x, v) => x.SortCalc = v).
                AddMapping(ExportedConsts.POST_EXT_COMPOUND, x => x.Compound, (x, v) => x.Compound = v).
                AddMapping(ExportedConsts.POST_EXT_VISITED, x => x.Visited, (x, v) => x.Visited = v).
                AddMapping(ExportedConsts.POST_EXT_MATCHES, x => x.Matches, (x, v) => x.Matches = v).
                AddMapping(ExportedConsts.POST_EXT_CONSIDERED, x => x.Considered, (x, v) => x.Considered = v);
        }

        public static FlagsConverter<Accounts.AccountXData> AccountXDataFlagsAdapter()
        {
            return new FlagsConverter<Accounts.AccountXData>().
                AddMapping(ExportedConsts.ACCOUNT_EXT_SORT_CALC, a => a.SortCalc, (a, v) => a.SortCalc = v).
                AddMapping(ExportedConsts.ACCOUNT_EXT_HAS_NON_VIRTUALS, a => a.HasNonVirtuals, (a, v) => a.HasNonVirtuals = v).
                AddMapping(ExportedConsts.ACCOUNT_EXT_HAS_UNB_VIRTUALS, a => a.HasUnbVirtuals, (a, v) => a.HasUnbVirtuals = v).
                AddMapping(ExportedConsts.ACCOUNT_EXT_AUTO_VIRTUALIZE, a => a.AutoVirtualize, (a, v) => a.AutoVirtualize = v).
                AddMapping(ExportedConsts.ACCOUNT_EXT_VISITED, a => a.Visited, (a, v) => a.Visited = v).
                AddMapping(ExportedConsts.ACCOUNT_EXT_MATCHING, a => a.Matching, (a, v) => a.Matching = v).
                AddMapping(ExportedConsts.ACCOUNT_EXT_TO_DISPLAY, a => a.ToDisplay, (a, v) => a.ToDisplay = v).
                AddMapping(ExportedConsts.ACCOUNT_EXT_DISPLAYED, a => a.Displayed, (a, v) => a.Displayed = v);
        }

        public static bool AccountHasXFlags(Accounts.Account account, uint flags) => account?.HasXFlags(axd => AccountXDataFlagsAdapter().HasFlags(axd, flags)) ?? false;

        public static int EnumToInt(Enum enumValue) => ((IConvertible)enumValue).ToInt32(System.Globalization.CultureInfo.CurrentCulture);
        public static int EnumToInt(int intValue) => intValue;  // Method substitution for backward compatibility with PythonNet 2.5.x.
    }
}
