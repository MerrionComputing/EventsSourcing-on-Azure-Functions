using EventSourcingOnAzureFunctions.Common.EventSourcing;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetailBank.AzureFunctionApp.Transfer.Events
{
    [EventName("Target Funds Deposited")]
    public sealed class TargetFundsDeposited
    {

        /// <summary>
        /// The amount deposited in the target account
        /// </summary>
        /// <remarks>
        /// This may be different to the requested transfer if the transfer process 
        /// allows for partial transfers
        /// </remarks>
        public decimal AmountDeposited { get; set; }

        /// <summary>
        /// For an internal source, the sequence number as at which the deposit was performed
        /// (Will be zero for an external account as this has no meaning for data we do not control)
        /// </summary>
        public int AsOfSequenceNumber { get; set; }

        /// <summary>
        /// Additional commentary on the funds deposit
        /// </summary>
        public string Commentary { get; set; }
    }
}
