import ntpath
from enum import Enum
from configparser import ConfigParser

# Options to load dotnet runtime
ClrRuntime = Enum('ClrRuntime', 'auto netfx core mono')
clrRuntime = ClrRuntime.auto

# full path to module config file name
configfile = ntpath.abspath(ntpath.join(ntpath.dirname(ntpath.realpath(__file__)), 'ledger.config'))

# full path to runtime file name
runtimefile = ntpath.abspath(ntpath.join(ntpath.dirname(ntpath.realpath(__file__)), '../runtime/NLedger.Extensibility.Python.dll'))

# full path to core runtime file name
corefile = ntpath.abspath(ntpath.join(ntpath.dirname(ntpath.realpath(__file__)), '../runtime/NLedger.runtimeconfig.json'))

# Validate package consistency
if not ntpath.isfile(configfile):
    raise Exception("File not found: " + configfile)
if not ntpath.isfile(runtimefile):
    raise Exception("File not found: " + runtimefile)
if not ntpath.isfile(corefile):
    raise Exception("File not found: " + corefile)

# Manage configuration file

def parse_ClrRuntime(str):
    try:
        return ClrRuntime[str.lower()]
    except:
        raise Exception("Incorrect ClrRuntime option '" + str + "'. Allowed values: auto netfx core mono")

def read_configfile():
    config = ConfigParser()
    config.read(configfile)

    global clrRuntime
    clrRuntime = parse_ClrRuntime(config.get('clr', 'runtime'))

def set_clrRuntime(str):
    global clrRuntime
    clrRuntime = parse_ClrRuntime(str)

    config = ConfigParser()
    config.read(configfile)
    config.set('clr', 'runtime', str)
    with open(configfile, 'w') as cf:
        config.write(cf)
    read_configfile()

# Read configuration

read_configfile()