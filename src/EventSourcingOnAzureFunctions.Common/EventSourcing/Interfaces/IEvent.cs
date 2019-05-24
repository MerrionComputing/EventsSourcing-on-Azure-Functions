using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces
{
    /// <summary>
    /// The additional data required for every event 
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// The readable type of this event
        /// </summary>
        string EventTypeName { get; }

         /// <summary>
         /// The underlying business data of the object
         /// </summary>
        object EventPayload { get; }

    }
}
