// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Journals;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Iterators
{
    public class XactsIterator : IIterator<Xact>
    {
        public XactsIterator()
        { }

        public XactsIterator(Journal journal)
        {
            Reset(journal);
        }

        public XactsIterator(IEnumerable<Xact> xacts)
        {
            Reset(xacts);
        }

        public IEnumerable<Xact> Xacts { get; private set;}

        public void Reset(Journal journal)
        {
            Xacts = new List<Xact>(journal.Xacts);
        }

        public void Reset(IEnumerable<Xact> xacts)
        {
            Xacts = new List<Xact>(xacts);
        }

        public IEnumerable<Xact> Get()
        {
            return Xacts ?? Enumerable.Empty<Xact>();
        }
    }
}
