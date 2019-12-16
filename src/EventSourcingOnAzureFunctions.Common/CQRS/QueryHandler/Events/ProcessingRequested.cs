using EventSourcingOnAzureFunctions.Common.EventSourcing;
using System;

namespace EventSourcingOnAzureFunctions.Common.CQRS.QueryHandler.Events
{

    /// <summary>
    /// The query has been set up ready to run and now the actual processing
    /// is requested
    /// </summary>
    /// <remarks>
    /// This may be subscribed to by the query handlers 
    /// </remarks>
    [EventName("Query Processing Requested")]
    public class QueryProcessingRequested
    {

        /// <summary>
        /// If there is a preference for a region or instance to process this 
        /// query it can be set in this property
        /// </summary>
        /// <remarks>
        /// In most cases this is not needed
        /// </remarks>
        public string AffinityPreference { get; set; }

    }
}
