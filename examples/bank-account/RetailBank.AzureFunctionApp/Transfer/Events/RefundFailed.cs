using EventSourcingOnAzureFunctions.Common.EventSourcing;

namespace RetailBank.AzureFunctionApp.Transfer.Events
{
    
    [EventName("Refund Failed")]
    public sealed class RefundFailed
    {

        public string Reason { get; set; }

        /// <summary>
        /// Additional commentary on the refund failure
        /// </summary>
        public string Commentary { get; set; }

    }
}
