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

from NLedger.Extensibility.Export import CommodityPool as ExportedCommodityPool
from NLedger.Extensibility.Export import Commodity as ExportedCommodity
from NLedger.Extensibility.Export import KeepDetails
from NLedger.Extensibility.Export import PricePoint
from NLedger.Extensibility.Export import Annotation
from NLedger.Extensibility.Export import AnnotatedCommodity
from NLedger.Extensibility.Export import Amount as ExportedAmount
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

#commodities = ExportedCommodityPool.commodities
COMMODITY_STYLE_DEFAULTS = ExportedCommodityPool.COMMODITY_STYLE_DEFAULTS
COMMODITY_STYLE_SUFFIXED = ExportedCommodityPool.COMMODITY_STYLE_SUFFIXED
COMMODITY_STYLE_SEPARATED = ExportedCommodityPool.COMMODITY_STYLE_SEPARATED
COMMODITY_STYLE_DECIMAL_COMMA = ExportedCommodityPool.COMMODITY_STYLE_DECIMAL_COMMA
COMMODITY_STYLE_TIME_COLON = ExportedCommodityPool.COMMODITY_STYLE_TIME_COLON
COMMODITY_STYLE_THOUSANDS = ExportedCommodityPool.COMMODITY_STYLE_THOUSANDS
COMMODITY_NOMARKET = ExportedCommodityPool.COMMODITY_NOMARKET
COMMODITY_BUILTIN = ExportedCommodityPool.COMMODITY_BUILTIN
COMMODITY_WALKED = ExportedCommodityPool.COMMODITY_WALKED
COMMODITY_KNOWN = ExportedCommodityPool.COMMODITY_KNOWN
COMMODITY_PRIMARY = ExportedCommodityPool.COMMODITY_PRIMARY

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


# Wrappers for NLedger exported classes
# They extend base export definitions with Python-related specifics
# Basically, python code is needed to manage two cases: 
# a) when operation result is an object of .Net class and we need to wrap it up (e.g. overloaded operators like +); 
# b) when an operation parameter is a python type and cannot be implicitly associated with a .Net method (e.g. datetime.datetime vs DateTime)

from NLedger.Amounts import Amount as OriginAmount
from NLedger.Commodities import CommodityPool as OriginCommodityPool
from NLedger.Commodities import Commodity as OriginCommodity

def to_amount(value):
    return value if type(value) == Amount else Amount(value)

# Amounts

class Amount(OriginAmount):

    def exact(value):
        return Amount(OriginAmount.Exact(value))

    def __eq__(self, o: object) -> bool:
        return super().__eq__(to_amount(o))

    def __ne__(self, o: object) -> bool:
        return super().__ne__(to_amount(o))

    def __lt__(self, o: object) -> bool:
        return super().__lt__(to_amount(o))

    def __le__(self, o: object) -> bool:
        return super().__le__(to_amount(o))

    def __gt__(self, o: object) -> bool:
        return super().__gt__(to_amount(o))

    def __ge__(self, o: object) -> bool:
        return super().__ge__(to_amount(o))

    def __neg__(self):
        return Amount(super().__neg__())

    def __bool__(self) -> bool:
        return self.IsNonZero

    __nonzero__ = __bool__

    def __add__(self, o: object):
        return Amount(super().__add__(to_amount(o)))

    def __radd__(self, o: object):
        return Amount(super().__radd__(to_amount(o)))

    def __sub__(self, o: object):
        return Amount(super().__sub__(to_amount(o)))

    def __rsub__(self, o: object):
        return Amount(super().__rsub__(to_amount(o)))

    def __mul__(self, o: object):
        return Amount(super().__mul__(to_amount(o)))

    def __rmul__(self, o: object):
        return Amount(super().__rmul__(to_amount(o)))

    def __truediv__(self, o: object):
        return Amount(super().__truediv__(to_amount(o)))

    def __rtruediv__(self, o: object):
        return Amount(super().__rtruediv__(to_amount(o)))

    @property
    def precision(self):
        return super().Precision

    @property
    def display_precision(self):
        return super().DisplayPrecision

    @property
    def keep_precision(self):
        return super().KeepPrecision

    @keep_precision.setter
    def keep_precision(self, value):
        return super().SetKeepPrecision(value)

    def negated(self):
        return Amount(self.Negated())

    def in_place_negate(self):
        self.InPlaceNegate()
        return self

    def abs(self):
        return Amount(self.Abs())

    __abs__ = abs

    def inverted(self):
        return Amount(self.Inverted())

    def rounded(self):
        return Amount(self.Rounded())

    def in_place_round(self):
        self.InPlaceRound()
        return self

    def truncated(self):
        return Amount(self.Truncated())

    def in_place_truncate(self):
        self.InPlaceTruncate()
        return self

    def floored(self):
        return Amount(self.Floored())

    def in_place_floor(self):
        self.InPlaceFloor()
        return self

    def unrounded(self):
        return Amount(self.Unrounded())

    def in_place_unround(self):
        self.InPlaceUnround()
        return self

    def reduced(self):
        return Amount(self.Reduced())

    def in_place_reduce(self):
        self.InPlaceReduce()
        return self

    def unreduced(self):
        return Amount(self.Unreduced())

    def in_place_unreduce(self):
        self.InPlaceUnreduce()
        return self

    # TBC

# Commodities

class CommodityPool:

    origin: None

    def __init__(self,origin) -> None:
        assert isinstance(origin, OriginCommodityPool)
        self.origin = origin

    @property
    def null_commodity(self):
        return Commodity(self.origin.NullCommodity)

    @property
    def default_commodity(self):
        return Commodity.get_or_none(self.origin.DefaultCommodity)

    @default_commodity.setter
    def set_default_commodity(self, value):
        self.origin.DefaultCommodity = value

    @property
    def keep_base(self):
        return self.origin.KeepBase

    @keep_base.setter
    def keep_base(self, value):
        self.origin.KeepBase = value

    @property
    def price_db(self):
        return self.origin.PriceDb

    @price_db.setter
    def price_db(self, value):
        self.origin.PriceDb = value

    @property
    def quote_leeway(self):
        return self.origin.QuoteLeeway

    @quote_leeway.setter
    def quote_leeway(self, value):
        self.origin.QuoteLeeway = value

    @property
    def get_quotes(self):
        return self.origin.GetQuotes

    @get_quotes.setter
    def get_quotes(self, value):
        self.origin.GetQuotes = value

    def create(self, symbol):
        return Commodity(self.origin.Create(symbol))

    # TBC

commodities = CommodityPool(OriginCommodityPool.Current)

class Commodity:

    origin: None

    def get_or_none(origin):
        return Commodity(origin) if origin != None else None

    def __init__(self,origin) -> None:
        assert isinstance(origin, OriginCommodity)
        self.origin = origin

    @property
    def symbol(self):
        return self.origin.Symbol

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
