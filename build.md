# .Net Ledger Development Notes

This document provides general development guidelines:

- How to build NLedger from sources by means of MSBuild;
- How to develop NLedger in Visual Studio;
- How to maintain Ledger tests;
- How to maintain versioning.

## Software Prerequisites

Briefly, you need to have Visual Studio 2015 (any edition) on your development environment
and PowerShell 5.0 - it is the same what I have on mine. Visual Studio 2017 is appropriate too,
but you might need to make a couple of corrections in the build script in case you want to use it.
It is also possible to build NLedger by means of MSBuild only with no Visual Studio installed;
you will need to install Microsoft build and testing components on your own. 

Therefore, the required development software components are:

1. PowerShell 4.0 or higher (5.0 is recommended);
2. In case you prefer to develop in Visual Studio:
   - Visual Studio 2015 or higher (any edition);
3. In case you do not want to have Visual Studio on your machine:
   - .Net Framework 4.0 or higher (4.6.1 is recommended);
   - MS Build v14.0 or higher;
   - VS Test Console.

## Build NLedger from Sources

There is MSBuild project file that performs all build actions; they are:

- Build binaries from sources;
- Execute internal tests (unit and in-proc integration tests that run Ledger test files);
- Compose NLedger release package;
- Run all Ledger tests by means of NLedger Testing Toolkit;
- Create zip package and put it to Bin folder.

There are two ways to run the build process:

- open the build console. It provides several short commands that allow to perform
  either all build actions or just some of them;
- perform all build actions by running a batch file.

Either way you run a build, it will generate an output in the root folder in subfolder
with name *Build-DATE-TIME*. Inside this folder you may find:

- MSBuild log file (short and detail versions);
- Unit test TRX file (a file with extension TRX);
- NLedger Testing Toolkit report (*NLTest.LastTestResults.html*)
- NLedger package (folder with name *package*)
- Nledger zip package (file with name *NLedger-vN.N.zip*); this file is also copied to Bin folder.

The last point is about administrative privileges. The build can work in two modes: with or without 
elevated permissions. The administrative privileges are needed to generate native 
images (call NGen) before running Ledger tests. In case of elevated mode, the build 
passes all tests much faster than otherwise. Therefore, administrative privileges are not
required but they significantly speed up the overall process.

### Build NLedger with Elevated Permissions

1. Open *Build* folder;
2. Execute *NLedgerBuild.Elevated.cmd*; confirm administrative privileges for this process;
3. Find the latest Build-DATE-TIME folder in the root folder and observe results.

### Build NLedger without Elevated Permissions

1. Open *Build* folder;
2. Execute *NLedgerBuild.cmd*;
3. Find the latest Build-DATE-TIME folder in the root folder and observe results.

### Build NLedger in Build Console

1. Open *Build* folder;
2. Execute *NLedgerBuild.Console.cmd*;
3. Read the prompt with the list of available options;
4. Type one of options and click Enter, e.g. *all* *ENTER*
3. Find the latest Build-DATE-TIME folder in the root folder and observe results;
6. Type *exit* to close the console.

## Build NLedger in Visual Studio

In case you use Visual Studio in your development, the steps to build NLedger are:

1. Open NLedger solution (*Source\NLedger.sln*);
2. Build all sources (Build All);
3. Execute unit tests (Run All). This step includes:
   - running all NLedger unit tests (NLedger.Tests);
   - running all ported Ledger unit tests (NLedger.IntegrationTests\unit);
   - running all Ledger test files (TestSetBaseline.cs, TestSetManual.cs, TestSetRegress.cs).

## Install Build Binaries

This step is not required but possible. The use of it is generating native images and, 
consequently, having much faster application that might simplify massive testing.

1. Open *Contrib\Install* folder
2. Execute *NLedger.Install.cmd*

The log of actions is on the screen. Close the console once you read it.

In case you want to revert changes, execute *NLedger.Uninstall.cmd*.

## Working with Ledger Tests

Once you have NLedger built on your dev environment, you may run all Ledger tests against it:

- Open NLedger Testing Toolkit console (*Contrib\NLTestToolkit\NLTest.cmd*) 
- Follow the instruction on the screen. E.g. type *run opt* to run all tests with "opt" in the name;

You can add your own Ledger test files; they are immediately available in the toolkit
if you put them to *Contrib\test* folder.

However, if you want to execute the same test files in Visual Studio as part of *NLedger.IntegrationTests* test,
you need to make several extra steps:

1. Create new Ledger test file and put it to either *test\baseline*, *test\manual* or *test\regress* folder.
2. Open *NLedger.IntegrationTests* in Solution Explorer;
3. Open corresponded subfolder under *test* and add link to the new test file;
3. Run corresponded T4 template (TestSetBaseline.tt, TestSetManual.tt, TestSetRegress.tt).

Now, the new file is available in Visual Studio integration tests.

## Manage Product Info

General product information is kept in the file *Source\ProductInfo.xml*. It has:

- Version information about the current code base (e.g. *NLedger 0.6 Public Beta*);
- Information link to the original source code base;
- Rules to populate Assembly Info version information;
- Licensing information (header template that should appear on the title of every source file).

In case of any changes in this file, the updated information should be applied to source code.
You may do it automatically by executing *Build\ProductInfoUpdate.cmd*. If this script
detects any differences, it automatically applies new information; you only need to 
observe changes and check in updated files when it finishes.

*Note: MSBuild script verifies that product info is actual in the source code and fails 
if it detects any differences*. In such cases, you also need to execute *Build\ProductInfoUpdate.cmd*
and commit updated files.

(c) 2017-2018 [Dmitry Merzlyakov](mailto:dmitry.merzlyakov@gmail.com)
