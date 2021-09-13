####################################################################################
# Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
# Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
# 
# This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
# Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
# See LICENSE.LEDGER file included with the distribution for details and disclaimer.
####################################################################################

###########################
# NLedger Python interface module

from typing import Iterable, List, Tuple, Dict
import os
import ntpath
import sys

# Helper functions

def getenv(name: str) -> str:
    return os.environ[name] if name in os.environ else None

def getpath(relative_path: str) -> str:
    return ntpath.abspath(ntpath.join(ntpath.dirname(ntpath.realpath(__file__)), relative_path))

# Static property attribute

class classproperty(property):
    def __get__(self, cls, owner):
        return classmethod(self.fget).__get__(None, owner)()

############################
# CLR Runtime initialization

# In case of using PythonNet 3, CLR Runtime initialization can be optionally regulated by means of "nledger_python_clr_runtime" environment variable.
# Possible values are: "netfx", "mono", "core". In case of "core", you should also specify a path to runtime config file ("nledger_python_clr_runtime_config" variable)
# For older versions of PythonNet (e.g. 2.5.1), the variable "nledger_python_clr_runtime" should be empty.

clr_runtime = getenv("nledger_python_clr_runtime")
if bool(clr_runtime):
    from pythonnet import set_runtime

    if clr_runtime == "netfx":
        from clr_loader import get_netfx
        set_runtime(get_netfx())
    elif clr_runtime == "mono":
        from clr_loader import get_mono
        set_runtime(get_mono())
    elif clr_runtime == "core":
        from clr_loader import get_coreclr
        runtime_config = getenv("nledger_python_clr_runtime_config")
        assert not bool(runtime_config)
        set_runtime(get_coreclr(runtime_config))
    else:
        raise Exception("Unsupported CLR Runtime selector (environment variable 'nledger_python_clr_runtime_config'): " + clr_runtime)

import clr

############################
# Load NLedger library

# The module looks for NLedger.dll in the local runtime folder until environment variable "nledger_extensibility_python_dll_path" is populated

nledger_extensibility_python_dll_path = getenv("nledger_extensibility_python_dll_path")
if not bool(nledger_extensibility_python_dll_path):
    nledger_extensibility_python_dll_path = getpath('../runtime/NLedger.Extensibility.Python.dll')
assert ntpath.isfile(nledger_extensibility_python_dll_path)

# Adding path to runtime dll to PATH and sending only name fixes a problem in pythonnet:
# it tries to use Assembly.Load for loading an assembly specified by path and name

dllFolder = ntpath.dirname(nledger_extensibility_python_dll_path)
dllName = ntpath.splitext(ntpath.basename(nledger_extensibility_python_dll_path))[0]

if not(dllFolder in sys.path):
   sys.path.append(dllFolder)

# Load NLedger.Extensibility.Python.dll (and NLedger.dll respectively)

clr.AddReference(dllName)

############################
# Import NLedger classes

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
from NLedger.Extensibility.Export import AnnotatedCommodity as ExportedAnnotatedCommodity
from NLedger.Extensibility.Export import Amount as ExportedAmount
from NLedger.Extensibility.Export import ValueType as ExportedValueType
from NLedger.Extensibility.Export import Value as ExportedValue
from NLedger.Extensibility.Export import ValueType as ExportedValueType
from NLedger.Extensibility.Export import Account as ExportedAccount
from NLedger.Extensibility.Export import AccountXData as ExportedAccountXData
from NLedger.Extensibility.Export import AccountXDataDetails as ExportedAccountXDataDetails
from NLedger.Extensibility.Export import Balance as ExportedBalance
from NLedger.Extensibility.Export import Expr as ExportedExpr
from NLedger.Extensibility.Export import Position as ExportedPosition
from NLedger.Extensibility.Export import Journal as ExportedJournal
from NLedger.Extensibility.Export import JournalItem as ExportedJournalItem
from NLedger.Extensibility.Export import State as ExportedState 
from NLedger.Extensibility.Export import Mask as ExportedMask
from NLedger.Extensibility.Export import Posting as ExportedPosting
from NLedger.Extensibility.Export import PostingXData as ExportedPostingXData
from NLedger.Extensibility.Export import SymbolKind as ExportedSymbolKind
from NLedger.Extensibility.Export import Session as ExportedSession
from NLedger.Extensibility.Export import SortValue as ExportedSortValue
from NLedger.Extensibility.Export import Times as ExportedTimes
from NLedger.Extensibility.Export import Transaction as ExportedTransaction
from NLedger.Extensibility.Export import AutomatedTransaction as ExportedAutomatedTransaction
from NLedger.Extensibility.Export import PeriodicTransaction as ExportedPeriodicTransaction

ANNOTATION_PRICE_CALCULATED = ExportedAnnotation.ANNOTATION_PRICE_CALCULATED
ANNOTATION_PRICE_FIXATED = ExportedAnnotation.ANNOTATION_PRICE_FIXATED
ANNOTATION_PRICE_NOT_PER_UNIT = ExportedAnnotation.ANNOTATION_PRICE_NOT_PER_UNIT
ANNOTATION_DATE_CALCULATED = ExportedAnnotation.ANNOTATION_DATE_CALCULATED
ANNOTATION_TAG_CALCULATED = ExportedAnnotation.ANNOTATION_TAG_CALCULATED
ANNOTATION_VALUE_EXPR_CALCULATED = ExportedAnnotation.ANNOTATION_VALUE_EXPR_CALCULATED

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

from NLedger.Extensibility import ExtendedSession
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
from NLedger.Accounts import AccountXDataDetails as OriginAccountXDataDetails
from NLedger.Accounts import AccountXData as OriginAccountXData
from NLedger.Scopus import SymbolKindEnum as SymbolKind
from NLedger.Scopus import Scope as OriginScope
from NLedger.Scopus import Session as OriginSession
from NLedger.Items import Item as OriginItem
from NLedger.Values import ValueTypeEnum as ValueType
from NLedger.Values import Value as OriginValue
from NLedger import Balance as OriginBalance
from NLedger import Mask as OriginMask
from NLedger.Items import ItemPosition as OriginItemPosition
from NLedger import Post as OriginPost
from NLedger import PostXData as OriginPostXData
from NLedger import SupportsFlagsEnum as OriginSupportsFlagsEnum
from NLedger.Xacts import XactBase as OriginXactBase
from NLedger.Xacts import Xact as OriginXact
from NLedger.Xacts import PeriodXact as OriginPeriodXact
from NLedger.Xacts import AutoXact as OriginAutoXact
from NLedger.Journals import Journal as OriginJournal
from NLedger.Journals import JournalFileInfo as OriginJournalFileInfo
from NLedger import Predicate
from NLedger.Expressions import Expr as OriginExpr

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
from NLedger.Extensibility.Export import ExportedConsts

from NLedger.Times import TimesCommon
from NLedger.Times import DateInterval

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

    def __len__(self) -> int:
        return self.origin.Count

    def __iter__(self):
        lst = []
        for index in range(len(self)):
            lst.append(self.to_pitem(self.origin[index]))
        return iter(lst)

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

# Lists

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

class PostingList(NList):
    def __init__(self, origin = None) -> None:
        super().__init__(origin=origin)

    def get_nclass(self) -> type:
        return NetListAdapter[OriginPost]

    def to_nitem(self, item):
        return item.origin if not item is None else None

    def to_pitem(self, item):
        return Posting.from_origin(item)

class AccountList(NList):
    def __init__(self, origin = None) -> None:
        super().__init__(origin=origin)

    def get_nclass(self) -> type:
        return NetListAdapter[OriginAccount]

    def to_nitem(self, item):
        return item.origin if not item is None else None

    def to_pitem(self, item):
        return Account.from_origin(item)

class AmountList(NList):
    def __init__(self, origin = None) -> None:
        super().__init__(origin=origin)

    def get_nclass(self) -> type:
        return NetListAdapter[OriginAmount]

    def to_nitem(self, item):
        return item.origin if not item is None else None

    def to_pitem(self, item):
        return Amount.from_origin(item)

class TransactionList(NList):
    def __init__(self, origin = None) -> None:
        super().__init__(origin=origin)

    def get_nclass(self) -> type:
        return NetListAdapter[OriginXact]

    def to_nitem(self, item):
        return item.origin if not item is None else None

    def to_pitem(self, item):
        return Transaction.from_origin(item)

class AutomatedTransactionList(NList):
    def __init__(self, origin = None) -> None:
        super().__init__(origin=origin)

    def get_nclass(self) -> type:
        return NetListAdapter[OriginAutoXact]

    def to_nitem(self, item):
        return item.origin if not item is None else None

    def to_pitem(self, item):
        return AutomatedTransaction.from_origin(item)

class PeriodicTransactionList(NList):
    def __init__(self, origin = None) -> None:
        super().__init__(origin=origin)

    def get_nclass(self) -> type:
        return NetListAdapter[OriginPeriodXact]

    def to_nitem(self, item):
        return item.origin if not item is None else None

    def to_pitem(self, item):
        return PeriodicTransaction.from_origin(item)

class FileInfoList(NList):
    def __init__(self, origin = None) -> None:
        super().__init__(origin=origin)

    def get_nclass(self) -> type:
        return NetListAdapter[OriginJournalFileInfo]

    def to_nitem(self, item):
        return item.origin if not item is None else None

    def to_pitem(self, item):
        return FileInfo.from_origin(item)

###########################
# Ported from py_expr.cc

class Expr:

    origin = None

    def __init__(self, val = None) -> None:
        if val is None:
            self.origin = OriginExpr()
        elif isinstance(val, str):
            self.origin = OriginExpr(val)
        elif isinstance(val, OriginExpr):
            self.origin = val
        else:
            raise Exception("Unexpected parameter type")

    @classmethod
    def from_origin(cls, origin) -> 'Expr':
        return Expr(origin) if not origin is None else None

    def __bool__(self) -> bool:
        return not self.origin.IsEmpty

    __nonzero__ = __bool__

    def text(self) -> str:
        return self.origin.Text

    def set_text(self, val: str):
        self.origin.Text = val

    def __call__(self, scope: 'Scope' = None) -> 'Value':
        if scope is None:
            return Value.to_value(self.origin.Calc())
        else:
            assert isinstance(scope, Scope)
            return Value.to_value(self.origin.Calc(scope.origin))

    @property
    def context(self) -> 'Scope':
        return Scope.from_origin(self.origin.Context)

    @context.setter
    def context(self, scope: 'Scope'):
        self.origin.Context = scope.origin if not scope is None else None

    def compile(self, scope: 'Scope'):
        assert isinstance(scope, Scope)
        self.origin.Compile(scope.origin)

    def is_constant(self) -> bool:
        return self.origin.IsConstant

###########################
# Ported from py_amount.cc

from NLedger.Extensibility.Export import ParseFlags     # ParseFlags enum

class Amount:

    origin = None

    def __init__(self,value = None, origin = None) -> None:
        if not (origin is None):
            assert isinstance(origin, OriginAmount)
            self.origin = origin
        else:
            if value is None:
                self.origin = OriginAmount()
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
        return OriginAmount.op_Equality(self.origin, Amount.to_amount(o).origin if not o is None else None)

    def __ne__(self, o: object) -> bool:
        return OriginAmount.op_Inequality(self.origin, Amount.to_amount(o).origin if not o is None else None)

    def __lt__(self, o: object) -> bool:
        return OriginAmount.op_LessThan(self.origin, Amount.to_amount(o).origin if not o is None else None)

    def __le__(self, o: object) -> bool:
        return OriginAmount.op_LessThanOrEqual(self.origin, Amount.to_amount(o).origin if not o is None else None)

    def __gt__(self, o: object) -> bool:
        return OriginAmount.op_GreaterThan(self.origin, Amount.to_amount(o).origin if not o is None else None)

    def __ge__(self, o: object) -> bool:
        return OriginAmount.op_GreaterThanOrEqual(self.origin, Amount.to_amount(o).origin if not o is None else None)

    def __neg__(self) -> 'Amount':
        return Amount.from_origin(OriginAmount.op_UnaryNegation(self.origin))

    def __bool__(self) -> bool:
        return self.origin.IsNonZero

    def __add__(self, o: object) -> 'Amount':
        return Amount.from_origin(OriginAmount.op_Addition(self.origin, Amount.to_amount(o).origin))

    def __radd__(self, o: object) -> 'Amount':
        return Amount.from_origin(OriginAmount.op_Addition(Amount.to_amount(o).origin, self.origin))

    def __sub__(self, o: object) -> 'Amount':
        return Amount.from_origin(OriginAmount.op_Subtraction(self.origin, Amount.to_amount(o).origin))

    def __rsub__(self, o: object) -> 'Amount':
        return Amount.from_origin(OriginAmount.op_Subtraction(Amount.to_amount(o).origin, self.origin))

    def __mul__(self, o: object) -> 'Amount':
        return Amount.from_origin(OriginAmount.op_Multiply(self.origin, Amount.to_amount(o).origin))

    def __rmul__(self, o: object) -> 'Amount':
        return Amount.from_origin(OriginAmount.op_Multiply(Amount.to_amount(o).origin, self.origin))

    def __truediv__(self, o: object) -> 'Amount':
        return Amount.from_origin(OriginAmount.op_Division(self.origin, Amount.to_amount(o).origin))

    def __rtruediv__(self, o: object) -> 'Amount':
        return Amount.from_origin(OriginAmount.op_Division(Amount.to_amount(o).origin, self.origin))

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

    def value(self, in_terms_of: 'Commodity' = None, moment = None) -> 'Amount':
        if in_terms_of is None:
            assert moment is None
            return Amount.from_origin(self.origin.Value(TimesCommon.Current.CurrentTime))
        else:
            assert isinstance(in_terms_of, Commodity)
            if moment is None:
                return Amount.from_origin(self.origin.Value(TimesCommon.Current.CurrentTime, in_terms_of.origin))
            elif isinstance(moment, datetime):
                return Amount.from_origin(self.origin.Value(to_ndatetime(moment), in_terms_of.origin))
            elif isinstance(moment, date):
                return Amount.from_origin(self.origin.Value(to_ndatetime(moment), in_terms_of.origin))
            else:
                raise Exception("Unexpected argument type: moment")

    def price(self) -> 'Amount':
        return Amount.from_origin(self.origin.Price)

    def sign(self) -> int:
        return self.origin.Sign

    def is_nonzero(self) -> bool:
        return self.origin.IsNonZero

    __nonzero__ = is_nonzero

    def is_zero(self) -> bool:
        return self.origin.IsZero

    def is_realzero(self) -> bool:
        return self.origin.IsRealZero

    def is_null(self) -> bool:
        return OriginAmount.IsNullOrEmpty(self.origin)

    def to_double(self) -> float:
        return self.origin.ToDouble()

    __float__ = to_double

    def to_long(self) -> int:
        return self.origin.ToLong()

    __int__ = to_long

    def fits_in_long(self) -> bool:
        return self.origin.FitsInLong

    def to_string(self) -> str:
        return self.origin.ToString()

    __str__ =  to_string

    def to_fullstring(self) -> str:
        return self.origin.ToFullString()

    __repr__ = to_fullstring

    def quantity_string(self) -> str:
        return self.origin.QuantityString()

    @property
    def commodity(self) -> 'Commodity':
        return Commodity.from_origin(self.origin.Commodity)

    @commodity.setter
    def commodity(self, value: 'Commodity'):
        self.origin.Commodity = value.origin if not value is None else None

    def has_commodity(self) -> bool:
        return self.origin.HasCommodity

    def with_commodity(self, comm: 'Commodity') -> 'Amount':
        return Amount.from_origin(self.origin.WithCommodity(comm.origin)) if not comm is None else Amount.from_origin(self.origin.WithCommodity(None))

    def clear_commodity(self):
        return self.origin.ClearCommodity()

    def number(self) -> 'Amount':
        return Amount.from_origin(self.origin.Number())

    def annotate(self, details: 'Annotation'):
        assert isinstance(details, Annotation)
        self.origin.Annotate(details.origin)

    def has_annotation(self) -> bool:
        return self.origin.HasAnnotation

    @property
    def annotation(self) -> 'Annotation':
        return Annotation.from_origin(self.origin.Annotation)

    def strip_annotations(self, what_to_keep: 'KeepDetails' = None) -> 'Amount':
        if what_to_keep is None:
            return Amount.from_origin(self.origin.StripAnnotations(OriginAnnotationKeepDetails()))
        else:
            assert isinstance(what_to_keep, KeepDetails)
            return Amount.from_origin(self.origin.StripAnnotations(what_to_keep.origin))

    def parse(self, s: str, flags: 'ParseFlags' = None):
        if flags is None:
            self.origin.Parse(s)
        else:
            assert isinstance(flags, ParseFlags)
            self.origin.Parse(s, FlagsAdapter.ToAmountFlags(flags))

    @classmethod
    def parse_conversion(cls, larger_str: str, smaller_str: str):
        return OriginAmount.ParseConversion(larger_str, smaller_str)

    def valid(self) -> bool:
        return self.origin.Valid()

###########################
# Ported from py_balance.cc

class Balance:

    origin = None

    def __init__(self, val = None) -> None:
        if val is None:
            self.origin = OriginBalance()
        elif isinstance(val, OriginBalance):
            self.origin = val
        elif isinstance(val, Amount) or isinstance(val, Balance):
            self.origin = OriginBalance(val.origin)
        elif isinstance(val, str) or isinstance(val, int) or isinstance(val, float):
            self.origin = OriginBalance(val)
        else:
            raise Exception("Unexpected argument type")

    @classmethod
    def from_origin(cls, origin) -> 'Balance':
        return Balance(origin) if not origin is None else None

    def __add__(self, o: object) -> 'Balance':
        if isinstance(o, Amount) or isinstance(o, Balance):
            return Balance.from_origin(OriginBalance.op_Addition(self.origin, o.origin))
        elif isinstance(o, int) or isinstance(o, float):
            return Balance.from_origin(OriginBalance.op_Addition(self.origin, Amount.to_amount(o).origin))
        else:
            raise Exception("Unexpected argument type")

    __iadd__ = __add__

    def __sub__(self, o: object) -> 'Balance':    
        if isinstance(o, Amount) or isinstance(o, Balance):
            return Balance.from_origin(OriginBalance.op_Subtraction(self.origin, o.origin))
        elif isinstance(o, int) or isinstance(o, float):
            return Balance.from_origin(OriginBalance.op_Subtraction(self.origin, Amount.to_amount(o).origin))
        else:
            raise Exception("Unexpected argument type")

    __isub__ = __sub__

    def __mul__(self, o: object) -> 'Balance':
        if isinstance(o, Amount):
            return Balance.from_origin(OriginBalance.op_Multiply(self.origin, o.origin))
        elif isinstance(o, int) or isinstance(o, float):
            return Balance.from_origin(OriginBalance.op_Multiply(self.origin, Amount.to_amount(o).origin))
        else:
            raise Exception("Unexpected argument type")

    __imul__ = __mul__

    def __truediv__ (self, o: object) -> 'Balance':
        if isinstance(o, Amount):
            return Balance.from_origin(OriginBalance.op_Division(self.origin, o.origin))
        elif isinstance(o, int) or isinstance(o, float):
            return Balance.from_origin(OriginBalance.op_Division(self.origin, Amount.to_amount(o).origin))
        else:
            raise Exception("Unexpected argument type")

    __itruediv__ = __truediv__

    def __neg__(self) -> 'Balance':
        return Balance.from_origin(OriginBalance.op_UnaryNegation(self.origin))

    def __eq__(self, o: object) -> bool:
        if isinstance(o, Value):
            return o.__eq__(self)
        if isinstance(o, Amount) or isinstance(o, Balance):
            return OriginBalance.op_Equality(self.origin, o.origin)
        elif isinstance(o, int) or isinstance(o, float):
            return OriginBalance.op_Equality(self.origin, Amount.to_amount(o).origin)
        else:
            raise Exception("Unexpected argument type")

    def __ne__(self, o: object) -> bool:
        if isinstance(o, Value):
            return o.__ne__(self)
        if isinstance(o, Amount) or isinstance(o, Balance):
            return OriginBalance.op_Inequality(self.origin, o.origin)
        elif isinstance(o, int) or isinstance(o, float):
            return OriginBalance.op_Inequality(self.origin, Amount.to_amount(o).origin)
        else:
            raise Exception("Unexpected argument type")

    def __bool__(self) -> bool:
        return OriginBalance.op_Explicit(self.origin)

    def to_string(self) -> str:
        return self.origin.ToString()

    __str__ = to_string

    def negated(self) -> 'Balance':
        return Balance.from_origin(self.origin.Negated())

    def abs(self) -> 'Balance':
        return Balance.from_origin(self.origin.Abs())

    __abs__ = abs

    def __len__(self) -> int:
        return NetListAdapter.GetAmounts(self.origin).Count

    def __getitem__(self, row: int) -> 'Amount':
        return Amount.from_origin(NetListAdapter.GetAmounts(self.origin)[row])

    def __iter__(self):
        return iter(AmountList(NetListAdapter.GetAmounts(self.origin)))

    def in_place_negate(self):
        self.origin.InPlaceNegate()

    def rounded(self) -> 'Balance':
        return Balance.from_origin(self.origin.Rounded())

    def in_place_round(self):
        self.origin.InPlaceRound()

    def truncated(self) -> 'Balance':
        return Balance.from_origin(self.origin.Truncated())

    def in_place_truncate(self):
        self.origin.InPlaceTruncate()

    def floored(self) -> 'Balance':
        return Balance.from_origin(self.origin.Floored())

    def in_place_floor(self):
        self.origin.InPlaceFloor()

    def unrounded(self) -> 'Balance':
        return Balance.from_origin(self.origin.Unrounded())

    def in_place_unround(self):
        self.origin.InPlaceUnround()

    def reduced(self) -> 'Balance':
        return Balance.from_origin(self.origin.Reduced())

    def in_place_reduce(self):
        self.origin.InPlaceReduce()

    def unreduced(self) -> 'Balance':
        return Balance.from_origin(self.origin.Unreduced())

    def in_place_unreduce(self):
        self.origin.InPlaceUnreduce()

    def value(self, in_terms_of: 'Commodity' = None, moment = None) -> 'Balance':
        if moment is None and in_terms_of is None:
            return Balance.from_origin(self.origin.Value(TimesCommon.Current.CurrentTime))
        elif moment is None:
            assert isinstance(in_terms_of, Commodity)
            return Balance.from_origin(self.origin.Value(TimesCommon.Current.CurrentTime, in_terms_of.origin))
        elif isinstance(moment, date) or isinstance(moment, datetime):
            assert isinstance(in_terms_of, Commodity)
            return Balance.from_origin(self.origin.Value(to_ndatetime(moment), in_terms_of.origin))
        else:
            raise Exception("Unexpected argument type")

    def is_nonzero(self) -> bool:
        return self.origin.IsNonZero

    __nonzero__ = is_nonzero

    def is_zero(self) -> bool:
        return self.origin.IsZero

    def is_realzero(self) -> bool:
        return self.origin.IsRealZero

    def is_empty(self) -> bool:
        return self.origin.IsEmpty

    def single_amount(self) -> 'Amount':
        return Amount.from_origin(self.origin.SingleAmount)

    def to_amount(self) -> 'Amount':
        return Amount.from_origin(self.origin.ToAmount())

    def commodity_count(self) -> int:
        return self.origin.CommodityCount

    def commodity_amount(self, commodity: 'Commodity' = None) -> 'Amount':
        if commodity is None:
            return Amount.from_origin(self.origin.CommodityAmount())
        else:
            assert isinstance(commodity, Commodity)
            return Amount.from_origin(self.origin.CommodityAmount(commodity.origin))

    def number(self) -> 'Balance':
        return Balance.from_origin(self.origin.Number())

    def strip_annotations(self, keep: 'KeepDetails' = None) -> 'Balance':
        if keep is None:
            return Balance.from_origin(self.origin.StripAnnotations(OriginAnnotationKeepDetails()))
        else:
            assert isinstance(keep, KeepDetails)
            return Balance.from_origin(self.origin.StripAnnotations(keep.origin))

    def valid(self) -> bool:
        return self.origin.Valid()

###########################
# Ported from py_commodity.cc

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

COMMODITY_STYLE_DEFAULTS = ExportedConsts.COMMODITY_STYLE_DEFAULTS
COMMODITY_STYLE_SUFFIXED = ExportedConsts.COMMODITY_STYLE_SUFFIXED
COMMODITY_STYLE_SEPARATED = ExportedConsts.COMMODITY_STYLE_SEPARATED
COMMODITY_STYLE_DECIMAL_COMMA = ExportedConsts.COMMODITY_STYLE_DECIMAL_COMMA
COMMODITY_STYLE_TIME_COLON = ExportedConsts.COMMODITY_STYLE_TIME_COLON
COMMODITY_STYLE_THOUSANDS = ExportedConsts.COMMODITY_STYLE_THOUSANDS
COMMODITY_NOMARKET = ExportedConsts.COMMODITY_NOMARKET
COMMODITY_BUILTIN = ExportedConsts.COMMODITY_BUILTIN
COMMODITY_WALKED = ExportedConsts.COMMODITY_WALKED
COMMODITY_KNOWN = ExportedConsts.COMMODITY_KNOWN
COMMODITY_PRIMARY = ExportedConsts.COMMODITY_PRIMARY

class Commodity:

    origin = None

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

    def strip_annotations(self, keep: 'KeepDetails' = None) -> 'Commodity':
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

    def find_price(self, commodity: 'Commodity' = None, moment = None, oldest = None) -> 'PricePoint':
        PricePoint.from_origin(self.origin.FindPrice(commodity.origin if not commodity is None else None, to_ndatetime(moment) if not moment is None else DateTime.MinValue, to_ndatetime(oldest) if not oldest is None else DateTime.MinValue))

    def check_for_updated_price(self, point: 'PricePoint' = None, moment = None, inTermsOf: 'Commodity' = None) -> 'PricePoint':
        PricePoint.from_origin(self.origin.CheckForUpdatedPrice(point.origin if not point is None else None, to_ndatetime(moment) if not moment is None else DateTime.MinValue, inTermsOf.origin if not inTermsOf is None else None))

    def valid(self):
        self.origin.Valid()

class Annotation:

    origin = None
    flags_adapter = FlagsAdapter.AnnotationFlagsAdapter()

    def __init__(self,origin=None) -> None:
        if origin is None:
            self.origin = OriginAnnotation()
        else:
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

    origin = None

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

    origin = None

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

class AnnotatedCommodity(Commodity):

    origin = None

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

###########################
# Ported from py_account.cc

ACCOUNT_EXT_SORT_CALC = ExportedConsts.ACCOUNT_EXT_SORT_CALC
ACCOUNT_EXT_HAS_NON_VIRTUALS = ExportedConsts.ACCOUNT_EXT_HAS_NON_VIRTUALS
ACCOUNT_EXT_HAS_UNB_VIRTUALS = ExportedConsts.ACCOUNT_EXT_HAS_UNB_VIRTUALS
ACCOUNT_EXT_AUTO_VIRTUALIZE = ExportedConsts.ACCOUNT_EXT_AUTO_VIRTUALIZE
ACCOUNT_EXT_VISITED = ExportedConsts.ACCOUNT_EXT_VISITED
ACCOUNT_EXT_MATCHING = ExportedConsts.ACCOUNT_EXT_MATCHING
ACCOUNT_EXT_TO_DISPLAY = ExportedConsts.ACCOUNT_EXT_TO_DISPLAY
ACCOUNT_EXT_DISPLAYED = ExportedConsts.ACCOUNT_EXT_DISPLAYED

class AccountXDataDetails:

    origin = None

    def __init__(self, origin = None) -> None:
        if not(origin is None):
            assert isinstance(origin, OriginAccountXDataDetails)
            self.origin = origin
        else:
            self.origin = OriginAccountXDataDetails()

    @classmethod
    def from_origin(cls, origin):
        return AccountXDataDetails(origin=origin) if not origin is None else None

    @property
    def total(self) -> 'Value':
        return Value.to_value(self.origin.Total)

    @property
    def real_total(self) -> 'Value':
        return Value.to_value(self.origin.RealTotal)

    @property
    def calculated(self) -> bool:
        return self.origin.Calculated

    @property
    def gathered(self) -> bool:
        return self.origin.Gathered

    @property
    def posts_count(self) -> int:
        return self.origin.PostsCount

    @property
    def posts_virtuals_count(self) -> int:
        return self.origin.PostsVirtualsCount

    @property
    def posts_cleared_count(self) -> int:
        return self.origin.PostsClearedCount

    @property
    def posts_last_7_count(self) -> int:
        return self.origin.PostsLast7Count

    @property
    def posts_last_30_count(self) -> int:
        return self.origin.PostsLast30Count

    @property
    def posts_this_month_count(self) -> int:
        return self.origin.PostsThisMountCount

    @property
    def earliest_post(self) -> date:
        return to_pdate(self.origin.EarliestPost)

    @property
    def earliest_cleared_post(self) -> date:
        return to_pdate(self.origin.EarliestClearedPost)

    @property
    def latest_post(self) -> date:
        return to_pdate(self.origin.LatestPost)

    @property
    def latest_cleared_post(self) -> date:
        return to_pdate(self.origin.LatestClearedPost)

    @property
    def filenames(self) -> Iterable:
        return list(self.origin.Filenames)

    @property
    def accounts_referenced(self) -> Iterable:
        return list(self.origin.AccountsReferenced)

    @property
    def payees_referenced(self) -> Iterable:
        return list(self.origin.PayeesReferenced)

    def __iadd__(self, o: object) -> 'Value':
        assert isinstance(o, AccountXDataDetails)
        self.origin.Add(o.origin)
        return self

    def update(self, post: 'Posting', gather_all: bool = None):
        if gather_all is None:
            self.origin.Update(post.origin)
        else:
            self.origin.Update(post.origin, gather_all)

class AccountXData:

    origin = None
    flags_adapter = FlagsAdapter.AccountXDataFlagsAdapter()

    def __init__(self, origin = None) -> None:
        if not(origin is None):
            assert isinstance(origin, OriginAccountXData)
            self.origin = origin
        else:
            self.origin = OriginAccountXData()

    @classmethod
    def from_origin(cls, origin):
        return AccountXData(origin=origin) if not origin is None else None

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
    def self_details(self) -> AccountXDataDetails:
        return AccountXDataDetails.from_origin(self.origin.SelfDetails)

    @property
    def family_details(self) -> AccountXDataDetails:
        return AccountXDataDetails.from_origin(self.origin.FamilyDetails)

    @property
    def reported_posts(self) -> Iterable:
        return PostingList(NetListAdapter.GetPosts(self.origin))

    @property
    def sort_values(self) -> Iterable:
        return SortValueList(NetListAdapter.GetAccountXDataSortValues(self.origin))

ACCOUNT_NORMAL = ExportedConsts.ACCOUNT_NORMAL
ACCOUNT_KNOWN = ExportedConsts.ACCOUNT_KNOWN
ACCOUNT_TEMP = ExportedConsts.ACCOUNT_TEMP
ACCOUNT_GENERATED = ExportedConsts.ACCOUNT_GENERATED

class Account:

    origin = None
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

    def add_post(self, post: 'Posting'):
        assert isinstance(post, Posting)
        self.origin.AddPost(post.origin)

    def remove_post(self, post: 'Posting'):
        assert isinstance(post, Posting)
        self.origin.RemovePost(post.origin)

    def valid(self) -> bool:
        return self.origin.Valid()

    def __len__(self) -> int:
        return NetListAdapter.GetAccounts(self.origin).Count

    def __getitem__(self, row: int) -> 'Account':
        return Account.from_origin(NetListAdapter.GetAccounts(self.origin)[row])

    def __iter__(self):
        return iter(self.accounts())

    def accounts(self) -> Iterable:
        return AccountList(NetListAdapter.GetAccounts(self.origin))

    def posts(self) -> Iterable:
        return PostingList(NetListAdapter.GetPosts(self.origin))

    def has_xdata(self) -> bool:
        return self.origin.HasXData

    def clear_xdata(self):
        return self.origin.ClearXData()

    def xdata(self) -> AccountXData:
        return AccountXData.from_origin(self.origin.XData)

    def amount(self, expr: Expr = None) -> 'Value':
        if expr is None:
            return Value.to_value(self.origin.Amount())
        else:
            assert isinstance(expr, Expr)
            return Value.to_value(self.origin.Amount(False, expr.origin))

    def total(self, expr: Expr = None) -> 'Value':
        if expr is None:
            return Value.to_value(self.origin.Total())
        else:
            assert isinstance(expr, Expr)
            return Value.to_value(self.origin.Total(expr.origin))

    def self_details(self, gather_all: bool = None) -> AccountXDataDetails:
        if gather_all is None:
            return AccountXDataDetails.from_origin(self.origin.SelfDetails())
        else:
            return AccountXDataDetails.from_origin(self.origin.SelfDetails(gather_all))

    def family_details(self, gather_all: bool = None) -> AccountXDataDetails:
        if gather_all is None:
            return AccountXDataDetails.from_origin(self.origin.FamilyDetails())
        else:
            return AccountXDataDetails.from_origin(self.origin.FamilyDetails(gather_all))

    def has_xflags(self, flags: int) -> bool:
        return FlagsAdapter.AccountHasXFlags(self.origin, flags)

    def children_with_flags(self, to_display: bool, visited: bool) -> int:
        return self.origin.ChildrenWithFlags(to_display, visited)

###########################
# Ported from py_item.cc

class Scope:

    origin = None

    def __init__(self,origin) -> None:
        assert isinstance(origin, OriginScope)
        self.origin = origin

    @classmethod
    def from_origin(cls, origin):
        if origin is None:
            return None
        if isinstance(origin, OriginSession):
            return Session.from_origin(origin)
        if isinstance(origin, OriginPost):
            return Posting.from_origin(origin)
        if isinstance(origin, OriginXactBase):
            return TransactionBase.from_origin(origin)
        raise Exception("Unexpected origin type")

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

ITEM_NORMAL = ExportedConsts.ITEM_NORMAL
ITEM_GENERATED = ExportedConsts.ITEM_GENERATED
ITEM_TEMP = ExportedConsts.ITEM_TEMP

from NLedger.Items import ItemStateEnum as State    # State enum

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
    def metadata(self) -> Dict[str, Tuple]:
        result = {}
        data = self.origin.GetMetadata()
        if not data is None:
            for key_value in data:
                result[key_value.Key] = (Value.to_value(key_value.Value.Value), bool(key_value.Value.IsParsed)) if not key_value.Value is None else None
        return result

    def copy_details(self, item: 'JournalItem'):
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
    def sort_values(self) -> Iterable:
        return SortValueList(NetListAdapter.GetPostXDataSortValues(self.origin))

class Posting(JournalItem):

    def __init__(self, origin = None) -> None:
        if not origin is None:
            assert isinstance(origin, OriginPost)
        else:
            origin = OriginPost()

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

    @property
    def journal(self) -> 'Journal':
        return Journal.from_origin(self.origin.Journal)

    @journal.setter
    def journal(self, value: 'Journal'):
        self.origin.Journal = value.origin if not value is None else None

    def __len__(self) -> int:
        return NetListAdapter.GetPosts(self.origin).Count

    def __getitem__(self, row):
        return Posting.from_origin(NetListAdapter.GetPosts(self.origin)[row])

    def add_post(self, post: Posting):
        assert isinstance(post, Posting)
        self.origin.AddPost(post.origin)

    def remove_post(self, post: Posting) -> bool:
        assert isinstance(post, Posting)
        return self.origin.RemovePost(post.origin)

    def finalize(self) -> bool:
        return self.origin.FinalizeXact()

    def posts(self) -> Iterable:
        return PostingList(NetListAdapter.GetPosts(self.origin))

    def __iter__(self):
        return iter(self.posts())

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

    def id(self) -> str:
        return self.origin.Id

    def seq(self) -> int:
        return self.origin.Seq

    @property
    def code(self) -> str:
        return self.origin.Code

    @code.setter
    def code(self, value: str):
        self.origin.Code = value

    @property
    def payee(self) -> str:
        return self.origin.Payee

    @payee.setter
    def payee(self, value: str):
        self.origin.Payee = value

    def magnitude(self) -> 'Value':
        return Value.to_value(self.origin.Magnitude())

    def has_xdata(self) -> bool:
        return self.origin.HasXData

    def clear_xdata(self):
        self.origin.ClearXData()

    def __str__(self) -> str:
        return ""  # See py_xact.cc, py_xact_to_string, jww (2012-03-01)

class PeriodicTransaction(TransactionBase):

    def __init__(self, origin = None) -> None:
        if not origin is None:
            assert isinstance(origin, OriginPeriodXact)
        else:
            origin = OriginPeriodXact()
        super().__init__(origin)

    @classmethod
    def from_origin(cls, origin):
        return PeriodicTransaction(origin=origin) if not origin is None else None

    @property
    def period(self) -> DateInterval:
        return self.origin.Period

    @property
    def period_string(self) -> DateInterval:
        return self.origin.PeriodSting

class AutomatedTransaction(TransactionBase):

    def __init__(self, origin = None) -> None:
        if not origin is None:
            assert isinstance(origin, OriginAutoXact)
        else:
            origin = OriginAutoXact()
        super().__init__(origin)

    @classmethod
    def from_origin(cls, origin):
        return AutomatedTransaction(origin=origin) if not origin is None else None

    @property
    def predicate(self) -> Predicate:
        return self.origin.Predicate

    def extend_xact(self, xact_base: TransactionBase):
        self.origin.ExtendXact(xact_base.origin, None)

###########################
# Ported from py_journal.cc

class FileInfo:

    origin = None

    def __init__(self, val = None) -> None:
        if val is None:
            self.origin = OriginJournalFileInfo()
        elif isinstance(val, OriginJournalFileInfo):
            self.origin = val
        elif isinstance(val, str):
            self.origin = OriginJournalFileInfo(val)
        else:
            raise Exception("Unexpected argument type")

    @classmethod
    def from_origin(cls, origin):
        return FileInfo(origin) if not origin is None else None

    @property
    def filename(self) -> str:
        return self.origin.FileName

    @property
    def size(self) -> int:
        return self.origin.Size

    @property
    def modtime(self) -> datetime:
        return to_pdatetime(self.origin.ModTime)

    @property
    def from_stream(self) -> bool:
        return self.origin.FromStream

# Journal

class Journal:

    origin = None

    def __init__(self, origin = None) -> None:
        if origin is None:
            self.origin = OriginJournal()
        else:
            assert isinstance(origin, OriginJournal)
            self.origin = origin

    @classmethod
    def from_origin(cls, origin):
        return Journal(origin) if not origin is None else None

    @property
    def master(self) -> Account:
        return Account.from_origin(self.origin.Master)

    @master.setter
    def master(self, val: Account):
        self.origin.Master = val.origin if not val is None else None

    @property
    def bucket(self) -> Account:
        return Account.from_origin(self.origin.Bucket)

    @bucket.setter
    def bucket(self, val: Account):
        self.origin.Bucket = val.origin if not val is None else None

    def add_account(self, acct: Account):
        assert isinstance(acct, Account)
        self.origin.AddAccount(acct.origin)

    def remove_account(self, acct: Account) -> bool:
        assert isinstance(acct, Account)
        return self.origin.RemoveAccount(acct.origin)

    def find_account(self, name: str, auto_create: bool = None) -> Account:
        assert isinstance(name, str)
        if auto_create is None:
            return Account.from_origin(self.origin.FindAccount(name))
        else:
            return Account.from_origin(self.origin.FindAccount(name, auto_create))

    def find_account_re(self, regexp: str) -> Account:
        assert isinstance(regexp, str)
        return Account.from_origin(self.origin.FindAccountRe(regexp))

    def register_account(self, name: str, post: Posting) -> Account:
        assert isinstance(name, str) or name is None
        assert isinstance(post, Posting) or post is None
        master = self.master
        return Account.from_origin(self.origin.RegisterAccount(name, (post.origin if not post is None else None), (master.origin if not master is None else None)))

    def expand_aliases(self, name: str) -> Account:
        assert isinstance(name, str)
        return Account.from_origin(self.origin.ExpandAliases(name))

    def add_xact(self, xact: Transaction) -> bool:
        assert isinstance(xact, Transaction)
        return self.origin.AddXact(xact.origin)

    def remove_xact(self, xact: Transaction) -> bool:
        assert isinstance(xact, Transaction)
        return self.origin.RemoveXact(xact.origin)

    def __len__(self) -> int:
        return NetListAdapter.GetXacts(self.origin).Count

    def __getitem__(self, row: int) -> 'Transaction':
        return Transaction.from_origin(NetListAdapter.GetXacts(self.origin)[row])

    def __iter__(self):
        return iter(self.xacts())

    def xacts(self) -> Iterable:
        return TransactionList(NetListAdapter.GetXacts(self.origin))

    def auto_xacts(self) -> Iterable:
        return AutomatedTransactionList(NetListAdapter.GetAutoXacts(self.origin))

    def period_xacts(self) -> Iterable:
        return PeriodicTransactionList(NetListAdapter.GetPeriodXacts(self.origin))

    def sources(self) -> Iterable:
        return FileInfoList(NetListAdapter.GetFileInfos(self.origin))

    def has_xdata(self) -> bool:
        return self.origin.HasXData()

    def clear_xdata(self):
        self.origin.ClearXData()

    def query(self, query_text:str) -> Iterable:
        return PostingList(NetListAdapter.GetQuery(self.origin, query_text))

    def valid(self) -> bool:
        return self.origin.Valid()

###########################
# Ported from py_session.cc

# Session

class Session(Scope):

    origin = None

    def __init__(self, origin) -> None:
        assert isinstance(origin, OriginSession)
        self.origin = origin

    @classmethod
    def from_origin(cls, origin):
        return Session(origin=origin) if not origin is None else None

    def read_journal(self, path_name: str) -> Journal:
        return Journal.from_origin(self.origin.ReadJournal(path_name))

    def read_journal_from_string(self, data: str) -> Journal:
        return Journal.from_origin(self.origin.ReadJournalFromString(data))

    def read_journal_files(self) -> Journal:
        return Journal.from_origin(self.origin.ReadJournalFiles())

    def close_journal_files(self):
        self.origin.CloseJournalFiles()

    def journal(self) -> Journal:
        return Journal.from_origin(self.origin.Journal)

session = Session(ExtendedSession.Current)

def read_journal(path_name: str) -> Journal:
    assert isinstance(session, Session)
    session.close_journal_files()   # [DM] preventive cleaning added for simplifying repetitive readings
    return session.read_journal(path_name)

def read_journal_from_string(data: str) -> Journal:
    assert isinstance(session, Session)
    return session.read_journal_from_string(data)

# Values

class Mask:

    origin = None

    def __init__(self, val = None) -> None:
        if val is None:
            self.origin = OriginMask()
        elif isinstance(val, OriginMask):
            self.origin = val
        else:
            self.origin = OriginMask(val)

    @classmethod
    def from_origin(cls, origin):
        return Mask(origin) if not origin is None else None

    def match(self, text: str) -> bool:
        return self.origin.Match(text)

    @property
    def is_empty(self) -> bool:
        return self.origin.IsEmpty

    def str(self) -> str:
        return self.origin.Str()

    __str__ = str

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
