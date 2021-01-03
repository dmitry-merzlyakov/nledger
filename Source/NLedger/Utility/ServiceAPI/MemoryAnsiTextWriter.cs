// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility.ServiceAPI
{
    public class MemoryAnsiTextWriter : BaseAnsiTextWriter
    {
        public MemoryAnsiTextWriter(TextWriter consoleOut)
            : base(consoleOut)
        { }

        public override string ToString()
        {
            return ConsoleOut.ToString();
        }

        protected override void ConfigureActions()
        {
            Parser.AddAction(new AnsiCsiAction(NormalColor, () =>
            {
                CloseScope();
                ForegroundColor = null;
                BackgroundColor = null;
            }));

            Parser.AddAction(new AnsiCsiAction(ForegroundBold, () =>
            {
                CloseScope();
                if (ForegroundColor.HasValue && (int)ForegroundColor < 8)
                    ForegroundColor = ForegroundColor + 8;
                OpenScope();
            }));

            Parser.AddAction(new AnsiCsiAction(BackgroundColorBlack, () => SetBackgroundColor(ConsoleColor.Black)));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorRed, () => SetBackgroundColor(ConsoleColor.DarkRed)));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorGreen, () => SetBackgroundColor(ConsoleColor.DarkGreen)));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorYellow, () => SetBackgroundColor(ConsoleColor.DarkYellow)));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorBlue, () => SetBackgroundColor(ConsoleColor.DarkBlue)));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorMagenta, () => SetBackgroundColor(ConsoleColor.DarkMagenta)));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorCyan, () => SetBackgroundColor(ConsoleColor.DarkCyan)));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorWhite, () => SetBackgroundColor(ConsoleColor.DarkGray)));

            Parser.AddAction(new AnsiCsiAction(BackgroundColorBoldBlack, () => SetBackgroundColor(ConsoleColor.Gray)));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorBoldRed, () => SetBackgroundColor(ConsoleColor.Red)));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorBoldGreen, () => SetBackgroundColor(ConsoleColor.Green)));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorBoldYellow, () => SetBackgroundColor(ConsoleColor.Yellow)));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorBoldBlue, () => SetBackgroundColor(ConsoleColor.Blue)));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorBoldMagenta, () => SetBackgroundColor(ConsoleColor.Magenta)));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorBoldCyan, () => SetBackgroundColor(ConsoleColor.Cyan)));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorBoldWhite, () => SetBackgroundColor(ConsoleColor.White)));

            Parser.AddAction(new AnsiCsiAction(ForegroundColorBlack, () => SetForegroundColor(ConsoleColor.Black)));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorRed, () => SetForegroundColor(ConsoleColor.DarkRed)));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorGreen, () => SetForegroundColor(ConsoleColor.DarkGreen)));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorYellow, () => SetForegroundColor(ConsoleColor.DarkYellow)));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorBlue, () => SetForegroundColor(ConsoleColor.DarkBlue)));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorMagenta, () => SetForegroundColor(ConsoleColor.DarkMagenta)));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorCyan, () => SetForegroundColor(ConsoleColor.DarkCyan)));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorWhite, () => SetForegroundColor(ConsoleColor.DarkGray)));

            Parser.AddAction(new AnsiCsiAction(ForegroundColorBoldBlack, () => SetForegroundColor(ConsoleColor.Gray)));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorBoldRed, () => SetForegroundColor(ConsoleColor.Red)));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorBoldGreen, () => SetForegroundColor(ConsoleColor.Green)));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorBoldYellow, () => SetForegroundColor(ConsoleColor.Yellow)));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorBoldBlue, () => SetForegroundColor(ConsoleColor.Blue)));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorBoldMagenta, () => SetForegroundColor(ConsoleColor.Magenta)));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorBoldCyan, () => SetForegroundColor(ConsoleColor.Cyan)));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorBoldWhite, () => SetForegroundColor(ConsoleColor.White)));
        }

        private ConsoleColor? ForegroundColor { get; set; }
        private ConsoleColor? BackgroundColor { get; set; }
        private bool IsInScope => ForegroundColor.HasValue || BackgroundColor.HasValue;


        private void SetForegroundColor(ConsoleColor consoleColor)
        {
            CloseScope();
            ForegroundColor = consoleColor;
            OpenScope();
        }

        private void SetBackgroundColor(ConsoleColor consoleColor)
        {
            CloseScope();
            BackgroundColor = consoleColor;
            OpenScope();
        }

        private void CloseScope()
        {
            if (IsInScope)
                ConsoleOut.Write("</span>");
        }

        private void OpenScope()
        {
            if (IsInScope)
            {
                var foregroundStyle = ForegroundColor.HasValue ? $"fg" + (int)ForegroundColor.Value : String.Empty;
                var backgroundStyle = BackgroundColor.HasValue ? $"bg" + (int)BackgroundColor.Value : String.Empty;
                var space = ForegroundColor.HasValue && BackgroundColor.HasValue ? " " : String.Empty;
                ConsoleOut.Write($"<span class=\"{foregroundStyle}{space}{backgroundStyle}\">");
            }
        }

    }
}
