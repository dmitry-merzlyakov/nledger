// **********************************************************************************
// Copyright (c) 2015-2023, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2023, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Utility
{
    public class ObjectExtensionsTests
    {
        [Fact]
        public void  ObjectExtensions_SafeGetType_ReturnsNullIfObjectIsNull()
        {
            Assert.Equal(typeof(int), ObjectExtensions.SafeGetType(2));
            Assert.Null(ObjectExtensions.SafeGetType(null));
        }

        [Fact]
        public void ObjectExtensions_Compare_ReturnsTrueForEqualObjects()
        {
            IDictionary<string, string> dict1 = new Dictionary<string, string>();
            IDictionary<string, string> dict2 = new Dictionary<string, string>();
            Assert.True(ObjectExtensions.Compare(dict1, dict2));

            dict1 = null;
            dict2 = null;
            Assert.True(ObjectExtensions.Compare(dict1, dict2));
        }

        [Fact]
        public void ObjectExtensions_Compare_ReturnsFalseIfOneOfObjectsIsNull()
        {
            IDictionary<string, string> dict1 = new Dictionary<string, string>();
            IDictionary<string, string> dict2 = new Dictionary<string, string>();
            Assert.False(ObjectExtensions.Compare(null, dict2));
            Assert.False(ObjectExtensions.Compare(dict1, null));
        }

        [Fact]
        public void ObjectExtensions_Compare_ReturnsFalseIfNumbersOfItemsAreDifferent()
        {
            IDictionary<string, string> dict1 = new Dictionary<string, string>();
            dict1["k1"] = "v1";
            dict1["k2"] = "v2";

            IDictionary<string, string> dict2 = new Dictionary<string, string>();
            dict2["k1"] = "v1";

            Assert.False(ObjectExtensions.Compare(dict1, dict2));
        }

        [Fact]
        public void ObjectExtensions_Compare_ReturnsFalseIfKeysAreDifferent()
        {
            IDictionary<string, string> dict1 = new Dictionary<string, string>();
            dict1["k1"] = "v1";
            dict1["k2"] = "v2";

            IDictionary<string, string> dict2 = new Dictionary<string, string>();
            dict2["k-1"] = "v1";
            dict2["k-2"] = "v2";

            Assert.False(ObjectExtensions.Compare(dict1, dict2));
        }

        [Fact]
        public void ObjectExtensions_Compare_ReturnsFalseIfValuesAreDifferent()
        {
            IDictionary<string, string> dict1 = new Dictionary<string, string>();
            dict1["k1"] = "v1";
            dict1["k2"] = "v2";

            IDictionary<string, string> dict2 = new Dictionary<string, string>();
            dict2["k1"] = "v-1";
            dict2["k2"] = "v-2";

            Assert.False(ObjectExtensions.Compare(dict1, dict2));
        }

        [Fact]
        public void ObjectExtensions_Compare_ReturnsTrueIfKeyAndValuesAreTheSame()
        {
            IDictionary<string, string> dict1 = new Dictionary<string, string>();
            dict1["k1"] = "v1";
            dict1["k2"] = "v2";

            IDictionary<string, string> dict2 = new Dictionary<string, string>();
            dict2["k1"] = "v1";
            dict2["k2"] = "v2";

            Assert.True(ObjectExtensions.Compare(dict1, dict2));
        }

        [Fact]
        public void ObjectExtensions_RecursiveEnum_IteratesTreeLikeStucture()
        {
            var val1 = new TreeNode<int>(1);
            var val11 = val1.AddChild(11);
            var val111 = val1.AddChild(111);
            var val12 = val1.AddChild(12);
            var val121 = val1.AddChild(121);

            var result = val1.RecursiveEnum(c => c.Children).ToList();

            Assert.NotNull(result);
            Assert.Equal(result[0], val1);
            Assert.Equal(result[1], val11);
            Assert.Equal(result[2], val111);
            Assert.Equal(result[3], val12);
            Assert.Equal(result[4], val121);
        }

        public class TreeNode<T>
        {
            public TreeNode(T data)
            {
                Data = data;
            }

            public T Data { get; private set; }

            public IEnumerable<TreeNode<T>> Children
            {
                get { return ChildrenList; }
            }

            public TreeNode<T> AddChild(T data)
            {
                TreeNode<T> child = new TreeNode<T>(data);
                ChildrenList.Add(child);
                return child;
            }

            private readonly List<TreeNode<T>> ChildrenList = new List<TreeNode<T>>();
        }

    }
}
