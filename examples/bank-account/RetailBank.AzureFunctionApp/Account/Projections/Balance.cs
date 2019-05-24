using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetailBank.AzureFunctionApp.Account.Projections
{
    /// <summary>
    /// The running balance of the account
    /// </summary>
    public class Balance
        : ProjectionBase
    {

        private decimal currentBalance;

        public decimal CurrentBalance
        {
            get
            {
                return currentBalance;
            }
        }

        /// <summary>
        /// What types of event does this projection care about?
        /// </summary>
        /// <param name="eventTypeName">
        /// The name of the type of event
        /// </param>
        public override bool HandlesEventType(string eventTypeName)
        {
            if (eventTypeName == EventNameAttribute.GetEventName(typeof(Events.MoneyDeposited)))
            {
                return true;
            }

            if (eventTypeName == EventNameAttribute.GetEventName(typeof(Events.MoneyWithdrawn)))
            {
                return true;
            }

            return false;
        }

        public override void HandleEvent(string eventTypeName, object eventToHandle)
        {
            throw new NotImplementedException();
        }



    }
}
