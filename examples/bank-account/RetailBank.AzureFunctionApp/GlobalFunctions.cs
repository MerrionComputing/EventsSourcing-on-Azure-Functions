using Microsoft.AspNetCore.Http;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
        public static Task EventGridEcho([EventGridTrigger]EventGridEvent eventGridEvent, 
            ILogger log,
            [SignalR(HubName = "retailbank")]IAsyncCollector<SignalRMessage> signalRMessages)
        {
            if (null != eventGridEvent)
            {
                log.LogInformation($"{eventGridEvent.EventType} :: {eventGridEvent.Subject}");
                log.LogInformation(eventGridEvent.Data.ToString());

                return signalRMessages.AddAsync(
                    new SignalRMessage
                    {
                        Target = eventGridEvent.EventType,
                        Arguments = new[] { eventGridEvent.Data }
                    });

            }
            else
            {
                log.LogError($"Event grid event is null");
                return Task.CompletedTask; 
            }
        }


        [FunctionName("negotiate")]
        public static SignalRConnectionInfo Negotiate(
               [HttpTrigger(AuthorizationLevel.Anonymous)]HttpRequest req,
               [SignalRConnectionInfo
                   (HubName = "retailbank", 
                    UserId = "{headers.x-ms-client-principal-id}")]
               SignalRConnectionInfo connectionInfo)
        {
            // connectionInfo contains an access key token with a name identifier claim set to the authenticated user
            return connectionInfo;
        }
    }
}
