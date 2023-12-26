// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Amounts
{
    /// <summary>
    /// Ported from: amount_error (amount.h)
    /// </summary>
    public class AmountError : Exception
    {
        public const string ErrorMessageNonClosedQuote = "Quoted commodity symbol lacks closing quote";
        public const string ErrorMessageMoreThanOnePriceForCommodity = "Commodity specifies more than one price";
        public const string ErrorMessageCommodityLotPriceLacksClosingBrace = "Commodity lot price lacks closing brace";
        public const string ErrorMessageCommodityLotPriceLacksDoubleClosingBrace = "Commodity lot price lacks double closing brace";
        public const string ErrorMessageCommoditySpecifiesMoreThanOneDate = "Commodity specifies more than one date";
        public const string ErrorMessageCommodityDateLacksClosingBracket = "Commodity date lacks closing bracket";
        public const string ErrorMessageCommoditySpecifiesMoreThanOneExpression = "Commodity specifies more than one valuation expresion";
        public const string ErrorMessageCommodityExpressionLacksClosingParentheses = "Commodity valuation expression lacks closing parentheses";
        public const string ErrorMessageCommoditySpecifiesMoreThanOneTag = "Commodity specifies more than one tag";
        public const string ErrorMessageCommodityTagLacksClosingParenthesis = "Commodity tag lacks closing parenthesis";
        public const string ErrorMessageNoQuantitySpecifiedForAmount = "No quantity specified for amount";
        public const string ErrorMessageTooManyPeriodsInAmount = "Too many periods in amount";
        public const string ErrorMessageIncorrectUseOfThousandMarkPeriod = "Incorrect use of thousand-mark period";
        public const string ErrorMessageTooManyCommasInAmount = "Too many commas in amount";
        public const string ErrorMessageIncorrectUseOfDecimalComma = "Incorrect use of decimal comma";
        public const string ErrorMessageIncorrectUseOfThousandMarkComma = "Incorrect use of thousand-mark comma";
        public const string ErrorMessageDivideByZero = "Divide by zero";
        public const string ErrorMessageCannotDivideAnAmountByAnUninitializedAmount = "Cannot divide an amount by an uninitialized amount";
        public const string ErrorMessageCannotDivideAnUninitializedAmountByAnAnAmount = "Cannot divide an uninitialized amount by an amount";
        public const string ErrorMessageCannotDivideTwoUninitializedAmounts = "Cannot divide two uninitialized amounts";
        public const string ErrorMessageCannotMultiplyAnAmountByAnUninitializedAmount = "Cannot multiply an amount by an uninitialized amount";
        public const string ErrorMessageCannotMultiplyAnUninitializedAmountByAnAnAmount = "Cannot multiply an uninitialized amount by an amount";
        public const string ErrorMessageCannotMultiplyTwoUninitializedAmounts = "Cannot multiply two uninitialized amounts";
        public const string ErrorMessageCannotReduceUninitializedAmounts = "Cannot reduce an uninitialized amount";
        public const string ErrorMessageCannotUnroundUninitializedAmounts = "Cannot unround an uninitialized amount";
        public const string ErrorMessageCannotNegateUninitializedAmounts = "Cannot negate an uninitialized amount";
        public const string ErrorMessageCannotSetRoundingForAnUninitializedAmounts = "Cannot set rounding for an uninitialized amount";
        public const string ErrorMessageCannotDetermineIfUninitializedAmountsCommodityIsAnnotated = "Cannot determine if an uninitialized amount's commodity is annotated";
        public const string ErrorMessageCannotAnnotateCommodityOfUninitializedAmount = "Cannot annotate the commodity of an uninitialized amount";
        public const string ErrorMessageCannotAddUninitializedAmountToAmount = "Cannot add an uninitialized amount to an amount";
        public const string ErrorMessageCannotAddAmountToUninitializedAmount = "Cannot add an amount to an uninitialized amount";
        public const string ErrorMessageCannotAddTwoUninitializedAmounts = "Cannot add two uninitialized amounts";
        public const string ErrorMessageCannotSubtractUninitializedAmountFromAmount = "Cannot subtract an uninitialized amount from an amount";
        public const string ErrorMessageCannotSubtractAmountFromUninitializedAmount = "Cannot subtract an amount from an uninitialized amount";
        public const string ErrorMessageCannotSubtractTwoUninitializedAmounts = "Cannot subtract two uninitialized amounts";
        public const string ErrorMessageAddingAmountsWithDifferentCommodities = "Adding amounts with different commodities: '{0}' != '{1}'";
        public const string ErrorMessageSubtractingAmountsWithDifferentCommodities = "Subtracting amounts with different commodities: '{0}' != '{1}'";
        public const string ErrorMessageCannotStripCommodityAnnotationsFromUninitializedAmount = "Cannot strip commodity annotations from an uninitialized amount";
        public const string ErrorMessageRequestedAmountOfBalanceWithMultipleCommodities = "Requested amount of a balance with multiple commodities: {0}";
        public const string ErrorMessageCannotDetermineIfUninitializedAmountIsZero = "Cannot determine if an uninitialized amount is zero";
        public const string ErrorMessageCannotCompareAmountToUninitializedAmount = "Cannot compare an amount to an uninitialized amount";
        public const string ErrorMessageCannotCompareUninitializedAmountToAmount = "Cannot compare an uninitialized amount to an amount";
        public const string ErrorMessageCannotCompareTwoUninitializedAmounts = "Cannot compare two uninitialized amounts";
        public const string ErrorMessageCannotCompareAmountsWithDifferentCommodities = "Cannot compare amounts with different commodities: '%1%' and '%2%'";
        public const string ErrorMessageCannotReturnCommodityAnnotationDetailsOfUninitializedAmount = "Cannot return commodity annotation details of an uninitialized amount";
        public const string ErrorMessageRequestForAnnotationDetailsFromUnannotatedAmount = "Request for annotation details from an unannotated amount";
        public const string ErrorMessageAutomatedTransactionsPostingHasNoAmount = "Automated transaction's posting has no amount";
        public const string ErrorMessageAmountExpressionsMustResultInASimpleAmount = "Amount expressions must result in a simple amount";
        public const string ErrorMessageAPostingsCostMustBeOfADifferentCommodityThanItsAmount = "A posting's cost must be of a different commodity than its amount";
        public const string ErrorMessageCannotDetermineValueOfAnUninitializedAmount = "Cannot determine value of an uninitialized amount";
        public const string ErrorMessageCannotDetermineSignOfAnUninitializedAmount = "Cannot determine sign of an uninitialized amount";
        public const string ErrorMessageCannotUnreduceAnUninitializedAmount = "Cannot unreduce an uninitialized amount";
        public const string ErrorMessageCannotComputeFloorOnAnUninitializedAmount = "Cannot compute floor on an uninitialized amount";
        public const string ErrorMessageCannotTruncateAnUninitializedAmount = "Cannot truncate an uninitialized amount";
        public const string ErrorMessageCannotComputeCeilingOnAnUninitializedAmount = "Cannot compute ceiling on an uninitialized amount";
        public const string ErrorMessageCannotRoundAnUninitializedAmount = "Cannot round an uninitialized amount";
        public const string ErrorMessageCannotDeterminePrecisionOfAnUninitializedAmount = "Cannot determine precision of an uninitialized amount";
        public const string ErrorMessageCannotDetermineDisplayPrecisionOfAnUninitializedAmount = "Cannot determine display precision of an uninitialized amount";
        public const string ErrorMessageCannotInvertUninitializedAmount = "Cannot invert an uninitialized amount";
        public const string ErrorMessageCannotSetWhetherToKeepThePrecisionOfAnUninitializedAmount = "Cannot set whether to keep the precision of an uninitialized amount";

        public AmountError(string message)
            : base(message)
        { }
    }
}
