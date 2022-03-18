using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;

namespace EventSourcingOnAzureFunctions.Common.CQRS.Common.Listener
{
    /// <summary>
    /// A listener that tracks progress and triggers steps for a query instance
    /// </summary>
    internal sealed class QueryListener
        : CQRSListenerBase
    {

        // Orchestration steps
        // "Who" - deciding which entities to include in the query (classifiers)
        // "What / When" - running projection over those entites
        // "What next" - what action to perform over the returned result set
        // "Notify" - return the result to every registered interested party (target) that the query has completed

        public QueryListener(string domainName,
            string queryName,
            string instanceIdentifier) 
            : base("Query", domainName , queryName , instanceIdentifier )
        {
        }

    }
}
