// **********************************************************************************
// Copyright (c) 2015-2017, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2017, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NLedger.IntegrationTests
{
    public sealed class TestRunner
    {
        public TestRunner(string fileName)
        {
            if (String.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException("fileName");

            FileName = Path.GetFullPath(fileName);
            TestCases = ParseTestFile(File.ReadAllText(fileName));
        }

        public string FileName { get; private set; }
        public IEnumerable<TestCase> TestCases { get; private set; }

        public void Run()
        {
            // Tests are configured for 80 symbols in row
            Environment.SetEnvironmentVariable("COLUMNS", "80");

            foreach (var testCase in TestCases)
            {
                Console.WriteLine($"Test case: file {Path.GetFileName(testCase.FileName)}; arguments: {testCase.CommandLine}");
                try
                {
                    var expectedExitCode = 0;

                    var args = testCase.CommandLine;

                    // Find expected exit code of exists
                    var regexExitCode = Regex.Match(args, "(.*) -> ([0-9]+)");
                    if (regexExitCode.Success)
                    {
                        expectedExitCode = Int32.Parse(regexExitCode.Groups[2].Value);
                        args = args.Substring(regexExitCode.Groups[1].Index, regexExitCode.Groups[1].Length);
                    }

                    // Check whether -f option is already presented; add it if does not
                    var regexHasFileOption = Regex.Match(args, @"(^|\s)-f\s");
                    if (!regexHasFileOption.Success)
                        args += $" -f '{FileName}'";

                    // Check whether output is redirected to null; remove it if so
                    var ignoreStdErr = false;
                    var stdErrRedirectToNull = "2>/dev/null";
                    if (args.Contains(stdErrRedirectToNull))
                    {
                        args = args.Replace(stdErrRedirectToNull, "");
                        ignoreStdErr = true;
                    }

                    // Check whether input pipe is needed
                    string stdInContent = String.Empty;
                    var regexStdIn = Regex.Match(args, @"-f (-|/dev/stdin)(\s|$)");
                    if (regexStdIn.Success)
                        stdInContent = File.ReadAllText(FileName);

                    // Check whether arguments have escaped dollar sign; remove it if so
                    args = args.Replace(@" \$ ", " $ ");

                    var envs = new Dictionary<string, string>();

                    using (var inReader = new StringReader(stdInContent))
                    {
                        using (var outWriter = new StringWriter())
                        {
                            using (var errWriter = new StringWriter())
                            {
                                var main = new Main();
                                MainApplicationContext.Current.IsAtty = false; // Simulating pipe redirection in original tests
                                MainApplicationContext.Current.TimeZoneId = "Central Standard Time"; // Equals to TZ=America/Chicago

                                var exitCode = main.Execute(args, envs, inReader, outWriter, errWriter);

                                outWriter.Flush();
                                var output = outWriter.ToString();

                                errWriter.Flush();
                                var err = errWriter.ToString();

                                var normalizedExpectedOutput = NormalizeOutput(testCase.ExpectedOutput);
                                var normalizedOutput = NormalizeOutput(output);

                                var normalizedExpectedErr = NormalizeOutput(testCase.ExpectedError);
                                var normalizedErr = ignoreStdErr ? "" : NormalizeOutput(err);

                                Assert.AreEqual(normalizedExpectedOutput, normalizedOutput, $"Produced output does not match expected one ({DescribeDifference(normalizedExpectedOutput, normalizedOutput)})");
                                Assert.AreEqual(normalizedExpectedErr, normalizedErr, $"Produced error stream does not match expected one ({DescribeDifference(normalizedExpectedErr, normalizedErr)})");

                                Assert.AreEqual(expectedExitCode, exitCode, "Unexpected exit code");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Test case failed with runtime error: {ex.Message}");
                }
            }
        }

        private IEnumerable<TestCase> ParseTestFile(string textContext)
        {
            if (String.IsNullOrWhiteSpace(textContext))
                throw new ArgumentNullException("textContext");

            var lines = textContext.Split(new[] { '\r', '\n' });

            var testCases = new List<TestCase>();
            var startTestLine = -1;
            var commandLine = string.Empty;
            var output = new StringBuilder();
            var err = new StringBuilder();
            var directToErr = false;

            for (int lineNum=0; lineNum<lines.Length; lineNum++)
            {
                var line = lines[lineNum];

                if (line.StartsWith("test "))
                {
                    startTestLine = lineNum;
                    commandLine = line.Substring("test".Length).Trim();
                    output.Clear();
                    err.Clear();
                }
                else
                {
                    if (line.StartsWith("__ERROR__"))
                    {
                        directToErr = true;
                    }
                    else
                    {
                        if (line.StartsWith("end test"))
                        {
                            testCases.Add(new TestCase(FileName, startTestLine, lineNum, commandLine, output.ToString(), err.ToString()));
                            directToErr = false;
                        }
                        else
                        {
                            // If current line contains a template "$sourcepath" - adopt it to current folder
                            var regexFileName = Regex.Match(line, @"\""\$sourcepath/([^\""]*)\""");
                            if (regexFileName.Success)
                            {
                                string fileName = Path.GetFullPath(regexFileName.Groups[1].Value);
                                line = line.Remove(regexFileName.Index, regexFileName.Length);
                                line = line.Insert(regexFileName.Index, "\"" + fileName + "\"");
                            }
                            // The same but w/o quites
                            regexFileName = Regex.Match(line, @"\$sourcepath/(.*)");
                            if (regexFileName.Success)
                            {
                                string fileName = Path.GetFullPath(regexFileName.Groups[1].Value);
                                line = line.Remove(regexFileName.Index, regexFileName.Length);
                                line = line.Insert(regexFileName.Index, fileName);
                            }

                            // If the line contains $FILE template - replace it with current file name
                            line = line.Replace("$FILE", FileName);

                            // Special case: if error message contains a relative path in a file name, expand it
                            var match = ErrFileName.Match(line);
                            if (match.Success)
                            {
                                var idx = match.Groups["path"].Index;
                                var len = match.Groups["path"].Length;
                                line = line.Remove(idx, len).Insert(idx, Directory.GetCurrentDirectory() + "\\");
                            }

                            if (directToErr)
                                err.AppendLine(line);
                            else
                                output.AppendLine(line);
                        }
                    }
                }
            }

            return testCases;
        }

        private static Regex ErrFileName = new Regex(@"Error: File to include was not found: ""(?<path>\.\/)", RegexOptions.Compiled);

        private string NormalizeOutput(string s)
        {
            // TODO - should we consider tailing \n as a problem for .Net code? 
            return s.Replace("\r\n", "\n").Trim();
        }

        private string DescribeDifference (string a, string b)
        {
            a = a ?? String.Empty;
            b = b ?? String.Empty;

            int pos = 0;
            for (int i = 0; i < Math.Max(a.Length, b.Length); i++)
                if (a.Length <= i || b.Length <= i || a[i] != b[i])
                {
                    pos = i;
                    break;
                }

            return $"First difference at {pos} position: '{SafeSubstring(a, pos - 5, 10)}' vs '{SafeSubstring(b, pos - 5, 10)}'";
        }

        private string SafeSubstring(string s, int pos, int len)
        {
            if (String.IsNullOrEmpty(s))
                return String.Empty;

            if (pos < 0)
                pos = 0;
            if (pos > s.Length - 1)
                pos = s.Length - 1;

            if (pos + len > s.Length)
                len = s.Length - pos;

            return s.Substring(pos, len);
        }

    }
}
