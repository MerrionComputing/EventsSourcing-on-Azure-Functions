using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using System;
using System.Threading.Tasks;

namespace EventSourcingOnAzureFunctions.Common.CQRS.ClassifierHandler.Functions
{
    /// <summary>
    /// Event Grid triggered functions used to run classifications for commands and queries
    /// </summary>
    /// <remarks>
    /// There are separate functions for handling classifications for commands and queries as,
    /// although they are very similar, we might want to separate them completely
    /// </remarks>
    public class ClassifierHandlerFunctions
    {

        /// <summary>
        /// A classification has been requested in processing a query.  This
        /// function will run it and attach the result back to the query
        /// event stream when complete
        /// </summary>
        /// <param name="eventGridEvent">
        /// The event grid notification that triggered the request for the
        /// classification to be run
        /// </param>
        [FunctionName(nameof(OnQueryClassificationHandler))]
        public static Task OnQueryClassificationHandler([EventGridTrigger] EventGridEvent eventGridEvent)
        {
            if (eventGridEvent != null)
            {
                // Get the data from the event that describes what classification is requested
                ClassifierRequestedEventGridEventData classifierRequestData = eventGridEvent.Data as ClassifierRequestedEventGridEventData;
                if (classifierRequestData!= null)
                {

                }
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// A classification has been requested in processing a command.  This
        /// function will run it and attach the result back to the command
        /// event stream when complete
        /// </summary>
        /// <param name="eventGridEvent">
        /// The event grid notification that triggered the request for the
        /// classification to be run
        /// </param>
        [FunctionName(nameof(OnCommandClassificationHandler))]
        public static Task OnCommandClassificationHandler([EventGridTrigger] EventGridEvent eventGridEvent)
        {

            throw new NotImplementedException();
        }

    }
}
