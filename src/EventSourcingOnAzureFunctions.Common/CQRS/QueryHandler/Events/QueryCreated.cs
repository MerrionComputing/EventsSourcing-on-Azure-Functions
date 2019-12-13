using EventSourcingOnAzureFunctions.Common.EventSourcing;
using System;


namespace EventSourcingOnAzureFunctions.Common.CQRS.QueryHandler.Events
{
    /// <summary>
    /// A new query instance was created
    /// </summary>
    /// <remarks>
    /// This provides additional information above and beyond what is available in the query event 
    /// stream created notification
    /// </remarks>
    [EventName("Query Created")]
    public class Created
    {
        /// <summary>
        /// The date/time the new query was logged by the system
        /// </summary>
        public DateTime DateLogged { get; set; }

        /// <summary>
        /// If the system that initiated this query has its own way of identifying command instances
        /// this will be recorded here
        /// </summary>
        public string ExternalSystemUniqueIdentifier { get; set; }

        /// <summary>
        /// For queries that rely on authorisation this is the token passed in to test
        /// for the authorisation process
        /// </summary>
        public string AuthorisationToken { get; set; }

    }
}
