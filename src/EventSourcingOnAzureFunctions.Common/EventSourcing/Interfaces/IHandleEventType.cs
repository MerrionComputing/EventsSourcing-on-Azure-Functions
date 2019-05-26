using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces
{
    /// <summary>
    /// An interface to make type-safe projections which handle a particular event type
    /// </summary>
    /// <typeparam name="TEventType">
    /// The type of this event to handle
    /// </typeparam>
    /// <remarks>
    /// This can make it easier to code projections by passing on the specific event type onto the handler 
    /// coded specifically for it
    /// </remarks>
    public interface IHandleEventType<TEventType>
    {

        /// <summary>
        /// Handle the given instance of this event type
        /// </summary>
        /// <param name="eventInstance">
        /// The specific instance of this event type with its data properties set
        /// </param>
        void HandleEventInstance(TEventType eventInstance);

    }
}
