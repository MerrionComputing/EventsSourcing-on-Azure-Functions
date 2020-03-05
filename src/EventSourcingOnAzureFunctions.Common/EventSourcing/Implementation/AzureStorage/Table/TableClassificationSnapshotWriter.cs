using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Table
{
    /// <summary>
    /// A snapshot writer to persist a classification snapshot
    /// into an Azure table storage
    /// </summary>
    public sealed class TableClassificationSnapshotWriter
        : TableClassificationSnapshotBase,
        IClassificationSnapshotWriter
    {


        public Task WriteSnapshot(ISnapshot snapshot, ClassificationResponse state)
        {
            throw new System.NotImplementedException();
        }


        public TableClassificationSnapshotWriter(
                IEventStreamIdentity identity,
                string classificationName,
                string connectionStringName = @"")
            : base(identity,
                  classificationName,
                  true,
                  connectionStringName)
        {

        }
    }
}
