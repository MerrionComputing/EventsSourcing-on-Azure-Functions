using System;
using System.Collections.Generic;
using System.Text;

namespace RetailBank.AzureFunctionApp.Transfer.Functions
{
    /// <summary>
    /// Data used to initiate a "money transfer" request
    /// </summary>
    public sealed class MoneyTransferData
    {

        /// <summary>
        /// The amount we want to transfer
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The account that the transfer will come from
        /// </summary>
        public string SourceAccountNumber { get; set; }

        /// <summary>
        /// The account which the transfer will be paid into
        /// </summary>
        public string TargetAccountNumber { get; set; }

        /// <summary>
        /// Extra text attached to the transfer
        /// </summary>
        public string Commentary { get; set; }
    }
}
