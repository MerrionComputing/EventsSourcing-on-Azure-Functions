using EventSourcingOnAzureFunctions.Common.EventSourcing;
using System;

namespace EventSourcingOnAzureFunctions.Common.CQRS.QueryHandler.Events
{
    /// <summary>
    /// A location to which the query results should be sent has been added to 
    /// this query
    /// </summary>
    /// <remarks>
    /// It may be possible to add these in-flight as well
    /// </remarks>
    [EventName("Output Location Set")]
    public class OutputLocationSet
    {

        /// <summary>
        /// The target to return the results to 
        /// </summary>
        /// <remarks>
        /// This can be a URI or other depending on the location type
        /// </remarks>
        public string Location { get; set; }



    }
}
