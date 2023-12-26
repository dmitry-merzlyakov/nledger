# .Net Ledger Development Roadmap

This is a roadmap for development of next NLedger releases.

## NLedger 0.7

**Code completion and verification; improve builds.**

- [Complete] Code completion: add arbitrary precision arithmetic to deal with Amount quantities;
  enable validation of big numbers in Ledger unit tests;
- [Complete] Code verification: process all *TODO/TBD/In Progress/NotImplemented* markers in code;
- [Complete] Code verification: process warnings in unit test projects;
- [Complete] Code completion: verify and complete DEBUG and TRACE messages;
- [Complete] Code completion: handle special options (*verify*, *verbose*, *memory*, *init-file*);
- [Complete] Code completion: add *man* command (show current Ledger documentation in browser);
- [Complete] Update the source code and tests to the latest Ledger version (branch NEXT);
- [Complete] Build: configure VSTS builds on hosted agents;
- [Complete] Build: VSTS: add automatic commits for created packages and build logs; publish MD5 hashes;
- [Complete] Build: VSTS: create DEV branches and configure Continuous Integration builds for them;
- [Complete] Build: improve MD5 to HTML conversion; add automatic updates for doc files (licensing, bug tracking);
- [Complete] Bug fixing: prompt in the interactive mode;
- [Complete] Bug fixing: fix filtering by categories in *--debug* option;
- [Complete] Deploy package: add interactive examples in a console;
- [Complete] Deploy package: add a setup console;
- [Complete] Build: add creating MSI packages (Wix);

## NLedger 0.8

**Support third-party software development, technology modernization and code completion**

- [Complete] Update the source code and tests to the latest Ledger version (done, updated to 3.2.1);
- [Complete] Enhancement: present a development API for embedding NLedger functionality into third-party applications (Service API). Includes:
  - abstraction and virtualization level (virtual file system, console, environment and devices);
  - extendable ANSI preprocessor to encapsulate colorization;
  - extendable configuration;
  - application service model (engine host, session and request object management);
  - isolated thread-safe model with cancellation support;
- [Complete] Publish NLedger in NuGet repository;
- [Complete] Technology modernization: support Support .Net SDK. Includes:
  - migration to SDK-style projects;
  - migration from MSTest to xUnits;
  - generating and verification .Net Framework 4.5, .Net Standard 2.0 and .Net Core 3.1 binaries;
  - multi-platform capabilities: build, testing and functioning on Windows, Linux and OSX;
- [Complete] Bug fixing (GH #2, GH #7);

## NLedger 0.9

**Extending functional and data integration API; two-way integration with other language environments; code completion**

- [Complete] Create Extensibility API allowing seamless integration with Ledger domain model for external software
- [Complete] Create .Net extension that integrates the Ledger runtime with external .Net applications
- [Complete] Create Python extension that allows to extend Ledger data files with Python functions
- [Complete] Create .Net Ledger Python module that provides access to Ledger domain objects in a Python interpreter session
- [Complete] Code completion: enable Ledger tests that require Python integration;
- [Complete] Added support for multi-target deployment; migrating solution to LTS .Net versions (.Net Framework 4.8, .Net 6.0, .Net 8.0) GH #32
- [Complete] Reorganizing Powershell helper scripts; created a common .Net Ledger Tools management console
- [Complete] Bug fixing (GH #24 #25 #26 #29)
- Update the source code and tests to the latest Ledger version (branch NEXT)
- Update Python Extension to support PythonNet 3.0.3 (NLedger binary and Python module)
- Code completion: cover Powershell scripts by Pester tests and include them into the Release build
- Migrate NLedger Release build from Azure to GitHub Actions; upgrade WiX project
- Reorganize NLedger documentation

## NLedger 1.0

**Bug fixing, code completion, optimization and stabilization**

- Code completion: complete well-formatted code documentation, enable generating XML documentation file and include into NuGet package.
- Code completion: add localization capabilities for error and warning messages; 
  verify and process Ledger methods *gettext_*, *throw_*;
- Code optimization (analyze efficient of *lookup* implementation and other critical places);
- General performance analyzing and optimization;
- Code completion: solve issues with .Net date parser error messages; 
  consider creating an own date parser; enable corresponded Ledger unit and integration tests;
- Code completion: revisit and manage postponed improvements listed in ProductInfo.xml
- Code stabilization and bug fixing;

## Further Enhancements

**Features planned for future releases**

- Enhancement (Service API): add support of semantical tokenization for NLedger output;
- Enhancement (Service API): add auxiliary classes that help to build textual Ledger commands programmatically;
- Enhancement (Service API): add auxiliary classes that can render Ledger Journal from a Journal object instance
- Code completion: add integrated pager; add complete support of external pagers
- Create Powershell extension that allows to extend Ledger data files with Powershell functions


(c) 2017-2023 [Dmitry Merzlyakov](mailto:dmitry.merzlyakov@gmail.com)
