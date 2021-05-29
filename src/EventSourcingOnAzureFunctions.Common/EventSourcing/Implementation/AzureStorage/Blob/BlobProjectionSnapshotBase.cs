using EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.AppendBlob;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Blob
{
    public class BlobSnapshotBase
    {

        /// <summary>
        /// Turn the given snapshot definition into an unique filename
        /// </summary>
        /// <param name="snapshot">
        /// The snapshot definition to be saved or read
        /// </param>
        /// <remarks>
        /// The filename is the sequence number with the rest of the snapshot definition
        /// going into the folder path
        /// </remarks>
        public static string MakeSnapshotFilename(ISnapshot snapshot)
        {
            if (snapshot != null)
            {
                return BlobEventStreamBase.MakeValidStorageFolderName($"{snapshot.CurrentSequenceNumber}");
            }
            return string.Empty;
        }

        /// <summary>
        /// Make a folder path in which a snapshot for a projection can be saved
        /// </summary>
        /// <param name="snapshot"></param>
        /// <returns></returns>
        public static string MakeSnapshotFolder(ISnapshot snapshot)
        {
            if (snapshot != null)
            {
                return BlobEventStreamBase.MakeValidStorageFolderName($"{snapshot.DomainName}/{snapshot.EntityTypeName}/{snapshot.InstanceKey}");
            }
            return string.Empty;
        }

    }
}
