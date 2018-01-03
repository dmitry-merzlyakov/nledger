// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLedger.Accounts;
using NLedger.Amounts;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Tests.Accounts
{
    [TestClass]
    public class AccountTests : TestFixture
    {
        [TestMethod]
        public void Account_Tests_DefaultContructorCreatesARootAccount()
        {
            Account account = new Account();
            Assert.IsNull(account.Parent);
            Assert.IsTrue(string.IsNullOrEmpty(account.Name));
        }

        [TestMethod]
        public void Account_Tests_RootAccountHasEmptyNameAndFullName()
        {
            Account account = new Account();
            Assert.IsTrue(string.IsNullOrEmpty(account.Name));
            Assert.IsTrue(string.IsNullOrEmpty(account.FullName));
        }

        [TestMethod]
        public void Account_Tests_RootAccountCanHaveName()
        {
            Account account = new Account(null, "root");
            Assert.AreEqual("root", account.Name);
            Assert.AreEqual("root", account.FullName);
        }

        [TestMethod]
        public void Account_Tests_FullNameConcantenatesAllParentNames()
        {
            Account root = new Account(null, "root");
            Account branch = new Account(root, "branch");
            Account leaf = new Account(branch, "leaf");
            Assert.AreEqual("root", root.FullName);
            Assert.AreEqual("root:branch", branch.FullName);
            Assert.AreEqual("root:branch:leaf", leaf.FullName);
        }

        [TestMethod]
        public void Account_Tests_FullNameIgnoresEmptyParentNames()
        {
            Account root = new Account(null, "root");
            Account branch = new Account(root, "");
            Account leaf = new Account(branch, "leaf");
            Assert.AreEqual("root", root.FullName);
            Assert.AreEqual("root:", branch.FullName);
            Assert.AreEqual("root:leaf", leaf.FullName);

            Account parent = new Account(null, "");
            Account child = new Account(parent, "child");
            Assert.AreEqual("child", child.FullName);
        }

        [TestMethod]
        public void Account_Tests_FindAccountAddsChildAccount()
        {
            Account root = new Account();
            Account child = root.FindAccount("child");
            Assert.IsNotNull(child);
            Assert.AreEqual("child", child.Name);
            Assert.IsTrue(root.Accounts.ContainsKey("child"));
            Assert.AreEqual(child, root.Accounts["child"]);
        }

        [TestMethod]
        public void Account_Tests_FindAccountUsesExistingChildAccount()
        {
            Account root = new Account();
            Account child = root.FindAccount("child");
            Account child2 = root.FindAccount("child");
            Assert.AreEqual(child, child2);
            Assert.AreEqual(1, root.Accounts.Count);
        }

        [TestMethod]
        public void Account_Tests_FindAccountCreatesTree()
        {
            Account root = new Account();
            Account child3 = root.FindAccount("child:child2:child3");
            Assert.IsNotNull(child3);
            Assert.AreEqual("child3", child3.Name);

            Account child = root.Accounts["child"];
            Assert.IsNotNull(child);
            Assert.AreEqual("child", child.Name);

            Account child2 = child.Accounts["child2"];
            Assert.IsNotNull(child2);
            Assert.AreEqual("child2", child2.Name);

            Account _child3 = child2.Accounts["child3"];
            Assert.IsNotNull(_child3);
            Assert.AreEqual(_child3, child3);
        }

        [TestMethod]
        public void Account_Tests_FindAccountDoesNotCreateIfNoAutoCreate()
        {
            Account root = new Account();

            Account child1 = root.FindAccount("child1", false);
            Assert.IsNull(child1);

            Account child2 = root.FindAccount("child2", true);
            Assert.IsNotNull(child2);

            Account child3 = root.FindAccount("child3");  // Default is True
            Assert.IsNotNull(child3);
        }

        [TestMethod]
        public void Account_Tests_FindAccountCopiesOptionsToCreatedAccount()
        {
            Account root = new Account() { IsGeneratedAccount = true, IsTempAccount = true };

            Account child = root.FindAccount("child");
            Assert.IsTrue(child.IsGeneratedAccount);
            Assert.IsTrue(child.IsTempAccount);
        }

        [TestMethod]
        public void Account_Amount_ReturnsVoidIfNoXDataOrNotVisited()
        {
            Account account = new Account();
            Value value = account.Amount();  // No XData
            Assert.IsTrue(Value.IsNullOrEmpty(value));

            account = new Account();
            account.XData.Visited = false;
            value = account.Amount();  // Not Visited
            Assert.IsTrue(Value.IsNullOrEmpty(value));
        }

        [TestMethod]
        public void Account_Amount_SummarizesVisitedPosts()
        {
            Account account = new Account();
            account.XData.Visited = true;

            Post post1 = new Post();
            post1.XData.Visited = true;
            post1.Amount = new Amount(10);

            Post post2 = new Post();
            post2.XData.Visited = true;
            post2.Amount = new Amount(20);

            account.Posts.Add(post1);
            account.Posts.Add(post2);
                       
            Value value = account.Amount();

            Assert.IsFalse(Value.IsNullOrEmpty(value));
            Assert.AreEqual(30, value.AsLong);
            Assert.AreEqual(30, account.XData.SelfDetails.Total.AsLong);

            Assert.IsTrue(post1.XData.Considered);
            Assert.IsTrue(post2.XData.Considered);
            Assert.AreEqual(2, account.XData.SelfDetails.LastPost);
        }

        [TestMethod]
        public void Account_Amount_SummarizesVisitedReportedPosts()
        {
            Account account = new Account();
            account.XData.Visited = true;

            Post post1 = new Post();
            post1.XData.Visited = true;
            post1.Amount = new Amount(10);

            Post post2 = new Post();
            post2.XData.Visited = true;
            post2.Amount = new Amount(20);

            account.XData.ReportedPosts.Add(post1);
            account.XData.ReportedPosts.Add(post2);

            Value value = account.Amount();

            Assert.IsFalse(Value.IsNullOrEmpty(value));
            Assert.AreEqual(30, value.AsLong);
            Assert.AreEqual(30, account.XData.SelfDetails.Total.AsLong);

            Assert.IsTrue(post1.XData.Considered);
            Assert.IsTrue(post2.XData.Considered);
            Assert.AreEqual(2, account.XData.SelfDetails.LastReportedPost);
        }

        [TestMethod]
        public void Account_Amount_ContinuesSummmarizingFromLastPosition()
        {
            Account account = new Account();
            account.XData.Visited = true;

            Post post1 = new Post();
            post1.XData.Visited = true;
            post1.Amount = new Amount(10);

            Post post2 = new Post();
            post2.XData.Visited = true;
            post2.Amount = new Amount(20);

            account.Posts.Add(post1);
            account.Posts.Add(post2);

            Value value = account.Amount();  // First call

            Post post3 = new Post();
            post3.XData.Visited = true;
            post3.Amount = new Amount(30);

            account.Posts.Add(post3);

            value = account.Amount(); // Next call

            Assert.IsFalse(Value.IsNullOrEmpty(value));
            Assert.AreEqual(60, value.AsLong);
            Assert.AreEqual(60, account.XData.SelfDetails.Total.AsLong);

            Assert.IsTrue(post1.XData.Considered);
            Assert.IsTrue(post2.XData.Considered);
            Assert.IsTrue(post3.XData.Considered);
            Assert.AreEqual(3, account.XData.SelfDetails.LastPost);
        }

        [TestMethod]
        public void Account_Description_ReturnsFullNameWithPrefix()
        {
            Account account = new Account(null, "root");
            Account account1 = new Account(account, "child");
            Assert.AreEqual("account root", account.Description);
            Assert.AreEqual("account root:child", account1.Description);
        }

        [TestMethod]
        public void Account_FindAccountRe_LooksForAccountsByRegex()
        {
            Account account = new Account(null, "root");
            Account account1 = account.FindAccount("child");

            Account result = account.FindAccountRe("child");
            Assert.AreEqual(result, account1);
        }

        [TestMethod]
        public void Account_FindAccountRe_ReturnsNullIfCannotFind()
        {
            Account account = new Account(null, "root");
            Assert.IsNull(account.FindAccountRe("child"));
        }

        [TestMethod]
        public void Account_AddPost_AddsPostToPostsCollection()
        {
            Account account = new Account();
            Post post = new Post();

            account.AddPost(post);

            Post result = account.Posts.First();
            Assert.AreEqual(result, post);
        }

        [TestMethod]
        public void Account_AddPost_UpdatesPostXData()
        {
            Account account = new Account();
            account.XData.SelfDetails.Gathered = true;
            account.XData.SelfDetails.Calculated = true;
            account.XData.FamilyDetails.Gathered = true;
            account.XData.FamilyDetails.Calculated = true;
            Post post = new Post();

            account.AddPost(post);

            Assert.IsFalse(account.XData.SelfDetails.Gathered);
            Assert.IsFalse(account.XData.SelfDetails.Calculated);
            Assert.IsFalse(account.XData.FamilyDetails.Gathered);
            Assert.IsFalse(account.XData.FamilyDetails.Calculated);
        }

        [TestMethod]
        public void Account_AddDeferredPosts_PopulatesDeferredPosts()
        {
            Account account = new Account();
            Assert.IsNull(account.DeferredPosts);
            account.AddDeferredPosts("uuid", new Post());
            Assert.IsNotNull(account.DeferredPosts);
        }

        [TestMethod]
        public void Account_AddDeferredPosts_AddsUUID()
        {
            Account account = new Account();
            account.AddDeferredPosts("1234", new Post());
            Assert.AreEqual("1234", account.DeferredPosts.First().Key);
        }

        [TestMethod]
        public void Account_AddDeferredPosts_AddsPostToUUID()
        {
            Post post1 = new Post();
            Post post2 = new Post();
            Post post3 = new Post();
            Account account = new Account();

            account.AddDeferredPosts("uuid1", post1);
            account.AddDeferredPosts("uuid2", post2);
            account.AddDeferredPosts("uuid2", post3);

            Assert.AreEqual(2, account.DeferredPosts.Count);
            Assert.AreEqual(1, account.DeferredPosts["uuid1"].Count);
            Assert.AreEqual(2, account.DeferredPosts["uuid2"].Count);
            Assert.AreEqual(post1, account.DeferredPosts["uuid1"].First());
            Assert.AreEqual(post2, account.DeferredPosts["uuid2"].First());
            Assert.AreEqual(post3, account.DeferredPosts["uuid2"].Last());
        }

        [TestMethod]
        public void Account_ApplyDeferredPosts_DoesNothingIfNoDeferredPosts()
        {
            Account account = new Account();
            account.ApplyDeferredPosts();
            Assert.IsNull(account.DeferredPosts);
        }

        [TestMethod]
        public void Account_ApplyDeferredPosts_AddsAllDeferredPostsToItsAccountsAndClearsList()
        {
            Account account1 = new Account();
            Post post1 = new Post(account1, new Amount(1));
            Post post2 = new Post(account1, new Amount(1));
            Post post3 = new Post(account1, new Amount(1));
            Account account = new Account();

            account.AddDeferredPosts("uuid1", post1);
            account.AddDeferredPosts("uuid2", post2);
            account.AddDeferredPosts("uuid2", post3);

            account.ApplyDeferredPosts();

            Assert.AreEqual(3, account1.Posts.Count);
            Assert.AreEqual(post1, account1.Posts[0]);
            Assert.AreEqual(post2, account1.Posts[1]);
            Assert.AreEqual(post3, account1.Posts[2]);
            Assert.IsNull(account.DeferredPosts);
        }

        [TestMethod]
        public void Account_ApplyDeferredPosts_HandlesChildAccounts()
        {
            Account accountToAccumulate = new Account();
            Account accountRoot = new Account(null, "root");
            Account accountChild = accountRoot.FindAccount("child");

            Post post1 = new Post(accountToAccumulate, new Amount(1));
            Post post2 = new Post(accountToAccumulate, new Amount(1));
            Post post3 = new Post(accountToAccumulate, new Amount(1));

            accountChild.AddDeferredPosts("uuid1", post1);
            accountChild.AddDeferredPosts("uuid2", post2);
            accountChild.AddDeferredPosts("uuid2", post3);

            accountRoot.ApplyDeferredPosts();

            Assert.AreEqual(3, accountToAccumulate.Posts.Count);
            Assert.AreEqual(post1, accountToAccumulate.Posts[0]);
            Assert.AreEqual(post2, accountToAccumulate.Posts[1]);
            Assert.AreEqual(post3, accountToAccumulate.Posts[2]);
            Assert.IsNull(accountChild.DeferredPosts);
        }

        [TestMethod]
        public void Account_ClearXData_ClearsXDataForThisAndChildAccounts()
        {
            Account accountRoot = new Account(null, "root");
            Account accountChild = accountRoot.FindAccount("child");

            accountRoot.XData.HasNonVirtuals = true;
            accountChild.XData.HasNonVirtuals = true;

            accountRoot.ClearXData();

            Assert.IsFalse(accountRoot.HasXData);
            Assert.IsFalse(accountChild.HasXData);
        }

        [TestMethod]
        public void Account_GetDeferredPosts_ReturnsNullIsNoPosts()
        {
            Account account = new Account(null, "root");
            Assert.IsNull(account.GetDeferredPosts("uuid1"));
        }

        [TestMethod]
        public void Account_GetDeferredPosts_ReturnsDeferredPostsForUUID()
        {
            Account account1 = new Account();
            Post post1 = new Post(account1, new Amount(1));
            Post post2 = new Post(account1, new Amount(1));
            Post post3 = new Post(account1, new Amount(1));
            Account account = new Account();

            account.AddDeferredPosts("uuid1", post1);
            account.AddDeferredPosts("uuid2", post2);
            account.AddDeferredPosts("uuid2", post3);

            var uuid1Posts = account.GetDeferredPosts("uuid1");
            var uuid2Posts = account.GetDeferredPosts("uuid2");

            Assert.AreEqual(1, uuid1Posts.Count());
            Assert.AreEqual(2, uuid2Posts.Count());
            Assert.AreEqual(post1, uuid1Posts.ElementAt(0));
            Assert.AreEqual(post2, uuid2Posts.ElementAt(0));
            Assert.AreEqual(post3, uuid2Posts.ElementAt(1));
        }

        [TestMethod]
        public void Account_GetDeferredPosts_DoesNothingIsNoPosts()
        {
            Account account = new Account(null, "root");
            account.DeleteDeferredPosts("uuid1");
        }

        [TestMethod]
        public void Account_GetDeferredPosts_RemovesDeferredPostsForUUID()
        {
            Account account1 = new Account();
            Post post1 = new Post(account1, new Amount(1));
            Post post2 = new Post(account1, new Amount(1));
            Post post3 = new Post(account1, new Amount(1));
            Account account = new Account();

            account.AddDeferredPosts("uuid1", post1);
            account.AddDeferredPosts("uuid2", post2);
            account.AddDeferredPosts("uuid2", post3);

            account.DeleteDeferredPosts("uuid2");

            Assert.AreEqual(1, account.DeferredPosts.Count);
            Assert.AreEqual(post1, account.DeferredPosts["uuid1"].First());
        }

        [TestMethod]
        public void Account_HasXFlags_ReturnsFalseIfNoXData()
        {
            Account account = new Account();
            Assert.IsFalse(account.HasXData);
            Assert.IsFalse(account.HasXFlags(d => d.Visited));
        }

        [TestMethod]
        public void Account_HasXFlags_ProcessesXDataIfItExists()
        {
            Account account = new Account();
            account.XData.Visited = true;
            Assert.IsTrue(account.HasXFlags(d => d.Visited));
            account.XData.Visited = false;
            Assert.IsFalse(account.HasXFlags(d => d.Visited));
        }
    }
}
