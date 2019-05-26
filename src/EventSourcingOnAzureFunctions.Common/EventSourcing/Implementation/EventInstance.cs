using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation
{
    public class EventInstance
        : IEvent
    {

        /// <summary>
        /// The name of this type of event
        /// </summary>
        public string EventTypeName { get; }

        /// <summary>
        /// The business data payload for the event
        /// </summary>
        public object EventPayload { get; }

        internal EventInstance(string typeName,
            object instanceData)
        {
            EventTypeName = typeName;
            EventPayload = instanceData;
        }

        public static IEvent Wrap(object eventPayload)
        {
            // If it already implements IEvent just return that
            IEvent ret = eventPayload as IEvent;
            if (null != ret)
            {
                return ret;
            }
            else
            {
                // Otherwise wrap and return it
                return new EventInstance(EventNameAttribute.GetEventName(eventPayload.GetType()), eventPayload);
            }
    }
}
