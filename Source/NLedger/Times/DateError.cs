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

namespace NLedger.Times
{
    public class DateError : Exception
    {
        public const string ErrorMessageUnexpectedEndOfExpression = "Unexpected end of expression";
        public const string ErrorMessageUnexpectedDatePeriodToken = "Unexpected date period token '{0}'";
        public const string ErrorMessageUnexpectedEnd = "Unexpected end";
        public const string ErrorMessageMissing = "Missing '{0}'";
        public const string ErrorMessageInvalidChar = "Invalid char '{0}'";
        public const string ErrorMessageInvalidCharWanted = "Invalid char '{0}' (wanted '{1}')";
        public const string ErrorMessageInvalidDate = "Invalid date: {0}";
        public const string ErrorMessageInvalidDateIntervalNeitherStartNorFinishNorDuration = "Invalid date interval: neither start, nor finish, nor duration";
        public const string ErrorMessageCannotIncrementAnUnstartedDateInterval = "Cannot increment an unstarted date interval";
        public const string ErrorMessageCannotIncrementADateIntervalWithoutADuration = "Cannot increment a date interval without a duration";

        public DateError(string message) : base (message)
        { }
    }
}
