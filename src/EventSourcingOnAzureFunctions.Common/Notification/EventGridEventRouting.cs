using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.Notification
{
    /// <summary>
    /// A wrapper for an event to specify where it is to be 
    /// sent to
    /// </summary>
    public class EventGridEventRouting
    {

        public string EventType { get; set; }

        public string Subject { get; set; }

        public string EventGridTopicEndpoint { get; set; }

    }
}
