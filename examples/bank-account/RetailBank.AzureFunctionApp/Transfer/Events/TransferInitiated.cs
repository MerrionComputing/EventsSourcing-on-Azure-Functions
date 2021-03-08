using EventSourcingOnAzureFunctions.Common.EventSourcing;

namespace RetailBank.AzureFunctionApp.Transfer.Events
{
    /// <summary>
    /// A money transfer between the source and target accounts has been initiated
    /// </summary>
    [EventName("Money Transfer Initiated")]
    public sealed class TransferInitiated
    {

        /// <summary>
        /// The amount being transfered
        /// </summary>
        /// <remarks>
        /// This is currently a "same currency" transfer - we may consider expanding the example
        /// to include FX rates at a later date
        /// </remarks>
        public decimal Amount { get; set; }

        /// <summary>
        /// The account number the transfer is coming from
        /// </summary>
        public string SourceAccountNumber { get; set; }

        /// <summary>
        /// True if the account we are transfering from is in our bank
        /// </summary>
        public bool SourceIsInternal { get; set; }

        /// <summary>
        /// The account the transfer is going to
        /// </summary>
        public string TargetAccountNumber { get; set; }

        /// <summary>
        /// True if the account we are transfering to is in our bank
        /// </summary>
        public bool TargetIsInternal { get; set; }
    }
}
