// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Abstracts.Impl
{
    public sealed class ProcessManager : IProcessManager
    {
        public const int DefaultExecutionTimeout = 60 * 1000; // 60 seconds

        public class ProcessExecutionResult
        {
            public string StandardOutput { get; set; }
            public string StandardError { get; set; }
            public int ExitCode { get; set; }
            public TimeSpan ExecutionTime { get; set; }
            public bool IsTimeouted { get; set; }
        }

        public int Execute(string fileName, string arguments, string workingDirectory, out string output)
        {
            var result = RunProcess(fileName, arguments, workingDirectory);
            output = result.StandardOutput;
            return result.ExitCode;
        }

        public static ProcessExecutionResult RunProcess(string fileName, string arguments, string workingDirectory = null,  string stdInput = null, int msTimeout = DefaultExecutionTimeout)
        {
            bool hasInput = !String.IsNullOrEmpty(stdInput);

            // Process info

            ProcessStartInfo pInfo = new ProcessStartInfo();
            pInfo.FileName = fileName;
            pInfo.Arguments = arguments;
            if (!String.IsNullOrEmpty(workingDirectory))
                pInfo.WorkingDirectory = workingDirectory;

            pInfo.RedirectStandardInput = hasInput;
            pInfo.RedirectStandardOutput = true;
            pInfo.RedirectStandardError = true;

            pInfo.UseShellExecute = false;
            pInfo.CreateNoWindow = true;

            pInfo.StandardOutputEncoding = Encoding.UTF8;
            pInfo.StandardErrorEncoding = Encoding.UTF8;

            // Run process

            Process process = new Process();
            process.StartInfo = pInfo;
            var stopWatch = Stopwatch.StartNew();
            process.Start();

            if (hasInput)
            {
                process.StandardInput.Write(stdInput);
                process.StandardInput.Close();
            }

            var outTask = process.StandardOutput.ReadToEndAsync();
            var errTask = process.StandardError.ReadToEndAsync();
            var isFinished = process.WaitForExit(msTimeout);
            if (!isFinished)
                process.Kill();
            stopWatch.Stop();

            // Finalization

            return new ProcessExecutionResult()
            {
                StandardOutput = outTask.Result,
                StandardError = errTask.Result,
                ExitCode = process.ExitCode,
                ExecutionTime = stopWatch.Elapsed,
                IsTimeouted = !isFinished
            };
        }
    }
}
