using EventSourcingOnAzureFunctions.Common.EventSourcing;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetailBank.AzureFunctionApp.Account.Events
{
    /// <summary>
    /// An account was opened
    /// </summary>
    [EventName("Account Opened")]
    public class Opened
    {

        /// <summary>
        /// The date/time this account was opened in our system
        /// </summary>
        public DateTime LoggedOpeningDate { get; set; }

        /// <summary>
        /// The commentary attached to the account being opened 
        /// </summary>
        public string Commentary { get; set; }

    }
}
