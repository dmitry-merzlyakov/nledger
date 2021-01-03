// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utility;
using NLedger.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Abstracts.Impl
{
    /// <summary>
    /// Default implementation of pagination
    /// </summary>
    public class PagerProvider : IPagerProvider
    {
        public string GetDefaultPagerPath()
        {
            var defaultPager = MainApplicationContext.Current.DefaultPager;
            if (String.IsNullOrEmpty(defaultPager))
                defaultPager = VirtualEnvironment.GetEnvironmentVariable("PAGER");

            return defaultPager;
        }

        public TextWriter GetPager(string pagerPath)
        {
            return new PagerTextWriter(pagerPath);
        }
    }

    public class PagerTextWriter : StringWriter
    {
        public PagerTextWriter(string pagerPath)
        {
            PagerPath = pagerPath;
        }

        public string PagerPath { get; private set; }

        public bool ForceOutput
        {
            get { return String.Equals(VirtualEnvironment.GetEnvironmentVariable("nledgerPagerForceOutput"), "true", StringComparison.InvariantCultureIgnoreCase); }
        }

        public override void Close()
        {
            base.Close();
            RunPager(PagerPath, ToString());
        }

        public virtual void RunPager(string pagerPath, string outputText)
        {
            // Optionally copy the output text to stdout to reach compatibility with standard tests.
            if (ForceOutput)
                VirtualConsole.Output.WriteLine(outputText);

            // Run pager
            try
            {
                // Extract possible command line arguments
                var index = pagerPath.IndexOf('|');
                var args = index > 0 ? pagerPath.Substring(index + 1) : null;
                var path = index > 0 ? pagerPath.Substring(0, index) : pagerPath;

                Logger.Current.Debug("pager", () => String.Format("Looking for a pager by path {0}; arguments {1}", path, args));
                var exePath = NLedger.Utility.PlatformHelper.IsWindows() ? FileSystem.GetExecutablePath(path) : path;
                if (String.IsNullOrEmpty(exePath))
                    throw new InvalidOperationException(String.Format("Application '{0}' (arguments: '{1}') not found", path, args));

                Logger.Current.Debug("pager", () => String.Format("Found path {0}", exePath));
                MainApplicationContext.Current.ApplicationServiceProvider.ProcessManager.Execute(exePath, args, null, outputText, noTimeout: true);
            }
            catch (Exception ex)
            {
                Logger.Current.Debug("pager", () => String.Format("Error occured during pager execution: {0}", ex.Message));
                throw new LogicError(String.Format("Error in the pager: {0}", ex.Message));
            }
        }
    }
}
