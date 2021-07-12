# NLedger Python Extensibility module tests (ledger)

import unittest
import ntpath
import os
import os.path
import sys
import re

# We need to find path to the latest NLedger.Extensibility.Python.dll. 
# Since this test is intended to be run on dev environment only, the function below looks for the dll by Debug/Release paths only
def get_nledger_dll_path():
    debugDLL = ntpath.abspath(ntpath.join(ntpath.dirname(ntpath.realpath(__file__)), '../../../../Source/NLedger.Extensibility.Python/bin/Debug/netstandard2.0/NLedger.Extensibility.Python.dll'))
    releaseDLL = ntpath.abspath(ntpath.join(ntpath.dirname(ntpath.realpath(__file__)), '../../../../Source/NLedger.Extensibility.Python/bin/Release/netstandard2.0/NLedger.Extensibility.Python.dll'))

    if os.path.isfile(releaseDLL):
        if os.path.isfile(debugDLL):
            debugMTXDate = os.path.getmtime(debugDLL)
            releaseMTXDate = os.path.getmtime(releaseDLL)
            if debugMTXDate < releaseMTXDate:
                return releaseDLL
            else:
                return debugDLL
        else:
            return releaseDLL
    else:
        if os.path.isfile(debugDLL):
            return debugDLL

    raise Exception("Cannot find MTX.dll by paths: " + debugDLL + " and " + releaseDLL)

# Configure ledger module to use found NLedger.dll (it uses a pre-defined internal path by default)
nledger_dll_path = get_nledger_dll_path()
os.environ["nledger_extensibility_python_dll_path"] = nledger_dll_path
print("Found path to NLedger Python dll: " + nledger_dll_path)

# Import ledger. Note: it is important to add path to source code to the first position to override already installed module
sys.path.insert(0, ntpath.join(ntpath.dirname(ntpath.realpath(__file__)), '..', 'src'))
import ledger
from ledger import Amount
print("Module ledger is properly imported")

from datetime import datetime
from datetime import date
from System import DateTime
from NLedger.Utility import Date

from NLedger.Annotate import Annotation as OriginAnnotation

class LedgerModuleTests(unittest.TestCase):

    def test_commodity_scope_attributes(self):

        self.assertEqual(0, ledger.COMMODITY_STYLE_DEFAULTS)
        self.assertEqual(1, ledger.COMMODITY_STYLE_SUFFIXED)
        self.assertEqual(2, ledger.COMMODITY_STYLE_SEPARATED)
        self.assertEqual(4, ledger.COMMODITY_STYLE_DECIMAL_COMMA)
        self.assertEqual(8, ledger.COMMODITY_STYLE_THOUSANDS)
        self.assertEqual(0x10, ledger.COMMODITY_NOMARKET)
        self.assertEqual(0x20, ledger.COMMODITY_BUILTIN)
        self.assertEqual(0x40, ledger.COMMODITY_WALKED)
        self.assertEqual(0x80, ledger.COMMODITY_KNOWN)
        self.assertEqual(0x1000, ledger.COMMODITY_STYLE_TIME_COLON)

        self.assertIsNotNone(0x1000, ledger.commodities)
        self.assertEqual("<class 'ledger.CommodityPool'>", str(type(ledger.commodities)))

    def test_to_pdate(self):

        self.assertIsNone(ledger.to_pdate(None))

        ndatetime = DateTime(2021, 5, 22, 23, 55, 50, 99)
        self.assertEqual(date(2021, 5, 22), ledger.to_pdate(ndatetime))

        ndate = Date(2021, 5, 22)
        self.assertEqual(date(2021, 5, 22), ledger.to_pdate(ndate))

        pdatetime = datetime(2021, 5, 22, 23, 55, 50, 99)
        self.assertEqual(date(2021, 5, 22), ledger.to_pdate(pdatetime))

        pdate = date(2021, 5, 22)
        self.assertEqual(date(2021, 5, 22), ledger.to_pdate(pdate))

    def test_to_pdatetime(self):

        self.assertIsNone(ledger.to_pdatetime(None))

        ndatetime = DateTime(2021, 5, 22, 23, 55, 50, 99)
        self.assertEqual(datetime(2021, 5, 22, 23, 55, 50, 99), ledger.to_pdatetime(ndatetime))

        ndate = Date(2021, 5, 22)
        self.assertEqual(datetime(2021, 5, 22, 0, 0, 0, 0), ledger.to_pdatetime(ndate))

        pdatetime = datetime(2021, 5, 22, 23, 55, 50, 99)
        self.assertEqual(datetime(2021, 5, 22, 23, 55, 50, 99), ledger.to_pdatetime(pdatetime))

        pdate = date(2021, 5, 22)
        self.assertEqual(datetime(2021, 5, 22, 0, 0, 0, 0), ledger.to_pdatetime(pdate))

    def test_to_ndate(self):

        self.assertEqual(None, ledger.to_ndate(None))

        pdatetime = datetime(2021, 5, 22, 23, 55, 50, 99)
        self.assertEqual(Date(2021, 5, 22), ledger.to_ndate(pdatetime))

        pdate = date(2021, 5, 22)
        self.assertEqual(Date(2021, 5, 22), ledger.to_ndate(pdate))

        ndatetime = DateTime(2021, 5, 22, 23, 55, 50, 99)
        self.assertEqual(Date(2021, 5, 22), ledger.to_ndate(ndatetime))

        ndate = Date(2021, 5, 22)
        self.assertEqual(Date(2021, 5, 22), ledger.to_ndate(ndate))

    def test_to_ndatetime(self):

        self.assertEqual(None, ledger.to_ndatetime(None))

        pdatetime = datetime(2021, 5, 22, 23, 55, 50, 99)
        self.assertEqual(DateTime(2021, 5, 22, 23, 55, 50, 99), ledger.to_ndatetime(pdatetime))

        pdate = date(2021, 5, 22)
        self.assertEqual(DateTime(2021, 5, 22), ledger.to_ndatetime(pdate))

        ndatetime = DateTime(2021, 5, 22, 23, 55, 50, 99)
        self.assertEqual(DateTime(2021, 5, 22, 23, 55, 50, 99), ledger.to_ndatetime(ndatetime))

        ndate = Date(2021, 5, 22)
        self.assertEqual(DateTime(2021, 5, 22), ledger.to_ndatetime(ndate))


# Commodities

class CommodityPoolTests(unittest.TestCase):

    def test_commodity_pool_null_commodity(self):

        null_commodity = ledger.commodities.null_commodity
        self.assertIsNotNone(null_commodity)
        self.assertFalse(bool(null_commodity.symbol))
        self.assertEqual("<class 'ledger.Commodity'>", str(type(null_commodity)))

    def test_commodity_pool_null_commodity(self):

        default_commodity = ledger.commodities.default_commodity
        self.assertIsNone(default_commodity)

    def test_commodity_pool_keep_base(self):

        commodity_pool = ledger.commodities
        keep_base = commodity_pool.keep_base

        commodity_pool.keep_base = True
        self.assertTrue(commodity_pool.keep_base)

        commodity_pool.keep_base = False
        self.assertFalse(commodity_pool.keep_base)

        commodity_pool.keep_base = keep_base
        self.assertEqual(keep_base, commodity_pool.keep_base)

    def test_commodity_pool_price_db(self):

        commodity_pool = ledger.commodities
        price_db = commodity_pool.price_db

        commodity_pool.price_db = "some-price_db"
        self.assertEqual("some-price_db", commodity_pool.price_db)

        commodity_pool.price_db = price_db
        self.assertEqual(price_db, commodity_pool.price_db)

    def test_commodity_pool_quote_leeway(self):

        commodity_pool = ledger.commodities
        quote_leeway = commodity_pool.quote_leeway

        commodity_pool.quote_leeway = 111
        self.assertEqual(111, commodity_pool.quote_leeway)

        commodity_pool.quote_leeway = quote_leeway
        self.assertEqual(quote_leeway, commodity_pool.quote_leeway)

    def test_commodity_pool_get_quotes(self):

        commodity_pool = ledger.commodities
        get_quotes = commodity_pool.get_quotes

        commodity_pool.get_quotes = True
        self.assertTrue(commodity_pool.get_quotes)

        commodity_pool.get_quotes = False
        self.assertFalse(commodity_pool.get_quotes)

        commodity_pool.get_quotes = get_quotes
        self.assertEqual(get_quotes, commodity_pool.get_quotes)

    def test_commodity_pool_create(self):

        commodity_pool = ledger.commodities
        commodity = commodity_pool.create("XYZ1")

        self.assertIsNotNone(commodity)
        self.assertEqual("<class 'ledger.Commodity'>", str(type(commodity)))
        self.assertEqual('"XYZ1"', commodity.symbol)

class AnnotationTests(unittest.TestCase):

    def test_annotation_eq(self):

        annotation1 = ledger.Annotation(OriginAnnotation(None, None, None))
        annotation2 = ledger.Annotation(OriginAnnotation(None, None, None))
        self.assertTrue(annotation1 == annotation2)
        self.assertFalse(annotation1 != annotation2)

        annotation1 = ledger.Annotation(OriginAnnotation(ledger.Amount(10), Date(2021, 5, 10), "tag"))
        annotation2 = ledger.Annotation(OriginAnnotation(ledger.Amount(10), Date(2021, 5, 10), "tag"))
        self.assertTrue(annotation1 == annotation2)
        self.assertFalse(annotation1 != annotation2)

        annotation1 = ledger.Annotation(OriginAnnotation(None, Date(2021, 5, 10), "tag1"))
        annotation2 = ledger.Annotation(OriginAnnotation(None, Date(2021, 5, 10), "tag2"))
        self.assertFalse(annotation1 == annotation2)
        self.assertTrue(annotation1 != annotation2)

    def test_annotation_bool(self):

        self.assertFalse(bool(ledger.Annotation(OriginAnnotation(None, None, None))))
        self.assertTrue(bool(ledger.Annotation(OriginAnnotation(ledger.Amount(10), None, None))))
        self.assertTrue(bool(ledger.Annotation(OriginAnnotation(None, Date(2021, 5, 10), None))))
        self.assertTrue(bool(ledger.Annotation(OriginAnnotation(None, None, "tag"))))

    def test_annotation_flags(self):

        annotation1 = ledger.Annotation(OriginAnnotation(None, None, None))
        self.assertEqual(0, annotation1.flags)
        annotation1.flags = ledger.ANNOTATION_DATE_CALCULATED | ledger.ANNOTATION_PRICE_FIXATED
        self.assertEqual(ledger.ANNOTATION_DATE_CALCULATED | ledger.ANNOTATION_PRICE_FIXATED, annotation1.flags)

    def test_annotation_has_flags(self):

        annotation1 = ledger.Annotation(OriginAnnotation(None, None, None))
        annotation1.flags = ledger.ANNOTATION_DATE_CALCULATED | ledger.ANNOTATION_PRICE_FIXATED

        self.assertTrue(annotation1.has_flags(ledger.ANNOTATION_DATE_CALCULATED))
        self.assertTrue(annotation1.has_flags(ledger.ANNOTATION_PRICE_FIXATED))
        self.assertTrue(annotation1.has_flags(ledger.ANNOTATION_DATE_CALCULATED | ledger.ANNOTATION_PRICE_FIXATED))
        self.assertFalse(annotation1.has_flags(ledger.ANNOTATION_VALUE_EXPR_CALCULATED))

    def test_annotation_clear_flags(self):

        annotation1 = ledger.Annotation(OriginAnnotation(None, None, None))
        annotation1.flags = ledger.ANNOTATION_DATE_CALCULATED | ledger.ANNOTATION_PRICE_FIXATED

        annotation1.clear_flags()
        self.assertEqual(0, annotation1.flags)

    def test_annotation_add_flags(self):

        annotation1 = ledger.Annotation(OriginAnnotation(None, None, None))
        annotation1.flags = ledger.ANNOTATION_DATE_CALCULATED

        annotation1.add_flags(ledger.ANNOTATION_PRICE_FIXATED)
        self.assertTrue(annotation1.has_flags(ledger.ANNOTATION_DATE_CALCULATED | ledger.ANNOTATION_PRICE_FIXATED))

    def test_annotation_drop_flags(self):

        annotation1 = ledger.Annotation(OriginAnnotation(None, None, None))
        annotation1.flags = ledger.ANNOTATION_DATE_CALCULATED | ledger.ANNOTATION_PRICE_FIXATED

        annotation1.drop_flags(ledger.ANNOTATION_PRICE_FIXATED)
        self.assertFalse(annotation1.has_flags(ledger.ANNOTATION_PRICE_FIXATED))
        self.assertTrue(annotation1.has_flags(ledger.ANNOTATION_DATE_CALCULATED))

    def test_annotation_price(self):

        annotation1 = ledger.Annotation(OriginAnnotation(ledger.Amount(10), None, None))
        price = annotation1.price
        self.assertTrue(isinstance(price, ledger.Amount))
        self.assertEqual(ledger.Amount(10), price)
        annotation1.price = ledger.Amount(20)
        self.assertEqual(ledger.Amount(20), annotation1.price)

    def test_annotation_date(self):

        annotation1 = ledger.Annotation(OriginAnnotation(None, Date(2021, 5, 22), None))
        date1 = annotation1.date
        self.assertTrue(isinstance(date1, date))
        self.assertEqual(date(2021, 5, 22), date1)
        annotation1.date = date(2021, 5, 23)
        self.assertEqual(date(2021, 5, 23), annotation1.date)

    def test_annotation_tag(self):

        annotation1 = ledger.Annotation(OriginAnnotation(None, None, "tag-1"))
        tag1 = annotation1.tag
        self.assertTrue(isinstance(tag1, str))
        self.assertEqual("tag-1", tag1)
        annotation1.tag = "tag-2"
        self.assertEqual("tag-2", annotation1.tag)

    def test_annotation_valid(self):

        annotation1 = ledger.Annotation(OriginAnnotation(ledger.Amount(10), None, "tag-1"))
        self.assertTrue(annotation1.valid())

# Amount tests

class AmountTests(unittest.TestCase):

    def test_amount_number_constructors(self):

        amount = ledger.Amount(10)
        self.assertEqual("10",str(amount))

        amount = ledger.Amount(20)
        self.assertEqual("20",str(amount))

    def test_amount_str_constructors(self):

        amount = ledger.Amount("10")
        self.assertEqual("10",str(amount))

        amount = ledger.Amount("20 EUR")
        self.assertEqual("20 EUR",str(amount))

    def test_amount_exact(self):

        amount = ledger.Amount.exact("20 EUR")
        self.assertEqual("20 EUR",str(amount))

    def test_amount_classname(self):

        amount = ledger.Amount(10)
        self.assertEqual("<class 'ledger.Amount'>",str(type(amount)))

    def test_amount_eq(self):

        # Amount vs amount

        a1 = ledger.Amount("20 EUR")
        a2 = ledger.Amount("30 EUR")
        a3 = ledger.Amount("20 EUR")

        self.assertFalse(a1 == a2)
        self.assertTrue(a1 != a2)

        self.assertTrue(a1 == a3)
        self.assertFalse(a1 != a3)

        # Amount vs number

        a1 = ledger.Amount("20")
        
        self.assertTrue(a1 == 20)
        self.assertFalse(a1 != 20)
        self.assertFalse(a1 == 30)
        self.assertTrue(a1 != 30)

        self.assertTrue(20 == a1)
        self.assertFalse(20 != a1)
        self.assertFalse(30 == a1)
        self.assertTrue(30 != a1)

    def test_amount_bool(self):

        a1 = ledger.Amount(0)
        a2 = ledger.Amount(10)

        self.assertFalse(bool(a1))
        self.assertTrue(bool(a2))

        self.assertFalse(a1)
        self.assertTrue(a2)

        self.assertTrue(not a1)
        self.assertFalse(not a2)

    def test_amount_less(self):

        a1 = ledger.Amount(0)
        a2 = ledger.Amount(10)
        a3 = ledger.Amount(10)

        self.assertTrue(a1 < a2)
        self.assertTrue(a1 <= a2)

        self.assertFalse(a2 < a1)
        self.assertFalse(a2 <= a1)

        self.assertFalse(a2 < a3)
        self.assertTrue(a2 <= a3)

        self.assertFalse(a3 < a2)
        self.assertTrue(a3 <= a2)

        self.assertTrue(0 < a2)
        self.assertTrue(0 <= a2)

        self.assertFalse(a2 < 0)
        self.assertFalse(a2 <= 0)

        self.assertFalse(10 < a3)
        self.assertTrue(10 <= a3)

        self.assertFalse(a3 < 10)
        self.assertTrue(a3 <= 10)

    def test_amount_greater(self):

        a1 = ledger.Amount(0)
        a2 = ledger.Amount(10)
        a3 = ledger.Amount(10)

        self.assertFalse(a1 > a2)
        self.assertFalse(a1 >= a2)

        self.assertTrue(a2 > a1)
        self.assertTrue(a2 >= a1)

        self.assertFalse(a2 > a3)
        self.assertTrue(a2 >= a3)

        self.assertFalse(a3 > a2)
        self.assertTrue(a3 >= a2)

        self.assertFalse(0 > a2)
        self.assertFalse(0 >= a2)

        self.assertTrue(a2 > 0)
        self.assertTrue(a2 >= 0)

        self.assertFalse(10 > a3)
        self.assertTrue(10 >= a3)

        self.assertFalse(a3 > 10)
        self.assertTrue(a3 >= 10)

    def test_amount_plus(self):

        a1 = ledger.Amount(10)
        a2 = ledger.Amount(10)
        a3 = ledger.Amount(20)

        self.assertEqual(a3, a1 + a2)
        self.assertEqual(a3, a1 + 10)
        self.assertEqual(a3, 10 + a1)

        a1 += a2
        a2 += 10
        self.assertEqual(a3, a1)
        self.assertEqual(a3, a2)

        self.assertEqual("<class 'ledger.Amount'>", str(type(a1 + a2)))
        self.assertEqual("<class 'ledger.Amount'>", str(type(10 + a2)))

    def test_amount_minus(self):

        a1 = ledger.Amount(30)
        a2 = ledger.Amount(10)
        a3 = ledger.Amount(20)

        self.assertEqual(a3, a1 - a2)
        self.assertEqual(a3, a1 - 10)
        self.assertEqual(a3, 30 - a2)

        a1 -= a2
        self.assertEqual(a3, a1)
        a3 -= 10
        self.assertEqual(a2, a3)

        self.assertEqual("<class 'ledger.Amount'>", str(type(a1 - a2)))
        self.assertEqual("<class 'ledger.Amount'>", str(type(10 - a2)))

    def test_amount_multiply(self):

        a1 = ledger.Amount(2)
        a2 = ledger.Amount(3)
        a3 = ledger.Amount(6)

        self.assertEqual(a3, a1 * a2)
        self.assertEqual(a3, a1 * 3)
        self.assertEqual(a3, 2 * a2)

        a1 *= a2
        a2 *= 2
        self.assertEqual(a3, a1)
        self.assertEqual(a3, a2)

        self.assertEqual("<class 'ledger.Amount'>", str(type(a1 * a2)))
        self.assertEqual("<class 'ledger.Amount'>", str(type(10 * a2)))

    def test_amount_divide(self):

        a1 = ledger.Amount(6)
        a2 = ledger.Amount(3)
        a3 = ledger.Amount(2)

        self.assertEqual(a3, a1 / a2)
        self.assertEqual(a3, a1 / 3)
        self.assertEqual(a2, 6 / a3)

        a1 /= a2
        self.assertEqual(a3, a1)

        a1 = ledger.Amount(6)
        a1 /= 2
        self.assertEqual(a2, a1)

        self.assertEqual("<class 'ledger.Amount'>", str(type(a2 / a2)))
        self.assertEqual("<class 'ledger.Amount'>", str(type(6 / a2)))

    def test_amount_neg(self):

        a1 = ledger.Amount(6)
        a2 = ledger.Amount(-6)

        self.assertEqual(a1, -a2)
        self.assertEqual(a2, -a1)
        self.assertEqual("<class 'ledger.Amount'>", str(type(-a1)))

    def test_amount_precision(self):

        a1 = ledger.Amount(10)
        self.assertEqual(0, a1.precision)

        a1 = ledger.Amount("11.11 ZXA")
        self.assertEqual(2, a1.precision)

    def test_amount_display_precision(self):

        a1 = ledger.Amount(10)
        self.assertEqual(0, a1.display_precision)

        a1 = ledger.Amount("11.11 ZXB")
        self.assertEqual(2, a1.display_precision)

    def test_amount_keep_precision(self):

        a1 = ledger.Amount(10)
        self.assertFalse(a1.keep_precision)
        a1.keep_precision = True
        self.assertTrue(a1.keep_precision)
        a1.keep_precision = False
        self.assertFalse(a1.keep_precision)

    def test_amount_negated(self):

        a1 = ledger.Amount(10)
        a2 = a1.negated()

        self.assertEqual(ledger.Amount(-10), a2)
        self.assertEqual(ledger.Amount(10), a1)
        self.assertEqual("<class 'ledger.Amount'>", str(type(a2)))

    def test_amount_in_place_negate(self):

        a1 = ledger.Amount(10)
        a2 = a1.in_place_negate()

        self.assertEqual(ledger.Amount(-10), a2)
        self.assertEqual(ledger.Amount(-10), a1)
        self.assertEqual("<class 'ledger.Amount'>", str(type(a2)))

    def test_amount_abs(self):

        a1 = ledger.Amount(-10)
        a2 = a1.abs()

        self.assertEqual(ledger.Amount(10), a2)
        self.assertEqual(ledger.Amount(-10), a1)
        self.assertEqual("<class 'ledger.Amount'>", str(type(a2)))

    def test_amount_inverted(self):

        a1 = ledger.Amount(10)
        a2 = a1.inverted()

        self.assertEqual(ledger.Amount(0.1), a2)
        self.assertEqual(ledger.Amount(10), a1)
        self.assertEqual("<class 'ledger.Amount'>", str(type(a2)))

    def test_amount_rounded(self):

        a1 = ledger.Amount("2.00 ZXC")
        a1.keep_precision = True
        a2 = a1 / 3

        self.assertEqual("2.00 ZXC", str(a1))
        self.assertEqual("0.66666667 ZXC", str(a2))
        self.assertEqual("0.67 ZXC", str(a2.rounded()))
        self.assertEqual("<class 'ledger.Amount'>", str(type(a2.rounded())))

    def test_amount_in_place_round(self):

        a1 = ledger.Amount("2.00 ZXC")
        a1.keep_precision = True

        a1 = a1 / 3
        
        self.assertEqual("0.66666667 ZXC", str(a1))
        a1.in_place_round()
        self.assertEqual("0.67 ZXC", str(a1))
        self.assertEqual("<class 'ledger.Amount'>", str(type(a1)))

    def test_amount_truncated(self):

        a1 = ledger.Amount("2.00 ZXC")
        a2 = a1 / 3

        self.assertEqual("2.00 ZXC", str(a1))
        self.assertEqual("0.67 ZXC", str(a2))
        self.assertEqual("0.67 ZXC", str(a2.truncated()))
        self.assertEqual("<class 'ledger.Amount'>", str(type(a2.truncated())))

    def test_amount_in_place_truncate(self):

        a1 = ledger.Amount("2.00 ZXC")
        a1 = a1 / 3
        
        a1.in_place_truncate()
        self.assertEqual("0.67 ZXC", str(a1))
        self.assertEqual("<class 'ledger.Amount'>", str(type(a1)))

    def test_amount_floored(self):

        a1 = ledger.Amount("2.00 ZXD")
        a2 = a1 / 3

        self.assertEqual("2.00 ZXD", str(a1))
        self.assertEqual("0.67 ZXD", str(a2))
        self.assertEqual("0.00 ZXD", str(a2.floored()))
        self.assertEqual("<class 'ledger.Amount'>", str(type(a2.floored())))

    def test_amount_in_place_floor(self):

        a1 = ledger.Amount("2.00 ZXD")
        a1 = a1 / 3
        
        a1.in_place_floor()
        self.assertEqual("0.00 ZXD", str(a1))
        self.assertEqual("<class 'ledger.Amount'>", str(type(a1)))

    def test_amount_unrounded(self):

        a1 = ledger.Amount("2.00 ZXD")
        a2 = a1 / 3

        self.assertEqual("2.00 ZXD", str(a1))
        self.assertEqual("0.67 ZXD", str(a2))
        self.assertEqual("0.66666667 ZXD", str(a2.unrounded()))
        self.assertEqual("<class 'ledger.Amount'>", str(type(a2.unrounded())))

    def test_amount_in_place_unround(self):

        a1 = ledger.Amount("2.00 ZXD")
        a1 = a1 / 3
        
        a1.in_place_unround()
        self.assertEqual("0.66666667 ZXD", str(a1))
        self.assertEqual("<class 'ledger.Amount'>", str(type(a1)))

    def test_amount_reduced(self):

        a1 = ledger.Amount("2.00 ZXD")
        a2 = a1 / 3

        self.assertEqual("2.00 ZXD", str(a1))
        self.assertEqual("0.67 ZXD", str(a2))
        self.assertEqual("0.67 ZXD", str(a2.reduced()))
        self.assertEqual("<class 'ledger.Amount'>", str(type(a2.reduced())))

    def test_amount_in_place_reduce(self):

        a1 = ledger.Amount("2.00 ZXD")
        a1 = a1 / 3
        
        a1.in_place_reduce()
        self.assertEqual("0.67 ZXD", str(a1))
        self.assertEqual("<class 'ledger.Amount'>", str(type(a1)))

    def test_amount_unreduced(self):

        a1 = ledger.Amount("2.00 ZXD")
        a2 = a1 / 3

        self.assertEqual("2.00 ZXD", str(a1))
        self.assertEqual("0.67 ZXD", str(a2))
        self.assertEqual("0.67 ZXD", str(a2.unreduced()))
        self.assertEqual("<class 'ledger.Amount'>", str(type(a2.unreduced())))

    def test_amount_in_place_unreduce(self):

        a1 = ledger.Amount("2.00 ZXD")
        a1 = a1 / 3
        
        a1.in_place_unreduce()
        self.assertEqual("0.67 ZXD", str(a1))
        self.assertEqual("<class 'ledger.Amount'>", str(type(a1)))


if __name__ == '__main__':
    unittest.main()
