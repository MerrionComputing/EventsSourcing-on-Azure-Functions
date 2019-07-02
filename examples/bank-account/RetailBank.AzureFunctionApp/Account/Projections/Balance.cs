using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using Newtonsoft.Json;
using RetailBank.AzureFunctionApp.Account.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetailBank.AzureFunctionApp.Account.Projections
{
    /// <summary>
    /// The running balance of the account
    /// </summary>
    public class Balance
        : ProjectionBase,
        IHandleEventType<MoneyDeposited>,
        IHandleEventType<MoneyWithdrawn >
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
    }
}
