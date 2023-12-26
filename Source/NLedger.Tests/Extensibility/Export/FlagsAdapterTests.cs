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
using NLedger.Annotate;
using NLedger.Extensibility.Export;
using NLedger.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NLedger.Tests.Extensibility.Export
{
    public class FlagsAdapterTests
    {
        [Fact]
        public void FlagsAdapter_AnnotationFlagsAdapter_Tests()
        {
            var adapter = FlagsAdapter.AnnotationFlagsAdapter();
            var annotation = new Annotation();

            adapter.SetFlags(annotation, ExportedConsts.ANNOTATION_PRICE_CALCULATED);
            Assert.True(annotation.IsPriceCalculated);
            adapter.DropFlags(annotation, ExportedConsts.ANNOTATION_PRICE_CALCULATED);
            Assert.False(annotation.IsPriceCalculated);

            adapter.SetFlags(annotation, ExportedConsts.ANNOTATION_PRICE_FIXATED);
            Assert.True(annotation.IsPriceFixated);
            adapter.DropFlags(annotation, ExportedConsts.ANNOTATION_PRICE_FIXATED);
            Assert.False(annotation.IsPriceFixated);

            adapter.SetFlags(annotation, ExportedConsts.ANNOTATION_PRICE_NOT_PER_UNIT);
            Assert.True(annotation.IsPriceNotPerUnit);
            adapter.DropFlags(annotation, ExportedConsts.ANNOTATION_PRICE_NOT_PER_UNIT);
            Assert.False(annotation.IsPriceNotPerUnit);

            adapter.SetFlags(annotation, ExportedConsts.ANNOTATION_DATE_CALCULATED);
            Assert.True(annotation.IsDateCalculated);
            adapter.DropFlags(annotation, ExportedConsts.ANNOTATION_DATE_CALCULATED);
            Assert.False(annotation.IsDateCalculated);

            adapter.SetFlags(annotation, ExportedConsts.ANNOTATION_TAG_CALCULATED);
            Assert.True(annotation.IsTagCalculated);
            adapter.DropFlags(annotation, ExportedConsts.ANNOTATION_TAG_CALCULATED);
            Assert.False(annotation.IsTagCalculated);

            adapter.SetFlags(annotation, ExportedConsts.ANNOTATION_VALUE_EXPR_CALCULATED);
            Assert.True(annotation.IsValueExprCalculated);
            adapter.DropFlags(annotation, ExportedConsts.ANNOTATION_VALUE_EXPR_CALCULATED);
            Assert.False(annotation.IsValueExprCalculated);
        }

        [Fact]
        public void FlagsAdapter_AccountFlagsAdapter_Tests()
        {
            var adapter = FlagsAdapter.AccountFlagsAdapter();
            var account = new Account();

            adapter.SetFlags(account, ExportedConsts.ACCOUNT_KNOWN);
            Assert.True(account.IsKnownAccount);
            adapter.DropFlags(account, ExportedConsts.ACCOUNT_KNOWN);
            Assert.False(account.IsKnownAccount);

            adapter.SetFlags(account, ExportedConsts.ACCOUNT_TEMP);
            Assert.True(account.IsTempAccount);
            adapter.DropFlags(account, ExportedConsts.ACCOUNT_TEMP);
            Assert.False(account.IsTempAccount);

            adapter.SetFlags(account, ExportedConsts.ACCOUNT_GENERATED);
            Assert.True(account.IsGeneratedAccount);
            adapter.DropFlags(account, ExportedConsts.ACCOUNT_GENERATED);
            Assert.False(account.IsGeneratedAccount);
        }

        [Fact]
        public void FlagsAdapter_PostXDataFlagsAdapter_Tests()
        {
            var adapter = FlagsAdapter.PostXDataFlagsAdapter();
            var postXData = new PostXData();

            adapter.SetFlags(postXData, ExportedConsts.POST_EXT_RECEIVED);
            Assert.True(postXData.Received);
            adapter.DropFlags(postXData, ExportedConsts.POST_EXT_RECEIVED);
            Assert.False(postXData.Received);

            adapter.SetFlags(postXData, ExportedConsts.POST_EXT_HANDLED);
            Assert.True(postXData.Handled);
            adapter.DropFlags(postXData, ExportedConsts.POST_EXT_HANDLED);
            Assert.False(postXData.Handled);

            adapter.SetFlags(postXData, ExportedConsts.POST_EXT_DISPLAYED);
            Assert.True(postXData.Displayed);
            adapter.DropFlags(postXData, ExportedConsts.POST_EXT_DISPLAYED);
            Assert.False(postXData.Displayed);

            adapter.SetFlags(postXData, ExportedConsts.POST_EXT_DIRECT_AMT);
            Assert.True(postXData.DirectAmt);
            adapter.DropFlags(postXData, ExportedConsts.POST_EXT_DIRECT_AMT);
            Assert.False(postXData.DirectAmt);

            adapter.SetFlags(postXData, ExportedConsts.POST_EXT_SORT_CALC);
            Assert.True(postXData.SortCalc);
            adapter.DropFlags(postXData, ExportedConsts.POST_EXT_SORT_CALC);
            Assert.False(postXData.SortCalc);

            adapter.SetFlags(postXData, ExportedConsts.POST_EXT_COMPOUND);
            Assert.True(postXData.Compound);
            adapter.DropFlags(postXData, ExportedConsts.POST_EXT_COMPOUND);
            Assert.False(postXData.Compound);

            adapter.SetFlags(postXData, ExportedConsts.POST_EXT_VISITED);
            Assert.True(postXData.Visited);
            adapter.DropFlags(postXData, ExportedConsts.POST_EXT_VISITED);
            Assert.False(postXData.Visited);

            adapter.SetFlags(postXData, ExportedConsts.POST_EXT_MATCHES);
            Assert.True(postXData.Matches);
            adapter.DropFlags(postXData, ExportedConsts.POST_EXT_MATCHES);
            Assert.False(postXData.Matches);

            adapter.SetFlags(postXData, ExportedConsts.POST_EXT_CONSIDERED);
            Assert.True(postXData.Considered);
            adapter.DropFlags(postXData, ExportedConsts.POST_EXT_CONSIDERED);
            Assert.False(postXData.Considered);
        }

        [Fact]
        public void FlagsAdapter_AccountXDataFlagsAdapter_Tests()
        {
            var adapter = FlagsAdapter.AccountXDataFlagsAdapter();
            var accountXData = new AccountXData();

            adapter.SetFlags(accountXData, ExportedConsts.ACCOUNT_EXT_SORT_CALC);
            Assert.True(accountXData.SortCalc);
            adapter.DropFlags(accountXData, ExportedConsts.ACCOUNT_EXT_SORT_CALC);
            Assert.False(accountXData.SortCalc);

            adapter.SetFlags(accountXData, ExportedConsts.ACCOUNT_EXT_HAS_NON_VIRTUALS);
            Assert.True(accountXData.HasNonVirtuals);
            adapter.DropFlags(accountXData, ExportedConsts.ACCOUNT_EXT_HAS_NON_VIRTUALS);
            Assert.False(accountXData.HasNonVirtuals);

            adapter.SetFlags(accountXData, ExportedConsts.ACCOUNT_EXT_HAS_UNB_VIRTUALS);
            Assert.True(accountXData.HasUnbVirtuals);
            adapter.DropFlags(accountXData, ExportedConsts.ACCOUNT_EXT_HAS_UNB_VIRTUALS);
            Assert.False(accountXData.HasUnbVirtuals);

            adapter.SetFlags(accountXData, ExportedConsts.ACCOUNT_EXT_AUTO_VIRTUALIZE);
            Assert.True(accountXData.AutoVirtualize);
            adapter.DropFlags(accountXData, ExportedConsts.ACCOUNT_EXT_AUTO_VIRTUALIZE);
            Assert.False(accountXData.AutoVirtualize);

            adapter.SetFlags(accountXData, ExportedConsts.ACCOUNT_EXT_VISITED);
            Assert.True(accountXData.Visited);
            adapter.DropFlags(accountXData, ExportedConsts.ACCOUNT_EXT_VISITED);
            Assert.False(accountXData.Visited);

            adapter.SetFlags(accountXData, ExportedConsts.ACCOUNT_EXT_MATCHING);
            Assert.True(accountXData.Matching);
            adapter.DropFlags(accountXData, ExportedConsts.ACCOUNT_EXT_MATCHING);
            Assert.False(accountXData.Matching);

            adapter.SetFlags(accountXData, ExportedConsts.ACCOUNT_EXT_TO_DISPLAY);
            Assert.True(accountXData.ToDisplay);
            adapter.DropFlags(accountXData, ExportedConsts.ACCOUNT_EXT_TO_DISPLAY);
            Assert.False(accountXData.ToDisplay);

            adapter.SetFlags(accountXData, ExportedConsts.ACCOUNT_EXT_DISPLAYED);
            Assert.True(accountXData.Displayed);
            adapter.DropFlags(accountXData, ExportedConsts.ACCOUNT_EXT_DISPLAYED);
            Assert.False(accountXData.Displayed);
        }

        [Fact]
        public void FlagsAdapter_AccountHasXFlags_ChecksXFlags()
        {
            var account = new Account();
            account.XData.Displayed = true;
            account.XData.Visited = false;

            Assert.True(FlagsAdapter.AccountHasXFlags(account, ExportedConsts.ACCOUNT_EXT_DISPLAYED));
            Assert.False(FlagsAdapter.AccountHasXFlags(account, ExportedConsts.ACCOUNT_EXT_VISITED));
        }

        [Fact]
        public void FlagsAdapter_EnumToInt_ReturnsIntegerForAnyEnums()
        {
            Assert.Equal(0, FlagsAdapter.EnumToInt(AmountPrintEnum.AMOUNT_PRINT_NO_FLAGS));
            Assert.Equal(1, FlagsAdapter.EnumToInt(AmountPrintEnum.AMOUNT_PRINT_RIGHT_JUSTIFY));
            Assert.Equal(2, FlagsAdapter.EnumToInt(AmountPrintEnum.AMOUNT_PRINT_COLORIZE));
            Assert.Equal(4, FlagsAdapter.EnumToInt(AmountPrintEnum.AMOUNT_PRINT_NO_COMPUTED_ANNOTATIONS));

            Assert.Equal(0, FlagsAdapter.EnumToInt(ItemStateEnum.Uncleared));
            Assert.Equal(1, FlagsAdapter.EnumToInt(ItemStateEnum.Cleared));
            Assert.Equal(2, FlagsAdapter.EnumToInt(ItemStateEnum.Pending));
        }

        [Fact]
        public void FlagsAdapter_EnumToInt_ReturnsSameIntegerForInteger()
        {
            Assert.Equal(0, FlagsAdapter.EnumToInt(0));
            Assert.Equal(1, FlagsAdapter.EnumToInt(1));
            Assert.Equal(2, FlagsAdapter.EnumToInt(2));
        }
    }
}
