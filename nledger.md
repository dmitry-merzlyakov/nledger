## .Net Ledger Documentation

> The document describes how to use .Net Ledger as a software product. If you look for development guidelines, please, check [Development Notes](https://github.com/dmitry-merzlyakov/nledger/blob/master/build.md). If you want to use .Net Ledger Python module, please, refer to its [Readme](https://github.com/dmitry-merzlyakov/nledger/blob/master/Source/NLedger.Extensibility.Python.Module/README.md) file.

.Net Ledger (NLedger) is a .Net port of Ledger accounting system (www.ledger-cli.org).
You can refer to the [original documentation](https://www.ledger-cli.org/docs.html) to get 
general guidelines how to use Ledger; this documentation is completely applicable to NLedger as well.

This document focuses more on the specificity of the .Net Ledger product. In particular,
it describes the installation process, specific configuration options and how to use the test framework.

## System Requirements

### Basic Requirements

NLedger is a .Net console application, so basic requirements are minimal; you have to have either .Net Framework or .Net Core installed on your machine:

- [.Net Framework 4.7.2 or later](https://dotnet.microsoft.com/download/dotnet-framework) (for .Net Framework version of NLedger)
- [.Net Core SDK or Runtime 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1) (for .Net Core version)

Your choice depends on which host operation system you have. For Linux ans OSX, you should have .Net Core SDK. For Windows,
.Net Framework is preferable if you want to install an MSI package; the ZIP package can work with both frameworks.

NLedger deployment includes a set of Powershell helper scripts for configuring the product, testing, running a demo and managing environment settings.
They are not required but are generally recommended. Therefore:

- [Optionally] Powershell (you can find a latest version available for your host operation system [here](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell?view=powershell-7))

### Python Integration

.Net Ledger Python integration requires a local Python deployment, so if you want to enable this feature, you need to have Python installed on your machine:

- Python 3.6 (64-bit) or later (you can find the latest version for Windows [here](https://www.python.org/downloads/windows/) or follow Python installation recommendations for your OS)
  - The requirement for 64-bit Python stems from the assumption that the .Net Ledger is running as a 64-bit process. This is the correct assumption for the vast majority of modern environments. However, if your .Net Ledger is running as a 32-bit process for some reason, you should have 32-bit Python.

If you are going to use Python Toolset script (this is the recommended approach for setting up Python integration), you should also have the following Python modules installed:
- `Pip` is the [package installer](https://pypi.org/project/pip/) for Python. You can find installation instruction [here](https://pip.pypa.io/en/stable/installation/). I personally prefer using [get-pip-py](https://pip.pypa.io/en/stable/installation/#get-pip-py)
   - Use the command `python -m pip --version` to check whether Pip is installed
- `find-libpython` module is needed for Python ToolSet to find LibPython location. It will be installed automatically.

If you are going to build .Net Ledger from source code with enabled Python features, you should have everything listed above and a few more modules:
- `Wheel` module is needed to build .Net Ledger Python module
- `PythonNet` module is needed for testing .Net Ledger Python module after build
  - If you have issues with installing PythonNet, please check Troubleshooting section in the module [Readme](https://github.com/dmitry-merzlyakov/nledger/blob/master/Source/NLedger.Extensibility.Python.Module/README.md) file.

> Windows users can relay on the *Python ToolSet* helper script; it may automatically install and configure the embedded version of Python for the .Net Ledger.

### Build from source code

If you want to build NLedger from source code, you also need for [Git](https://git-scm.com/downloads) command line tool.

## Installation

There are three options for installing NLedger:
- For Windows, Linux and OSX: build from source code (.Net SDK build)
- For Windows: install MSI package
- For Windows: download pre-built binaries (ZIP package)

### Build and install NLedger from source code

This option works on Windows, Linux and OSX. It includes:
- Downloading NLedger source code
- Configuring Python connection
- Build binaries
- Passing unit and integration tests
- Installing binaries (adding to PATH variable and creating 'ledger' alias)

Prerequisites: .Net Core SDK 3.1, Powershell, Python 3.6, Git command line tool

#### Installing (Windows, Linux and OSX)

1. Open command line (or terminal) window
2. On Windows, navigate to a folder that is appropriate to contain NLedger binary files. On Linux and OSX, your home folder should be acceptable
3. Execute the following commands:

```
git clone https://github.com/dmitry-merzlyakov/nledger
cd nledger
pwsh -file ./nledger/get-nledger-tools.ps1 -pythonConnect
pwsh -file ./nledger/get-nledger-up.ps1 -install
```

On Windows, depending on your Powershell version, the last commands might be:

```
PowerShell -ExecutionPolicy RemoteSigned -File ./get-nledger-tools.ps1 -pythonConnect
powershell -ExecutionPolicy RemoteSigned -File ./get-nledger-up.ps1 -install
```

Remarks:
- Setting up the Python connection (the command *./get-nledger-tools.ps1 -pythonConnect*) enables Python-related unit and integration tests, and also triggers the creation of .Net Ledger Python module. If you are not interested in Python integration, you can skip this step
- If Python connection is not configured, build warnings will be displayed indicating skipped steps. You can suppress them by running the build with `noPython` flag:
```
pwsh -file ./nledger/get-nledger-up.ps1 -install -noPython
```
- Building Python Module requires "pip", "wheel" and, optionally, "pythonnet" modules to be installed in Python. See more information in `Manage Python Integration` section

#### Uninstalling NLedger

Uninstalling includes removing NLedger from PATH variable and deleting a short alias. It does not remove `nledger` folder with source code and binary files; if you should do it yourself.

Open a terminal window, navigate to the folder containing NLedger and execute:
```
pwsh -file ./get-nledger-tools.ps1 -uninstall
```
Remember that you might need to type `powershell -ExecutionPolicy RemoteSigned` instead of `pwsh` for old Powershell versions on Windows.

### Installing from MSI package

You can get the latest MSI package by this [link](https://github.com/dmitry-merzlyakov/nledger/releases).

NLedger installer is a regular Microsoft Windows MSI package. It does:

- Managing installed components. Besides binary files that are always required, you can make a decision whether to install documentation, 
  testing framework with test files, interactive Live Demo console, user setting manager and other helper scripts;

  - "Typical" option installs everything besides the testing framework and helper scripts;
  - "Complete" option install everything;
  - "Custom" option allows you to select components and change the installation folder.

- Performs initial registering of copied binaries:

  - Creates "ledger" alias (a hard link to "NLedger-cli.exe");
  - Adds the path to the installation folder to PATH environment variable;
  - Calls NGen to speed up application binaries;

- Modifies the deployment. You can change the list of installed components;

- Upgrades the installed version;

- Uninstalls NLedger and removes its settings and other files.

#### Installing NLedger

Installing NLedger from an installation package is pretty easy; your steps are:

- Check prerequisites. You must have .Net Framework 4.7.2 or later; it is absolutely required.
  If you want to use any component that requires Powershell, check, please, that you have Powershell 4.5 or higher:

  - In command line window, type *powershell* and once it shows its prompt, type *$PSVersionTable*. The field *PSVersion* contains its version;

- Run the installer and follow the wizard.

#### Uninstalling NLedger

You can uninstall NLedger in usual way: either run the installer and select the option "Remove" or open "Add or Remove Programs", find NLedger and select "Uninstall" option.

*Note: you may be asked whether to remove user settings and other files that were added after initial installation. Please, carefully check the list of folders that are going to be removed 
before confirming deletion.*

### Installing from ZIP package

You can get the latest ZIP package by this [link](https://github.com/dmitry-merzlyakov/nledger/releases).

If you do not want to run NLedger Installer (MSI package) for some reason, you can get binaries from a ZIP package. The package contains binary files for Windows, so this option works on this operation system only.

Prerequisites: .Net Framework 4.7.2 or .Net Core SDK 3.1 (the package contains binaries for both frameworks)

Basically, NLedger binaries are immediately ready for using once they are unpacked.
However, there are three additional recommended steps that make your work with NLedger more comfortable:

1. It is recommended to create native images for NLedger binaries by calling NGen. 
   Native images contain very efficient code that speeds up NLedger several times;
2. It is recommended to add the path to NLedger binaries to PATH environment variable.
   It allows you omit path to NLedger in the command line;
3. You might find it useful to create an short alias to NLedger command line utility.
   It is easier to type "ledger" instead of "nledger.cli" every time you call it.
   The script creates a hard link with name "ledger.exe" for it.

Installation scripts perform these actions automatically. They can also
remove changes if you decide to uninstall NLedger.

*Note: calling NGen and making changes in PATH require administrative privileges.
The script will request elevated privileges when you run it.*

#### Installing NLedger

The steps to install NLedger are:

1. Download and unpack NLedger zip package;
2. Open unpacked files; move *NLedger* folder to any appropriate place (e.g. *"C:\Program Files"*);
3. Open *NLedger\Install* folder;
4. Execute *NLedger.Install.cmd* command file; confirm requested elevated permissions;
5. In the console window:
    - Type `install` if you want to install .Net Framework version of NLedger
    - Type `install -core` if you want to install .Net Core version of NLedger
5. Observe the log of installation actions in the console and close it.

Now NLedger is ready for using. For example, open new Windows Command Prompt and type *ledger*:
the standard prompt should appear. 

#### Uninstalling NLedger

If you decide to remove NLedger from the system, perform the steps:

1. Open *NLedger\Install* folder;
2. Execute *NLedger.Uninstall.cmd* command file; confirm requested elevated permissions;
3. Observe the log of uninstalling actions in the console and close it;
4. Delete the folder *NLedger* (of course, make sure that you do not have your own files in this folder).

## Using NLedger

Once NLedger is installed, it is available in any command line or terminal window by typing *ledger* 
(or *NLedger-cli* in case the short alias has not been created). 

As it was mentioned above, NLedger is completely compatible with Ledger, so it is recommended
to be familiar with the [Ledger documentation](https://www.ledger-cli.org/docs.html), instructions and [guidelines](http://plaintextaccounting.org/). Ledger community provides
huge amount of good examples, best practices and recommendations how to deal with command line 
accounting systems.

*Note: the example journal file (drewr3.dat) and some other example files that are mentioned in 
the documentation are available in the folder with Ledger tests (nledger/test/input).*

### Enable Python Integration

Python integration is disabled by default, so you will get error messages like *Error: 'python' directive seen, but Python support is missing* if you try to use this feature.

In order to enable the integration, you should run the Python ToolSet console and execute `enable` command:

1. Click on the '.Net Ledger Python Toolkit' menu shortcut or run the file [Install Folder]/Contrib/Python/GetPythonEnvironment.Console.cmd
   - Similar command for Linux and OSX users is:

```console
$pwsh -file ./get-nledger-tools.ps1 -pythonTools
```

2. Type `enable` in the console

This is basically enough; the integration becomes available once that command finishes successfully. Corresponding commands in data files are now processed correctly:

```console
C:\NLedger>NLedger-cli.exe -f feat-value_py3.test reg
<class 'bool'> True
<class 'datetime.date'> 2010-08-10
<class 'ledger.Amount'> 10
```
You can find more information about Python integration settings (including troubleshooting recommendations) in `Manage Python Integration` section.

In order to disable integration, you should run the same console and execute `disable` command.

> Hint: refer to Ledger documentation to find more examples how to use Python in Ledger data files. You can also check integration tests with "py" in names.

### Enable .Net Integration

Similarly to Python integration, .Net Ledger also supports integration with .Net code.
When .Net Integration is enabled, you can specify which assembly to load and which function to use in data files.

The example below illustrates how to call .Net function File.Exists() from a data file. You can find more examples in the folder with NLedger tests (nledger/test/nledger; files nl-baseline-net-*.test).

```
import assemblies

tag PATH
    check System.IO.File.Exists(value)

2012-02-29 KFC
    ; PATH: test/baseline/feat-import_py.test
    Expenses:Food                $20
    Assets:Cash
```

There are three logical steps to make .Net Integration work:

1. Enable .Net Integration provider: set NLedger configuration setting `ExtensionProvider` to `dotnet`
2. Import .Net assembly that contains the function you want to call
3. Use either a fully-qualified path to this function or its alias in Ledger expressions

#### Enable .Net Integration provider

NLedger configuration setting ExtensionProvider specifies which extension will be used. If it is empty, all related directives in data files are inactive; 
attempts to use them will lead to errors like *directive seen, but ... support is missing*. 

For .Net integration, you should set this setting to `dotnet`. The simplest way to do so is to use Setup Console:
- Run [Install Folder]/Contrib/NLManagement/NLSetup.Console.cmd (or corresponded menu shortcut if you installed MSI)
- Type *set-setting ExtensionProvider dotnet -user*

```console
NLedger Setup>set-setting ExtensionProvider dotnet -user
[OK] Setting 'ExtensionProvider' is set to 'dotnet' (Scope 'user')
```

> Hint: you can use the command *show-details ExtensionProvider -allValues* to check whether the setting is properly set

If you want to disable .Net integration, set an empty value to this setting:

```console
NLedger Setup>remove-setting ExtensionProvider
[OK] Setting 'ExtensionProvider' is removed (Scope 'user')
```

#### Import .Net Assembly

When .Net extension provider is enabled, you can use "import" directive to load an assembly. The supported options are:

- `import assemblies` makes all functions in currently loaded assemblies available by FQN (fully qualified name) path. For example, you can call File.Exists() by its path System.IO.File.Exists()
- `import assembly [Assembly Name]` loads the specified assembly and makes all its functions available by FQN path. The assembly file should be available by assembly binding path.
- `import assembly [Assembly File]` loads the specified assembly file and makes all its functions available by FQN path. You can use related or absolute file name.
- `import alias [Alias Name] for [FQN function path]` makes the function specified by FQN path available by a short alias

The example below illustrates the use of short aliases:

```console
import assemblies
import alias check_path for System.IO.File.Exists

tag PATH
    check check_path(value)
```

#### Use .Net Function in expressions

You can use .Net functions from imported assemblies by their FQN (fully qualified name) path. It should include a namespace, a class name, and a static member (method or property). In case the static member is a property, you can provide a further path to navigate through the object tree to the function you want to use. 

If you need to send some arguments to the function, you may specify them in the data file. NLedger will find argument values by their names in the current call context. Basically, whatever Ledger allows to use in expressions, can be used as arguments for .Net functions. For example, this is a legal expression declaration that calls a .Net function (arguments xact and amount will be properly found in the transaction context):

```console
= expr 'Sample.AverageCalculator.Calculate(xact,amount)'
  [$account:AVERAGE]  ( Sample.AverageCalculator.Result() )
  [AVERAGE]  ( -Sample.AverageCalculator.Result() )
```

When all argument values are evaluated, NLedger looks for a function with the specified name and argument list that best matches the input value types. Binding rules are:
- It prefers a function with the widest list of arguments that fit input value types
- If takes into account function arguments with default values (in case the list of input values is shorter)
- It takes into account possible implicit value casting (for example, long to integer)

Function result is converted into Value object so that NLedger seamlessly uses it for further calculation. .Net functions have full access to all NLedger domain classes and context objects (via MainApplicationContext.Current), so the function for the example above might look like (notice how it works with Ledger classes and objects):

```c#
using NLedger;
...
namespace Sample
{
  public static class AverageCalculator
  {
    public static bool Calculate(Xact x, Amount a) => (Cache[MainApplicationContext.Current] = MyCalculationLogic(x, a, CommodityPool.Current.Find("QFE"))) > 0;
    public static Amount Result() => Cache[MainApplicationContext.Current];
    ...
    private static Amount MyCalculationLogic(Xact x, Amount a, Commodity c)
    {
      NLedger.Utils.Logger.Current.Info(() => "My calculation is starting...")
      ...
    }
    ...
    private static readonly IDictionary<MainApplicationContext,Amount> Cache = new Dictionary<MainApplicationContext,Amount>();
  }
  ...
}
```
> More information about how to develop with NLedger domain model you can find in [Development Notes](https://github.com/dmitry-merzlyakov/nledger/blob/master/build.md).

### Setup Console

Basically, NLedger settings are available in its configuration file (NLedger-cli.exe.config for .Net Framework or NLedger-cli.dll.config for .Net Core); you can manually change them anytime. However, this approach is not generally recommended by two reasons:

- If you make changes manually, you need strictly know the syntax of settings and available values;
- For Windows, if is not a good approach to make any manual changes in *Program Files* folder.

NLedger provides an alternative way to specify user settings on a machine without changing Program Files content:

- It can read user settings from extra optional files that represent Common (for any user) and User (for an individual user) settings. 
  They have the same format as the main config file;
- It provides a helping test console that allows you to manage NLedger settings on your machine:

  - It shows help instructions;
  - It shows current options and their effective values (having in mind that the options might be overridden);
  - It allows to set an option value for both Common and User scope (and even for the app config).

On Windows, you can run the tool by executing *NLedger/Contrib/NLManagement/NLSetup.Console.cmd*. Type "help" in the console for further information. Typically, you will use *show*, *set-setting*.

If you build NLedger from the sources, you can run the tool by the command:
```
pwsh -file ./get-nledger-tools.ps1 -settings
```

*Note: this tool requires Powershell*

### Live Demo Web Console

In a nutshell, it is a web page that shows the original Ledger documentation
but also allows to run all its examples in an interactive manner. You just select an action on any example and the console runs NLedger with all necessary parameters, 
so that you see the result in a command line console popup.

You can also repeat the command, change command line arguments to see the result, change the file and run the command again. If you did something wrong in the file, 
you can revert your changes and continue experimenting with it. So, it is some kind of a playground that let you learn NLedger features in easy and efficient way.

On Windows, you can run the console by executing *NLedger\Contrib\NLManagement\NLDoc.LiveDemo.WebConsole.cmd*.

If you build NLedger from the sources, you can run the tool by the command:
```
pwsh -file ./get-nledger-tools.ps1 -demo
```

Technically, it will run a powershell tool that starts http listener and runs your default browser.
If you have any troubles with default HTTP settings (e.g. port number for HTTP listener) or you want to use another editor or browser - use Setup Console to customize Live Demo settings.

*Note: this tool requires Powershell*

### About Coloring and Pagination

The original Ledger colors the output by means of VT100 color codes. Since the standard Windows console
does not support them, NLedger manages coloring on its own by direct calling of console API functions
(see *AnsiTerminalEmulation* setting that is turned on by default).

If you prefer using a specific terminal application with full VT100 support
(for example, [ANSICON](https://github.com/adoxa/ansicon/downloads)), you may want
to turn the embedded coloring off by setting *AnsiTerminalEmulation* to False.

By default, NLedger produces the output without pagination. The default Windows console
allows scrolling and searching (by Ctrl-F); it may be sufficient in most cases. However,
if you have a favorite pager with wider capabilities (for example, [Less for Windows](http://gnuwin32.sourceforge.net/packages/less.htm)),
you can integrate it with NLedger. You can either set it as a default pager 
in the configuration file (see *DefaultPager*) or you can use an input parameter *pager*.

*Note: if your pager does not support VT100 codes, you may either run NLedger
in a specific terminal application that supports VT100 or you can disable coloring 
at all to get rid of artifacts in the output text.*

## NLedger Testing Toolkit

Like the original application, NLedger has a special testing framework that executes Ledger-style test files
and verifies that they pass well. It consists of two parts:
- the folder *NLTestToolkit* has the testing runtime (PowerShell scripts that run tests);
- the folder *test* that contain the original set of Ledger test files (baseline, input, manual and regress).
 
Main testing toolkit features are:

- of course, **run test files**. The toolkit reads a test file, runs NLedger with test parameters and validates 
  the output in the same manner as the original testing toolkit does;
- **select tests to execute**. By default, the toolkit runs all test files that are in *test* sub-folder. 
  The user can select a subset of files by typing a search criteria (regex); the toolkit will only run the tests
  with matched file names;
- **display results**. Besides showing test results in the console, the toolkit can also generate report files
  with detail information in HTML or XML formats;
- manage **list of test files to ignore**. Some Ledger tests are not applicable to Windows environment, so we need
  to skip them every time. The toolkit reads this list and do not execute these tests;
- provide an **easy way to communicate with the user**. The toolkit provides a special console with several
  easy commands. It allows you perform any kind of testing actions just by typing a couple of letters.

*Note: this tool requires Powershell*

### Running Tests

On Windows, you can open NLedger Testing Framework console by clicking on *NLTestToolkit\NLTest.cmd*. 

If you build NLedger from the sources, you can run the tool by the command:
```
pwsh -file ./get-nledger-tools.ps1 -testConsole
```

The prompt will show you available commands and other recommendations. For example, simply type *run* and click *Enter* to execute 
all test files that you have in *test* folder.

*Note: typical time to execute all tests is about 1 minute in case you created native images and about 5 minutes
otherwise.*

### Creating Tests

You can create own test files according to recommendations in Ledger documentation. Created file with .test extension
should be put into *test* folder (or any its sub-folder). The testing toolkit re-scans the content of the test folder
every time so your file is immediately available.

If you already have your own set of test files, you can put them to the test folder too.

## Manage Python Integration

.Net Ledger Python integration implements the original Ledger Python extension concept. There are two targets:
- Extending data files with additional directives and commands that allow users to create their own processing rules using Python code.
  In particular, the `import` and `python` directives enable importing or creating Python functions that you can later enter into Ledger expressions.
  The commands `python` and `server` let you even run a Python script with custom data processing logic.
  This is a Ledger-host integration when the Ledger application is responsible to run Python code.
- Providing access to Ledger object model and running Ledger functions from Python interpreter session.
  This is a Python-host integration when Python process calls Ledger domain objects from its own session. This feature is implemented by means of .Net Ledger Python module.

This section described details of Ledger-host integration - basically, how to run Python code inside .Net Ledger. If you need information about .Net Ledger Python module, 
that is, how access Ledger data in Python session - please, refer to the module's [Readme](https://github.com/dmitry-merzlyakov/nledger/blob/master/Source/NLedger.Extensibility.Python.Module/README.md) file.

### Technical Design

.Net Ledger Python integration is implemented as a software component (the assembly NLedger.Extensibility.Python) that handles two-way communication with Python libraries.
In .Net Ledger terms, this is an optional extension provider based on the Extensibility API.  
.Net Ledger enables it when you set the `ExtensionProvider` config parameter to `python`. 
On startup, it imports the LibPython library (via PythonNet) and interacts directly with Python runtime objects. 
This provides basic integration capabilities: cross-method invocation on both sides (the ability to call Python code from .Net and vice versa) and data conversion for primitive data types.

Seamless integration of domain objects on both sides is provides by a specially written Python module `ledger`. 
When it is imported, it directly links .Net and Python objects, providing the required additional data conversion for complex data types.
The state of objects is constantly in sync, so changes to objects on one side are immediately reflected in linked objects on the opposite side.
This ensures that your Python code has full access to .Net Ledger domain objects: each object at each moment of time contains actual data in properties and child structures.
The Python extension provider ensures that the module is available and can be imported at the start of every session.
The module can be also distributed as a standalone software that provides Python-host integration scenarios.

Thus, when we look at the Ledger-host integration, the Python extension provider takes two steps when it is enabled and started: 
- It initializes Python integration by accessing LibPython. To do this, it needs to know the full path to the LibPython library.
- It imports `ledger` module after extracting from embedded resources.

The extension provider is then ready to go. It serves input requests, invokes code on both sides and keeps domain objects in sync state until the session containing the provider ends.

### Configuration concept

Basically, Python extension provider only needs for one configuration parameter: the full path to LibPython. This would be an additional .Net Ledger configuration setting.
However, after analyzing practical cases, it was decided to create a separate configuration file for Python extension provider:
- It is not easy for users to find the location of LibPython (especially on Unit-like operation systems). 
  You may use auxiliary libraries like `find-libpython`, which in turn require the full path to the Python executable.
- It is easy to make a mistake if you specify configuration settings for Python integration manually.
  It is preferable to use a helper script that can manage and verify settings.
- Python integration settings are needed at the moment of building .Net Ledger binaries (for example, for running Python unit tests).
  Thus these settings should not depend on .Net Ledger configuration.

As a result, I ended up with following structure of the configuration file:
- It contains two consistent values: the full path Python executable file and the full path to LibPython library.
- It is located by a specific path (for the current user) and has a well-known name.
- It has a well-known format (XML)

Here is an example of the Python integration configuration file (Windows):
```xml
<?xml version="1.0" encoding="UTF-8"?>
<!--NLedger Python Integration Settings-->
<nledger-python-settings>
  <py-executable>C:\Users\Dmitry\AppData\Local\NLedger\python-3.6.1-embed-amd64\python.exe</py-executable>
  <py-dll>C:\Users\Dmitry\AppData\Local\NLedger\python-3.6.1-embed-amd64\python36.dll</py-dll>
</nledger-python-settings>
```
The same file on Linux:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<!--NLedger Python Integration Settings-->
<nledger-python-settings>
  <py-executable>/usr/bin/python3</py-executable>
  <py-dll>/usr/lib/x86_64-linux-gnu/libpython3.8.so.1.0</py-dll>
</nledger-python-settings>
```
The file has two elements `py-executable` and `py-dll`, which contain the full paths to the Python executable and LibPython library, respectively. 

The file is always located in the following path (Windows and Linux); one user can only have one file:
```console
C:\Users\[USER]\AppData\Local\NLedger\NLedger.Extensibility.Python.settings.xml 
/home/[USER]/.local/share/NLedger/NLedger.Extensibility.Python.settings.xml
```
*Note*: corresponded .Net code that builds the full path is *{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/NLedger/NLedger.Extensibility.Python.settings.xml*

*Note*: One file for one users implies that the configuration is shared across all .Net Ledger instances for the current user.
If you strictly need to specify alternative values for an individual .Net Ledger, you may use the following environment variables:
- `NLedgerPythonConnectionStatus` should contain `Active` value
- `NLedgerPythonConnectionPyDll` should contain path to LibPython
- `NLedgerPythonConnectionAppModulesPath` should contain path to `ledger` module. It can be empty if you have already installed the module on your Python.

One more comment about the contents of the `NLedger` folder: the Python extension provider automatically creates a subfolder with name `PyModules`
that contains `ledger` modules extracted from the embedded resources (for each version of the assembly). It was decided to keep modules as physical files
in order to speed up Python runtime: it can cache parsed code in `__pycache__` subfolder.

*Summary*: .Net Ledger Python integration requires two changes in configuration file:
- The file .../NLedger/NLedger.Extensibility.Python.settings.xml should contain information about the connected Python runtime
- .Net Ledger configuration setting `ExtensionProvider` should have `python` value

> If the file NLedger.Extensibility.Python.settings.xml does not exist by the expected path, the integration is considered to be disabled even though `ExtensionProvider` setting contains `python` value.

Changing the configuration settings manually is possible, but not recommended due to the complexity and potential for error. 
You may consider using Python ToolSet script that can make these changes automatically (see the next section).

### Python ToolSet helper script

The Python ToolSet is a Powershell script that was designed to simplify setting up a local environment for .Net Ledger Python integration.
It basically covers the following activities:
- Quick environment setup for .Net Ledger Python integration or .Net Ledger build
- Discovering local environment settings (primarily local Python deployments) 
- Managing and validating .Net Ledger Python integration settings
- Installing embedded Python deployments (any versions)
- Installing and managing required Python modules
- Installing and testing .Net Ledger Python module

The script is basically designed to work in [REPL](https://en.wikipedia.org/wiki/Read%E2%80%93eval%E2%80%93print_loop) mode in an interactive console, 
but you can also call commands from the command line.

You can run the script by means of the following commands (Windows):
```console
>PowerShell -ExecutionPolicy RemoteSigned -File ./get-nledger-tools.ps1 -pythonTools
```
or Linux:
```console
$pwsh ./get-nledger-tools.ps1 -pythonTools
```
Another way is to run /Contrib/Python/GetPythonEnvironment.ps1 file. Windows users can execute (/Contrib/Python/GetPythonEnvironment.Console.cmd).
People who installed MSI, can simply run the corresponded menu shortcut.

> The the script runs, it shows a list of available commands. You can get a quick help for every command by typing `get-help [command]`

The script provides the following features:

- `discover` shows you local Python deployments. It checks PATH variable, the folder with embedded Python deployments and checks what is installed by the path specified in 
  NLedger.Extensibility.Python.settings.xml. If you have a local Python in another location, you can check it by adding `-path [PATH]` argument.

  What you can check in the output: Python version (it should be in the expected range), Python platform (`x64` is mainly expected), 
  `pip` version (you may install if you want to use this Python deployment),
  `wheel` version (you should install it if you want to build .Net Ledger Python module: connect this Python and use `install-wheel`),
  `pythonnet` version (you should install it if you want to test .Net Ledger Python module: connect this Python and use `install-pythonnet`. Refer to the module's Readme if you have issues with this step).

- `status` shows you your current integration status. It basically checks whether the file NLedger.Extensibility.Python.settings.xml exists and has valid content.
  Also, it checks that the Python extension provider is enabled. If you see both checks passed, then your Python integration should work as expected.

  Another use of this command is to check the expected location of the configuration file, path to connected Python deployment and
  whether PythonNet and Ledger modules are installed on that deployment.

- `connect` creates or updates the connection file NLedger.Extensibility.Python.settings.xml.
  If you run this command with no parameters, it will search for Python deployment by PATH and use it if successful.
  Otherwise, on Windows, it will download and install the embedded Python 3.8.1. Eventually, it will create or update the connection configuration file.

  If you want to connect to a specific local Python deployment, add `-path [PATH]` argument. 
  If you want to connect to a specific version of the embedded Python, add `-embed [N.N.N]` argument where the numbers are a three-digit Python version. 
  Notice that if the connection file already exists, it will try to use the specified there deployment. Execute `disconnect` before this command to make it work by default sequence.

  You can execute `status` after this command to check that the connection is created as expected.

- `enable` turns on the Python extension provider in .Net Ledger settings. Basically, it sets `python` value to `ExtensionProvider` parameter.
  An important feature of this command is that it also runs the `connect` command before it, so you can get a fully configured environment by simply executing `enable`. 

  Notice that it changes `ExtensionProvider` setting on user level that implies it updates `\NLedger\user.config` file. Settings on the application level can override it.
  Use NLManagement/NLSetup.Console.ps1 console (command "show-details ExtensionProvider -allValues") to check this point in case of issues.

  You can execute `status` after this command to check that the provider is enabled as expected.

- `disable` turns off the Python extension provider by placing an empty value in the `ExtensionProvider` parameter.

- `disconnect` removes the current connection file NLedger.Extensibility.Python.settings.xml.

There are additional auxiliary commands that help to manage the local environment:

- `install-python` and `uninstall-python` install and remove embedded Python deployments. 
  You may specify `-version [N.N.N]` argument to specify a three-digit version of Python that you want to install or remove.

- `install-pythonnet` and `uninstall-pythonnet` install and remove PythonNet module on the connected Python. 
  By default, it installs the public release from PyPi (PythonNet 2.5.2). 
  If it does not work (for example, for Python 3.9 or later), you may try to install pre-release version of PythonNet 3. In this case, add `-pre` switch to the command.
  This command re-installs PythonNet, so that it will uninstall a previous version if it installed.

  Note: Installing PythonNet might require additional steps on some environments. Please, refer to Troubleshooting section in 
  the Ledger module's [Readme](https://github.com/dmitry-merzlyakov/nledger/blob/master/Source/NLedger.Extensibility.Python.Module/README.md) to get more information.

- `install-ledger` and `uninstall-ledger` install and remove .Net Ledger Python module on the connected Python. 
  The module should exists in /Contrib/Python/ folder (it is important if you build .Net Ledger from source code, so finish the build first). 
  Installing Ledger module requires PythonNet, so try to install it before if you have problems with this command.
  
- `test-ledger` runs Python unit tests for the installed .Net Ledger Python module.

- `test-wheel` installs "wheel" module on the connected Python if it has not been installed yet. No changes otherwise.

### Best Practices

Here is a list of best practices for setting up your environment for specific cases. You can find your case in the list and follow recommendations.

- I installed .Net Ledger from MSI (or pre-built binaries) and now I want to enable Python integration:
  - Open Python ToolSet console
  - Execute `enable`
  - In case of any errors - execute `connect` and troubleshoot the issue. On Windows, you can try `connect -embed 3.8.1` to force installing the isolated embedded Python.
  - Execute `status` and see that both checks are Green now

- I downloaded .Net Ledger sources and I want to build it from source code:
  - Open Python ToolSet console
  - Execute `connect`
  - In case of any errors - troubleshoot the issue. On Windows, you can try `connect -embed 3.8.1` to force installing the isolated embedded Python.
  - Execute `status` and see that `Python Connection` check is Green (the second one is expectable Red)
  - Execute `install-wheel`
  - Run build. Note: .Net Ledger Python module will not be tested

- I downloaded .Net Ledger sources and I want to build it from source code including proper testing of .Net Ledger Python module:
  - Open Python ToolSet console
  - Execute `connect`
  - In case of any errors - troubleshoot the issue. On Windows, you can try `connect -embed 3.8.1` to force installing the isolated embedded Python.
  - Execute `status` and see that `Python Connection` check is Green (the second one is expectable Red)
  - Execute `install-wheel`
  - Execute `install-pythonnet`. In case of issues, try `install-pythonnet -pre` or troubleshoot and install PythonNet manually.
  - Run build. No errors and no warnings expected.

- I have a local Python but I want to connect to an isolated embedded Python with a specific version:
  - Open Python ToolSet console
  - Execute `connect -embed [N.N.N]` where `[N.N.N]` is a three-digit Python version
  - Execute `enable`
  - Execute `status` and see that both checks are Green

- I have a local Python but its folder is not listed in PATH. I want to use it.
  - Open Python ToolSet console
  - Execute `discover -path [PATH]` where `[PATH]` is a full path to your Python executable file. Example:
    - *discover -path C:\Users\dmitry\AppData\Local\NLedger\python-3.9.2-embed-amd64\python.exe*
  - Verify that your Python matches integration requirements (3.6.1 or later, x64, pip is installed)
  - Execute `connect -path [PATH]`
  - Execute `enable`
  - Execute `status` and see that both checks are Green

- My connection is configured but I want to use another Python
  - Open Python ToolSet console
  - Execute `disconnect`
  - Execute `connect NNN` where NNN are your preferences (none or `path` or `embed`)
  - Execute `status` and see that you are connected to preferred Python now

- Python integration is enabled on my machine. I want to install and use .Net Ledger Python module
  - Make sure that the `ledger-0.8.N-py3-none-any.whl` file exists in `/Contrib/Python/` folder.
  - Open Python ToolSet console
  - Execute `status` and make sure that `PythonNet Module` is installed. Execute `install-pythonnet` otherwise (additional actions might be required)
  - Execute `install-ledger`
  - Execute `status` and make sure that `Ledger Module` shows a valid version
  - Execute `test-ledger` (no errors expected)
  - Execute `status`, copy `Python Executable` value and run it in a command line
  - Type `import ledger` in the Python console

- I do not know whether I have a local Python
  - Open Python ToolSet console
  - Execute `discover`
  - See what you have in the output:
    - `[Local Python]` refers to a local Python available by PATH
    - `[Embed Python]` contains a list of embedded Pythons in NLedger folder (Windows only)
    - `[NLedger Settings]` shows what is currently specified in the connection file (if exists)
  - If a local Python found, the command `connect` will use it. Otherwise, `connect` will install an embedded Python (Windows only).
    On other OS, you should install Python and repeat.

- I do not want to use Python integration anymore:
  - Open Python ToolSet console
  - Execute `disable`. It will remove `ExtensionProvider` setting, so .Net Ledger will not use Python extension.
  - Execute `disconnect`. It will disable Python feature for the build process (Python-related tests are ignored; Python module is not built) and for integration tests.
  - Execute `status` and see that both checks are Red now
  - Note: if you run the build with disabled Python integration, you will see warnings. Add `-noPython` flag to suppress them (*get-nledger-up.ps1 -noPython*)

### Troubleshooting

Here is a list of guidelines that can be helpful in troubleshooting Python integration issues. Please, follow it if you have problems.

1) Open Python ToolSet console, execute `status` and make sure that both checks (`Python Connection` and `Python Extension`) indicate that they are `[Enabled]`.
   Take corrective actions if one of them is not enabled.
2) Make sure that .Net Ledger configuration setting returns a valid effective value `python`. Run `NLSetup.Console` and execute `show-details ExtensionProvider -allValues`.
   Check that `Effective Value` returns `python`. Take corrective actions otherwise.
3) Check Python version and make sure that the connected Python is 64-bit (assuming that your .Net Ledger runs as 64-bit process as well).
   - Open Python ToolSet console
   - Execute `status` and copy `Python Executable`
   - Execute `discover -path [Python Executable]`
   - Check that you have a valid output (no error messages). Troubleshoot the issue otherwise
   - Check that `Python Version` is in valid range (3.6.1 or later)
   - Check that `Python Platform` is `x64`
   - Check that `Pip` is installed; install it otherwise
4) Check that the connection file contains valid values
   - Open Python ToolSet console
   - Execute `status` and note the path `Connection Settings`
   - Open this file in any text editor and check that both values (`py-executable` and `py-dll`) point at valid files. At least, they should physically exist
5) If you use a local Python and cannot localize a problem, try to use an isolated embedded Python (Windows only):
   - Open Python ToolSet console
   - Execute `connect -embed 3.8.1` (ot whatever Python version you prefer)

You can request additional help by posting an issue on GitHub.

## NLedger Configuration File

As a regular .Net application, NLedger command line utility has the own configuration file: *NLedger-cli.exe.config* (*NLedger-cli.dll.config* for Core application).
It contains several options that are specific for .Net product and Windows environment.

*Note: consider managing of user settings by means of Setup Console.*

Available configuration options are:

- **IsAtty** (Boolean, default value is True) - indicates whether the output console supports extended ATTY functions.
  Basically, this option specifies the result of `isatty` function that Ledger uses in code.
  If this function returns True, Ledger colorizes the output (adds VT100 codes) and never does it otherwise;
- **AnsiTerminalEmulation** (Boolean, default value is True) - adds a handler to the output stream that
  processes VT100 codes and colorizes the console. If this option is enabled, you will see colored output
  like the original Ledger. Otherwise, you will see bare VT100 codes in the console;
- **OutputEncoding** (String, default value is empty) - forces switching the output stream to a specified encoding.
  If this option is empty, NLedger uses the default output encoding that depends on your local console settings.
  If it is a single byte code page encoding (SBCSCodePageEncoding) you might have troubles with reading Unicode
  characters like '℃'. In this case, you need to set this option to `utf-8`;
- **TimeZoneId** (String, default value is empty) - specifies the current time zone that Ledger uses
  when converts local date and time to UTC (e.g. in the method *format_emacs_posts*).
  If this value is empty, it uses the computer time zone.
  It should be a valid Time Zome Info name (see more about TimeZoneInfo.FindSystemTimeZoneById),
  for example `Central Standard Time`
- **DefaultPager** (String, default value is empty) - specifies a default pager name.
  When this value is not empty and `IsAtty` is turned on, NLedger attempts to find
  the specified application and sends all the output to its input stream. If the name does not have a path,
  NLedger searches an executable file in folders listed in PATH variable. Extension can be omitted in this case.
  Command line arguments (if any) should be separated from the name by '|' symbol.
- **ExtensionProvider** (String, default value is empty) - specifies the name of an extension provider.
  If this value is empty, integration capabilities are disabled; any attempts to use integration directives will lead to *... not supported* errors.
  Possible values are `python` (Python extension provider) and `dotnet` (.Net extension provider).

*Note: if any modifications in the configuration file are not acceptable, NLedger can receive these values
by means of environment variables. It checks the variables with the same names and with the prefix "nledger":
nledgerIsAtty, nledgerAnsiTerminalEmulation, nledgerOutputEncoding and nledgerTimeZoneId.*

(c) 2017-2022 [Dmitry Merzlyakov](mailto:dmitry.merzlyakov@gmail.com)