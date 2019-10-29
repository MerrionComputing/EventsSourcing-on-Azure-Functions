
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace RetailBank.AzureFunctionApp
{
    /// <summary>
    /// Functions triggered by the Event Grid notifications that come out of the 
    /// event sourcing library to demonstrate how they could be used to wire together
    /// microservices style applications
    /// </summary>
    /// <remarks>
    /// These functions are wired-up to the notification event grid by configuration
    /// </remarks>
    public static class NotificationFunctions
    {

        /// <summary>
        /// This event is triggered whenever a new entity notification is sent via EventGrid
        /// </summary>
        [FunctionName("OnNewEntityNotification")]
        public static Task OnNewEntityNotificationRun([EventGridTrigger]EventGridEvent eventGridEvent,
            [SignalR(HubName = "retailbanknotification")]IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {

            #region Logging
            if (null != log)
            {
                log.LogInformation("OnNewEntityNotification called");
                if (null != eventGridEvent.Data)
                {
                    log.LogInformation(eventGridEvent.Data.ToString());
                }
            }
            #endregion


            if (null != eventGridEvent.Data)
            {
                // Turn the eventgrid message into a SignalR notification and send it on..
                return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "NewEntity",
                    Arguments = new[] { eventGridEvent.Data }
                });
            }
            else
            {
                return Task.FromException(new ArgumentException( "Event grid message has no data"));
            }

        }

        /// <summary>
        /// This event is triggered whenever a new event notification is sent via EventGrid
        /// </summary>
        [FunctionName("OnNewEventNotification")]
        public static Task OnNewEventNotificationRun([EventGridTrigger]EventGridEvent eventGridEvent,
            [SignalR(HubName = "retailbanknotification")]IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {

            #region Logging
            if (null != log)
            {
                log.LogInformation("OnNewEventNotification called");
                if (null != eventGridEvent.Data)
                {
                    log.LogInformation(eventGridEvent.Data.ToString());
                }
            }
            #endregion


            if (null != eventGridEvent.Data  )
            { 
            // Turn the eventgrid message into a SignalR notification and send it on..
            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    Target = "NewEvent",
                    Arguments = new[] { eventGridEvent.Data }
                });
            }
            else
            {
                return Task.FromException(new ArgumentException("Event grid message has no data"));
            }

        }
    }
}
