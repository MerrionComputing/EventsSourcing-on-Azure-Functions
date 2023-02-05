using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Blob
{
    /// <summary>
    /// Writer to save projection snapshots in an Azure storage blob
    /// </summary>
    public sealed class BlobProjectionSnapshotWriter
        : BlobSnapshotBase, 
          IProjectionSnapshotWriter
    {


        public async Task WriteSnapshot<TProjection>(ISnapshot snapshot, 
            TProjection state) where TProjection : IProjectionWithSnapshots
        {
            if (snapshot != null)
            {
                if (state != null)
                {
                    // Create a filename from the snapshot
                    string filename = MakeSnapshotFilename(snapshot);
                    string filepath = MakeSnapshotFolder(snapshot);


                }

            }
            throw new NotImplementedException();
        }


    }
}
