// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utility.Rnd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Utility.Rnd
{
    public class CharGeneratorTests
    {
        [Fact]
        public void CharGenerator_Value_ProvidesMassiveOfCharsInSpecifiedRange()
        {
            int size = 1000;
            CharGenerator charGen = new CharGenerator('a', 'c');

            char[] massive = new char[size];
            for (int i = 0; i < size; i++)
                massive[i] = charGen.Value();
            var usedNumbers = massive.Distinct().OrderBy(i => i).ToList();

            Assert.Equal('a', usedNumbers[0]);
            Assert.Equal('b', usedNumbers[1]);
            Assert.Equal('c', usedNumbers[2]);
        }

    }
}
