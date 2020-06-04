using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingOnAzureFunctions.Common.CQRS.ProjectionHandler.Functions
{
    /// <summary>
    /// Event Grid triggered functions used to run projections for commands and queries
    /// </summary>
    /// <remarks>
    /// There are separate functions for handling projections for commands and queries as,
    /// although they are very similar, we might want to separate them completely
    /// </remarks>
    public class ProjectionHandlerFunctions
    {

    }
}
