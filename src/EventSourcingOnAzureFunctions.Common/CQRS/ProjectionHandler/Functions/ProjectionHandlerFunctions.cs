using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using System;
using System.Threading.Tasks;

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

        /// <summary>
        /// A projection has been requested in processing a query.  This
        /// function will run it and attach the result back to the query
        /// event stream when complete
        /// </summary>
        /// <param name="eventGridEvent">
        /// The event grid notification that triggered the request for the
        /// projection to be run
        /// </param>
        /// <returns></returns>
        [FunctionName(nameof(OnQueryProjectionHandler))]
        public static Task OnQueryProjectionHandler([EventGridTrigger] EventGridEvent eventGridEvent)
        {

            throw new NotImplementedException();
        }

        /// <summary>
        /// A projection has been requested in processing a command.  This
        /// function will run it and attach the result back to the command
        /// event stream when complete
        /// </summary>
        /// <param name="eventGridEvent">
        /// The event grid notification that triggered the request for the
        /// projection to be run
        /// </param>
        /// <returns></returns>
        [FunctionName(nameof(OnCommandProjectionHandler))]
        public static Task OnCommandProjectionHandler([EventGridTrigger] EventGridEvent eventGridEvent)
        {

            throw new NotImplementedException();
        }
    }
}
