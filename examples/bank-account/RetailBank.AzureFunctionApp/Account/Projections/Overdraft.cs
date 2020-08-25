using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using RetailBank.AzureFunctionApp.Account.Events;

namespace RetailBank.AzureFunctionApp.Account.Projections
{
    /// <summary>
    /// The overdraft limit in force for this account
    /// </summary>
    [ProjectionName("Overdraft Limit") ]
    public class OverdraftLimit
        : ProjectionBase,
        IHandleEventType<OverdraftLimitSet>
    {

        private decimal currentOverdraft;


        /// <summary>
        /// The overdraft limit found for a bank account event stream
        /// </summary>
        public decimal CurrentOverdraftLimit
        {
            get
            {
                return currentOverdraft;
            }
        }

        public void HandleEventInstance(OverdraftLimitSet eventInstance)
        {
            if (null != eventInstance)
            {
                currentOverdraft = eventInstance.OverdraftLimit;
            }
        }

        public override string ToString()
        {
            return $"Overdraft limit {CurrentOverdraftLimit}");
        }
    }
}
