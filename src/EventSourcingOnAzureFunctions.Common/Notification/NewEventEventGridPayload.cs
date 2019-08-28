using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.Notification
{
    /// <summary>
    /// Event Grid message payload for a new event notification
    /// </summary>
    public class NewEventEventGridPayload
    {

        /// <summary>
        /// The notification instance identifier (for idempotency checking)
        /// </summary>
        [JsonProperty(PropertyName = "instanceId")]
        public string InstanceId { get; set; }


        /// <summary>
        /// The unique identifier of the instance that had an event occur to is
        /// </summary>
        /// <remarks>
        /// The domain and entity type will be in the event subject
        /// </remarks>
        [JsonProperty(PropertyName = "entityUniqueInstanceIdentifier")]
        public string EntityUniqueInstanceIdentifier { get; set; }

        /// <summary>
        /// The type of the event that was appended to the event stream for this entity
        /// </summary>
        [JsonProperty(PropertyName = "eventType")]
        public string EventType { get; set; }

        /// <summary>
        /// The sequence number of the just-added event
        /// </summary>
        [JsonProperty(PropertyName = "sequenceNumber")]
        public int SequenceNumber { get; set; }

        public NewEventEventGridPayload()
        {
        }
    }
}
