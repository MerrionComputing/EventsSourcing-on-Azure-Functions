using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Implementation.AzureStorage.Table
{
    /// <summary>
    /// A snapshot reader to load classification snapshots
    /// from an Azure table storage
    /// </summary>
    public sealed class TableClassificationSnapshotReader
        : TableClassificationSnapshotBase,
        IClassificationSnapshotReader
    {

        public Task<ClassificationResponse> LoadClassification<TClassification>(ISnapshot snapshot) where TClassification : IClassification, new()
        {
            throw new System.NotImplementedException();
        }


        public TableClassificationSnapshotReader(
            IEventStreamIdentity identity,
            string classificationName,
            string connectionStringName = @"")
            : base(identity,
                  classificationName ,
                  false,
                  connectionStringName)
        {

        }
    }
}
