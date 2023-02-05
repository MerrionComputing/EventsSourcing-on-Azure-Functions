using System;
using System.Collections.Generic;
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
        /// Set a named parameter to be used when performing the 
        /// classification
        /// </summary>
        /// <param name="parameterName">
        /// The name of the parameter - this must be unique within the classification
        /// </param>
        /// <param name="parameterValue">
        /// The value to assign to the named parameter
        /// </param>
        void SetParameter(string parameterName, object parameterValue);

        /// <summary>
        /// Get the name-value set of patameters for this classifier
        /// </summary>
        Dictionary<string, object> Parameters { get; }


        /// <summary>
        /// Run the given classification over the underlying event stream
        /// </summary>
        /// <typeparam name="TClassification">
        /// The type of classification to run
        /// </typeparam>
        /// <param name="asOfDate">
        /// If set, only run the classification up until this date/time
        /// </param>
        Task<ClassificationResponse> Classify<TClassification>(DateTime? asOfDate = null) where TClassification : IClassification , new();

        /// <summary>
        /// Run the given classification over the underlying event stream
        /// </summary>
        /// <param name="classificationToRun">
        /// The instance of the classification to run
        /// </param>
        /// <param name="asOfDate">
        /// If set, only run the classification up until this date/time
        /// </param>
        Task<ClassificationResponse> Classify(IClassification classificationToRun, DateTime? asOfDate = null);

        /// <summary>
        /// Get all of the unique instances of this domain/entity type
        /// </summary>
        /// <param name="asOfDate">
        /// (Optional) The date as of which to get all the instance keys
        /// </param>
        /// <remarks>
        /// This is to allow for set-based functionality
        /// </remarks>    
        Task<IEnumerable<string>> GetAllInstanceKeys(DateTime? asOfDate);
    }
}
