using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;

namespace EventSourcingOnAzureFunctions.Common.CQRS.ProjectionHandler.Events
{

    /// <summary>
    /// The result of running a projection has been returned to the requester
    /// </summary>
    [EventName("Projection Value Returned")]
    public class ProjectionValueReturned
        : IEventStreamIdentity,
        IProjectionRequest,
        IEquatable<IProjectionRequest>,
        IEquatable<ProjectionRequested>,
        IEquatable<ProjectionValueReturned>
    {

        /// <summary>
        /// The domain name of the event stream over which the projection was run
        /// </summary>
        public string DomainName { get; set; }

        /// <summary>
        /// The entity type for which the projection was run
        /// </summary>
        public string EntityTypeName { get; set; }

        /// <summary>
        /// The unique instance of the event stream over which the 
        /// projection was run
        /// </summary>
        public string InstanceKey { get; set; }

        /// <summary>
        /// The name of the projection we ran over that event stream
        /// </summary>
        public string ProjectionTypeName { get; set; }

        /// <summary>
        /// The date up-to which we wanted the projection to be run
        /// </summary>
        public Nullable<DateTime> AsOfDate { get; set; }


        /// <summary>
        /// The sequence number of the last event read when running the projection
        /// </summary>
        /// <remarks>
        /// This can be used for concurrency protection
        /// </remarks>
        public int AsOfSequenceNumber { get; set; }

        /// <summary>
        /// The value returned from the projection
        /// </summary>
        public object Value { get; set; }


        /// <summary>
        /// The date/time the projection response was logged by the system
        /// </summary>
        public DateTime DateLogged { get; set; }

        /// <summary>
        /// An unique identifier set by the caller to trace this projection operation
        /// </summary> 
        public string CorrelationIdentifier { get; set; }


        #region Equality comparison
        public bool Equals(ProjectionRequested other)
        {
            if (null != other)
            {
                if (other.DomainName.Equals(DomainName))
                {
                    if (other.EntityTypeName.Equals(EntityTypeName))
                    {
                        if (other.InstanceKey.Equals(InstanceKey))
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

        public bool Equals(ProjectionValueReturned other)
        {
            if (null != other)
            {
                if (other.DomainName.Equals(DomainName))
                {
                    if (other.EntityTypeName.Equals(EntityTypeName))
                    {
                        if (other.InstanceKey.Equals(InstanceKey))
                        {
                            if (other.AsOfSequenceNumber.Equals(AsOfSequenceNumber))
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
            }
            return false;
        }

        public bool Equals(IProjectionRequest other)
        {
            if (null != other)
            {
                if (other.DomainName.Equals(DomainName))
                {
                    if (other.EntityTypeName.Equals(EntityTypeName))
                    {
                        if (other.InstanceKey.Equals(InstanceKey))
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
