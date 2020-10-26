using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;

namespace EventSourcingOnAzureFunctions.Common.Notification
{
    /// <summary>
    /// A notification dispatcher that does not do anything
    /// </summary>
    /// <remarks>
    /// This can be used in unit testing or if you have an application that you do not want to 
    /// do any notification
    /// </remarks>
    public class NullNotificationDispatcher
        : INotificationDispatcher
    {

        /// <summary>
        /// The name by which this notification dispatcher is known
        /// </summary>
        public string Name => nameof(NullNotificationDispatcher);

        public Task NewEntityCreated(IEventStreamIdentity newEntity,
            string commentary=@"",
            IWriteContext context = null)
        {
            // do nothing
            return Task.CompletedTask;
        }

        public Task NewEventAppended(IEventStreamIdentity targetEntity, string eventType, 
            int sequenceNumber,
            string commentary = @"",
            object messagePayload = null,
            IWriteContext context = null)
        {
            // do nothing
            return Task.CompletedTask;
            
        }

        public Task ProjectionCompleted(IEventStreamIdentity targetEntity, 
            string projectionType, 
            int asOfSequenceNumber, 
            DateTime? asOfDate, 
            object currentValue, 
            string commentary = "")
        {
            // do nothing
            return Task.CompletedTask;
        }

        public Task ClassificationCompleted(IEventStreamIdentity targetEntity, 
            string classificationType, 
            Dictionary<string, object> parameters, 
            int asOfSequenceNumber, 
            DateTime? asOfDate, 
            ClassificationResponse response, 
            string commentary = "")
        {
            // do nothing
            return Task.CompletedTask;
        }

        public Task ExistingEntityDeleted(IEventStreamIdentity deletedEntity, 
            string commentary = "", 
            IWriteContext context = null)
        {
            // do nothing
            return Task.CompletedTask;
        }
    }
}
