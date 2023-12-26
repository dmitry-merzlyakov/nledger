// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Amounts;
using NLedger.Extensibility.Python.Platform;
using NLedger.Utility;
using NLedger.Values;
using NLedger.Xacts;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Extensibility.Python.Tests
{
    public class PythonValueConverterTests : IDisposable
    {
        public PythonValueConverterTests()
        {
            Assert.True(PythonConnector.Current.IsAvailable);
            PythonConnectionContext = PythonConnector.Current.Connect();
            PythonConnector.Current.KeepAlive = false;
        }

        public PythonConnectionContext PythonConnectionContext { get; }

        public void Dispose()
        {
            PythonConnectionContext.Dispose();
        }

        [PythonFact]
        public void PythonValueConverter_GetObject_Conversions()
        {
            PythonSession.PythonModuleInitialization();
            try
            {
                PythonSession.Current.Initialize();
                using(PythonSession.Current.GIL())
                {
                    var converter = new PythonValueConverter(PythonSession.Current);
                    Value val = null;
                    PyObject py = null;

                    // Boolean/False
                    val = Value.Get(false);
                    Assert.Equal(ValueTypeEnum.Boolean, val.Type);
                    py = converter.GetObject(val);
                    Assert.Equal("bool", py.GetPythonTypeName());
                    Assert.Equal("False", py.ToString());

                    // Boolean/True
                    val = Value.Get(true);
                    Assert.Equal(ValueTypeEnum.Boolean, val.Type);
                    py = converter.GetObject(val);
                    Assert.Equal("bool", py.GetPythonTypeName());
                    Assert.Equal("True", py.ToString());

                    // Integer/10
                    val = Value.Get(10);
                    Assert.Equal(ValueTypeEnum.Integer, val.Type);
                    py = converter.GetObject(val);
                    Assert.Equal("int", py.GetPythonTypeName());
                    Assert.Equal("10", py.ToString());

                    // DateTime/2021-10-20 23:55:50
                    val = Value.Get(new DateTime(2021, 10, 20, 23, 55, 50));
                    Assert.Equal(ValueTypeEnum.DateTime, val.Type);
                    py = converter.GetObject(val);
                    Assert.Equal("datetime", py.GetPythonTypeName());
                    Assert.Equal("2021-10-20 23:55:50", py.ToString());

                    // Date/2021-10-20
                    val = Value.Get(new Date(2021, 10, 20));
                    Assert.Equal(ValueTypeEnum.Date, val.Type);
                    py = converter.GetObject(val);
                    Assert.Equal("date", py.GetPythonTypeName());
                    Assert.Equal("2021-10-20", py.ToString());

                    // String/some-string
                    val = Value.StringValue("some-string");
                    Assert.Equal(ValueTypeEnum.String, val.Type);
                    py = converter.GetObject(val);
                    Assert.Equal("str", py.GetPythonTypeName());
                    Assert.Equal("some-string", py.ToString());

                    // Amount/10
                    val = Value.Get(new Amount(10));
                    Assert.Equal(ValueTypeEnum.Amount, val.Type);
                    py = converter.GetObject(val);
                    Assert.Equal("Amount", py.GetPythonTypeName());
                    Assert.Equal("10", py.ToString());

                    // Balance/10
                    val = Value.Get(new Balance(10));
                    Assert.Equal(ValueTypeEnum.Balance, val.Type);
                    py = converter.GetObject(val);
                    Assert.Equal("Balance", py.GetPythonTypeName());
                    Assert.Equal("10", py.ToString());

                    // Mask/mask
                    val = Value.MaskValue("mask");
                    Assert.Equal(ValueTypeEnum.Mask, val.Type);
                    py = converter.GetObject(val);
                    Assert.Equal("Value", py.GetPythonTypeName());
                    Assert.Equal("mask", py.ToString());

                    // Sequence
                    val = Value.Get(new List<Value>() { Value.Get(10), Value.Get(20), Value.Get(30) });
                    Assert.Equal(ValueTypeEnum.Sequence, val.Type);
                    py = converter.GetObject(val);
                    Assert.Equal("list", py.GetPythonTypeName());
                    Assert.Equal("[10, 20, 30]", py.ToString());

                    // Post
                    val = Value.Get(new Post());
                    Assert.Equal(ValueTypeEnum.Scope, val.Type);
                    py = converter.GetObject(val);
                    Assert.Equal("Posting", py.GetPythonTypeName());

                    // Xact
                    val = Value.Get(new Xact());
                    Assert.Equal(ValueTypeEnum.Scope, val.Type);
                    py = converter.GetObject(val);
                    Assert.Equal("Transaction", py.GetPythonTypeName());

                    // Account
                    val = Value.Get(new Account());
                    Assert.Equal(ValueTypeEnum.Scope, val.Type);
                    py = converter.GetObject(val);
                    Assert.Equal("Account", py.GetPythonTypeName());

                    // PeriodXact
                    val = Value.Get(new PeriodXact());
                    Assert.Equal(ValueTypeEnum.Scope, val.Type);
                    py = converter.GetObject(val);
                    Assert.Equal("PeriodicTransaction", py.GetPythonTypeName());

                    // AutoXact
                    val = Value.Get(new AutoXact());
                    Assert.Equal(ValueTypeEnum.Scope, val.Type);
                    py = converter.GetObject(val);
                    Assert.Equal("AutomatedTransaction", py.GetPythonTypeName());
                }
            }
            finally
            {
                PythonSession.PythonModuleShutdown();
            }
        }

        [PythonFact]
        public void PythonValueConverter_GetValue_Conversions()
        {
            PythonSession.PythonModuleInitialization();
            try
            {
                PythonSession.Current.Initialize();
                using (PythonSession.Current.GIL())
                {
                    var converter = new PythonValueConverter(PythonSession.Current);
                    Value val = null;
                    PyObject py = null;

                    Assert.Equal(ValueTypeEnum.Void, converter.GetValue(null).Type);
                    Assert.Equal(ValueTypeEnum.Void, converter.GetValue(Runtime.None).Type);

                    var scope = PythonSession.Current.MainModule.ModuleObject;
                    scope.Import("datetime");

                    py = scope.Eval("True");
                    val = converter.GetValue(py);
                    Assert.Equal(ValueTypeEnum.Boolean, val.Type);
                    Assert.True(val.AsBoolean);

                    py = scope.Eval("False");
                    val = converter.GetValue(py);
                    Assert.Equal(ValueTypeEnum.Boolean, val.Type);
                    Assert.False(val.AsBoolean);

                    py = scope.Eval("10");
                    val = converter.GetValue(py);
                    Assert.Equal(ValueTypeEnum.Integer, val.Type);
                    Assert.Equal(10, val.AsLong);

                    py = scope.Eval("'some-string'");
                    val = converter.GetValue(py);
                    Assert.Equal(ValueTypeEnum.String, val.Type);
                    Assert.Equal("some-string", val.AsString);

                    py = scope.Eval("datetime.date(2021, 10, 20)");
                    val = converter.GetValue(py);
                    Assert.Equal(ValueTypeEnum.Date, val.Type);
                    Assert.Equal(new Date(2021, 10, 20), val.AsDate);

                    py = scope.Eval("datetime.datetime(2021, 10, 20, 23, 59, 33)");
                    val = converter.GetValue(py);
                    Assert.Equal(ValueTypeEnum.DateTime, val.Type);
                    Assert.Equal(new DateTime(2021, 10, 20, 23, 59, 33), val.AsDateTime);

                    py = scope.Eval("ledger.Balance(10)");
                    val = converter.GetValue(py);
                    Assert.Equal(ValueTypeEnum.Balance, val.Type);
                    Assert.Equal("10", val.AsBalance.ToString().Trim());

                    py = scope.Eval("ledger.Amount(10)");
                    val = converter.GetValue(py);
                    Assert.Equal(ValueTypeEnum.Amount, val.Type);
                    Assert.Equal("10", val.AsAmount.ToString());

                    py = scope.Eval("ledger.Mask('mask')");
                    val = converter.GetValue(py);
                    Assert.Equal(ValueTypeEnum.Mask, val.Type);
                    Assert.Equal("mask", val.AsMask.ToString());

                    py = scope.Eval("ledger.Posting()");
                    val = converter.GetValue(py);
                    Assert.Equal(ValueTypeEnum.Scope, val.Type);

                    py = scope.Eval("ledger.Transaction()");
                    val = converter.GetValue(py);
                    Assert.Equal(ValueTypeEnum.Scope, val.Type);

                    py = scope.Eval("ledger.PeriodicTransaction()");
                    val = converter.GetValue(py);
                    Assert.Equal(ValueTypeEnum.Scope, val.Type);

                    py = scope.Eval("ledger.AutomatedTransaction()");
                    val = converter.GetValue(py);
                    Assert.Equal(ValueTypeEnum.Scope, val.Type);

                    py = scope.Eval("ledger.Account()");
                    val = converter.GetValue(py);
                    Assert.Equal(ValueTypeEnum.Scope, val.Type);

                    py = scope.Eval("ledger.Value(10)");
                    val = converter.GetValue(py);
                    Assert.Equal(ValueTypeEnum.Integer, val.Type);

                    py = scope.Eval("[10, 20, 30]");
                    val = converter.GetValue(py);
                    Assert.Equal(ValueTypeEnum.Sequence, val.Type);
                    Assert.Equal("20", val.AsSequence[1].ToString());
                }
            }
            finally
            {
                PythonSession.PythonModuleShutdown();
            }
        }

    }
}
