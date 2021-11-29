# .Net Ledger Python Module

.Net Ledger Python module provides .Net Ledger capabilities in Python interpreter session. 

## Introduction

[.Net Ledger](https://github.com/dmitry-merzlyakov/nledger) is a ported Ledger accounting system (www.ledger-cli.org) to the .Net platform. 
It fully supports all original features without limitation. .Net Ledger produces exactly the same output as the original software.

> [Ledger](https://www.ledger-cli.org/) is a powerful double-entry accounting system. You can find more information [here](https://github.com/ledger/ledger).

This module provides Ledger functionality in Python sessions including reading journal files, accessing data objects, and executing commands. 
It gives equal functional scope as Python extension in Ledger (naming conventions, class members, data types, functions, see [Ledger documentation](https://www.ledger-cli.org/3.0/doc/ledger3.html#Extending-with-Python)), and also includes some exclusive features like executing Ledger commands.

.Net Ledger Python module can be distributed as standalone software. No additional action is required except for installing the package in Wheel format.

## Requirements

- `Python` 3.6 or later (Windows, Linux, OSX) (32-bit or 64-bit)
- `PythonNet` module 2.5.2 or later ([pypi](https://pypi.org/project/pythonnet/), [github](https://github.com/pythonnet/pythonnet)) (PythonNet 3 is supported)


## Installation

.Net Ledger Python module is distributed as a Wheel file (ledger-x.x.x-py3-none-any.whl) and can be installed in the usual way, for example with Pip:

```console
$ python -m pip install ./ledger-0.8.4-py3-none-any.whl
```
By default, Pip resolves dependencies and installs the officially published PythonNet 2.5.x.
If you want to run a module on PythonNet 3, you need to install this version of PythonNet in advance.
If you run into problems at this point, see the Troubleshooting section.

The installed module can be verified using unit tests. The test file can be found in the NLedger deployment (/Contrib/Python/ledger_tests.py) or in the source code repository (/Source/NLedger.Extensibility.Python.Module/tests/ledger_tests.py).
Like regular Python unit tests, they should be run from the command line console:
```console
$ python ./ledger_tests.py
```

## Usage

.Net Ledger Python module is completely ready to work right after installation:
```python
import ledger
```
The main function of the module is the ability to read Ledger journal files. 
The loaded journal is saved in the current session. 
Once you have access to the journal, you can iterate through transactions or query posts as many times as you like. 

```console
>>> for post in ledger.read_journal('drewr3.dat').query('expenses'):
...   print(post.amount)
...
$ 37.50
$ 37.50
$ 37.50
$ 37.50
$ 37.50
$ 37.50
$ 500.00
$ 300.00
$ 65.00
$ 44.00
$ 5,500.00
$ 20.00
```
When you finish working with the journal, you should clear loaded data. This is especially necessary if you want to load other journal files.
```console
>>> ledger.session.close_journal_files()
```
Another important thing you can do with the module is manipulate Ledger objects. You can get or create commodities, amounts, balances, accounts, posts, transactions and even journals.

The code sample below illustrates reading a journal file, querying data by means of an expression and manipulating with Ledger data objects (it is the Ledger unit test 78AB4B87).

```python
from __future__ import print_function

import ledger

eur = ledger.commodities.find_or_create('EUR')

total_eur = ledger.Amount("0.00 EUR")
total_gbp = ledger.Amount("0.00 GBP")
total = ledger.Amount("0.00 EUR")

for post in ledger.read_journal("test/regress/78AB4B87.dat").query("^income:"):
    print(post.amount)
    print(post.amount.commodity)
    if post.amount.commodity == "EUR":
        total_eur += post.amount
    elif post.amount.commodity == "GBP":
        total_gbp += post.amount

    a = post.amount.value(eur)
    if a:
        print("Total is presently: (%s)" % total)
        print("Converted to EUR:   (%s)" % a)
        total += a
        print("Total is now:       (%s)" % total)
    else:
        print("Cannot convert '%s'" % post.amount)
    print()

print(total)
```
.Net Ledger Python module also allows you to execute regular Ledger commands with parameters such as "bal ^Expenses". They produce formatted text output like the original Ledger console application.

```console
>>> import ledger
>>> jrn = ledger.read_journal("drewr3.dat")
>>> ledger.print_command("bal ^Expenses")
          $ 6,654.00  Expenses
          $ 5,500.00    Auto
             $ 20.00    Books
            $ 300.00    Escrow
            $ 334.00    Food:Groceries
            $ 500.00    Interest:Mortgage
--------------------
          $ 6,654.00
>>>
```
You can find more information in [Ledger documentation](https://www.ledger-cli.org/3.0/doc/ledger3.html#Extending-with-Python). 
It is also recommended that you familiarize yourself with the unit test file (ledger_tests.py), which is full of examples of how each individual class and method can be used. 
You can also review the module interface itself, for example using the "help (ledger)" Python command. 

### Configuration Settings

.Net ledger runtime settings can be changed within the current Python session using the `config` module variable. This variable holds `Config` object instance that provides `is_atty` property and `get_env`/`set_env` methods:
```python
ledger.config.is_atty = True
```
Module initialization can be regulated by means of environment variables:
- Variable `nledger_extensibility_python_dll_path` can contain an alternative path to `NLedger.Extensibility.Python.dll`. 
- If you use the PythonNet 3, you can specify which .Net runtime to run using `nledger_python_clr_runtime` variable. Possible values are `netfx`, `mono`, `core`. For `core`, you may also specify the path to the runtime config in `nledger_python_clr_runtime_config` variable. See PythonNet 3 documentation for more details.

## Technologies

.Net Ledger functionality is encapsulated into an assembly file in .Net Standard 2.0 format so it is compatible with the majority of .Net platforms (.Net, Core, Framework, Mono) and can work on any OS (Windows, Mac OS, Linux).

Integration with Python is build over [PythonNet](https://github.com/pythonnet/pythonnet) library. .Net Ledger is compatible with all actual PythonNet versions (official 2.5.x or modernized 3.x). You can make a decision which PythonNet to use depending on your preferences.

## How It Works

The .Net Ledger Python module is mainly designed to adapt the .Net Ledger domain model to the Ledger Python domain model specified in the original Ledger. 

Adapting covers:
- Name conventions (namespace, class names, class types, member names)
- Data type conversions for primitive types (dates, enums, lists) and domain objects
- Object life cycle management (creating standalone and referenced wrappers, managing data structures and nullable types)
- Managing specialized package members (functions, constants, .Net Ledger extras like executing commands)
- Technological aspects (configuration settings, startup management, context management, stream redirection)

In accordance with the purposes of adaptation, the principles of conceptual design are:
- The module specifies a set of classes, functions and variables that are structurally equal to Ledger Python domain model (same names for the module, classes and all members; same method signatures)
- Each Python class is associated with a corresponding .Net object from the .Net Ledger domain model. It creates .Net object if it was instantiated in Python code or takes an instance if it wraps an exported .Net object
- Each Python method calls a corresponded .Net method from an associated .Net object. It is responsible to convert parameters from Python types to .Net types before the call and convert the result from .Net objects to their Python representation

PythonNet library is responsible for communication between Python and CLR objects. It is also responsible for primitive data type conversion (string, int and other). The module manages more complicated cases (list and flag adapters, date conversions etc)

This design ensures full compatibility with the Ledger Python domain model.

## Troubleshooting

### Installation Problems

If you get errors while trying to install the module, they are most likely caused by trying to install the dependent PythonNet.
In this case, you should install PythonNet manually and then try to install the .Net Ledger Python module again. 

First of all, check whether PythonNet is installed on your Python environment (it assumes you have Pip already installed):
```console
$python -m pip show pythonnet
Name: pythonnet
Version: 2.5.2 
```
If you see a result similar to above, the installation issue is caused by other factors. Please try to analyze deeper into the installation error messages. Otherwise, you can take further steps.

> Disclaimer: PythonNet is under active development and there may be better solutions at the time of reading this thread than those described below. 
Please, check recent updates on PythonNet community resources [Gitter](https://gitter.im/pythonnet/pythonnet) and [Github Memory](https://githubmemory.com/@pythonnet)

Make sure all prerequisites are installed:

- Windows users: make sure that [.Net Framework 4.7.2 or later](https://dotnet.microsoft.com/download/dotnet-framework) is installed.
  Basically, PythonNet can use other .Net platforms, but this requires additional configuration, which can complicate your steps. 
- Linux users (see more [here](https://www.activestate.com/resources/quick-reads/pip-install-pythonnet/) and [here](https://stackoverflow.com/questions/55058757/install-pythonnet-on-ubuntu-18-04-python-3-6-7-64-bit-mono-5-16-fails)):
  - `clang` is installed (sudo apt-get install clang)
  - `nuget` is installed (sudo apt install nuget)
  - **Complete** `Mono` is installed on your machine. Find an appropriate instruction (for example, Ubuntu users can find it [here](https://linuxize.com/post/how-to-install-mono-on-ubuntu-20-04/). The key command is *sudo apt install mono-complete*)
  - `pycparser` is installed (e.g. python3 -m pip install -U pycparser --user)
- OSX users - can follow ActiveState's [instruction](https://www.activestate.com/resources/quick-reads/pip-install-pythonnet/):
  - Install `Mono` from brew and correct environment variables as it is described in the instruction
  - Install `pycparser` (pip install pycparser)


If all prerequisites are installed, try to install PythonNet from PyPi using the following command:
```console
>python -m pip install pythonnet
```
or
```console
$python3 -m pip install -U pythonnet --user
```

It will install the official release PythonNet 2.5.2. This PythonNet version properly supports Python versions 3.5.x - 3.8.x.
If you have Python 3.9.x or later, you may download the unofficial package `pythonnet‑2.5.2‑cp39‑cp39‑win_amd64.whl` (or `pythonnet‑2.5.2‑cp39‑cp39‑win32.whl` for 32-bit Python respectively) from this [resource](https://www.lfd.uci.edu/~gohlke/pythonlibs/#pythonnet) and install it. More information is [here](https://stackoverflow.com/questions/67418533/how-to-fix-error-during-pythonnet-installation).

Another option is to install pre-release version of PythonNet 3. .Net Ledger Python module was tested with PythonNet 3, so it might be a good choice. Add "--pre" flag to the command:
```console
>python -m pip install pythonnet --pre
```

Ultimately, Linux and OSX users can build PythonNet 3 from source code. Use path to GitHub PythonNet repository for it:
```console
/usr/bin/python3 -m pip install -U git+https://github.com/pythonnet/pythonnet --user --egg 
```

If PythonNet is installed properly, the command `python -m pip show pythonnet` should show you a correct version of the installed package.

### Runtime Problems

If you get error messages while trying to import the Ledger module, you should first check if the PythonNet module is working correctly.
Try the following piece of code and check the result:
```console
>>> import clr
>>> from System import DateTime
>>> print(DateTime.Now.ToString())
10/21/2021 8:46:15 PM
```
If you see another result (not the current time), you should troubleshoot and resolve your PythonNet installation issues.

>Disclaimer: If you are using PythonNet 3, remember that it is currently under development.
Although it passed all the tests as of the time the .Net Ledger Python module was released, there may be issues in later versions.
If you run into intractable problems, consider using PythonNet 2.5.x

If you get an irrelevant calculation results, it is recommended to verify your case with the original Ledger console application. 
This might be an expected case and you probably need to clarify the issue requesting help on Ledger resources.

If detect a problem with .Net Ledger Python module specifically - please, add an issue on .Net Ledger [GitHub](https://github.com/dmitry-merzlyakov/nledger/issues).

## Resources

- .Net Ledger on [GitHub](https://github.com/dmitry-merzlyakov/nledger)
- [Ledger documentation](https://www.ledger-cli.org/docs.html) or [other resources](http://plaintextaccounting.org/)
- PythonNet on [GitHub](https://github.com/pythonnet/pythonnet)
- PythonNet community on [StackOverflow](https://stackoverflow.com/questions/tagged/python.net)

## Acknowledgments

- [Ledger: Command-Line Accounting](https://github.com/ledger/ledger)
- [Python.NET](https://github.com/pythonnet/pythonnet)

## Contact

- Send an email to [Dmitry Merzlyakov](mailto:dmitry.merzlyakov@gmail.com)
- Contact me via [GitHub](https://github.com/dmitry-merzlyakov/nledger)

## Licensing

The code is licensed under 3-clause [FreeBSD license](https://github.com/dmitry-merzlyakov/nledger/blob/master/LICENSE).

(c) 2017-2021 [Dmitry Merzlyakov](mailto:dmitry.merzlyakov@gmail.com)
