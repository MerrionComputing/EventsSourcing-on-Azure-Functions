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


        /// <summary>
        /// A projection was completed
        /// </summary>
        /// <param name="targetEntity">
        /// The entity on which event stream the projection ran
        /// </param>
        /// <param name="projectionType">
        /// The type of the projection
        /// </param>
        /// <param name="asOfSequenceNumber">
        /// The sequence number of the last event read in processing the projection
        /// </param>
        /// <param name="asOfDate">
        /// The as-of date up to which the rpojection ran
        /// </param>
        /// <param name="currentValue">
        /// The ending projection results when the projection completed
        /// </param>
        /// <param name="commentary">
        /// (Optional) Additional commentary to pass with the notification
        /// </param>
        Task ProjectionCompleted(IEventStreamIdentity targetEntity,
            string projectionType,
            int asOfSequenceNumber,
            Nullable<DateTime> asOfDate,
            object currentValue,
            string commentary = @"");


        /// <summary>
        /// A classification completed
        /// </summary>
        /// <param name="targetEntity">
        /// The entity over which the classification process was run
        /// </param>
        /// <param name="classificationType">
        /// The type of the classification
        /// </param>
        /// <param name="parameters">
        /// Any additional parameters used when processing the classification
        /// </param>
        /// <param name="asOfSequenceNumber">
        /// The sequence number of the last event read in processing the classification
        /// </param>
        /// <param name="asOfDate">
        /// The as-of date up to which the classification was ran
        /// </param>
        /// <param name="commentary">
        /// (Optional) Additional commentary to pass with the notification
        /// </param>
        Task ClassificationCompleted(IEventStreamIdentity targetEntity,
            string classificationType,
            Dictionary<string, object> parameters,
            int asOfSequenceNumber,
            Nullable<DateTime> asOfDate,
            ClassificationResponse response,
            string commentary = @"");


        /// <summary>
        /// Notify that a new entity (or event stream) instance was deleted
        /// </summary>
        /// <param name="deletedEntity">
        /// The new entity that was deleted
        /// </param>
        /// <param name="commentary">
        /// (Optional) Additional commentary for the new entity deletion for logging / diagnostics
        /// </param>
        /// <param name="context">
        /// (Optional) The additional context with which the entity was deleted
        /// </param>
        Task ExistingEntityDeleted(IEventStreamIdentity deletedEntity,
            string commentary = @"",
            IWriteContext context = null);

    }
}
