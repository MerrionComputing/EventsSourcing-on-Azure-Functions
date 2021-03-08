using EventSourcingOnAzureFunctions.Common.EventSourcing;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetailBank.AzureFunctionApp.Transfer.Events
{

    /// <summary>
    /// Funds for this transfer were withdrawn from the source account
    /// </summary>
    [EventName("Source Funds Withdrawn")]
    public sealed class SourceFundsWithdrawn
    {

        /// <summary>
        /// The amount withdrawn fro the source account
        /// </summary>
        /// <remarks>
        /// This may be different to the requested transfer if the transfer process 
        /// allows for partial transfers
        /// </remarks>
        public decimal AmountWithdrawn { get; set; }

        /// <summary>
        /// For an internal source, the sequence number as at which the withdrawal was performed
        /// (Will be zero for an external account as this has no meaning for data we do not control)
        /// </summary>
        public int AsOfSequenceNumber { get; set; }

        /// <summary>
        /// Additional commentary on the funds withdrawal
        /// </summary>
        public string Commentary { get; set; }
    }
}
