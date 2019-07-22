using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing
{
    /// <summary>
    /// Settings in the [EventStreamSettings] section of the JSON settings file(s)
    /// </summary>
    public sealed class EventMaps
    {

    }

    /// <summary>
    /// A single mapping between an event name and the CLR class used to encapsulate it
    /// </summary>
    public sealed class EventMap
    {

        /// <summary>
        /// The unique event name as it is known to the application
        /// </summary>
        string EventName { get; }

        /// <summary>
        /// The name of the CLR class that implements that event
        /// </summary>
        string EventImplementationClassName { get; }

    }
}
