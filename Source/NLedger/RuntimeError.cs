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

namespace NLedger
{
    public class RuntimeError : Exception
    {
        public const string ErrorMessageNoDefaultScopeInWhichToReadJournalFile = "No default scope in which to read journal file '{0}'";
        public const string ErrorMessageTransactionsWithTheSameUUIDmustHaveEquivalentPostings = "Transactions with the same UUID must have equivalent postings";
        public const string ErrorMessageDateIntervalIsImproperlyInitialized = "Date interval is improperly initialized";
        public const string ErrorMessageAttemptingToGetArgumentAtIndexSmthFromSmth = "Attempting to get argument at index {0} from {1}";
        public const string ErrorMessageAttemptingToGetIndexSmthFromSmthWithSmthElements = "Attempting to get index {0} from {1} with {2} elements";
        public const string ErrorMessageAttemptingToNailDownSmth = "Attempting to nail down {0}";
        public const string ErrorMessageTheSmthValueExpressionVariableIsNoLongerSupported = "The {0} value expression variable is no longer supported";
        public const string ErrorMessageFailedToFinalizeDerivedTransactionCheckCommodities = "Failed to finalize derived transaction (check commodities)";
        public const string ErrorMessageInvalidXactCommandArguments = "Invalid xact command arguments";
        public const string ErrorMessageXactCommandRequiresAtLeastAPayee = "'xact' command requires at least a payee";
        public const string ErrorMessageNoAccountsAndNoPastTransactionMatchingSmth = "No accounts, and no past transaction matching '{0}'";
        public const string ErrorMessageExpectedStringOrMaskForArgument1ButReceivedSmth = "Expected string or mask for argument 1, but received {0}";
        public const string ErrorMessageCouldNotFindAnAccountMatchingSmth = "Could not find an account matching '{0}'";
        public const string ErrorMessageTooFewArgumentsToFunction = "Too few arguments to function";
        public const string ErrorMessageTooManyArgumentsToFunction = "Too many arguments to function";
        public const string ErrorMessageExpectedMasksForArguments1and2ButReceivedSmthAndSmth = "Expected masks for arguments 1 and 2, but received {0} and {1}";
        public const string ErrorMessageFileToIncludeWasNotFound = "File to include was not found: \"{0}\"";
        public const string ErrorMessageEndOrEndApplyFoundButNoEnclosingApplyDirective = "'end' or 'end apply' found, but no enclosing 'apply' directive";
        public const string ErrorMessageEndApplySmthFoundButNoEnclosingApplyDirective = "end apply {0}' found, but no enclosing 'apply' directive";
        public const string ErrorMessageEndApplySmthDirectiveDoesNotMatchApplySmthDirective = "'end apply {0}' directive does not match 'apply {1}' directive";
        public const string ErrorMessageErrorInFixedDirective = "Error in fixed directive";

        public RuntimeError(string message)
            : base(message)
        { }
    }
}
