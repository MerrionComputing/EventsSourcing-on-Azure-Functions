using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces
{
    /// <summary>
    /// Interface for any class that can map a classification name to its implementation type
    /// </summary>
    /// <remarks>
    /// This can be used with dependency injection to inject a hard-coded map for faster
    /// application startup, or any custom mapping needed for sharing projections
    /// </remarks>
    public interface IClassificationMaps
    {

        /// <summary>
        /// Create the .NET class for a particular classification type from its name
        /// </summary>
        /// <param name="classificationName">
        /// The "business" name of the classification to map to a .NET class
        /// </param>
        IClassification CreateClassificationClass(string classificationName);

    }
}
