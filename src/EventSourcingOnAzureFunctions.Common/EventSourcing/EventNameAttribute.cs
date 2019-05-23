using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing
{
    /// <summary>
    /// Attribute to allow an event class to be tagged with the event name
    /// </summary>
    /// <remarks>
    /// This allows the event name to be independent of the language used to read/write it for shared event streams
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class EventNameAttribute
        : Attribute 
    {

        /// <summary>
        /// The business meaningful name of the event held in this class
        /// </summary>
        public string Name { get; }


        public EventNameAttribute(string eventName)
        {
            Name = eventName;
        }


        public static string GetEventName(Type eventType)
        {
            foreach (EventNameAttribute item in eventType.GetCustomAttributes(typeof(EventNameAttribute), true))
            {
                return item.Name;
            }

            // fall back on type full name
            return eventType.FullName;
        }



    }
}
