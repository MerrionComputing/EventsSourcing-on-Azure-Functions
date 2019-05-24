using EventSourcingOnAzureFunctions.Common.EventSourcing;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetailBank.AzureFunctionApp.Account.Events
{
    /// <summary>
    /// An account was opened
    /// </summary>
    [EventName("Account Opened")]
    public class Opened
    {

    }
}
