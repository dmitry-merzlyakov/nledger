# .Net Ledger Python Module

.Net Ledger Python module provides .Net Ledger capabilities in Python interpreter session. 

## Introduction

[.Net Ledger](https://github.com/dmitry-merzlyakov/nledger) is a ported Ledger accounting system (www.ledger-cli.org) to the .Net platform. It fully supports all original functions without limitations. .Net Ledger produces exactly the same output as the original software.

> [Ledger](https://www.ledger-cli.org/) is a powerful double-entry accounting system. You can find more information [here](https://github.com/ledger/ledger).

This module supplies Ledger functionality in Python sessions including reading journal files, accessing data objects and executing commands. It provides the same functional interface as Ledger (naming conventions, class members, data types, functions, see [Ledger documentation](https://www.ledger-cli.org/3.0/doc/ledger3.html#Extending-with-Python)), but also includes some extensions.

.Net Ledger Python module can be distributed as standalone software. No additional actions, except for installing the package in the Wheel format, are required.

## Installation

.Net Ledger Python module is distributed as a Wheel file (ledger-x.x.x-py3-none-any.whl) and can be installed in a usual way, e.g. by means of Pip:

```console
$ python -m pip install ./ledger-0.8.4-py3-none-any.whl
```
By default, Pip will resolve dependencies and install an officially published PythonNet 2.5.x. If you want to run the module on PythonNet 3, you will need to install this version of PythonNet in advance.

The installed module can be verified by unit tests. The test file can be found in NLedger deployment (/Contrib/Python/ledger_tests.py) or in the source code repository (/Source/NLedger.Extensibility.Python.Module/tests/ledger_tests.py).
As usual Python unit tests, they should be run in a command line console:
```console
$ python ./ledger_tests.py
```

## Usage

.Net Ledger Python module is completely ready to work right after import:
```python
import ledger
```
The primary module feature is ability to read Ledger journal files. The loaded journal is stored in the current session. Accessing the journal, you can iterate through transactions or query posts as many times as you need.

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
When you finish working with the current journal, you should clean up loaded data. This is especially necessary if you want to load other journal files.
```console
>>> ledger.session.close_journal_files()
```
Another important thing you can do with the module is manipulate Ledger objects. You can get or create commodities, amounts, balances, accounts, posts, transactions and even compose journals.

The code sample below illustrates reading a journal file, querying data with an expression and manipulating with Ledger data objects (it is Ledger unit test 78AB4B87).

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
.Net Ledger Python module also allows executing regular Ledger commands with parameters, for example "bal ^Expenses". They produce formatted textual output like the original Ledger console application.

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
You can find more information in [Ledger documentation](https://www.ledger-cli.org/3.0/doc/ledger3.html#Extending-with-Python). It is also recommended to observe the unit test file (ledger_tests.py) that is full of examples how to use every individual class and method. You can also review the module interface itself, for example, by means of "help(ledger)" Python command.

### Configuration Settings

The module variable "config" (containing Config class instance) can be used for changing Ledger runtime settings. Currently supported "is_atty" property and "get_env"/"set_env" methods:
```python
ledger.config.is_atty = True
```
Module initialization can be regulated by means of environment variables:
- Variable "nledger_python_clr_runtime" tells PythonNet 3 which .Net runtime to run. Possible values are "netfx", "mono", "core". For "core", you may also specify the path to the runtime config in "nledger_python_clr_runtime_config" variable. See PythonNet 3 documentation for details.
- Variable "nledger_extensibility_python_dll_path" can contain an alternative path to "NLedger.Extensibility.Python.dll". 

## Technologies

.Net Ledger functionality is encapsulated into an assembly file in .Net Standard 2.0 format so it is compatible with the majority of .Net platforms (.Net, Core, Framework, Mono) and can work on any OS (WIndows, Mac OS, Linux).

Integration with Python is build over [PythonNet](https://github.com/pythonnet/pythonnet) library. It can work with any actual PythonNet versions (official 2.5.x or modernized 3.x). You can make a decision which PythonNet to use depending on your preferences.

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

The .Net Ledger Python module is built on PythonNet, so it is important that the latter works well. The first thing you need to check is that PythonNet functions properly, for example:

```console
>>> import clr
>>> from System import DateTime
>>> print(DateTime.Now.ToString())
10/21/2021 8:46:15 PM
```

If you see a different result other than the current time, you should troubleshoot and resolve your PythonNet installation issues.

>Disclaimer: PythonNet 3 is currently in development. It has successfully passed all tests at the time the .Net Ledger Python module was released, but later versions may have issues. If you run into intractable problems, consider using PythonNet 2.5.x

If you have an irrelevant results, it is recommended to verify your case with the original Ledger console application. It might be an expected result and you probably need to clarify the case requesting help on Ledger resources.

If detect a problem with .Net Ledger Python module specifically - please, add an issue on .Net Ledger GitHub.

## Resources

- .Net Ledger on [GitHub](https://github.com/dmitry-merzlyakov/nledger)
- [Ledger documentation](https://www.ledger-cli.org/docs.html) or [other resources](http://plaintextaccounting.org/)
- PythonNet on [GitHub](https://github.com/pythonnet/pythonnet)

## Acknowledgments

- [Ledger: Command-Line Accounting](https://github.com/ledger/ledger)
- [Python.NET](https://github.com/pythonnet/pythonnet)

## Contact

- Send an email to [Dmitry Merzlyakov](mailto:dmitry.merzlyakov@gmail.com)
- Contact me via [GitHub](https://github.com/dmitry-merzlyakov/nledger)

## Licensing

The code is licensed under 3-clause [FreeBSD license](https://github.com/dmitry-merzlyakov/nledger/blob/master/LICENSE).

(c) 2017-2021 [Dmitry Merzlyakov](mailto:dmitry.merzlyakov@gmail.com)
