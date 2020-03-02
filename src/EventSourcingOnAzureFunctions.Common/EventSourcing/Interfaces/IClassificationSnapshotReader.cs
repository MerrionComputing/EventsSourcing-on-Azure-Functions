using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces
{
    /// <summary>
    /// Reader to load snapshots of a classification
    /// </summary>
    public interface IClassificationSnapshotReader
    {

        /// <summary>
        /// Read the snapshot of the given classification type closest to the 
        /// requested snapshot location
        /// </summary>
        /// <typeparam name="TClassification">
        /// The specific type of classification to load - this is used to locate
        /// the snapshot to read
        /// </typeparam>
        /// <param name="snapshot">
        /// The entity instance and sequence number to get the snapshot from
        /// (If no sequence number is specified, get the latest snapshot)
        /// </param>
        Task<ClassificationResponse> LoadClassification<TClassification>(ISnapshot snapshot) where TClassification : IClassification, new();

    }
}
