## .Net Ledger Documentation

> This document describes practical aspects of using NLeger as a tool. If you need development guidelines, please, check [Development Notes](https://github.com/dmitry-merzlyakov/nledger/blob/master/build.md).

NLedger (.Net Ledger) is a .Net port of Ledger accounting system (www.ledger-cli.org).
You can refer to the [original documentation](https://www.ledger-cli.org/docs.html) to get 
general guidelines how to use Ledger; this documentation is completely applicable to NLedger as well.

This document more focuses on things that are specific for .Net product. In particular,
it describes installation process, special configuration options and the use of the testing framework.

## System Requirements

NLedger is a .Net console application so basic requirements are very simple; you have to have either .Net Framework or .Net Core installed on your machine:

- [.Net Framework 4.5 or higher](https://dotnet.microsoft.com/download/dotnet-framework) (for .Net Framework version of NLedger)
- [.Net Core SDK or Runtime 3.1 or higher](https://dotnet.microsoft.com/download/dotnet-core/3.1) (for .Net Core version)

Your choice depends on which host operation system you have. For Linux ans OSX, you should have .Net Core SDK. For Windows,
.Net Framework is only preferable if you want to install MSI package; ZIP package can work with both frameworks.

NLedger deployment includes a collection of helper Powershell scripts: for installation, testing, showing demo and managing environment settings.
They are not required, but generally recommended. Therefore:

- [Optionally] Powershell (you can find a latest version available for your host operation system [here](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell?view=powershell-7))

If you want to build NLedger from source code, you also need for [Git](https://git-scm.com/downloads) command line tool.

## Installation

There are three options to install NLedger: 
- For Windows, Linux and OSX: build from source code (.Net SDK build)
- For Windows: install MSI package
- For Windows: download pre-built binaries (ZIP package)

### Build and install NLedger from source code

This option works on Windows, Linux and OSX. It includes:
- Downloading NLedger source code
- Build binaries
- Passing unit and integration tests
- Installing binaries (adding to PATH variable and creating 'ledger' alias)

Prerequisites: .Net Core SDK 3.1, Powershell, Git command line tool

#### Installing on Windows

In a command line window, navigate to a folder that is appropriate to contain NLedger binary files and execute the following commands:

```
git clone https://github.com/dmitry-merzlyakov/nledger
cd nledger
powershell -ExecutionPolicy RemoteSigned -File ./get-nledger-up.ps1 -install
```

#### Installing on Linux or OSX

In a terminal window, execute the following commands:

```
git clone https://github.com/dmitry-merzlyakov/nledger
cd nledger
pwsh -file ./nledger/get-nledger-up.ps1 -install
```

#### Uninstalling NLedger

Uninstalling includes removing NLedger from PATH variable and deleting a short alias. It does not remove `nledger` folder with source code and binary files; if you should do it yourself.

On Windows - open a command line window, navigate to the folder containing NLedger and execute:
```
powershell -ExecutionPolicy RemoteSigned -File ./get-nledger-tools.ps1 -uninstall
```

On Linux or OSX - open a terminal window, navigate to the folder containing NLedger and execute:
```
pwsh -file ./get-nledger-tools.ps1 -uninstall
```

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

- Check prerequisites. You must have .Net Framework 4.5 or higher; it is absolutely required. 
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

Prerequisites: .Net Framework 4.5 or .Net Core SDK 3.1 (the package contains binaries for both frameworks)

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
you can revert your changes and continue experimenting with it. So, it is some kind of a playground that let you learn NLedger features in easy and efficiemt way.

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

## NLedger Configuration File

As a regular .Net application, NLedger command line utility has the own configuration file: *NLedger-cli.exe.config* (*NLedger-cli.dll.config* for Core application).
It contains several options that are specific for .Net product and Windows environment.

*Note: consider managing of user settings by means of Setup Console.*

Available configuration options are:

- **IsAtty** (Boolean, default value is True) - indicates whether the output console supports extended ATTY functions.
  Basically, this option specifies the result of *isatty* function that Ledger uses in code.
  If this function returns True, Ledger colorizes the output (adds VT100 codes) and never does it otherwise;
- **AnsiTerminalEmulation** (Boolean, default value is True) - adds a handler to the output stream that
  processes VT100 codes and colorizes the console. If this option is enabled, you will see colored output
  like the original Ledger. Otherwise, you will see bare VT100 codes in the console;
- **OutputEncoding** (String, default value is empty) - forces switching the output stream to a specified encoding.
  If this option is empty, NLedger uses the default output encoding that depends on your local console settings.
  If it is a single byte code page encoding (SBCSCodePageEncoding) you might have troubles with reading Unicode
  characters like '℃'. In this case, you need to set this option to *utf-8*;
- **TimeZoneId** (String, default value is empty) - specifies the current time zone that Ledger uses
  when converts local date and time to UTC (e.g. in the method *format_emacs_posts*).
  If this value is empty, it uses the computer time zone.
  It should be a valid Time Zome Info name (see more about TimeZoneInfo.FindSystemTimeZoneById),
  for example "*Central Standard Time*"
- **DefaultPager** (String, default value is empty) - specifies a default pager name.
  When this value is not empty and *IsAtty* is turned on, NLedger attempts to find
  the specified application and sends all the output to its input stream. If the name does not have a path,
  NLedger searches an executable file in folders listed in PATH variable. Extension can be omitted in this case.
  Command line arguments (if any) should be separated from the name by '|' symbol.

*Note: if any modifications in the configuration file are not acceptable, NLedger can receive these values
by means of environment variables. It checks the variables with the same names and with the prefix "nledger":
nledgerIsAtty, nledgerAnsiTerminalEmulation, nledgerOutputEncoding and nledgerTimeZoneId.*

(c) 2017-2020 [Dmitry Merzlyakov](mailto:dmitry.merzlyakov@gmail.com)