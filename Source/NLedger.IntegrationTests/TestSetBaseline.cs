// **********************************************************************************
// Copyright (c) 2015-2020, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2020, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace NLedger.IntegrationTests
{
    [TestClass]
    [DeploymentItem(@"test\baseline", @"test\baseline")]
    [DeploymentItem(@"test\input", @"test\input")]
    [DeploymentItem(@"cat.cmd")]
    public class TestSet1_test_baseline
    {
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_cmd_accounts()
        {
            new TestRunner(@"test\baseline\cmd-accounts.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_cmd_balance()
        {
            new TestRunner(@"test\baseline\cmd-balance.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_cmd_budget()
        {
            new TestRunner(@"test\baseline\cmd-budget.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_cmd_cleared()
        {
            new TestRunner(@"test\baseline\cmd-cleared.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_cmd_commodities()
        {
            new TestRunner(@"test\baseline\cmd-commodities.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_cmd_convert()
        {
            new TestRunner(@"test\baseline\cmd-convert.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_cmd_csv()
        {
            new TestRunner(@"test\baseline\cmd-csv.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_cmd_echo()
        {
            new TestRunner(@"test\baseline\cmd-echo.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_cmd_entry()
        {
            new TestRunner(@"test\baseline\cmd-entry.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_cmd_equity()
        {
            new TestRunner(@"test\baseline\cmd-equity.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_cmd_payees()
        {
            new TestRunner(@"test\baseline\cmd-payees.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_cmd_pricedb()
        {
            new TestRunner(@"test\baseline\cmd-pricedb.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_cmd_pricemap()
        {
            new TestRunner(@"test\baseline\cmd-pricemap.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_cmd_prices()
        {
            new TestRunner(@"test\baseline\cmd-prices.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_cmd_print()
        {
            new TestRunner(@"test\baseline\cmd-print.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_cmd_register()
        {
            new TestRunner(@"test\baseline\cmd-register.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_cmd_script()
        {
            new TestRunner(@"test\baseline\cmd-script.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_cmd_select()
        {
            new TestRunner(@"test\baseline\cmd-select.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_cmd_source()
        {
            new TestRunner(@"test\baseline\cmd-source.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_cmd_stats()
        {
            new TestRunner(@"test\baseline\cmd-stats.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_cmd_tags()
        {
            new TestRunner(@"test\baseline\cmd-tags.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_cmd_xact()
        {
            new TestRunner(@"test\baseline\cmd-xact.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_dir_account()
        {
            new TestRunner(@"test\baseline\dir-account.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_dir_alias_fail()
        {
            new TestRunner(@"test\baseline\dir-alias-fail.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_dir_alias()
        {
            new TestRunner(@"test\baseline\dir-alias.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_dir_apply()
        {
            new TestRunner(@"test\baseline\dir-apply.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_dir_commodity_alias()
        {
            new TestRunner(@"test\baseline\dir-commodity-alias.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_dir_commodity_value()
        {
            new TestRunner(@"test\baseline\dir-commodity-value.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_dir_commodity()
        {
            new TestRunner(@"test\baseline\dir-commodity.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_dir_fixed()
        {
            new TestRunner(@"test\baseline\dir-fixed.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        [Ignore]  // Reason: Requires Python integration
        public void IntegrationTest_test_baseline_dir_import_py()
        {
            new TestRunner(@"test\baseline\dir-import_py.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_dir_payee()
        {
            new TestRunner(@"test\baseline\dir-payee.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        [Ignore]  // Reason: Requires Python integration
        public void IntegrationTest_test_baseline_dir_python_py()
        {
            new TestRunner(@"test\baseline\dir-python_py.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_dir_tag()
        {
            new TestRunner(@"test\baseline\dir-tag.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_feat_annotations()
        {
            new TestRunner(@"test\baseline\feat-annotations.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_feat_balance_assignments()
        {
            new TestRunner(@"test\baseline\feat-balance-assignments.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_feat_balance_assert_off()
        {
            new TestRunner(@"test\baseline\feat-balance_assert-off.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_feat_balance_assert()
        {
            new TestRunner(@"test\baseline\feat-balance_assert.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_feat_balance_assert_split()
        {
            new TestRunner(@"test\baseline\feat-balance_assert_split.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_feat_check()
        {
            new TestRunner(@"test\baseline\feat-check.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_feat_convert_with_directives()
        {
            new TestRunner(@"test\baseline\feat-convert-with-directives.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_feat_fixated_prices()
        {
            new TestRunner(@"test\baseline\feat-fixated-prices.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        [Ignore]  // Reason: Requires Python integration
        public void IntegrationTest_test_baseline_feat_import_py()
        {
            new TestRunner(@"test\baseline\feat-import_py.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        [Ignore]  // Reason: Requires Python integration
        public void IntegrationTest_test_baseline_feat_option_py()
        {
            new TestRunner(@"test\baseline\feat-option_py.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_feat_value_expr()
        {
            new TestRunner(@"test\baseline\feat-value-expr.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        [Ignore]  // Reason: Requires Python integration
        public void IntegrationTest_test_baseline_feat_value_py2()
        {
            new TestRunner(@"test\baseline\feat-value_py2.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        [Ignore]  // Reason: Requires Python integration
        public void IntegrationTest_test_baseline_feat_value_py3()
        {
            new TestRunner(@"test\baseline\feat-value_py3.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_abbrev_len()
        {
            new TestRunner(@"test\baseline\opt-abbrev-len.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_account_width()
        {
            new TestRunner(@"test\baseline\opt-account-width.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_account()
        {
            new TestRunner(@"test\baseline\opt-account.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_actual()
        {
            new TestRunner(@"test\baseline\opt-actual.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_add_budget()
        {
            new TestRunner(@"test\baseline\opt-add-budget.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_amount_data()
        {
            new TestRunner(@"test\baseline\opt-amount-data.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_amount_width()
        {
            new TestRunner(@"test\baseline\opt-amount-width.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_amount()
        {
            new TestRunner(@"test\baseline\opt-amount.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_auto_match()
        {
            new TestRunner(@"test\baseline\opt-auto-match.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_aux_date()
        {
            new TestRunner(@"test\baseline\opt-aux-date.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_average_lot_prices()
        {
            new TestRunner(@"test\baseline\opt-average-lot-prices.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_average()
        {
            new TestRunner(@"test\baseline\opt-average.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_balance_format()
        {
            new TestRunner(@"test\baseline\opt-balance-format.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_base()
        {
            new TestRunner(@"test\baseline\opt-base.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_basis()
        {
            new TestRunner(@"test\baseline\opt-basis.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_begin()
        {
            new TestRunner(@"test\baseline\opt-begin.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_bold_if()
        {
            new TestRunner(@"test\baseline\opt-bold-if.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_budget_format()
        {
            new TestRunner(@"test\baseline\opt-budget-format.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_budget()
        {
            new TestRunner(@"test\baseline\opt-budget.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_budget_only()
        {
            new TestRunner(@"test\baseline\opt-budget_only.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_budget_range()
        {
            new TestRunner(@"test\baseline\opt-budget_range.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_by_payee()
        {
            new TestRunner(@"test\baseline\opt-by-payee.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_check_payees()
        {
            new TestRunner(@"test\baseline\opt-check-payees.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_cleared_format()
        {
            new TestRunner(@"test\baseline\opt-cleared-format.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_cleared()
        {
            new TestRunner(@"test\baseline\opt-cleared.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_code_as_account()
        {
            new TestRunner(@"test\baseline\opt-code-as-account.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_code_as_payee()
        {
            new TestRunner(@"test\baseline\opt-code-as-payee.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_collapse_if_zero()
        {
            new TestRunner(@"test\baseline\opt-collapse-if-zero.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_collapse()
        {
            new TestRunner(@"test\baseline\opt-collapse.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_collapse_reg()
        {
            new TestRunner(@"test\baseline\opt-collapse_reg.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_color()
        {
            new TestRunner(@"test\baseline\opt-color.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_columns()
        {
            new TestRunner(@"test\baseline\opt-columns.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_commodity_as_account()
        {
            new TestRunner(@"test\baseline\opt-commodity-as-account.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_commodity_as_payee()
        {
            new TestRunner(@"test\baseline\opt-commodity-as-payee.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_cost()
        {
            new TestRunner(@"test\baseline\opt-cost.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_count()
        {
            new TestRunner(@"test\baseline\opt-count.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_csv_format()
        {
            new TestRunner(@"test\baseline\opt-csv-format.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_current()
        {
            new TestRunner(@"test\baseline\opt-current.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_daily()
        {
            new TestRunner(@"test\baseline\opt-daily.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_date_format()
        {
            new TestRunner(@"test\baseline\opt-date-format.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_date_width()
        {
            new TestRunner(@"test\baseline\opt-date-width.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_date()
        {
            new TestRunner(@"test\baseline\opt-date.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_datetime_format()
        {
            new TestRunner(@"test\baseline\opt-datetime-format.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_day_break()
        {
            new TestRunner(@"test\baseline\opt-day-break.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_dc()
        {
            new TestRunner(@"test\baseline\opt-dc.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_decimal_comma()
        {
            new TestRunner(@"test\baseline\opt-decimal-comma.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_depth()
        {
            new TestRunner(@"test\baseline\opt-depth.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_depth_flat()
        {
            new TestRunner(@"test\baseline\opt-depth_flat.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_deviation()
        {
            new TestRunner(@"test\baseline\opt-deviation.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_display_amount()
        {
            new TestRunner(@"test\baseline\opt-display-amount.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_display_total()
        {
            new TestRunner(@"test\baseline\opt-display-total.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_display()
        {
            new TestRunner(@"test\baseline\opt-display.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_dow()
        {
            new TestRunner(@"test\baseline\opt-dow.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_empty()
        {
            new TestRunner(@"test\baseline\opt-empty.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_empty_bal()
        {
            new TestRunner(@"test\baseline\opt-empty_bal.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_empty_bal_flat()
        {
            new TestRunner(@"test\baseline\opt-empty_bal_flat.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_empty_collapse()
        {
            new TestRunner(@"test\baseline\opt-empty_collapse.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_end()
        {
            new TestRunner(@"test\baseline\opt-end.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_equity()
        {
            new TestRunner(@"test\baseline\opt-equity.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_exact()
        {
            new TestRunner(@"test\baseline\opt-exact.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_exchange()
        {
            new TestRunner(@"test\baseline\opt-exchange.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_explicit()
        {
            new TestRunner(@"test\baseline\opt-explicit.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_file()
        {
            new TestRunner(@"test\baseline\opt-file.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_flat()
        {
            new TestRunner(@"test\baseline\opt-flat.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_force_color()
        {
            new TestRunner(@"test\baseline\opt-force-color.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_forecast_while()
        {
            new TestRunner(@"test\baseline\opt-forecast-while.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_forecast_years()
        {
            new TestRunner(@"test\baseline\opt-forecast-years.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_forecast_only()
        {
            new TestRunner(@"test\baseline\opt-forecast_only.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_format()
        {
            new TestRunner(@"test\baseline\opt-format.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_gain()
        {
            new TestRunner(@"test\baseline\opt-gain.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_group_by()
        {
            new TestRunner(@"test\baseline\opt-group-by.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_group_title_format()
        {
            new TestRunner(@"test\baseline\opt-group-title-format.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_head()
        {
            new TestRunner(@"test\baseline\opt-head.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_historical()
        {
            new TestRunner(@"test\baseline\opt-historical.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_immediate()
        {
            new TestRunner(@"test\baseline\opt-immediate.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_init_file()
        {
            new TestRunner(@"test\baseline\opt-init-file.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_inject()
        {
            new TestRunner(@"test\baseline\opt-inject.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_input_date_format()
        {
            new TestRunner(@"test\baseline\opt-input-date-format.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_invert()
        {
            new TestRunner(@"test\baseline\opt-invert.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_limit()
        {
            new TestRunner(@"test\baseline\opt-limit.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_lot_dates()
        {
            new TestRunner(@"test\baseline\opt-lot-dates.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_lot_notes()
        {
            new TestRunner(@"test\baseline\opt-lot-notes.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_lot_prices()
        {
            new TestRunner(@"test\baseline\opt-lot-prices.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_lot_tags()
        {
            new TestRunner(@"test\baseline\opt-lot-tags.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_lots_actual()
        {
            new TestRunner(@"test\baseline\opt-lots-actual.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_lots()
        {
            new TestRunner(@"test\baseline\opt-lots.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_lots_basis()
        {
            new TestRunner(@"test\baseline\opt-lots_basis.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_lots_basis_base()
        {
            new TestRunner(@"test\baseline\opt-lots_basis_base.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_market()
        {
            new TestRunner(@"test\baseline\opt-market.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_master_account()
        {
            new TestRunner(@"test\baseline\opt-master-account.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_meta_width()
        {
            new TestRunner(@"test\baseline\opt-meta-width.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_meta()
        {
            new TestRunner(@"test\baseline\opt-meta.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_monthly()
        {
            new TestRunner(@"test\baseline\opt-monthly.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_no_aliases()
        {
            new TestRunner(@"test\baseline\opt-no-aliases.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_no_revalued()
        {
            new TestRunner(@"test\baseline\opt-no-revalued.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_no_rounding()
        {
            new TestRunner(@"test\baseline\opt-no-rounding.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_no_titles()
        {
            new TestRunner(@"test\baseline\opt-no-titles.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_no_total()
        {
            new TestRunner(@"test\baseline\opt-no-total.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_now()
        {
            new TestRunner(@"test\baseline\opt-now.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_only()
        {
            new TestRunner(@"test\baseline\opt-only.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_output()
        {
            new TestRunner(@"test\baseline\opt-output.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_pager()
        {
            new TestRunner(@"test\baseline\opt-pager.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_payee_as_account()
        {
            new TestRunner(@"test\baseline\opt-payee-as-account.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_payee_width()
        {
            new TestRunner(@"test\baseline\opt-payee-width.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_payee()
        {
            new TestRunner(@"test\baseline\opt-payee.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_pedantic()
        {
            new TestRunner(@"test\baseline\opt-pedantic.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_pending()
        {
            new TestRunner(@"test\baseline\opt-pending.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_percent()
        {
            new TestRunner(@"test\baseline\opt-percent.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_period()
        {
            new TestRunner(@"test\baseline\opt-period.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_permissive()
        {
            new TestRunner(@"test\baseline\opt-permissive.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_pivot()
        {
            new TestRunner(@"test\baseline\opt-pivot.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_plot_amount_format()
        {
            new TestRunner(@"test\baseline\opt-plot-amount-format.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_plot_total_format()
        {
            new TestRunner(@"test\baseline\opt-plot-total-format.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_prepend_format()
        {
            new TestRunner(@"test\baseline\opt-prepend-format.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_prepend_width()
        {
            new TestRunner(@"test\baseline\opt-prepend-width.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_price_db()
        {
            new TestRunner(@"test\baseline\opt-price-db.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_price()
        {
            new TestRunner(@"test\baseline\opt-price.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_pricedb_format()
        {
            new TestRunner(@"test\baseline\opt-pricedb-format.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_prices_format()
        {
            new TestRunner(@"test\baseline\opt-prices-format.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_primary_date()
        {
            new TestRunner(@"test\baseline\opt-primary-date.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_quantity()
        {
            new TestRunner(@"test\baseline\opt-quantity.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_quarterly()
        {
            new TestRunner(@"test\baseline\opt-quarterly.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_raw()
        {
            new TestRunner(@"test\baseline\opt-raw.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_real()
        {
            new TestRunner(@"test\baseline\opt-real.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_recursive_aliases()
        {
            new TestRunner(@"test\baseline\opt-recursive-aliases.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_register_format()
        {
            new TestRunner(@"test\baseline\opt-register-format.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_related_all()
        {
            new TestRunner(@"test\baseline\opt-related-all.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_related()
        {
            new TestRunner(@"test\baseline\opt-related.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_revalued_only()
        {
            new TestRunner(@"test\baseline\opt-revalued-only.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_revalued()
        {
            new TestRunner(@"test\baseline\opt-revalued.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_rich_data()
        {
            new TestRunner(@"test\baseline\opt-rich-data.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_script()
        {
            new TestRunner(@"test\baseline\opt-script.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_sort_all()
        {
            new TestRunner(@"test\baseline\opt-sort-all.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_sort_xacts()
        {
            new TestRunner(@"test\baseline\opt-sort-xacts.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_sort()
        {
            new TestRunner(@"test\baseline\opt-sort.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_start_of_week()
        {
            new TestRunner(@"test\baseline\opt-start-of-week.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_strict()
        {
            new TestRunner(@"test\baseline\opt-strict.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_subtotal()
        {
            new TestRunner(@"test\baseline\opt-subtotal.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_tail()
        {
            new TestRunner(@"test\baseline\opt-tail.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_time_colon()
        {
            new TestRunner(@"test\baseline\opt-time-colon.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_time_report()
        {
            new TestRunner(@"test\baseline\opt-time-report.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_total_data()
        {
            new TestRunner(@"test\baseline\opt-total-data.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_total_width()
        {
            new TestRunner(@"test\baseline\opt-total-width.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_total()
        {
            new TestRunner(@"test\baseline\opt-total.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_trace()
        {
            new TestRunner(@"test\baseline\opt-trace.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_truncate()
        {
            new TestRunner(@"test\baseline\opt-truncate.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_unbudgeted()
        {
            new TestRunner(@"test\baseline\opt-unbudgeted.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_uncleared()
        {
            new TestRunner(@"test\baseline\opt-uncleared.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_unrealized_gains()
        {
            new TestRunner(@"test\baseline\opt-unrealized-gains.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_unrealized_losses()
        {
            new TestRunner(@"test\baseline\opt-unrealized-losses.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_unrealized()
        {
            new TestRunner(@"test\baseline\opt-unrealized.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_unround()
        {
            new TestRunner(@"test\baseline\opt-unround.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_value_expr()
        {
            new TestRunner(@"test\baseline\opt-value-expr.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_values()
        {
            new TestRunner(@"test\baseline\opt-values.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_weekly()
        {
            new TestRunner(@"test\baseline\opt-weekly.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_weekly_empty()
        {
            new TestRunner(@"test\baseline\opt-weekly_empty.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_wide()
        {
            new TestRunner(@"test\baseline\opt-wide.test").Run();
        }
		
        [TestMethod]
        [TestCategory("Integration")]
        public void IntegrationTest_test_baseline_opt_yearly()
        {
            new TestRunner(@"test\baseline\opt-yearly.test").Run();
        }
		
    }
}
