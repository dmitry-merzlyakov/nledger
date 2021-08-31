from typing import Iterable, List, Tuple
import typing
import ledger.conf
from ledger.conf import clrRuntime
from ledger.conf import ClrRuntime

from clr_loader import get_coreclr
from clr_loader import get_mono
from clr_loader import get_netfx

from pythonnet import set_runtime

# Static property helper

class classproperty(property):
    def __get__(self, cls, owner):
        return classmethod(self.fget).__get__(None, owner)()

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
from NLedger.Extensibility.Export import KeepDetails as ExportedKeepDetails
from NLedger.Extensibility.Export import PricePoint as ExportedPricePoint
from NLedger.Extensibility.Export import Annotation as ExportedAnnotation
from NLedger.Extensibility.Export import AnnotatedCommodity
from NLedger.Extensibility.Export import Amount as ExportedAmount
from NLedger.Extensibility.Export import ParseFlags
from NLedger.Extensibility.Export import ValueType
from NLedger.Extensibility.Export import Value as ExportedValue
from NLedger.Extensibility.Export import ValueType as ExportedValueType
from NLedger.Extensibility.Export import Account as ExportedAccount
from NLedger.Extensibility.Export import AccountXData as ExportedAccountXData
from NLedger.Extensibility.Export import AccountXDataDetails as ExportedAccountXDataDetails
from NLedger.Extensibility.Export import Balance as ExportedBalance
from NLedger.Extensibility.Export import Expr
from NLedger.Extensibility.Export import FileInfo
from NLedger.Extensibility.Export import Position as ExportedPosition
from NLedger.Extensibility.Export import Journal
from NLedger.Extensibility.Export import JournalItem
from NLedger.Extensibility.Export import State as ExportedState 
from NLedger.Extensibility.Export import Mask as ExportedMask
from NLedger.Extensibility.Export import Posting as ExportedPosting
from NLedger.Extensibility.Export import PostingXData as ExportedPostingXData
from NLedger.Extensibility.Export import SymbolKind as ExportedSymbolKind
from NLedger.Extensibility.Export import Session
from NLedger.Extensibility.Export import SessionScopeAttributes
from NLedger.Extensibility.Export import SortValue
from NLedger.Extensibility.Export import Times
from NLedger.Extensibility.Export import Transaction as ExportedTransaction
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

ANNOTATION_PRICE_CALCULATED = ExportedAnnotation.ANNOTATION_PRICE_CALCULATED
ANNOTATION_PRICE_FIXATED = ExportedAnnotation.ANNOTATION_PRICE_FIXATED
ANNOTATION_PRICE_NOT_PER_UNIT = ExportedAnnotation.ANNOTATION_PRICE_NOT_PER_UNIT
ANNOTATION_DATE_CALCULATED = ExportedAnnotation.ANNOTATION_DATE_CALCULATED
ANNOTATION_TAG_CALCULATED = ExportedAnnotation.ANNOTATION_TAG_CALCULATED
ANNOTATION_VALUE_EXPR_CALCULATED = ExportedAnnotation.ANNOTATION_VALUE_EXPR_CALCULATED

ACCOUNT_NORMAL = ExportedAccount.ACCOUNT_NORMAL
ACCOUNT_KNOWN = ExportedAccount.ACCOUNT_KNOWN
ACCOUNT_TEMP = ExportedAccount.ACCOUNT_TEMP
ACCOUNT_GENERATED = ExportedAccount.ACCOUNT_GENERATED

ACCOUNT_EXT_SORT_CALC = ExportedAccountXData.ACCOUNT_EXT_SORT_CALC
ACCOUNT_EXT_HAS_NON_VIRTUALS = ExportedAccountXData.ACCOUNT_EXT_HAS_NON_VIRTUALS
ACCOUNT_EXT_HAS_UNB_VIRTUALS = ExportedAccountXData.ACCOUNT_EXT_HAS_UNB_VIRTUALS
ACCOUNT_EXT_AUTO_VIRTUALIZE = ExportedAccountXData.ACCOUNT_EXT_AUTO_VIRTUALIZE
ACCOUNT_EXT_VISITED = ExportedAccountXData.ACCOUNT_EXT_VISITED
ACCOUNT_EXT_MATCHING = ExportedAccountXData.ACCOUNT_EXT_MATCHING
ACCOUNT_EXT_TO_DISPLAY = ExportedAccountXData.ACCOUNT_EXT_TO_DISPLAY
ACCOUNT_EXT_DISPLAYED = ExportedAccountXData.ACCOUNT_EXT_DISPLAYED

ITEM_NORMAL = JournalItem.ITEM_NORMAL
ITEM_GENERATED = JournalItem.ITEM_GENERATED
ITEM_TEMP = JournalItem.ITEM_TEMP

POST_VIRTUAL = ExportedPosting.POST_VIRTUAL
POST_MUST_BALANCE = ExportedPosting.POST_MUST_BALANCE
POST_CALCULATED = ExportedPosting.POST_CALCULATED
POST_COST_CALCULATED = ExportedPosting.POST_COST_CALCULATED

POST_EXT_RECEIVED = ExportedPostingXData.POST_EXT_RECEIVED
POST_EXT_HANDLED = ExportedPostingXData.POST_EXT_HANDLED
POST_EXT_DISPLAYED = ExportedPostingXData.POST_EXT_DISPLAYED
POST_EXT_DIRECT_AMT = ExportedPostingXData.POST_EXT_DIRECT_AMT
POST_EXT_SORT_CALC = ExportedPostingXData.POST_EXT_SORT_CALC
POST_EXT_COMPOUND = ExportedPostingXData.POST_EXT_COMPOUND
POST_EXT_VISITED = ExportedPostingXData.POST_EXT_VISITED
POST_EXT_MATCHES = ExportedPostingXData.POST_EXT_MATCHES
POST_EXT_CONSIDERED = ExportedPostingXData.POST_EXT_CONSIDERED


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

from NLedger.Extensibility.Export import FlagsAdapter
from NLedger.Amounts import Amount as OriginAmount
from NLedger.Commodities import CommodityPool as OriginCommodityPool
from NLedger.Commodities import Commodity as OriginCommodity
from NLedger.Annotate import AnnotatedCommodity as OriginAnnotatedCommodity
from NLedger.Commodities import PricePoint as OriginPricePoint
from NLedger.Commodities import CommodityFlagsEnum
from NLedger.Annotate import Annotation as OriginAnnotation
from NLedger.Annotate import AnnotationKeepDetails as OriginAnnotationKeepDetails
from NLedger.Accounts import Account as OriginAccount
from NLedger.Scopus import SymbolKindEnum as SymbolKind
from NLedger.Scopus import Scope as OriginScope
from NLedger.Items import Item as OriginItem
from NLedger.Values import ValueTypeEnum as ValueType
from NLedger.Values import Value as OriginValue
from NLedger import Balance as OriginBalance
from NLedger import Mask as OriginMask
from NLedger.Items import ItemPosition as OriginItemPosition
from NLedger.Items import ItemStateEnum as State
from NLedger import Post as OriginPost
from NLedger import PostXData as OriginPostXData
from NLedger import SupportsFlagsEnum as OriginSupportsFlagsEnum
from NLedger.Xacts import XactBase as OriginXactBase
from NLedger.Xacts import Xact as OriginXact
from NLedger.Xacts import PeriodXact as OriginPeriodXact
from NLedger.Xacts import AutoXact as OriginAutoXact


# Manage date conversions

from datetime import datetime
from datetime import date
from collections.abc import MutableSequence

from NLedger.Utility import Date
from System import DateTime
from System.Globalization import DateTimeStyles
from System.Collections.Generic import List as NetList
from System import Tuple as NetTuple
from NLedger.Extensibility.Export import ListAdapter as NetListAdapter

from NLedger.Times import TimesCommon

# Converts to Python date
def to_pdate(value) -> date:
    if value is None:
        return None
    elif isinstance(value, DateTime):
        return date(value.Year, value.Month, value.Day)
    elif isinstance(value, Date):
        return date(value.Year, value.Month, value.Day)
    elif isinstance(value, datetime):
        return value.date()
    elif isinstance(value, date):
        return value
    else:
        raise Exception("Date value is expected")

# Converts to Python datetime
def to_pdatetime(value) -> datetime:
    if value is None:
        return None
    elif isinstance(value, DateTime):
        return datetime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Millisecond * 1000)
    elif isinstance(value, Date):
        return datetime(value.Year, value.Month, value.Day, 0, 0, 0, 0)
    elif isinstance(value, datetime):
        return value
    elif isinstance(value, date):
        return datetime.combine(value, datetime.min.time())
    else:
        raise Exception("Date value is expected")

# Converts to .Net Ledger Date
def to_ndate(value) -> Date:
    if value is None:
        return None
    elif isinstance(value, DateTime):
        return Date(value.Year, value.Month, value.Day)
    elif isinstance(value, Date):
        return value
    elif isinstance(value, datetime):
        return Date(value.year, value.month, value.day)
    elif isinstance(value, date):
        return Date(value.year, value.month, value.day)
    else:
        raise Exception("Date value is expected")

# Converts to .Net DateTime
def to_ndatetime(value) -> DateTime:
    if value is None:
        return None
    elif isinstance(value, DateTime):
        return value
    elif isinstance(value, Date):
        return DateTime(value.Year, value.Month, value.Day)
    elif isinstance(value, datetime):
        return DateTime(value.year, value.month, value.day, value.hour, value.minute, value.second, value.microsecond // 1000)
    elif isinstance(value, date):
        return DateTime(value.year, value.month, value.day)
    else:
        raise Exception("Date value is expected")

# .Net List wrapper

class NList(MutableSequence):

    origin = None
    nclass = None

    def __init__(self, origin = None) -> None:
        super(NList, self).__init__()

        self.nclass = self.get_nclass()
        assert isinstance(self.nclass, type)

        if origin is None:
            self.origin = self.nclass()
        elif isinstance(origin,Iterable):
            self.origin = self.nclass()
            for item in origin:
                self.append(item)
        else:
            assert isinstance (origin, self.nclass)
            self.origin = origin

    def get_nclass(self) -> type:
        raise Exception("This method should return .Net class for expected origin")

    def to_nitem(self, item):
        raise Exception("This method should convert a Python item to .Net item")

    def to_pitem(self, item):
        raise Exception("This method should convert a .Net item to Python item")

    def __repr__(self):
        return "<{0} {1}>".format(self.__class__.__name__, self.origin)

    def __len__(self):
        return self.origin.Count

    def __getitem__(self, row):
        item = self.origin[row] if row < self.origin.Count else None
        return self.to_pitem(item) if not item is None else None

    def __delitem__(self, row):
        self.origin.RemoveAt(row)

    def __setitem__(self, row, item):
        item = self.to_nitem(item) if not item is None else None
        self.origin[row] = item

    def __str__(self):
        return str(self.origin)

    def insert(self, row, item):
        item = self.to_nitem(item) if not item is None else None
        self.origin.Insert(row, item)

    def append(self, item):
        item = self.to_nitem(item) if not item is None else None
        self.origin.Add(item)

# List of Values

class ValueList(NList):
    def __init__(self, origin = None) -> None:
        super().__init__(origin=origin)

    def get_nclass(self) -> type:
        return NetListAdapter[OriginValue]

    def to_nitem(self, item):
        return Value.to_value(item).origin

    def to_pitem(self, item):
        return Value.to_value(item)

class SortValueList(NList):
    def __init__(self, origin = None) -> None:
        super().__init__(origin=origin)

    def get_nclass(self) -> type:
        return NetListAdapter[NetTuple[OriginValue,bool]]

    def to_nitem(self, item):
        (value, inverted) = item
        return NetTuple[OriginValue,bool](Value.to_value(value).origin,inverted)

    def to_pitem(self, item):
        return (Value.to_value(item.Item1), item.Item2)


# Amounts

class Amount:

    origin = None

    def __init__(self,value,origin = None) -> None:
        if not (origin is None):
            assert isinstance(origin, OriginAmount)
            self.origin = origin
        else:
            self.origin = OriginAmount(value)

    @classmethod
    def from_origin(cls, origin) -> 'Amount':        
        return Amount(None, origin) if not origin is None else None

    @classmethod
    def to_amount(cls, value) -> 'Amount':
        if isinstance(value, Amount):
            return value
        if isinstance(value, Value):
            return value.to_amount()
        return Amount(value)

    @classmethod
    def exact(cls, value) -> 'Amount':
        return Amount.from_origin(OriginAmount.Exact(value))

    def __eq__(self, o: object) -> bool:
        return self.origin == Amount.to_amount(o).origin

    def __ne__(self, o: object) -> bool:
        return self.origin != Amount.to_amount(o).origin

    def __lt__(self, o: object) -> bool:
        return self.origin < Amount.to_amount(o).origin

    def __le__(self, o: object) -> bool:
        return self.origin <= Amount.to_amount(o).origin

    def __gt__(self, o: object) -> bool:
        return self.origin > Amount.to_amount(o).origin

    def __ge__(self, o: object) -> bool:
        return self.origin >= Amount.to_amount(o).origin

    def __neg__(self) -> 'Amount':
        return Amount.from_origin(-self.origin)

    def __bool__(self) -> bool:
        return self.origin.IsNonZero

    __nonzero__ = __bool__

    def __add__(self, o: object) -> 'Amount':
        return Amount.from_origin(self.origin + Amount.to_amount(o).origin)

    def __radd__(self, o: object) -> 'Amount':
        return Amount.from_origin(Amount.to_amount(o).origin + self.origin)

    def __sub__(self, o: object) -> 'Amount':
        return Amount.from_origin(self.origin - Amount.to_amount(o).origin)

    def __rsub__(self, o: object) -> 'Amount':
        return Amount.from_origin(Amount.to_amount(o).origin - self.origin)

    def __mul__(self, o: object) -> 'Amount':
        return Amount.from_origin(self.origin * Amount.to_amount(o).origin)

    def __rmul__(self, o: object) -> 'Amount':
        return Amount.from_origin(Amount.to_amount(o).origin * self.origin)

    def __truediv__(self, o: object) -> 'Amount':
        return Amount.from_origin(self.origin / Amount.to_amount(o).origin)

    def __rtruediv__(self, o: object) -> 'Amount':
        return Amount.from_origin(Amount.to_amount(o).origin / self.origin)

    @property
    def precision(self) -> int:
        return self.origin.Precision

    @property
    def display_precision(self) -> int:
        return self.origin.DisplayPrecision

    @property
    def keep_precision(self) -> bool:
        return self.origin.KeepPrecision

    @keep_precision.setter
    def keep_precision(self, value: bool):
        return self.origin.SetKeepPrecision(value)

    def negated(self) -> 'Amount':
        return Amount.from_origin(self.origin.Negated())

    def in_place_negate(self) -> 'Amount':
        self.origin.InPlaceNegate()
        return self

    def abs(self) -> 'Amount':
        return Amount.from_origin(self.origin.Abs())

    __abs__ = abs

    def inverted(self) -> 'Amount':
        return Amount.from_origin(self.origin.Inverted())

    def rounded(self) -> 'Amount':
        return Amount.from_origin(self.origin.Rounded())

    def in_place_round(self) -> 'Amount':
        self.origin.InPlaceRound()
        return self

    def truncated(self) -> 'Amount':
        return Amount.from_origin(self.origin.Truncated())

    def in_place_truncate(self) -> 'Amount':
        self.origin.InPlaceTruncate()
        return self

    def floored(self) -> 'Amount':
        return Amount.from_origin(self.origin.Floored())

    def in_place_floor(self) -> 'Amount':
        self.origin.InPlaceFloor()
        return self

    def unrounded(self) -> 'Amount':
        return Amount.from_origin(self.origin.Unrounded())

    def in_place_unround(self) -> 'Amount':
        self.origin.InPlaceUnround()
        return self

    def reduced(self) -> 'Amount':
        return Amount.from_origin(self.origin.Reduced())

    def in_place_reduce(self) -> 'Amount':
        self.origin.InPlaceReduce()
        return self

    def unreduced(self) -> 'Amount':
        return Amount.from_origin(self.origin.Unreduced())

    def in_place_unreduce(self) -> 'Amount':
        self.origin.InPlaceUnreduce()
        return self

    @property
    def commodity(self) -> 'Commodity':
        return Commodity.from_origin(self.origin.Commodity)

    @commodity.setter
    def commodity(self, value: 'Commodity'):
        self.origin.Commodity = value.origin if not value is None else None

    def to_string(self) -> str:
        return self.origin.ToString()

    __str__ =  to_string

    # TBC

    @property
    def annotation(self) -> 'Annotation':
        return Annotation.from_origin(self.origin.Annotation)


class Balance:

    origin = None

    def __init__(self, val) -> None:
        if isinstance(val,Balance) or isinstance(val,Amount):
            val = val.origin
        if isinstance(val, OriginBalance):
            self.origin = val
        else:
            self.origin = OriginBalance(val)

    @classmethod
    def from_origin(cls, origin) -> 'Balance':
        return Balance(origin) if not origin is None else None

    # TBC

# Commodities

class CommodityPool:

    origin: None

    def __init__(self,origin) -> None:
        assert isinstance(origin, OriginCommodityPool)
        self.origin = origin

    @property
    def null_commodity(self):
        return Commodity.from_origin(self.origin.NullCommodity)

    @property
    def default_commodity(self):
        return Commodity.from_origin(self.origin.DefaultCommodity)

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

    def create(self, symbol: str, details: 'Annotation' = None):
        assert details is None or isinstance(details, Annotation)
        return Commodity.from_origin(self.origin.Create(symbol) if details is None else self.origin.Create(symbol, details.origin))

    def find_or_create(self, symbol: str, details = None):
        assert details is None or isinstance(details, Annotation)
        return Commodity.from_origin(self.origin.FindOrCreate(symbol) if details is None else self.origin.FindOrCreate(symbol, details.origin))

    def find(self, symbol: str, details = None):
        assert details is None or isinstance(details, Annotation)
        return Commodity.from_origin(self.origin.Find(symbol) if details is None else self.origin.Find(symbol, details.origin))

    # Allowed arguments for 'exchange' method: 
    # exchange(commodity: Commodity, per_unit_cost: Amount)
    # exchange(commodity: Commodity, per_unit_cost: Amount, moment: datetime)
    # exchange(amount: Amount, cost: Amount, is_per_unit: bool, add_prices: bool, moment: datetime = None, tag: str = None)
    def exchange(self,*args):
        if len(args) == 2:
            assert isinstance(args[0], Commodity)
            assert isinstance(args[1], Amount)
            self.origin.Exchange(args[0].origin, args[1].origin, TimesCommon.Current.CurrentTime)
        elif len(args) == 3:
            assert isinstance(args[0], Commodity)
            assert isinstance(args[1], Amount)
            assert isinstance(args[2], datetime) or isinstance(args[2], date)
            self.origin.Exchange(args[0].origin, args[1].origin, to_ndatetime(args[2]))
        elif len(args) >= 4 and len(args) <= 6:
            assert isinstance(args[0], Amount)
            assert isinstance(args[1], Amount)
            assert isinstance(args[2], bool)
            assert isinstance(args[3], bool)
            assert len(args) < 5 or (isinstance(args[4], datetime) or isinstance(args[4], date))
            assert len(args) < 6 or isinstance(args[5], str)
            self.origin.Exchange(args[0].origin, args[1].origin, args[2], args[3], to_ndatetime(args[4]) if len(args) >= 5 else None, args[5] if len(args) == 6 else None)
        else:
            raise Exception("Unexpected number of 'exchange' arguments (allowed from 2 to 6")

    def parse_price_directive(self, line: str, do_not_add_price: bool = False, no_date: bool = False):
        pair = self.origin.ParsePriceDirective(line, do_not_add_price, no_date)
        return (Commodity.from_origin(pair.Item1), PricePoint.from_origin(pair.Item2)) if not pair is None else None

    def parse_price_expression(self, line: str, add_price: bool = True, moment: datetime = None):
        return Commodity.from_origin(self.origin.ParsePriceExpression(line, add_price, to_ndatetime(moment)))

    def __getitem__(self, symbol):
        comm = Commodity.from_origin(self.origin.Find(symbol))
        if comm is None:
            raise ValueError("Could not find commodity " + str(symbol))
        return comm

    def keys(self):
        return list(self.origin.Commodities.Keys)

    def has_key(self, symbol:str) -> bool:
        return not self.origin.Find(symbol) is None

    __contains__ = has_key

    def values(self):
        return [Commodity.from_origin(comm) for comm in self.origin.Commodities.Values]

    def items(self):
        return [(kv.Key, Commodity.from_origin(kv.Value)) for kv in self.origin.Commodities]

    def __iter__(self):
        return iter(self.keys())

commodities = CommodityPool(OriginCommodityPool.Current)

class Annotation:

    origin: None
    flags_adapter = FlagsAdapter.AnnotationFlagsAdapter()

    def __init__(self,origin) -> None:
        assert isinstance(origin, OriginAnnotation)
        self.origin = origin

    @classmethod
    def from_origin(cls, origin) -> 'Annotation':
        return Annotation(origin=origin) if not origin is None else None

    def __eq__(self, o: object) -> bool:
        return self.origin.Equals(o.origin)

    def __ne__(self, o: object) -> bool:
        return not self.origin.Equals(o.origin)

    def __bool__(self) -> bool:
        return not OriginAnnotation.IsNullOrEmpty(self.origin)

    __nonzero__ = __bool__

    @property
    def flags(self):
        return self.flags_adapter.GetFlags(self.origin)

    @flags.setter
    def flags(self,value):
        return self.flags_adapter.SetFlags(self.origin, value)

    def has_flags(self,flag):
        return self.flags_adapter.HasFlags(self.origin, flag)

    def clear_flags(self):
        return self.flags_adapter.ClearFlags(self.origin)

    def add_flags(self,flag):
        return self.flags_adapter.AddFlags(self.origin,flag)

    def drop_flags(self,flag):
        return self.flags_adapter.DropFlags(self.origin,flag)

    @property
    def price(self) -> Amount:
        return Amount.from_origin(self.origin.Price)

    @price.setter
    def price(self, value: Amount):
        self.origin.Price = value.origin if not value is None else None

    @property
    def date(self):
        return to_pdate(self.origin.Date)

    @date.setter
    def date(self,value):
        self.origin.Date = to_ndate(value)

    @property
    def tag(self):
        return self.origin.Tag

    @tag.setter
    def tag(self,value):
        self.origin.Tag = value

    def valid(self) -> bool:
        return True # valid() is not implemented in NLedger's annotation

class KeepDetails:

    origin: None

    def __init__(self, keepPrice = False, keepDate = False, keepTag = False, onlyActuals = False, origin = None) -> None:

        if not(origin is None):
            assert isinstance(origin, OriginAnnotationKeepDetails)
            self.origin = origin
        else:
            self.origin = OriginAnnotationKeepDetails(keepPrice, keepDate, keepTag, onlyActuals)

    @classmethod
    def from_origin(cls, origin):
        return KeepDetails(origin=origin) if not origin is None else None

    @property
    def keep_price(self) -> bool:
        return self.origin.KeepPrice

    @keep_price.setter
    def keep_price(self,value):
        self.origin.KeepPrice = value

    @property
    def keep_date(self) -> bool:
        return self.origin.KeepDate

    @keep_date.setter
    def keep_date(self,value):
        self.origin.KeepDate = value

    @property
    def keep_tag(self) -> bool:
        return self.origin.KeepTag

    @keep_tag.setter
    def keep_tag(self,value):
        self.origin.KeepTag = value

    @property
    def only_actuals(self) -> bool:
        return self.origin.OnlyActuals

    @only_actuals.setter
    def only_actuals(self,value):
        self.origin.OnlyActuals = value

    def keep_all(self, comm = None):
        return self.origin.KeepAll() if comm is None else self.origin.KeepAll(comm.origin)

    def keep_any(self, comm = None):
        return self.origin.KeepAny() if comm is None else self.origin.KeepAny(comm.origin)

class PricePoint:

    origin: None

    def __init__(self, when, price, origin = None) -> None:

        if not(origin is None):
            assert isinstance(origin, OriginPricePoint)
            self.origin = origin
        else:
            assert price is None or isinstance(price, Amount)
            self.origin = OriginPricePoint(to_ndatetime(when), price.origin if not price is None else None)

    @classmethod
    def from_origin(cls, origin):
        return PricePoint(None, None, origin) if not origin is None else None

    def __eq__(self, o: object) -> bool:
        return self.origin.Equals(o.origin if isinstance(o, PricePoint) else o)

    def __ne__(self, o: object) -> bool:
        return not self.origin.Equals(o.origin if isinstance(o, PricePoint) else o)

    @property
    def when(self) -> datetime:
        return to_pdatetime(self.origin.When)

    @when.setter
    def when(self,value):
        self.origin.When = to_ndatetime(value)

    @property
    def price(self) -> Amount:
        return Amount.from_origin(self.origin.Price)

    @price.setter
    def price(self, value: Amount):
        assert value is None or isinstance(value, Amount)
        self.origin.Price = value.origin if not value is None else None

class Commodity:

    origin: None

    def __init__(self,origin) -> None:
        assert isinstance(origin, OriginCommodity)
        self.origin = origin

    @classmethod
    def from_origin(cls, origin) -> 'Commodity':
        if origin is None:
            return None

        if isinstance(origin, OriginAnnotatedCommodity):
            return AnnotatedCommodity(origin)

        return Commodity(origin)

    @classproperty
    def decimal_comma_by_default(cls) -> bool:
        return OriginCommodity.Defaults.DecimalCommaByDefault

    @decimal_comma_by_default.setter
    def decimal_comma_by_default(cls, value: bool):
        OriginCommodity.Defaults.DecimalCommaByDefault = value

    def __eq__(self, o: object) -> bool:
        return self.origin.Equals(o.origin if isinstance(o, Commodity) else o)

    def __ne__(self, o: object) -> bool:
        return not self.origin.Equals(o.origin if isinstance(o, Commodity) else o)

    @property
    def flags(self) -> int:
        return FlagsAdapter.CommodityFlagsToInt(self.origin.Flags)

    @flags.setter
    def flags(self, value:int):
        self.origin.Flags = CommodityFlagsEnum(value)

    def has_flags(self, value:int) -> bool:
        return (self.flags & value) == value

    def clear_flags(self):
        self.flags = 0

    def add_flags(self, value:int):
        self.flags |= value

    def drop_flags(self, value:int):
        self.flags &= ~value

    def __str__(self) -> str:
        return self.origin.Symbol

    def __bool__(self) -> bool:
        return not self.origin.Equals(OriginCommodityPool.Current.NullCommodity)

    __nonzero__ = __bool__

    @staticmethod
    def symbol_needs_quotes(symbol: str) -> bool:
        return OriginCommodity.SymbolNeedsQuotes(symbol)

    @property
    def referent(self) -> 'Commodity':
        return Commodity.from_origin(self.origin.Referent)

    def has_annotation(self) -> bool:
        return self.origin.IsAnnotated

    def strip_annotations(self, keep: KeepDetails = None) -> 'Commodity':
        return Commodity.from_origin(self.origin.StripAnnotations(keep.origin if not keep is None else KeepDetails().origin))

    def write_annotations(self) -> str:
        return self.origin.WriteAnnotations()

    def pool(self) -> CommodityPool:
        return CommodityPool(self.origin.Pool)

    @property
    def base_symbol(self) -> str:
        return self.origin.BaseSymbol

    @property
    def symbol(self) -> str:
        return self.origin.Symbol

    @property
    def name(self) -> str:
        return self.origin.Name

    @name.setter
    def name(self, value: str):
        self.origin.SetName(value)

    @property
    def note(self) -> str:
        return self.origin.Note

    @note.setter
    def note(self, value: str):
        self.origin.SetNote(value)

    @property
    def precision(self) -> int:
        return self.origin.Precision

    @precision.setter
    def precision(self, value: int):
        self.origin.Precision = value

    @property
    def smaller(self) -> Amount:
        return Amount.from_origin(self.origin.Smaller)

    @smaller.setter
    def smaller(self, value: Amount):
        self.origin.Smaller = value.origin if not value is None else None

    @property
    def larger(self) -> Amount:
        return Amount.from_origin(self.origin.Larger)

    @larger.setter
    def larger(self, value: Amount):
        self.origin.Larger = value.origin if not value is None else None

    def add_price(self, date, price: Amount, reflexive: bool = None):
        if (reflexive is None):
            self.origin.AddPrice(to_ndatetime(date), price.origin)
        else:
            self.origin.AddPrice(to_ndatetime(date), price.origin, reflexive)

    def remove_price(self, date, commodity: 'Commodity'):
        self.origin.RemovePrice(to_ndatetime(date), commodity.origin)

    def find_price(self, commodity: 'Commodity' = None, moment = None, oldest = None) -> PricePoint:
        PricePoint.from_origin(self.origin.FindPrice(commodity.origin if not commodity is None else None, to_ndatetime(moment) if not moment is None else DateTime.MinValue, to_ndatetime(oldest) if not oldest is None else DateTime.MinValue))

    def check_for_updated_price(self, point: 'PricePoint' = None, moment = None, inTermsOf: 'Commodity' = None) -> PricePoint:
        PricePoint.from_origin(self.origin.CheckForUpdatedPrice(point.origin if not point is None else None, to_ndatetime(moment) if not moment is None else DateTime.MinValue, inTermsOf.origin if not inTermsOf is None else None))

    def valid(self):
        self.origin.Valid()

class AnnotatedCommodity(Commodity):

    origin: None

    def __init__(self,origin) -> None:
        assert isinstance(origin, OriginAnnotatedCommodity)
        self.origin = origin

    @classmethod
    def from_origin(cls, origin) -> 'AnnotatedCommodity':
        return Commodity(origin=origin) if not origin is None else None

    @property
    def details(self) -> Annotation:
        return Annotation.from_origin(self.origin.Details)

    @details.setter
    def details(self, value: Annotation) -> Annotation:
        return self.origin.SetDetails(value.origin)

# Accounts

class Account:

    origin: None
    flags_adapter = FlagsAdapter.AccountFlagsAdapter()

    def __init__(self, parent: 'Account' = None, name: str = None, note: str = None, origin = None) -> None:

        if not(origin is None):
            assert isinstance(origin, OriginAccount)
            self.origin = origin
        else:
            self.origin = OriginAccount(parent.origin if not parent is None else None, name, note)

    @classmethod
    def from_origin(cls, origin):
        return Account(origin=origin) if not origin is None else None

    @property
    def flags(self):
        return self.flags_adapter.GetFlags(self.origin)

    @flags.setter
    def flags(self,value):
        return self.flags_adapter.SetFlags(self.origin, value)

    def has_flags(self,flag):
        return self.flags_adapter.HasFlags(self.origin, flag)

    def clear_flags(self):
        return self.flags_adapter.ClearFlags(self.origin)

    def add_flags(self,flag):
        return self.flags_adapter.AddFlags(self.origin,flag)

    def drop_flags(self,flag):
        return self.flags_adapter.DropFlags(self.origin,flag)

    @property
    def parent(self) -> 'Account':
        return Account.from_origin(self.origin.Parent)

    @property
    def name(self) -> str:
        return self.origin.Name

    @property
    def note(self) -> str:
        return self.origin.Note

    @property
    def depth(self) -> int:
        return self.origin.Depth

    def __str__(self) -> str:
        return self.origin.ToString()

    def fullname(self) -> str:
        return self.origin.FullName

    def partial_name(self, flat: bool = None) -> str:
        return self.origin.PartialName(flat) if flat else self.origin.PartialName()

    def add_account(self, account: 'Account'):
        self.origin.AddAccount(account.origin)

    def remove_account(self, account: 'Account'):
        self.origin.RemoveAccount(account.origin)

    def find_account(self, acctname: str, auto_create: bool = None) -> 'Account':
        return Account.from_origin(self.origin.FindAccount(acctname, auto_create) if not auto_create is None else self.origin.FindAccount(acctname))

    def find_account_re(self, regexp: str) -> 'Account':
        return Account.from_origin(self.origin.FindAccountRe(regexp))

    # TBC

# Scope

class Scope:

    origin = None

    def __init__(self,origin) -> None:
        assert isinstance(origin, OriginScope)
        self.origin = origin

    @property
    def description(self) -> str:
        return self.origin.Description

    @property
    def type_context(self) -> ValueType:
        return self.origin.TypeContext

    @property
    def type_required(self) -> bool:
        return self.origin.TypeRequired

class Position:

    origin: None

    def __init__(self, origin = None) -> None:
        if origin is None:
            self.origin = OriginItemPosition()
        else:
            assert isinstance(origin, OriginItemPosition)
            self.origin = origin

    @classmethod
    def from_origin(cls, origin) -> 'Position':
        if origin is None:
            return None

        return Position(origin)

    @property
    def pathname(self) -> str:
        return self.origin.PathName

    @pathname.setter
    def pathname(self, val: str):
        self.origin.PathName = val

    @property
    def beg_pos(self) -> int:
        return self.origin.BegPos

    @beg_pos.setter
    def beg_pos(self, val: int):
        self.origin.BegPos = val

    @property
    def beg_line(self) -> int:
        return self.origin.BegLine

    @beg_line.setter
    def beg_line(self, val: int):
        self.origin.BegLine = val

    @property
    def end_pos(self) -> int:
        return self.origin.EndPos

    @end_pos.setter
    def end_pos(self, val: int):
        self.origin.EndPos = val

    @property
    def end_line(self) -> int:
        return self.origin.EndLine

    @end_line.setter
    def end_line(self, val: int):
        self.origin.EndLine = val

class JournalItem(Scope):

    def __init__(self, origin) -> None:
        assert isinstance(origin, OriginItem)
        super().__init__(origin)

    @property
    def flags(self) -> int:
        return FlagsAdapter.SupportsFlagsToInt(self.origin.Flags)

    @flags.setter
    def flags(self, value:int):
        self.origin.Flags = OriginSupportsFlagsEnum(value)

    def has_flags(self, value:int) -> bool:
        return (self.flags & value) == value

    def clear_flags(self):
        self.flags = 0

    def add_flags(self, value:int):
        self.flags |= value

    def drop_flags(self, value:int):
        self.flags &= ~value

    @property
    def note(self) -> str:
        return self.origin.Note

    @note.setter
    def note(self, val: str):
        self.origin.Note = val

    @property
    def pos(self) -> Position:
        return Position(self.origin.Pos)

    @pos.setter
    def pos(self, val: Position):
        self.origin.Pos = val.origin if not val is None else None

    @property
    def metadata(self) -> typing.Dict[str, Tuple]:
        result = {}
        data = self.origin.GetMetadata()
        if not data is None:
            for key_value in data:
                result[key_value.Key] = (Value.to_value(key_value.Value.Value), bool(key_value.Value.IsParsed)) if not key_value.Value is None else None
        return result

    def copy_details(self, item: JournalItem):
        assert isinstance(item, JournalItem)
        self.origin.CopyDetails(item.origin)

    def __eq__(self, o: object) -> bool:
        if o is None:
            return False
        assert isinstance(o, JournalItem)
        return self.origin == o.origin

    def __ne__(self, o: object) -> bool:
        if o is None:
            return False
        assert isinstance(o, JournalItem)
        return self.origin != o.origin

    def has_tag(self, tag, val = None) -> bool:
        if isinstance(tag, str) and val is None:
            return self.origin.HasTag(tag)
        assert isinstance(tag, Mask)
        if val is None:
            return self.origin.HasTag(tag.origin)
        assert isinstance(val, Mask)
        return self.origin.HasTag(tag.origin, val.origin)

    def get_tag(self, tag, val = None) -> 'Value':
        if isinstance(tag, str) and val is None:
            return Value.to_value(self.origin.GetTag(tag))
        assert isinstance(tag, Mask)
        if val is None:
            return Value.to_value(self.origin.GetTag(tag.origin))
        assert isinstance(val, Mask)
        return Value.to_value(self.origin.GetTag(tag.origin, val.origin))

    tag = get_tag

    def set_tag(self, tag: str, val: 'Value' = None, overwrite_existing: bool = None):
        if not overwrite_existing is None:
            self.origin.SetTag(tag, Value.to_value(val).origin, overwrite_existing)
        elif not val is None:
            self.origin.SetTag(tag, Value.to_value(val).origin)
        else:
            self.origin.SetTag(tag)

    def parse_tags(self, note: str, scope: Scope, overwrite_existing: bool = None):
        assert isinstance(note, str)
        assert isinstance(scope, Scope)
        if overwrite_existing is None:
            self.origin.ParseTags(note, scope.origin)
        else:
            assert isinstance(overwrite_existing, bool)
            return self.origin.ParseTags(note, scope.origin, overwrite_existing)

    def append_note(self, note: str, scope: Scope, overwrite_existing: bool = None):
        assert isinstance(note, str)
        assert isinstance(scope, Scope)
        if overwrite_existing is None:
            return self.origin.AppendNote(note, scope.origin)
        else:
            assert isinstance(overwrite_existing, bool)
            return self.origin.AppendNote(note, scope.origin, overwrite_existing)

    @classproperty
    def use_aux_date(cls) -> bool:
        return OriginItem.UseAuxDate

    @use_aux_date.setter
    def use_aux_date(cls, value: bool):
        OriginItem.UseAuxDate = value

    @property
    def date(self) -> date:
        return to_pdate(self.origin.GetDate())

    @date.setter
    def date(self, val: date):
        self.origin.Date = to_ndate(val)

    @property
    def aux_date(self) -> date:
        return to_pdate(self.origin.DateAux)

    @aux_date.setter
    def aux_date(self, val: date):
        self.origin.DateAux = to_ndate(val)

    @property
    def state(self) -> date:
        return self.origin.State

    @state.setter
    def state(self, val: State):
        self.origin.State = val

    def lookup(self, kind: SymbolKind, name: str):
        return self.origin.Lookup(kind, name)

    def valid(self):
        return self.origin.Valid()

# Posts

class PostingXData:

    origin = None
    flags_adapter = FlagsAdapter.PostXDataFlagsAdapter()

    def __init__(self, origin = None) -> None:
        if origin is None:
            self.origin = OriginPostXData()
        else:
            assert isinstance(origin, OriginPostXData)
            self.origin = origin

    @classmethod
    def from_origin(cls, origin):
        return PostingXData(origin=origin) if not origin is None else None

    @property
    def flags(self):
        return self.flags_adapter.GetFlags(self.origin)

    @flags.setter
    def flags(self,value):
        return self.flags_adapter.SetFlags(self.origin, value)

    def has_flags(self,flag):
        return self.flags_adapter.HasFlags(self.origin, flag)

    def clear_flags(self):
        return self.flags_adapter.ClearFlags(self.origin)

    def add_flags(self,flag):
        return self.flags_adapter.AddFlags(self.origin,flag)

    def drop_flags(self,flag):
        return self.flags_adapter.DropFlags(self.origin,flag)

    @property
    def visited_value(self) -> 'Value':
        return Value.to_value(self.origin.VisitedValue)

    @visited_value.setter
    def visited_value(self, value: 'Value'):
        self.origin.VisitedValue = value.origin if not value is None else None 

    @property
    def compound_value(self) -> 'Value':
        return Value.to_value(self.origin.CompoundValue)

    @compound_value.setter
    def compound_value(self, value: 'Value'):
        self.origin.CompoundValue = value.origin if not value is None else None 

    @property
    def total(self) -> 'Value':
        return Value.to_value(self.origin.Total)

    @total.setter
    def total(self, value: 'Value'):
        self.origin.Total = value.origin if not value is None else None 

    @property
    def count(self) -> 'int':
        return self.origin.Count

    @count.setter
    def count(self, value: 'int'):
        self.origin.Count = value

    @property
    def date(self) -> date:
        return to_pdate(self.origin.Date)

    @date.setter
    def date(self, value: date):
        self.origin.Date = to_ndate(value)

    @property
    def datetime(self) -> datetime:
        return to_pdatetime(self.origin.Datetime)

    @datetime.setter
    def datetime(self, value: datetime):
        self.origin.Datetime = to_ndatetime(value)

    @property
    def account(self) -> 'Account':
        return Account.from_origin(self.origin.Account)

    @account.setter
    def account(self, value: 'Account'):
        self.origin.Account = value.origin if not value is None else None 

    @property
    def sort_values(self):
        return SortValueList(NetListAdapter.GetPostXDataSortValues(self.origin))

class Posting(JournalItem):

    def __init__(self, origin) -> None:
        assert isinstance(origin, OriginPost)
        super().__init__(origin)

    @classmethod
    def from_origin(cls, origin):
        return Posting(origin=origin) if not origin is None else None

    def id(self) -> str:
        return self.origin.Id

    def seq(self) -> int:
        return self.origin.Seq

    @property
    def xact(self) -> 'Transaction':
        return Transaction.from_origin(self.origin.Xact)

    @xact.setter
    def xact(self, trx: 'Transaction'):
        self.origin.Xact = trx.origin if not trx is None else None

    @property
    def account(self) -> 'Account':
        return Account.from_origin(self.origin.Account)

    @account.setter
    def account(self, acc: 'Account'):
        self.origin.Account = acc.origin if not acc is None else None

    @property
    def amount(self) -> 'Amount':
        return Amount.from_origin(self.origin.Amount)

    @amount.setter
    def amount(self, amt: 'Amount'):
        self.origin.Amount = amt.origin if not amt is None else None

    @property
    def cost(self) -> 'Amount':
        return Amount.from_origin(self.origin.Cost)

    @cost.setter
    def cost(self, amt: 'Amount'):
        self.origin.Cost = amt.origin if not amt is None else None

    @property
    def given_cost(self) -> 'Amount':
        return Amount.from_origin(self.origin.GivenCost)

    @given_cost.setter
    def given_cost(self, amt: 'Amount'):
        self.origin.GivenCost = amt.origin if not amt is None else None

    @property
    def assigned_amount(self) -> 'Amount':
        return Amount.from_origin(self.origin.AssignedAmount)

    @assigned_amount.setter
    def assigned_amount(self, amt: 'Amount'):
        self.origin.AssignedAmount = amt.origin if not amt is None else None

    def must_balance(self) -> bool:
        return self.origin.MustBalance

    def has_xdata(self) -> bool:
        return self.origin.HasXData

    def clear_xdata(self):
        self.origin.ClearXData()

    def xdata(self) -> PostingXData:
        return PostingXData.from_origin(self.origin.XData)

    def reported_account(self) -> Account:
        return Account.from_origin(self.origin.ReportedAccount)

    def set_reported_account(self, acc: Account):
        self.origin.ReportedAccount = acc.origin if not acc is None else None

# Transactions

class TransactionBase(JournalItem):

    def __init__(self, origin) -> None:
        assert isinstance(origin, OriginXactBase)
        super().__init__(origin)

    @classmethod
    def from_origin(cls, origin):
        if origin is None:
            return None
        if isinstance(origin, OriginXact):
            return Transaction(origin=origin)
        if isinstance(origin, OriginPeriodXact):
            return PeriodicTransaction(origin=origin)
        if isinstance(origin, OriginAutoXact):
            return AutomatedTransaction(origin=origin)
        raise Exception("Incorrect origin for transaction base")

    # TBC

class Transaction(TransactionBase):

    def __init__(self, origin = None) -> None:
        if not origin is None:
            assert isinstance(origin, OriginXact)
        else:
            origin = OriginXact()
        super().__init__(origin)

    @classmethod
    def from_origin(cls, origin):
        return Transaction(origin=origin) if not origin is None else None

    # TBC

class PeriodicTransaction(TransactionBase):

    def __init__(self, origin) -> None:
        assert isinstance(origin, OriginPeriodXact)
        super().__init__(origin)

    @classmethod
    def from_origin(cls, origin):
        return PeriodicTransaction(origin=origin) if not origin is None else None

    # TBC

class AutomatedTransaction(TransactionBase):

    def __init__(self, origin) -> None:
        assert isinstance(origin, OriginAutoXact)
        super().__init__(origin)

    @classmethod
    def from_origin(cls, origin):
        return AutomatedTransaction(origin=origin) if not origin is None else None

    # TBC

# Session

class Session(Scope):

    origin: None

    def __init__(self) -> None:
        raise Exception("Abstract class") 

    @classmethod
    def from_origin(cls, origin):
        return Account(origin=origin) if not origin is None else None

    # TBC

# Values

class Mask:

    origin = None

    def __init__(self,val) -> None:
        if isinstance(val, OriginMask):
            self.origin = val
        else:
            self.origin = OriginMask(val)

    @classmethod
    def from_origin(cls, origin):
        return Mask(origin) if not origin is None else None

    # TBC


class Value:

    origin = None

    def __init__(self,val) -> None:
        if isinstance(val, datetime):
            val = to_ndatetime(val)
        if isinstance(val, date):
            val = to_ndate(val)
        if isinstance(val, Amount) or isinstance(val, Balance) or isinstance(val, Mask) or isinstance(val, Value):
            val = val.origin

        if isinstance(val, ValueList):
            self.origin = NetListAdapter.CreateValue(val.origin)
        elif isinstance(val, OriginValue):
            self.origin = val
        else:
            self.origin = OriginValue(val)

    @classmethod
    def to_value(cls, val) -> 'Value':
        if val is None:
            return None
        return val if isinstance(val, Value) else Value(val)

    def type(self) -> ValueType:
        return self.origin.Type

    def is_equal_to(self, val) -> bool:
        return self.origin.IsEqualTo(val.origin)

    def is_less_than(self, val) -> bool:
        return self.origin.IsLessThan(val.origin)

    def is_greater_than(self, val) -> bool:
        return self.origin.IsGreaterThan(val.origin)

    def __eq__(self, o: object) -> bool:
        return self.origin == Value.to_value(o).origin

    def __ne__(self, o: object) -> bool:
        return self.origin != Value.to_value(o).origin

    def __bool__(self) -> bool:
        return self.origin.Bool

    def __lt__(self, o: object) -> bool:
        return self.origin < Value.to_value(o).origin

    def __le__(self, o: object) -> bool:
        return self.origin <= Value.to_value(o).origin

    def __gt__(self, o: object) -> bool:
        return self.origin > Value.to_value(o).origin

    def __ge__(self, o: object) -> bool:
        return self.origin >= Value.to_value(o).origin

    def __add__(self, o: object) -> 'Value':
        return Value.to_value(self.origin + Value.to_value(o).origin)

    def __radd__(self, o: object) -> 'Value':
        return Value.to_value(Value.to_value(o).origin + self.origin)

    def __sub__(self, o: object) -> 'Value':
        return Value.to_value(self.origin - Value.to_value(o).origin)

    def __rsub__(self, o: object) -> 'Value':
        return Value.to_value(Value.to_value(o).origin - self.origin)

    def __mul__(self, o: object) -> 'Value':
        return Value.to_value(self.origin * Value.to_value(o).origin)

    def __rmul__(self, o: object) -> 'Value':
        return Value.to_value(Value.to_value(o).origin * self.origin)

    def __truediv__(self, o: object) -> 'Value':
        return Value.to_value(self.origin / Value.to_value(o).origin)

    def __rtruediv__(self, o: object) -> 'Value':
        return Value.to_value(Value.to_value(o).origin / self.origin)

    def negated(self) -> 'Value':
        return Value.to_value(self.origin.Negated())

    def in_place_negate(self):
        self.origin.InPlaceNegate()

    def in_place_not(self):
        self.origin.InPlaceNot()

    def __neg__(self) -> 'Value':
        return Value.to_value(-self.origin)

    def abs(self) -> 'Value':
        return Value.to_value(self.origin.Abs())

    __abs__ = abs

    def rounded(self) -> 'Value':
        return Value.to_value(self.origin.Rounded())

    def in_place_round(self):
        self.origin.InPlaceRound()

    def truncated(self) -> 'Value':
        return Value.to_value(self.origin.Truncated())

    def in_place_truncate(self):
        self.origin.InPlaceTruncate()

    def floored(self) -> 'Value':
        return Value.to_value(self.origin.Floored())

    def in_place_floor(self):
        self.origin.InPlaceFloor()

    def unrounded(self) -> 'Value':
        return Value.to_value(self.origin.Unrounded())

    def in_place_unround(self):
        self.origin.InPlaceUnround()

    def reduced(self) -> 'Value':
        return Value.to_value(self.origin.Reduced())

    def in_place_reduce(self):
        self.origin.InPlaceReduce()

    def unreduced(self) -> 'Value':
        return Value.to_value(self.origin.Unreduced())

    def in_place_unreduce(self):
        self.origin.InPlaceUnreduce()

    def value(self, in_terms_of : Commodity = None, moment = None) -> 'Value':

        if in_terms_of is None:
            return Value.to_value(self.origin.ValueOf(TimesCommon.Current.CurrentTime))

        assert(isinstance(in_terms_of, Commodity))

        if moment is None:
            return Value.to_value(self.origin.ValueOf(TimesCommon.Current.CurrentTime, in_terms_of.origin))

        return Value.to_value(self.origin.ValueOf(to_ndatetime(moment), in_terms_of.origin))

    def exchange_commodities(self, commodities: str, addPrices: bool = None, moment = None) -> 'Value':

        assert(not (commodities is None) and isinstance(commodities, str))
        if addPrices is None:
            return Value.to_value(self.origin.ExchangeCommodities(commodities, False, DateTime())) # default(DateTime) caused PythonNet issued, so arguments are populated explicitely

        assert(isinstance(addPrices, bool))
        if moment is None:
            return Value.to_value(self.origin.ExchangeCommodities(commodities, addPrices, DateTime()))

        return Value.to_value(self.origin.ExchangeCommodities(commodities, addPrices, to_ndatetime(moment)))

    def is_nonzero(self) -> bool:
        return self.origin.IsNonZero

    __nonzero__ = is_nonzero

    def is_realzero(self) -> bool:
        return self.origin.IsRealZero

    def is_zero(self) -> bool:
        return self.origin.IsZero

    def is_null(self) -> bool:
        return OriginValue.IsNullOrEmpty(self.origin)

    def type(self) -> ValueType:
        return self.origin.Type

    def is_type(self, type_enum: ValueType) -> bool:
        return self.origin.Type == type_enum

    def is_boolean(self) -> bool:
        return self.origin.Type == ValueType.Boolean

    def set_boolean(self, val: bool):
        self.origin.SetBoolean(val)

    def is_datetime(self) -> bool:
        return self.origin.Type == ValueType.DateTime

    def set_datetime(self, val: datetime):
        self.origin.SetDateTime(to_ndatetime(val))

    def is_date(self) -> bool:
        return self.origin.Type == ValueType.Date

    def set_date(self, val: date):
        self.origin.SetDate(to_ndate(val))

    def is_long(self) -> bool:
        return self.origin.Type == ValueType.Integer

    def set_long(self, val: int):
        self.origin.SetLong(val)

    def is_amount(self) -> bool:
        return self.origin.Type == ValueType.Amount

    def set_amount(self, val: Amount):
        self.origin.SetAmount(val.origin)

    def is_balance(self) -> bool:
        return self.origin.Type == ValueType.Balance

    def set_balance(self, val: Balance):
        self.origin.SetBalance(val.origin)

    def is_string(self) -> bool:
        return self.origin.Type == ValueType.String

    def set_string(self, val: str):
        self.origin.SetString(val)

    def is_mask(self) -> bool:
        return self.origin.Type == ValueType.Mask

    def set_mask(self, val: Mask):
        self.origin.SetMask(val.origin)

    def is_sequence(self) -> bool:
        return self.origin.Type == ValueType.Sequence

    def set_sequence(self, val: Iterable):
        NetListAdapter.SetValueSequence(self.origin, ValueList(val).origin)

    def to_boolean(self) -> bool:
        return self.origin.AsBoolean

    def to_long(self) -> int:
        return self.origin.AsLong

    __int__ = to_long

    def to_datetime(self) -> datetime:
        return to_pdatetime(self.origin.AsDateTime)

    def to_date(self) -> date:
        return to_pdate(self.origin.AsDate)

    def to_amount(self) -> Amount:
        return Amount.from_origin(self.origin.AsAmount)

    def to_balance(self) -> Balance:
        return Balance.from_origin(self.origin.AsBalance)

    def to_string(self) -> str:
        return self.origin.AsString

    __str__ = to_string

    def to_mask(self) -> Mask:
        return Mask.from_origin(self.origin.AsMask)

    def to_sequence(self) -> Iterable:
        return ValueList(NetListAdapter.GetValueSequence(self.origin))

    def __repr__(self) -> str:
        return self.origin.Dump()

    def casted(self, type: ValueType) -> 'Value':
        return Value.to_value(self.origin.Casted(type))

    def in_place_cast(self, type: ValueType):
        self.origin.InPlaceCast(type)

    def simplified(self) -> 'Value':
        return Value.to_value(self.origin.Simplified())

    def in_place_simplify(self):
        self.origin.InPlaceSimplify()

    def number(self) -> 'Value':
        return Value.to_value(self.origin.Number())

    def annotate(self, details: Annotation):
        self.origin.Annotate(details.origin)

    def has_annotation(self) -> bool:
        return self.origin.HasAnnotation

    @property
    def annotation(self) -> Annotation:
        return Annotation.from_origin(self.origin.Annotation)

    def strip_annotations(self, keep_details: KeepDetails = None) -> 'Value':
        if keep_details is None:
            return Value.to_value(self.origin.StripAnnotations(OriginAnnotationKeepDetails()))
        return Value.to_value(self.origin.StripAnnotations(keep_details.origin))

    def push_back(self,val):
        self.origin.PushBack(Value.to_value(val).origin)

    def pop_back(self):
        self.origin.PopBack()

    def size(self) -> int:
        return self.origin.Size

    def label(self) -> str:
        return self.origin.Label()

    def valid(self) -> bool:
        return self.origin.IsValid

    def basetype(self):
        if self.is_boolean():
            return type(bool)
        if self.is_long():
            return type(int)
        if self.is_string():
            return type(str)

        return self.origin.BaseType()

NULL_VALUE = Value(OriginValue.Empty)

def string_value(s: str) -> Value:
    return Value.to_value(OriginValue.StringValue(s))

def mask_value(s: str) -> Value:
    return Value.to_value(OriginValue.MaskValue(s))

def value_context(val: Value) -> str:
    return OriginValue.ValueContext(val.origin)

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
