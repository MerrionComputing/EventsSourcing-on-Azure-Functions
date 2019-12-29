using System;

using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.Azure.WebJobs.Description;

namespace EventSourcingOnAzureFunctions.Common.Binding
{
    /// <summary>
    /// An attribute to mark a query to be run by a serverless function
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    [Binding]
    public class QueryAttribute
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

        private readonly string _uniqueIdentifier;
        /// <summary>
        /// The unique instance of the query to run
        /// </summary>
        /// <remarks>
        /// If this is not set then a new GUID will be used instead
        /// </remarks>
        [AutoResolve]
        public string UniqueIdentifier
        {
            get
            {
                return _uniqueIdentifier;
            }
        }

        // Note: The parameter names need to match the property names (except for the camelCase) because 
        // the autoresolve uses this fact to perform the instantiation
        public QueryAttribute(string domainName, 
            string queryName,
            string uniqueIdentifier = @"")
        {
            _domainName = domainName;
            _queryName = queryName;
            if (string.IsNullOrWhiteSpace(uniqueIdentifier ) )
            {
                _uniqueIdentifier = Guid.NewGuid().ToString("D");
            }
            else
            {
                _uniqueIdentifier = uniqueIdentifier;
            }
        }

    }
}
