using Microsoft.Azure.WebJobs.Description;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.Binding
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    [Binding]
    public sealed class QueryTriggerAttribute
        : Attribute
    {

        private readonly string _domainName;
        /// <summary>
        /// The business domain in which the query is located
        /// </summary>
        [AutoResolve]
        public string DomainName
        {
            get
            {
                return _domainName;
            }
        }

        private readonly string _queryName;
        /// <summary>
        /// The name of the query to run
        /// </summary>
        [AutoResolve]
        public string QueryName
        {
            get
            {
                return _queryName;
            }
        }


    }
}
