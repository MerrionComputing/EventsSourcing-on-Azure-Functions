using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using Newtonsoft.Json;

namespace EventSourcingOnAzureFunctions.Common.Notification
{
    public sealed class DeletedEntityEventGridPayload
         : IEventStreamIdentity
    {
        public const string EVENT_TYPE = @"eventsourcing.DeletedEntity";
        public const string DATA_VERSION = "1.0"; // Update if the members change

        /// <summary>
        /// The notification instance identifier (for logical idempotency checking)
        /// </summary>
        [JsonProperty(PropertyName = "notificationId")]
        public string NotificationId { get; set; }

        /// <summary>
        /// The domain in which the new entity was created
        /// </summary>
        [JsonProperty(PropertyName = "domainName")]
        public string DomainName { get; set; }

        /// <summary>
        /// The type of entity that was created 
        /// </summary>
        [JsonProperty(PropertyName = "entityTypeName")]
        public string EntityTypeName { get; set; }

        /// <summary>
        /// The unique identifier of the new entity instance that was created
        /// </summary>
        [JsonProperty(PropertyName = "instanceKey")]
        public string InstanceKey { get; set; }

        /// <summary>
        /// Comments that can be passed for logging / diagnostic or other purpose
        /// </summary>
        [JsonProperty(PropertyName = "commentary")]
        public string Commentary { get; set; }


        [JsonProperty(PropertyName = "context")]
        public IWriteContext Context { get; set; }

        /// <summary>
        /// Empty constructor for serialisation
        /// </summary>
        public DeletedEntityEventGridPayload()
        {
        }

        public static DeletedEntityEventGridPayload Create(IEventStreamIdentity deletedEntity,
            string notificationId = @"",
            string commentary = @"",
            IWriteContext context = null)
        {

            if (null == deletedEntity)
            {
                throw new ArgumentNullException(nameof(deletedEntity));
            }

            // Default the notification id if not are provided
            if (string.IsNullOrEmpty(notificationId))
            {
                notificationId = Guid.NewGuid().ToString("N");
            }

            return new DeletedEntityEventGridPayload()
            {
                DomainName = deletedEntity.DomainName,
                EntityTypeName = deletedEntity.EntityTypeName,
                InstanceKey = deletedEntity.InstanceKey,
                NotificationId = notificationId,
                Commentary = commentary,
                Context = context
            };

        }

        /// <summary>
        /// Make an event type name for the deleted entity notification message
        /// </summary>
        /// <param name="newEntity">
        /// The newly created entity
        /// </param>
        public static string MakeEventTypeName(IEventStreamIdentity deletedEntity)
        {
            if (null == deletedEntity)
            {
                return EVENT_TYPE;
            }

            return $"{deletedEntity.DomainName}.{deletedEntity.EntityTypeName}.DeletedEntity";
        }

    }
}
