// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Expressions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Utility
{
    public class ErrorContext
    {
        public static readonly ErrorContext Current = new ErrorContext();

        public static string FileContext(string path, long line)
        {
            return String.Format("\"{0}\", line {1}:", path, line);
        }

        public static string LineContext(string line, int pos = 0, int endPos = 0)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("  " + line);
            if (pos != 0)
            {
                sb.Append("  ");
                if (endPos == 0)
                {
                    for (int i = 0; i < pos; i++) sb.Append(" ");
                    sb.Append("^");
                }
                else
                {
                    for (int i = 0; i < endPos; i++)
                    {
                        if (i >= pos)
                            sb.Append("^");
                        else
                            sb.Append(" ");
                    }
                }
            }
            return sb.ToString();
        }

        public static string OpContext(ExprOp op, ExprOp locus = null)
        {
            ExprOpContext context = new ExprOpContext(op, locus);
            StringBuilder sb = new StringBuilder("  ");
            string buf = null;
            if (op.Print(ref buf, context))
            {
                sb.AppendLine(buf);
                for (int i = 0; i < context.EndPos; i++)
                {
                    if (i > context.StartPos)
                        sb.Append('^');
                    else
                        sb.Append(' ');
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Ported from source_context
        /// </summary>
        public static string SourceContext(string file, long pos, long endPos, string prefix)
        {
            long len = endPos - pos;
            if (len == 0 || String.IsNullOrEmpty(file))
                return "<no source context>";

            if (len < 0 || len > 8192)
                throw new InvalidOperationException("len");

            var buf = FileSystem.GetStringFromFile(file, pos, len).TrimEnd();
            string[] lines = buf.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            StringBuilder sb = new StringBuilder();
            foreach (string line in lines)
                sb.AppendLine(prefix + line);

            return sb.ToString().TrimEnd();
        }

        public void AddErrorContext(string msg)
        {
            if (TxtBuffer.Length != 0)
                TxtBuffer.AppendLine();

            TxtBuffer.Append(msg);
        }

        public string GetContext()
        {
            string context = TxtBuffer.ToString();
            TxtBuffer.Clear();
            return context;
        }

        public void WriteError(string msg)
        {
            if (!String.IsNullOrEmpty(msg))
                FileSystem.ConsoleError.WriteLine("{0}", msg);
        }

        /// <summary>
        /// Portd from warning_func
        /// </summary>
        public void WriteWarning(string msg)
        {
            if (!String.IsNullOrEmpty(msg))
                FileSystem.ConsoleError.WriteLine("Warning: {0}", msg);
        }

        public void WriteError(Exception ex)
        {
            if (ex == null)
                return;

            WriteError(String.Format("Error: {0}", ex.Message));
        }

        private StringBuilder TxtBuffer = new StringBuilder();
    }
}
