using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces
{
    /// <summary>
    /// Common functionality for all snapshots - of projections or of
    /// classifiers
    /// </summary>
    /// <remarks>
    /// This builds on the event stream identity which uniquely identifies
    /// the entity that was snapshotted
    /// </remarks>
    public interface ISnapshot
        : IEventStreamIdentity 
    {

        /// <summary>
        /// The current sequence number of the last event that was read in 
        /// producing the state that is being persisted to this snapshot
        /// </summary>
        /// <remarks> 
        /// The snapshot can be used by taking this number + 1 as the start
        /// of the stream of events to be considered "new"
        /// </remarks>
        int CurrentSequenceNumber { get; }

    }
}
