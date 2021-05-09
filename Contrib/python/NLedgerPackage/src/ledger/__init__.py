import ledger.conf
from ledger.conf import clrRuntime
from ledger.conf import ClrRuntime

from clr_loader import get_coreclr
from clr_loader import get_mono
from clr_loader import get_netfx

from pythonnet import set_runtime

# Set preferrable runtime and load clr

if clrRuntime == ClrRuntime.core:
    set_runtime(get_coreclr(ledger.conf.corefile))
elif clrRuntime == ClrRuntime.mono:
    set_runtime(get_mono())
elif clrRuntime == ClrRuntime.netfx:
    set_runtime(get_netfx())

import clr

# Load library dll

#Adding path to runtime dll to PATH and sending only name fixes a problem in pythonnet:
#it tries to use Assembly.Load for loading an assembly specified by path and name
import ntpath
import sys

dllFolder = ntpath.dirname(ledger.conf.runtimefile)
dllName = ntpath.splitext(ntpath.basename(ledger.conf.runtimefile))[0]

if not(dllFolder in sys.path):
   sys.path.append(dllFolder)

clr.AddReference(dllName)

# Import library classes and specifying custom helper methods

# Module initialization

from NLedger.Extensibility.Python import PythonSession
PythonSession.PythonModuleInitialization()

# Export Ledger classes and globals

from NLedger.Extensibility.Export import CommodityPool
from NLedger.Extensibility.Export import Commodity

commodities = CommodityPool.commodities
COMMODITY_STYLE_DEFAULTS = CommodityPool.COMMODITY_STYLE_DEFAULTS;
COMMODITY_STYLE_SUFFIXED = CommodityPool.COMMODITY_STYLE_SUFFIXED;
COMMODITY_STYLE_SEPARATED = CommodityPool.COMMODITY_STYLE_SEPARATED;
COMMODITY_STYLE_DECIMAL_COMMA = CommodityPool.COMMODITY_STYLE_DECIMAL_COMMA;
COMMODITY_STYLE_TIME_COLON = CommodityPool.COMMODITY_STYLE_TIME_COLON;
COMMODITY_STYLE_THOUSANDS = CommodityPool.COMMODITY_STYLE_THOUSANDS;
COMMODITY_NOMARKET = CommodityPool.COMMODITY_NOMARKET;
COMMODITY_BUILTIN = CommodityPool.COMMODITY_BUILTIN;
COMMODITY_WALKED = CommodityPool.COMMODITY_WALKED;
COMMODITY_KNOWN = CommodityPool.COMMODITY_KNOWN;
COMMODITY_PRIMARY = CommodityPool.COMMODITY_PRIMARY;

# Routine to acquire and release output streams

from io import StringIO

class RedirectWrapperIO(StringIO):

    is_error = False

    def __init__(self, is_error):
        self.is_error = is_error

    def write(self,s):
        return PythonSession.ConsoleWrite(s, self.is_error)

_stdout = sys.stdout
_stderr = sys.stderr

def acquire_output_streams():
    sys.stdout = RedirectWrapperIO(False)
    sys.stderr = RedirectWrapperIO(True)

def release_output_streams():
    sys.stdout = _stdout
    sys.stderr = _stderr
