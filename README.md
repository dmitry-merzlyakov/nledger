[![License](https://img.shields.io/badge/license-BSD-blue.svg?style=flat)](http://opensource.org/licenses/BSD-3-Clause)
[![GitHub release](https://img.shields.io/github/release/dmitry-merzlyakov/nledger.svg?style=flat)](https://github.com/dmitry-merzlyakov/nledger/releases)
[![Gitter chat room](https://badges.gitter.im/nledger/Lobby.svg)](https://gitter.im/nledger/lobby)

# .Net Ledger: Double-Entry Accounting System

.Net Ledger (NLedger) is a complete .Net port of the [Ledger](http://ledger-cli.org), an excellent powerful command line accounting system. 
The main purpose of this project is to enable all Ledger features to .Net world. 

NLedger command line utility (NLedger.cli) is highly compatible with the original Ledger. 
It supports all original features and command line options; the produced output is 100% identical to the original.
Reliable compatibility level confirmed by 99% Ledger tests (except for a few that require deprecated features).
Everything is done to ensure that users have the feeling of using a genuine Ledger. 

NLedger is written in pure C# with no dependencies on any external libraries or any platform-specific features
(with the exception of additional extensions that may depend on the connection libraries).
This allows NLedger to work properly on any .Net compatible system and be embedded in any .Net application. 

## Quick Start

If you have just installed NLedger MSI or ZIP package on Windows and want to know what to do next, you can try:

- Run NLedger in a command line:
  - Click *.Net Ledger Folder* icon in the main menu;
  - Type *cmd* in the address bar of Windows Explorer window;
  - In the command line window, type *ledger --version* - NLedger should show its version;
  - Type *ledger bal -f test/input/drewr3.dat* - you will get the balance for one of the example files that are in *test\input* folder;
- Run NLedger Live Demo:
  - Click *.Net Ledger Live Demo* icon in the main menu;
  - Check out the documentation and try all the examples in action;
- Read this file to the end, look at *Ledger 3 Documentation*, *.Net Ledger Guide* and try to find a use of this tool.

If you installed NLedger from source code, you can open a new command prompt window, go to the NLedger folder and try:
```
ledger --version
ledger bal -f test/input/drewr3.dat
pwsh -file ./get-nledger-tools.ps1 -demo
```

## Features

NLedger faithfully reproduces all valuable Ledger capabilities.
Basically, anything the Ledger can do, NLedger can.
Here is a short list of the most important Ledger features:

- As an excellent accounting system:
  - **Double-entry accounting**. Nuff said; it is a must-have feature;
  - **Multi-posting transactions** allow to incorporate several 
    monetary movements into one logical operation. For example, one transaction may include
    cash withdrawal, writing off money from the card account and corresponded bank fees;
  - **Commodities and Currencies** can represent whatever you work with as a dimension 
    of measurement. It may be currencies, shares, time and whatever else
    that can be measured in numbers and need to be included into accounting;
  - **Amounts** are "smart" numbers that incorporate a number and a corresponded commodity.
    It gives exact information about the matter of an amount and let the application 
    properly manipulate with it. For example, it never adds up 10.00 US dollars with
    5 AAPL but can convert one amount to another; 
  - **Prices and Price History** gives information how to convert one amount to another
    at any moment. You can give this information explicitly (as a price table) but 
    the application can also collect it also in implicit way, by analyzing transactions and
    data conversions in them;
  - **Balancing and auto-balancing** make accounting transactions smarter: the application
    can validate that a transaction balances even it has posts in different currencies
    or commodities. Moreover, in some conditions it can auto-complete it (calculate the last amount)
    that simplify writing the journal records;
  - **Journal, Account** and other basic accounting attributes are here;
- As an effective command line utility:
  - **Text files** are the only source for the accounting utility. It does not require
    any other components, applications, special data files - only text. It is completely
    under your control;
  - **Never change any file** - yes, Ledger only reads input files and generates reports;
  - **Easy syntax of the journal** lets you keep your accounting files in 
    the natural language;
  - **Huge amount of reports** lets you organize your work in the most efficient way;
  - **Command line interface** is very flexible thing that let you configure
    your typical commands in batch files. Once you have them, your efficiency will be 
    much better rather you get the same result in any UI tool;
- As a technically advanced tool:
  - **Inline expressions** allow you to configure your own functions in the journal;
  - **Functions and evaluations** let you do the same in the command line;
  - **Tags** in the journal file let you add meta data to your transactions;

*And many other features, really*. Ledger is very efficient tool with wide capabilities,
so it is highly recommended to familiar with the [original documentation](https://www.ledger-cli.org/docs.html),
read other resources (http://plaintextaccounting.org)
and keep yourself in loop with [Ledger Community](https://www.ledger-cli.org/contribute.html).

### The Use of NLedger

*So, if Ledger is so nice, why NLedger?* Here is the answer or, in other words, the project mission :)

The first benefit of NLedger is bringing Ledger functionality to the Windows world. 
Even as a developer, I ran into difficulties running Ledger 3.1.1 on Windows 10, 
and it seems to me that for inexperienced users this will be an unsolvable problem.

I want everyone to be able to install NLedger with one click and use it exactly as described in the Ledger documentation. 
Ledger, but on any Windows - this was the first target.

The second benefit of NLedger is that it is a native .Net application. 
This gives unlimited possibilities for expanding it with additional functions and integration with other products on the .Net platform. 
It was the second reason why it was decided to port Ledger to .Net.

## Project Vision and Development Progress

The ultimate overall goal of this project is to have a fully functional Ledger
on .Net platform in the form of a command line utility plus provide a way to give 
seamless access to the same functions for external .Net applications.

- Current **Project Status** is:
  - Ported from [Ledger 3.2.1](https://github.com/ledger/ledger), branch Master, commit 56c42e11; 2020/5/18
  - All functionality ported; command line utility is available;
  - Program API for developing with NLedger is created; NuGet packages are available;
  - Ledger testing framework is ported; 
  - Ledger tests are passed:
    - 99% (708 out of 711) test cases passed;
    - 3 test cases are ignored because of known limitations;
    - 0 failed.
  - The Ledger Python module is available as a standalone product for Python users. 
- **Current limitations** (technical restrictions that will be addressed by next releases) are:
  - The .Net DateTime parser throws less specific error messages and cannot catch the same errors as the Ledger. 
    The corresponded test is disabled pending a further decision;
  - NLedger Python extension supports Python 3.6 or later.
    The test designed for Python 2 is disabled as deprecated;
  - It was found that under some conditions, the original Ledger gave incorrect rounding in the last processing step (*stream_out_mpq*).
    The problem was caused by the arbitrary precision arithmetic library that Ledger used. 
    Certain combinations of dividend and divisor produce a rounded result that does not match the expected bank rounding. 
    Affected Ledger tests were corrected (opt-lot-prices, opt-lots, opt-lots_basis).
      
- **Development Roadmap** is available by this [link](https://github.com/dmitry-merzlyakov/nledger/blob/master/roadmap.md).
  It describes the plan to complete all intended features by the version 1.0 and notes for further steps.

## Installation

NLedger can run on any system that supports .Net (either .Net Framework or .Net Core).
However, the testing framework and helper tools use PowerShell, so you can only run Ledger tests if you also have PowerShell installed on your system. 
If you also want to use the Python extension, you should have Python on your machine. 

### System Requirements

- .Net Framework 4.7.2 or higher and/or .Net Core SDK 3.1 or higher. It is required component to run the command line application;
- PowerShell 5.0 or higher. It is needed to run testing framework and other tools.
- Python 3.6 or later. It is required for Python extension.

PowerShell is optional, you can still use NLedger if it is not installed, but the ability to run PowerShell scripts makes your life easier.

### Build and Install from source code

(Windows, Linux, OSX) Execute the following commands:

```
git clone https://github.com/dmitry-merzlyakov/nledger
cd nledger
pwsh -file ./get-nledger-tools.ps1 -pythonConnect
pwsh -file ./get-nledger-up.ps1 -install
```

On Windows, depending on your Powershell version, the last commands may look like:
```
powershell -ExecutionPolicy RemoteSigned -File ./get-nledger-tools.ps1 -pythonConnect
powershell -ExecutionPolicy RemoteSigned -File ./get-nledger-up.ps1 -install
```

The command `pythonConnect` is needed if you want to run Python-related unit tests. It can be skipped otherwise.

### Install from NLedger Installation Package

(Windows only)

- Download the latest NLedger installation package (MSI file) from [Releases](https://github.com/dmitry-merzlyakov/nledger/releases);
- Run the installer and follow instructions on the screen;
- Review *nledger.html* when installation finishes.

*Note: the installer will request elevated permissions to call NGen.*

### Install from Binary Package

(Windows only)

- Download the latest NLedger installation package from [Releases](https://github.com/dmitry-merzlyakov/nledger/releases);
- Unpack the package to a temp folder;
- Open the file *nledger.html* and follow the installation instruction.

OR (for impatient people):

- move unpacked NLedger to a place of permanent location (e.g. *Program Files*);
- Open *NLedger/Install* folder and execute NLedger.Install.cmd (confirm administrative privileges to let it call NGen); close the console;
- Run Windows Command Line (e.g. type *cmd* in the search bar) and type *ledger* in it.

## NLedger NuGet package

NLedger is available as NuGet packages. Read more how to develop with NLedger [here](https://github.com/dmitry-merzlyakov/nledger/blob/master/build.md).

## NLedger Python Module

NLedger Python module provides Ledger features in a Python interpreter session.
It can be used as stand-alone software; it does not require installing NLedger.
The latest package can be downloaded from [Releases](https://github.com/dmitry-merzlyakov/nledger/releases).
You can find more information in [.Net Ledger Documentation](https://github.com/dmitry-merzlyakov/nledger/blob/master/nledger.md) or in the module [Readme](https://github.com/dmitry-merzlyakov/nledger/blob/master/Source/NLedger.Extensibility.Python.Module/README.md) file.

## Documentation

As mentioned, the main source of information on how to use this application are Ledger-related resources and the community. 
Therefore:
- Refer to [Ledger documentation](https://www.ledger-cli.org/docs.html) or 
  read [other resources](http://plaintextaccounting.org/) to get conceptual 
  information about command line accounting and Ledger capabilities;
- Refer to [NLedger documentation](https://github.com/dmitry-merzlyakov/nledger/blob/master/nledger.md) if you have questions about running .Net 
  application in your system.

## Contribution

NLedger is currently in active development and some enhancements are still coming.
You can check the planes in [Roadmap](https://github.com/dmitry-merzlyakov/nledger/blob/master/roadmap.md).
However, code quality is a primary focus, so any bug fixing requests and/or fixing changes will be processed in the first order. 
Therefore, if you want to help this project:

- Any help with testing NLedger is greatly appreciated. You can leave information about found 
  defect on [Issues](https://github.com/dmitry-merzlyakov/nledger/issues) tab; they will be processed in the first order;
- Anyone who would like to provide a fix for any found defect are welcome.
  Please, create pull requests for fixing changes; they will be processed in the first order as well;
- Enhancements are developed under my control.
  Of course, anyone of you can make a fork from this code and extend it on your own, enjoy!

### How to inform about found defects

1. First of all, please check project [Issues](https://github.com/dmitry-merzlyakov/nledger/issues) 
   on GitHub and Known Issues in [CHANGELOG](https://github.com/dmitry-merzlyakov/nledger/blob/master/CHANGELOG.md).
   The issue you found might be already recorded.
2. Check that the issue is reproducible and describe it.
3. Ideally, locate the defect and create Ledger test file that exposes the problem.
   This file should properly pass test with the original Ledger and fail with NLedger.

## Credits

Special thanks to *John Wiegley* for the nicest accounting tool I've ever seen.
I really like it very much and it was a great pleasure for me to analyze its code
in the smallest detail. Thought it was quite big challenge for me 
(GDB left the corns on my hands :)) I've got an invaluable experience. Thank you! 

I would like to express my gratitude to the creators of the [PythonNet](https://github.com/pythonnet/pythonnet) library, 
who have worked hard to solve complex problems of interacting with Python and provided the developers with an effective tool. 

## Contact

- Join us in the chat room here: [![Gitter chat room](https://badges.gitter.im/nledger/Lobby.svg)](https://gitter.im/nledger/lobby);
- Twitter: [#nledger](https://twitter.com/search?q=%23nledger) .Net Ledger news;
- Send an email to [Dmitry Merzlyakov](mailto:dmitry.merzlyakov@gmail.com)

## Licensing

The code is licensed under 3-clause [FreeBSD license](https://github.com/dmitry-merzlyakov/nledger/blob/master/LICENSE).

(c) 2017-2022 [Dmitry Merzlyakov](mailto:dmitry.merzlyakov@gmail.com)
