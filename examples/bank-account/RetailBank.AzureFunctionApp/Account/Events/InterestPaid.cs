using EventSourcingOnAzureFunctions.Common.EventSourcing;

namespace RetailBank.AzureFunctionApp.Account.Events
{
    [EventName("Interest Paid")]
    public class InterestPaid
    {

        /// <summary>
        /// The amount of money interest paid for the account
        /// </summary>
        /// <remarks>
        /// This can be negative interest was charged
        /// </remarks>
        public decimal AmountAccrued { get; set; }

        /// <summary>
        /// The commentary attached to the interest 
        /// </summary>
        public string Commentary { get; set; }

    }
}
