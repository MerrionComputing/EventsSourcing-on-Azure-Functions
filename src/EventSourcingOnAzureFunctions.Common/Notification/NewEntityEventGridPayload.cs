using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.Notification
{
    /// <summary>
    /// Event Grid message payload for a new entity creation notification
    /// </summary>
    public class NewEntityEventGridPayload
        : IEventStreamIdentity
    {

        public const string EVENT_TYPE = @"eventsourcing.NewEntity";
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

        /// <summary>
        /// Empty constructor for serialisation
        /// </summary>
        public NewEntityEventGridPayload()
        {
        }

        public static NewEntityEventGridPayload Create(IEventStreamIdentity newEntity,
            string notificationId = @"",
            string commentary = @"")
        {

            if (null == newEntity )
            {
                throw new ArgumentNullException(nameof(newEntity)); 
            }

            // Default the notification id if not are provided
            if (string.IsNullOrEmpty(notificationId )  )
            {
                notificationId = Guid.NewGuid().ToString("N");   
            }

            return new NewEntityEventGridPayload() {
                DomainName = newEntity.DomainName ,
                EntityTypeName = newEntity.EntityTypeName ,
                InstanceKey = newEntity.InstanceKey ,
                NotificationId = notificationId ,
                Commentary = commentary 
            };

        }
    }
}
