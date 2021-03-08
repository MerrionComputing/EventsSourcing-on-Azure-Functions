using EventSourcingOnAzureFunctions.Common.EventSourcing;

namespace RetailBank.AzureFunctionApp.Transfer.Events
{
    [EventName("Refund Initiated")]
    public sealed class RefundInitiated
    {

        /// <summary>
        /// The amount to refund to the source
        /// </summary>
        /// <remarks>
        /// This may not be the same as the original transfer
        /// request if there were non-refundable charges etc
        /// </remarks>
        public decimal AmountToRefund { get; set; }

        /// <summary>
        /// The reason that this refund was initiated
        /// </summary>
        public string Reason { get; set; }
    }
}
