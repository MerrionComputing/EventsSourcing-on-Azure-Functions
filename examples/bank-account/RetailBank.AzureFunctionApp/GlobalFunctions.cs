using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetailBank.AzureFunctionApp
{
    /// <summary>
    /// Utility, test and debugging functions
    /// </summary>
    public partial class GlobalFunctions
    {

        /// <summary>
        /// Echo whatever is received from the event grid to log so that we can test the 
        /// event grid messages are wired up and working
        /// </summary>
        /// <param name="eventGridEvent"></param>
        /// <param name="log"></param>
        [FunctionName(nameof(EventGridEcho))]
        public static void EventGridEcho([EventGridTrigger]EventGridEvent eventGridEvent, 
            ILogger log)
        {
            if (null != eventGridEvent)
            {
                log.LogInformation($"{eventGridEvent.EventType} :: {eventGridEvent.Subject}");
                log.LogInformation(eventGridEvent.Data.ToString());
            }
            else
            {
                log.LogError($"Event grid event is null");
            }
        }

    }
}
