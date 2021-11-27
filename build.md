# .Net Ledger Development Guidelines

> This document provides guidelines how to develop software using .Net Ledger as a library.
If you are looking for information on how to use .Net Ledger as a software application, 
please refer to [.Net Ledger Documentation](https://github.com/dmitry-merzlyakov/nledger/blob/master/nledger.md).
If you want to use .Net Ledger Python module, please, 
refer to its [Readme](https://github.com/dmitry-merzlyakov/nledger/blob/master/Source/NLedger.Extensibility.Python.Module/README.md) file.

## Introduction

.Net Ledger as a software product consists of library files that include data processing logic, and an executable file that users run as a console application.
Library files are .Net assemblies that can be linked with your software to get Ledger functionality.

Library files are distributed as NuGet packages. Package binaries are compiled for .Net Framework 4.7.2 and Standard 2.0, so they should match most target platforms.
These binaries are actually the same files that are distributed with the console application, which means they have been properly tested with all unit and integration tests.

The packages are available for download in the [NuGet](https://www.nuget.org/packages/NLedger/) repository. 
You find more information in the section `Where to get NLedger NuGet packages`.

There are several ways to import .Net Ledger functionality into your software. 
It actually depends on what and how you want to calculate with the Ledger library.
For better understanding which way is more appropriate for you, please, observe logical layers in which .Net Ledger is split. They are:

- Assembly `NLedger.dll`
  - `Core` layer contains Ledger Domain model, data processors and environmental abstractions. 
    This part is accurately ported from the original C++ application. All structures, function signatures and even original comments are kept unchanged.
    This ensures that the original logic is replicated correctly and provides an easy way to copy new updates from C++ code to a .Net library.
    The only difference is in the environment related functions, which are replaced by abstract interfaces in .Net code.
  - `Auxiliary` layer contains service classes that basically manage domain objects and implement abstractions. They cover environmental interactions,
    provide adapters for specific functionality like Date class or Boost functions, supply thread context management, control session life cycle,
    operate initialization process and manage configuration settings. In a nutshell, they provide a required minimum to run a console application.
  - `Service API` is a component that provides a programming interface for external software to use .Net Ledger functions.
    In a nutshell, it is an efficient sandbox that allows you to load data, execute one or more Ledger commands, and get the output in text format. 
    It is based on the Ledger REPL feature (one load - multiple commands) and provides exactly the same behavior.
    There are helper classes to simplify session management and asynchronous interactions. 
    In fact, it demonstrates sufficient performance and is ready for mass computing in high-load systems. 
  - `Extensibility API` provides basic abstractions for .Net Ledger extensions.
    Conceptually, this component generalizes the original Ledger extension approach used to integrate Python, so you can build integration with any software in the same way. 
    This approach specifies a two-way integration where you can extend Ledger data processing with custom functions that have access to the Ledger domain model as well as allow  external software to create its own data processing routines that work with Ledger domain objects and functions. 
  - `.Net Extensibility` implements Extensibility API for .Net platform. 
    When this extension is active, the .Net Ledger console application can load .Net assemblies and use their functions and objects. 
    External software that use Service API can enable the extension and specify its own data processing or validation functions.
    As an alternative, the external software can establish Standalone session.
    It gives them a scope where the outer code has full access to Ledger domain objects and functions: they can load data, execute queries and commands and specify own calculation code.

- Assembly `NLedger.Extensibility.Python.dll`
  - `Python Extensibility` implements Extensibility API for Python integration. 
    It provides full compatibility with the original specification for Ledger Python extension.
    In short, this means the console application and Service API clients can add Python code to data files while Python code has full access to Ledger domain objects. 
    In addition, the extension is the basis for the Python .Net Ledger module, which provides users with access to Ledger domain objects and functions in a Python interpreter session.

- Executable file `NLedger-cli.exe`
  - The .Net Ledger console applications that users can run in the command line.

Depending on your interest, you can explore one of the following options more deeply:
  - Using Service API is probably the most common option. It supports all data processing capabilities exactly as described in the documentation.
    It provides a simple usage API, hides details of session management, easy to configure, open for extensions, ready for massive async computing.
    In combination with .Net Extensibility, you can define own .Net function or objects that can change calculation logic.
    A downside that can confuse users is the fact that the result is always text output, but this is due to the way Ledger handles commands. 
  - Using .Net Extensibility and Standalone Session might be interesting if you are primarily focusing on using Ledger domain objects. 
    For example, you can get the result of a query as a collection of domain objects and use them to compute something specific to you. 
    You can use Ledger classes like Commodities, Amounts, Balances to implement a sophisticated calculation algorithm for your software.
    Ultimately, you can even build a journal file manually and execute regular Ledger commands to compose reports.
    All these scenario can be done in scope of Standalone session.
  - You might take a look at the Extensibility API if you want to create your own integration extension for .Net Ledger.
    For example, it might be an extension for another scripting platform or you might need to integrate your software in a very specific way.
  - Eventually, you can use Core and Auxiliary objects directly managing their initialization and life cycle on your own.
    This is actually possible, but requires a deep understanding of the Ledger domain model and how to deal with them. 

Note that most of the functionality is concentrated in NLedger.dll, you may need the NuGet package of the same name. 
The NLedger.Extensibility.Python NuGet package is only required if you want to enable the Python extension for data files. 
  
### Where to get NLedger NuGet packages

Primarily, .Net Ledger NuGet packages are available [here](https://www.nuget.org/packages/NLedger/) in the public NuGet repository. 
Use the `NLedger` key word for search.
They are periodically updated with every new NLedger release (starting with version 0.8).

>Note: you need to check `Include Prerelease` checkbox in NuGet Package Manager to include NLedger into search results (because it is in pre-release status yet).

If you want to get the latest CI package, you can download it [here](https://github.com/dmitry-merzlyakov/nledger/blob/next-dev/_CI.BuildLog.md). 
If you are not sure how to install a local NuGet package file, you can check the answers to this question: [How do I install a NuGet package .nupkg file locally?](https://stackoverflow.com/questions/10240029/how-do-i-install-a-nuget-package-nupkg-file-locally).

Ultimately, you can create a NuGet package from source code by means of `dotnet pack --configuration Release` command (see more about local builds below).

### .Net Ledger Domain Model

Core layer contains Ledger domain classes and data processors. 
They can be divided into the following functional groups:

- Managing commodity pools
  - `CommodityPool` is a common registry for all commodities involved into your accounting.
    It is responsible for creating and managing commodity instances, so if you need to get a commodity, you should use this class, as in the example below.
    It also manages the history of commodity prices and provides conversion between commodities.
  - `Commodity` class represents an individual commodity item. It basically contains a commodity symbol and other descriptive information.
  - `AnnotatedCommodity` is an commodity that is extended with Annotation details. 
    Each annotated commodity refers to a regular commodity but contains a unique annotation.
    CommodityPool guarantees the uniqueness of regular and annotated commodities in the pool.
  - `Annotation` is an object that annotates the commodity instance. 
    Annotation details include price value, date, custom tags and flags that define price characteristics.
  - `AnnotationKeepDetails` (also known as KeepDetails) is a helper class that defines how to transform Annotation objects.
  - `PricePoint` is a helper class that combines a date (When) and price (Price) into a single object that help managing price history.
```c#
var commodity = CommodityPool.Current.FindOrCreate('USD');
```
- Mathematical operations
  - `Amount` is the most basic numerical type in Ledger that represents infinite-precision amounts.
    It is typically annotated with a commodity that allows correctly manage mathematical operations with amounts of commodities.
    The result of mathematical operations always contain a proper commodity and precision size.
  - `Balance` is the basic type for managing amounts of multiple commodities all together.
    It properly distributes the result of mathematical operations among the amounts of involved commodities.
    Typically, it is used for building a running balance after calculations of transaction elements.
- Account hierarchy
  - `Account` represents an individual account in a hierarchical account structure.
    It has a name, notes, reference to a parent node and a collection of children.
  - `AccountXData` and `AccountXDataDetails` are holders for optional extended data for an account.
    It is primarily needed to improve data processing performance by keeping associated data like transaction posts handy.
    They are populated and cleaned up by report handlers.
- Transactions
  - `XactBase` (also known as TransactionBase) is the abstract base class for all kinds of Ledger transactions.
    It manages a collection of posts and is responsible for verifying that the transaction object is in good condition.
  - `Xact` (also known as Transaction) is regular Ledger transaction object.
    In additional to the basic properties, it contains a code and a payee.
  - `AutoXact` (also known as AutomatedTransaction) is a special kind of transaction which adds its postings 
     to other transactions any time one of that other transactions’ postings matches its predicate. 
  - `PeriodXact` (also known as PeriodicTransaction) is a special kind of transaction that is used for budgeting and forecasting.
  - `Post` (also known as Posting) represent a transaction posting.
    It is basically a central element in accounting that specifies an amount of commodity that either 
    need to be added or subtracted from a specified account in scope of current transaction.
  - `PostingXData` is a holder for optional extended data for a posting.
    It is usually populated and cleaned up by report handlers.
- Journals
  - `Journal` class represent an accounting journal.
    It manages a collection of transactions, a hierarchy of accounts and additional metadata.
  - `Item` (also known as JournalItem) represents a transactional object that can refer to a journal (posting, transaction).
  - `JournalFileInfo` (aka FileInfo) and `ItemPosition` (aka Position) are helper classes that specify a connection 
     between parsed transactional data and textual data source.
- Expression and function evaluation
  - `Expr` is a class that is responsible to parse and evaluate an expression.
  - `Value` is a dynamic type representing various numeric types.
    It is basically used for computing value expressions and context functions as well-known representations of data.
  - `Scope` is an abstract basis for all Ledger objects that can be used in function evaluation.
    It provides a generalized mechanism (Lookup) for retrieving data from object properties or executing methods. 
- Sessions
  - `Session` class defines a work session of the Ledger domain objects. It manages initialization and destroying objects;
    holds control settings and provides basic actions like reading a journal.

The special member of the Ledger Domain model is the `Main` class which replicates the function `main` from original code.
This function controls the launch of the console application and contains a specific execution flow that is not appropriate for other use cases.
The class is used by .Net console application only.

Added synonyms (also known as) might be helpful if you look for class names in Python module.

> If you want to try the Ledger domain model in action, it might be a good idea to test it by means of the .Net Ledger Python module. 
Python session allows you to create and use domain objects interactively.

### Abstractions and Auxiliary Classes

.Net Ledger Domain Model is surrounded by a set of interfaces that make up the abstraction layer of the environment.
Each interface represents a specific aspect of interaction with the external environment.
The interfaces are:
- `IFileSystemProvider` - file system abstraction
- `IVirtualConsoleProvider` - text console abstraction
- `IProcessManager` - running new processes
- `IQuoteProvider` - requesting updates for stock quotes
- `IManPageProvider` - requesting a user manual
- `IPagerProvider` - requesting output pagination

Auxiliary layer provides various classes that implement these abstractions. The most commonly used implementations and helpers are:
- IFileSystemProvider
  - `FileSystemProvider` interacts with the real file system by means of System.IO
  - `MemoryFileSystemProvider` simulates the file structure in memory
- IVirtualConsoleProvider
  - `VirtualConsoleProvider` uses .Net console input and output but can be redirected to other streams
  - `MemoryStreamManager` simulates input/output console streams in memory and can be used in combination with VirtualConsoleProvider
  - BaseAnsiTextWriter is an abstract preprocessor of VT-100 ANSI colorization codes in a stream. The implementers are:
    - `AnsiTextWriter` manages colorization using .Net console capabilities
    - `MemoryAnsiTextWriter` replaces colorization codes with html tokens and sends them to the output

### Application Context

The original Ledger Domain classes use static variables to get access to global objects like commodity pool.
To adapt this solution to the sandboxed multi-threaded model, the `MainApplicationContext` class was created. 
It solves the following tasks:
- It contains all global objects and global configuration options 
- It contains a collection of virtual provider factories 
- It exposes its instance by means of the ThreadStatic variable.
- It ensures the consistency in the scope of current thread (only one instance of the context class can be active in the current thread) 
- It enables multi-threading by sharing a context instance across multiple threads. 

The context class is designed to be in two states: attached and detached.
- If a new context is created, it is detached. It means that it is not available for Ledger Domain objects
- In order to attach a context, the `AcquireCurrentThread` method should be called. It returns an IDisposable object that defines the scope in which the context is active.
  At this point, the context ensures that there are no other active context objects and the thread can be acquired.
  Ledger Domain objects can work.
- When the scope object is released, the context is detached and is no longer available to domain objects.
- When you need to share the context, you should `Clone` it and use the copy in another thread.

> Important: Ledger Domain object can only function when the MainApplicationContext acquires the current thread.

The following Ledger classes handle acquiring the current thread by their own:
- `Main` - when it launches the console application and runs data processing
- Service API - `ServiceSession` and `BaseServiceResponse` classes activate the context in their threads
- Extensibility API - `ExtendedSession` class acquires the current thread when it creates a Standalone session.

If you intend to use Ledger Domain objects in other ways, you should manage acquiring the current thread yourself. 

### .Net Ledger Service API

The `Service API` is a component that allows you to run .Net Ledger commands inside your application. 
It gives the same functionality as the console application, that is, it loads input data in Ledger text format, executes Ledger commands, and produces text output. 
The component provides the necessary virtualization to interact with the core Ledger layer in an isolated sandbox;
supplies efficient data processing in a multi-threading environment and exposes helper functions such as pre-processing ANSI codes.

It is important to understand that this api is for executing Ledger commands and processing data. 
By itself, It does **not** provide access Ledger domain objects.
This task is for extensions that are well supported by the Service API.
For example, you can customize data processing using .Net or Python functions via the appropriate extensions, which in turn can interact with Ledger domain objects. 

Specifically, .Net Ledger Service API covers the following tasks:
- Virtualization: provide a collection of implementers that allow to run NLedger in a sandbox, for example:
    - `FileSystemProvider` interacts with System.IO (the real file system); `MemoryFileSystemProvider` simulates the file structure in memory.
    - `MemoryStreamManager` manages input/output console streams in memory.
- Pre-processing VT-100 ANSI colorization code:
    - `BaseAnsiTextWriter` extracts ANSI code from the stream.
    - `MemoryAnsiTextWriter` can put corresponded html tokens to the output.
- Providing a common service model:
    - The classes `ServiceEngine`, `ServiceSession`, `ServiceResponse` provide a simple way to call any NLedger functions.
    - They are thread safe, cancellable, and provide a good level of isolation. 
- Flexible configuration and extensibility:
    - All components for the virtual environment can be configured or re-configured.
    - All abstractions can be re-implemented as needed. 

The component is located in the namespace `NLedger.Utility.ServiceAPI` and contains three main classes:

- `ServiceEngine` is a factory that creates service session objects; it holds a configuration that represents a virtual NLedger environment;
- `ServiceSession` encapsulates reading and building Journal object from data file(s) or an input stream. Once Journal is built, the session is ready to serve requests;
- `ServiceResponse` contains the result for an individual request, e.g. it can return a balance or registry table.

So, when you need to execute a Ledger command through the Service API, your code should do:

- Create ServiceEngine object and optionally configure it. 
  Typically, you only need one ServiceEngine per application (you may need to create more if you have different configurations for virtual environments) 
- Call ServiceEngine to create ServiceSession object. 
  Here you only need to specify the data source (either the filenames or just plain text input).
  When a ServiceSession is created, it reads the input, creates a journal (and all other ledger data), and prepares everything to be ready to serve requests (ledger commands). 
- Call ServiceSession to execute the command and create a ServiceResponse object. 
  At this point, it executes the command and populates the ServiceResponse with output and contextual information. 

Here is an example of getting the balance for the input journal:
```c#
using NLedger.Utility.ServiceAPI;

var InputText = "2009/10/29 (XFER) Panera Bread"  // Complete input text...

var engine = new ServiceEngine();
var session = engine.CreateSession("-f /dev/stdin", InputText);
var response = session.ExecuteCommand("bal checking --account=code");

System.Console.WriteLine (response.OutputText);   // Print balance...
```

API supports async methods, so this code can be used to read data and execute command asynchronously:

```c#
private async Task<ServiceResponse> GetBalance(ServiceEngine serviceEngine, string fileName)
{
   var session = await serviceEngine.CreateSessionAsync($"-f {fileName}");
   var response = await session.ExecuteCommandAsync("bal");
   return response;
}
```

By default, the API disables ANSI coloring (sets IsAtty = false) to strip ANSI code from the output text. 
You can change this behavior and replace ANSI code with HTML tags.
This example shows how to configure the ServiceEngine to use a MemoryAnsiTextWriter for this purpose.

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

Input data can be specified as input text or file name. 
By default, the API uses the real file system, so in the latter case it will look for a physical file. 
You can configure a virtual file system and put the input in memory:

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

If you need to specify environment variables, you can do it like this:

```c#
var engine = new ServiceEngine(
      configureContext: context => 
         {
            context.SetEnvironmentVariables(new Dictionary<string, string>() { { "COLUMNS", "120" } });
         });
```

Service API fully supports the extensions feature, so you can add custom functions that interact with Ledger Domain model.
To do this, you should specify an extension provider factory in ServiceEngine configuration.
The example below shows how to set up a .Net extension that uses a custom Average calculator.
Notice how the provider declares the custom functions `averageCalculator` and `calculatedAverage` to make them available to the Ledger runtime. 

```c#
var testAverageCalculator = new TestAverageCalculator();

var engine = new ServiceEngine(
      createCustomProvider: mem =>
      {
          return new ApplicationServiceProvider(
              virtualConsoleProviderFactory: () => new VirtualConsoleProvider(mem.ConsoleInput, mem.ConsoleOutput, mem.ConsoleError),
              extensionProviderFactory: () => new NetExtensionProvider(
                          configureAction: extendedSession =>
                          {
                              extendedSession.DefineGlobal("averageCalculator", (Func<Xact, Amount, bool>)((x, a) => testAverageCalculator.Process(x, a)));
                              extendedSession.DefineGlobal("calculatedAverage", (Func<Amount>)(() => testAverageCalculator.Average));
                          }
                    ));
      });
```

After configuring the extension, you can use custom functions in Ledger data files:

```console
= expr 'averageCalculator(xact,amount)'
  [$account:AVERAGE]  ( calculatedAverage )
  [AVERAGE]  ( -calculatedAverage )
```

It is also possible to define global objects or even simple values. They will be available by their global names:

```c#
return new ApplicationServiceProvider(
    virtualConsoleProviderFactory: () => new VirtualConsoleProvider(mem.ConsoleInput, mem.ConsoleOutput, mem.ConsoleError),
    extensionProviderFactory: () => new NetExtensionProvider(
            configureAction: extendedSession =>
            {
                extendedSession.DefineGlobal("customValue", "test/baseline/feat-import_py.test");
            }
        ));
```

Service API supports any extension providers. For example, you can set up a Python extension in the same way:

```c#
var engine = new ServiceEngine(
    createCustomProvider: mem =>
    {
        return new ApplicationServiceProvider(
            virtualConsoleProviderFactory: () => new VirtualConsoleProvider(mem.ConsoleInput, mem.ConsoleOutput, mem.ConsoleError),
            extensionProviderFactory: () => new PythonExtensionProvider());
    });
```

This allows adding Python functions to Ledger data files. 
Of course, they can also interact with Ledger Domain objects:

```console
python
    import os
    def check_path(path):
        return path=='expected_path'

tag PATH
    check check_path(value)

2012-02-29 KFC
    ; PATH: expected_path
    Expenses:Food                $20
    Assets:Cash
```

### Extensibility API

Extensibility API provides basic abstractions to create extensions for .Net Ledger. 
By implementing abstract classes, you can develop your own integration component that satisfies the general requirements:
- It can work with the console application
- It can work with Service API
- It supports a standalone session scenario

> This topic might be of interest to people who need to implement a specific integration scenario, such as adding Lua scripting support to Ledger data files.
In most cases, existing .Net or Python extensions should suit your needs.

Extensibility API covers the following aspects:
- It defines how to build your own integration with .Net Ledger. 
- It ensures that if you implement abstract members correctly, your component will be properly integrated into the .Net Ledger runtime. 
- It manages basic interop activities with .Net Ledger runtime so it does not complicate your code.
- It manages the basic operations of interacting with the .Net Ledger runtime, so it doesn't complicate your code. 
- It provides the concept of a standalone session that defines the scope in which .Net Ledger domain objects work correctly in your code. 
- It provides helper functions that simplify using of domain objects.

Basically, you need to implement the following members to create your own integration:
- `ExtendedSession` represents a basic Session object with extended integration capabilities. 
  You need to inherit this class and make the following changes:
  - Implement `Initialize` and `IsInitialized` methods to match details of the initialization process on your platform.
  - Implement `DefineGlobal` method to manage an own collection of global objects.
  - Implement `ImportOption` method. It will be called if you add `import` directive to your data file.
  - Implement `Eval` method. It will be called if you add `python` directive to your data file.
  - Implement `LookupFunction` method. It is called when .Net Ledger runtime evaluates an expression and resolves a function or operand name.
    You may also consider overriding `Lookup` method if you need more control over the lookup process.
  - Override `PythonCommand` method if you want to manage Ledger `python` command line option.
  - Override `ServerCommand` method if you want to manage Ledger `server` command line option.
  - Create an own static method to expose the basic `CreateStandaloneSession`. 
    It is required if you want to provide Standalone session capabilities for your integration.
- You should `IExtensionProvider` interface that presents a factory for your extended session.
  If you have configuration settings specific to your integration, you can put them here.

You can leave some of the implemented methods empty if you are not going to manage a specific case.
You may also look at .Net and Python extension code for a better understanding of how a particular method should be implemented.

Note that .Net Ledger console application must be recompiled if you want to support your extension in it. 
It is not needed if you want to use your extension for Service API or Standalone session.

Another common limitation is that you cannot combine different extensions in the same session.
You may inherit an existing extension and add your own functionality if you need such a solution. 

### .Net Extension

.Net Extension implements Extensibility API for .Net platform.
First of all, the extension allows you to load .Net assemblies and use functions from them in Ledger data files. 
This method is available for the console application and for the Service API. 

If you use the extension with the Service API, you have more options to interact with your code.
You can directly manipulate the list of global objects, fill it with functions and values, and compose complex calculation algorithms. 

> Remember that your code must be thread safe if you are using it with the Service API. This is especially necessary if you are changing the state of global objects.  

A completely alternative way of working with the .Net Extension is to use a standalone session. 
With the Service API, you inject your code into the Ledger data processing as functions. 
The Standalone session allows you to create a limited scope in your code where you can freely use any of the Ledger Domain objects and functions.
This way is similar to extending your code with Ledger capabilities, rather than extending Ledger with your own functions.

The example below shows how to use Ledger domain objects in a Standalone session: 

```c#
var sb = new StringBuilder();

using(var session = NetSession.CreateStandaloneSession())
{
    var eur = CommodityPool.Current.FindOrCreate("EUR");

    var totalEur = new Amount("0.00 EUR");
    var totalGbp = new Amount("0.00 GBP");
    var total = new Amount("0.00 EUR");

    foreach(var post in session.ReadJournalFromString(NetSession_IntegrationTest6_Input).Query("^income:"))
    {
        sb.AppendLine($"{post.Amount}");
        sb.AppendLine($"{post.Amount.Commodity.Symbol}");

        if (post.Amount.Commodity.ToString() == "EUR")
            totalEur += post.Amount;
        else if (post.Amount.Commodity.ToString() == "GBP")
            totalGbp += post.Amount;

        var a = post.Amount.Value(default(DateTime), eur);
        if((bool)a)
        {
            sb.AppendLine($"Total is presently: ({total})");
            sb.AppendLine($"Converted to EUR:   ({a})");
            total += a;
            sb.AppendLine($"Total is now:       ({total})");
        }
        else
        {
            sb.AppendLine($"Cannot convert '{post.Amount}'");
        }
        sb.AppendLine();
    }

    sb.AppendLine($"{total}");
}
```

The following helper classes can be helpful when interacting with the Ledger Domain model in a standalone session:
- `NetSession.CreateStandaloneSession` - creates a standalone session
- `Session.ReadJournalFromString` - loads a Ledger data file from a string
- `JournalExtensions.Query` - queries posting objects from the current journal
- `SessionExtensions.ExecuteCommand` - executes a command and returns the result in text format

### Troubleshooting

If you have an unexpected calculation result, the first thing you can do is check how your example works in NLedger or Ledger applications. 
Some of Ledger's features can be a little tricky, so it's a good idea to check if your case is performing as you'd expect from the original application. 
The best way to do this is to create a test file (see Testing Framework capabilities).
You can use this file in a discussion with the Ledger community to decide if your case is a defect or a feature. 

Otherwise, if you see that API works differently rather than the application, please debug NLedger source code or create a [bug](https://github.com/dmitry-merzlyakov/nledger/issues).

## Maintaining NLedger

You may need to troubleshoot NLedger or build it from source. This section describes how to develop NLedger. 

### Software Prerequisites

Here is a complete list of recommended software components for NLedger development. 
Some of them may not be available on your operating system or are not needed for your purposes. 

- .Net Core SDK 3.1 or later
- .Net Framework 4.7.2 or later (if you want to build NLedger for .Net Framework)
- Visual Studio 2017 or Visual Studio Code with c# extension
- Powershell (7.0 is recommended though 5.0 can work properly on Windows)
- Git command line tool (only if you prefer command line)
- MS Build 15.0 and [Wix ToolSet](http://wixtoolset.org/releases) 3.12 or higher (only if you want to run an "official" build with MSBuild)
- Python 64-bit 3.6 or later with Pip installed (only if you want to run Python-related unit or integration tests)
  - Wheels module should be installed (If you want to build .Net Ledger Python module)
  - PythonNet 2.5.x or 3.x module should be installed (If you want to test .Net Ledger Python module)

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
   - NLedger Python Extension unit tests (NLedger.Extensibility.Python.Tests - if Python connection is configured);

If you want to build MSI and ZIP packages, you should go to Packaging Build section.

> Note: both builds produce equal binaries with one distinction: development build does not populate Patch and Build version numbers and does not set SourceRevisionId value.

There are several options; you may follow any of listed below.

#### Preconditions

The presence of Python connection determines whether Python-related features are enabled for the build.
If the connection is not configured: 
 - all Python-related unit and integration tests are ignored
 - .Net Ledger Python module is not built and tested
It does not prevent the build but you might see warnings indicating that some features are skipped.
This does not block the build, but you may see warnings that some features are missing. 

If you want to enable Python connection:

1. Go to *nledger/* folder
2. Execute: 
    - `pwsh -file ./get-nledger-tools.ps1 -pythonConnect`
    - On Windows, you might need to type: `powershell -ExecutionPolicy RemoteSigned` instead of `pwsh`

If you have any issues, see more information in `Manage Python Integration` section [here](https://github.com/dmitry-merzlyakov/nledger/blob/master/nledger.md).

#### .Net SDK build

1. Go to *nledger/Source/* folder
2. Execute `dotnet build --configuration Release`
    - If you want to build Core binaries only, the command is: `dotnet build --configuration Release /p:CoreOnly=True`
    - For OSX, you also need to add a switch `-r osx-x64`
3. Execute `dotnet test --configuration Release`
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

(c) 2017-2021 [Dmitry Merzlyakov](mailto:dmitry.merzlyakov@gmail.com)
