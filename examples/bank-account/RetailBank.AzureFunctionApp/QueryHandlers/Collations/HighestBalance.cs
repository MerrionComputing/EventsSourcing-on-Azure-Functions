using EventSourcingOnAzureFunctions.Common.EventSourcing;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetailBank.AzureFunctionApp.QueryHandlers.Collations
{
    /// <summary>
    /// The account that has the highest balance in a given query result set
    /// </summary>
    public sealed class HighestBalance
    {

        string AccountNumber { get; set; }

        Nullable<decimal> CurrentBalance { get; set; }

        int AsOfSequenceNumber { get; set; }

    }
}
