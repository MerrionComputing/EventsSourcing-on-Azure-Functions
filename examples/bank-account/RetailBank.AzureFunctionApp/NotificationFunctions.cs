
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

        /// <summary>
        /// This event is triggered whenever a new bank account entity notification is sent via EventGrid,
        /// and just adds a row to the "all-bank-accounts.txt" file
        /// </summary>
        [FunctionName("OnNewBankAccountNotification")]
        public static void OnNewBankAccountNotification([EventGridTrigger()]EventGridEvent eventGridEvent,
            [Blob("bank/reference-data/all-bank-accounts.txt", 
            FileAccess.Write , 
            Connection = "RetailBank")] TextWriter bankAccountList,
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
                NewEntityEventGridPayload payload = eventGridEvent.Data.ToObjectFromJson<NewEntityEventGridPayload>();
                if (null != payload )
                {
                    // New bank account is uniquely identified by key
                    string newAccountNumber = payload.InstanceKey;  
                    if (! string.IsNullOrWhiteSpace(newAccountNumber ) )
                    {
                        bankAccountList.WriteLine(newAccountNumber);
                        bankAccountList.Flush(); 
                    }
                }
            }
            else
            {
                throw new ArgumentException( "Event grid message has no data");
            }

        }
    }
}
