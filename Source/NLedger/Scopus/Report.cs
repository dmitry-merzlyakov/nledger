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
using NLedger.Annotate;
using NLedger.Chain;
using NLedger.Commodities;
using NLedger.Drafts;
using NLedger.Expressions;
using NLedger.Filters;
using NLedger.Formatting;
using NLedger.Items;
using NLedger.Iterators;
using NLedger.Output;
using NLedger.Print;
using NLedger.Querying;
using NLedger.Times;
using NLedger.Utility;
using NLedger.Utils;
using NLedger.Values;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Scopus
{
    public class Report : Scope
    {
        public const string CurrentReportDescription = "current report";

        #region Option Names
        public const string OptionAbbrevLen = "abbrev_len_";
        public const string OptionAccount = "account_";
        public const string OptionActual = "actual";
        public const string OptionAddBudget = "add_budget";
        public const string OptionAmount = "amount_";
        public const string OptionAmountData = "amount_data";
        public const string OptionAnon = "anon";
        public const string OptionAutoMatch = "auto_match";
        public const string OptionAverage = "average";
        public const string OptionBalanceFormat = "balance_format_";
        public const string OptionBase = "base";
        public const string OptionBasis = "basis";
        public const string OptionBegin = "begin_";
        public const string OptionBoldIf = "bold_if_";
        public const string OptionBudget = "budget";
        public const string OptionBudgetFormat = "budget_format_";
        public const string OptionByPayee = "by_payee";
        public const string OptionCleared = "cleared";
        public const string OptionClearedFormat = "cleared_format_";
        public const string OptionColor = "color";
        public const string OptionCollapse = "collapse";
        public const string OptionCollapseIfZero = "collapse_if_zero";
        public const string OptionColumns = "columns_";
        public const string OptionCount = "count";
        public const string OptionCsvFormat = "csv_format_";
        public const string OptionCurrent = "current";
        public const string OptionDaily = "daily";
        public const string OptionDate = "date_";
        public const string OptionDateFormat = "date_format_";
        public const string OptionDateTimeFormat = "datetime_format_";
        public const string OptionDc = "dc";
        public const string OptionDepth = "depth_";
        public const string OptionDeviation = "deviation";
        public const string OptionDisplay = "display_";
        public const string OptionDisplayAmount = "display_amount_";
        public const string OptionDisplayTotal = "display_total_";
        public const string OptionDow = "dow";
        public const string OptionAuxDate = "aux_date";
        public const string OptionEmpty = "empty";
        public const string OptionEnd = "end_";        
        public const string OptionEquity = "equity";
        public const string OptionExact = "exact";
        public const string OptionExchange = "exchange_";
        public const string OptionFlat = "flat";
        public const string OptionForceColor = "force_color";
        public const string OptionForcePager = "force_pager";
        public const string OptionForecastWhile = "forecast_while_";
        public const string OptionForecastYears = "forecast_years_";
        public const string OptionFormat = "format_";
        public const string OptionGain = "gain";
        public const string OptionGenerated = "generated";
        public const string OptionGroupBy = "group_by_";
        public const string OptionGroupTitleFormat = "group_title_format_";
        public const string OptionHead = "head_";
        public const string OptionHistorical = "historical";
        public const string OptionImmediate = "immediate";
        public const string OptionInject = "inject_";
        public const string OptionInvert = "invert";
        public const string OptionLimit = "limit_";
        public const string OptionLotDates = "lot_dates";
        public const string OptionLotPrices = "lot_prices";
        public const string OptionLotNotes = "lot_notes";
        public const string OptionLots = "lots";
        public const string OptionLotsActual = "lots_actual";
        public const string OptionMarket = "market";
        public const string OptionMeta = "meta_";
        public const string OptionMonthly = "monthly";
        public const string OptionNoColor = "no_color";
        public const string OptionNoRevalued = "no_revalued";
        public const string OptionNoRounding = "no_rounding";
        public const string OptionNoTitles = "no_titles";
        public const string OptionNoTotal = "no_total";
        public const string OptionNow = "now_";
        public const string OptionOnly = "only_";
        public const string OptionOutput = "output_";
        public const string OptionPager = "pager_";
        public const string OptionNoPager = "no_pager";
        public const string OptionPayee = "payee_";
        public const string OptionPending = "pending";
        public const string OptionPercent = "percent";
        public const string OptionPeriod = "period_";
        public const string OptionPivot = "pivot_";
        public const string OptionPlotAmountFormat = "plot_amount_format_";
        public const string OptionPlotTotalFormat = "plot_total_format_";
        public const string OptionPrependFormat = "prepend_format_";
        public const string OptionPrependWidth = "prepend_width_";
        public const string OptionPrice = "price";
        public const string OptionPricesFormat = "prices_format_";
        public const string OptionPriceDbFormat = "pricedb_format_";
        public const string OptionPrimaryDate = "primary_date";
        public const string OptionQuantity = "quantity";
        public const string OptionQuarterly = "quarterly";
        public const string OptionRaw = "raw";
        public const string OptionReal = "real";
        public const string OptionRegisterFormat = "register_format_";
        public const string OptionRelated = "related";
        public const string OptionRelatedAll = "related_all";
        public const string OptionRevalued = "revalued";
        public const string OptionRevaluedOnly = "revalued_only";
        public const string OptionRevaluedTotal = "revalued_total_";
        public const string OptionRichData = "rich_data";
        public const string OptionSeed = "seed_";
        public const string OptionSort = "sort_";
        public const string OptionSortAll = "sort_all_";
        public const string OptionSortXacts = "sort_xacts_";
        public const string OptionStartOfWeek = "start_of_week_";
        public const string OptionSubTotal = "subtotal";
        public const string OptionTail = "tail_";
        public const string OptionTimeReport = "time_report";
        public const string OptionTotal = "total_";
        public const string OptionTotalData = "total_data";
        public const string OptionTruncate = "truncate_";
        public const string OptionUnbudgeted = "unbudgeted";
        public const string OptionUncleared = "uncleared";
        public const string OptionUnrealized = "unrealized";
        public const string OptionUnrealizedGains = "unrealized_gains_";
        public const string OptionUnrealizedLosses = "unrealized_losses_";
        public const string OptionUnround = "unround";
        public const string OptionWeekly = "weekly";
        public const string OptionWide = "wide";
        public const string OptionYearly = "yearly";
        public const string OptionMetaWidth = "meta_width_";
        public const string OptionDateWidth = "date_width_";
        public const string OptionPayeeWidth = "payee_width_";
        public const string OptionAccountWidth = "account_width_";
        public const string OptionAmountWidth = "amount_width_";
        public const string OptionAverageLotPrices = "average_lot_prices";
        public const string OptionTotalWidth = "total_width_";
        public const string OptionValues = "values";
        #endregion

        public Report()
        {
            CreateOptions();
            CreateLookupItems();
        }

        public Report (Session session) :this()
        {
            Session = session;
            Terminus = TimesCommon.Current.CurrentTime;
        }

        public Report(Report report) : this()
        {
            Session = report.Session;
            OutputStream = report.OutputStream;
            Terminus = report.Terminus;
            BudgetFlags = report.BudgetFlags;
        }

        public override string Description
        {
            get { return CurrentReportDescription; }
        }

        public Session Session { get; private set; }
        public ReportBudgetFlags BudgetFlags { get; private set; }
        public DateTime Terminus { get; private set; }

        public TextWriter OutputStream { get; set; }

        #region Options
        public Option AbbrevLenHandler { get; private set; }
        public Option AccountHandler { get; private set; }
        public Option ActualHandler { get; private set; }
        public Option AddBudgetHandler { get; private set; }
        public MergedExprOption AmountHandler { get; private set; }
        public Option AmountDataHandler { get; private set; }
        public Option AnonHandler { get; private set; }
        public Option AutoMatchHandler { get; private set; }
        public Option AverageHandler { get; private set; }
        public Option BalanceFormatHandler { get; private set; }
        public Option BaseHandler { get; private set; }
        public Option BasisHandler { get; private set; }
        public Option BeginHandler { get; private set; }
        public ExprOption BoldIfHandler { get; private set; }
        public Option BudgetHandler { get; private set; }
        public Option BudgedFormatHandler { get; private set; }
        public Option ByPayeeHandler { get; private set; }
        public Option ClearedHandler { get; private set; }
        public Option ClearedFormatHandler { get; private set; }
        public Option ColorHandler { get; private set; }
        public Option CollapseHandler { get; private set; }
        public Option CollapseIfZeroHandler { get; private set; }
        public Option ColumnsHandler { get; private set; }
        public Option CountHandler { get; private set; }
        public Option CsvFormatHandler { get; private set; }
        public Option CurrentHandler { get; private set; }
        public Option DailyHandler { get; private set; }
        public Option DateHandler { get; private set; }
        public Option DateFormatHandler { get; private set; }
        public Option DateTimeFormatHandler { get; private set; }
        public Option DcHandler { get; private set; }
        public Option DepthHandler { get; private set; }
        public Option DeviationHandler{ get; private set; }
        public Option DisplayHandler { get; private set; }
        public MergedExprOption DisplayAmountHandler { get; private set; }
        public MergedExprOption DisplayTotalHandler { get; private set; }
        public Option DowHandler { get; private set; }
        public Option AuxDateHandler { get; private set; }
        public Option EmptyHandler { get; private set; }
        public Option EndHandler { get; private set; }
        public Option EquityHandler { get; private set; }
        public Option ExactHandler { get; private set; }
        public Option ExchangeHandler { get; private set; }
        public Option FlatHandler { get; private set; }
        public Option ForceColorHandler { get; private set; }
        public Option ForcePagerHandler { get; private set; }
        public Option ForecastWhileHandler { get; private set; }
        public Option ForecastYearsHandler { get; private set; }
        public Option FormatHandler { get; private set; }
        public Option GainHandler { get; private set; }
        public Option GeneratedHandler { get; private set; }
        public ExprOption GroupByHandler { get; private set; }
        public Option GroupTitleFormatHandler { get; private set; }
        public Option HeadHandler { get; private set; }
        public Option HistoricalHandler { get; private set; }
        public Option ImmediateHandler { get; private set; }
        public Option InjectHandler { get; private set; }
        public Option InvertHandler { get; private set; }
        public Option LimitHandler { get; private set; }
        public Option LotDatesHandler { get; private set; }
        public Option LotPricesHandler { get; private set; }
        public Option LotNotesHandler { get; private set; }
        public Option LotsHandler { get; private set; }
        public Option LotsActualHandler { get; private set; }
        public Option MarketHandler { get; private set; }
        public Option MetaHandler { get; private set; }
        public Option MonthlyHandler { get; private set; }
        public Option NoColorHandler { get; private set; }
        public Option NoRevaluedHandler { get; private set; }
        public Option NoRoundingHandler { get; private set; }
        public Option NoTitlesHandler { get; private set; }
        public Option NoTotalHandler { get; private set; }
        public Option NowHandler { get; private set; }
        public Option OnlyHandler { get; private set; }
        public Option OutputHandler { get; private set; }
        public Option PagerHandler { get; private set; }
        public Option NoPagerHandler { get; private set; }
        public Option PayeeHandler { get; private set; }
        public Option PendingHandler { get; private set; }
        public Option PercentHandler { get; private set; }
        public Option PeriodHandler { get; private set; }
        public Option PivotHandler { get; private set; }
        public Option PlotAmountFormatHandler { get; private set; }
        public Option PlotTotalFormatHandler { get; private set; }
        public Option PrependFormatHandler { get; private set; }
        public Option PrependWidthHandler { get; private set; }
        public Option PriceHandler { get; private set; }
        public Option PricesFormatHandler { get; private set; }
        public Option PriceDbFormatHandler { get; private set; }
        public Option PrimaryDateHandler { get; private set; }
        public Option QuantityHandler { get; private set; }
        public Option QuarterlyHandler { get; private set; }
        public Option RawHandler { get; private set; }
        public Option RealHandler { get; private set; }
        public Option RegisterFormatHandler { get; private set; }
        public Option RelatedHandler { get; private set; }
        public Option RelatedAllHandler { get; private set; }
        public Option RevaluedHandler { get; private set; }
        public Option RevaluedOnlyHandler { get; private set; }
        public ExprOption RevaluedTotalHandler { get; private set; }
        public Option RichDataHandler { get; private set; }
        public Option SeedHandler { get; private set; }
        public Option SortHandler { get; private set; }
        public Option SortAllHandler { get; private set; }
        public Option SortXactsHandler { get; private set; }
        public Option StartOfWeekHandler { get; private set; }
        public Option SubTotalHandler { get; private set; }
        public Option TailHandler { get; private set; }
        public Option TimeReportHandler { get; private set; }
        public MergedExprOption TotalHandler { get; private set; }
        public Option TotalDataHandler { get; private set; }
        public Option TruncateHandler { get; private set; }
        public Option UnbudgetedHandler { get; private set; }
        public Option UnclearedHandler { get; private set; }
        public Option UnrealizedHandler { get; private set; }
        public Option UnrealizedGainsHandler { get; private set; }
        public Option UnrealizedLossesHandler { get; private set; }
        public Option UnroundHandler { get; private set; }
        public Option WeeklyHandler { get; private set; }
        public Option WideHandler { get; private set; }
        public Option YearlyHandler { get; private set; }
        public Option MetaWidthHandler { get; private set; }
        public Option DateWidthHandler { get; private set; }
        public Option PayeeWidthHandler { get; private set; }
        public Option AccountWidthHandler { get; private set; }
        public Option AmountWidthHandler { get; private set; }
        public Option AverageLotPricesHandler { get; private set; }
        public Option TotalWidthHandler { get; private set; }
        public Option ValuesHandler { get; private set; }
        #endregion

        public override ExprOp Lookup(SymbolKindEnum kind, string name)
        {
            ExprOp def = Session.Lookup(kind, name);
            if (def != null)
                return def;

            return LookupItems.Lookup(kind, name, this);
        }

        public override void Define(SymbolKindEnum kind, string name, ExprOp def)
        {
            Session.Define(kind, name, def);
        }

        public void QuickClose()
        {
            if (OutputStream != null)
                OutputStream = FileSystem.OutputStreamClose(OutputStream);
        }

        public string ReportOptions()
        {
            return Options.Report();
        }

        public void NormalizeOptions(string verb)
        {
            // Patch up some of the reporting options based on what kind of
            // command it was.

            // #if HAVE_ISATTY
            if (!ForceColorHandler.Handled)
            {
                if (!NoColorHandler.Handled && VirtualConsole.IsAtty())
                    ColorHandler.On("?normalize");
                if (ColorHandler.Handled && !VirtualConsole.IsAtty())
                    ColorHandler.Off();
            }
            else
            {
                ColorHandler.On("?normalize");
            }

            if (!ForcePagerHandler.Handled)
            {
                if (PagerHandler.Handled && !VirtualConsole.IsAtty())
                    PagerHandler.Off();
            }
            // #endif

            if (OutputHandler.Handled)
            {
                if (ColorHandler.Handled && !ForceColorHandler.Handled)
                    ColorHandler.Off();
                if (PagerHandler.Handled && !ForcePagerHandler.Handled)
                    PagerHandler.Off();
            }

            Item.UseAuxDate = AuxDateHandler.Handled && !PrimaryDateHandler.Handled;

            CommodityPool.Current.KeepBase = BaseHandler.Handled;
            CommodityPool.Current.GetQuotes = Session.DownloadHandler.Handled;

            if (Session.PriceExpHandler.Handled)
                CommodityPool.Current.QuoteLeeway = long.Parse(Session.PriceExpHandler.Value) * 3600;

            if (Session.PriceDbHandler.Handled)
                CommodityPool.Current.PriceDb = Session.PriceDbHandler.Str();
            else
                CommodityPool.Current.PriceDb = null;

            if (DateFormatHandler.Handled)
                TimesCommon.Current.SetDateFormat(DateFormatHandler.Str());
            if (DateTimeFormatHandler.Handled)
                TimesCommon.Current.SetDateTimeFormat(DateTimeFormatHandler.Str());
            if (StartOfWeekHandler.Handled)
            {
                DayOfWeek? weekday = DateParserLexer.StringToDayOfWeek(StartOfWeekHandler.Str());
                if (weekday.HasValue)
                    TimesCommon.Current.StartToWeek = weekday.Value;
            }

            long metaWidth = -1;

            if (!PrependFormatHandler.Handled && MetaHandler.Handled)
            {
                if (!MetaWidthHandler.Handled)
                {
                    int i = MetaHandler.Str().IndexOf(':');
                    if (i >= 0)
                    {
                        MetaWidthHandler.On("?normalize", MetaHandler.Str().Substring(i + 1));
                        MetaHandler.On("?normalize", MetaHandler.Str().Substring(0, i));
                    }
                }
                if (MetaWidthHandler.Handled)
                {
                    PrependFormatHandler.On("?normalize", String.Format("%(justify(truncated(tag(\"{0}\"), {1} - 1), {1}))",
                        MetaHandler.Str(), MetaWidthHandler.Value));
                    metaWidth = long.Parse(MetaWidthHandler.Value);
                }
                else
                {
                    PrependFormatHandler.On("?normalize", String.Format("%(tag(\"{0}\"))", MetaHandler.Str()));
                }
            }

            if (verb == "print" || verb == "xact" || verb == "dump")
            {
                RelatedAllHandler.Parent = this;
                RelatedAllHandler.On("?normalize");
            }
            else if (verb == "equity")
            {
                EquityHandler.On("?normalize");
            }

            if (!verb.StartsWith("b") && !verb.StartsWith("r"))
                BaseHandler.On("?normalize");

            // If a time period was specified with -p, check whether it also gave a
            // begin and/or end to the report period (though these can be overridden
            // using -b or -e).  Then, if no _duration_ was specified (such as monthly),
            // then ignore the period since the begin/end are the only interesting
            // details.
            if (PeriodHandler.Handled)
                NormalizePeriod();

            // If -j or -J were specified, set the appropriate format string now so as
            // to avoid option ordering issues were we to have done it during the
            // initial parsing of the options.
            if (AmountDataHandler.Handled)
                FormatHandler.On("?normalize", PlotAmountFormatHandler.Value);
            else if (TotalDataHandler.Handled)
                FormatHandler.On("?normalize", PlotTotalFormatHandler.Value);

            // If the --exchange (-X) option was used, parse out any final price
            // settings that may be there.
            if (ExchangeHandler.Handled && ExchangeHandler.Str().Contains('='))
                Value.Get(0).ExchangeCommodities(ExchangeHandler.Str(), true, Terminus);

            if (PercentHandler.Handled)
            {
                Commodity.Defaults.DecimalCommaByDefault = false;
                if (MarketHandler.Handled)
                    TotalHandler.On("?normalize",
                        "(__tmp = market(parent.total, value_date, exchange);" +
                        " ((is_account & parent & __tmp) ?" +
                        "   percent(scrub(market(total, value_date, exchange)), " +
                        "           scrub(__tmp)) : 0))");
            }

            if (ImmediateHandler.Handled && MarketHandler.Handled)
                AmountHandler.On("?normalize", "market(amount_expr, value_date, exchange)");

            long cols = 0;

            if (ColumnsHandler.Handled)
                cols = long.Parse(ColumnsHandler.Value);
            else
            {
                string columns = VirtualEnvironment.GetEnvironmentVariable("COLUMNS");
                if (!string.IsNullOrEmpty(columns))
                    cols = long.Parse(columns);
                else
                    cols = 80;
            }

            if (metaWidth > 0)
                cols = cols - metaWidth;

            if (cols > 0)
            {
                Logger.Current.Debug("auto.columns", () => String.Format("cols = {0}", cols));

                long dateWidth = DateWidthHandler.Handled ? long.Parse(DateWidthHandler.Str()) : TimesCommon.Current.FormatDate(TimesCommon.Current.CurrentDate, FormatTypeEnum.FMT_PRINTED).Length;
                long payeeWidth = PayeeWidthHandler.Handled ? long.Parse(PayeeWidthHandler.Str()) : (long)(((double)cols) * 0.263157);
                long accountWidth = AccountWidthHandler.Handled ? long.Parse(AccountWidthHandler.Str()) : (long)(((double)cols) * 0.302631);
                long amountWidth = AmountWidthHandler.Handled ? long.Parse(AmountWidthHandler.Str()) : (long)(((double)cols) * 0.157894);
                long totalWidth = TotalWidthHandler.Handled ? long.Parse(TotalWidthHandler.Str()) : amountWidth;

                Logger.Current.Debug("auto.columns", () => String.Format("date_width    = {0}", dateWidth));
                Logger.Current.Debug("auto.columns", () => String.Format("payee_width   = {0}", payeeWidth));
                Logger.Current.Debug("auto.columns", () => String.Format("account_width = {0}", accountWidth));
                Logger.Current.Debug("auto.columns", () => String.Format("amount_width  = {0}", amountWidth));
                Logger.Current.Debug("auto.columns", () => String.Format("total_width   = {0}", totalWidth));

                if (!DateWidthHandler.Handled && !PayeeWidthHandler.Handled && !AccountWidthHandler.Handled && !AmountWidthHandler.Handled && !TotalWidthHandler.Handled)
                {
                    long total = (4 /* the spaces between */ + dateWidth + payeeWidth +
                        accountWidth + amountWidth + totalWidth +
                        (DcHandler.Handled ? 1 + amountWidth : 0));

                    while (total > cols && accountWidth > 5 && payeeWidth > 5)
                    {
                        Logger.Current.Debug("auto.columns", () => "adjusting account down");
                        if (total > cols)
                        {
                            --accountWidth;
                            --total;
                            if (total > cols)
                            {
                                --accountWidth;
                                --total;
                            }
                        }
                        if (total > cols)
                        {
                            --payeeWidth;
                            --total;
                        }
                        Logger.Current.Debug("auto.columns", () => String.Format("account_width now = {0}", accountWidth));
                    }
                }

                if (!MetaWidthHandler.Handled)
                    MetaWidthHandler.Value = "0";
                if (!PrependWidthHandler.Handled)
                    PrependWidthHandler.Value = "0";
                if (!DateWidthHandler.Handled)
                    DateWidthHandler.Value = dateWidth.ToString();
                if (!PayeeWidthHandler.Handled)
                    PayeeWidthHandler.Value = payeeWidth.ToString();
                if (!AccountWidthHandler.Handled)
                    AccountWidthHandler.Value = accountWidth.ToString();
                if (!AmountWidthHandler.Handled)
                    AmountWidthHandler.Value = amountWidth.ToString();
                if (!TotalWidthHandler.Handled)
                    TotalWidthHandler.Value = totalWidth.ToString();
            }
        }

        public void NormalizePeriod()
        {
            DateInterval interval = new DateInterval(PeriodHandler.Str());

            Date? begin = interval.Begin;
            Date? end = interval.End;

            if (!BeginHandler.Handled && begin.HasValue)
            {
                string predicate = "date>=[" + TimesCommon.ToIsoExtendedString(begin.Value) + "]";
                LimitHandler.On("?normalize", predicate);
            }
            if (!EndHandler.Handled && end.HasValue)
            {
                string predicate = "date<[" + TimesCommon.ToIsoExtendedString(end.Value) + "]";
                LimitHandler.On("?normalize", predicate);
            }

            if (interval.Duration == null)
                PeriodHandler.Off();
            else if (!SortAllHandler.Handled)
                SortXactsHandler.On("?normalize");
        }

        public void ParseQueryArgs(Value args, string whence)
        {
            Query query = new Query(args, WhatToKeep());

            if (query.HasQuery(QueryKindEnum.QUERY_LIMIT))
            {
                LimitHandler.On(whence, query.GetQuery(QueryKindEnum.QUERY_LIMIT));
                Logger.Current.Debug("report.predicate", () => String.Format("Limit predicate   = {0}", LimitHandler.Str()));
            }

            if (query.HasQuery(QueryKindEnum.QUERY_ONLY))
            {
                OnlyHandler.On(whence, query.GetQuery(QueryKindEnum.QUERY_ONLY));
                Logger.Current.Debug("report.predicate", () => String.Format("Only predicate    = {0}", OnlyHandler.Str()));
            }

            if (query.HasQuery(QueryKindEnum.QUERY_SHOW))
            {
                DisplayHandler.On(whence, query.GetQuery(QueryKindEnum.QUERY_SHOW));
                Logger.Current.Debug("report.predicate", () => String.Format("Display predicate = {0}", DisplayHandler.Str()));
            }

            if (query.HasQuery(QueryKindEnum.QUERY_BOLD))
            {
                BoldIfHandler.On(whence, query.GetQuery(QueryKindEnum.QUERY_BOLD));
                Logger.Current.Debug("report.predicate", () => String.Format("Bolding predicate = {0}", BoldIfHandler.Str()));
            }

            if (query.HasQuery(QueryKindEnum.QUERY_FOR))
            {
                PeriodHandler.On(whence, query.GetQuery(QueryKindEnum.QUERY_FOR));
                Logger.Current.Debug("report.predicate", () => String.Format("Report period     = {0}", PeriodHandler.Str()));

                NormalizePeriod();   // it needs normalization
            }
        }

        /// <summary>
        /// Ported from report_t::posts_report
        /// </summary>
        public void PostsReport(PostHandler handler)
        {
            handler = ChainCommon.ChainPostHandlers(handler, this);
            if (GroupByHandler.Handled)
            {
                PostSplitter splitter = new PostSplitter(handler, this, GroupByHandler.Expr);
                splitter.PostFlushFunc = v => Session.Journal.ClearXData();
                handler = splitter;
            }
            handler = ChainCommon.ChainPrePostHandlers(handler, this);

            JournalPostsIterator walker = new JournalPostsIterator(Session.Journal);
            new PassDownPosts(handler, walker.Get());

            if (!GroupByHandler.Handled)
                Session.Journal.ClearXData();
        }

        /// <summary>
        /// Ported from report_t::generate_report
        /// </summary>
        public void GenerateReport(PostHandler handler)
        {
            handler = ChainCommon.ChainPostHandlers(handler, this);

            GeneratePostsIterator walker = new GeneratePostsIterator(Session, 
                SeedHandler.Handled ? Int32.Parse(SeedHandler.Str()) : 0,
                HeadHandler.Handled ? Int32.Parse(HeadHandler.Str()) : 50);

            new PassDownPosts(handler, walker.Get());
        }

        /// <summary>
        /// Ported from report_t::accounts_report
        /// </summary>
        public void AccountsReport(AccountHandler handler)
        {
            PostHandler chain = ChainCommon.ChainPostHandlers(new IgnorePosts(), this, /* for_accounts_report= */ true);

            if (GroupByHandler.Handled)
            {
                PostSplitter splitter = new PostSplitter(chain, this, GroupByHandler.Expr);
                splitter.PreFlushFunc = new AccountsTitlePrinter(handler, this).Handle;
                splitter.PostFlushFunc = new AccountsFlusher(handler, this).Handle;
                chain = splitter;
            }
            chain = ChainCommon.ChainPrePostHandlers(chain, this);

            // The lifetime of the chain object controls the lifetime of all temporary
            // objects created within it during the call to pass_down_posts, which will
            // be needed later by the pass_down_accounts.
            JournalPostsIterator walker = new JournalPostsIterator(Session.Journal);
            new PassDownPosts(chain, walker.Get());

            if (!GroupByHandler.Handled)
                new AccountsFlusher(handler, this).Handle(Value.Empty);
        }

        /// <summary>
        /// Ported from struct accounts_title_printer
        /// </summary>
        private class AccountsTitlePrinter
        {
            public AccountsTitlePrinter(AccountHandler handler, Report report)
            {
                Handler = handler;
                Report = report;
            }

            public AccountHandler Handler { get; private set; }
            public Report Report { get; private set; }

            public void Handle(Value value)
            {
                if (!Report.NoTitlesHandler.Handled)
                {
                    string buf = value.Print();
                    Handler.Title(buf);
                }
            }
        }

        private class AccountsFlusher
        {
            public AccountsFlusher(AccountHandler handler, Report report)
            {
                Handler = handler;
                Report = report;
            }

            public AccountHandler Handler { get; private set; }
            public Report Report { get; private set; }

            public void Handle(Value value)
            {
                Report.AmountHandler.Expr.MarkUncomplited();
                Report.TotalHandler.Expr.MarkUncomplited();
                Report.DisplayAmountHandler.Expr.MarkUncomplited();
                Report.DisplayTotalHandler.Expr.MarkUncomplited();
                Report.RevaluedTotalHandler.Expr.MarkUncomplited();

                if (Report.DisplayHandler.Handled)
                {
                    Logger.Current.Debug("report.predicate", () => String.Format("Display predicate = {0}", Report.DisplayHandler.Str()));
                    if (!Report.SortHandler.Handled)
                    {
                        BasicAccountsIterator iter = new BasicAccountsIterator(Report.Session.Journal.Master);
                        new PassDownAccounts(Handler, iter.Get(), new Predicate(Report.DisplayHandler.Str(), Report.WhatToKeep()), Report);
                    }
                    else
                    {
                        Expr sortExpr = new Expr(Report.SortHandler.Str());
                        sortExpr.Context = Report;
                        SortedAccountsIterator iter = new SortedAccountsIterator(Report.Session.Journal.Master, sortExpr, Report, Report.FlatHandler.Handled);
                        new PassDownAccounts(Handler, iter.Get(), new Predicate(Report.DisplayHandler.Str(), Report.WhatToKeep()), Report);
                    }
                }
                else
                {
                    if (!Report.SortHandler.Handled)
                    {
                        BasicAccountsIterator iter = new BasicAccountsIterator(Report.Session.Journal.Master);
                        new PassDownAccounts(Handler, iter.Get());
                    }
                    else
                    {
                        Expr sortExpr = new Expr(Report.SortHandler.Str());
                        sortExpr.Context = Report;
                        SortedAccountsIterator iter = new SortedAccountsIterator(Report.Session.Journal.Master, sortExpr, Report, Report.FlatHandler.Handled);
                        new PassDownAccounts(Handler, iter.Get());
                    }
                }

                Report.Session.Journal.ClearXData();
            }
        }

        public void CommoditiesReport(PostHandler handler)
        {
            handler = ChainCommon.ChainHandlers(handler, this);

            PostCommoditiesIterator walker = new PostCommoditiesIterator(Session.Journal);
            new PassDownPosts(handler, walker.Get());

            Session.Journal.ClearXData();
        }

        public Value DisplayValue (Value val)
        {
            Value temp = val.StripAnnotations(WhatToKeep());
            if (BaseHandler.Handled)
                return temp;
            else
                return temp.Unreduced();
        }

        public static Value TopAmount(Value val)
        {
            if (val.Type == ValueTypeEnum.Balance)
                return Value.Get(val.AsBalance.Amounts.Values.First());

            if (val.Type == ValueTypeEnum.Sequence)
                return TopAmount(val.AsSequence.First());

            return val;
        }

        public Value FnTopAmount(CallScope args)
        {
            return TopAmount(args[0]);
        }

        public Value FnAmountExpr(CallScope scope)
        {
            return AmountHandler.Expr.Calc(scope);
        }

        public Value FnTotalExpr(CallScope scope)
        {
            return TotalHandler.Expr.Calc(scope);
        }

        public Value FnDisplayAmount(CallScope scope)
        {
            return DisplayAmountHandler.Expr.Calc(scope);
        }

        public Value FnDisplayTotal(CallScope scope)
        {
            return DisplayTotalHandler.Expr.Calc(scope);
        }

        public Value FnShouldBold(CallScope scope)
        {
            if (BoldIfHandler.Handled)
                return BoldIfHandler.Expr.Calc(scope);
            else
                return Value.False;
        }

        public Value FnAveragedLots(CallScope args)
        {
            if (args.Has<Balance>(0))
                return Value.Get(Balance.AverageLotPrices(args.Get<Balance>(0)));
            else
                return args[0];
        }

        public Value FnMarket(CallScope args)
        {
            Value result;
            Value arg0 = args[0];

            DateTime moment = default(DateTime);
            if (args.Has<DateTime>(1))
                moment = args.Get<DateTime>(1);

            if (arg0.Type == ValueTypeEnum.String)
            {
                Amount tmp = new Amount(1, CommodityPool.Current.FindOrCreate(arg0.AsString));
                arg0 = Value.Get(tmp);
            }

            string targetCommodity = null;
            if (args.Has<string>(2))
                targetCommodity = args.Get<string>(2);

            if (!String.IsNullOrEmpty(targetCommodity))
                result = arg0.ExchangeCommodities(targetCommodity, /* add_prices= */ false, moment);
            else
                result = arg0.ValueOf(moment);

            return !Value.IsNullOrEmpty(result) ? result : arg0;
        }

        public Value FnGetAt(CallScope args)
        {
            int index = args.Get<int>(1);
            if (index == 0)
            {
                if (args[0].Type != ValueTypeEnum.Sequence)
                    return args[0];
            }
            else if (args[0].Type != ValueTypeEnum.Sequence)
            {
                throw new RuntimeError(String.Format(RuntimeError.ErrorMessageAttemptingToGetArgumentAtIndexSmthFromSmth, index, args[0]));
            }

            var seq = args[0].AsSequence;
            if (index >= seq.Count)
                throw new RuntimeError(String.Format(RuntimeError.ErrorMessageAttemptingToGetIndexSmthFromSmthWithSmthElements, index, args[0], seq.Count));

            return seq[index];
        }

        public Value FnIsSeq(CallScope scope)
        {
            return Value.Get(scope.Value().Type == ValueTypeEnum.Sequence);
        }

        public Value FnStrip(CallScope args)
        {
            return args.Value().StripAnnotations(WhatToKeep());
        }

        public Value FnTrim(CallScope args)
        {
            string temp = args.Value().ToString().Trim();
            return Value.Get(temp);            
        }

        public Value FnFormat(CallScope args)
        {
            Format format = new Format(args.Get<string>(0));
            string stringValue = format.Calc(args);
            return Value.StringValue(stringValue);            
        }

        public Value FnPrint(CallScope args)
        {
            for (int i = 0; i<args.Size; i++)
                OutputStream.Write(args[i].Print());
            OutputStream.WriteLine();
            return Value.True;
        }

        public Value FnScrub(CallScope args)
        {
            return DisplayValue(args.Value());
        }

        public Value FnRounded(CallScope args)
        {
            return args.Value().Rounded();
        }

        public Value FnUnrounded(CallScope args)
        {
            return args.Value().Unrounded();
        }

        public Value FnQuantity(CallScope args)
        {
            return Value.Get(args.Get<Amount>(0).Number());
        }

        public Value FnFloor(CallScope args)
        {
            return args[0].Floored();
        }

        public Value FnCeiling(CallScope args)
        {
            return args[0].Ceilinged();
        }

        public Value FnRound(CallScope args)
        {
            return args[0].Rounded();
        }

        public Value FnRoundTo(CallScope args)
        {
            return args[0].RoundTo(args.Get<int>(0));
        }

        public Value FnUnround(CallScope args)
        {
            return args[0].Unrounded();
        }

        public Value FnAbs(CallScope args)
        {
            return args[0].Abs();
        }

        public Value FnTruncated(CallScope args)
        {
            return Value.StringValue(Format.Truncate(args.Get<string>(0),
                args.Has<int>(1) && args.Get<int>(1) > 0 ? args.Get<int>(1) : 0,
                args.Has<int>(2) ? args.Get<int>(2) : 0));
        }

        public Value FnJustify(CallScope args)
        {
            AmountPrintEnum flags = AmountPrintEnum.AMOUNT_PRINT_ELIDE_COMMODITY_QUOTES;

            if (args.Has<bool>(3) && args.Get<bool>(3))
                flags |= AmountPrintEnum.AMOUNT_PRINT_RIGHT_JUSTIFY;
            if (args.Has<bool>(4) && args.Get<bool>(4))
                flags |= AmountPrintEnum.AMOUNT_PRINT_COLORIZE;

            string outStr = args[0].Print(args.Get<int>(1), args.Has<int>(2) ? args.Get<int>(2) : -1, flags);

            return Value.StringValue(outStr);
        }

        public Value FnQuoted(CallScope args)
        {
            string strOut = string.Format("\"{0}\"", args.Get<string>(0)?.Replace("\"", "\\\\"));
            return Value.StringValue(strOut);
        }

        public Value FnQuotedRfc4180(CallScope args)
        {
            var sb = new StringBuilder();

            sb.Append('"');
            string arg = args.Get<string>(0);
            foreach (char ch in arg)
            {
                if (ch == '"')
                    sb.Append("\"\"");
                else
                    sb.Append(ch);
            }
            sb.Append('"');

            return Value.StringValue(sb.ToString());
        }

        public Value FnJoin(CallScope args)
        {
            string strOut = args.Get<string>(0)?.Replace("\n", "\\n");
            return Value.StringValue(strOut);
        }

        public Value FnFormatDate(CallScope args)
        {
            if (args.Has<string>(1))
                return Value.StringValue(TimesCommon.Current.FormatDate(args.Get<Date>(0), FormatTypeEnum.FMT_CUSTOM, args.Get<string>(1)));
            else
                return Value.StringValue(TimesCommon.Current.FormatDate(args.Get<Date>(0), FormatTypeEnum.FMT_PRINTED));
        }

        public Value FnFormatDateTime(CallScope args)
        {
            if (args.Has<string>(1))
                return Value.StringValue(TimesCommon.Current.FormatDateTime(args.Get<DateTime>(0), FormatTypeEnum.FMT_CUSTOM, args.Get<string>(1)));
            else
                return Value.StringValue(TimesCommon.Current.FormatDateTime(args.Get<DateTime>(0), FormatTypeEnum.FMT_PRINTED));
        }

        public Value FnAnsifyIf(CallScope args)
        {
            if (args.Has<string>(1))
            {
                string color = args.Get<string>(1);
                StringBuilder sb = new StringBuilder();
                if (color == "black") sb.Append(AnsiTextWriter.ForegroundColorBlack);
                else if (color == "red") sb.Append(AnsiTextWriter.ForegroundColorRed);
                else if (color == "green") sb.Append(AnsiTextWriter.ForegroundColorGreen);
                else if (color == "yellow") sb.Append(AnsiTextWriter.ForegroundColorYellow);
                else if (color == "blue") sb.Append(AnsiTextWriter.ForegroundColorBlue);
                else if (color == "magenta") sb.Append(AnsiTextWriter.ForegroundColorMagenta);
                else if (color == "cyan") sb.Append(AnsiTextWriter.ForegroundColorCyan);
                else if (color == "white") sb.Append(AnsiTextWriter.ForegroundColorWhite);
                else if (color == "bold") sb.Append(AnsiTextWriter.ForegroundBold);
                else if (color == "underline") sb.Append(AnsiTextWriter.ForegroundBold);
                else if (color == "blink") sb.Append(AnsiTextWriter.ForegroundBlink);
                sb.Append(args[0]);
                sb.Append(AnsiTextWriter.NormalColor);
                return Value.StringValue(sb.ToString());
            }
            return args[0];
        }

        public Value FnPercent(CallScope args)
        {
            return Value.Get(new Amount("100.00%") * (args.Get<Amount>(0) / args.Get<Amount>(1)).Number());
        }

        public Value FnCommodity(CallScope args)
        {
            return Value.Get(args.Get<Amount>(0).Commodity.Symbol);
        }

        public Value FnClearCommodity(CallScope args)
        {
            Amount amt = args.Get<Amount>(0);
            amt.ClearCommodity();
            return Value.Get(amt);
        }

        /// <summary>
        /// Ported from value_t report_t::fn_nail_down(call_scope_t& args)
        /// </summary>
        public Value FnNailDown(CallScope args)
        {
            Value arg0 = args[0];
            Value arg1 = args[1];

            switch(arg0.Type)
            {
                case ValueTypeEnum.Amount: 
                    {
                        Amount tmp = arg0.AsAmount;
                        if (tmp.HasCommodity && !tmp.IsEmpty && !tmp.IsRealZero)
                        {
                            arg1 = Value.Get(arg1.StripAnnotations(new AnnotationKeepDetails()).AsAmount);
                            Expr valueExpr = new Expr(Expr.IsExpr(arg1) ? Expr.AsExpr(arg1) : ExprOp.WrapValue(arg1.Unrounded() / arg0.Number()));

                            string buf = valueExpr.Print();
                            valueExpr.Text = buf;

                            tmp.SetCommodity(tmp.Commodity.NailDown(valueExpr));
                        }
                        return Value.Get(tmp);
                    }

                case ValueTypeEnum.Balance:
                    {
                        Balance tmp = new Balance();
                        foreach(Amount amount in arg0.AsBalance.Amounts.Values)
                        {
                            CallScope innerArgs = new CallScope(args.Parent);
                            innerArgs.PushBack(Value.Get(amount));
                            innerArgs.PushBack(arg1);
                            tmp = tmp.Add(FnNailDown(innerArgs).AsAmount);
                        }
                        return Value.Get(tmp);
                    }

                case ValueTypeEnum.Sequence:
                    {
                        Value tmp = new Value();
                        foreach(Value value in arg0.AsSequence)
                        {
                            CallScope innerArgs = new CallScope(args.Parent);
                            innerArgs.PushBack(value);
                            innerArgs.PushBack(arg1);
                            tmp.PushBack(FnNailDown(innerArgs));
                        }
                        return tmp;
                    }

                default:
                    throw new RuntimeError(String.Format(RuntimeError.ErrorMessageAttemptingToNailDownSmth, args[0]));
            }            
        }

        public Value FnLotDate(CallScope args)
        {
            if (args[0].HasAnnotation)
            {
                Annotation details = args[0].Annotation;
                if (details.Date.HasValue)
                    return Value.Get(details.Date.Value);
            }
            return Value.Empty;
        }

        public Value FnLotPrice(CallScope args)
        {
            if (args[0].HasAnnotation)
            {
                Annotation details = args[0].Annotation;
                if (details.Price != null)
                    return Value.Get(details.Price);
            }
            return Value.Empty;
        }

        public Value FnLotTag(CallScope args)
        {
            if (args[0].HasAnnotation)
            {
                Annotation details = args[0].Annotation;
                if (!String.IsNullOrEmpty(details.Tag))
                    return Value.StringValue(details.Tag);
            }
            return Value.Empty;
        }

        public Value FnToBoolean(CallScope args)
        {
            return Value.Get(args.Get<bool>(0));
        }

        public Value FnToInt(CallScope args)
        {
            // This method is not called fn_to_long, because that would be
            // confusing to users who don't care about the distinction between
            // integer and long.
            return Value.Get(args.Get<long>(0));
        }

        public Value FnToDateTime(CallScope args)
        {
            return Value.Get(args.Get<DateTime>(0));
        }

        public Value FnToDate(CallScope args)
        {
            return Value.Get(args.Get<Date>(0));
        }

        public Value FnToAmount(CallScope args)
        {
            return Value.Get(args.Get<Amount>(0));
        }

        public Value FnToBalance(CallScope args)
        {
            return Value.Get(args.Get<Balance>(0));
        }

        public Value FnToString(CallScope args)
        {
            return Value.StringValue(args.Get<string>(0));
        }

        public Value FnToMask(CallScope args)
        {
            return Value.Get(args.Get<Mask>(0));
        }

        public Value FnToSequence(CallScope args)
        {
            return Value.Get(args[0].AsSequence);
        }

        public static Value FnBlack(CallScope args)
        {
            return Value.StringValue("black");
        }

        public static Value FnBlink(CallScope args)
        {
            return Value.StringValue("blink");
        }

        public static Value FnBlue(CallScope args)
        {
            return Value.StringValue("blue");
        }

        public static Value FnBold(CallScope args)
        {
            return Value.StringValue("bold");
        }

        public static Value FnCyan(CallScope args)
        {
            return Value.StringValue("cyan");
        }

        public static Value FnGreen(CallScope args)
        {
            return Value.StringValue("green");
        }

        public static Value FnMagenta(CallScope args)
        {
            return Value.StringValue("magenta");
        }

        public static Value FnRed(CallScope args)
        {
            return Value.StringValue("red");
        }

        public static Value FnUnderline(CallScope args)
        {
            return Value.StringValue("underline");
        }

        public static Value FnWhite(CallScope args)
        {
            return Value.StringValue("white");
        }

        public static Value FnYellow(CallScope args)
        {
            return Value.StringValue("yellow");
        }

        public static Value FnFalse(CallScope args)
        {
            return Value.False;
        }

        public static Value FnNull(CallScope args)
        {
            return Value.Empty;
        }

        public Value FnNow(CallScope args)
        {
            return Value.Get(Terminus);
        }

        public Value FnToday(CallScope args)
        {
            return Value.Get((Date)Terminus.Date);
        }

        public Value FnOptions(CallScope args)
        {
            return Value.ScopeValue(this);
        }

        public string ReportFormat(Option option)
        {
            if (FormatHandler.Handled)
                return FormatHandler.Str();
            return option.Str();
        }

        public string MaybeFormat(Option option)
        {
            if (Option.IsNotNullAndHandled(option))
                return option.Str();
            return String.Empty;
        }

        public Value FnRuntimeError(CallScope args)
        {
            throw new RuntimeError(String.Format(RuntimeError.ErrorMessageTheSmthValueExpressionVariableIsNoLongerSupported, args[0]));
        }

        public Value ReloadCommand(CallScope args)
        {
            Session.CloseJournalFiles();
            Session.ReadJournalFiles();
            return Value.True;
        }

        // stats.h - report_statistics
        public static Value ReportStatistics(CallScope args)
        {
            Report report = args.FindScope<Report>();
            StringBuilder sb = new StringBuilder();

            AccountXDataDetails statistics = report.Session.Journal.Master.FamilyDetails(true);

            if (statistics.EarliestPost == null || statistics.LatestPost == null)
                return Value.Empty;

            sb.AppendFormat("Time period: {0} to {1} ({2} days)",
                TimesCommon.Current.FormatDate(statistics.EarliestPost),
                TimesCommon.Current.FormatDate(statistics.LatestPost),
                (statistics.LatestPost - statistics.EarliestPost).Days);
            sb.AppendLine();
            sb.AppendLine();

            sb.AppendLine("  Files these postings came from:");

            foreach (string fileName in statistics.Filenames.Where(s => !String.IsNullOrEmpty(s)))
                sb.AppendLine("    " + fileName);
            sb.AppendLine();

            sb.AppendFormat("  Unique payees:          {0,6}", statistics.PayeesReferenced.Count());
            sb.AppendLine();

            sb.AppendFormat("  Unique accounts:        {0,6}", statistics.AccountsReferenced.Count());
            sb.AppendLine();

            sb.AppendLine();

            sb.AppendFormat("  Number of postings:     {0,6}", statistics.PostsCount);

            sb.AppendFormat(" ({0:0.00} per day)", (double)statistics.PostsCount / (statistics.LatestPost - statistics.EarliestPost).Days);
            sb.AppendLine();
            sb.AppendFormat("  Uncleared postings:     {0,6}", statistics.PostsCount - statistics.PostsClearedCount);
            sb.AppendLine();

            sb.AppendLine();

            sb.AppendFormat("  Days since last post:   {0,6}", (TimesCommon.Current.CurrentDate - statistics.LatestPost).Days);
            sb.AppendLine();

            sb.AppendFormat("  Posts in last 7 days:   {0,6}", statistics.PostsLast7Count);
            sb.AppendLine();
            sb.AppendFormat("  Posts in last 30 days:  {0,6}", statistics.PostsLast30Count);
            sb.AppendLine();
            sb.AppendFormat("  Posts seen this month:  {0,6}", statistics.PostsThisMountCount);
            sb.AppendLine();

            report.OutputStream.Write(sb.ToString());
            report.OutputStream.Flush();

            return Value.Empty;
        }

        public Value EchoCommand(CallScope args)
        {
            OutputStream.WriteLine(args.Get<string>(0));
            return Value.True;
        }

        public Value PricemapCommand(CallScope args)
        {
            string buf = CommodityPool.Current.CommodityPriceHistory.PrintMap(args.Has<string>(0) ? TimesCommon.Current.ParseDate(args.Get<string>(0)) : default(DateTime));
            OutputStream.Write(buf);
            return Value.True;
        }

        public void XactReport(PostHandler handler, Xact xact)
        {
            handler = ChainCommon.ChainHandlers(handler, this);

            XactPostsIterator walker = new XactPostsIterator(xact);
            new PassDownPosts(handler, walker.Get());

            xact.ClearXData();
        }

        public AnnotationKeepDetails WhatToKeep()
        {
            bool lots = LotsHandler.Handled || LotsActualHandler.Handled;
            return new AnnotationKeepDetails()
            {
                KeepPrice = lots || LotPricesHandler.Handled,
                KeepDate = lots || LotDatesHandler.Handled,
                KeepTag = lots || LotNotesHandler.Handled,
                OnlyActuals = LotsActualHandler.Handled
            };
        }

        private void CreateOptions()
        {
            #region Create Options

            AbbrevLenHandler = Options.Add(new Option(OptionAbbrevLen));
            AbbrevLenHandler.On(null, "2");

            AccountHandler = Options.Add(new Option(OptionAccount));
            ActualHandler = Options.Add(new Option(OptionActual, (o, w) => LimitHandler.On(w, "actual")));
            AddBudgetHandler = Options.Add(new Option(OptionAddBudget, (o, w) => BudgetFlags = BudgetFlags | ReportBudgetFlags.BUDGET_BUDGETED | ReportBudgetFlags.BUDGET_UNBUDGETED ));
            AmountHandler = Options.Add(new MergedExprOption("amount_expr", "amount", OptionAmount, (o, w, s) => ((MergedExprOption)o).Expr.Append(s)));
            AmountDataHandler = Options.Add(new Option(OptionAmountData));
            AnonHandler = Options.Add(new Option(OptionAnon));
            AutoMatchHandler = Options.Add(new Option(OptionAutoMatch));
            AverageHandler = Options.Add(new Option(OptionAverage, (o, w) =>
                {
                    EmptyHandler.On(w);
                    DisplayTotalHandler.On(w, "count>0?(display_total/count):0");
                }));

            BalanceFormatHandler = Options.Add(new Option(OptionBalanceFormat));
            BalanceFormatHandler.On(null,
               "%(ansify_if(" +
               "  justify(scrub(display_total), 20," +
               "          20 + int(prepend_width), true, color)," +
               "            bold if should_bold))" +
               "  %(!options.flat ? depth_spacer : \"\")" +
               "%-(ansify_if(" +
               "   ansify_if(partial_account(options.flat), blue if color)," +
               "             bold if should_bold))\n%/" +
               "%$1\n%/" +
               "%(prepend_width ? \" \" * int(prepend_width) : \"\")" +
               "--------------------\n");
            
            BaseHandler = Options.Add(new Option(OptionBase));
            BasisHandler = Options.Add(new Option(OptionBasis, (o, w) =>
                {
                    RevaluedHandler.On(w);
                    AmountHandler.Expr.BaseExpr = "rounded(cost)";
                }));
            BeginHandler = Options.Add(new Option(OptionBegin, (o, w, s) =>
            {
                DateInterval interval = new DateInterval(s);
                Date? begin = interval.Begin;
                if (begin.HasValue)
                {
                    string predicate = "date>=[" + TimesCommon.ToIsoExtendedString(begin.Value) + "]";
                    LimitHandler.On(w, predicate);
                }
                else
                {
                    throw new ArgumentException(String.Format("Could not determine beginning of period '{0}'"), s);
                }
            }));
            
            BoldIfHandler = Options.Add(new ExprOption(OptionBoldIf, (o, w, s) => ((ExprOption)o).Expr = new Expr(s)));
            
            BudgetHandler = Options.Add(new Option(OptionBudget, (o, w) => BudgetFlags = BudgetFlags | ReportBudgetFlags.BUDGET_BUDGETED));

            BudgedFormatHandler = Options.Add(new Option(OptionBudgetFormat));
            BudgedFormatHandler.On(null,
               "%(justify(scrub(get_at(display_total, 0)), int(amount_width), -1, true, color))" +
               " %(justify(-scrub(get_at(display_total, 1)), int(amount_width), " +
               "           int(amount_width) + 1 + int(amount_width), true, color))" +
               " %(justify(scrub((get_at(display_total, 1) || 0) + " +
               "                 (get_at(display_total, 0) || 0)), int(amount_width), " +
               "           int(amount_width) + 1 + int(amount_width) + 1 + int(amount_width), true, color))" +
               " %(ansify_if(" +
               "   justify((get_at(display_total, 1) ? " +
               "            (100% * quantity(scrub(get_at(display_total, 0)))) / " +
               "             -quantity(scrub(get_at(display_total, 1))) : 0), " +
               "           5, -1, true, false)," +
               "   magenta if (color and get_at(display_total, 1) and " +
               "               (abs(quantity(scrub(get_at(display_total, 0))) / " +
               "                    quantity(scrub(get_at(display_total, 1)))) >= 1))))" +
               "  %(!options.flat ? depth_spacer : \"\")" +
               "%-(ansify_if(partial_account(options.flat), blue if color))\n" +
               "%/%$1 %$2 %$3 %$4\n%/" +
               "%(prepend_width ? \" \" * int(prepend_width) : \"\")" +
               "------------ ------------ ------------ -----\n");

            ByPayeeHandler = Options.Add(new Option(OptionByPayee));

            ClearedHandler = Options.Add(new Option(OptionCleared, (o, w) =>
                {
                    LimitHandler.On(w, "cleared");
                }));

            ClearedFormatHandler = Options.Add(new Option(OptionClearedFormat));
            ClearedFormatHandler.On(null,
               "%(justify(scrub(get_at(display_total, 0)), 16, 16 + int(prepend_width), " +
               " true, color))  %(justify(scrub(get_at(display_total, 1)), 18, " +
               " 36 + int(prepend_width), true, color))" +
               "    %(latest_cleared ? format_date(latest_cleared) : \"         \")" +
               "    %(!options.flat ? depth_spacer : \"\")" +
               "%-(ansify_if(partial_account(options.flat), blue if color))\n%/" +
               "%$1  %$2    %$3\n%/" +
               "%(prepend_width ? \" \" * int(prepend_width) : \"\")" +
               "----------------    ----------------    ---------\n");

            ColorHandler = Options.Add(new Option(OptionColor));

            CollapseHandler = Options.Add(new Option(OptionCollapse, (o, w) =>
                {
                      // Make sure that balance reports are collapsed too, but only apply it
                      // to account xacts
                    DisplayHandler.On(w, "post|depth<=1");
                }));

            CollapseIfZeroHandler = Options.Add(new Option(OptionCollapseIfZero, (o, w) =>
                {
                    CollapseHandler.On(w);
                }));

            ColumnsHandler = Options.Add(new Option(OptionColumns));
            CountHandler = Options.Add(new Option(OptionCount));
            
            CsvFormatHandler = Options.Add(new Option(OptionCsvFormat));
            CsvFormatHandler.On(null,
               "%(quoted(date))," +
               "%(quoted(code))," +
               "%(quoted(payee))," +
               "%(quoted(display_account))," +
               "%(quoted(commodity(scrub(display_amount))))," +
               "%(quoted(quantity(scrub(display_amount))))," +
               "%(quoted(cleared ? \"*\" : (pending ? \"!\" : \"\")))," +
               "%(quoted(join(note | xact.note)))\n");

            CurrentHandler = Options.Add(new Option(OptionCurrent, (o, w) =>
                {
                    LimitHandler.On(w, "date<=today");
                }));

            DailyHandler = Options.Add(new Option(OptionDaily, (o, w) =>
                {
                    PeriodHandler.On(w, "daily");
                }));

            DateHandler = Options.Add(new Option(OptionDate));
            DateFormatHandler = Options.Add(new Option(OptionDateFormat));
            DateTimeFormatHandler = Options.Add(new Option(OptionDateTimeFormat));

            DcHandler = Options.Add(new Option(OptionDc, (o, w) =>
                {
                    AmountHandler.Expr.BaseExpr = "(amount > 0 ? amount : 0, amount < 0 ? amount : 0)";
                    RegisterFormatHandler.On(null,
                        "%(ansify_if(" +
                        "  ansify_if(justify(format_date(date), int(date_width))," +
                        "            green if color and date > today)," +
                        "            bold if should_bold))" +
                        " %(ansify_if(" +
                        "   ansify_if(justify(truncated(payee, int(payee_width)), int(payee_width)), " +
                        "             bold if color and !cleared and actual)," +
                        "             bold if should_bold))" +
                        " %(ansify_if(" +
                        "   ansify_if(justify(truncated(display_account, int(account_width), " +
                        "                               int(abbrev_len)), int(account_width))," +
                        "             blue if color)," +
                        "             bold if should_bold))" +
                        " %(ansify_if(" +
                        "   justify(scrub(abs(get_at(display_amount, 0))), int(amount_width), " +
                        "           3 + int(meta_width) + int(date_width) + int(payee_width)" +
                        "             + int(account_width) + int(amount_width) + int(prepend_width)," +
                        "           true, color)," +
                        "           bold if should_bold))" +
                        " %(ansify_if(" +
                        "   justify(scrub(abs(get_at(display_amount, 1))), int(amount_width), " +
                        "           4 + int(meta_width) + int(date_width) + int(payee_width)" +
                        "             + int(account_width) + int(amount_width) + int(amount_width) + int(prepend_width)," +
                        "           true, color)," +
                        "           bold if should_bold))" +
                        " %(ansify_if(" +
                        "   justify(scrub(get_at(display_total, 0) + get_at(display_total, 1)), int(total_width), " +
                        "           5 + int(meta_width) + int(date_width) + int(payee_width)" +
                        "             + int(account_width) + int(amount_width) + int(amount_width) + int(total_width)" +
                        "             + int(prepend_width), true, color)," +
                        "           bold if should_bold))\n%/" +
                        "%(justify(\" \", int(date_width)))" +
                        " %(ansify_if(" +
                        "   justify(truncated(has_tag(\"Payee\") ? payee : \" \", " +
                        "                     int(payee_width)), int(payee_width))," +
                        "             bold if should_bold))" +
                        " %$3 %$4 %$5 %$6\n");
                    BalanceFormatHandler.On(null,
                        "%(ansify_if(" +
                        "  justify(scrub(abs(get_at(display_total, 0))), 14," +
                        "          14 + int(prepend_width), true, color)," +
                        "            bold if should_bold)) " +
                        "%(ansify_if(" +
                        "  justify(scrub(abs(get_at(display_total, 1))), 14," +
                        "          14 + 1 + int(prepend_width) + int(total_width), true, color)," +
                        "            bold if should_bold)) " +
                        "%(ansify_if(" +
                        "  justify(scrub(get_at(display_total, 0) + get_at(display_total, 1)), 14," +
                        "          14 + 2 + int(prepend_width) + int(total_width) + int(total_width), true, color)," +
                        "            bold if should_bold))" +
                        "  %(!options.flat ? depth_spacer : \"\")" +
                        "%-(ansify_if(" +
                        "   ansify_if(partial_account(options.flat), blue if color)," +
                        "             bold if should_bold))\n%/" +
                        "%$1 %$2 %$3\n%/"+
                        "%(prepend_width ? \" \" * int(prepend_width) : \"\")" +
                        "--------------------------------------------\n");
                }));

            DepthHandler = Options.Add(new Option(OptionDepth, (o, w, s) =>
                {
                    DisplayHandler.On(w, "depth<=" +  s);
                }));

            DeviationHandler = Options.Add(new Option(OptionDeviation, (o, w) =>
                {
                    DisplayTotalHandler.On(w, "display_amount-display_total");
                }));

            DisplayHandler = Options.Add(new Option(OptionDisplay, (o, w, s) =>
                {
                     if (o.Handled)
                         o.Value = String.Format("({0})&({1})", o.Value, s);
                }));

            DisplayAmountHandler = Options.Add(new MergedExprOption("display_amount", "amount_expr", OptionDisplayAmount,  (o, w, s) => ((MergedExprOption)o).Expr.Append(s)));
            DisplayTotalHandler = Options.Add(new MergedExprOption("display_total", "total_expr", OptionDisplayTotal,  (o, w, s) => ((MergedExprOption)o).Expr.Append(s)));

            DowHandler = Options.Add(new Option(OptionDow));
            AuxDateHandler = Options.Add(new Option(OptionAuxDate));
            EmptyHandler = Options.Add(new Option(OptionEmpty));

            EndHandler = Options.Add(new Option(OptionEnd, (o, w, s) =>
                {
                    // Use begin() here so that if the user says --end=2008, we end on
                    // 2008/01/01 instead of 2009/01/01 (which is what end() would
                    // return).
                    DateInterval interval = new DateInterval(s);
                    Date? end = interval.Begin;
                    if (end.HasValue)
                    {
                        string predicate = "date<[" + TimesCommon.ToIsoExtendedString(end.Value) + "]";
                        LimitHandler.On(w, predicate);

                        Terminus = end.Value;
                    }
                    else
                    {
                        throw new ArgumentException(String.Format("Could not determine end of period '{0}'", s));
                    }
                }));

            EquityHandler = Options.Add(new Option(OptionEquity));
            ExactHandler = Options.Add(new Option(OptionExact));

            ExchangeHandler = Options.Add(new Option(OptionExchange, (o, w, s) =>
            {
                // Using -X implies -V.  The main difference is that now
                // HANDLER(exchange_) contains the name of a commodity, which
                // is accessed via the "exchange" value expression function.
                MarketHandler.On(w);
            }));

            FlatHandler = Options.Add(new Option(OptionFlat));
            ForceColorHandler = Options.Add(new Option(OptionForceColor));
            ForcePagerHandler = Options.Add(new Option(OptionForcePager));
            ForecastWhileHandler = Options.Add(new Option(OptionForecastWhile));
            ForecastYearsHandler = Options.Add(new Option(OptionForecastYears));
            FormatHandler = Options.Add(new Option(OptionFormat));

            GainHandler = Options.Add(new Option(OptionGain, (o, w) =>
            {
                RevaluedHandler.On(w);
                AmountHandler.Expr.BaseExpr = "(amount, cost)";

                // Since we are displaying the amounts of revalued postings, they
                // will end up being composite totals, and hence a pair of pairs.
                DisplayAmountHandler.On(w,
                    "use_direct_amount ? amount :" +
                    " (is_seq(get_at(amount_expr, 0)) ?" +
                    "  get_at(get_at(amount_expr, 0), 0) :" +
                    "  market(get_at(amount_expr, 0), value_date, exchange)" +
                    "  - get_at(amount_expr, 1))");
                RevaluedTotalHandler.On(w,
                    "(market(get_at(total_expr, 0), value_date, exchange), " +
                    "get_at(total_expr, 1))");
                DisplayTotalHandler.On(w,
                    "use_direct_amount ? total_expr :" +
                    " market(get_at(total_expr, 0), value_date, exchange)" +
                    " - get_at(total_expr, 1)");
            }));

            GeneratedHandler = Options.Add(new Option(OptionGenerated));

            GroupByHandler = Options.Add(new ExprOption(OptionGroupBy, (o, w, s) => ((ExprOption)o).Expr = new Expr(s)));
            
            GroupTitleFormatHandler = Options.Add(new Option(OptionGroupTitleFormat));
            GroupTitleFormatHandler.On(null, "%(value)\n");

            HeadHandler = Options.Add(new Option(OptionHead));

            HistoricalHandler = Options.Add(new Option(OptionHistorical, (o, w) =>
            {
                MarketHandler.On(w);
                AmountHandler.On(w, "nail_down(amount_expr, market(amount_expr, value_date, exchange))");
            }));

            ImmediateHandler = Options.Add(new Option(OptionImmediate));
            InjectHandler = Options.Add(new Option(OptionInject));

            InvertHandler = Options.Add(new Option(OptionInvert, (o, w) =>
            {
                DisplayAmountHandler.On(w, "-display_amount");
                DisplayTotalHandler.On(w, "-display_total");
            }));

            LimitHandler = Options.Add(new Option(OptionLimit, (o, w, s) =>
            {
                if (o.Handled)
                    o.Value = String.Format("({0})&({1})", o.Value, s);
            }));

            LotDatesHandler = Options.Add(new Option(OptionLotDates));
            LotPricesHandler = Options.Add(new Option(OptionLotPrices));
            AverageLotPricesHandler = Options.Add(new Option(OptionAverageLotPrices, (o, w) =>
            {
                LotPricesHandler.On(w);
                DisplayAmountHandler.On(w, "averaged_lots(display_amount)");
                DisplayTotalHandler.On(w, "averaged_lots(display_total)");
            }));
            LotNotesHandler = Options.Add(new Option(OptionLotNotes));
            LotsHandler = Options.Add(new Option(OptionLots));
            LotsActualHandler = Options.Add(new Option(OptionLotsActual));

            MarketHandler = Options.Add(new Option(OptionMarket, (o, w) =>
            {
                RevaluedHandler.On(w);

                DisplayAmountHandler.On(w,"market(display_amount, value_date, exchange)");
                DisplayTotalHandler.On(w,"market(display_total, value_date, exchange)");
            }));

            MetaHandler = Options.Add(new Option(OptionMeta));

            MonthlyHandler = Options.Add(new Option(OptionMonthly, (o, w) =>
            {
                PeriodHandler.On(w, "monthly");
            }));

            NoColorHandler = Options.Add(new Option(OptionNoColor, (o, w) =>
            {
                ColorHandler.Off();
            }));

            NoRevaluedHandler = Options.Add(new Option(OptionNoRevalued, (o, w) =>
            {
                RevaluedHandler.Off();
            }));

            NoRoundingHandler = Options.Add(new Option(OptionNoRounding));
            NoTitlesHandler = Options.Add(new Option(OptionNoTitles));
            NoTotalHandler = Options.Add(new Option(OptionNoTotal));

            NowHandler = Options.Add(new Option(OptionNow, (o, w, s) =>
            {
                DateInterval interval = new DateInterval(s);
                Date? begin = interval.Begin;
                if (begin.HasValue)
                    TimesCommon.Current.Epoch = Terminus = begin.Value;
                else
                    throw new ArgumentException(String.Format("Could not determine beginning of period '{0}'"), s);
            }));

            OnlyHandler = Options.Add(new Option(OptionOnly, (o, w, s) =>
            {
                if (o.Handled)
                    o.Value = String.Format("({0})&({1})", o.Value, s);
            }));

            OutputHandler = Options.Add(new Option(OptionOutput));

            // setenv() is not available on WIN32
            PagerHandler = Options.Add(new Option(OptionPager));
            var defaultPagerPath = VirtualPager.GetDefaultPagerPath();
            if (!String.IsNullOrEmpty(defaultPagerPath) && VirtualConsole.IsAtty())
                PagerHandler.On(null, defaultPagerPath);

            NoPagerHandler = Options.Add(new Option(OptionNoPager, (o, w) =>
            {
                PagerHandler.Off();
            }));

            PayeeHandler = Options.Add(new Option(OptionPayee));

            PendingHandler = Options.Add(new Option(OptionPending, (o, w) =>
            {
                LimitHandler.On(w, "pending");
            }));

            PercentHandler = Options.Add(new Option(OptionPercent, (o, w) =>
            {
                TotalHandler.On(w, "((is_account&parent&parent.total)?  percent(scrub(total), scrub(parent.total)):0)");
            }));

            PeriodHandler = Options.Add(new Option(OptionPeriod, (o, w, s) =>
            {
                if (o.Handled)
                    o.Value += " " + s;
            }));

            PivotHandler = Options.Add(new Option(OptionPivot));

            PlotAmountFormatHandler = Options.Add(new Option(OptionPlotAmountFormat));
            PlotAmountFormatHandler.On(null, "%(format_date(date, \"%Y-%m-%d\")) %(quantity(scrub(display_amount)))\n");

            PlotTotalFormatHandler = Options.Add(new Option(OptionPlotTotalFormat));
            PlotTotalFormatHandler.On(null, "%(format_date(date, \"%Y-%m-%d\")) %(quantity(scrub(display_total)))\n");

            PrependFormatHandler = Options.Add(new Option(OptionPrependFormat));
            PrependWidthHandler = Options.Add(new Option(OptionPrependWidth));

            PriceHandler = Options.Add(new Option(OptionPrice, (o, w) =>
            {
                AmountHandler.Expr.BaseExpr = "price";
            }));

            PricesFormatHandler = Options.Add(new Option(OptionPricesFormat));
            PricesFormatHandler.On(null, "%(date) %-8(display_account) %(justify(scrub(display_amount), 12,     2 + 9 + 8 + 12, true, color))\n");

            PriceDbFormatHandler = Options.Add(new Option(OptionPriceDbFormat));
            PriceDbFormatHandler.On(null, "P %(datetime) %(display_account) %(scrub(display_amount))\n");

            PrimaryDateHandler = Options.Add(new Option(OptionPrimaryDate));

            QuantityHandler = Options.Add(new Option(OptionQuantity, (o, w) =>
            {
                RevaluedHandler.Off();

                AmountHandler.Expr.BaseExpr = "amount";
                TotalHandler.Expr.BaseExpr = "total";
            }));

            QuarterlyHandler = Options.Add(new Option(OptionQuarterly, (o, w) =>
            {
                PeriodHandler.On(w, "quarterly");
            }));

            RawHandler = Options.Add(new Option(OptionRaw));

            RealHandler = Options.Add(new Option(OptionReal, (o, w) =>
            {
                LimitHandler.On(w, "real");
            }));

            RegisterFormatHandler = Options.Add(new Option(OptionRegisterFormat));
            RegisterFormatHandler.On(null, 
               "%(ansify_if("+
               "  ansify_if(justify(format_date(date), int(date_width)),"+
               "            green if color and date > today),"+
               "            bold if should_bold))"+
               " %(ansify_if("+
               "   ansify_if(justify(truncated(payee, int(payee_width)), int(payee_width)), "+
               "             bold if color and !cleared and actual),"+
               "             bold if should_bold))"+
               " %(ansify_if("+
               "   ansify_if(justify(truncated(display_account, int(account_width), "+
               "                               int(abbrev_len)), int(account_width)),"+
               "             blue if color),"+
               "             bold if should_bold))"+
               " %(ansify_if("+
               "   justify(scrub(display_amount), int(amount_width), "+
               "           3 + int(meta_width) + int(date_width) + int(payee_width)"+
               "             + int(account_width) + int(amount_width) + int(prepend_width),"+
               "           true, color),"+
               "           bold if should_bold))"+
               " %(ansify_if("+
               "   justify(scrub(display_total), int(total_width), "+
               "           4 + int(meta_width) + int(date_width) + int(payee_width)"+
               "             + int(account_width) + int(amount_width) + int(total_width)"+
               "             + int(prepend_width), true, color),"+
               "           bold if should_bold))\n%/"+
               "%(justify(\" \", int(date_width)))"+
               " %(ansify_if("+
               "   justify(truncated(has_tag(\"Payee\") ? payee : \" \", "+
               "                     int(payee_width)), int(payee_width)),"+
               "             bold if should_bold))"+
               " %$3 %$4 %$5\n");

            RelatedHandler = Options.Add(new Option(OptionRelated));

            RelatedAllHandler = Options.Add(new Option(OptionRelatedAll, (o, w) =>
            {
                RelatedHandler.On(w);
            }));

            RevaluedHandler = Options.Add(new Option(OptionRevalued));
            RevaluedOnlyHandler = Options.Add(new Option(OptionRevaluedOnly));

            RevaluedTotalHandler = Options.Add(new ExprOption(OptionRevaluedTotal, (o, w, s) => ((ExprOption)o).Expr = new Expr(s)));

            RichDataHandler = Options.Add(new Option(OptionRichData));

            SeedHandler = Options.Add(new Option(OptionSeed));

            SortHandler = Options.Add(new Option(OptionSort, (o, w, s) =>
            {
                SortXactsHandler.Off();
                SortAllHandler.Off();
            }));

            SortAllHandler = Options.Add(new Option(OptionSortAll, (o, w, s) =>
            {
                SortHandler.On(w, s);
                SortXactsHandler.Off();
            }));

            SortXactsHandler = Options.Add(new Option(OptionSortXacts, (o, w, s) =>
            {
                SortHandler.On(w, s);
                SortAllHandler.Off();
            }));

            StartOfWeekHandler = Options.Add(new Option(OptionStartOfWeek));
            SubTotalHandler = Options.Add(new Option(OptionSubTotal));
            TailHandler = Options.Add(new Option(OptionTail));

            TimeReportHandler = Options.Add(new Option(OptionTimeReport, (o, w) =>
            {
                BalanceFormatHandler.On(null, 
                    "%(ansify_if(justify(earliest_checkin ? " +
                    "     format_datetime(earliest_checkin) : \"\", 19, -1, true)," +
                    "     bold if latest_checkout_cleared))  " +
                    "%(ansify_if(justify(latest_checkout ? " +
                    "     format_datetime(latest_checkout) : \"\", 19, -1, true), " +
                    "     bold if latest_checkout_cleared)) " +
                    "%(latest_checkout_cleared ? \"*\" : \" \")  " +
                    "%(ansify_if(" +
                    "  justify(scrub(display_total), 8," +
                    "          8 + 4 + 19 * 2, true, color), bold if should_bold))" +
                    "  %(!options.flat ? depth_spacer : \"\")" +
                    "%-(ansify_if(" +
                    "   ansify_if(partial_account(options.flat), blue if color)," +
                    "             bold if should_bold))\n%/" +
                    "%$1  %$2  %$3\n%/" +
                    "%(prepend_width ? \" \" * int(prepend_width) : \"\")" +
                    "--------------------------------------------------\n");
            }));

            TotalHandler = Options.Add(new MergedExprOption("total_expr", "total", OptionTotal, (o, w, s) => ((MergedExprOption)o).Expr.Append(s)));

            TotalDataHandler = Options.Add(new Option(OptionTotalData));

            TruncateHandler = Options.Add(new Option(OptionTruncate, (o, w, s) =>
            {
                if (s == "leading")
                    Format.DefaultStyle = FormatElisionStyleEnum.TRUNCATE_LEADING;
                else if (s == "middle")
                    Format.DefaultStyle = FormatElisionStyleEnum.TRUNCATE_MIDDLE;
                else if (s == "trailing")
                    Format.DefaultStyle = FormatElisionStyleEnum.TRUNCATE_TRAILING;
                else 
                    throw new ArgumentException(String.Format("Unrecognized truncation style: '{0}'"), s);

                Format.DefaultStyleChanged = true;
            }));

            UnbudgetedHandler = Options.Add(new Option(OptionUnbudgeted, (o, w) => BudgetFlags = BudgetFlags | ReportBudgetFlags.BUDGET_UNBUDGETED ));

            UnclearedHandler = Options.Add(new Option(OptionUncleared, (o, w) =>
            {
                LimitHandler.On(w, "uncleared|pending");
            }));

            UnrealizedHandler = Options.Add(new Option(OptionUnrealized));

            UnrealizedGainsHandler = Options.Add(new Option(OptionUnrealizedGains));
            UnrealizedLossesHandler = Options.Add(new Option(OptionUnrealizedLosses));

            UnroundHandler = Options.Add(new Option(OptionUnround, (o, w) =>
            {
                AmountHandler.On(w, "unrounded(amount_expr)");
                TotalHandler.On(w, "unrounded(total_expr)");
            }));

            WeeklyHandler = Options.Add(new Option(OptionWeekly, (o, w) =>
            {
                PeriodHandler.On(w, "weekly");
            }));

            WideHandler = Options.Add(new Option(OptionWide, (o, w) =>
            {
                ColumnsHandler.On(w, "132");
            }));

            YearlyHandler = Options.Add(new Option(OptionYearly, (o, w) =>
            {
                PeriodHandler.On(w, "yearly");
            }));

            MetaWidthHandler = Options.Add(new Option(OptionMetaWidth));
            DateWidthHandler = Options.Add(new Option(OptionDateWidth));
            PayeeWidthHandler = Options.Add(new Option(OptionPayeeWidth));
            AccountWidthHandler = Options.Add(new Option(OptionAccountWidth));
            AmountWidthHandler = Options.Add(new Option(OptionAmountWidth));
            TotalWidthHandler = Options.Add(new Option(OptionTotalWidth));
            ValuesHandler = Options.Add(new Option(OptionValues));

            #endregion

            #region Register Options
            Options.AddLookupArgs(OptionPercent, "%");
            Options.AddLookupArgs(OptionAverage, "A");
            Options.AddLookupArgs(OptionBasis, "B");
            Options.AddLookupArgs(OptionCleared, "C");
            Options.AddLookupArgs(OptionDaily, "D");
            Options.AddLookupArgs(OptionEmpty, "E");
            Options.AddLookupArgs(OptionFormat, "F");
            Options.AddLookupArgs(OptionGain, "G");
            Options.AddLookupArgs(OptionHistorical, "H");
            Options.AddLookupArgs(OptionPrice, "I");
            Options.AddLookupArgs(OptionTotalData, "J");
            Options.AddLookupArgs(OptionActual, "L");
            Options.AddLookupArgs(OptionMonthly, "M");
            Options.AddLookupArgs(OptionQuantity, "O");
            Options.AddLookupArgs(OptionByPayee, "P");
            Options.AddLookupArgs(OptionReal, "R");
            Options.AddLookupArgs(OptionSort, "S");
            Options.AddLookupArgs(OptionTotal, "T");
            Options.AddLookupArgs(OptionUncleared, "U");
            Options.AddLookupArgs(OptionMarket, "V");
            Options.AddLookupArgs(OptionWeekly, "W");
            Options.AddLookupArgs(OptionExchange, "X");
            Options.AddLookupArgs(OptionYearly, "Y");

            Options.AddLookupOpt(OptionAbbrevLen);
            Options.AddLookupOptArgs(OptionAccount, "a");
            Options.AddLookupOpt(OptionActual);
            Options.AddLookupOpt(OptionAddBudget);
            Options.AddLookupOpt(OptionAmount);
            Options.AddLookupOpt(OptionAmountData);
            Options.AddLookupArgs(OptionAmountData, "j");
            Options.AddLookupOptAlt(OptionPrimaryDate, "actual_dates");
            Options.AddLookupOpt(OptionAnon);
            Options.AddLookupOptAlt(OptionPrimaryDate, "ansi");
            Options.AddLookupOpt(OptionAutoMatch);
            Options.AddLookupOpt(OptionAuxDate);
            Options.AddLookupOpt(OptionAverage);
            Options.AddLookupOpt(OptionAccountWidth);
            Options.AddLookupOpt(OptionAverageLotPrices);
            Options.AddLookupOpt(OptionAmountWidth);

            Options.AddLookupOpt(OptionBalanceFormat);
            Options.AddLookupOpt(OptionBase);
            Options.AddLookupOpt(OptionBasis);
            Options.AddLookupOptArgs(OptionBegin, "b");
            Options.AddLookupOpt(OptionBoldIf);
            Options.AddLookupOpt(OptionBudget);
            Options.AddLookupOpt(OptionBudgetFormat);
            Options.AddLookupOpt(OptionByPayee);

            Options.AddLookupOpt(OptionCsvFormat);
            Options.AddLookupOptAlt(OptionGain, "change");
            Options.AddLookupOpt(OptionCleared);
            Options.AddLookupOpt(OptionClearedFormat);
            Options.AddLookupOpt(OptionCollapse);
            Options.AddLookupOpt(OptionCollapseIfZero);
            Options.AddLookupOpt(OptionColor);
            Options.AddLookupOpt(OptionColumns);
            Options.AddLookupOptAlt(OptionBasis, "cost");
            Options.AddLookupOptArgs(OptionCurrent, "c");
            Options.AddLookupOpt(OptionCount);

            Options.AddLookupOpt(OptionDaily);
            Options.AddLookupOpt(OptionDate);
            Options.AddLookupOpt(OptionDateFormat);
            Options.AddLookupOpt(OptionDateTimeFormat);
            Options.AddLookupOpt(OptionDc);
            Options.AddLookupOpt(OptionDepth);
            Options.AddLookupOpt(OptionDeviation);
            Options.AddLookupOptAlt(OptionRichData, "detail");
            Options.AddLookupOptArgs(OptionDisplay, "d");
            Options.AddLookupOpt(OptionDisplayAmount);
            Options.AddLookupOpt(OptionDisplayTotal);
            Options.AddLookupOptAlt(OptionDow, "days_of_week");
            Options.AddLookupOpt(OptionDateWidth);

            Options.AddLookupOpt(OptionEmpty);
            Options.AddLookupOptArgs(OptionEnd, "e");
            Options.AddLookupOpt(OptionEquity);
            Options.AddLookupOpt(OptionExact);
            Options.AddLookupOpt(OptionExchange);
            Options.AddLookupOptAlt(OptionAuxDate, "effective");

            Options.AddLookupOpt(OptionFlat);
            Options.AddLookupOptAlt(OptionForecastWhile, "forecast_");
            Options.AddLookupOpt(OptionForecastYears);
            Options.AddLookupOpt(OptionFormat);
            Options.AddLookupOpt(OptionForceColor);
            Options.AddLookupOpt(OptionForcePager);
            Options.AddLookupOptAlt(OptionHead, "first_");

            Options.AddLookupOpt(OptionGain);
            Options.AddLookupOpt(OptionGroupBy);
            Options.AddLookupOpt(OptionGroupTitleFormat);
            Options.AddLookupOpt(OptionGenerated);

            Options.AddLookupOpt(OptionHead);
            Options.AddLookupOpt(OptionHistorical);

            Options.AddLookupOpt(OptionInvert);
            Options.AddLookupOpt(OptionInject);
            Options.AddLookupOpt(OptionImmediate);

            Options.AddLookupArgs(OptionImmediate, "j");

            Options.AddLookupOptArgs(OptionLimit, "l");
            Options.AddLookupOpt(OptionLotDates);
            Options.AddLookupOpt(OptionLotPrices);
            Options.AddLookupOptAlt(OptionLotNotes, "lot_tags");
            Options.AddLookupOpt(OptionLots);
            Options.AddLookupOpt(OptionLotsActual);
            Options.AddLookupOptAlt(OptionTail, "last_");

            Options.AddLookupOpt(OptionMarket);
            Options.AddLookupOpt(OptionMonthly);
            Options.AddLookupOpt(OptionMeta);
            Options.AddLookupOpt(OptionMetaWidth);

            Options.AddLookupArgs(OptionCollapse, "n");
            Options.AddLookupOpt(OptionNoColor);
            Options.AddLookupOpt(OptionNoPager);
            Options.AddLookupOpt(OptionNoRevalued);
            Options.AddLookupOpt(OptionNoRounding);
            Options.AddLookupOpt(OptionNoTitles);
            Options.AddLookupOpt(OptionNoTotal);
            Options.AddLookupOpt(OptionNow);

            Options.AddLookupOpt(OptionOnly);
            Options.AddLookupOptArgs(OptionOutput, "o");

            Options.AddLookupOpt(OptionPager);
            Options.AddLookupOpt(OptionPayee);
            Options.AddLookupOpt(OptionPending);
            Options.AddLookupOpt(OptionPercent);
            Options.AddLookupOptArgs(OptionPeriod, "p");
            Options.AddLookupOptAlt(OptionSortXacts, "period_sort_");
            Options.AddLookupOpt(OptionPivot);
            Options.AddLookupOpt(OptionPlotAmountFormat);
            Options.AddLookupOpt(OptionPlotTotalFormat);
            Options.AddLookupOpt(OptionPrice);
            Options.AddLookupOpt(OptionPricesFormat);
            Options.AddLookupOpt(OptionPriceDbFormat);
            Options.AddLookupOpt(OptionPrimaryDate);
            Options.AddLookupOpt(OptionPayeeWidth);
            Options.AddLookupOpt(OptionPrependFormat);
            Options.AddLookupOpt(OptionPrependWidth);

            Options.AddLookupOpt(OptionQuantity);
            Options.AddLookupOpt(OptionQuarterly);

            Options.AddLookupOpt(OptionRaw);
            Options.AddLookupOpt(OptionReal);
            Options.AddLookupOpt(OptionRegisterFormat);
            Options.AddLookupOptArgs(OptionRelated, "r");
            Options.AddLookupOpt(OptionRelatedAll);
            Options.AddLookupOpt(OptionRevalued);
            Options.AddLookupOpt(OptionRevaluedOnly);
            Options.AddLookupOpt(OptionRevaluedTotal);
            Options.AddLookupOpt(OptionRichData);

            Options.AddLookupOpt(OptionSort);
            Options.AddLookupOpt(OptionSortAll);
            Options.AddLookupOpt(OptionSortXacts);
            Options.AddLookupOptArgs(OptionSubTotal, "s");
            Options.AddLookupOpt(OptionStartOfWeek);
            Options.AddLookupOpt(OptionSeed);

            Options.AddLookupArgs(OptionAmount, "t");
            Options.AddLookupOpt(OptionTail);
            Options.AddLookupOpt(OptionTotal);
            Options.AddLookupOpt(OptionTotalData);
            Options.AddLookupOpt(OptionTruncate);
            Options.AddLookupOpt(OptionTotalWidth);
            Options.AddLookupOpt(OptionTimeReport);

            Options.AddLookupOpt(OptionUnbudgeted);
            Options.AddLookupOpt(OptionUncleared);
            Options.AddLookupOpt(OptionUnrealized);
            Options.AddLookupOpt(OptionUnrealizedGains);
            Options.AddLookupOpt(OptionUnrealizedLosses);
            Options.AddLookupOpt(OptionUnround);

            Options.AddLookupOptAlt(OptionMarket, "value");
            Options.AddLookupOpt(OptionValues);

            Options.AddLookupOpt(OptionWeekly);
            Options.AddLookupOptArgs(OptionWide, "w");

            Options.AddLookupArgs(OptionDateFormat, "y");
            Options.AddLookupOpt(OptionYearly);
            #endregion
        }

        private void CreateLookupItems()
        {
            // Support 2.x's single-letter value expression names.
            LookupItems.MakeFunctor("d", scope => FnNow((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("m", scope => FnNow((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("P", scope => FnMarket((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("t", scope => FnDisplayAmount((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("T", scope => FnDisplayTotal((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("U", scope => FnAbs((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("S", scope => FnStrip((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("i", scope => FnRuntimeError(CallScope.Create("i")), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("A", scope => FnRuntimeError(CallScope.Create("A")), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("v", scope => FnRuntimeError(CallScope.Create("v")), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("V", scope => FnRuntimeError(CallScope.Create("V")), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("I", scope => FnRuntimeError(CallScope.Create("I")), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("B", scope => FnRuntimeError(CallScope.Create("B")), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("g", scope => FnRuntimeError(CallScope.Create("g")), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("G", scope => FnRuntimeError(CallScope.Create("G")), SymbolKindEnum.FUNCTION);

            LookupItems.MakeFunctor("amount_expr", scope => FnAmountExpr((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("ansify_if", scope => FnAnsifyIf((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("abs", scope => FnAbs((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("averaged_lots", scope => FnAveragedLots((CallScope)scope), SymbolKindEnum.FUNCTION);

            LookupItems.MakeFunctor("black", scope => FnBlack((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("blink", scope => FnBlink((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("blue", scope => FnBlue((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("bold", scope => FnBold((CallScope)scope), SymbolKindEnum.FUNCTION);

            LookupItems.MakeFunctor("cyan", scope => FnCyan((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("commodity", scope => FnCommodity((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("ceiling", scope => FnCeiling((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("clear_commodity", scope => FnClearCommodity((CallScope)scope), SymbolKindEnum.FUNCTION);

            LookupItems.MakeFunctor("display_amount", scope => FnDisplayAmount((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("display_total", scope => FnDisplayTotal((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("date", scope => FnToday((CallScope)scope), SymbolKindEnum.FUNCTION);

            LookupItems.MakeFunctor("format_date", scope => FnFormatDate((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("format_datetime", scope => FnFormatDateTime((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("format", scope => FnFormat((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("floor", scope => FnFloor((CallScope)scope), SymbolKindEnum.FUNCTION);

            LookupItems.MakeFunctor("get_at", scope => FnGetAt((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("green", scope => FnGreen((CallScope)scope), SymbolKindEnum.FUNCTION);

            LookupItems.MakeFunctor("is_seq", scope => FnIsSeq((CallScope)scope), SymbolKindEnum.FUNCTION);

            LookupItems.MakeFunctor("justify", scope => FnJustify((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("join", scope => FnJoin((CallScope)scope), SymbolKindEnum.FUNCTION);

            LookupItems.MakeFunctor("market", scope => FnMarket((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("magenta", scope => FnMagenta((CallScope)scope), SymbolKindEnum.FUNCTION);

            LookupItems.MakeFunctor("null", scope => FnNull((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("now", scope => FnNow((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("nail_down", scope => FnNailDown((CallScope)scope), SymbolKindEnum.FUNCTION);

            LookupItems.MakeFunctor("options", scope => FnOptions((CallScope)scope), SymbolKindEnum.FUNCTION);

            LookupItems.MakeFunctor("post", scope => FnFalse((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("percent", scope => FnPercent((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("print", scope => FnPrint((CallScope)scope), SymbolKindEnum.FUNCTION);

            LookupItems.MakeFunctor("quoted", scope => FnQuoted((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("quoted_rfc4180", scope => FnQuotedRfc4180((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("quantity", scope => FnQuantity((CallScope)scope), SymbolKindEnum.FUNCTION);

            LookupItems.MakeFunctor("rounded", scope => FnRounded((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("red", scope => FnRed((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("round", scope => FnRound((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("roundto", scope => FnRoundTo((CallScope)scope), SymbolKindEnum.FUNCTION);

            LookupItems.MakeFunctor("scrub", scope => FnScrub((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("strip", scope => FnStrip((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("should_bold", scope => FnShouldBold((CallScope)scope), SymbolKindEnum.FUNCTION);

            LookupItems.MakeFunctor("truncated", scope => FnTruncated((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("total_expr", scope => FnTotalExpr((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("today", scope => FnToday((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("trim", scope => FnTrim((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("top_amount", scope => FnTopAmount((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("to_boolean", scope => FnToBoolean((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("to_int", scope => FnToInt((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("to_datetime", scope => FnToDateTime((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("to_date", scope => FnToDate((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("to_amount", scope => FnToAmount((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("to_balance", scope => FnToBalance((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("to_string", scope => FnToString((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("to_mask", scope => FnToMask((CallScope)scope), SymbolKindEnum.FUNCTION);

            LookupItems.MakeFunctor("underline", scope => FnUnderline((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("unround", scope => FnUnround((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("unrounded", scope => FnUnrounded((CallScope)scope), SymbolKindEnum.FUNCTION);

            LookupItems.MakeFunctor("value_date", scope => FnNow((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("white", scope => FnWhite((CallScope)scope), SymbolKindEnum.FUNCTION);
            LookupItems.MakeFunctor("yellow", scope => FnYellow((CallScope)scope), SymbolKindEnum.FUNCTION);

            // Check if they are trying to access an option's setting or value.
            LookupItems.MakeOptionFunctors(Options);            
            LookupItems.MakeOptionHandlers(Options);

            ///// COMMANDS

            // a
            var accountsReporter = PostsReporter(new ReportAccounts(this), "#accounts");
            LookupItems.MakeFunctor("accounts", scope => accountsReporter.Handle((CallScope)scope), SymbolKindEnum.COMMAND);

            // b
            LookupItems.MakeFunctor("b", scope => FormattedAccountsReporter(BalanceFormatHandler, "#balance").Handle((CallScope)scope), SymbolKindEnum.COMMAND);
            LookupItems.MakeFunctor("bal", scope => FormattedAccountsReporter(BalanceFormatHandler, "#balance").Handle((CallScope)scope), SymbolKindEnum.COMMAND);
            LookupItems.MakeFunctor("balance", scope => FormattedAccountsReporter(BalanceFormatHandler, "#balance").Handle((CallScope)scope), SymbolKindEnum.COMMAND);

            LookupItems.MakeFunctor("budget", scope =>
            {
                AmountHandler.On("#budget", "(amount, 0)");
                BudgetFlags |= ReportBudgetFlags.BUDGET_WRAP_VALUES;
                if (!BudgetFlags.HasFlag(ReportBudgetFlags.BUDGET_BUDGETED) && !BudgetFlags.HasFlag(ReportBudgetFlags.BUDGET_UNBUDGETED))
                    BudgetFlags |= ReportBudgetFlags.BUDGET_BUDGETED;
                return FormattedAccountsReporter(BudgedFormatHandler, "#budget").Handle((CallScope)scope);
            }, SymbolKindEnum.COMMAND);

            // c
            LookupItems.MakeFunctor("csv", scope => FormattedPostsReporter(CsvFormatHandler, "#csv").Handle((CallScope)scope), SymbolKindEnum.COMMAND);

            LookupItems.MakeFunctor("cleared", scope =>
            {
                AmountHandler.On("#cleared", "(amount, cleared ? amount : 0)");
                return FormattedAccountsReporter(ClearedFormatHandler, "#cleared").Handle((CallScope)scope);
            }, SymbolKindEnum.COMMAND);

            LookupItems.MakeFunctor("convert", scope => new Converter().ConvertCommand((CallScope)scope), SymbolKindEnum.COMMAND);

            LookupItems.MakeFunctor("commodities", scope => PostsReporter(new ReportCommodities(this), "#commodities").Handle((CallScope)scope), SymbolKindEnum.COMMAND);

            // d
            LookupItems.MakeFunctor("draft", scope => DraftsCommon.XactCommand((CallScope)scope), SymbolKindEnum.COMMAND);

            // e
            LookupItems.MakeFunctor("equity", scope => 
                {
                    GeneratedHandler.On("#equity");
                    return PostsReporter(new PrintXacts(this), "#equity").Handle((CallScope)scope);
                }, SymbolKindEnum.COMMAND);

            LookupItems.MakeFunctor("entry", scope => DraftsCommon.XactCommand((CallScope)scope), SymbolKindEnum.COMMAND);

            LookupItems.MakeFunctor("emacs", scope => PostsReporter(new FormatEmacsPosts(this.OutputStream), "#emacs").Handle((CallScope)scope), SymbolKindEnum.COMMAND);

            LookupItems.MakeFunctor("echo", scope => EchoCommand((CallScope)scope), SymbolKindEnum.COMMAND);

            // l
            LookupItems.MakeFunctor("lisp", scope => PostsReporter(new FormatEmacsPosts(this.OutputStream), "#lisp").Handle((CallScope)scope), SymbolKindEnum.COMMAND);

            // p

            LookupItems.MakeFunctor("p", scope => PostsReporter(new PrintXacts(this, this.RawHandler.Handled), "#print").Handle((CallScope)scope), SymbolKindEnum.COMMAND);
            LookupItems.MakeFunctor("print", scope => PostsReporter(new PrintXacts(this, this.RawHandler.Handled), "#print").Handle((CallScope)scope), SymbolKindEnum.COMMAND);

            LookupItems.MakeFunctor("prices", scope => FormattedCommoditiesReporter(PricesFormatHandler, "#print").Handle((CallScope)scope), SymbolKindEnum.COMMAND);

            LookupItems.MakeFunctor("pricedb", scope => FormattedCommoditiesReporter(PriceDbFormatHandler, "#pricedb").Handle((CallScope)scope), SymbolKindEnum.COMMAND);
            LookupItems.MakeFunctor("pricesdb", scope => FormattedCommoditiesReporter(PriceDbFormatHandler, "#pricedb").Handle((CallScope)scope), SymbolKindEnum.COMMAND);

            LookupItems.MakeFunctor("pricemap", scope => PricemapCommand((CallScope)scope), SymbolKindEnum.COMMAND);

            LookupItems.MakeFunctor("payees", scope => PostsReporter(new ReportPayees(this), "#payees").Handle((CallScope)scope), SymbolKindEnum.COMMAND);

            // r

            LookupItems.MakeFunctor("r", scope => FormattedPostsReporter(RegisterFormatHandler, "#register").Handle((CallScope)scope), SymbolKindEnum.COMMAND);
            LookupItems.MakeFunctor("reg", scope => FormattedPostsReporter(RegisterFormatHandler, "#register").Handle((CallScope)scope), SymbolKindEnum.COMMAND);
            LookupItems.MakeFunctor("register", scope => FormattedPostsReporter(RegisterFormatHandler, "#register").Handle((CallScope)scope), SymbolKindEnum.COMMAND);

            LookupItems.MakeFunctor("reload", scope => ReloadCommand((CallScope)scope), SymbolKindEnum.COMMAND);

            // s

            LookupItems.MakeFunctor("stats", scope => ReportStatistics((CallScope)scope), SymbolKindEnum.COMMAND);
            LookupItems.MakeFunctor("stat", scope => ReportStatistics((CallScope)scope), SymbolKindEnum.COMMAND);

            LookupItems.MakeFunctor("source", scope => Expr.SourceCommand((CallScope)scope), SymbolKindEnum.COMMAND);

            LookupItems.MakeFunctor("select", scope => Select.SelectCommand((CallScope)scope), SymbolKindEnum.COMMAND);

            // t

            LookupItems.MakeFunctor("tags", scope => PostsReporter(new ReportTags(this), "#tags").Handle((CallScope)scope), SymbolKindEnum.COMMAND);

            // x

            LookupItems.MakeFunctor("xact", scope => DraftsCommon.XactCommand((CallScope)scope), SymbolKindEnum.COMMAND);

            LookupItems.MakeFunctor("xml", scope => PostsReporter(new FormatPTree(this), "#xml").Handle((CallScope)scope), SymbolKindEnum.COMMAND);

            ///// PRE-COMMANDS

            // a

            LookupItems.MakeFunctor("args", scope => PreCmd.QueryCommand((CallScope)scope), SymbolKindEnum.PRECOMMAND);

            // e

            LookupItems.MakeFunctor("eval", scope => PreCmd.EvalCommand((CallScope)scope), SymbolKindEnum.PRECOMMAND);
            LookupItems.MakeFunctor("expr", scope => PreCmd.ParseCommand((CallScope)scope), SymbolKindEnum.PRECOMMAND);

            // f

            LookupItems.MakeFunctor("format", scope => PreCmd.FormatCommand((CallScope)scope), SymbolKindEnum.PRECOMMAND);

            // g

            var generateReporter = PostsReporter(new PrintXacts(this), "#generate", p => this.GenerateReport(p));
            LookupItems.MakeFunctor("generate", scope => generateReporter.Handle((CallScope)scope), SymbolKindEnum.PRECOMMAND);

            // p

            LookupItems.MakeFunctor("parse", scope => PreCmd.ParseCommand((CallScope)scope), SymbolKindEnum.PRECOMMAND);
            LookupItems.MakeFunctor("period", scope => PreCmd.PeriodCommand((CallScope)scope), SymbolKindEnum.PRECOMMAND);

            // q

            LookupItems.MakeFunctor("query", scope => PreCmd.QueryCommand((CallScope)scope), SymbolKindEnum.PRECOMMAND);

            // s

            LookupItems.MakeFunctor("script", scope => Expr.SourceCommand((CallScope)scope), SymbolKindEnum.PRECOMMAND);

            // t

            LookupItems.MakeFunctor("template", scope => DraftsCommon.TemplateCommand((CallScope)scope), SymbolKindEnum.PRECOMMAND);
        }

        private Reporter<Account, AccountHandler> FormattedAccountsReporter(Option format, string whence)
        {
            return new Reporter<Account, AccountHandler>(
                new FormatAccounts(this, ReportFormat(format), MaybeFormat(PrependFormatHandler), PrependWidthHandler.Handled ? Int32.Parse(PrependWidthHandler.Str()) : 0),
                this, whence, AccountsReport);
        }

        private Reporter<Post, PostHandler> PostsReporter(PostHandler formatter, string whence, Action<PostHandler> method = null)
        {
            return new Reporter<Post, PostHandler>(formatter, this, whence, method == null ? PostsReport : method);
        }

        private Reporter<Post, PostHandler> FormattedPostsReporter(Option format, string whence)
        {
            return PostsReporter(new FormatPosts(this, ReportFormat(format), MaybeFormat(PrependFormatHandler),
                PrependWidthHandler.Handled ? Int32.Parse(PrependWidthHandler.Str()) : 0), whence);
        }

        private Reporter<Post, PostHandler> FormattedCommoditiesReporter(Option format, string whence)
        {
            return PostsReporter(new FormatPosts(this, ReportFormat(format), MaybeFormat(PrependFormatHandler),
                PrependWidthHandler.Handled ? Int32.Parse(PrependWidthHandler.Str()) : 0), whence, CommoditiesReport);
        }

        private readonly OptionCollection Options = new OptionCollection();
        private readonly ExprOpCollection LookupItems = new ExprOpCollection();
    }

    public class MergedExprOption : Option
    {
        public MergedExprOption(string exprTerm, string exprExpr, string name, HandlerThunkStrDelegate onHandlerThunk)
            : base(name, onHandlerThunk)
        {
            Expr = new MergedExpr(exprTerm, exprExpr);
        }

        public MergedExpr Expr { get; private set; }
    }

    public class ExprOption : Option
    {
        public ExprOption(string name, HandlerThunkStrDelegate onHandlerThunk)
            : base(name, onHandlerThunk)
        {
            Expr = Expr.Empty;
        }

        public Expr Expr { get; set; }
    }

    public class Reporter<T, H>
        where H : ItemHandler<T>
        where T : class
    {
        public Reporter(H handler, Report report, string whence, Action<H> method)
        {
            Handler = handler;
            Report = report;
            Whence = whence;

            Method = method;
        }

        public Value Handle(CallScope args)
        {
            if (args.Size > 0)
                Report.ParseQueryArgs(args.Value(), Whence);

            Method(Handler);

            return Value.True;
        }

        private H Handler { get; set; }
        private Report Report { get; set; }
        private string Whence { get; set; }
        private Action<H> Method { get; set; }
    }
}
