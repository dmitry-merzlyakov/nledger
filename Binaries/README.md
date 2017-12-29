# NLedger Installation Packages

This folder contains NLedger installation packages
that are ready to deploy on a target machine.

## Installation Packages in ZIP Archives

Current NLedger version provides the packages as zip archives with pre-built binaries and
other deployment artifacts. You may download the package
(e.g. *NLedger-v0.6.zip*), unpack to any local temp folder,
open the file [NLedger\nledger.html](https://github.com/dmitry-merzlyakov/nledger/blob/master/nledger.md) and follow the instruction
in the section *Installation*.

### For Very Busy People :) Quick Start

1. Download NLedger package (*NLedger-v0.6.zip*)
2. Unpack the package
3. Open the folder with unpacked files; move *NLedger* to any appropriate location;
4. Open *NLedger/Install* folder and execute NLedger.Install.cmd (confirm administrative privileges to let it call NGen); close the console;
5. Run Windows Command Line (e.g. type *cmd* in the search bar) and type *ledger* in it.

That is it; NLedger shows a standard prompt.

## Build Logs

In case you want to observe build logs and test results, you may 
look at files with names *NLedger-BuildLog-vN.N.zip*. They contain:

- MS Build log files;
- Unit test results (TRX file);
- NLedger Testing Toolkit test report (*NLTest.LastTestResults.html*).

(c) 2017 [Dmitry Merzlyakov](mailto:dmitry.merzlyakov@gmail.com)
