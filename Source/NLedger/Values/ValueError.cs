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

namespace NLedger.Values
{
    public class ValueError : Exception
    {
        public const string ValueException_NotAStringValue = "Not A String Value";
        public const string ValueException_NotASequence = "Not A Sequenece";
        public const string ValueException_NotADateTime = "Not A DateTime";
        public const string ValueException_NotAnInteger = "Not An Integer";
        public const string ValueException_NotAnAmount = "Not An Amount";
        public const string ValueException_NotABalance = "Not A Balance";
        public const string ValueException_CannotAddSequencesOfDifferentLengths = "Cannot add sequences of different lengths";
        public const string CannotConvertBooleanToDate = "Cannot Convert Boolean To Date";
        public const string CannotConvertBooleanToDateTime = "Cannot Convert Boolean To DateTime";
        public const string CannotConvertBooleanToBalance = "Cannot Convert Boolean To Balance";
        public const string CannotConvertBooleanToMask = "Cannot Convert Boolean To Mask";
        public const string CannotConvertDateTimeToBalance = "Cannot Convert DateTime To Balance";
        public const string CannotConvertDateTimeToMask = "Cannot Convert DateTime To Mask";
        public const string CannotConvertLongToMask = "Cannot Convert Long To Mask";
        public const string CannotConvertAmountToMask = "Cannot Convert Amount To Mask";
        public const string CannotConvertBalanceToBoolean = "Cannot Convert Balance To Boolean";
        public const string CannotConvertBalanceToDate = "Cannot Convert Balance To Date";
        public const string CannotConvertBalanceToDateTime = "Cannot Convert Balance To DateTime";
        public const string CannotConvertBalanceToInteger = "Cannot Convert Balance To Integer";
        public const string CannotConvertBalanceWithMultipleCommoditiesToAmount = "Cannot Convert Balance With Multiple Commodities To Amount";
        public const string CannotConvertBalanceToMask = "Cannot Convert Balance To Mask";
        public const string CannotConvertStringToBalance = "Cannot Convert String To Balance";
        public const string CannotConvertMaskToDate = "Cannot Convert Mask To Date";
        public const string CannotConvertMaskToDateTime = "Cannot Convert Mask To DateTime";
        public const string CannotConvertMaskToLong = "Cannot Convert Mask To Long";
        public const string CannotConvertMaskToAmount = "Cannot Convert Mask To Amount";
        public const string CannotConvertMaskToBalance = "Cannot Convert Mask To Balance";
        public const string CannotConvertSequenceToDate = "Cannot Convert Sequence To Date";
        public const string CannotConvertSequenceToDateTime = "Cannot Convert Sequence To DateTime";
        public const string CannotConvertSequenceToLong = "Cannot Convert Sequence To Long";
        public const string CannotConvertSequenceToAmount = "Cannot Convert Sequence To Amount";
        public const string CannotConvertSequenceToBalance = "Cannot Convert Sequence To Balance";
        public const string CannotConvertSequenceToMask = "Cannot Convert Sequence To Mask";
        public const string CannotConvertScopeToDate = "Cannot Convert Scope To Date";
        public const string CannotConvertScopeToDateTime = "Cannot Convert Scope To DateTime";
        public const string CannotConvertScopeToLong = "Cannot Convert Scope To Long";
        public const string CannotConvertScopeToAmount = "Cannot Convert Scope To Amount";
        public const string CannotConvertScopeToBalance = "Cannot Convert Scope To Balance";
        public const string CannotConvertScopeToMask = "Cannot Convert Scope To Mask";
        public const string CannotDetermineIfItIsReallyZero = "Cannot determine if {0} is really zero";
        public const string ValueIsUninitialized = "Value Is Uninitialized";
        public const string ValueFunctionRecursionDepthTooDeep = "Function recursion_depth too deep (> 256)";
        public const string CannotCallSmthAsFunction = "Cannot call {0} as a function";
        public const string CannotCompareSmthToSmth  = "Cannot compare {0} to {1}";
        public const string CannotFindTheValueOfSmth  = "Cannot find the value of {0}";
        public const string WhileFindingValuationOfSmth = "While finding valuation of {0}:";
        public const string CannotDetermineTruthOfSmth = "Cannot determine truth of {0} (did you mean 'account =~ {1}'?)";
        public const string CannotRequestAnnotationOfSmth = "Cannot request annotation of {0}";
        public const string CannotSetRoundingForSmth = "Cannot set rounding for {0}";
        public const string CannotUnroundSmth = "Cannot unround {0}";
        public const string CannotFloorSmth = "Cannot floor {0}";
        public const string CannotCeilingSmth = "Cannot ceiling {0}";
        public const string CannotAbsSmth = "Cannot abs {0}";
        public const string CannotDetermineNumericValueOfSmth = "Cannot determine numeric value of {0}";
        public const string CannotAnnotateSmth = "Cannot annotate {0}";

        public ValueError(string message)
            : base(message)
        {  }
    }
}
