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
print("Module mtx_licensing is properly imported")

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
        self.assertEqual("EUR20",str(amount))

    def test_amount_classname(self):

        amount = ledger.Amount(10)
        self.assertEqual("<class 'ledger.Amount'>",str(type(amount)))


if __name__ == '__main__':
    unittest.main()
