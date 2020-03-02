using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces
{
    /// <summary>
    /// Reader to load snapshots of a projection
    /// </summary>
    public interface IProjectionSnapshotReader
    {

        /// <summary>
        /// Read the snapshot of the given projection type closest to the 
        /// requested snapshot location
        /// </summary>
        /// <typeparam name="TProjection">
        /// The specific type of projection to load
        /// </typeparam>
        /// <param name="snapshot">
        /// The entity instance and sequence number to get the snapshot from
        /// (If no sequence number is specified, get the latest snapshot)
        /// </param>
        Task<TProjection> LoadProjection<TProjection>(ISnapshot snapshot) where TProjection : IProjection, new();

    }
}
