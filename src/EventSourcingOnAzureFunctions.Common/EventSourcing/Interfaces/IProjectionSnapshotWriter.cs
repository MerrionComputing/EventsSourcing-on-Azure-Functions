using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces
{    
    
    /// <summary>
    /// A writer to save a projection snapshot so that the 
    /// projection processor can start form the last event 
    /// covered by the snapshot
    /// </summary>
    public interface IProjectionSnapshotWriter
    {

        /// <summary>
        /// Write a snapshot from a projection
        /// </summary>
        /// <param name="snapshot">
        /// The identification of the stream and sequence of the snapshot
        /// </param>
        /// <param name="state">
        /// The projection state as at that point
        /// </param>
        Task WriteSnapshot<TProjection>(ISnapshot snapshot, TProjection state) where TProjection : IProjection;

    }
}
