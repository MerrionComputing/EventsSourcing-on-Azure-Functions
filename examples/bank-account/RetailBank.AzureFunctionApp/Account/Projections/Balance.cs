using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using RetailBank.AzureFunctionApp.Account.Events;

namespace RetailBank.AzureFunctionApp.Account.Projections
{
    /// <summary>
    /// The running balance of the account
    /// </summary>
    public class Balance
        : ProjectionBase,
        IHandleEventType<MoneyDeposited>,
        IHandleEventType<MoneyWithdrawn >,
        IHandleEventType<InterestPaid > 
    {

        private decimal currentBalance;

        /// <summary>
        /// The current balance after the projection has run over a bank account event stream
        /// </summary>
        public decimal CurrentBalance
        {
            get
            {
                return currentBalance;
            }
        }

        

        public void HandleEventInstance(MoneyDeposited eventInstance)
        {
            if (null != eventInstance )
            {
                currentBalance += eventInstance.AmountDeposited;
            }
        }

        public void HandleEventInstance(MoneyWithdrawn eventInstance)
        {
            if (null != eventInstance )
            {
                currentBalance -= eventInstance.AmountWithdrawn;
            }
        }

        public void HandleEventInstance(InterestPaid eventInstance)
        {
            if (null != eventInstance)
            {
                currentBalance += eventInstance.AmountPaid;
            }
        }

        public Balance()
        {
            
        }
    }
}
