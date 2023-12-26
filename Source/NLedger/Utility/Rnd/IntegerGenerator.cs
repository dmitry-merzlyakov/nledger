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

namespace NLedger.Utility.Rnd
{
    public class IntegerGenerator
    {
        public IntegerGenerator(int minValue, int maxValue)
            : this(null, minValue, maxValue)
        { }

        public IntegerGenerator(Random random, int minValue, int maxValue)
        {
            Random = random ?? new Random();
            MinValue = minValue;
            MaxValue = maxValue + 1;  // Because Random.Next returns integers greater than or equal to minValue and less than maxValue
        }

        public int Value()
        {
            return Random.Next(MinValue, MaxValue);
        }

        private readonly int MinValue;
        private readonly int MaxValue;
        private readonly Random Random;
    }
}
