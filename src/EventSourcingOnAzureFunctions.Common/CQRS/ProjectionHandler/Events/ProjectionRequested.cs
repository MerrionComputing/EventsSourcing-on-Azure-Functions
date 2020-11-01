using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

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

        /// <summary>
        /// Parameterless constructor for serialisation
        /// </summary>
        public ProjectionRequested()
        {
        }

        /// <summary>
        /// Turn a queue notification message back into a projection requested event
        /// </summary>
        /// <param name="queuedMessage">
        /// The message sent by the <see cref="Notification.QueueNotificationDispatcher"/> for the command or query
        /// </param>
        /// <returns>
        /// If the string can be turned into a projection request then it is - otherwise null
        /// </returns>
        /// <remarks>
        /// Message is pipe separated
        /// E|Projection Requested|{command/query id}|{command/query sequence number}|null||{ProjectionDomainName}|{ProjectionEntityTypeName}|{ProjectionInstanceKey}|{ProjectionTypeName}|{AsOfDate}|{CorrelationIdentifier}
        /// </remarks>
        public static ProjectionRequested FromQueuedMessage(string queuedMessage)
        {
            if (! string.IsNullOrWhiteSpace(queuedMessage ) )
            {
                string[] messageParts = queuedMessage.Split('|');
                if (messageParts.Count() > 5 )
                {
                    // Message has parts that can be converted to a projection request
                    ProjectionRequested request = new ProjectionRequested();
                    if (messageParts.Count() >= 6)
                    {
                        request.ProjectionDomainName =  messageParts[5];
                    }
                    if (messageParts.Count() >= 7)
                    {
                        request.ProjectionEntityTypeName = messageParts[6];
                    }
                    if (messageParts.Count() >=8 )
                    {
                        request.ProjectionInstanceKey = messageParts[7];
                    }
                    if (messageParts.Count() >= 9)
                    {
                        request.ProjectionTypeName = messageParts[8];
                    }
                    if (messageParts.Count() >= 10)
                    {
                        //AsOfDate
                        if (! string.IsNullOrWhiteSpace(messageParts[9]) )
                        {
                            DateTime asOf;
                            if (DateTime.TryParse(messageParts[9], out asOf ))
                            {
                                request.AsOfDate = asOf;
                            }
                        }
                    }
                    if (messageParts.Count() >= 11)
                    {
                        request.CorrelationIdentifier = messageParts[10];
                    }

                    return request;
                }
            }
            return null;
        }

        /// <summary>
        /// Turn a projection requested event to the <see cref="Notification.QueueNotificationDispatcher"/> type of message
        /// </summary>
        /// <param name="evtPayload">
        /// The event for a projection being requested
        /// </param>
        /// <returns></returns>
        public static string ToQueueMessage(ProjectionRequested evtPayload)
        {
            if (evtPayload  != null)
            {
                string asOf = "null";
                if (evtPayload.AsOfDate.HasValue )
                {
                    asOf = evtPayload.AsOfDate.Value.ToString("O");
                }
                return $"|{evtPayload.ProjectionDomainName}|{evtPayload.ProjectionEntityTypeName}|{evtPayload.ProjectionInstanceKey}|{evtPayload.ProjectionTypeName}|{asOf}|{evtPayload.CorrelationIdentifier}";
            }
            return "";
        }
    }
}
