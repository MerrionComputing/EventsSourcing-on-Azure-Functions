using EventSourcingOnAzureFunctions.Common.EventSourcing;

namespace RetailBank.AzureFunctionApp.Account.Events
{
    [EventName("Overdraft Limit Set")]
    public class OverdraftLimitSet
    {

        /// <summary>
        /// The maximum amount the account can be overdrawn
        /// </summary>
        public decimal OverdraftLimit { get; set; }

        /// <summary>
        /// Additional notes on the overdraft being set
        /// </summary>
        public string Commentary { get; set; }

        /// <summary>
        /// Is the overdraft unauthorized (due to a mandatory withdrawal exceeding
        /// the current overdraft limit)
        /// </summary>
        public bool Unauthorised { get; set; }
    }
}
