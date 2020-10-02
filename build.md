# .Net Ledger Development Notes

This document provides guidelines for developing software that uses NLedger as a component or maintaining NLedger source code itself.

## Developing with NLedger

NLedger as an application consists of two components: NLedger.dll that contains all core functions and an executable file. 
The former is a .Net assembly that can be linked to any host .Net application and provide NLedger capabilities.

In order to simplify such an integration, NLedger code base was extended with API that implements a simple service model; NLedger.dll was also published as a NuGet package.
Therefore, if you want to extend your software with NLedger functionality, you can install NLedger NuGet package and call NLedger functions by means of that API.

The main purpose for API is to provide an abstraction and virtualization layer over the core code so that you can build your software running NLedger functions like in a sandbox. It also provides secure multi-threading, pre-processing ANSI codes and other helper functions.

NLedger NuGet package contains binaries compiled for .Net Framework 4.5 and Standard 2.0. Actually, they are the same dlls that are created for the console application, 
so they passed all unit and integration tests.

### Where to get NLedger NuGet package

Primarily, NLedger NuGet package is available in the public NuGet repository. It is periodically updated with every new NLedger release (starting with version 0.8).

If you want to get the latest CI package, you can download it [here](https://github.com/dmitry-merzlyakov/nledger/blob/next-dev/_CI.BuildLog.md). 
If you are not sure how to install a local NuGet package file, you can check the answers to this question: [How do I install a NuGet package .nupkg file locally?](https://stackoverflow.com/questions/10240029/how-do-i-install-a-nuget-package-nupkg-file-locally).

Ultimately, you can create a NuGet package from source code by means of `dotnet pack --configuration Release` command (see more about local builds below).

### How to use NLedger Service API

The integration API (NLedger Service API) is intended to manage the following tasks:

- Abstraction: provide a collection of abstract interfaces presenting NLedger environment:
    - `IFileSystemProvider` - file system abstraction;
    - `IVirtualConsoleProvider` - text console abstraction;
    - `IProcessManager` - running new processes;
    - `IQuoteProvider` - requesting updates for stock quotes;
    - `IManPageProvider` - requesting a user manual;
    - `IPagerProvider` - requesting output pagination;
- Virtualization: provide a collection of implementers that allow to run NLedger in a sandbox, for example:
    - `FileSystemProvider` operates with System.IO (real file system); `MemoryFileSystemProvider` simulates the file structure in memory;
    - `MemoryStreamManager` simulates input/output console streams in memory;
- Pre-processing VT-100 ANSI colorization code:
    - `BaseAnsiTextWriter` extracts ANSI code from the stream;
    - `MemoryAnsiTextWriter` can put corresponded html tokens to the output;
- Access via a common service model:
    - The classes `ServiceEngine`, `ServiceSession`, `ServiceResponse` provide a simple way to call any NLedger functions;
    - They are thread-safe, support cancellation and provide good level of isolation;
- Controllable configuring and Extendability:
    - All components for a virtual environment can be configured or re-configured;
    - All abstractions can be re-implemented in case of any needs. 

The integration API is located in the namespace `NLedger.Utility.ServiceAPI` and contains three main classes:

- `ServiceEngine` is a factory that creates service session objects; it holds a configuration that represents a virtual NLedger environment;
- `ServiceSession` encapsulates reading and building Journal object from data file(s) or an input stream. Once Journal is built, the session is ready to serve requests;
- `ServiceResponse` contains the result for an individual request, e.g. it can return a balance or registry table.

So, when you need to execute an NLedger command via Service API, you code should do:

- Create ServiceEngine object and, optionally, configure it. Typically, you only need for a single ServiceEngine object in an application (you might need to create more if you have different configurations for virtual environments);
- Call ServiceEngine to create ServiceSession object. Here, you only need to specify a data source (either file names or just an input data as a text). When ServiceSession is being created, it reads input data, builds Journal (and all other ledger stuff) and prepares everything to be ready to serve requests (ledger commands);
- Call ServiceSession to execute a command and create ServiceResponse object. At this moment, it executes a command and populates ServiceResponse with output texts and contextual information.

Here is an example of getting a balance report from an input text:
```c#
using NLedger.Utility.ServiceAPI;

var InputText = "2009/10/29 (XFER) Panera Bread"  // Complete input text...

var engine = new ServiceEngine();
var session = engine.CreateSession("-f /dev/stdin", InputText);
var response = session.ExecuteCommand("bal checking --account=code");

System.Console.WriteLine (response.OutputText);   // Print balance...
```

API supports async methods, so this code might be used to read data and execute a command in an async way:

```c#
private async Task<ServiceResponse> GetBalance(ServiceEngine serviceEngine, string fileName)
{
   var session = await serviceEngine.CreateSessionAsync($"-f {fileName}");
   var response = await session.ExecuteCommandAsync("bal");
   return response;
}
```

By default, API disables ANSI colorization (sets IsAtty = false) to clear out ANSI code from the output text. You can decide to change this behavior and replace ANSI code with HTML tags.
This example shows how to configure ServiceEngine to use MemoryAnsiTextWriter for this purpose.

```c#
var engine = new ServiceEngine(
      configureContext: context => { context.IsAtty = true; },   // Enable colorization
      createCustomProvider: mem =>
      {
         mem.Attach(w => new MemoryAnsiTextWriter(w));           // MemoryAnsiTextWriter replaces ANSI with HTML
         return null;
      });

var session = engine.CreateSession("-f /dev/stdin", InputText);  // Etc..
```

Here is an example of the output text (css class name "fg" means "foreground"; "fb" - background; the numbers are equal to [ConsoleColor](https://docs.microsoft.com/en-us/dotnet/api/system.consolecolor?view=netcore-3.1) enum):
```html
              $20.00  <span class="fg1">DEP:Assets:Checking</span>
              <span class="fg4">$-9.00</span>  <span class="fg1">XFER:Assets:Checking</span>
--------------------
              $11.00
```

Input data can be specified as an input text or a file name. By default, API uses a real file system, so in the latter case it will look for a physical file. 
You can configure a virtual file system and locate the input data in memory:

```c#
var fs = new MemoryFileSystemProvider();
fs.CreateFile("input.dat", InputText);  // Put input data to a virtual file with name 'input.dat'

var engine = new ServiceEngine(
      createCustomProvider: mem =>
      {
         return new ApplicationServiceProvider(
            fileSystemProviderFactory: () => fs,
            virtualConsoleProviderFactory: () => new VirtualConsoleProvider(mem.ConsoleInput, mem.ConsoleOutput, mem.ConsoleError));
      });

var session = engine.CreateSession("-f input.dat");  // Use the virtual file
var response = session.ExecuteCommand("bal checking --account=code");
```

If you need to specify environment variables, you can do it in this way:

```c#
var engine = new ServiceEngine(
      configureContext: context => 
         {
            context.SetEnvironmentVariables(new Dictionary<string, string>() { { "COLUMNS", "120" } });
         });
```

### Troubleshooting

If you have an unexpected calculation result, the first thing you can do is to check how your example works in NLedger or Ledger applications. Some Ledger features might be a bit tricky,
so it is recommended to validate whether your case works as you expect in the original application. The best way to do so is to create a test file (see Testing FRamework capabilities);
you can use this file in discussion with Ledger community to decide whether your case is a defect or a feature.

Otherwise, if you see that API works differently rather than the application - you can debug NLedger source code or create a [bug](https://github.com/dmitry-merzlyakov/nledger/issues).

## Maintaining NLedger

You might need to troubleshoot NLedger or build it from the sources. This section describes how to develop NLedger.

### Software Prerequisites

Here is a full list of software components that are recommended for NLedger development. Some of them might be not available on your help operation system or not needed for your activities. 

- .Net Core SDK 3.1 or later
- .Net Framework 4.5.2 or later (if you want to build NLedger for .Net Framework)
- Visual Studio 2017 or Visual Studio Code with c# extension
- Powershell (7.0 is recommended though 5.0 can work properly on Windows)
- Git command line tool (only if you prefer command line)
- MS Build 15.0 and [Wix ToolSet](http://wixtoolset.org/releases) 3.12 or higher (only if you want to run an "official" build with MSBuild)

### Where to get NLedger source code

NLedger source code is on GitHub, so the simplest way is to execute Git command:

```
git clone https://github.com/dmitry-merzlyakov/nledger.git
```

This command downloads the content of `master` branch, so you will have source code for the latest public release. If you want to get the latest development updates, you should switch to `next-dev` branch:

```
git checkout next-dev
```

If you want to make changes in code and save them for further work, the better idea of to *fork* the source code base so that you will have an own copy of code.

### Build NLedger source code

This action implies a manual development build: compile binaries (.Net Framework and/or .Net Core) and run unit tests. Unit tests checks binaries for both platforms and include:
   - NLedger unit tests (NLedger.Tests);
   - ported Ledger unit tests (NLedger.IntegrationTests/unit);
   - Ledger testing framework files (TestSetBaseline.cs, TestSetManual.cs, TestSetRegress.cs) and NLedger additions (TestSetNLedger.cs).

If you want to build MSI and ZIP packages, you should go to Packaging Build section.

> Note: both builds produce equal binaries with one distinction: development build does not populate Patch and Build version numbers and does not set SourceRevisionId value.

There are several options; you may follow any of listed below.

#### .Net SDK build

1. Go to *nledger/Source/* folder
2. Execute `dotnet build --configuration Release`
    - If you want to build Core binaries only, the command is: `dotnet build --configuration Release /p:CoreOnly=True`
    - For OSX, you also need to add a switch `-r osx-x64`
2. Execute `dotnet test --configuration Release`
    - For Core binaries only: `dotnet test --configuration Release /p:CoreOnly=True`
    - For OSX, add a switch `-r osx-x64`

#### Running 'get-nledger-up.ps1' helper script

1. Go to *nledger/* folder
2. Execute: 
    - `pwsh -file ./get-nledger-up.ps1 -noNLTests`
    - On Windows, you might need to type: `powershell -ExecutionPolicy RemoteSigned` instead of `pwsh`

> Note: the switch -noNLTests disables integration tests but you can run with them.

#### Build with Visual Studio 2017 or later

1. Open NLedger solution (*Source\NLedger.sln*)
2. Build all sources (Build All)
3. Execute unit tests (Run All)

#### Build with Visual Studio Code

Note: current c# extension was not able to open multi-target project files. You may manually need to change project files:
open all *.csproj files and update `TargetFrameworks` tag. You need to remove unwanted target (e.g. net45).

The build process is similar to ".Net SDK build".

### NLedger testing framework (Integration Tests)

The original Ledger source includes a set of functional and regression tests that fairly complete cover the application functionality. 
These tests are implemented as text files containing an input data, a command and an expected result.
Since NLedger is functionally equal to Ledger, all these tests are applicable to it as well.
NLedger contributes all the original Ledger tests and a special software component (testing framework) that runs all these tests.

> We may consider these tests to be a primary validation mechanism that ensures that NLedger has no defects and works exactly as the original Ledger.

#### Running integration tests

1. Navigate to *nledger/* folder
2. Execute: 
    - `pwsh -file ./get-nledger-tools.ps1 -testConsole`
    - Remember that you might need to type `powershell -ExecutionPolicy RemoteSigned` instead of `pwsh` on Windows
3. Follow instructions

On Windows, you can also just run the file *nledger\Contrib\NLTestToolkit\NLTest.cmd*; it opens the testing toolkit console as well.

The main capabilities of the testing toolkit are:
- Commands `run`/`xrun`/`all` search all test files in *nledger/Contrib/test/* folder and run all of them;
- Adding a search criteria selects a subset of tests by matching file names. For example, *run opt* runs all tests with "opt" in the name;
- Command `run` also creates an HTML report file and opens it (if GUI is available, it runs a default browser);
    - Note: the result files are saved in the folder *MyDocuments/NLedger/* (Windows) or *~/NLedger/* (Linux, OSX)
- The toolkit looks for all available NLedger binaries. If there are binaries for both platforms (Framework and Core), it prefers Framework.
  You can change the target executable by the command `platform -core`.

#### Adding new integration tests

If you either troubleshoot an issue or developing a feature, you may want to describe an expected behavior for a specific input data.
The testing toolkit is exactly what you need for in such cases: you can write one or more test cases to cover your feature or a detected issue.

> You can refer to the original Ledger documentation (section 14.4.2 Testing Framework) for general guidelines.

All new files that you put into *nledger/Contrib/test/* are immediately available in the testing framework.

However, if you want to include the added test file to the set of *NLedger.IntegrationTests* unit tests (and run it with all other unit tests),
you need to make a couple of more steps:

1. In Visual Studio, open *NLedger.IntegrationTests* in Solution Explorer;
2. Run corresponded T4 template (TestSetBaseline.tt, TestSetManual.tt, TestSetRegress.tt, TestSetNLedger.tt);
3. Make sure that the new unit test is properly added.

If Visual Studio is not available, you may update the corresponded *.cs file manually.

### Installing and Uninstalling Binaries

In some cases, you may want to "install" the creates binaries that basically means:

- Adding a path to your binaries to PATH variable
- Creating a hard link 'ledger' to your executable binary so that you can run NLedger just by typing "ledger"
- On Windows, it also makes sense to call NGen for .Net Framework binary file to optimize and speed up it

You can perform these actions automatically by the following command:

1. Navigate to *nledger/* folder
2. Execute: 
    - `pwsh -file ./get-nledger-tools.ps1 -install`
    - Remember that you might need to type `powershell -ExecutionPolicy RemoteSigned` instead of `pwsh` on Windows
    
Notes:
- Switch `-installPreferCore` installs .Net Core binaries if you have files for both platforms (otherwise, it prefers .Net Framework)
- Switch `-installConsole` opens a console where you can troubleshoot issues with installing
- On Windows, you can also run the console by running the file *nledger\Contrib\Install\NLedger.Install.cmd*

> It is highly recommended to re-open the console after installing or uninstalling.

For uninstalling (that means reverting the described changes), you run `get-nledger-tools.ps1` with `-uninstall` switch
(or use the installation console or run *nledger\Contrib\Install\NLedger.Uninstall.cmd* on Windows).

### Packaging Build (Official Build)

This build is unlikely interesting for developers; it is designed to be run in Azure Pipeline to produce an "official" NuGet, MSI and ZIP packages.
Therefore, it has special requirements to installed pre-requisites (MSBuild, Wix) and provides a bit more functions rather than a regular development build
(validating licensing, wix structure, adding version and commit info etc). If you have all listed software (see the section Software Prerequisites above), 
you can run this build.

#### Build NLedger from Sources

There is MSBuild project file that performs all build actions; they are:

- Build binaries from sources;
- Execute internal tests (unit and in-proc integration tests that run Ledger test files);
- Create NLedger NuGet package;
- Compose NLedger release package;
- Run all Ledger tests by means of NLedger Testing Toolkit;
- Create zip package and put it to Bin folder;
- Create msi package and put it to Bin folder;

There are two ways to run the build process:

- run the build console. It provides several short commands that allow to perform
  either all build actions or just some of them;
- perform all build actions by running a batch file.

Either way you run a build, it will generate an output in the root folder in sub-folder
with name *Build-DATE-TIME*. Inside this folder you may find:

- MSBuild log file (short and detail versions);
- Unit test TRX file (a file with extension TRX);
- NLedger Testing Toolkit report (*NLTest.LastTestResults.html*);
- NLedger package (folder with name *package*);
- NLedger zip package (file with name *NLedger-vN.N.zip*); this file is also copied to Bin folder;
- NLedger msi package (file with name *NLedger-vN.N.msi*); this file is copied to Bin folder too;
- NLedger NuGet package.

There is a point about administrative privileges. The build can work in two modes: with or without 
elevated permissions. The administrative privileges are needed to generate native 
images (call NGen) before running Ledger tests. In case of elevated mode, the build 
passes all tests much faster than otherwise. Therefore, administrative privileges are not
required but they significantly speed up the overall process.

#### Build NLedger with Elevated Permissions

1. Open *Build* folder;
2. Execute *NLedgerBuild.Elevated.cmd*; confirm administrative privileges for this process;
3. Find the latest Build-DATE-TIME folder in the root folder and observe results.

#### Build NLedger without Elevated Permissions

1. Open *Build* folder;
2. Execute *NLedgerBuild.cmd*;
3. Find the latest Build-DATE-TIME folder in the root folder and observe results.

#### Build NLedger in Build Console

1. Open *Build* folder;
2. Execute *NLedgerBuild.Console.cmd*;
3. Read the prompt with the list of available options;
4. Type one of options and click Enter, e.g. *all* *ENTER*
3. Find the latest Build-DATE-TIME folder in the root folder and observe results;
6. Type *exit* to close the console.

### Manage Product Info

General product information is kept in the file *Source\ProductInfo.xml*. It has:

- Version information about the current code base;
- Information link to the original source code base;
- Rules to populate Version Info information in project files;
- Licensing information (header template that should appear on the title of every source file).

In case of any changes in this file, the updated information should be applied to source code.
You may do it automatically by executing *Build\ProductInfoUpdate.cmd*. If this script
detects any differences, it automatically applies new information; you only need to 
observe changes and check in updated files when it finishes.

*Note: MSBuild script verifies that product info is actual in the source code and fails 
if it detects any differences*. In such cases, you also need to execute *Build\ProductInfoUpdate.cmd*
and commit updated files.

(c) 2017-2020 [Dmitry Merzlyakov](mailto:dmitry.merzlyakov@gmail.com)
