// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Expressions
{
    public class CalcError : Exception
    {
        public const string ErrorMessageTooFewArgumentsToFunction = "Too few arguments to function";
        public const string ErrorMessageExpectedSmthForArgumentSmthButReceivedSmth = "Expected '{0}' for argument '{1}', but received '{2}'";
        public const string ErrorMessageInvalidFunctionOrLambdaParameter = "Invalid function or lambda parameter: {0}";
        public const string ErrorMessageSyntaxError = "Syntax error";
        public const string ErrorMessageUnknownIdentifier = "Unknown identifier '{0}'";
        public const string ErrorMessageUnexpectedExprNode = "Unexpected expr node '{0}'";
        public const string ErrorMessageExpectedReturnOfSmthButReceivedSmth = "Expected return of '{0}', but received '{1}'";
        public const string ErrorMessageLeftOperandDoesNotEvaluateToObject = "Left operand does not evaluate to an object";
        public const string ErrorMessageInvalidFunctionDefinition = "Invalid function definition";
        public const string ErrorMessageTooFewArgumentsInFunctionCall = "Too few arguments in function call (saw {0}, wanted {1})";
        public const string ErrorMessageUnhandledOperator = "Unhandled operator";
        public const string ErrorMessageCouldNotDetermineSortingValueBasedAnExpression = "Could not determine sorting value based an expression";
        public const string ErrorMessageAnAccountDoesNotHaveACostValue = "An account does not have a 'cost' value";

        public CalcError(string message)
            : base(message)
        { }
    }
}
