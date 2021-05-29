using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Blob
{
    /// <summary>
    /// Class to read a projection snapshot from an Azure storage blob
    /// </summary>
    public sealed class BlobProjectionSnapshotReader
        : BlobSnapshotBase,
          IProjectionSnapshotReader
    {

        /// <summary>
        /// Loads the snapshot record closes to the requested as-of sequence number 
        /// from which to start running a projection
        /// </summary>
        /// <typeparam name="TProjection">
        /// The projection into which to load the initial snapshot
        /// </typeparam>
        /// <param name="snapshot">
        /// The snapshot definition from which to load
        /// </param>
        public async Task<TProjection> LoadProjection<TProjection>(ISnapshot snapshot) where TProjection : IProjectionWithSnapshots, new()
        {
            if (snapshot != null)
            {
                if (snapshot.CurrentSequenceNumber <= 0)
                {
                    // We are just asking for the latest available snapshot

                }
                // Create a filename from the snapshot
                string filename = MakeSnapshotFilename(snapshot);
                string filepath = MakeSnapshotFolder(snapshot);

                TProjection projectionToLoad = new TProjection();
                
                return projectionToLoad;
            }
            throw new NotImplementedException();
        }
    }
}
