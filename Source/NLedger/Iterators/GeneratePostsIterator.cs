// **********************************************************************************
// Copyright (c) 2015-2018, Dmitry Merzlyakov.  All rights reserved.
// Licensed under the FreeBSD Public License. See LICENSE file included with the distribution for details and disclaimer.
// 
// This file is part of NLedger that is a .Net port of C++ Ledger tool (ledger-cli.org). Original code is licensed under:
// Copyright (c) 2003-2018, John Wiegley.  All rights reserved.
// See LICENSE.LEDGER file included with the distribution for details and disclaimer.
// **********************************************************************************
using NLedger.Scopus;
using NLedger.Textual;
using NLedger.Times;
using NLedger.Utility;
using NLedger.Utility.Rnd;
using NLedger.Utils;
using NLedger.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Iterators
{
    /// <summary>
    /// Ported from generate_posts_iterator
    /// </summary>
    public class GeneratePostsIterator : IIterator<Post>
    {
        public GeneratePostsIterator(Session session, int seed = 0, int quantity = 100)
        {
            Session = session;
            Seed = seed;
            Quantity = quantity;

            Random random = Seed != 0 ? new Random(Seed) : new Random();
            ThreeGen = new IntegerGenerator(random, 1, 3);
            SixGen = new IntegerGenerator(random, 1, 6);
            StrLenGen = new IntegerGenerator(random, 1, 40);
            UpcharGen = new CharGenerator(random, 'A', 'Z');
            DowncharGen = new CharGenerator(random, 'a', 'z');
            NumcharGen = new CharGenerator(random, '0', '9');
            TruthGen = new BoolGenerator(random);
            NegNumberGen = new DoubleGenerator(random, - 10000, -1);
            PosNumberGen = new DoubleGenerator(random, 1, 10000);
            YearGen = new IntegerGenerator(random, 1900, 2300);
            MonGen = new IntegerGenerator(random, 1, 12);
            DayGen = new IntegerGenerator(random, 1, 28);

            NextDate = TimesCommon.Current.ParseDate(GenerateDate());
            NextAuxDate = TimesCommon.Current.ParseDate(GenerateDate());
        }

        public int Seed { get; private set; }
        public Session Session { get; private set; }
        public int Quantity { get; private set; }

        public IntegerGenerator StrLenGen { get; private set; }
        public IntegerGenerator ThreeGen { get; private set; }
        public IntegerGenerator SixGen { get; private set; }
        public CharGenerator UpcharGen { get; private set; }
        public CharGenerator DowncharGen { get; private set; }
        public CharGenerator NumcharGen { get; private set; }
        public BoolGenerator TruthGen { get; private set; }
        public DoubleGenerator NegNumberGen { get; private set; }
        public DoubleGenerator PosNumberGen { get; private set; }
        public IntegerGenerator YearGen { get; private set; }
        public IntegerGenerator MonGen { get; private set; }
        public IntegerGenerator DayGen { get; private set; }

        public Date NextDate { get; private set; }
        public Date NextAuxDate { get; private set; }

        /// <summary>
        /// Ported from void generate_posts_iterator::increment
        /// </summary>
        public IEnumerable<Post> Get()
        {
            IList<Post> posts = new List<Post>();

            int i = 0;
            while (i < Quantity)
            {
                string buf = GenerateXact();

                Logger.Current.Debug("generate.post", () => String.Format("The post we intend to parse:\r\n{0}", buf));

                try
                {
                    ParseContextStack parsingContext = new ParseContextStack();
                    parsingContext.Push(new TextualReader(FileSystem.GetStreamReaderFromString(buf)));
                    parsingContext.GetCurrent().Journal = Session.Journal;
                    parsingContext.GetCurrent().Scope = Session;

                    if (Session.Journal.Read(parsingContext) != 0)
                    {
                        Validator.Verify(() => Session.Journal.Xacts.Last().Valid());
                        XactPostsIterator iterPosts = new XactPostsIterator(Session.Journal.Xacts.Last());
                        foreach (Post post in iterPosts.Get())
                        {
                            if (i++ >= Quantity)
                                break;
                            posts.Add(post);
                        }
                    }

                }
                catch
                {
                    ErrorContext.Current.AddErrorContext(String.Format("While parsing generated transaction (seed {0}):", Seed));
                    ErrorContext.Current.AddErrorContext(buf.ToString());
                    throw;
                }
            }

            return posts;
        }

        /// <summary>
        /// Ported from void generate_posts_iterator::generate_string
        /// </summary>
        public string GenerateString(int len, bool onlyAlpha = false)
        {
            Logger.Current.Debug("generate.post.string", () => String.Format("Generating string of length {0}, only alpha {1}", len, onlyAlpha));

            StringBuilder sb = new StringBuilder();

            int last = -1;
            bool first = true;
            for (int i = 0; i < len; i++)
            {
                int next = onlyAlpha ? 3 : ThreeGen.Value();
                bool output = true;
                switch (next)
                {
                    case 1:                     // colon
                        if (!first && last == 3 && StrLenGen.Value() % 10 == 0 && i + 1 != len)
                            sb.Append(':');
                        else
                        {
                            i--;
                            output = false;
                        }
                        break;
                    case 2:                     // space
                        if (!first && last == 3 && StrLenGen.Value() % 20 == 0 && i + 1 != len)
                            sb.Append(' ');
                        else
                        {
                            i--;
                            output = false;
                        }
                        break;
                    case 3:                     // character
                        switch (ThreeGen.Value())
                        {
                            case 1:             // uppercase
                                sb.Append(UpcharGen.Value());
                                break;
                            case 2:             // lowercase
                                sb.Append(DowncharGen.Value());
                                break;
                            case 3:             // number
                                if (!onlyAlpha && !first)
                                    sb.Append(NumcharGen.Value());
                                else
                                {
                                    i--;
                                    output = false;
                                }
                                break;
                        }
                        break;
                }
                if (output)
                {
                    last = next;
                    first = false;
                }
            }

            return sb.ToString();
        }

        public bool GenerateAccount(StringBuilder sb, bool noVirtual = false)
        {
            bool mustBalance = true;
            bool isVirtual = false;

            if (!noVirtual)
            {
                switch (ThreeGen.Value())
                {
                    case 1:
                        sb.Append('[');
                        isVirtual = true;
                        break;
                    case 2:
                        sb.Append('(');
                        mustBalance = false;
                        isVirtual = true;
                        break;
                    case 3:
                        break;
                }
            }

            sb.Append(GenerateString(StrLenGen.Value()));

            if (isVirtual)
            {
                if (mustBalance)
                    sb.Append(']');
                else
                    sb.Append(')');
            }

            return mustBalance;
        }

        public string GenerateCommodity(string exclude = null)
        {
            string comm;
            do
            {
                comm = GenerateString(SixGen.Value(), true);
            }
            while (comm == exclude || comm == "h" || comm == "m" || comm == "s" ||
                   comm == "and" || comm == "any" || comm == "all" || comm == "div" ||
                   comm == "false" || comm == "or" || comm == "not" ||
                   comm == "true" || comm == "if" || comm == "else");

            return comm;
        }

        public string GenerateAmount(StringBuilder sbOut, Value notThisAmount = null, bool noNegative = false, string exclude = null)
        {
            StringBuilder sb = new StringBuilder();

            if (TruthGen.Value())
            {            // commodity goes in front
                sb.Append(GenerateCommodity(exclude));
                if (TruthGen.Value())
                    sb.Append(' ');
                if (noNegative || TruthGen.Value())
                    sb.Append(PosNumberGen.Value());
                else
                    sb.Append(NegNumberGen.Value());
            }
            else
            {
                if (noNegative || TruthGen.Value())
                    sb.Append(PosNumberGen.Value());
                else
                    sb.Append(NegNumberGen.Value());
                if (TruthGen.Value())
                    sb.Append(' ');
                sb.Append(GenerateCommodity(exclude));
            }

            // Possibly generate an annotized commodity, but make it rarer
            if (!noNegative && ThreeGen.Value() == 1)
            {
                if (ThreeGen.Value() == 1)
                {
                    sb.Append(" {");
                    GenerateAmount(sb, Value.Empty, true);
                    sb.Append('}');
                }
                if (SixGen.Value() == 1)
                {
                    sb.Append(" [");
                    sb.Append(GenerateDate());
                    sb.Append(']');
                }
                if (SixGen.Value() == 1)
                {
                    sb.Append(" (");
                    sb.Append(GenerateString(SixGen.Value()));
                    sb.Append(')');
                }
            }

            if (!Value.IsNullOrEmpty(notThisAmount) && Value.Get(sb.ToString()).AsAmount.Commodity == notThisAmount.AsAmount.Commodity)
                return String.Empty;

            sbOut.Append(sb.ToString());

            return sb.ToString();
        }

        public bool GeneratePost(StringBuilder sb, bool noAmount = false)
        {
            sb.Append("    ");
            bool mustBalance = GenerateAccount(sb, noAmount);
            sb.Append("  ");

            if (!noAmount)
            {
                Value amount = Value.Get(GenerateAmount(sb));
                if (TruthGen.Value())
                    sb.Append(GenerateCost(amount));
            }
            if (TruthGen.Value())
                sb.Append(GenerateNote());
            sb.AppendLine();

            return mustBalance;
        }

        public string GenerateCost(Value amount)
        {
            StringBuilder sb = new StringBuilder();

            if (TruthGen.Value())
                sb.Append(" @ ");
            else
                sb.Append(" @@ ");

            if (!String.IsNullOrEmpty(GenerateAmount(sb, amount, true, amount.AsAmount.Commodity.Symbol)))
                return sb.ToString();
            else
                return String.Empty;
        }

        public string GenerateDate()
        {
            return String.Format("{0:0000}/{1:00}/{2:00}", YearGen.Value(), MonGen.Value(), DayGen.Value());
        }

        public string GenerateState()
        {
            switch (ThreeGen.Value())
            {
                case 1:
                    return "* ";
                case 2:
                    return "! ";
                case 3:
                default:
                    return String.Empty;
            }
        }

        public string GenerateCode()
        {
            return String.Format("({0})", GenerateString(SixGen.Value()));
        }

        public string GeneratePayee()
        {
            return GenerateString(StrLenGen.Value());
        }

        public string GenerateNote()
        {
            return String.Format("\n    ; {0}", GenerateString(StrLenGen.Value()));
        }

        public string GenerateXact()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Times.TimesCommon.Current.FormatDate(NextDate, FormatTypeEnum.FMT_WRITTEN));
            NextDate = NextDate.AddDays(SixGen.Value());
            if (TruthGen.Value())
            {
                sb.Append('=');
                sb.Append(Times.TimesCommon.Current.FormatDate(NextAuxDate, FormatTypeEnum.FMT_WRITTEN));
                NextAuxDate = NextAuxDate.AddDays(SixGen.Value());
            }
            sb.Append(' ');

            sb.Append(GenerateState());
            sb.Append(GenerateCode());
            sb.Append(GeneratePayee());
            if (TruthGen.Value())
                sb.Append(GenerateNote());
            sb.AppendLine();

            int count = ThreeGen.Value() * 2;
            bool hasMustBalance = false;
            for (int i = 0; i < count; i++)
            {
                if (GeneratePost(sb))
                    hasMustBalance = true;
            }
            if (hasMustBalance)
                GeneratePost(sb, true);

            sb.AppendLine();

            return sb.ToString();
        }
    }
}
