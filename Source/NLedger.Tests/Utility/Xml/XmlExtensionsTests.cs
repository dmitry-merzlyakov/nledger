// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Utility.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NLedger.Tests.Utility.Xml
{
    [TestClass]
    public class XmlExtensionsTests
    {
        [TestMethod]
        public void XmlExtensions_CreateLedgerDoc_CreatesEmptyXmlDocument()
        {
            string s = "<ledger version=\"196867\" />";
            Assert.AreEqual(s, XmlExtensions.CreateLedgerDoc().ToString());
        }

        [TestMethod]
        public void XmlExtensions_AddElement_AddsElementWithGivenNameToContainer()
        {
            XDocument xdoc = XmlExtensions.CreateLedgerDoc();
            XElement xelem = xdoc.Root.AddElement("test");

            string expectedDoc =
@"<ledger version=""196867"">
  <test />
</ledger>";
            Assert.AreEqual(expectedDoc.Replace("\r\n", "\n"), xdoc.ToString().Replace("\r\n", "\n"));
            Assert.AreEqual("<test />", xelem.ToString());
        }

        [TestMethod]
        public void XmlExtensions_AddElement_AddsElementWithGivenNameAndValueToContainer()
        {
            XDocument xdoc = XmlExtensions.CreateLedgerDoc();
            XElement xelem = xdoc.Root.AddElement("test", "val");

            string expectedDoc =
@"<ledger version=""196867"">
  <test>val</test>
</ledger>";
            Assert.AreEqual(expectedDoc.Replace("\r\n", "\n"), xdoc.ToString().Replace("\r\n", "\n"));
            Assert.AreEqual("<test>val</test>", xelem.ToString());
        }

    }
}
