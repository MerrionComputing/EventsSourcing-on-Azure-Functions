using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventSourcingOnAzureFunctions.Common.EventSourcing;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces
{
    /// <summary>
    /// A writer to save a classification snapshot so that the 
    /// classification processor can start form the last event 
    /// covered by the snapshot
    /// </summary>
    public interface IClassificationSnapshotWriter
    {

        /// <summary>
        /// Write a snapshot from a classification
        /// </summary>
        /// <param name="snapshot">
        /// The identification of the stream and sequence of the snapshot
        /// </param>
        /// <param name="state">
        /// The classification state as at that point
        /// </param>
        Task WriteSnapshot(ISnapshot snapshot, ClassificationResponse state);

    }
}
