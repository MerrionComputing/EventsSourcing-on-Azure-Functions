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
    {

        /// <summary>
        /// The notification instance identifier (for logical idempotency checking)
        /// </summary>
        [JsonProperty(PropertyName = "instanceId")]
        public string InstanceId { get; set; }

        /// <summary>
        /// The unique identifier of the instance that was created
        /// </summary>
        [JsonProperty(PropertyName = "entityUniqueInstanceIdentifier")]
        public string EntityUniqueInstanceIdentifier { get; set; }


        public NewEntityEventGridPayload()
        {
        }
    }
}
