using EventSourcingOnAzureFunctions.Common.EventSourcing;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetailBank.AzureFunctionApp.Account.Events
{
    /// <summary>
    /// Money was withdrawn from a customer account
    /// </summary>
    [EventName("Money Withdrawn")]
    public class MoneyWithdrawn
    {

    }
}
