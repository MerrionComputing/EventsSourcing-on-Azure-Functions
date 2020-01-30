using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.Notification
{
    /// <summary>
    /// Interface describing the methods used to dispatch notifications
    /// </summary>
    public interface INotificationDispatcher
    {

        /// <summary>
        /// Notify that a new entity (or event stream) instance was created
        /// </summary>
        /// <param name="newEntity">
        /// The new entity that was created
        /// </param>
        /// <param name="commentary">
        /// (Optional) Additional commentary for the new entity creation for logging / diagnostics
        /// </param>
        /// <param name="context">
        /// (Optional) The additional context with which the new entity was written
        /// </param>
        Task NewEntityCreated(IEventStreamIdentity newEntity,
            string commentary = @"",
            IWriteContext  context = null);

        /// <summary>
        /// A new event was appended to an event stream - notify the world
        /// </summary>
        /// <param name="targetEntity">
        /// The entity on which event stream the event was appended
        /// </param>
        /// <param name="eventType">
        /// The type of event that occured
        /// </param>
        /// <param name="sequenceNumber">
        /// The sequence number of the new event that was appended
        /// </param>
        /// <param name="commentary">
        /// (Optional) Additional commentary for the new event for logging / diagnostics
        /// </param>
        /// <param name="eventPayload">
        /// (Optional) The payload content of the event appended to the event stream
        /// </param>
        /// <param name="context">
        /// (Optional) The additional context with which the new event was written
        /// </param>
        Task NewEventAppended(IEventStreamIdentity targetEntity,
            string eventType,
            int sequenceNumber,
            string commentary = @"",
            object eventPayload = null,
            IWriteContext context = null);


        Task ProjectionCompleted(IEventStreamIdentity targetEntity,
            string projectionType,
            int asOfSequenceNumber,
            Nullable<DateTime> asOfDate,
            IEnumerable<ProjectionSnapshotProperty> currentValues,
            string commentary = @"");


        // TODO: Task ClassificationCompleted();
    }
}
