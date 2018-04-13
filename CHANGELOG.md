## 0.7 (In development; not released yet)

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
- Added continuous integration for a development repository ("next-dev");
  build logs and current status are in the file _CI.BuildLog.md;

### Known Issues

TBD

### Bug Fixing

*None* public bug fixing requests at the memoment.

- Fixed handling a category name for "debug" option;

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

(c) 2017-2018 [Dmitry Merzlyakov](mailto:dmitry.merzlyakov@gmail.com)
