// **********************************************************************************
// Copyright (c) 2015-2022, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2022, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NLedger.Abstracts.Impl
{
    [Flags]
    public enum RunProcessOptionsEnum
    {
        None = 0,
        NoWindow = 1,
        RedirectOutput = 2,
        RedirectError = 4
    }

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

        public int Execute(string fileName, string arguments, string workingDirectory, string input, bool noTimeout = false)
        {
            var timeOut = noTimeout ? Timeout.Infinite : DefaultExecutionTimeout;
            var result = RunProcess(fileName, arguments, workingDirectory, stdInput: input, runProcessOptions: RunProcessOptionsEnum.None, msTimeout: timeOut);
            return result.ExitCode;
        }

        public int ExecuteShellCommand(string command, string workingDirectory, out string output)
        {
            return Execute(GetShellName(), GetShellCommandPrefix() + command, workingDirectory, out output);
        }

        public static ProcessExecutionResult RunProcess(string fileName, string arguments, string workingDirectory = null,  string stdInput = null, int msTimeout = DefaultExecutionTimeout, 
            RunProcessOptionsEnum runProcessOptions = RunProcessOptionsEnum.NoWindow | RunProcessOptionsEnum.RedirectOutput | RunProcessOptionsEnum.RedirectError)
        {
            bool hasInput = !String.IsNullOrEmpty(stdInput);

            // Process info

            ProcessStartInfo pInfo = new ProcessStartInfo();
            pInfo.FileName = fileName;
            pInfo.Arguments = arguments;
            if (!String.IsNullOrEmpty(workingDirectory))
                pInfo.WorkingDirectory = workingDirectory;

            pInfo.RedirectStandardInput = hasInput;
            pInfo.RedirectStandardOutput = runProcessOptions.HasFlag(RunProcessOptionsEnum.RedirectOutput);
            pInfo.RedirectStandardError = runProcessOptions.HasFlag(RunProcessOptionsEnum.RedirectError);

            pInfo.UseShellExecute = false;
            pInfo.CreateNoWindow = runProcessOptions.HasFlag(RunProcessOptionsEnum.NoWindow);

            if (pInfo.RedirectStandardOutput)
                pInfo.StandardOutputEncoding = Encoding.UTF8;
            if (pInfo.RedirectStandardError)
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

            var outTask = runProcessOptions.HasFlag(RunProcessOptionsEnum.RedirectOutput) ? process.StandardOutput.ReadToEndAsync() : null;
            var errTask = runProcessOptions.HasFlag(RunProcessOptionsEnum.RedirectError) ? process.StandardError.ReadToEndAsync() : null;
            var isFinished = process.WaitForExit(msTimeout);
            if (!isFinished)
                process.Kill();
            stopWatch.Stop();

            // Finalization

            return new ProcessExecutionResult()
            {
                StandardOutput = outTask?.Result,
                StandardError = errTask?.Result,
                ExitCode = process.ExitCode,
                ExecutionTime = stopWatch.Elapsed,
                IsTimeouted = !isFinished
            };
        }

        public bool Start(string fileName, string arguments = null)
        {
            if (String.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException("fileName");

            try
            {
                var processStartInfo = new ProcessStartInfo()
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = true
                };

                Process.Start(processStartInfo);
                return true;
            }
            catch (Exception ex)
            {
                VirtualConsole.Error.WriteLine(String.Format("Error: {0}; file name: {1}", ex.Message, fileName));
                return false;
            }
        }

        private string GetShellName()
        {
            return PlatformHelper.IsWindows() ? "cmd" : "bash";
        }

        private string GetShellCommandPrefix()
        {
            return PlatformHelper.IsWindows() ? "/c " : "";
        }

    }
}
