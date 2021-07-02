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
from NLedger.Extensibility.Export import KeepDetails
from NLedger.Extensibility.Export import PricePoint
from NLedger.Extensibility.Export import Annotation
from NLedger.Extensibility.Export import AnnotatedCommodity
from NLedger.Extensibility.Export import Amount
from NLedger.Extensibility.Export import ParseFlags
from NLedger.Extensibility.Export import ValueType
from NLedger.Extensibility.Export import Value
from NLedger.Extensibility.Export import ValueType
from NLedger.Extensibility.Export import Account
from NLedger.Extensibility.Export import AccountXData
from NLedger.Extensibility.Export import AccountXDataDetails
from NLedger.Extensibility.Export import Balance
from NLedger.Extensibility.Export import Expr
from NLedger.Extensibility.Export import FileInfo
from NLedger.Extensibility.Export import Position
from NLedger.Extensibility.Export import Journal
from NLedger.Extensibility.Export import JournalItem
from NLedger.Extensibility.Export import State
from NLedger.Extensibility.Export import Mask
from NLedger.Extensibility.Export import Posting
from NLedger.Extensibility.Export import PostingXData
from NLedger.Extensibility.Export import SymbolKind
from NLedger.Extensibility.Export import Session
from NLedger.Extensibility.Export import SessionScopeAttributes
from NLedger.Extensibility.Export import SortValue
from NLedger.Extensibility.Export import Times
from NLedger.Extensibility.Export import Transaction
from NLedger.Extensibility.Export import AutomatedTransaction
from NLedger.Extensibility.Export import PeriodicTransaction

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

ANNOTATION_PRICE_CALCULATED = Annotation.ANNOTATION_PRICE_CALCULATED;
ANNOTATION_PRICE_FIXATED = Annotation.ANNOTATION_PRICE_FIXATED;
ANNOTATION_PRICE_NOT_PER_UNIT = Annotation.ANNOTATION_PRICE_NOT_PER_UNIT;
ANNOTATION_DATE_CALCULATED = Annotation.ANNOTATION_DATE_CALCULATED;
ANNOTATION_TAG_CALCULATED = Annotation.ANNOTATION_TAG_CALCULATED;
ANNOTATION_VALUE_EXPR_CALCULATED = Annotation.ANNOTATION_VALUE_EXPR_CALCULATED;

NULL_VALUE = Value.NULL_VALUE

ACCOUNT_NORMAL = Account.ACCOUNT_NORMAL
ACCOUNT_KNOWN = Account.ACCOUNT_KNOWN
ACCOUNT_TEMP = Account.ACCOUNT_TEMP
ACCOUNT_GENERATED = Account.ACCOUNT_GENERATED

ACCOUNT_EXT_SORT_CALC = AccountXData.ACCOUNT_EXT_SORT_CALC
ACCOUNT_EXT_HAS_NON_VIRTUALS = AccountXData.ACCOUNT_EXT_HAS_NON_VIRTUALS
ACCOUNT_EXT_HAS_UNB_VIRTUALS = AccountXData.ACCOUNT_EXT_HAS_UNB_VIRTUALS
ACCOUNT_EXT_AUTO_VIRTUALIZE = AccountXData.ACCOUNT_EXT_AUTO_VIRTUALIZE
ACCOUNT_EXT_VISITED = AccountXData.ACCOUNT_EXT_VISITED
ACCOUNT_EXT_MATCHING = AccountXData.ACCOUNT_EXT_MATCHING
ACCOUNT_EXT_TO_DISPLAY = AccountXData.ACCOUNT_EXT_TO_DISPLAY
ACCOUNT_EXT_DISPLAYED = AccountXData.ACCOUNT_EXT_DISPLAYED

ITEM_NORMAL = JournalItem.ITEM_NORMAL
ITEM_GENERATED = JournalItem.ITEM_GENERATED
ITEM_TEMP = JournalItem.ITEM_TEMP

POST_VIRTUAL = Posting.POST_VIRTUAL
POST_MUST_BALANCE = Posting.POST_MUST_BALANCE
POST_CALCULATED = Posting.POST_CALCULATED
POST_COST_CALCULATED = Posting.POST_COST_CALCULATED

POST_EXT_RECEIVED = PostingXData.POST_EXT_RECEIVED
POST_EXT_HANDLED = PostingXData.POST_EXT_HANDLED
POST_EXT_DISPLAYED = PostingXData.POST_EXT_DISPLAYED
POST_EXT_DIRECT_AMT = PostingXData.POST_EXT_DIRECT_AMT
POST_EXT_SORT_CALC = PostingXData.POST_EXT_SORT_CALC
POST_EXT_COMPOUND = PostingXData.POST_EXT_COMPOUND
POST_EXT_VISITED = PostingXData.POST_EXT_VISITED
POST_EXT_MATCHES = PostingXData.POST_EXT_MATCHES
POST_EXT_CONSIDERED = PostingXData.POST_EXT_CONSIDERED


def string_value(str):
    return Value.string_value(str)

def mask_value(str):
    return Value.mask_value(str)

def value_context(str):
    return Value.value_context(str)

# Session scope attributes and functions

session = SessionScopeAttributes.session

def read_journal(pathName):
    return SessionScopeAttributes.read_journal(pathName)

def read_journal_from_string(data):
    return SessionScopeAttributes.read_journal_from_string(data)

# Times functions

def parse_datetime(str):
    return Times.parse_datetime(str)

def parse_date(str):
    return Times.parse_date(str)

def times_initialize():
    Times.times_initialize()

def times_shutdown():
    Times.times_shutdown()


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
