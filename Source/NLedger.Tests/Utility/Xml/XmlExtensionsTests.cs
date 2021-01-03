// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utility.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace NLedger.Tests.Utility.Xml
{
    public class XmlExtensionsTests
    {
        [Fact]
        public void XmlExtensions_CreateLedgerDoc_CreatesEmptyXmlDocument()
        {
            string s = "<ledger version=\"197121\" />";
            Assert.Equal(s, XmlExtensions.CreateLedgerDoc().ToString());
        }

        [Fact]
        public void XmlExtensions_AddElement_AddsElementWithGivenNameToContainer()
        {
            XDocument xdoc = XmlExtensions.CreateLedgerDoc();
            XElement xelem = xdoc.Root.AddElement("test");

            string expectedDoc =
@"<ledger version=""197121"">
  <test />
</ledger>";
            Assert.Equal(expectedDoc.Replace("\r\n", "\n"), xdoc.ToString().Replace("\r\n", "\n"));
            Assert.Equal("<test />", xelem.ToString());
        }

        [Fact]
        public void XmlExtensions_AddElement_AddsElementWithGivenNameAndValueToContainer()
        {
            XDocument xdoc = XmlExtensions.CreateLedgerDoc();
            XElement xelem = xdoc.Root.AddElement("test", "val");

            string expectedDoc =
@"<ledger version=""197121"">
  <test>val</test>
</ledger>";
            Assert.Equal(expectedDoc.Replace("\r\n", "\n"), xdoc.ToString().Replace("\r\n", "\n"));
            Assert.Equal("<test>val</test>", xelem.ToString());
        }

    }
}
