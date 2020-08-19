using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces
{
    /// <summary>
    /// Interface for any class that can map a projection name to its implementation type
    /// </summary>
    /// <remarks>
    /// This can be used with dependency injection to inject a hard-coded map for faster
    /// application startup, or any custom mapping needed for sharing projections
    /// </remarks>
    public interface IProjectionMaps
    {

        /// <summary>
        /// Create the .NET class for a particular projection type from its name
        /// </summary>
        /// <param name="projectionName">
        /// The "business" name of the projection to map to a .NET class
        /// </param>
        IProjection CreateProjectionClass(string projectionName);

    }
}
