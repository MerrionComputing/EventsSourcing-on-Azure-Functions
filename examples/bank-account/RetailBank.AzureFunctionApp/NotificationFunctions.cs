
using Azure.Messaging.EventGrid;
using EventSourcingOnAzureFunctions.Common.Notification;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
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

    }
}
