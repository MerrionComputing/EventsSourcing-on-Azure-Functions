using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces
{
    /// <summary>
    /// A projection which supports saving a state to, and reading a state from, a snapshot
    /// </summary>
    public interface IProjectionWithSnapshots
        : IProjection 
    {

        /// <summary>
        /// Read the current state of a projection from a snapshot 
        /// </summary>
        /// <param name="sequenceNumber">
        /// The sequence number at which the snapshot was taken
        /// </param>
        /// <param name="asOfDate">
        /// The as-of date at which the snapshot was taken
        /// </param>
        /// <param name="snapshot">
        /// The raw data object of the state as at when the snapshot was taken
        /// </param>
        void FromSnapshot(int sequenceNumber,
            Nullable<DateTime> asOfDate,
            object snapshot);

        /// <summary>
        /// Turn the current state of this projection into a snapshot object to be saved
        /// </summary>
        object ToSnapshot();

    }
}
