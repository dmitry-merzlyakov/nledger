// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger
{
    public class LogicError : Exception
    {
        public LogicError(string message) : base(message)
        { }

        public const string ErrorMessageArgumentToTraceMustBeInteger = "Argument to --trace must be an integer";
        public const string ErrorMessagePostingWithNullAmountsAccountMayBeMisspelled = "Posting with null amount's account may be misspelled:\n  \"{0}\"";
        public const string ErrorMessageOnlyOnePostingWithNullAmountAllowedPerTransaction = "Only one posting with null amount allowed per transaction";
        public const string ErrorMessageFailedToExpandHistoryReference = "Failed to expand history reference '{0}'";
        public const string ErrorMessageInvalidUseOfBackslash = "Invalid use of backslash";
        public const string ErrorMessageUnterminatedStringExpectedSmth = "Unterminated string, expected '{0}'";
        public const string ErrorMessageUnrecognizedCommand = "Unrecognized command '{0}'";
        public const string ErrorMessageFailedToFindPeriodForPeriodicTransaction = "Failed to find period for periodic transaction";
        public const string ErrorMessageEquityCannotAcceptVirtualAndNonVirtualPostingsToTheSameAccount = "'equity' cannot accept virtual and non-virtual postings to the same account";
        public const string ErrorMessageFailedToFindPeriodForIntervalReport = "Failed to find period for interval report";
        public const string ErrorMessageUsageSelectText = "Usage: select TEXT";
        public const string ErrorMessageUsageParseText = "Usage: parse TEXT";
        public const string ErrorMessageUsageFormatText = "Usage: format TEXT";
        public const string ErrorMessageUsagePeriodText = "Usage: period TEXT";
    }
}
