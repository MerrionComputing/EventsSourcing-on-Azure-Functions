using EventSourcingOnAzureFunctions.Common.EventSourcing;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetailBank.AzureFunctionApp.Account.Events
{

    /// <summary>
    /// Money was deposited into a customer account
    /// </summary>
    [EventName("Money Deposited")]
    public class MoneyDeposited
    {

    }
}
