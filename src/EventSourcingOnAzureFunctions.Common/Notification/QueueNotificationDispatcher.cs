using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using Azure.Storage.Queues;
using System.Threading.Tasks;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.AppendBlob;

namespace EventSourcingOnAzureFunctions.Common.Notification
{
    /// <summary>
    /// Common routines for sending out notifications via named queues when an event sourcing 
    /// event has occured
    /// </summary>
    /// <remarks> 
    /// Notifications are raised when a new entity is created (i.e. when a new event strem is created) and
    /// when a new event is appended to an event stream
    /// </remarks>
    public sealed class QueueNotificationDispatcher
        : INotificationDispatcher
    {

        // Special queue names for CQRS processing
        public const string QUEUE_QUERY_PROJECTIONS = "query-projection-requests";
        public const string QUEUE_QUERY_CLASSIFICATIONS = "query-classification-requests";
        public const string QUEUE_COMMAND_PROJECTIONS = "command-projection-requests";
        public const string QUEUE_COMMAND_CLASSIFICATIONS = "query-classification-requests";


        public Task NewEntityCreated(IEventStreamIdentity newEntity, 
            string commentary = "", 
            IWriteContext context = null)
        {
            throw new NotImplementedException();
        }

        public Task NewEventAppended(IEventStreamIdentity targetEntity, 
            string eventType, 
            int sequenceNumber, 
            string commentary = "", 
            object eventPayload = null, 
            IWriteContext context = null)
        {
            throw new NotImplementedException();
        }

        public Task ExistingEntityDeleted(IEventStreamIdentity deletedEntity, 
            string commentary = "", 
            IWriteContext context = null)
        {
            throw new NotImplementedException();
        }





        public Task ProjectionCompleted(IEventStreamIdentity targetEntity, 
            string projectionType, 
            int asOfSequenceNumber, 
            DateTime? asOfDate, 
            object currentValue, 
            string commentary = "")
        {
            throw new NotImplementedException();
        }

        public Task ClassificationCompleted(IEventStreamIdentity targetEntity, 
            string classificationType, 
            Dictionary<string, object> parameters, 
            int asOfSequenceNumber, 
            DateTime? asOfDate, 
            ClassificationResponse response, 
            string commentary = "")
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Turn the identity of an event stream backed entity into the name of a 
        /// queue down which the notifications about that entity should be posted
        /// </summary>
        /// <param name="targetEntity">
        /// The entity for which a notification has occurred
        /// </param>
        /// <returns></returns>
        public static string MakeQueueName(IEventStreamIdentity targetEntity)
        {
            if (targetEntity != null)
            {
                string basename= BlobEventStreamBase.MakeValidStorageFolderName($"{targetEntity.DomainName}-{targetEntity.EntityTypeName}");
                // Queues also cannot start with a number
                return basename.TrimStart(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });
            }

            // The target that applies if the entity cannoy be identified
            return $"unidentified-entities";
        }
    }
}
