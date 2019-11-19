using System;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces
{
    public interface IClassificationProcessor
    {

        /// <summary>
        /// Does the event stream over which this projection is slated to run exist
        /// </summary>
        Task<bool> Exists();

        /// <summary>
        /// Run the given classification over the underlying event stream
        /// </summary>
        /// <typeparam name="TClassification">
        /// The type of classification to run
        /// </typeparam>
        /// <param name="asOfDate">
        /// If set, only run the classification up until this date/time
        /// </param>
        Task<Classification.ClassificationResults> Classify<TClassification>(DateTime? asOfDate = null) where TClassification : IClassification , new();
    }
}
