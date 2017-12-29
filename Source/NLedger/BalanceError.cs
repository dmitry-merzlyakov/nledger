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
    public class BalanceError : Exception
    {
        public const string ErrorMessageCannotAddUninitializedAmountToBalance = "Cannot add an uninitialized amount to a balance";
        public const string ErrorMessageCannotSubtractUninitializedAmountFromBalance = "Cannot subtract an uninitialized amount from a balance";
        public const string ErrorMessageCannotMultiplyBalanceByUninitializedAmount = "Cannot multiply a balance by an uninitialized amount";
        public const string ErrorMessageCannotCompareBalanceToUninitializedAmount = "Cannot compare a balance to an uninitialized amount";
        public const string ErrorMessageCannotMultiplyBalanceWithAnnotatedCommoditiesByCommoditizedAmount = "Cannot multiply a balance with annotated commodities by a commoditized amount";
        public const string ErrorMessageCannotMultiplyMultiCommodityBalanceByCommoditizedAmount = "Cannot multiply a multi-commodity balance by a commoditized amount";
        public const string ErrorMessageCannotDivideBalanceByUninitializedAmount = "Cannot divide a balance by an uninitialized amount";
        public const string ErrorMessageCannotDivideBalanceWithAnnotatedCommoditiesByCommoditizedAmount = "Cannot divide a balance with annotated commodities by a commoditized amount";
        public const string ErrorMessageCannotDivideMultiCommodityBalanceByCommoditizedAmount = "Cannot divide a multi-commodity balance by a commoditized amount";
        public const string ErrorMessageDivideByZero = "Divide by zero";
        public const string ErrorMessageTransactionDoesNotBalance = "Transaction does not balance";
        public const string ErrorMessageAPostingsCostMustBeOfADifferentCommodityThanItsAmount = "A posting's cost must be of a different commodity than its amount";
        public const string ErrorMessageThereCannotBeNullAmountsAfterBalancingATransaction = "There cannot be null amounts after balancing a transaction";

        public BalanceError(string message)
            : base(message)
        { }
    }
}
