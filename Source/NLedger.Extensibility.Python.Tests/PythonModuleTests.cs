using NLedger.Abstracts.Impl;
using NLedger.Extensibility.Python.Platform;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Extensibility.Python.Tests
{
    public class PythonModuleTests : IDisposable
    {
        public PythonModuleTests()
        {
            Assert.True(PythonConnector.Current.IsAvailable);
            PythonConnectionContext = PythonConnector.Current.Connect();
            PythonConnector.Current.KeepAlive = false;
        }

        public PythonConnectionContext PythonConnectionContext { get; }

        public void Dispose()
        {
            PythonConnectionContext.Dispose();
        }

        /// <summary>
        /// This method runs all Python Module unit tests defined in the file /NLedger.Extensibility.Python.Module/tests/ledger_tests.py
        /// It uses application-hosted PythonNet connection, so it does not require PythonNet to be installed in Python environment.
        /// It is expected that produced test result is equal to the test result in Python console (>python.exe ledger_tests.py)
        /// Remember that running unit tests in Python console requires PythonNet installed.
        /// 
        /// Troubleshooting: if this method fails, you should run unit tests in Python console and make sure that they pass well.
        /// If further iinvestigation is needed, check comments in this method below explaining how to get the test result collection and observe it.
        /// </summary>
        [PythonFact]
        public void PythonModule_ExecuteUnitTests()
        {
            var moduleTestFile = Path.GetFullPath("../../../../NLedger.Extensibility.Python.Module/tests/ledger_tests.py");
            Assert.True(File.Exists(moduleTestFile));
            var moduleTestFolder = Path.GetDirectoryName(moduleTestFile);

            using (Py.GIL())
            {
                using (var scope = Py.CreateScope("ledger_tests"))
                {
                    // Unit tests are executed in local scope. Since Python unit test runner can only manage "main" scope,
                    // it is necessary to run it explicitely (unittest.main(...)) specifying a test file name as a parameter
                    // Finally, get a boolean value that indicates whether all tests passed well or not (...result.wasSuccessful())

                    // Test runner parameters are: 
                    // exit=False - supresses exiting the current process when all tests are done
                    // module=None - supresses module importing (basically, "main")
                    // argv[0] - just a string indicating the current process name (required)
                    // argv[1] - name of a unit test file (should be searchable by Python paths)

                    // This method only use a final boolean value (Yes/No) to check whether tests are passed without any diagnostics.
                    // If it is neccessary to troubleshoot this step, it is recommended to extract a test result object and go through detected errors
                    // FOr example: var res = scope.Eval(@"unittest.main(exit=False,module=None,argv=('EmbeddedHost','ledger_tests')).result");

                    scope.Set("module_test_folder", moduleTestFolder);

                    scope.Import("sys");
                    scope.Exec(@"sys.path.insert(0, module_test_folder)");

                    scope.Import("unittest");
                    Assert.True(scope.Eval<bool>(@"unittest.main(exit=False,module=None,argv=('EmbeddedHost','ledger_tests')).result.wasSuccessful()"));
                }
            }
        }
    }
}
