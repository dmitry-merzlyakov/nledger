# NLedger Development Roadmap

This is a roadmap for development of next NLedger releases.

## NLedger 0.7

**Code completion and verification; improve builds.**

- Code verification: process all *TODO/TBD/In Progress/NotImplemented* markers in code;
- Code verification: process warnings in unit test projects;
- Code completion: verify and complete DEBUG and TRACE messages;
- Code completion: handle special options (*verify*, *verbose*, *memory*, *init-file*);
- Code completion: add *man* command (show current Ledger documentation in browser);
- Update the source code and tests to the latest Ledger version (branch NEXT);
- Build: configure VSTS builds on hosted agents;
- Build: VSTS: add automatic commits for built packages; publish MD5 hashes;
- Build: VSTS: configure Continuous Integration builds for in-development releases;
- Bug fixing: prompt in the interactive mode;
- Bug fixing: fix filtering by categories in *--debug* option;
- Deploy package: add more interactive examples in a console;
- Build: add creating MSI packages (Wix);

## NLedger 0.8

**Address complicated issues that require big code changes but have minor effect 
on general functionality: date parsing, pager, localization, decimal limitations.**

- Update the source code and tests to the latest Ledger version (branch NEXT);
- Bug fixing: solve issues with .Net date parser error messages; 
  consider creating an own date parser; enable corresponded Ledger unit and integration tests;
- Bug fixing: solve issues with file names in error messages (absolute vs relative); 
  simplify corresponded Ledger integration tests;
- Code completion: add integrated pager; add complete support of external pagers;
  solve issues with colorization; simulate "cat" pager and enable corresponded Ledger test;
- Code completion: add localization for errors and messages; 
  verify and process method *gettext_*, *throw_*;
- Code completion: consider replacement of decimal for number calculation with
  bigger alternatives; solve limitations in Ledger unit tests; consider 
  rounding issues in ledger integration tests;

## NLedger 0.9

**Big enhancements: connector library and seamless integration with other language environments.**

- Enhancement: create a adapting library (*connector*) that allows seamless integration
  between external applications and NLedger core functions;
  - add virtualization of the file system;
  - encapsulate environment configuration;
  - add generic processor for colorization tokens;
  - add a wrapper class for NLedger commands and arguments;
  - add an engine factory and host to manage life cycles of NLedger objects;
  - implement single-domain and single-thread life cycles;
  - implement single and multi-command modes (to simulate Ledger integrative mode);
  - verify multi-threading and async calls; verify static members in NLedger code;
  - add support for STOP signal in NLedger code; 
  - simple API and data transfer objects to communicate with NLedger domain model and methods;
- Code completion: consider adding support of Python integration and other languages;
  - encapsulate code that requires Python integration to wrappers;
  - implement two-way Python integration by means of Iron Python;
  - enable Ledger integration tests that require Python;
  - generalize integration API and enable a way to integrate with other languages (like PowerShell environment);
- Build: publish NLedger in NuGet repository;

## NLedger 1.0

**Code optimization and stabilization**

- Update the source code and tests to the latest Ledger version (branch NEXT);
- Code optimization (analyze efficient of *lookup* implementation and other critical places);
- General performance analyzing and optimization;
- Code stabilization and bug fixing;

(c) 2017 [Dmitry Merzlyakov](mailto:dmitry.merzlyakov@gmail.com)
