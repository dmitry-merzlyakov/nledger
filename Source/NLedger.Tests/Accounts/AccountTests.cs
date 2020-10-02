// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Amounts;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Accounts
{
    public class AccountTests : TestFixture
    {
        [Fact]
        public void Account_Tests_DefaultContructorCreatesARootAccount()
        {
            Account account = new Account();
            Assert.Null(account.Parent);
            Assert.True(string.IsNullOrEmpty(account.Name));
        }

        [Fact]
        public void Account_Tests_RootAccountHasEmptyNameAndFullName()
        {
            Account account = new Account();
            Assert.True(string.IsNullOrEmpty(account.Name));
            Assert.True(string.IsNullOrEmpty(account.FullName));
        }

        [Fact]
        public void Account_Tests_RootAccountCanHaveName()
        {
            Account account = new Account(null, "root");
            Assert.Equal("root", account.Name);
            Assert.Equal("root", account.FullName);
        }

        [Fact]
        public void Account_Tests_FullNameConcantenatesAllParentNames()
        {
            Account root = new Account(null, "root");
            Account branch = new Account(root, "branch");
            Account leaf = new Account(branch, "leaf");
            Assert.Equal("root", root.FullName);
            Assert.Equal("root:branch", branch.FullName);
            Assert.Equal("root:branch:leaf", leaf.FullName);
        }

        [Fact]
        public void Account_Tests_FullNameIgnoresEmptyParentNames()
        {
            Account root = new Account(null, "root");
            Account branch = new Account(root, "");
            Account leaf = new Account(branch, "leaf");
            Assert.Equal("root", root.FullName);
            Assert.Equal("root:", branch.FullName);
            Assert.Equal("root:leaf", leaf.FullName);

            Account parent = new Account(null, "");
            Account child = new Account(parent, "child");
            Assert.Equal("child", child.FullName);
        }

        [Fact]
        public void Account_Tests_FindAccountAddsChildAccount()
        {
            Account root = new Account();
            Account child = root.FindAccount("child");
            Assert.NotNull(child);
            Assert.Equal("child", child.Name);
            Assert.True(root.Accounts.ContainsKey("child"));
            Assert.Equal(child, root.Accounts["child"]);
        }

        [Fact]
        public void Account_Tests_FindAccountUsesExistingChildAccount()
        {
            Account root = new Account();
            Account child = root.FindAccount("child");
            Account child2 = root.FindAccount("child");
            Assert.Equal(child, child2);
            Assert.Equal(1, root.Accounts.Count);
        }

        [Fact]
        public void Account_Tests_FindAccountCreatesTree()
        {
            Account root = new Account();
            Account child3 = root.FindAccount("child:child2:child3");
            Assert.NotNull(child3);
            Assert.Equal("child3", child3.Name);

            Account child = root.Accounts["child"];
            Assert.NotNull(child);
            Assert.Equal("child", child.Name);

            Account child2 = child.Accounts["child2"];
            Assert.NotNull(child2);
            Assert.Equal("child2", child2.Name);

            Account _child3 = child2.Accounts["child3"];
            Assert.NotNull(_child3);
            Assert.Equal(_child3, child3);
        }

        [Fact]
        public void Account_Tests_FindAccountDoesNotCreateIfNoAutoCreate()
        {
            Account root = new Account();

            Account child1 = root.FindAccount("child1", false);
            Assert.Null(child1);

            Account child2 = root.FindAccount("child2", true);
            Assert.NotNull(child2);

            Account child3 = root.FindAccount("child3");  // Default is True
            Assert.NotNull(child3);
        }

        [Fact]
        public void Account_Tests_FindAccountCopiesOptionsToCreatedAccount()
        {
            Account root = new Account() { IsGeneratedAccount = true, IsTempAccount = true };

            Account child = root.FindAccount("child");
            Assert.True(child.IsGeneratedAccount);
            Assert.True(child.IsTempAccount);
        }

        [Fact]
        public void Account_Amount_ReturnsVoidIfNoXDataOrNotVisited()
        {
            Account account = new Account();
            Value value = account.Amount();  // No XData
            Assert.True(Value.IsNullOrEmpty(value));

            account = new Account();
            account.XData.Visited = false;
            value = account.Amount();  // Not Visited
            Assert.True(Value.IsNullOrEmpty(value));
        }

        [Fact]
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

            Assert.False(Value.IsNullOrEmpty(value));
            Assert.Equal(30, value.AsLong);
            Assert.Equal(30, account.XData.SelfDetails.Total.AsLong);

            Assert.True(post1.XData.Considered);
            Assert.True(post2.XData.Considered);
            Assert.Equal(2, account.XData.SelfDetails.LastPost);
        }

        [Fact]
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

            Assert.False(Value.IsNullOrEmpty(value));
            Assert.Equal(30, value.AsLong);
            Assert.Equal(30, account.XData.SelfDetails.Total.AsLong);

            Assert.True(post1.XData.Considered);
            Assert.True(post2.XData.Considered);
            Assert.Equal(2, account.XData.SelfDetails.LastReportedPost);
        }

        [Fact]
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

            Assert.False(Value.IsNullOrEmpty(value));
            Assert.Equal(60, value.AsLong);
            Assert.Equal(60, account.XData.SelfDetails.Total.AsLong);

            Assert.True(post1.XData.Considered);
            Assert.True(post2.XData.Considered);
            Assert.True(post3.XData.Considered);
            Assert.Equal(3, account.XData.SelfDetails.LastPost);
        }

        [Fact]
        public void Account_Description_ReturnsFullNameWithPrefix()
        {
            Account account = new Account(null, "root");
            Account account1 = new Account(account, "child");
            Assert.Equal("account root", account.Description);
            Assert.Equal("account root:child", account1.Description);
        }

        [Fact]
        public void Account_FindAccountRe_LooksForAccountsByRegex()
        {
            Account account = new Account(null, "root");
            Account account1 = account.FindAccount("child");

            Account result = account.FindAccountRe("child");
            Assert.Equal(result, account1);
        }

        [Fact]
        public void Account_FindAccountRe_ReturnsNullIfCannotFind()
        {
            Account account = new Account(null, "root");
            Assert.Null(account.FindAccountRe("child"));
        }

        [Fact]
        public void Account_AddPost_AddsPostToPostsCollection()
        {
            Account account = new Account();
            Post post = new Post();

            account.AddPost(post);

            Post result = account.Posts.First();
            Assert.Equal(result, post);
        }

        [Fact]
        public void Account_AddPost_UpdatesPostXData()
        {
            Account account = new Account();
            account.XData.SelfDetails.Gathered = true;
            account.XData.SelfDetails.Calculated = true;
            account.XData.FamilyDetails.Gathered = true;
            account.XData.FamilyDetails.Calculated = true;
            Post post = new Post();

            account.AddPost(post);

            Assert.False(account.XData.SelfDetails.Gathered);
            Assert.False(account.XData.SelfDetails.Calculated);
            Assert.False(account.XData.FamilyDetails.Gathered);
            Assert.False(account.XData.FamilyDetails.Calculated);
        }

        [Fact]
        public void Account_AddDeferredPosts_PopulatesDeferredPosts()
        {
            Account account = new Account();
            Assert.Null(account.DeferredPosts);
            account.AddDeferredPosts("uuid", new Post());
            Assert.NotNull(account.DeferredPosts);
        }

        [Fact]
        public void Account_AddDeferredPosts_AddsUUID()
        {
            Account account = new Account();
            account.AddDeferredPosts("1234", new Post());
            Assert.Equal("1234", account.DeferredPosts.First().Key);
        }

        [Fact]
        public void Account_AddDeferredPosts_AddsPostToUUID()
        {
            Post post1 = new Post();
            Post post2 = new Post();
            Post post3 = new Post();
            Account account = new Account();

            account.AddDeferredPosts("uuid1", post1);
            account.AddDeferredPosts("uuid2", post2);
            account.AddDeferredPosts("uuid2", post3);

            Assert.Equal(2, account.DeferredPosts.Count);
            Assert.Equal(1, account.DeferredPosts["uuid1"].Count);
            Assert.Equal(2, account.DeferredPosts["uuid2"].Count);
            Assert.Equal(post1, account.DeferredPosts["uuid1"].First());
            Assert.Equal(post2, account.DeferredPosts["uuid2"].First());
            Assert.Equal(post3, account.DeferredPosts["uuid2"].Last());
        }

        [Fact]
        public void Account_ApplyDeferredPosts_DoesNothingIfNoDeferredPosts()
        {
            Account account = new Account();
            account.ApplyDeferredPosts();
            Assert.Null(account.DeferredPosts);
        }

        [Fact]
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

            Assert.Equal(3, account1.Posts.Count);
            Assert.Equal(post1, account1.Posts[0]);
            Assert.Equal(post2, account1.Posts[1]);
            Assert.Equal(post3, account1.Posts[2]);
            Assert.Null(account.DeferredPosts);
        }

        [Fact]
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

            Assert.Equal(3, accountToAccumulate.Posts.Count);
            Assert.Equal(post1, accountToAccumulate.Posts[0]);
            Assert.Equal(post2, accountToAccumulate.Posts[1]);
            Assert.Equal(post3, accountToAccumulate.Posts[2]);
            Assert.Null(accountChild.DeferredPosts);
        }

        [Fact]
        public void Account_ClearXData_ClearsXDataForThisAndChildAccounts()
        {
            Account accountRoot = new Account(null, "root");
            Account accountChild = accountRoot.FindAccount("child");

            accountRoot.XData.HasNonVirtuals = true;
            accountChild.XData.HasNonVirtuals = true;

            accountRoot.ClearXData();

            Assert.False(accountRoot.HasXData);
            Assert.False(accountChild.HasXData);
        }

        [Fact]
        public void Account_GetDeferredPosts_ReturnsNullIsNoPosts()
        {
            Account account = new Account(null, "root");
            Assert.Null(account.GetDeferredPosts("uuid1"));
        }

        [Fact]
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

            Assert.Single(uuid1Posts);
            Assert.Equal(2, uuid2Posts.Count());
            Assert.Equal(post1, uuid1Posts.ElementAt(0));
            Assert.Equal(post2, uuid2Posts.ElementAt(0));
            Assert.Equal(post3, uuid2Posts.ElementAt(1));
        }

        [Fact]
        public void Account_GetDeferredPosts_DoesNothingIsNoPosts()
        {
            Account account = new Account(null, "root");
            account.DeleteDeferredPosts("uuid1");
        }

        [Fact]
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

            Assert.Equal(1, account.DeferredPosts.Count);
            Assert.Equal(post1, account.DeferredPosts["uuid1"].First());
        }

        [Fact]
        public void Account_HasXFlags_ReturnsFalseIfNoXData()
        {
            Account account = new Account();
            Assert.False(account.HasXData);
            Assert.False(account.HasXFlags(d => d.Visited));
        }

        [Fact]
        public void Account_HasXFlags_ProcessesXDataIfItExists()
        {
            Account account = new Account();
            account.XData.Visited = true;
            Assert.True(account.HasXFlags(d => d.Visited));
            account.XData.Visited = false;
            Assert.False(account.HasXFlags(d => d.Visited));
        }

        [Fact]
        public void Account_Valid_DetectsToBigDepth()
        {
            Account account = new Account();
            for(var i=0;i<256;i++)
                account = new Account(account, "acc");

            Assert.Equal(256, account.Depth);
            Assert.True(account.Valid());

            account = new Account(account, "acc");

            Assert.Equal(257, account.Depth);
            Assert.False(account.Valid());
        }

        [Fact]
        public void Account_Valid_DetectsSelfLoops()
        {
            Account account = new Account();
            Assert.True(account.Valid());

            account.Accounts.Add("wrong-self-loop", account);
            Assert.False(account.Valid());
        }

        [Fact]
        public void Account_Valid_DetectsInvalidChidren()
        {
            Account account = new Account();
            Assert.True(account.Valid());

            Account childAcc = account;
            for (var i = 0; i < 256; i++)
            {
                childAcc = new Account(childAcc, "acc");
                childAcc.Parent.Accounts.Add("child", childAcc);
            }
            Assert.True(account.Valid());

            childAcc = new Account(childAcc, "acc");
            childAcc.Parent.Accounts.Add("child", childAcc);

            Assert.False(account.Valid());
            Assert.False(childAcc.Valid());
        }

    }
}
