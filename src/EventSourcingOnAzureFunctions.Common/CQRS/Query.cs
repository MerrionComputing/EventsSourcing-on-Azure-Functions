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
                _queryName = attribute.QueryName;
                _uniqueIdentifier = attribute.UniqueIdentifier;
            }
        }
    }
}
