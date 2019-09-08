using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common
{
    /// <summary>
    /// Configuration options for the event sourcing on azure extension/library.
    /// </summary>
    public class EventSourcingOnAzureOptions
    {

        /// <summary>
        /// Should this function app raise notifications when a new entity instance
        /// is created
        /// </summary>
        public bool RaiseEntityCreationNotification { get; set; } = false;


        /// <summary>
        /// Should this function app raise notifications when an event is persisted
        /// to an event stream
        /// </summary>
        public bool RaiseEventNotification { get; set; } = false;

        /// <summary>
        /// The SAS key to use when communicating with event grid
        /// </summary>
        public string EventGridKeyValue { get; set; }

        /// <summary>
        /// The event grid topic endpoint used when communication notificatiosn via event grid
        /// </summary>
        public string EventGridTopicEndpoint { get; set; }

        /// <summary>
        /// The name of the app setting containing the key used for authenticating with the Azure Event Grid custom topic at <see cref="EventGridTopicEndpoint"/>.
        /// </summary>
        public string EventGridKeySettingName { get; set; }


        /// <summary>
        /// Gets or sets the Event Grid publish request retry count.
        /// </summary>
        /// <value>The number of retry attempts.
        /// The default is 10
        /// </value>
        public int EventGridPublishRetryCount { get; set; } = 10;

        /// <summary>
        /// Gets orsets the Event Grid publish request retry interval.
        /// </summary>
        /// <value>A <see cref="TimeSpan"/> representing the retry interval. 
        /// The default value is 5 minutes.</value>
        public TimeSpan EventGridPublishRetryInterval { get; set; } = TimeSpan.FromMinutes(5);
    }
}
