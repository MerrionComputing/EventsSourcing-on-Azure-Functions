using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces
{ 
    /// <summary>
    /// Interface for any class that can map an event name to its implementation type
    /// </summary>
    /// <remarks>
    /// This can be used with dependency injection to inject a hard-coded map for faster
    /// application startup, or any custom mapping needed for sharing event streams
    /// </remarks>
    public interface IEventMaps
    {

     /// <summary>
     /// Create the .NET class for a particular event type from its name
     /// </summary>
     /// <param name="eventName">
     /// The "business" name of the event to encapsulate in a .NET class
     /// </param>
     IEvent CreateEventClass(string eventName);

    }
}
