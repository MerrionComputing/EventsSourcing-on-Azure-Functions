using EventSourcingOnAzureFunctions.Common.EventSourcing;
using System;

namespace RetailBank.AzureFunctionApp.Account.Events
{
    [EventName("Interest Accrued")]
    public class InterestAccrued
    {

        /// <summary>
        /// The amount of money interest accrued for the account
        /// </summary>
        public decimal AmountAccrued { get; set; }

        /// <summary>
        /// The commentary attached to the interest 
        /// </summary>
        public string Commentary { get; set; }

        /// <summary>
        /// How much was the interest rate when this accrual happened
        /// </summary>
        public decimal InterestRateInEffect { get; set; }

        /// <summary>
        /// As of when was this accrual effective
        /// </summary>
        public DateTime AccrualEffectiveDate { get; set; }

    }
}
