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

namespace NLedger.Items
{
    /// <summary>
    /// Ported from position_t (item.h)
    /// </summary>
    public class ItemPosition
    {
        public ItemPosition()
        { }

        public ItemPosition(ItemPosition itemPosition)
        {
            if (itemPosition == null)
                throw new ArgumentNullException("itemPosition");

            PathName = itemPosition.PathName;
            BegPos = itemPosition.BegPos;
            BegLine = itemPosition.BegLine;
            EndPos = itemPosition.EndPos;
            EndLine = itemPosition.EndLine;
            Sequence = itemPosition.Sequence;
        }

        public string PathName { get; set; }
        public long BegPos { get; set; }
        public int BegLine { get; set; }
        public int EndPos { get; set; }
        public int EndLine { get; set; }
        public int Sequence { get; set; }
    }
}
