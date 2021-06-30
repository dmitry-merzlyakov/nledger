using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLedger.Journals;

namespace NLedger.Utility.ServiceAPI
{
    public class ServiceQueryResult : BaseServiceResponse
    {
        public ServiceQueryResult(ServiceSession serviceSession, string query, CancellationToken token)
            : base(serviceSession, token)
        {
            if (String.IsNullOrWhiteSpace(query))
                throw new ArgumentNullException(nameof(query));

            Query = query;
            Build(token);
        }

        public string Query { get; }
        public IEnumerable<Post> Posts { get; private set; }

        protected override void Workload()
        {
            try
            {
                Posts = ServiceSession.GlobalScope.Session.Journal.Query(Query).ToArray();
                Status = 0;
            }
            catch (Exception err)
            {
                ServiceSession.GlobalScope.ReportError(err);
            }
        }
    }

}
