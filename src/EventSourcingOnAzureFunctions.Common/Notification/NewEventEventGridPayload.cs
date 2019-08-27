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

        public NewEventEventGridPayload()
        {
        }
    }
}
