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

namespace NLedger.Chain
{
    public class ItemHandler<T> where T: class
    {
        public ItemHandler()
        { }

        public ItemHandler(ItemHandler<T> handler)
        {
            Handler = handler;
        }

        public ItemHandler<T> Handler { get; private set; }

        public virtual void Title(string str)
        {
            if (Handler != null)
                Handler.Title(str);
        }

        public virtual void Flush()
        {
            if (Handler != null)
                Handler.Flush();
        }

        public virtual void Handle(T item)
        {
            if (Handler != null)
                Handler.Handle(item);
        }

        public virtual void Clear()
        {
            if (Handler != null)
                Handler.Clear();
        }
    }
}
