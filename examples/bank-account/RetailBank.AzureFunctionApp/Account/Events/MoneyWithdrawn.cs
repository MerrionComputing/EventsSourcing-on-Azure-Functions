using EventSourcingOnAzureFunctions.Common.EventSourcing;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetailBank.AzureFunctionApp.Account.Events
{
    /// <summary>
    /// Money was withdrawn from a customer account
    /// </summary>
    [EventName("Money Withdrawn")]
    public class MoneyWithdrawn
    {

        /// <summary>
        /// The amount of money to be withdrawn into the account
        /// </summary>
        public decimal AmountWithdrawn { get; set; }

        /// <summary>
        /// The commentary attached to the money withdrawal 
        /// </summary>
        public string Commentary { get; set; }

        /// <summary>
        /// Where did the withdrawal go to 
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// If this withdrawal is part of a business transaction, this is the correlation identifier
        /// that links together all the component parts
        /// </summary>
        public string TransactionCorrelationIdentifier { get; set; }

        /// <summary>
        /// The date/time this withdrawal was recorded in our system
        /// </summary>
        public DateTime LoggedWithdrawalDate { get; set; }
    }
}
