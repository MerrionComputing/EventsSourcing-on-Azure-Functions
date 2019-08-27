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
    }
}
