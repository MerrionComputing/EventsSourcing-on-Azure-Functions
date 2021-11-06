using EventSourcingOnAzureFunctions.Common.CQRS.ProjectionHandler.Events;
using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetailBank.AzureFunctionApp.QueryHandlers.Collations
{
    /// <summary>
    /// A projection to return the account with the highest balance in a query 
    /// </summary>
    [ProjectionName("Highest Balance Collation")]
    public sealed class HighestBalanceQueryCollation
        : ProjectionBase,
        IHandleEventType<ProjectionValueReturned>
    {

        public void HandleEventInstance(ProjectionValueReturned eventInstance)
        {
            if (eventInstance != null)
            {
                // if the projection was "Get Balance" for an account...

            }
        }
    }
}
