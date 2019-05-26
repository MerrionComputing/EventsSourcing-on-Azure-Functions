using EventSourcingOnAzureFunctions.Common.EventSourcing;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetailBank.AzureFunctionApp.Account.Events
{

    /// <summary>
    /// Money was deposited into a customer account
    /// </summary>
    [EventName("Money Deposited")]
    public class MoneyDeposited
    {

        /// <summary>
        /// The amount of money deposited into the account
        /// </summary>
        public decimal AmountDeposited { get; set; }

        /// <summary>
        /// The commentary attached to the deposit 
        /// </summary>
        public string Commentary { get; set; }

        /// <summary>
        /// Where did the deposit originate from 
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// If this deposit is part of a business transaction, this is the correlation identifier
        /// that links together all the component parts
        /// </summary>
        public string TransactionCorrelationIdentifier { get; set; }

        /// <summary>
        /// The date/time this deposit was recorded in our system
        /// </summary>
        public DateTime LoggedDepositDate { get; set; }

        /// <summary>
        /// When the funds should expect to be cleared
        /// </summary>
        public DateTime ExpectedFundsClearedDate { get; set; }
    }
}
