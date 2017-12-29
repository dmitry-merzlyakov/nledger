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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility
{
    public sealed class AnsiTextWriter : TextWriter
    {
        public const char Esc = (char)27;
        public const char Csi = '[';

        #region ANSI codes
        public static readonly string NormalColor = "" + Esc + Csi + "0m";
        public static readonly string ForegroundBold = "" + Esc + Csi + "1m";
        public static readonly string ForegroundUnderline = "" + Esc + Csi + "4m";
        public static readonly string ForegroundBlink = "" + Esc + Csi + "5m";

        public static readonly string BackgroundColorBlack = "" + Esc + Csi + "40m";
        public static readonly string BackgroundColorRed = "" + Esc + Csi + "41m";
        public static readonly string BackgroundColorGreen = "" + Esc + Csi + "42m";
        public static readonly string BackgroundColorYellow = "" + Esc + Csi + "43m";
        public static readonly string BackgroundColorBlue = "" + Esc + Csi + "44m";
        public static readonly string BackgroundColorMagenta = "" + Esc + Csi + "45m";
        public static readonly string BackgroundColorCyan = "" + Esc + Csi + "46m";
        public static readonly string BackgroundColorWhite = "" + Esc + Csi + "47m";

        public static readonly string BackgroundColorBoldBlack = "" + Esc + Csi + "40;1m";
        public static readonly string BackgroundColorBoldRed = "" + Esc + Csi + "41;1m";
        public static readonly string BackgroundColorBoldGreen = "" + Esc + Csi + "42;1m";
        public static readonly string BackgroundColorBoldYellow = "" + Esc + Csi + "43;1m";
        public static readonly string BackgroundColorBoldBlue = "" + Esc + Csi + "44;1m";
        public static readonly string BackgroundColorBoldMagenta = "" + Esc + Csi + "45;1m";
        public static readonly string BackgroundColorBoldCyan = "" + Esc + Csi + "46;1m";
        public static readonly string BackgroundColorBoldWhite = "" + Esc + Csi + "47;1m";

        public static readonly string ForegroundColorBlack = "" + Esc + Csi + "30m";
        public static readonly string ForegroundColorRed = "" + Esc + Csi + "31m";
        public static readonly string ForegroundColorGreen = "" + Esc + Csi + "32m";
        public static readonly string ForegroundColorYellow = "" + Esc + Csi + "33m";
        public static readonly string ForegroundColorBlue = "" + Esc + Csi + "34m";
        public static readonly string ForegroundColorMagenta = "" + Esc + Csi + "35m";
        public static readonly string ForegroundColorCyan = "" + Esc + Csi + "36m";
        public static readonly string ForegroundColorWhite = "" + Esc + Csi + "37m";

        public static readonly string ForegroundColorBoldBlack = "" + Esc + Csi + "30;1m";
        public static readonly string ForegroundColorBoldRed = "" + Esc + Csi + "31;1m";
        public static readonly string ForegroundColorBoldGreen = "" + Esc + Csi + "32;1m";
        public static readonly string ForegroundColorBoldYellow = "" + Esc + Csi + "33;1m";
        public static readonly string ForegroundColorBoldBlue = "" + Esc + Csi + "34;1m";
        public static readonly string ForegroundColorBoldMagenta = "" + Esc + Csi + "35;1m";
        public static readonly string ForegroundColorBoldCyan = "" + Esc + Csi + "36;1m";
        public static readonly string ForegroundColorBoldWhite = "" + Esc + Csi + "37;1m";
        #endregion

        public static void Attach()
        {
            Console.SetOut(new AnsiTextWriter(Console.Out));
            Console.SetError(new AnsiTextWriter(Console.Error));
        }

        public static void Detach()
        {
            AnsiTextWriter ansiTextWriter = Console.Out as AnsiTextWriter;
            if (ansiTextWriter != null)
                Console.SetOut(ansiTextWriter.ConsoleOut);

            ansiTextWriter = Console.Error as AnsiTextWriter;
            if (ansiTextWriter != null)
                Console.SetError(ansiTextWriter.ConsoleOut);
        }

        public AnsiTextWriter(TextWriter consoleOut)
        {
            if (consoleOut == null)
                throw new ArgumentNullException("consoleOut");

            ConfigureActions();

            ConsoleOut = consoleOut;
        }

        public TextWriter ConsoleOut { get; private set; }

        public override Encoding Encoding
        {
            get { return ConsoleOut.Encoding; }
        }

        public override void Flush()
        {
            Parser.Reset();
            ConsoleOut.Flush();
        }

        public override void Write(char value)
        {
            Parser.ParseChar(value);

            if (Parser.IsSequenceDetected)
            {
                if (Parser.IsSequenceFound)
                {
                    Parser.ExecuteAction();
                    Parser.Reset();
                }
            }
            else
            {
                ConsoleOut.Write(value);
            }
        }

        private void ConfigureActions()
        {
            Parser.AddAction(new AnsiCsiAction(NormalColor, () => Console.ResetColor()));

            Parser.AddAction(new AnsiCsiAction(ForegroundBold, () =>
            {
                if ((int)Console.ForegroundColor < 8)
                    Console.ForegroundColor = Console.ForegroundColor + 8;
            }));

            Parser.AddAction(new AnsiCsiAction(BackgroundColorBlack, () => Console.BackgroundColor = ConsoleColor.Black));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorRed, () => Console.BackgroundColor = ConsoleColor.DarkRed));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorGreen, () => Console.BackgroundColor = ConsoleColor.DarkGreen));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorYellow, () => Console.BackgroundColor = ConsoleColor.DarkYellow));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorBlue, () => Console.BackgroundColor = ConsoleColor.DarkBlue));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorMagenta, () => Console.BackgroundColor = ConsoleColor.DarkMagenta));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorCyan, () => Console.BackgroundColor = ConsoleColor.DarkCyan));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorWhite, () => Console.BackgroundColor = ConsoleColor.DarkGray));

            Parser.AddAction(new AnsiCsiAction(BackgroundColorBoldBlack, () => Console.BackgroundColor = ConsoleColor.Gray));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorBoldRed, () => Console.BackgroundColor = ConsoleColor.Red));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorBoldGreen, () => Console.BackgroundColor = ConsoleColor.Green));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorBoldYellow, () => Console.BackgroundColor = ConsoleColor.Yellow));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorBoldBlue, () => Console.BackgroundColor = ConsoleColor.Blue));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorBoldMagenta, () => Console.BackgroundColor = ConsoleColor.Magenta));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorBoldCyan, () => Console.BackgroundColor = ConsoleColor.Cyan));
            Parser.AddAction(new AnsiCsiAction(BackgroundColorBoldWhite, () => Console.BackgroundColor = ConsoleColor.White));

            Parser.AddAction(new AnsiCsiAction(ForegroundColorBlack, () => Console.ForegroundColor = ConsoleColor.Black));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorRed, () => Console.ForegroundColor = ConsoleColor.DarkRed));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorGreen, () => Console.ForegroundColor = ConsoleColor.DarkGreen));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorYellow, () => Console.ForegroundColor = ConsoleColor.DarkYellow));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorBlue, () => Console.ForegroundColor = ConsoleColor.DarkBlue));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorMagenta, () => Console.ForegroundColor = ConsoleColor.DarkMagenta));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorCyan, () => Console.ForegroundColor = ConsoleColor.DarkCyan));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorWhite, () => Console.ForegroundColor = ConsoleColor.DarkGray));

            Parser.AddAction(new AnsiCsiAction(ForegroundColorBoldBlack, () => Console.ForegroundColor = ConsoleColor.Gray));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorBoldRed, () => Console.ForegroundColor = ConsoleColor.Red));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorBoldGreen, () => Console.ForegroundColor = ConsoleColor.Green));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorBoldYellow, () => Console.ForegroundColor = ConsoleColor.Yellow));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorBoldBlue, () => Console.ForegroundColor = ConsoleColor.Blue));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorBoldMagenta, () => Console.ForegroundColor = ConsoleColor.Magenta));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorBoldCyan, () => Console.ForegroundColor = ConsoleColor.Cyan));
            Parser.AddAction(new AnsiCsiAction(ForegroundColorBoldWhite, () => Console.ForegroundColor = ConsoleColor.White));
        }

        private class AnsiCsiAction
        {
            public AnsiCsiAction(string ansiCsiSequence, Action action)
            {
                if (String.IsNullOrEmpty(ansiCsiSequence))
                    throw new ArgumentNullException("ansiCsiSequence");
                if (action == null)
                    throw new ArgumentNullException("action");

                AnsiCsiSequence = ansiCsiSequence;
                Action = action;
            }

            public string AnsiCsiSequence { get; private set; }
            public Action Action { get; private set; }
        }

        private class AnsiCsiParserNode
        {
            public AnsiCsiParserNode(char ch)
            {
                Char = ch;
            }

            public bool IsLeaf
            {
                get { return Action != null; }
            }

            public Char Char { get; private set; }
            public AnsiCsiAction Action { get; set; }

            public void AddChild(AnsiCsiParserNode node)
            {
                if (node == null)
                    throw new ArgumentNullException("node");

                Nodes.Add(node.Char, node);
            }

            public AnsiCsiParserNode Find(char ch)
            {
                AnsiCsiParserNode node;
                Nodes.TryGetValue(ch, out node);
                return node;
            }

            private readonly IDictionary<Char, AnsiCsiParserNode> Nodes = new Dictionary<Char, AnsiCsiParserNode>();
        }

        private class AnsiCsiParser
        {
            public AnsiCsiParser()
            {
                Root = new AnsiCsiParserNode(default(char));
            }

            public bool IsSequenceDetected
            {
                get { return Current != null; }
            }

            public bool IsSequenceFound
            {
                get { return IsSequenceDetected && Current.IsLeaf; }
            }

            public AnsiCsiParserNode Root { get; private set; }
            public AnsiCsiParserNode Current { get; private set; }

            public void AddAction(AnsiCsiAction ansiCsiAction)
            {
                if (ansiCsiAction == null)
                    throw new ArgumentNullException("ansiCsiAction");

                AnsiCsiParserNode node = Root;
                foreach (Char ch in ansiCsiAction.AnsiCsiSequence)
                {
                    AnsiCsiParserNode childNode = node.Find(ch);
                    if (childNode == null)
                        node.AddChild(childNode = new AnsiCsiParserNode(ch));
                    node = childNode;

                    if (node.IsLeaf)
                        throw new InvalidOperationException("Unbalanced sequence lengthes");
                }
                node.Action = ansiCsiAction;
            }

            public void ParseChar(Char value)
            {
                if (IsSequenceDetected)
                    Current = Current.Find(value);
                else
                    Current = value != AnsiTextWriter.Esc ? null  // Just to fasten preprocessing non-ansi sequences
                        : Root.Find(value);
            }

            public void Reset()
            {
                Current = null;
            }

            public void ExecuteAction()
            {
                if (!IsSequenceFound)
                    throw new InvalidOperationException("Sequence is not found");

                Current.Action.Action();
            }
        }

        private AnsiCsiParser Parser = new AnsiCsiParser();
    }
}
