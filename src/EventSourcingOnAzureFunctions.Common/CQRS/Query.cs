using EventSourcingOnAzureFunctions.Common.Binding;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.CQRS
{
    /// <summary>
    /// A wrapper class used to trigger and manage a query orchestration
    /// </summary>
    public class Query
    {

        private readonly string _domainName;
        /// <summary>
        /// The business domain in which the query is located
        /// </summary>
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
        public string QueryName
        {
            get
            {
                return _queryName;
            }
        }

        private readonly string _uniqueIdentifier;
        /// <summary>
        /// The unique instance of the command to run
        /// </summary>
        public string UniqueIdentifier
        {
            get
            {
                return _uniqueIdentifier;
            }
        }

        public Query(QueryAttribute attribute)
        {
            if (null != attribute )
            {
                _domainName = attribute.DomainName;
                _queryName = attribute.QueryName;
                _uniqueIdentifier = attribute.UniqueIdentifier;
            }
        }


        /// <summary>
        /// Make a "queries" domain for the given top level domain
        /// </summary>
        /// <param name="domainName">
        /// The top level (business) domain
        /// </param>
        /// <remarks>
        /// This allows different domains' query names to be unique even if
        /// the base query names are not
        /// </remarks>
        public static string MakeDomainQueryName(string domainName)
        {
            if (!string.IsNullOrWhiteSpace(domainName))
            {
                return domainName.Trim() + @".Query";
            }
            else
            {
                return "Query";
            }
        }
    }
}
