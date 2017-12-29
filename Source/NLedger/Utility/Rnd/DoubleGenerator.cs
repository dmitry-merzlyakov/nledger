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

namespace NLedger.Utility.Rnd
{
    public class DoubleGenerator
    {
        public DoubleGenerator(int minValue, int maxValue)
            : this(null, minValue, maxValue)
        { }

        public DoubleGenerator(Random random, int minValue, int maxValue)
        {
            Random = random ?? new Random();
            MinValue = minValue;
            MaxValue = maxValue;
            Range = MaxValue - MinValue;
        }

        public double Value()
        {
            return (Random.NextDouble() * Range) + MinValue;
        }

        private readonly int MinValue;
        private readonly int MaxValue;
        private readonly int Range;
        private readonly Random Random;
    }
}
