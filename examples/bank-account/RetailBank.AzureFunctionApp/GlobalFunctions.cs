using Azure.Messaging.EventGrid;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

                // Get the signalR group from the event stream identity
                string groupName = string.Empty;
                IEventStreamIdentity eventTarget = eventGridEvent.Data.ToObjectFromJson<EventStreamIdentity>();
                if (eventTarget != null)
                {
                    groupName = MakeSignalRGroupName(eventTarget);
                }
                else
                {
                    groupName = "all";
                }

                return signalRMessages.AddAsync(
                    new SignalRMessage
                    { 
                        GroupName = groupName,
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

        /// <summary>
        /// Convert an event stream identity into a SignalR valid group name
        /// </summary>
        /// <param name="eventTarget">
        /// The entity we are raising an event notification about
        /// </param>
        /// <remarks>
        /// Group name should be less than or equals to 64 characters.
        /// Group name may only have alphanumeric characters and underscores
        /// </remarks>
        private static string MakeSignalRGroupName(IEventStreamIdentity eventTarget)
        {
            if (eventTarget != null)
            {
                string longGroupName = $"{eventTarget.DomainName}_{eventTarget.EntityTypeName}_{eventTarget.InstanceKey}";
                // replace any illegal characters
                char[] arr = longGroupName.Where(c => (char.IsLetterOrDigit(c) ||
                             char.IsWhiteSpace(c) ||
                             c == '_')).ToArray();

                longGroupName = new string(arr);
                // check length
                if (longGroupName.Length > 64)
                {
                    longGroupName = longGroupName.Substring(longGroupName.Length - 64); 
                }
                return longGroupName;
            }
            return string.Empty;
        }


        /// <summary>
        /// This function is used by SignalR clients looking to connect
        /// </summary>
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

        /// <summary>
        /// A subset of the eventgrid message containing the identifiy of the source stream
        /// that sent the message
        /// </summary>
        private class EventStreamIdentity
            : IEventStreamIdentity
        {
            public string DomainName { get; set; }

            public string EntityTypeName { get; set; }

            public string InstanceKey { get; set; }
        }
    }
}
