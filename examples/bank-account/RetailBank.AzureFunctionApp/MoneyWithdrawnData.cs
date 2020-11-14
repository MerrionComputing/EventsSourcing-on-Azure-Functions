using System;
using System.Collections.Generic;
using System.Text;

namespace RetailBank.AzureFunctionApp
{

    /// <summary>
    /// Payload of the "withdraw money" function
    /// </summary>
    public class MoneyWithdrawnData
    {

        /// <summary>
        /// The amount of money to be withdrawn into the account
        /// </summary>
        public decimal AmountWithdrawn { get; set; }

        /// <summary>
        /// The commentary attached to the money withdrawal 
        /// </summary>
        public string Commentary { get; set; }

    }
}
