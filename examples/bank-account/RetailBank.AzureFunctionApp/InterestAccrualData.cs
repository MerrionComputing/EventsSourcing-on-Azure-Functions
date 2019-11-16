using System;
using System.Collections.Generic;
using System.Text;

namespace RetailBank.AzureFunctionApp
{
    /// <summary>
    /// The data payload passed to an "accrue interest" command  
    /// </summary>
    internal class InterestAccrualData
    {

        /// <summary>
        /// The text commentary provided when accruing interest
        /// </summary>
        public string Commentary { get; set; }

        /// <summary>
        /// The interest rate to charge if the account is below zero
        /// </summary>
        public decimal DebitInterestRate { get; set; }

        /// <summary>
        /// The interest rate to charge if the account is above zero
        /// </summary>
        public decimal CreditInterestRate { get; set; }

    }
}
