using EventSourcingOnAzureFunctions.Common.EventSourcing;

namespace RetailBank.AzureFunctionApp.Transfer.Events
{
    [EventName("Source Funds Refunded")]
    public sealed class SourceFundsRefunded
    {

        /// <summary>
        /// The amount refunded to the source account
        /// </summary>
        public decimal AmountRefunded { get; set; }

        /// <summary>
        /// For an internal source, the sequence number as at which the refund was performed
        /// (Will be zero for an external account as this has no meaning for data we do not control)
        /// </summary>
        public int AsOfSequenceNumber { get; set; }


    }
}
