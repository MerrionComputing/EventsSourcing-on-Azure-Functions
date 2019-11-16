using EventSourcingOnAzureFunctions.Common.EventSourcing;
using EventSourcingOnAzureFunctions.Common.EventSourcing.Interfaces;
using RetailBank.AzureFunctionApp.Account.Events;

namespace RetailBank.AzureFunctionApp.Account.Projections
{
    /// <summary>
    /// How much interest is due for an account
    /// </summary>
    /// <remarks>
    /// A negative amount means that the bank is owed interest from the accont
    /// </remarks>
    public class InterestDue
        : ProjectionBase 
    {

        // 1 - What events does this projection care about ?


        // 2- What does it do with them ?

    }
}
