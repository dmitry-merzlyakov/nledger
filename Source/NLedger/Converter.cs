// **********************************************************************************
// Copyright (c) 2015-2021, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2021, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Accounts;
using NLedger.Csv;
using NLedger.Iterators;
using NLedger.Journals;
using NLedger.Print;
using NLedger.Scopus;
using NLedger.Textual;
using NLedger.Utility;
using NLedger.Values;
using NLedger.Xacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger
{
    public class Converter
    {
        public Value ConvertCommand(CallScope args)
        {
            Report report = args.Context<Report>();
            Journal journal = report.Session.Journal;

            string bucketName;
            if (report.AccountHandler.Handled)
                bucketName = report.AccountHandler.Str();
            else
                bucketName = "Equity:Unknown";

            Account bucket = journal.Master.FindAccount(bucketName);
            Account unknown = journal.Master.FindAccount("Expenses:Unknown");

            // Create a flat list
            IList<Xact> currentXacts = new List<Xact>(journal.Xacts);

            // Read in the series of transactions from the CSV file

            PrintXacts formatter = new PrintXacts(report);
            string csvFilePath = args.Get<string>(0);

            report.Session.ParsingContext.Push(csvFilePath);
            ParseContext context = report.Session.ParsingContext.GetCurrent();
            context.Journal = journal;
            context.Master = bucket;

            CsvReader reader = new CsvReader(context);

            try
            {
                Xact xact;
                while((xact = reader.ReadXact(report.RichDataHandler.Handled)) != null)
                {
                    if (report.InvertHandler.Handled)
                    {
                        foreach (Post post in xact.Posts)
                            post.Amount.InPlaceNegate();
                    }

                    string sref = xact.HasTag("UUID") ? xact.GetTag("UUID").ToString() : SHA1.GetHash(reader.LastLine);

                    if (journal.ChecksumMapping.ContainsKey(sref))
                        continue;

                    if (report.RichDataHandler.Handled && !xact.HasTag("UUID"))
                        xact.SetTag("UUID", Value.StringValue(sref));

                    if (xact.Posts.First().Account == null)
                    {
                        Account acct = report.AutoMatchHandler.Handled ? Lookup.LookupProbableAccount(xact.Payee, currentXacts.Reverse(), bucket).Item2 : null;
                        if (acct != null)
                            xact.Posts.First().Account = acct;
                        else
                            xact.Posts.First().Account = unknown;
                    }

                    if (!journal.AddXact(xact))
                    {
                        throw new RuntimeError(RuntimeError.ErrorMessageFailedToFinalizeDerivedTransactionCheckCommodities);
                    }
                    else
                    {
                        XactPostsIterator xactIter = new XactPostsIterator(xact);
                        foreach (Post post in xactIter.Get())
                            formatter.Handle(post);                        
                    }
                }
                formatter.Flush();
            }
            catch
            {
                ErrorContext.Current.AddErrorContext(String.Format("While parsing file {0}", ErrorContext.FileContext(reader.PathName, reader.LineNum)));
                ErrorContext.Current.AddErrorContext("While parsing CSV line:");
                ErrorContext.Current.AddErrorContext(ErrorContext.LineContext(reader.LastLine));
                throw;
            }

            // If not, transform the payee according to regexps

            // Set the account to a default vaule, then transform the account according
            // to the payee

            // Print out the final form of the transaction

            return Value.True;
        }
    }
}
