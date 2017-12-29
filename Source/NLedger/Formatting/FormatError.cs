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

namespace NLedger.Formatting
{
    public class FormatError : Exception
    {
        public const string ErrorMessageUnrecognizedFormattingCharacterSmth = "Unrecognized formatting character: {0}";
        public const string ErrorMessagePriorFieldReferenceButNoTemplate = "Prior field reference, but no template";
        public const string ErrorMessageFieldReferenceMustBeADigitFrom1To9 = "%$ field reference must be a digit from 1-9";
        public const string ErrorMessageReferenceToANonExistentPriorField = "%$ reference to a non-existent prior field";

        public FormatError(string message) : base(message)
        { }
    }
}
