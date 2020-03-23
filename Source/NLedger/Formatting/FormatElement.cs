// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Expressions;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Formatting
{
    /// <summary>
    /// Ported from format_t/element_t
    /// </summary>
    public class FormatElement
    {
        public FormatElementEnum Type { get; set; }
        public int MinWidth { get; set; }
        public int MaxWidth { get; set; }

        public BoostVariant Data
        {
            get { return _Data; }
        }

        public FormatElement Next { get; set; }
        public bool IsElementAlignLeft { get; set; }

        // Replacement for: element_t& operator=(const element_t& elem)
        public FormatElement Assign(FormatElement element)
        {
            if (element != this)
            {
                IsElementAlignLeft = element.IsElementAlignLeft;
                Type = element.Type;
                MinWidth = element.MinWidth;
                MaxWidth = element.MaxWidth;
                Data.SetValue(element.Data);
            }
            return this;
        }

        public string Dump()
        {
            StringBuilder sb = new StringBuilder("Element: ");
            
            switch(Type)
            {
                case FormatElementEnum.STRING: sb.Append(" STRING"); break;
                case FormatElementEnum.EXPR: sb.Append("   EXPR"); break;
                default: throw new InvalidOperationException();
            }

            sb.AppendFormat("  flags: 0x{0}", IsElementAlignLeft ? 1 : 0);
            sb.AppendFormat("  min: {0,2}", MinWidth);
            sb.AppendFormat("  max: {0,2}", MaxWidth);

            switch (Type)
            {
                case FormatElementEnum.STRING: sb.AppendFormat("   str: '{0}'", Data.GetValue<string>()); break;
                case FormatElementEnum.EXPR: sb.AppendFormat("  expr: {0}", Data.Value); break;
                default: throw new InvalidOperationException();
            }

            sb.AppendLine();
            return sb.ToString();
        }

        private readonly BoostVariant _Data = new BoostVariant(typeof(string), typeof(Expr));
    }
}
