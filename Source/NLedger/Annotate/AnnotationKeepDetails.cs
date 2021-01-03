// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Commodities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Annotate
{
    /// <summary>
    /// Ported from struct keep_details_t
    /// </summary>
    public struct AnnotationKeepDetails
    {
        public AnnotationKeepDetails(bool keepPrice = false, bool keepDate = false, bool keepTag = false, bool onlyActuals = false)
            : this()
        {
            KeepPrice = keepPrice;
            KeepDate = keepDate;
            KeepTag = keepTag;
            OnlyActuals = onlyActuals;
        }

        public bool KeepPrice { get; set; }
        public bool KeepDate { get; set; }
        public bool KeepTag { get; set; }
        public bool OnlyActuals { get; set; }

        public bool KeepAll()
        {
            return KeepPrice && KeepDate && KeepTag && !OnlyActuals;
        }

        public bool KeepAll(Commodity commodity)
        {
            if (commodity == null)
                throw new ArgumentNullException("commodity");

            return !commodity.IsAnnotated || KeepAll();
        }

        public bool KeepAny()
        {
            return KeepPrice || KeepDate || KeepTag;
        }

        public bool KeepAny(Commodity commodity = null)
        {
            if (commodity == null)
                throw new ArgumentNullException("commodity");

            return commodity.IsAnnotated && KeepAny();
        }
    }
}
