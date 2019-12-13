using EventSourcingOnAzureFunctions.Common.EventSourcing;
using System;

namespace EventSourcingOnAzureFunctions.Common.CQRS.QueryHandler.Events
{
    /// <summary>
    /// A multi-step (event stream backed) query has completed
    /// </summary>
    /// <remarks>
    /// Once a query is tagged as completed it cannot be reopened so this 
    /// should be the last event in the event stream
    /// </remarks>
    [EventName("Query Completed")]
    public class Completed
    {
        /// <summary>
        /// The date/time the query was completed by the system
        /// </summary>
        public DateTime DateCompleted { get; set; }

        /// <summary>
        /// Commentary on the query that has completed
        /// </summary>
        public string Notes { get; set; }

    }
}
