using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces
{

    /// <summary>
    /// Marker interface to denote anything as being a projection over the given event stream
    /// </summary>
    public interface IProjection
    {

        /// <summary>
        /// Does this projection use snapshots to save the latest state or does it need to rebuild the entire projection every time?
        /// </summary>
        bool SupportsSnapshots { get; }

        /// <summary>
        /// The current sequence number of the last event the projection ran to
        /// </summary>
        int CurrentSequenceNumber { get; }

        /// <summary>
        /// Called when a projection processor has handled an event 
        /// </summary>
        /// <param name="handledEventSequenceNumber">
        /// The sequence number of the event that has been competed - this allows the projection to keep 
        /// track of where in the event stream it has got to
        /// </param>
        void MarkEventHandled(int handledEventSequenceNumber);

        /// <summary>
        /// The current as-of date for this projection
        /// </summary>
        /// <remarks>
        /// This is only updated where an event is processed that has an as-of date field as part of its data properties
        /// </remarks>
        Nullable<DateTime> CurrentAsOfDate { get; }


        /// <summary>
        /// Does the projection handle the data for the given event type
        /// </summary>
        /// <param name="eventTypeName">
        /// The full name of the event type containing the data that may or may not be handled by the projection
        /// </param>
        /// <returns></returns>
        bool HandlesEventType(string eventTypeName);

        /// <summary>
        /// Perform whatever processing is required to handle the specific event
        /// </summary>
        /// <param name="eventTypeName">
        /// The full name of the event type containing the data for the event
        /// </param>
        /// <param name="eventToHandle">
        /// The specific event to handle and perform whatever processing is required
        /// </param>
        void HandleEvent(string eventTypeName, object eventToHandle);


        /// <summary>
        /// An event was read by the underlying event reader (whether it is handled or not)
        /// </summary>
        /// <param name="sequenceNumber">
        /// The sequence number of the event read
        /// </param>
        /// <param name="asOfDate">
        /// If the event has an "effective date" this is passed in here
        /// </param>
        /// <remarks>
        /// This is used for logging or debugging
        /// </remarks>
        void OnEventRead(int sequenceNumber, DateTime? asOfDate);

    }
}
