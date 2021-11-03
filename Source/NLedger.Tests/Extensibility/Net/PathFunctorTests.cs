// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Extensibility.Net;
using NLedger.Scopus;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Extensibility.Net
{
    public class PathFunctorTests
    {
        [Fact]
        public void PathFunctor_ParsePath_CreatesFunctorByPath()
        {
            var namespaceResolver = new NamespaceResolver();
            namespaceResolver.AddAssembly(typeof(Encoding).Assembly);

            var valueConverter = new ValueConverter();
            var pathFunctor = PathFunctor.ParsePath("System.Text.Encoding.Default.BodyName", namespaceResolver, valueConverter);

            Assert.Equal(typeof(System.Text.Encoding), pathFunctor.ClassType);
            Assert.Equal(new string[] { "Default", "BodyName" }, pathFunctor.Path);
            Assert.Equal(valueConverter, pathFunctor.ValueConverter);
        }

        [Fact]
        public void PathFunctor_Constructor_PopulatesProperties()
        {
            var classType = typeof(Encoding);
            var path = new string[] { "Default", "BodyName" };
            var valueConverter = new ValueConverter();

            var pathFunctor = new PathFunctor(classType, path, valueConverter);

            Assert.Equal(classType, pathFunctor.ClassType);
            Assert.Equal(path, pathFunctor.Path);
            Assert.Equal(valueConverter, pathFunctor.ValueConverter);
        }

        [Fact]
        public void PathFunctor_ExprFunc_CallsMethod()
        {
            var classType = typeof(Encoding);
            var path = new string[] { "GetEncoding" };
            var valueConverter = new ValueConverter();

            var pathFunctor = new PathFunctor(classType, path, valueConverter);
            var scope = new CallScope(new EmptyScope());
            scope.PushBack(Value.Get(ASCIIEncoding.ASCII.EncodingName));   // public static Encoding GetEncoding(string name);
            var result = pathFunctor.ExprFunc(scope);

            Assert.Equal(ASCIIEncoding.ASCII.ToString(), result.AsString);
        }

        [Fact]
        public void PathFunctor_ExprFunc_GetsProperty()
        {
            var classType = typeof(Encoding);
            var path = new string[] { "Default" };
            var valueConverter = new ValueConverter();

            var pathFunctor = new PathFunctor(classType, path, valueConverter);
            var scope = new CallScope(new EmptyScope());
            var result = pathFunctor.ExprFunc(scope);

            Assert.Equal(Encoding.Default.ToString(), result.AsString);
        }

        [Fact]
        public void PathFunctor_ExprFunc_GetsPropertyFromReferencedMember()
        {
            var classType = typeof(Encoding);
            var path = new string[] { "Default", "BodyName" };
            var valueConverter = new ValueConverter();

            var pathFunctor = new PathFunctor(classType, path, valueConverter);
            var scope = new CallScope(new EmptyScope());
            var result = pathFunctor.ExprFunc(scope);

            Assert.Equal(Encoding.Default.BodyName, result.AsString);
        }

    }
}
