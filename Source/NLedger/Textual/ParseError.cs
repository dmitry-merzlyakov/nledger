// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Textual
{
    public class ParseError : Exception
    {
        public static string ParseError_FailedToParseTransaction = "Failed to parse transaction";
        public static string ParseError_FailedToParseIdentifier = "Failed to parse identifier";
        public static string ParseError_PostingIsMissingCostAmount = "Posting is missing a cost amount";
        public static string ParseError_PostingCostMayNotBeNegative = "A posting's cost may not be negative";
        public static string ParseError_ExpectedCostAmount = "Expected a cost amount";
        public static string ParseError_BalanceAssignmentMustEvaluateToConstant = "Balance assignment must evaluate to a constant";
        public static string ParseError_BalanceAssertionMustEvaluateToConstant = "Balance assertion must evaluate to a constant";
        public static string ParseError_BalanceAssertionOffBySmthExpectedToSeeSmth = "Balance assertion off by {0} (expected to see {1})";
        public static string ParseError_ExpectedBalanceAssignmentOrAssertionAmount = "Expected an balance assignment/assertion amount";
        public static string ParseError_UnexpectedCharSmthNoteInlineMathRequiresParentheses = "Unexpected char '{0}' (Note: inline math requires parentheses)";
        public static string ParseError_NoJournalFileWasSpecified = "No journal file was specified (please use -f)";
        public static string ParseError_CouldNotFindSpecifiedPriceDbFile = "Could not find specified price-db file '{0}'";
        public static string ParseError_TransactionsNotAllowedInPriceHistoryFile = "Transactions not allowed in price history file";
        public static string ParseError_TransactionAssertionFailed = "Transaction assertion failed: {0}";
        public static string ParseError_TransactionCheckFailed = "Transaction check failed: {0}";
        public static string ParseError_TransactionsFoundInInitializationFile = "Transactions found in initialization file {0}";
        public static string ParseError_MetadataAssertionFailedFor = "Metadata assertion failed for ({0}: {1}): {2}";
        public static string ParseError_MetadataCheckFailedFor = "Metadata check failed for ({0}: {1}): {2}";
        public static string ParseError_CouldNotFindSpecifiedInitFile = "Could not find specified init file {0}";
        public static string ParseError_UnexpectedEndOfExpression = "Unexpected end of expression";
        public static string ParseError_UnexpectedEndOfExpressionWanted = "Unexpected end of expression (wanted '{0}')";
        public static string ParseError_UnexpectedString = "Unexpected string '{0}'";
        public static string ParseError_UnexpectedToken = "Unexpected token '{0}'";
        public static string ParseError_UnexpectedEnd = "Unexpected end";
        public static string ParseError_UnexpectedEOF = "Unexpected EOF";
        public static string ParseError_UnexpectedSymbol = "Unexpected symbol '{0}'";
        public static string ParseError_UnexpectedSymbolWanted = "Unexpected symbol '{0}' (wanted '{1}')";
        public static string ParseError_UnexpectedValue = "Unexpected value '{0}'";
        public static string ParseError_UnexpectedValueWanted = "Unexpected value '{0}' (wanted '{1}')";
        public static string ParseError_UnexpectedExpressionToken = "Unexpected expression token '{0}'";
        public static string ParseError_UnexpectedExpressionTokenWanted = "Unexpected expression token '{0}' (wanted '{1}')";
        public static string ParseError_MissingSmth = "Missing '{0}'";
        public static string ParseError_InvalidChar = "Invalid char '{0}'";
        public static string ParseError_InvalidCharWanted = "Invalid char '{0}' (wanted '{1}')";
        public static string ParseError_InvalidToken = "Invalid token '{0}'";
        public static string ParseError_InvalidTokenWanted = "Invalid token '{0}' (wanted '{1}')";
        public static string ParseError_UnexpectedBackslashAtEndOfPattern = "Unexpected '\\' at end of pattern";
        public static string ParseError_ExpectedSmthAtEndOfPattern = "Expected '{0}' at end of pattern";
        public static string ParseError_MatchPatternIsEmpty = "Match pattern is empty";
        public static string ParseError_OperatorNotFollowedByArgument = "{0} operator not followed by argument";
        public static string ParseError_MetadataEqualityOperatorNotFollowedByTerm = "Metadata equality operator not followed by term";
        public static string ParseError_DateSpecifierDoesNotReferToAStartingDate = "Date specifier does not refer to a starting date";
        public static string ParseError_PythonDirectiveSeenButPythonSupportIsMissing = "'python' directive seen, but Python support is missing";
        public static string ParseError_ImportDirectiveSeenButPythonSupportIsMissing = "'import' directive seen, but Python support is missing";
        public static string ParseError_PayeeDirectiveSmthRequiresAnArgument = "Payee directive '{0}' requires an argument";
        public static string ParseError_CommodityDirectiveSmthRequiresAnArgument = "Commodity directive '{0}' requires an argument";
        public static string ParseError_AssertionFailed = "Assertion failed: {0}";
        public static string ParseError_ArgumentSmthNotAValidYear = "Argument '{0}' not a valid year";
        public static string ParseError_IllegalAliasSmthEqualsSmth  = "Illegal alias {0}={1}";
        public static string ParseError_AccountDirectiveSmthRequiresAnArgument = "Account directive '{0}' requires an argument";
        public static string ParseError_PricingEntryFailedToParse = "Pricing entry failed to parse";
        public static string ParseError_FailedToRecordOutTimelogTransaction = "Failed to record 'out' timelog transaction";
        public static string ParseError_TimelogCheckoutEventWithoutACheckIn = "Timelog check-out event without a check-in";
        public static string ParseError_WhenMultipleCheckinsAreActiveCheckingOutRequiresAnAccount = "When multiple check-ins are active, checking out requires an account";
        public static string ParseError_TimelogCheckoutEventDoesNotMatchAnyCurrentCheckins = "Timelog check-out event does not match any current check-ins";
        public static string ParseError_TimelogCheckinHasNoCorrespondingCheckout = "Timelog check-in has no corresponding check-out";
        public static string ParseError_TimelogCheckoutHasNoCorrespondingCheckin = "Timelog check-out has no corresponding check-in";
        public static string ParseError_TimelogCheckoutDateLessThanCorrespondingCheckin = "Timelog check-out date less than corresponding check-in";
        public static string ParseError_TimelogCannotDoubleCheckinToTheSameAccount = "Cannot double check-in to the same account";
        public static string ParseError_TimelogCheckoutEventWithoutACheckin = "Timelog check-out event without a check-in";
        public static string ParseError_PeriodTransactionFailedToBalance = "Period transaction failed to balance";

        public ParseError(string message)
            : base (message)
        { }
    }
}
