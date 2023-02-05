
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces
{
    /// <summary>
    /// Definition for any implementation that can read events from an event stream
    /// </summary>
    public interface IEventStreamReader
        : IEventStreamIdentity
    {

        /// <summary>
        /// Does the underlying event stream for this reader exists
        /// </summary>
        Task<bool> Exists();

        /// <summary>
        /// Get the entire event stream for a given event stream identity
        /// </summary>
        Task<IEnumerable<IEvent>> GetAllEvents();


        /// <summary>
        /// Gets the event stream for a given event stream identity from a given starting place
        /// </summary>
        /// <param name="StartingSequenceNumber">
        /// The sequence number from which to get the events - if zero then all the events will be returned
        /// </param>
        /// <param name="effectiveDateTime">
        /// </param>
        /// <returns>
        /// This is used in scenario where we are starting from a given snapshot version
        /// </returns>
        Task<IEnumerable<IEvent>> GetEvents(int StartingSequenceNumber = 0,
            DateTime? effectiveDateTime = null);

        

        /// <summary>
        /// Gets the event stream with the additional context of the events for a given event stream identity from a given starting place
        /// </summary>
        /// <param name="StartingSequenceNumber">
        /// The sequence number from which to get the events - if zero then all the evenst will be returned
        /// </param>
        /// <param name="effectiveDateTime">
        /// </param>
        /// <returns>
        /// This is typically only used for audit trails as all business functionality should depend on the event data alone
        /// </returns>
        Task<IEnumerable<IEventContext>> GetEventsWithContext(int StartingSequenceNumber = 0,
            DateTime? effectiveDateTime = null);

        /// <summary>
        /// Get all of the unique instances of this domain/entity type
        /// </summary>
        /// <param name="asOfDate">
        /// (Optional) The date as of which to get all the instance keys
        /// </param>
        /// <remarks>
        /// This is to allow for set-based functionality
        /// </remarks>    
        Task<IEnumerable<string>> GetAllInstanceKeys(DateTime? asOfDate);
    }
}
