using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;


namespace EventSourcingOnAzureFunctions.Common.CQRS.ProjectionHandler.Events
{

    /// <summary>
    /// A projection was requested to be executed as part of a command or query
    /// </summary>
    /// <remarks>
    /// This event does not need to store who requested the projection as that can 
    /// be derived from whatever event stream it is appended to
    /// </remarks>
    [EventName("Projection Requested")]
    public class ProjectionRequested
        : IProjectionRequest,
        IEquatable<IProjectionRequest>,
        IEquatable<ProjectionRequested >,
        IEquatable<ProjectionValueReturned > 
    {

        /// <summary>
        /// The domain name of the event stream over which the projection is 
        /// to be run
        /// </summary>
        public string ProjectionDomainName { get; set; } 

        /// <summary>
        /// The entity type for which the projection will be run
        /// </summary>
        public string ProjectionEntityTypeName { get; set; }

        /// <summary>
        /// The unique instance of the event stream over which the 
        /// projection should run
        /// </summary>
        public string ProjectionInstanceKey { get; set; }

        /// <summary>
        /// The name of the projection to run over that event stream
        /// </summary>
        public string ProjectionTypeName { get; set; }

        /// <summary>
        /// The date up-to which we want the projection to be run
        /// </summary>
        public Nullable<DateTime> AsOfDate { get; set; }

        /// <summary>
        /// The date/time the projection request was logged by the system
        /// </summary>
        public DateTime DateLogged { get; set; }

        /// <summary>
        /// An unique identifier set by the caller to trace this projection operation
        /// </summary> 
        public string CorrelationIdentifier { get; set; }

        #region Equality comparison
        public bool Equals(ProjectionRequested other)
        {
            if (null != other )
            {
                if (other.ProjectionDomainName.Equals(ProjectionDomainName) )
                {
                    if (other.ProjectionEntityTypeName.Equals(ProjectionEntityTypeName)  )
                    {
                        if (other.ProjectionInstanceKey.Equals(ProjectionInstanceKey)  )
                        {
                            if (other.AsOfDate.HasValue  )
                            {
                                if (other.AsOfDate.Equals(AsOfDate )  )
                                {
                                    // Everything matched including the as-of date
                                    return true;
                                }
                            }
                            else
                            {
                                if (! AsOfDate.HasValue  )
                                {
                                    // Everything matched and the as-of date is empty
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        public bool Equals(ProjectionValueReturned other)
        {
            if (null != other)
            {
                if (other.ProjectionDomainName.Equals(ProjectionDomainName))
                {
                    if (other.ProjectionEntityTypeName.Equals(ProjectionEntityTypeName))
                    {
                        if (other.ProjectionInstanceKey.Equals(ProjectionInstanceKey))
                        {
                            if (other.AsOfDate.HasValue)
                            {
                                if (other.AsOfDate.Equals(AsOfDate))
                                {
                                    // Everything matched including the as-of date
                                    return true;
                                }
                            }
                            else
                            {
                                if (!AsOfDate.HasValue)
                                {
                                    // Everything matched and the as-of date is empty
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        public bool Equals(IProjectionRequest other)
        {
            if (null != other)
            {
                if (other.ProjectionDomainName.Equals(ProjectionDomainName))
                {
                    if (other.ProjectionEntityTypeName.Equals(ProjectionEntityTypeName))
                    {
                        if (other.ProjectionInstanceKey.Equals(ProjectionInstanceKey))
                        {
                            if (other.AsOfDate.HasValue)
                            {
                                if (other.AsOfDate.Equals(AsOfDate))
                                {
                                    // Everything matched including the as-of date
                                    return true;
                                }
                            }
                            else
                            {
                                if (!AsOfDate.HasValue)
                                {
                                    // Everything matched and the as-of date is empty
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
        #endregion
    }
}
