## 0.8.5 (2023-12-28)

### Release Information

- Support for multi-target deployments
- Migrating to LTS .Net versions
- Created .Net Ledger Tools management console
- Bug fixing

### Features

- Default solution targets are changed to current LTS versions (.Net Framework 4.8, .Net 6.0, .Net 8)
- Public packages (MSI and ZIP) are distributed with pre-built binaries for LTS targets
- Public NuGet package includes Standard 2.0, .Net 6.0 and .Net 8 targets
- Development build script `get-nledger-up.ps1` is changed to support custom targets. Developers can build and test binaries for any supported .Net versions.
- Development build script `get-nledger-up.ps1` is changed to reflect local environment limitations. Non-available targets are silently skipped.
- Added a single management console `.Net Ledger Tools` that incorporates commands for installation, configuration, testing, Python management and running interactive demo

### Known Issues

- Listed in ProductInfo.xml
- NLedger Python module requires PythonNet module version 2.5.2 or 3.0.0-pre. Adding support for PythonNet 3.0.3 is addressed by further development.

### Bug Fixing

- Bug fixing (GH #24 #25 #26 #29)

### Breaking Changes

- .Net Ledger Tools management console replaces interactive PowerShell scripts for installing, testing, Python management and running Live Demo.

## 0.8.4 (2021-12-01)

### Release Information

- Introduced Extensibility API allowing seamless integration with Ledger domain model for external software
- Created .Net and Python extensions for .Net Ledger
- Enabled Python integration tests
- Created .Net Ledger Python module as a standalone software product
- Bug fixing

### Features

- Building on the original Ledger Python integration concept, a generic Extension API has been created to provide software integration capabilities:
    - It manages custom code and functions in Ledger data files (custom functions in expressions, key words *import* and *python*, command *python*).
    - It provides full access to Ledger domain objects from custom code.
    - It exposes a Standalone Session concept that allows custom code to use Ledger domain objects directly in its own procedures.
    - It extends the console application and Service API so you can use integration capabilities along with other Ledger features.
- The Python Extension for .Net Ledger is built on the Extension API:
    - It fully supports all the original Ledger Python features. Python-related integration tests passed. 
    - Interoperability with the Python runtime is based on the PythonNet library, which implies a wide list of supported CPython distributions.
      Helper scripts minimize the effort of setting up a connection to the Python runtime. 
    - The .Net Ledger Python module is available as a Wheel package and can be redistributed as a standalone product. .Net Ledger installation is not required.
- The .Net extension integrates the Ledger runtime with external .Net code:
    - It can extend data processing with functions from custom .Net assemblies.
      With the Service API, it also allows direct interaction with global and context objects. 
    - It provides standalone session capabilities, which allows you to create a scope where your code interacts directly with Ledger domain objects.
    - All members of Ledger Domain model are accessible.

### Known Issues

Listed in ProductInfo.xml

### Bug Fixing

- Fixed multi-threading issue in string extensions (GetWidthAlignFormatString)
- Fixed Service API session disposal issues (access to global objects)
- Performance improvements: optimized creating lookup definitions for transactional objects
- Fixed [Integer] / [Amount] Value division (see commit 2d1570c6)
- Commodity Defaults are moved from a static variable to the application context

### Breaking Changes

- The helper method Amount.Parse(string) has been renamed to Amount.ParseString(string) to avoid assembly import issues for external libraries. 
  In particular, when importing an assembly, PythonNet couldn't find the difference between Amount.Parse(ref string) and other overloaded methods. 
  Modification does not affect the core code.
- .Net Framework version for executable binaries is changed to 4.7.2 for better management of Standard 2.0 dependencies.
- Changed format of .Net Ledger Test Toolkit metadata file (NLTest.Meta.xml).

## 0.8 (2020-10-02)

### Release Information

- Upgrade to Ledger 3.2.1 (May 2020)
- Support .Net Core and multi-platform capabilities 
- Added development API for embedding NLedger functionality into third-party software

### Features

- NLedger code base and functional tests are upgraded to Ledger 3.2.1 (commit 56c42e11; 5/18/2020)
- NLedger is migrated to .Net SDK-style project format; updated project dependencies (build script)
- Unit tests are migrated from MSTest to xUnits
- Multi-target builds (generating NLedger.dll for .Net Framework 4.5 and .Net Standard2.0; CLI executable - .Net Framework 4.5 and .Net Core 3.1).
  All binaries are verified with unit tests and Ledger testing framework
- Multi-platform builds (compilation, unit and integration testing on Windows, Linux ans OSX); added auxiliary quick-start tools
- Created a development API for embedding NLedger functionality into third-party applications (thread-safe isolated application service model)
- Published NLedger NuGet package

### Known Issues

Listed in ProductInfo.xml

### Bug Fixing

- Fixed NLedger GH#7 (Error in Calculation)
- Addressed NLedger GH#2 (Core double entry components in nLedger) by new NLedger Service API
- Fixed a problem with "--account=code" balance option in REPL mode

### Breaking Changes

- Since NLedger is migrated to .Net SDK-style project format, the minimal required version of Visual Studio is 2017; required version of .Net SDK is 3.1

## 0.7 (2018-08-12)

### Release Information

Code and feature completion; continuous integration.

### Features

- Completed all TRACE, DEBUG and INFO messages;
- Completed all VERIFY assertions;
- Added arbitrary precision arithmetic to deal with Amount
  quantities; original Ledger tests that validate big numbers are passed;
  performance degradation is not detected. Decimal arithmetic is still 
  available as a compilation option;
- Completed "download" and "price-db" features; added corresponded 
  tests (test/nledger/opt-download) and example scripts (Extras/getQuote);
- Added support of "--help" option; it shows Ledger Man Pages in a default browser;
- Added support of external pagers ("--pager" option);
- Code is synced with latest changed in Ledger code base (up to 4/9/2018);
- Added continuous integration for a development repository ("next-dev");
  build logs and current status are in the file _CI.BuildLog.md;
- Added interactive Live Demo web console;
- Added Setup console that manages application settings;
- Added MSI installation package;

### Known Issues

Listed in ProductInfo.xml

### Bug Fixing

*None* public bug fixing requests at the memoment.

- Fixed handling a category name for "debug" option;
- Fixed showing prompt in interactive mode;

### Breaking Changes

*None*

## 0.6 (2017-12-29)

### Release Information

First public beta of NLedger for community preview. 
Porting Ledger functionality is finished; Ledger tests are passed.

Current NLedger code base is equal to Ledger code 3.1.1 
(https://github.com/ledger/ledger ; branch NEXT; commit b414544; 2017/02/17).

Ledger tests: 98% passed (659 test cases); 13 test cases are ignored; 0 failed.

### Features

Completed basic functionality and main project components:

- All Ledger functions are ported and covered by unit tests;
- Command line utility is complete; its behavior is equal to the original application;
- Ledger unit tests (*test\unit*) are ported and included into the solution;
- Ledger integration tests (*.test* files) are run together with other unit tests;
- Created an external testing framework (Testing Toolkit) that runs Ledger tests against the executable file;
- Created a MSBuild project, installation scripts and a deployment package;

### Known Issues

Besides current general limitations (no Python integration, 
decimal type for arithmetic calculations), there are following issues
that will be addressed by further releases:

- Option *--debug* does not filter the output messages by categories;
- Incomplete *DEBUG* and *TRACE* messages in some components;
- No prompt character in interactive mode;
- Command *man* is incomplete; it does not show Ledger documentation file;
- Special command line options (*verify*, *verbose*, *memory*, *init-file*) are incomplete;
- No special integration with pagers; external pagers lose colors;
- Some date parsing error messages might be differ on .Net platform 
  (see the test *regress\BF3C1F82.test*)
- Some file error messages might be differ (NLedger always writes absolute paths in messages).

### Bug Fixing

*None* at the moment of the first public release.

### Breaking Changes

*None* at the moment of the first public release.

(c) 2017-2020 [Dmitry Merzlyakov](mailto:dmitry.merzlyakov@gmail.com)
