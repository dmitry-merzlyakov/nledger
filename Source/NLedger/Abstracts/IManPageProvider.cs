using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLedger.Abstracts
{
    /// <summary>
    /// A service provider that shows a man page in an appropriate way
    /// </summary>
    public interface IManPageProvider
    {
        /// <summary>
        /// Show man page and immediately return execution flow
        /// </summary>
        /// <returns>Indicates whether the operation is successfull</returns>
        bool Show();
    }
}
