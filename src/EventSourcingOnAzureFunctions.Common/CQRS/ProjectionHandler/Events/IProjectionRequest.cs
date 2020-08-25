using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.CQRS.ProjectionHandler.Events
{
    public interface IProjectionRequest
    {

        /// <summary>
        /// The domain name of the event stream over which the projection is 
        /// to be run
        /// </summary>
        string ProjectionDomainName { get; set; }

        /// <summary>
        /// The entity type for which the projection will be run
        /// </summary>
        string ProjectionEntityTypeName { get; set; }

        /// <summary>
        /// The unique instance of the event stream over which the 
        /// projection should run
        /// </summary>
        string ProjectionInstanceKey { get; set; }

        /// <summary>
        /// The name of the projection to run over that event stream
        /// </summary>
        string ProjectionTypeName { get; set; }

        /// <summary>
        /// The date up-to which we want the projection to be run
        /// </summary>
        Nullable<DateTime> AsOfDate { get; set; }

        /// <summary>
        /// An unique identifier set by the caller to trace this projection operation
        /// </summary> 
        string CorrelationIdentifier { get; set; }
    }
}
